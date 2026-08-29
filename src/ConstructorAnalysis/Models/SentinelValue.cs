namespace ConstructorAnalysis.Models;

internal sealed record SentinelValue(
    object? Value,
    SentinelGenerationStatus Status,
    string? Detail);
