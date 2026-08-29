using System.Reflection;

namespace ConstructorAnalysis.Models;

internal sealed class SentinelArgument
{
    public required ParameterInfo Parameter { get; init; }
    public object? Value { get; init; }
    public required SentinelGenerationStatus Status { get; init; }
    public string? Detail { get; init; }
}
