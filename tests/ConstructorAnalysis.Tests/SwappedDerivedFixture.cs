namespace ConstructorAnalysis.Tests;

public class SwappedDerivedFixture : SwappedBaseFixture
{
    public required string LocalCopy { get; set; }
    public SwappedDerivedFixture(string firstInput, string secondInput) : base(secondInput, firstInput)
    {
        LocalCopy = firstInput;
    }
}
