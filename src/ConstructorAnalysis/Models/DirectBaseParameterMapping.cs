using System.Reflection;

namespace ConstructorAnalysis.Models;

public sealed class DirectBaseParameterMapping
{
    public required int ParameterIndex { get; init; }
    public string? ParameterName { get; init; }
    public required PropertyInfo CorrelatedProperty { get; init; }
    public required ParameterInferenceOutcome Outcome { get; init; }
    public required FlowMappingConfidence Confidence { get; init; }
    public required FlowMappingProvenance Provenance { get; init; }
}
