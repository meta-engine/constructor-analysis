namespace ConstructorAnalysis;

internal class UniqueValueGenerator
{
    private int _counter = 0;

    public object CreateUniqueValue(Type type, bool useDefault = false)
    {
        if (useDefault)
        {
            return CreateDefaultValue(type);
        }

        // For reference types, use new object() for reference equality tracking
        if (!type.IsValueType && type != typeof(string))
        {
            return new object();
        }

        // For strings, use unique GUID
        if (type == typeof(string))
        {
            return Guid.NewGuid().ToString();
        }

        // For GUIDs, create unique values
        if (type == typeof(Guid))
        {
            return Guid.NewGuid();
        }

        // For nullable types
        if (Nullable.GetUnderlyingType(type) != null)
        {
            var underlyingType = Nullable.GetUnderlyingType(type);
            return CreateUniqueValue(underlyingType, false);
        }

        // For value types, create distinct non-default values
        if (type == typeof(int))
            return ++_counter;

        if (type == typeof(long))
            return (long)++_counter;

        if (type == typeof(bool))
            return true;

        if (type == typeof(DateTime))
            return new DateTime(2000, 1, 1).AddTicks(++_counter);

        if (type == typeof(decimal))
            return (decimal)++_counter;

        if (type == typeof(double))
            return (double)++_counter;

        if (type == typeof(float))
            return (float)++_counter;

        if (type.IsEnum)
        {
            var values = Enum.GetValues(type);
            return values.GetValue(Math.Min(1, values.Length - 1));
        }

        // Fallback: try to create default instance
        return Activator.CreateInstance(type);
    }

    private object CreateDefaultValue(Type type)
    {
        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        return null;
    }
}

