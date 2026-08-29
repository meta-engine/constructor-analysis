using System.Reflection;

namespace ConstructorAnalysis.Models;

public class ParameterMapping
{
    public ParameterInfo Parameter { get; set; }
    public List<PropertyInfo> AssignedProperties { get; set; } = new();
    public bool IsPassedToBase { get; set; }
}

