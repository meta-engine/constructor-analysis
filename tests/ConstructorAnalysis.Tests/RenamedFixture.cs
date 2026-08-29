namespace ConstructorAnalysis.Tests;

public class RenamedFixture
{
    public required string DisplayName { get; set; }
    public required int Age { get; set; }
    public RenamedFixture(string inputName, int age)
    {
        DisplayName = inputName;
        Age = age;
    }
}
