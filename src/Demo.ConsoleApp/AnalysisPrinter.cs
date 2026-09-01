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
            writer.WriteLine($"    Parameter outcome: {mapping.Outcome}");

            if (mapping.Detail is not null)
            {
                writer.WriteLine($"    Detail: {mapping.Detail}");
            }

            if (mapping.PropertyMappings.Count > 0)
            {
                writer.WriteLine("    Assigned to properties:");
                foreach (var propertyMapping in mapping.PropertyMappings)
                {
                    var prop = propertyMapping.Property;
                    var declaringClass = prop.DeclaringType?.Name ??
                        throw new InvalidOperationException($"Property '{prop.Name}' has no declaring type.");
                    writer.WriteLine(
                        $"      → {declaringClass}.{prop.Name} " +
                        $"(Confidence: {propertyMapping.Confidence}; " +
                        $"Provenance: {propertyMapping.Provenance})");
                }
            }

            if (mapping.DirectBaseOutcome != ParameterInferenceOutcome.Unmatched)
            {
                writer.WriteLine($"    Direct-base outcome: {mapping.DirectBaseOutcome}");
            }

            foreach (var baseCandidate in mapping.DirectBaseMappings)
            {
                var declaringClass = baseCandidate.CorrelatedProperty.DeclaringType?.Name ??
                    throw new InvalidOperationException(
                        $"Property '{baseCandidate.CorrelatedProperty.Name}' has no declaring type.");
                writer.WriteLine(
                    $"      ? Candidate parameter [{baseCandidate.ParameterIndex}] " +
                    $"{baseCandidate.ParameterName} via {declaringClass}.{baseCandidate.CorrelatedProperty.Name} " +
                    $"(Outcome: {baseCandidate.Outcome}; Confidence: {baseCandidate.Confidence}; " +
                    $"Provenance: {baseCandidate.Provenance})");
            }

            if (mapping.DirectBaseOutcome != ParameterInferenceOutcome.Unmatched &&
                mapping.DirectBaseMappings.Count == 0 &&
                mapping.DirectBaseDetail is not null)
            {
                writer.WriteLine($"      Detail: {mapping.DirectBaseDetail}");
            }
        }

        writer.WriteLine();
        writer.WriteLine("Properties set in constructor:");
        foreach (var prop in analysis.PropertiesSetInConstructor)
        {
            var declaringClass = prop.DeclaringType?.Name ??
                throw new InvalidOperationException($"Property '{prop.Name}' has no declaring type.");
            writer.WriteLine($"  - {declaringClass}.{prop.Name}");
        }
    }
}
