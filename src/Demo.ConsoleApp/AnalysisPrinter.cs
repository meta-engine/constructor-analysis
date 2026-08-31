using System.Text;
using ConstructorAnalysis;
using ConstructorAnalysis.Models;
using Demo.ConsoleApp.Examples;

namespace Demo.ConsoleApp;

public static class AnalysisPrinter
{
    public static string Render()
    {
        var output = new StringBuilder();
        using var writer = new StringWriter(output);
        Write(writer, new ConstructorFlowAnalyzer());
        return output.ToString();
    }

    public static void Write(TextWriter writer, ConstructorFlowAnalyzer analyzer)
    {
        writer.WriteLine("=== Runtime Constructor Flow Analysis Demo ===");
        writer.WriteLine();

        WriteExample(writer, analyzer, "Example 1: Basic Constructor", "─────────────────────────────", typeof(BasicExample));
        writer.WriteLine();
        writer.WriteLine();
        WriteExample(writer, analyzer, "Example 2: Inheritance with Base Class", "────────────────────────────────────────", typeof(User));
        writer.WriteLine();
        writer.WriteLine();
        WriteExample(writer, analyzer, "Example 3: Complex Inheritance", "───────────────────────────────", typeof(Employee));
    }

    public static string RenderType(ConstructorFlowAnalyzer analyzer, Type type)
    {
        var output = new StringBuilder();
        using var writer = new StringWriter(output);
        WriteType(writer, analyzer, type);
        return output.ToString();
    }

    private static void WriteExample(
        TextWriter writer,
        ConstructorFlowAnalyzer analyzer,
        string title,
        string underline,
        Type type)
    {
        writer.WriteLine(title);
        writer.WriteLine(underline);
        WriteType(writer, analyzer, type);
    }

    private static void WriteType(TextWriter writer, ConstructorFlowAnalyzer analyzer, Type type)
    {
        writer.WriteLine($"Type: {type.Name}");
        writer.WriteLine();

        var analysis = analyzer.Analyze(type);
        if (analysis is null)
        {
            writer.WriteLine("No constructor found.");
            return;
        }

        writer.WriteLine("Constructor Parameters:");
        foreach (var param in analysis.Constructor.GetParameters())
        {
            writer.WriteLine($"  - {param.ParameterType.Name} {param.Name}");
        }

        writer.WriteLine();
        writer.WriteLine("Parameter Flow Analysis:");
        foreach (var mapping in analysis.ParameterMappings)
        {
            writer.WriteLine();
            writer.WriteLine($"  Parameter: {mapping.Parameter.Name} ({mapping.Parameter.ParameterType.Name})");

            if (mapping.AssignedProperties.Any())
            {
                writer.WriteLine("    Assigned to properties:");
                foreach (var prop in mapping.AssignedProperties)
                {
                    var declaringClass = prop.DeclaringType?.Name ?? "Unknown";
                    writer.WriteLine($"      → {declaringClass}.{prop.Name}");
                }
            }

            foreach (var baseCandidate in mapping.DirectBaseMappings)
            {
                writer.WriteLine(
                    $"    ? Correlates with direct-base parameter [{baseCandidate.ParameterIndex}] " +
                    $"{baseCandidate.ParameterName} ({baseCandidate.Confidence}, {baseCandidate.Outcome})");
            }

            if (mapping.DirectBaseOutcome == ParameterInferenceOutcome.Ambiguous &&
                mapping.DirectBaseMappings.Count == 0)
            {
                writer.WriteLine($"    ? Direct-base flow is ambiguous: {mapping.DirectBaseDetail}");
            }
        }

        writer.WriteLine();
        writer.WriteLine("Properties set in constructor:");
        foreach (var prop in analysis.PropertiesSetInConstructor)
        {
            var declaringClass = prop.DeclaringType?.Name ?? "Unknown";
            writer.WriteLine($"  - {declaringClass}.{prop.Name}");
        }
    }
}
