namespace ConstructorAnalysis.Tests;

public class OverloadedBaseFixture
{
    public string First { get; }
    public string? Second { get; }

    protected OverloadedBaseFixture(string first)
    {
        First = first;
    }

    protected OverloadedBaseFixture(string first, string second)
    {
        First = first;
        Second = second;
    }
}

public sealed class OverloadedDerivedFixture : OverloadedBaseFixture
{
    public OverloadedDerivedFixture(string value)
        : base(value)
    {
    }
}
