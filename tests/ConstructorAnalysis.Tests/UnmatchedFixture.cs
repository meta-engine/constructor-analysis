namespace ConstructorAnalysis.Tests;

public sealed class UnmatchedFixture
{
    public string ConstantValue => "constant";

    public UnmatchedFixture(string ignored)
    {
    }
}
