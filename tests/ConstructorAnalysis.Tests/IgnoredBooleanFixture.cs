namespace ConstructorAnalysis.Tests;

public sealed class IgnoredBooleanFixture
{
    public bool Enabled => true;

    public IgnoredBooleanFixture(bool enabled)
    {
    }
}
