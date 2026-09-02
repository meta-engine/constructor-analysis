using ConstructorAnalysis.Models;

namespace ConstructorAnalysis;

internal sealed class DirectBaseCandidateFactory
{
    private readonly ReadOnlyAutoPropertyDetector _readOnlyPropertyDetector;

    public DirectBaseCandidateFactory(ReadOnlyAutoPropertyDetector readOnlyPropertyDetector)
    {
        _readOnlyPropertyDetector = readOnlyPropertyDetector;
    }

    public DirectBaseParameterMapping? Create(ParameterMapping derivedMapping, ParameterMapping baseMapping)
    {
        var correlations = FindCorrelations(derivedMapping, baseMapping);
        if (correlations.Count == 0)
        {
            return null;
        }

        var forwarded = correlations.FirstOrDefault(IsForwardedThroughBase);
        var correlation = forwarded ?? correlations[0];

        return new DirectBaseParameterMapping
        {
            ParameterIndex = baseMapping.Parameter.Position,
            ParameterName = baseMapping.Parameter.Name,
            CorrelatedProperty = correlation.BaseProperty.Property,
            Outcome = forwarded is null ? ParameterInferenceOutcome.Ambiguous : ParameterInferenceOutcome.Inferred,
            Confidence = forwarded is null ? FlowMappingConfidence.Heuristic : FlowMappingConfidence.Exact,
            Provenance = forwarded is null
                ? FlowMappingProvenance.DirectBasePropertyCorrelation
                : FlowMappingProvenance.ReadOnlyBaseSentinel
        };
    }

    private IReadOnlyList<PropertyCorrelation> FindCorrelations(
        ParameterMapping derivedMapping,
        ParameterMapping baseMapping)
    {
        return baseMapping.PropertyMappings
            .SelectMany(baseProperty => derivedMapping.PropertyMappings
                .Where(derivedProperty => IsSameProperty(derivedProperty, baseProperty))
                .Select(derivedProperty => new PropertyCorrelation(derivedProperty, baseProperty)))
            .ToArray();
    }

    private bool IsForwardedThroughBase(PropertyCorrelation correlation)
    {
        return correlation.DerivedProperty.Confidence == FlowMappingConfidence.Exact &&
            correlation.BaseProperty.Confidence == FlowMappingConfidence.Exact &&
            _readOnlyPropertyDetector.IsWrittenOnlyByDeclaringConstructor(correlation.BaseProperty.Property);
    }

    private bool IsSameProperty(PropertyFlowMapping first, PropertyFlowMapping second)
    {
        return first.Property.DeclaringType == second.Property.DeclaringType &&
            first.Property.Name == second.Property.Name;
    }

    private sealed record PropertyCorrelation(PropertyFlowMapping DerivedProperty, PropertyFlowMapping BaseProperty);
}
