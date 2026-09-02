using ConstructorAnalysis.Models;
using Xunit;

namespace ConstructorAnalysis.Tests;

public class DirectBaseFlowAnalyzerTests
{
    [Fact]
    public void InfersForwardingIntoReadOnlyBaseProperty()
    {
        var analysis = Analyze(typeof(ReadOnlyDerivedFixture));

        var input = Mapping(analysis, "input");
        var candidate = Assert.Single(input.DirectBaseMappings);
        Assert.Equal(0, candidate.ParameterIndex);
        Assert.Equal("value", candidate.ParameterName);
        Assert.Equal(nameof(ReadOnlyBaseFixture.Value), candidate.CorrelatedProperty.Name);
        Assert.Equal(ParameterInferenceOutcome.Inferred, candidate.Outcome);
        Assert.Equal(FlowMappingConfidence.Exact, candidate.Confidence);
        Assert.Equal(FlowMappingProvenance.ReadOnlyBaseSentinel, candidate.Provenance);
        Assert.Equal(ParameterInferenceOutcome.Inferred, input.DirectBaseOutcome);
        Assert.Null(input.DirectBaseDetail);
        Assert.True(input.IsPassedToBase);

        var local = Mapping(analysis, "local");
        Assert.Empty(local.DirectBaseMappings);
        Assert.Equal(ParameterInferenceOutcome.Unmatched, local.DirectBaseOutcome);
        Assert.False(local.IsPassedToBase);
    }

    [Theory]
    [InlineData(typeof(InitDerivedFixture))]
    [InlineData(typeof(FieldBackedDerivedFixture))]
    public void KeepsWritableBasePropertyCorrelationHeuristic(Type type)
    {
        var mapping = Mapping(Analyze(type), "value");

        var candidate = Assert.Single(mapping.DirectBaseMappings);
        Assert.Equal(ParameterInferenceOutcome.Ambiguous, candidate.Outcome);
        Assert.Equal(FlowMappingConfidence.Heuristic, candidate.Confidence);
        Assert.Equal(FlowMappingProvenance.DirectBasePropertyCorrelation, candidate.Provenance);
        Assert.Equal(ParameterInferenceOutcome.Ambiguous, mapping.DirectBaseOutcome);
        Assert.Contains("derived-constructor write", mapping.DirectBaseDetail);
        Assert.False(mapping.IsPassedToBase);
    }

    [Fact]
    public void MapsSwappedSameTypeArgumentsToOrderedDirectBaseParameters()
    {
        var analysis = Analyze(typeof(SwappedDerivedFixture));

        var first = Mapping(analysis, "firstInput");
        var firstBase = Assert.Single(first.DirectBaseMappings);
        Assert.Equal(1, firstBase.ParameterIndex);
        Assert.Equal("first", firstBase.ParameterName);
        Assert.Equal(ParameterInferenceOutcome.Ambiguous, firstBase.Outcome);
        Assert.Equal(FlowMappingConfidence.Heuristic, firstBase.Confidence);
        Assert.Equal(FlowMappingProvenance.DirectBasePropertyCorrelation, firstBase.Provenance);
        Assert.True(first.HasDirectBaseCandidate);
        Assert.False(first.IsPassedToBase);
        Assert.Contains(first.AssignedProperties, property => property.Name == nameof(SwappedBaseFixture.FirstValue));
        Assert.Contains(first.AssignedProperties, property => property.Name == nameof(SwappedDerivedFixture.LocalCopy));

        var second = Mapping(analysis, "secondInput");
        var secondBase = Assert.Single(second.DirectBaseMappings);
        Assert.Equal(0, secondBase.ParameterIndex);
        Assert.Equal("second", secondBase.ParameterName);
        Assert.Equal(new[] { 0, 1 }, analysis.ParameterMappings.SelectMany(mapping => mapping.DirectBaseMappings).OrderBy(mapping => mapping.ParameterIndex).Select(mapping => mapping.ParameterIndex));
    }

    [Fact]
    public void ReportsOverloadedDirectBaseAsAmbiguousWithoutChoosingAnOverload()
    {
        var mapping = Mapping(Analyze(typeof(OverloadedDerivedFixture)), "value");

        Assert.Equal(ParameterInferenceOutcome.Ambiguous, mapping.DirectBaseOutcome);
        Assert.Contains("overload", mapping.DirectBaseDetail, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(mapping.DirectBaseMappings);
        Assert.False(mapping.IsPassedToBase);
    }

    [Fact]
    public void ReportsDerivedWriteCorrelationAsAHeuristicCandidate()
    {
        var mapping = Mapping(Analyze(typeof(DerivedLocalWriteFixture)), "value");

        var candidate = Assert.Single(mapping.DirectBaseMappings);
        Assert.Equal(ParameterInferenceOutcome.Ambiguous, mapping.DirectBaseOutcome);
        Assert.Equal(ParameterInferenceOutcome.Ambiguous, candidate.Outcome);
        Assert.Equal(FlowMappingConfidence.Heuristic, candidate.Confidence);
        Assert.False(mapping.IsPassedToBase);
    }

    private static ConstructorFlowAnalysis Analyze(Type type)
    {
        return Assert.IsType<ConstructorFlowAnalysis>(new ConstructorFlowAnalyzer().Analyze(type));
    }

    private static ParameterMapping Mapping(ConstructorFlowAnalysis analysis, string parameterName)
    {
        return Assert.Single(analysis.ParameterMappings, mapping => mapping.Parameter.Name == parameterName);
    }
}
