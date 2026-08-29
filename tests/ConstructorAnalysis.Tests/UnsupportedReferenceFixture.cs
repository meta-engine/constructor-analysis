namespace ConstructorAnalysis.Tests;

public abstract class AbstractDependencyFixture
{
}

public sealed class UnsupportedReferenceFixture
{
    public AbstractDependencyFixture? Dependency { get; }
    public string Name { get; }

    public UnsupportedReferenceFixture(AbstractDependencyFixture? dependency, string name)
    {
        Dependency = dependency;
        Name = name;
    }
}
