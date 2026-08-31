using ConstructorAnalysis;
using Demo.ConsoleApp;
using Demo.ConsoleApp.Examples;
using Xunit;

namespace ConstructorAnalysis.Tests;

public sealed class DemoConsoleOutputTests
{
    [Fact]
    public void UserTranscriptMatchesThePublishedConsoleContract()
    {
        var transcript = AnalysisPrinter.RenderType(new ConstructorFlowAnalyzer(), typeof(User));

        Assert.Equal(UserTranscript, transcript.Replace("\r\n", "\n"));
    }

    [Fact]
    public void EmployeeTranscriptReportsRenamedRoleLandingAndBaseCandidate()
    {
        var transcript = AnalysisPrinter.RenderType(new ConstructorFlowAnalyzer(), typeof(Employee));

        Assert.Contains("Parameter: userRole (Role)", transcript);
        Assert.Contains("→ Employee.AccessLevel", transcript);
        Assert.Contains("→ Person.Role", transcript);
        Assert.Contains(
            "? Correlates with direct-base parameter [2] role (Heuristic, Ambiguous)",
            transcript);
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
            Assigned to properties:
              → User.Email

          Parameter: userId (Guid)
            Assigned to properties:
              → BaseEntity.Id
            ? Correlates with direct-base parameter [0] id (Heuristic, Ambiguous)

          Parameter: userName (String)
            Assigned to properties:
              → BaseEntity.Name
              → User.Username
            ? Correlates with direct-base parameter [1] name (Heuristic, Ambiguous)

        Properties set in constructor:
          - BaseEntity.Id
          - BaseEntity.Name
          - User.Email
          - User.Username

        """;
}
