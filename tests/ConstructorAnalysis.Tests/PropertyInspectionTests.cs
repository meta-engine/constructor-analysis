using ConstructorAnalysis.Models;
using Demo.ConsoleApp;
using Xunit;

namespace ConstructorAnalysis.Tests;

public sealed class PropertyInspectionTests
{
    [Fact]
    public void ReportsThrowingGetterWithoutReturningPartialMappings()
    {
        var analysis = Assert.IsType<ConstructorFlowAnalysis>(
            new ConstructorFlowAnalyzer().Analyze(typeof(ThrowingGetterFixture)));

        var mapping = Assert.Single(analysis.ParameterMappings);
        Assert.Equal(ParameterInferenceOutcome.InspectionFailed, mapping.Outcome);
        Assert.Empty(mapping.PropertyMappings);
        Assert.Empty(analysis.PropertiesSetInConstructor);
        Assert.Contains(nameof(ThrowingGetterFixture.Broken), mapping.Detail);
        Assert.Contains(nameof(InvalidOperationException), mapping.Detail);
        Assert.Contains("Getter unavailable", mapping.Detail);
    }

    [Fact]
    public void DoesNotInspectPrivateGetterOnPubliclyWritableProperty()
    {
        var analysis = Assert.IsType<ConstructorFlowAnalysis>(
            new ConstructorFlowAnalyzer().Analyze(typeof(PrivateGetterFixture)));

        var mapping = Assert.Single(analysis.ParameterMappings);
        Assert.Equal(ParameterInferenceOutcome.Unmatched, mapping.Outcome);
        Assert.Empty(mapping.PropertyMappings);
    }

    [Fact]
    public void KeepsDerivedObservationWhenDirectBaseGetterFails()
    {
        var analysis = Assert.IsType<ConstructorFlowAnalysis>(
            new ConstructorFlowAnalyzer().Analyze(typeof(DirectBaseGetterDerivedFixture)));

        var mapping = Assert.Single(analysis.ParameterMappings);
        Assert.Equal(ParameterInferenceOutcome.Inferred, mapping.Outcome);
        Assert.Equal(nameof(ThrowingGetterBaseFixture.Value), Assert.Single(mapping.PropertyMappings).Property.Name);
        Assert.Equal(ParameterInferenceOutcome.InspectionFailed, mapping.DirectBaseOutcome);
        Assert.Empty(mapping.DirectBaseMappings);
        Assert.Contains(nameof(ThrowingGetterBaseFixture.Broken), mapping.DirectBaseDetail);
    }

    [Fact]
    public void ConsoleReportsPropertyInspectionFailure()
    {
        var transcript = AnalysisPrinter.RenderType(
            new ConstructorFlowAnalyzer(), typeof(ThrowingGetterFixture));

        Assert.Contains("Parameter outcome: InspectionFailed", transcript);
        Assert.Contains("ThrowingGetterFixture.Broken", transcript);
        Assert.Contains("Getter unavailable", transcript);
        Assert.DoesNotContain("Assigned to properties:", transcript);
    }
}
