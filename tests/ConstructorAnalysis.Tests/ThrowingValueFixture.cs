namespace ConstructorAnalysis.Tests;

public struct ThrowingValueFixture
{
    public ThrowingValueFixture()
    {
        throw new InvalidOperationException("A parameter sentinel must not run this constructor.");
    }
}
