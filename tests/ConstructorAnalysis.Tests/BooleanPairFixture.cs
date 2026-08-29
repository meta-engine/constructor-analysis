namespace ConstructorAnalysis.Tests;

public class BooleanPairFixture
{
    public required bool First { get; set; }
    public required bool Second { get; set; }
    public BooleanPairFixture(bool first, bool second)
    {
        First = first;
        Second = second;
    }
}
