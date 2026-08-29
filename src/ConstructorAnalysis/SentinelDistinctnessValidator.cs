using ConstructorAnalysis.Models;

namespace ConstructorAnalysis;

internal sealed class SentinelDistinctnessValidator
{
    public IReadOnlyList<SentinelArgument> MarkCollisions(IReadOnlyList<SentinelArgument> arguments)
    {
        var collidingIndexes = FindCollidingIndexes(arguments);

        return arguments
            .Select((argument, index) => collidingIndexes.Contains(index)
                ? MarkAmbiguous(argument)
                : argument)
            .ToArray();
    }

    private IReadOnlySet<int> FindCollidingIndexes(IReadOnlyList<SentinelArgument> arguments)
    {
        var collidingIndexes = new HashSet<int>();
        for (var first = 0; first < arguments.Count; first++)
        {
            if (arguments[first].Status != SentinelGenerationStatus.Supported)
            {
                continue;
            }

            for (var second = first + 1; second < arguments.Count; second++)
            {
                if (arguments[second].Status == SentinelGenerationStatus.Supported &&
                    AreEquivalent(arguments[first], arguments[second]))
                {
                    collidingIndexes.Add(first);
                    collidingIndexes.Add(second);
                }
            }
        }

        return collidingIndexes;
    }

    private SentinelArgument MarkAmbiguous(SentinelArgument argument)
    {
        return new SentinelArgument
        {
            Parameter = argument.Parameter,
            Value = argument.Value,
            Status = SentinelGenerationStatus.Ambiguous,
            Detail = "The generated value is not distinct from another constructor argument."
        };
    }

    private bool AreEquivalent(SentinelArgument first, SentinelArgument second)
    {
        if (first.Value is null || second.Value is null)
        {
            return first.Value is null && second.Value is null;
        }

        return first.Value is string firstString && second.Value is string secondString
            ? string.Equals(firstString, secondString, StringComparison.Ordinal)
            : first.Parameter.ParameterType.IsValueType || second.Parameter.ParameterType.IsValueType
                ? first.Value.Equals(second.Value)
                : ReferenceEquals(first.Value, second.Value);
    }
}
