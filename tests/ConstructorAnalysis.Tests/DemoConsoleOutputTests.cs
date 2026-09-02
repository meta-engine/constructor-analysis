using ConstructorAnalysis;
using Demo.ConsoleApp;
using Demo.ConsoleApp.Examples;
using Xunit;

namespace ConstructorAnalysis.Tests;

public sealed class DemoConsoleOutputTests
{
    [Fact]
    public void UserTranscriptExposesParameterPropertyAndDirectBaseOutcomes()
    {
        var transcript = AnalysisPrinter.RenderType(new ConstructorFlowAnalyzer(), typeof(User));

        Assert.Equal(UserTranscript, transcript.Replace("\r\n", "\n"));
    }

    [Fact]
    public void EmployeeTranscriptReportsRenamedRoleLandingAndInferredBaseParameter()
    {
        var transcript = AnalysisPrinter.RenderType(new ConstructorFlowAnalyzer(), typeof(Employee));

        Assert.Contains("Parameter: userRole (Role)", transcript);
        Assert.Contains("Parameter outcome: Inferred", transcript);
        Assert.Contains("→ Employee.AccessLevel (Confidence: Exact; Provenance: ExactSentinel)", transcript);
        Assert.Contains("→ Person.Role (Confidence: Exact; Provenance: ExactSentinel)", transcript);
        Assert.Contains("Direct-base outcome: Inferred", transcript);
        Assert.Contains(
            "→ Base parameter [2] role via Person.Role " +
            "(Outcome: Inferred; Confidence: Exact; Provenance: ReadOnlyBaseSentinel)",
            transcript);
        Assert.DoesNotContain("? Candidate parameter", transcript);
    }

    [Fact]
    public void WritableBaseTranscriptKeepsTheCandidateMarker()
    {
        var transcript = AnalysisPrinter.RenderType(
            new ConstructorFlowAnalyzer(),
            typeof(DerivedLocalWriteFixture));

        Assert.Contains("Direct-base outcome: Ambiguous", transcript);
        Assert.Contains(
            "? Candidate parameter [0] value via WritableBaseFixture.Value " +
            "(Outcome: Ambiguous; Confidence: Heuristic; Provenance: DirectBasePropertyCorrelation)",
            transcript);
    }

    [Fact]
    public void NonInferredParameterTranscriptReportsOutcomeAndDetail()
    {
        var transcript = AnalysisPrinter.RenderType(
            new ConstructorFlowAnalyzer(),
            typeof(SingleBooleanFixture));

        Assert.Contains("Parameter outcome: Ambiguous", transcript);
        Assert.Contains("Boolean flow requires a contrast probe", transcript);
        Assert.DoesNotContain("Assigned to properties:", transcript);
    }

    [Fact]
    public void UnmatchedAndUnsupportedTranscriptsReportTheirTypedOutcomes()
    {
        var analyzer = new ConstructorFlowAnalyzer();

        var unmatched = AnalysisPrinter.RenderType(analyzer, typeof(UnmatchedFixture));
        var unsupported = AnalysisPrinter.RenderType(analyzer, typeof(UnsupportedValueFixture));

        Assert.Contains("Parameter outcome: Unmatched", unmatched);
        Assert.Contains("No property preserved the generated sentinel", unmatched);
        Assert.Contains("Parameter outcome: Unsupported", unsupported);
        Assert.Contains("has no collision-resistant sentinel strategy", unsupported);
    }

    [Fact]
    public void InstantiationFailureTranscriptReportsParameterAndDirectBaseDetails()
    {
        var transcript = AnalysisPrinter.RenderType(
            new ConstructorFlowAnalyzer(),
            typeof(ThrowingDerivedFixture));

        Assert.Contains("Parameter outcome: InstantiationFailed", transcript);
        Assert.Contains("Direct-base outcome: InstantiationFailed", transcript);
        Assert.Contains("Expected fixture failure", transcript);
    }

    [Fact]
    public void CandidateLessDirectBaseAmbiguityReportsItsDetail()
    {
        var transcript = AnalysisPrinter.RenderType(
            new ConstructorFlowAnalyzer(),
            typeof(OverloadedDerivedFixture));

        Assert.Contains("Direct-base outcome: Ambiguous", transcript);
        Assert.Contains("exposes multiple constructor overloads", transcript);
        Assert.DoesNotContain("? Candidate parameter", transcript);
    }

    private const string UserTranscript =
        """
        Type: User

        Constructor Parameters:
          - String email
          - Guid userId
          - String userName

        Parameter Flow Analysis:

          Parameter: email (String)
            Parameter outcome: Inferred
            Assigned to properties:
              → User.Email (Confidence: Exact; Provenance: ExactSentinel)

          Parameter: userId (Guid)
            Parameter outcome: Inferred
            Assigned to properties:
              → BaseEntity.Id (Confidence: Exact; Provenance: ExactSentinel)
            Direct-base outcome: Inferred
              → Base parameter [0] id via BaseEntity.Id (Outcome: Inferred; Confidence: Exact; Provenance: ReadOnlyBaseSentinel)

          Parameter: userName (String)
            Parameter outcome: Inferred
            Assigned to properties:
              → BaseEntity.Name (Confidence: Exact; Provenance: ExactSentinel)
              → User.Username (Confidence: Exact; Provenance: ExactSentinel)
            Direct-base outcome: Inferred
              → Base parameter [1] name via BaseEntity.Name (Outcome: Inferred; Confidence: Exact; Provenance: ReadOnlyBaseSentinel)

        Properties set in constructor:
          - BaseEntity.Id
          - BaseEntity.Name
          - User.Email
          - User.Username

        """;
}
