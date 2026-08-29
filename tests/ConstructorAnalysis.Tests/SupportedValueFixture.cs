namespace ConstructorAnalysis.Tests;

public class SupportedValueFixture
{
    public required FixtureStatus Status { get; set; }
    public required int? Count { get; set; }
    public SupportedValueFixture(FixtureStatus status, int? count)
    {
        Status = status;
        Count = count;
    }
}
