namespace ConstructorAnalysis.Tests;

public sealed class PrivateGetterFixture
{
    public string Value { private get; set; }

    public PrivateGetterFixture(string value)
    {
        Value = value;
    }
}
