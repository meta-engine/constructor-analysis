using System.Reflection;

namespace ConstructorAnalysis.Models;

public sealed class PropertyFlowMapping
{
    public required PropertyInfo Property { get; init; }
    public required FlowMappingConfidence Confidence { get; init; }
    public required FlowMappingProvenance Provenance { get; init; }
}
