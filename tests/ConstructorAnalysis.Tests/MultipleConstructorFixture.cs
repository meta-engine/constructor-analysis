namespace ConstructorAnalysis.Tests;

public class MultipleConstructorFixture
{
    public required string First { get; set; }
    public string? Second { get; set; }
    public string? Third { get; set; }
    public MultipleConstructorFixture(string first)
    {
        First = first;
    }
    public MultipleConstructorFixture(string first, string second, string third)
    {
        First = first;
        Second = second;
        Third = third;
    }
}
