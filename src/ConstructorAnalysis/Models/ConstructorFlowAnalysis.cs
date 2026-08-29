using System.Reflection;

namespace ConstructorAnalysis.Models;

public sealed class ConstructorFlowAnalysis
{
    public required ConstructorInfo Constructor { get; init; }
    public IReadOnlyList<ParameterMapping> ParameterMappings { get; init; } = [];
    public IReadOnlyList<PropertyInfo> PropertiesSetInConstructor { get; init; } = [];
}
