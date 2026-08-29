namespace ConstructorAnalysis.Tests;

public class UnsupportedValueFixture
{
    public required string Name { get; set; }
    public required FixtureToken Token { get; set; }
    public UnsupportedValueFixture(FixtureToken token, string name)
    {
        Token = token;
        Name = name;
    }
}
