namespace ConstructorAnalysis.Tests;

public sealed class SingleBooleanFixture
{
    public bool Enabled { get; }

    public SingleBooleanFixture(bool enabled)
    {
        Enabled = enabled;
    }
}
