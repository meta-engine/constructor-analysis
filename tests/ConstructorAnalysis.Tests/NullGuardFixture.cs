namespace ConstructorAnalysis.Tests;

public class NullGuardFixture
{
    public required string First { get; set; }
    public required string Second { get; set; }
    public static int InvocationCount { get; set; }
    public NullGuardFixture(string first, string second)
    {
        ArgumentNullException.ThrowIfNull(first);
        ArgumentNullException.ThrowIfNull(second);
        InvocationCount++;
        First = first;
        Second = second;
    }
}
