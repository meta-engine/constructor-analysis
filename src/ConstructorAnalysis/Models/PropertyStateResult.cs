namespace ConstructorAnalysis.Models;

internal sealed class PropertyStateResult
{
    public IReadOnlyList<PropertyValue> Values { get; init; } = [];
    public string? Failure { get; init; }
}
