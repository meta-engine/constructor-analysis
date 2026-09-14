namespace ConstructorAnalysis.Tests;

public sealed class ThrowingGetterFixture
{
    public string Value { get; }
    public string Broken => throw new InvalidOperationException("Getter unavailable");

    public ThrowingGetterFixture(string value)
    {
        Value = value;
    }
}
