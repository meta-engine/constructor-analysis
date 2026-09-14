using System.Reflection;
using System.Runtime.CompilerServices;
using ConstructorAnalysis.Models;

namespace ConstructorAnalysis;

internal sealed class SentinelValueFactory
{
    public SentinelValue Create(Type type, int index, int occurrence, int sameTypeCount)
    {
        var nullableType = Nullable.GetUnderlyingType(type);
        if (nullableType is not null)
        {
            return Create(nullableType, index, occurrence, sameTypeCount);
        }

        if (type == typeof(string))
        {
            return Supported($"__Constructor_Analysis_{index}_{Guid.NewGuid():N}_Aa__");
        }

        if (type == typeof(Guid))
        {
            return Supported(Guid.NewGuid());
        }

        if (type == typeof(bool))
        {
            return Ambiguous(
                occurrence == 0,
                "Boolean flow requires a contrast probe and cannot be inferred confidently in one pass.");
        }

        if (type.IsEnum)
        {
            return CreateEnumValue(type, occurrence, sameTypeCount);
        }

        var scalar = CreateScalarValue(type, index);
        if (scalar is not null)
        {
            return scalar;
        }

        if (type.IsArray)
        {
            var lengths = new int[type.GetArrayRank()];
            return Supported(Array.CreateInstance(type.GetElementType()!, lengths));
        }

        if (type.IsInterface)
        {
            try
            {
                return Supported(DispatchProxy.Create(type, typeof(SentinelDispatchProxy)));
            }
            catch (ArgumentException exception)
            {
                return Unsupported(null, $"Interface sentinel generation failed: {exception.Message}");
            }
        }

        if (type.IsClass && !type.IsAbstract && !typeof(Delegate).IsAssignableFrom(type))
        {
            try
            {
                return Supported(RuntimeHelpers.GetUninitializedObject(type));
            }
            catch (ArgumentException exception)
            {
                return Unsupported(null, $"Class sentinel generation failed: {exception.Message}");
            }
        }

        if (type.IsValueType)
        {
            return CreateUnsupportedValueType(type);
        }

        return Unsupported(null, $"Reference type '{type.Name}' cannot receive an assignable sentinel.");
    }

    private SentinelValue CreateEnumValue(Type type, int occurrence, int sameTypeCount)
    {
        var defaultValue = Activator.CreateInstance(type);
        var candidates = Enum.GetValues(type)
            .Cast<object>()
            .Distinct()
            .Where(value => !Equals(value, defaultValue))
            .ToArray();

        if (candidates.Length >= sameTypeCount)
        {
            return Supported(candidates[occurrence]);
        }

        var transportValue = candidates.Length == 0
            ? defaultValue
            : candidates[Math.Min(occurrence, candidates.Length - 1)];
        return Ambiguous(
            transportValue,
            $"Enum '{type.Name}' does not expose enough non-default values for distinct sentinels.");
    }

    private SentinelValue CreateUnsupportedValueType(Type type)
    {
        try
        {
            return Unsupported(
                RuntimeHelpers.GetUninitializedObject(type),
                $"User-defined struct '{type.Name}' has no collision-resistant sentinel strategy.");
        }
        catch (NotSupportedException)
        {
            return Unsupported(null, $"Value type '{type.Name}' cannot be boxed for constructor analysis.");
        }
    }

    private SentinelValue? CreateScalarValue(Type type, int index)
    {
        if (type == typeof(byte)) return index < byte.MaxValue
            ? Supported((byte)(byte.MaxValue - index))
            : Exhausted(type, default(byte));
        if (type == typeof(sbyte)) return index < sbyte.MaxValue
            ? Supported((sbyte)(sbyte.MaxValue - index))
            : Exhausted(type, default(sbyte));
        if (type == typeof(short)) return index < short.MaxValue
            ? Supported((short)(short.MaxValue - index))
            : Exhausted(type, default(short));
        if (type == typeof(ushort)) return index < ushort.MaxValue
            ? Supported((ushort)(ushort.MaxValue - index))
            : Exhausted(type, default(ushort));
        if (type == typeof(int)) return index < int.MaxValue
            ? Supported(int.MaxValue - index)
            : Exhausted(type, default(int));
        if (type == typeof(uint)) return Supported(uint.MaxValue - (uint)index);
        if (type == typeof(long)) return Supported(long.MaxValue - index);
        if (type == typeof(ulong)) return Supported(ulong.MaxValue - (ulong)index);
        if (type == typeof(float)) return Supported(1_000_000f + index);
        if (type == typeof(double)) return Supported(1_000_000d + index);
        if (type == typeof(decimal)) return Supported(1_000_000m + index);
        if (type == typeof(char)) return index < char.MaxValue
            ? Supported((char)(char.MaxValue - index))
            : Exhausted(type, default(char));
        if (type == typeof(DateTime)) return Supported(DateTime.MaxValue.AddTicks(-index));
        if (type == typeof(DateTimeOffset)) return Supported(DateTimeOffset.MaxValue.AddTicks(-index));
        if (type == typeof(TimeSpan)) return Supported(TimeSpan.MaxValue.Subtract(TimeSpan.FromTicks(index)));
        if (type == typeof(DateOnly)) return index < DateOnly.MaxValue.DayNumber
            ? Supported(DateOnly.MaxValue.AddDays(-index))
            : Exhausted(type, default(DateOnly));
        if (type == typeof(TimeOnly)) return Supported(TimeOnly.MaxValue.Add(TimeSpan.FromTicks(-index)));

        return null;
    }

    private SentinelValue Exhausted(Type type, object value) =>
        Ambiguous(value, $"Type '{type.Name}' has no remaining non-default distinct sentinel at this position.");

    private SentinelValue Supported(object value) =>
        new(value, SentinelGenerationStatus.Supported, null);

    private SentinelValue Ambiguous(object? value, string detail) =>
        new(value, SentinelGenerationStatus.Ambiguous, detail);

    private SentinelValue Unsupported(object? value, string detail) =>
        new(value, SentinelGenerationStatus.Unsupported, detail);
}
