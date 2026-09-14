using ConstructorAnalysis.Models;

namespace ConstructorAnalysis;

internal sealed class PropertyFlowMatcher
{
    private readonly PropertyStateReader _propertyStateReader;

    public PropertyFlowMatcher(PropertyStateReader propertyStateReader)
    {
        _propertyStateReader = propertyStateReader;
    }

    public IReadOnlyList<ParameterMapping> Match(Type type, InstanceStateResult state)
    {
        var propertyState = state.InstanceValue is null
            ? new PropertyStateResult()
            : _propertyStateReader.Read(type, state.InstanceValue);

        return state.Arguments
            .Select(argument => MatchArgument(argument, state.Failure, propertyState))
            .ToArray();
    }

    private ParameterMapping MatchArgument(
        SentinelArgument argument,
        InstanceCreationFailure? failure,
        PropertyStateResult propertyState)
    {
        if (argument.Status != SentinelGenerationStatus.Supported)
        {
            return CreateUnavailableMapping(argument);
        }

        if (failure is not null)
        {
            return new ParameterMapping
            {
                Parameter = argument.Parameter,
                Outcome = ParameterInferenceOutcome.InstantiationFailed,
                Detail = failure.Detail
            };
        }

        if (propertyState.Failure is not null)
        {
            return new ParameterMapping
            {
                Parameter = argument.Parameter,
                Outcome = ParameterInferenceOutcome.InspectionFailed,
                Detail = propertyState.Failure
            };
        }

        var exactMappings = propertyState.Values
            .Where(property => IsExactMatch(argument, property))
            .Select(property => new PropertyFlowMapping
            {
                Property = property.Property,
                Confidence = FlowMappingConfidence.Exact,
                Provenance = FlowMappingProvenance.ExactSentinel
            })
            .ToArray();

        if (exactMappings.Length > 0)
        {
            return Inferred(argument, exactMappings);
        }

        var transformedMappings = FindTransformedStringMappings(argument, propertyState.Values);
        return transformedMappings.Count > 0
            ? Inferred(argument, transformedMappings)
            : new ParameterMapping
            {
                Parameter = argument.Parameter,
                Outcome = ParameterInferenceOutcome.Unmatched,
                Detail = "No property preserved the generated sentinel."
            };
    }

    private ParameterMapping CreateUnavailableMapping(SentinelArgument argument)
    {
        var outcome = argument.Status == SentinelGenerationStatus.Ambiguous
            ? ParameterInferenceOutcome.Ambiguous
            : ParameterInferenceOutcome.Unsupported;

        return new ParameterMapping
        {
            Parameter = argument.Parameter,
            Outcome = outcome,
            Detail = argument.Detail
        };
    }

    private ParameterMapping Inferred(
        SentinelArgument argument,
        IReadOnlyList<PropertyFlowMapping> propertyMappings)
    {
        return new ParameterMapping
        {
            Parameter = argument.Parameter,
            Outcome = ParameterInferenceOutcome.Inferred,
            PropertyMappings = propertyMappings
        };
    }

    private bool IsExactMatch(SentinelArgument argument, PropertyValue property)
    {
        if (argument.Value is null || property.Value is null)
        {
            return false;
        }

        return argument.Value is string argumentString && property.Value is string propertyString
            ? string.Equals(argumentString, propertyString, StringComparison.Ordinal)
            : argument.Parameter.ParameterType.IsValueType
                ? argument.Value.Equals(property.Value)
                : ReferenceEquals(argument.Value, property.Value);
    }

    private IReadOnlyList<PropertyFlowMapping> FindTransformedStringMappings(
        SentinelArgument argument,
        IReadOnlyList<PropertyValue> propertyValues)
    {
        if (argument.Value is not string sentinel)
        {
            return [];
        }

        return propertyValues
            .Where(property => property.Value is string value &&
                value.Contains(sentinel, StringComparison.OrdinalIgnoreCase))
            .Select(property => new PropertyFlowMapping
            {
                Property = property.Property,
                Confidence = FlowMappingConfidence.Heuristic,
                Provenance = FlowMappingProvenance.TransformedStringContainment
            })
            .ToArray();
    }
}
