namespace ConstructorAnalysis.Tests;

public sealed class DirectBaseGetterDerivedFixture : ThrowingGetterBaseFixture
{
    public override string Broken => "Derived getter is available";

    public DirectBaseGetterDerivedFixture(string value) : base(value)
    {
    }
}
