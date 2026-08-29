using System.Reflection;

namespace ConstructorAnalysis.Models;

internal class InstanceStateResult
{
    public object InstanceValue { get; set; }
    public object[] Arguments { get; set; }
    public List<PropertyInfo> MatchedProperties { get; set; }
}

