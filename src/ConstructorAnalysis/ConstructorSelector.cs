using System.Reflection;

namespace ConstructorAnalysis;

internal sealed class ConstructorSelector
{
    public ConstructorInfo? Select(Type type)
    {
        return SelectAll(type).FirstOrDefault();
    }

    public IReadOnlyList<ConstructorInfo> SelectAll(Type type)
    {
        return type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .OrderByDescending(constructor => constructor.GetParameters().Length)
            .ThenBy(constructor => constructor.MetadataToken)
            .ToArray();
    }
}
