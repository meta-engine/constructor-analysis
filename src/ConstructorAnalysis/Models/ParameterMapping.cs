using System.Reflection;

namespace ConstructorAnalysis.Models;

public sealed class ParameterMapping
{
    public required ParameterInfo Parameter { get; init; }
    public required ParameterInferenceOutcome Outcome { get; init; }
    public string? Detail { get; init; }
    public IReadOnlyList<PropertyFlowMapping> PropertyMappings { get; init; } = [];
    public IReadOnlyList<PropertyInfo> AssignedProperties => PropertyMappings
        .Select(mapping => mapping.Property)
        .ToArray();
    public ParameterInferenceOutcome DirectBaseOutcome { get; internal set; } = ParameterInferenceOutcome.Unmatched;
    public string? DirectBaseDetail { get; internal set; }
    public IReadOnlyList<DirectBaseParameterMapping> DirectBaseMappings { get; internal set; } = [];
    public bool HasDirectBaseCandidate => DirectBaseMappings.Count > 0;
    public bool IsPassedToBase => DirectBaseOutcome == ParameterInferenceOutcome.Inferred;
}
