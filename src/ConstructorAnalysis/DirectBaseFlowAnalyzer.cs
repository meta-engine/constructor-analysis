using System.Reflection;
using ConstructorAnalysis.Models;

namespace ConstructorAnalysis;

internal sealed class DirectBaseFlowAnalyzer
{
    private readonly ConstructorSelector _constructorSelector;
    private readonly InstanceStateCreator _instanceStateCreator;
    private readonly PropertyFlowMatcher _propertyFlowMatcher;

    public DirectBaseFlowAnalyzer(
        ConstructorSelector constructorSelector,
        InstanceStateCreator instanceStateCreator,
        PropertyFlowMatcher propertyFlowMatcher)
    {
        _constructorSelector = constructorSelector;
        _instanceStateCreator = instanceStateCreator;
        _propertyFlowMatcher = propertyFlowMatcher;
    }

    public void Apply(Type type, IReadOnlyList<ParameterMapping> derivedMappings)
    {
        var baseType = GetDirectBaseType(type);
        if (baseType is null)
        {
            return;
        }

        var baseConstructors = _constructorSelector.SelectAll(baseType)
            .Where(constructor => CanBeCalledFromDerived(constructor, type))
            .ToArray();
        if (baseConstructors.Length == 0)
        {
            return;
        }

        if (baseConstructors.Length > 1)
        {
            MarkAmbiguousOverload(derivedMappings, baseType);
            return;
        }

        var baseConstructor = baseConstructors[0];
        if (baseConstructor.GetParameters().Length == 0)
        {
            return;
        }

        var baseState = _instanceStateCreator.Execute(baseConstructor);
        var baseMappings = _propertyFlowMatcher.Match(baseType, baseState);
        var baseFailure = baseMappings.FirstOrDefault(
            mapping => mapping.Outcome == ParameterInferenceOutcome.InstantiationFailed);
        if (baseFailure is not null)
        {
            MarkBaseProbeFailure(derivedMappings, baseFailure.Detail);
            return;
        }

        foreach (var derivedMapping in derivedMappings)
        {
            var candidates = Correlate(derivedMapping, baseMappings);
            derivedMapping.DirectBaseMappings = candidates;
            if (candidates.Count > 0)
            {
                derivedMapping.DirectBaseOutcome = ParameterInferenceOutcome.Ambiguous;
                derivedMapping.DirectBaseDetail =
                    "Property correlation cannot distinguish a direct-base argument from a derived-constructor write.";
            }
        }
    }

    private IReadOnlyList<DirectBaseParameterMapping> Correlate(
        ParameterMapping derivedMapping,
        IReadOnlyList<ParameterMapping> baseMappings)
    {
        return baseMappings
            .Select(baseMapping => CreateMapping(derivedMapping, baseMapping))
            .Where(mapping => mapping is not null)
            .Cast<DirectBaseParameterMapping>()
            .OrderBy(mapping => mapping.ParameterIndex)
            .ToArray();
    }

    private DirectBaseParameterMapping? CreateMapping(
        ParameterMapping derivedMapping,
        ParameterMapping baseMapping)
    {
        var correlatedProperty = baseMapping.PropertyMappings
            .FirstOrDefault(baseProperty => derivedMapping.PropertyMappings.Any(
                derivedProperty => IsSameProperty(derivedProperty, baseProperty)));
        if (correlatedProperty is null)
        {
            return null;
        }

        return new DirectBaseParameterMapping
        {
            ParameterIndex = baseMapping.Parameter.Position,
            ParameterName = baseMapping.Parameter.Name,
            CorrelatedProperty = correlatedProperty.Property,
            Outcome = ParameterInferenceOutcome.Ambiguous,
            Confidence = FlowMappingConfidence.Heuristic,
            Provenance = FlowMappingProvenance.DirectBasePropertyCorrelation
        };
    }

    private void MarkAmbiguousOverload(
        IReadOnlyList<ParameterMapping> derivedMappings,
        Type baseType)
    {
        foreach (var derivedMapping in derivedMappings)
        {
            derivedMapping.DirectBaseOutcome = ParameterInferenceOutcome.Ambiguous;
            derivedMapping.DirectBaseDetail =
                $"Direct base type '{baseType.Name}' exposes multiple constructor overloads; the runtime probe cannot identify the invoked overload.";
        }
    }

    private void MarkBaseProbeFailure(
        IReadOnlyList<ParameterMapping> derivedMappings,
        string? detail)
    {
        foreach (var derivedMapping in derivedMappings)
        {
            derivedMapping.DirectBaseOutcome = ParameterInferenceOutcome.InstantiationFailed;
            derivedMapping.DirectBaseDetail = detail;
        }
    }

    private bool IsSameProperty(PropertyFlowMapping first, PropertyFlowMapping second)
    {
        return first.Property.DeclaringType == second.Property.DeclaringType &&
            first.Property.Name == second.Property.Name;
    }

    private Type? GetDirectBaseType(Type type)
    {
        var baseType = type.BaseType;
        return baseType is null || baseType == typeof(object) || baseType == typeof(ValueType)
            ? null
            : baseType;
    }

    private bool CanBeCalledFromDerived(ConstructorInfo constructor, Type derivedType)
    {
        if (constructor.IsPublic || constructor.IsFamily || constructor.IsFamilyOrAssembly)
        {
            return true;
        }

        return (constructor.IsAssembly || constructor.IsFamilyAndAssembly) &&
            constructor.DeclaringType?.Assembly == derivedType.Assembly;
    }
}
