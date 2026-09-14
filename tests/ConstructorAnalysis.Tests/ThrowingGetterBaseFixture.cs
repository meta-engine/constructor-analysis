namespace ConstructorAnalysis.Tests;

public class ThrowingGetterBaseFixture
{
    public string Value { get; }
    public virtual string Broken => throw new InvalidOperationException("Base getter unavailable");

    protected ThrowingGetterBaseFixture(string value)
    {
        Value = value;
    }
}
