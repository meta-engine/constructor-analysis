using ConstructorAnalysis.Models;
using Xunit;

namespace ConstructorAnalysis.Tests;

public sealed class UnsupportedSentinelTests
{
    [Fact]
    public void DoesNotInvokeUnsupportedStructConstructor()
    {
        var analysis = Assert.IsType<ConstructorFlowAnalysis>(
            new ConstructorFlowAnalyzer().Analyze(typeof(ThrowingValueParameterFixture)));

        var unsupported = Assert.Single(analysis.ParameterMappings, mapping => mapping.Parameter.Name == "value");
        Assert.Equal(ParameterInferenceOutcome.Unsupported, unsupported.Outcome);
        Assert.Empty(unsupported.PropertyMappings);
        var supported = Assert.Single(analysis.ParameterMappings, mapping => mapping.Parameter.Name == "name");
        Assert.Equal(ParameterInferenceOutcome.Inferred, supported.Outcome);
        Assert.Equal(nameof(ThrowingValueParameterFixture.Name), Assert.Single(supported.PropertyMappings).Property.Name);
    }
}
