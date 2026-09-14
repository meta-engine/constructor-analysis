using System.Reflection;
using ConstructorAnalysis.Models;

namespace ConstructorAnalysis;

internal sealed class PropertyStateReader
{
    public PropertyStateResult Read(Type type, object instance)
    {
        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.GetMethod is { IsPublic: true } &&
                property.GetIndexParameters().Length == 0)
            .OrderBy(property => property.DeclaringType?.FullName, StringComparer.Ordinal)
            .ThenBy(property => property.MetadataToken);
        var values = new List<PropertyValue>();

        foreach (var property in properties)
        {
            try
            {
                values.Add(new PropertyValue(property, property.GetValue(instance)));
            }
            catch (TargetInvocationException exception)
            {
                return Failed(property, exception.InnerException ?? exception);
            }
            catch (MemberAccessException exception)
            {
                return Failed(property, exception);
            }
            catch (NotSupportedException exception)
            {
                return Failed(property, exception);
            }
        }

        return new PropertyStateResult { Values = values };
    }

    private PropertyStateResult Failed(PropertyInfo property, Exception exception)
    {
        return new PropertyStateResult
        {
            Failure = $"Property getter '{property.DeclaringType?.Name}.{property.Name}' failed " +
                $"with {exception.GetType().Name}: {exception.Message}"
        };
    }
}
