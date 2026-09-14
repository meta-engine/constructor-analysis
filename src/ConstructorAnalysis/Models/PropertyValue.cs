using System.Reflection;

namespace ConstructorAnalysis.Models;

internal sealed record PropertyValue(PropertyInfo Property, object? Value);
