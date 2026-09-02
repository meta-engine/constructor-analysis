using System.Reflection;
using System.Runtime.CompilerServices;

namespace ConstructorAnalysis;

internal sealed class ReadOnlyAutoPropertyDetector
{
    public bool IsWrittenOnlyByDeclaringConstructor(PropertyInfo property)
    {
        if (property.GetSetMethod(nonPublic: true) is not null)
        {
            return false;
        }

        var backingField = property.DeclaringType?.GetField(
            $"<{property.Name}>k__BackingField",
            BindingFlags.NonPublic | BindingFlags.Instance);

        return backingField is { IsPrivate: true, IsInitOnly: true } &&
            backingField.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false);
    }
}
