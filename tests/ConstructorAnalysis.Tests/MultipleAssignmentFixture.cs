namespace ConstructorAnalysis.Tests;

public class MultipleAssignmentFixture
{
    public required string Primary { get; set; }
    public required string Secondary { get; set; }
    public MultipleAssignmentFixture(string value)
    {
        Primary = value;
        Secondary = value;
    }
}
