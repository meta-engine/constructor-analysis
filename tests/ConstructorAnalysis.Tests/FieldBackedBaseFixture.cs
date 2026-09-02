namespace ConstructorAnalysis.Tests;

public class FieldBackedBaseFixture
{
    protected string BackingValue;

    public string Value => BackingValue;

    protected FieldBackedBaseFixture(string value)
    {
        BackingValue = value;
    }
}

public sealed class FieldBackedDerivedFixture : FieldBackedBaseFixture
{
    public FieldBackedDerivedFixture(string value)
        : base("constant")
    {
        BackingValue = value;
    }
}
