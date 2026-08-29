using System.Reflection;

namespace ConstructorAnalysis;

internal class SentinelDispatchProxy : DispatchProxy
{
    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var returnType = targetMethod?.ReturnType;
        return returnType is null || returnType == typeof(void) || !returnType.IsValueType
            ? null
            : Activator.CreateInstance(returnType);
    }
}
