namespace ConstructorAnalysis.Tests;

public sealed class ThrowingValueParameterFixture
{
    public ThrowingValueFixture Value { get; }
    public string Name { get; }

    public ThrowingValueParameterFixture(ThrowingValueFixture value, string name)
    {
        Value = value;
        Name = name;
    }
}
