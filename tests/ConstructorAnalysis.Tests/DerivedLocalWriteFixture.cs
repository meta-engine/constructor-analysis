namespace ConstructorAnalysis.Tests;

public class WritableBaseFixture
{
    public string Value { get; protected set; }

    protected WritableBaseFixture(string value)
    {
        Value = value;
    }
}

public sealed class DerivedLocalWriteFixture : WritableBaseFixture
{
    public DerivedLocalWriteFixture(string value)
        : base("constant")
    {
        Value = value;
    }
}
