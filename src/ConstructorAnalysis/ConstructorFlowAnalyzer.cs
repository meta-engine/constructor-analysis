using System.Reflection;
using ConstructorAnalysis.Models;

namespace ConstructorAnalysis;

public class ConstructorFlowAnalyzer
{
    private readonly UniqueValueGenerator _valueGenerator;
    private readonly InstanceStateCreator _instanceStateCreator;

    public ConstructorFlowAnalyzer()
    {
        _valueGenerator = new UniqueValueGenerator();
        _instanceStateCreator = new InstanceStateCreator(_valueGenerator);
    }

    public ConstructorFlowAnalysis Analyze(Type type)
    {
        var constructor = GetConstructor(type);
        if (constructor == null)
        {
            return null;
        }

        return AnalyzeConstructor(type, constructor);
    }

    public ConstructorFlowAnalysis AnalyzeConstructor(Type type, ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        var result = new ConstructorFlowAnalysis
        {
            Constructor = constructor,
            ParameterMappings = new List<ParameterMapping>()
        };

        var propertiesSetInConstructor = new HashSet<PropertyInfo>();

        // Analyze each parameter
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var instanceState = _instanceStateCreator.Execute(type, constructor, i);

            var mapping = new ParameterMapping
            {
                Parameter = parameter,
                AssignedProperties = instanceState.MatchedProperties
            };

            // Check if this parameter is passed to base class
            if (HasBaseClass(type))
            {
                mapping.IsPassedToBase = IsParameterPassedToBase(
                    type,
                    i,
                    instanceState.MatchedProperties);
            }

            result.ParameterMappings.Add(mapping);

            foreach (var prop in instanceState.MatchedProperties)
            {
                propertiesSetInConstructor.Add(prop);
            }
        }

        result.PropertiesSetInConstructor = propertiesSetInConstructor.ToList();

        return result;
    }

    private bool IsParameterPassedToBase(Type type, int parameterIndex, List<PropertyInfo> matchedProperties)
    {
        if (!HasBaseClass(type))
        {
            return false;
        }

        var baseConstructor = GetConstructor(type.BaseType);
        if (baseConstructor == null || baseConstructor.GetParameters().Length == 0)
        {
            return false;
        }

        // Check all base constructor parameters to see if any match our properties
        var baseParameters = baseConstructor.GetParameters();
        for (var i = 0; i < baseParameters.Length; i++)
        {
            var baseInstanceState = _instanceStateCreator.Execute(type.BaseType, baseConstructor, i);

            // If any property matches between derived and base, parameter flows to base
            var matchFound = matchedProperties.Any(derivedProp =>
                baseInstanceState.MatchedProperties.Any(baseProp =>
                    baseProp.Name == derivedProp.Name));

            if (matchFound)
            {
                return true;
            }
        }

        return false;
    }

    private static ConstructorInfo GetConstructor(Type type)
    {
        // Get first non-default constructor, or default if that's all there is
        var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        return constructors
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();
    }

    private static bool HasBaseClass(Type type)
    {
        return type.BaseType != null &&
               type.BaseType != typeof(object) &&
               type.BaseType != typeof(ValueType);
    }
}

