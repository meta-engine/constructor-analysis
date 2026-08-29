using System.Reflection;

namespace ConstructorAnalysis.Models;

public class ConstructorFlowAnalysis
{
    public ConstructorInfo Constructor { get; set; }
    public List<ParameterMapping> ParameterMappings { get; set; } = new();
    public List<PropertyInfo> PropertiesSetInConstructor { get; set; } = new();
}

