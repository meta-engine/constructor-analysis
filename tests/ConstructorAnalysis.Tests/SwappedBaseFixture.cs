namespace ConstructorAnalysis.Tests;

public class SwappedBaseFixture
{
    public required string FirstValue { get; set; }
    public required string SecondValue { get; set; }
    protected SwappedBaseFixture(string second, string first)
    {
        SecondValue = second;
        FirstValue = first;
    }
}
