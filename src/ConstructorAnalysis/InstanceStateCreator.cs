using System.Reflection;
using ConstructorAnalysis.Models;

namespace ConstructorAnalysis;

internal class InstanceStateCreator
{
    private readonly UniqueValueGenerator _valueGenerator;

    public InstanceStateCreator(UniqueValueGenerator valueGenerator)
    {
        _valueGenerator = valueGenerator;
    }

    public InstanceStateResult Execute(Type type, ConstructorInfo constructor, int parameterIndex)
    {
        var parameters = constructor.GetParameters();
        var typeArguments = new object[parameters.Length];

        // Create arguments: unique value at parameterIndex, defaults elsewhere
        for (var j = 0; j < typeArguments.Length; j++)
        {
            var parameterType = parameters[j].ParameterType;
            var useDefault = j != parameterIndex;
            typeArguments[j] = _valueGenerator.CreateUniqueValue(parameterType, useDefault);
        }

        // Instantiate with our sentinel values
        // Use BindingFlags to access protected constructors
        var instanceValue = Activator.CreateInstance(
            type,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null,
            typeArguments,
            null);

        // Find which properties received the unique value at parameterIndex
        var argumentType = parameters[parameterIndex].ParameterType;
        var matchedProperties = GetMatchedProperties(type, instanceValue, argumentType, typeArguments[parameterIndex]);

        return new InstanceStateResult
        {
            InstanceValue = instanceValue,
            Arguments = typeArguments,
            MatchedProperties = new List<PropertyInfo>(matchedProperties)
        };
    }

    private static IEnumerable<PropertyInfo> GetMatchedProperties(
        Type type,
        object instanceValue,
        Type argumentType,
        object argumentValue)
    {
        var list = new List<PropertyInfo>();

        if (argumentValue == null || instanceValue == null)
        {
            // For interfaces with single property of that type, we can infer
            if (argumentType.IsInterface &&
                type.GetProperties().Count(x => x.DeclaringType == type && x.PropertyType == argumentType) == 1)
            {
                var matchedProperty = type.GetProperties()
                    .Single(x => x.DeclaringType == type && x.PropertyType == argumentType);
                list.Add(matchedProperty);
            }
            return list;
        }

        // Check each property to see if it has our unique value
        foreach (var propertyInfo in type.GetProperties())
        {
            var propertyValue = propertyInfo.GetValue(instanceValue);
            var propertyType = propertyInfo.PropertyType;

            // Use reference equality for objects, value equality for value types
            if (propertyType.IsValueType
                ? argumentValue.Equals(propertyValue)
                : argumentValue == propertyValue)
            {
                list.Add(propertyInfo);
            }
        }

        return list;
    }
}

