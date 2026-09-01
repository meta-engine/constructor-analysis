namespace ConstructorAnalysis.Tests;

public class ThrowingBaseFixture
{
    protected ThrowingBaseFixture(string value)
    {
        throw new InvalidOperationException("Expected fixture failure.");
    }
}

public sealed class ThrowingDerivedFixture : ThrowingBaseFixture
{
    public ThrowingDerivedFixture(string value)
        : base(value)
    {
    }
}
