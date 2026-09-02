namespace ConstructorAnalysis.Tests;

public class ReadOnlyBaseFixture
{
    public string Value { get; }

    protected ReadOnlyBaseFixture(string value)
    {
        Value = value;
    }
}

public sealed class ReadOnlyDerivedFixture : ReadOnlyBaseFixture
{
    public string Local { get; }

    public ReadOnlyDerivedFixture(string input, string local)
        : base(input)
    {
        Local = local;
    }
}
