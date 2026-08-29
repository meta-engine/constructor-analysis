namespace ConstructorAnalysis.Models;

internal sealed class InstanceStateResult
{
    public object? InstanceValue { get; init; }
    public IReadOnlyList<SentinelArgument> Arguments { get; init; } = [];
    public InstanceCreationFailure? Failure { get; init; }
}
