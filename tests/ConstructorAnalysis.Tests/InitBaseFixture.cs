namespace ConstructorAnalysis.Tests;

public class InitBaseFixture
{
    public string Value { get; init; }

    protected InitBaseFixture(string value)
    {
        Value = value;
    }
}

public sealed class InitDerivedFixture : InitBaseFixture
{
    public InitDerivedFixture(string value)
        : base("constant")
    {
        Value = value;
    }
}
