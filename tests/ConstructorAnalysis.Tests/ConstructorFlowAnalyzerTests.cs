using ConstructorAnalysis;
using ConstructorAnalysis.Models;
using Xunit;
using System.Linq;

namespace ConstructorAnalysis.Tests;

public class ConstructorFlowAnalyzerTests
{
    [Fact]
    public void MapsExactAndRenamedValues()
    {
        var analysis = Analyze<RenamedFixture>();

        var name = Mapping(analysis, "inputName");
        Assert.Equal(ParameterInferenceOutcome.Inferred, name.Outcome);
        var property = Assert.Single(name.PropertyMappings);
        Assert.Equal(nameof(RenamedFixture.DisplayName), property.Property.Name);
        Assert.Equal(FlowMappingConfidence.Exact, property.Confidence);
        Assert.Equal(FlowMappingProvenance.ExactSentinel, property.Provenance);

        var age = Mapping(analysis, "age");
        Assert.Equal(nameof(RenamedFixture.Age), Assert.Single(age.AssignedProperties).Name);
    }
    [Fact]
    public void MapsOneParameterToMultipleProperties()
    {
        var mapping = Mapping(Analyze<MultipleAssignmentFixture>(), "value");

        Assert.Equal(ParameterInferenceOutcome.Inferred, mapping.Outcome);
        Assert.Equal(new[] { nameof(MultipleAssignmentFixture.Primary), nameof(MultipleAssignmentFixture.Secondary) }, mapping.AssignedProperties.Select(property => property.Name));
    }
    [Fact]
    public void UsesOneConstructorInvocationForNullGuardedParameters()
    {
        NullGuardFixture.InvocationCount = 0;

        var analysis = Analyze<NullGuardFixture>();

        Assert.Equal(1, NullGuardFixture.InvocationCount);
        Assert.All(analysis.ParameterMappings, mapping => Assert.Equal(ParameterInferenceOutcome.Inferred, mapping.Outcome));
    }
    [Fact]
    public void ReportsClassAndInterfaceSentinelsAsExact()
    {
        var analysis = Analyze<ReferenceFixture>();

        foreach (var mapping in analysis.ParameterMappings)
        {
            Assert.Equal(ParameterInferenceOutcome.Inferred, mapping.Outcome);
            Assert.Equal(FlowMappingProvenance.ExactSentinel, Assert.Single(mapping.PropertyMappings).Provenance);
        }
    }
    [Fact]
    public void ReportsTransformedStringMatchesAsHeuristic()
    {
        var analysis = Analyze<TransformedFixture>();

        foreach (var parameterName in new[] { "email", "first", "last" })
        {
            var mapping = Mapping(analysis, parameterName);
            Assert.Equal(ParameterInferenceOutcome.Inferred, mapping.Outcome);
            Assert.All(mapping.PropertyMappings, property =>
            {
                Assert.Equal(FlowMappingConfidence.Heuristic, property.Confidence);
                Assert.Equal(FlowMappingProvenance.TransformedStringContainment, property.Provenance);
            });
        }
    }
    [Fact]
    public void ReportsBooleanCollisionAsAmbiguous()
    {
        var analysis = Analyze<BooleanPairFixture>();

        Assert.All(analysis.ParameterMappings, mapping =>
        {
            Assert.Equal(ParameterInferenceOutcome.Ambiguous, mapping.Outcome);
            Assert.Empty(mapping.PropertyMappings);
        });
    }
    [Fact]
    public void DoesNotOverclaimOneAssignedBooleanSentinel()
    {
        var mapping = Mapping(Analyze<SingleBooleanFixture>(), "enabled");

        Assert.Equal(ParameterInferenceOutcome.Ambiguous, mapping.Outcome);
        Assert.Empty(mapping.PropertyMappings);
    }

    [Fact]
    public void DoesNotMistakeAnIgnoredBooleanForAConstantTrueProperty()
    {
        var mapping = Mapping(Analyze<IgnoredBooleanFixture>(), "enabled");

        Assert.Equal(ParameterInferenceOutcome.Ambiguous, mapping.Outcome);
        Assert.Empty(mapping.PropertyMappings);
    }
    [Fact]
    public void SupportsEnumAndNullableSentinels()
    {
        var analysis = Analyze<SupportedValueFixture>();

        Assert.All(analysis.ParameterMappings, mapping => Assert.Equal(ParameterInferenceOutcome.Inferred, mapping.Outcome));
        Assert.Equal(nameof(SupportedValueFixture.Status), Assert.Single(Mapping(analysis, "status").AssignedProperties).Name);
        Assert.Equal(nameof(SupportedValueFixture.Count), Assert.Single(Mapping(analysis, "count").AssignedProperties).Name);
    }
    [Fact]
    public void ReportsCustomStructAsUnsupportedWithoutDefaultMatch()
    {
        var analysis = Analyze<UnsupportedValueFixture>();

        var token = Mapping(analysis, "token");
        Assert.Equal(ParameterInferenceOutcome.Unsupported, token.Outcome);
        Assert.Empty(token.PropertyMappings);
        Assert.Contains("struct", token.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(ParameterInferenceOutcome.Inferred, Mapping(analysis, "name").Outcome);
    }
    [Fact]
    public void ReportsUnconstructableReferenceSentinelAsUnsupportedWithoutThrowing()
    {
        var analysis = Analyze<UnsupportedReferenceFixture>();

        var dependency = Mapping(analysis, "dependency");
        Assert.Equal(ParameterInferenceOutcome.Unsupported, dependency.Outcome);
        Assert.Empty(dependency.PropertyMappings);
        Assert.Contains("assignable sentinel", dependency.Detail, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(ParameterInferenceOutcome.Inferred, Mapping(analysis, "name").Outcome);
    }
    [Fact]
    public void MapsSwappedSameTypeArgumentsToOrderedDirectBaseParameters()
    {
        var analysis = Analyze<SwappedDerivedFixture>();

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
        var mapping = Mapping(Analyze<OverloadedDerivedFixture>(), "value");

        Assert.Equal(ParameterInferenceOutcome.Ambiguous, mapping.DirectBaseOutcome);
        Assert.Contains("overload", mapping.DirectBaseDetail, StringComparison.OrdinalIgnoreCase);
        Assert.Empty(mapping.DirectBaseMappings);
        Assert.False(mapping.IsPassedToBase);
    }

    [Fact]
    public void ReportsDerivedWriteCorrelationAsAHeuristicCandidate()
    {
        var mapping = Mapping(Analyze<DerivedLocalWriteFixture>(), "value");

        var candidate = Assert.Single(mapping.DirectBaseMappings);
        Assert.Equal(ParameterInferenceOutcome.Ambiguous, mapping.DirectBaseOutcome);
        Assert.Equal(ParameterInferenceOutcome.Ambiguous, candidate.Outcome);
        Assert.Equal(FlowMappingConfidence.Heuristic, candidate.Confidence);
        Assert.False(mapping.IsPassedToBase);
    }
    [Fact]
    public void ReturnsNullWhenTypeHasNoConstructor()
    {
        var analyzer = new ConstructorFlowAnalyzer();

        Assert.Null(analyzer.Analyze(typeof(INoConstructorFixture)));
    }
    [Fact]
    public void SelectsRichestConstructor()
    {
        var analysis = Analyze<MultipleConstructorFixture>();

        Assert.Equal(3, analysis.Constructor.GetParameters().Length);
        Assert.Equal(3, analysis.ParameterMappings.Count);
    }
    [Fact]
    public void RepeatedRunsHaveDeterministicResults()
    {
        var analyzer = new ConstructorFlowAnalyzer();

        var first = Assert.IsType<ConstructorFlowAnalysis>(analyzer.Analyze(typeof(SwappedDerivedFixture)));
        var second = Assert.IsType<ConstructorFlowAnalysis>(analyzer.Analyze(typeof(SwappedDerivedFixture)));

        var firstProjection = first.ParameterMappings.Select(Project).ToArray();
        var secondProjection = second.ParameterMappings.Select(Project).ToArray();
        Assert.Equal(firstProjection, secondProjection);
    }
    private static ConstructorFlowAnalysis Analyze<T>()
    {
        return Assert.IsType<ConstructorFlowAnalysis>(new ConstructorFlowAnalyzer().Analyze(typeof(T)));
    }
    private static ParameterMapping Mapping(ConstructorFlowAnalysis analysis, string parameterName)
    {
        return Assert.Single(analysis.ParameterMappings, mapping => mapping.Parameter.Name == parameterName);
    }
    private static string Project(ParameterMapping mapping)
    {
        var properties = string.Join(",", mapping.PropertyMappings.Select(property => $"{property.Property.DeclaringType?.Name}.{property.Property.Name}:{property.Provenance}"));
        var baseParameters = string.Join(",", mapping.DirectBaseMappings.Select(baseMapping => $"{baseMapping.ParameterIndex}:{baseMapping.ParameterName}:{baseMapping.Outcome}:{baseMapping.Confidence}"));
        return $"{mapping.Parameter.Position}:{mapping.Outcome}:{properties}:{mapping.DirectBaseOutcome}:{baseParameters}";
    }
}
