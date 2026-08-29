using ConstructorAnalysis;
using Demo.ConsoleApp.Examples;

Console.WriteLine("=== Runtime Constructor Flow Analysis Demo ===\n");

var analyzer = new ConstructorFlowAnalyzer();

Console.WriteLine("Example 1: Basic Constructor");
Console.WriteLine("─────────────────────────────");
AnalyzeType(analyzer, typeof(BasicExample));

Console.WriteLine("\n");

Console.WriteLine("Example 2: Inheritance with Base Class");
Console.WriteLine("────────────────────────────────────────");
AnalyzeType(analyzer, typeof(User));

Console.WriteLine("\n");

Console.WriteLine("Example 3: Complex Inheritance");
Console.WriteLine("───────────────────────────────");
AnalyzeType(analyzer, typeof(Employee));

static void AnalyzeType(ConstructorFlowAnalyzer analyzer, Type type)
{
    Console.WriteLine($"Type: {type.Name}\n");

    var analysis = analyzer.Analyze(type);

    if (analysis == null)
    {
        Console.WriteLine("No constructor found.");
        return;
    }

    Console.WriteLine("Constructor Parameters:");
    foreach (var param in analysis.Constructor.GetParameters())
    {
        Console.WriteLine($"  - {param.ParameterType.Name} {param.Name}");
    }

    Console.WriteLine("\nParameter Flow Analysis:");
    foreach (var mapping in analysis.ParameterMappings)
    {
        Console.WriteLine($"\n  Parameter: {mapping.Parameter.Name} ({mapping.Parameter.ParameterType.Name})");

        if (mapping.AssignedProperties.Any())
        {
            Console.WriteLine("    Assigned to properties:");
            foreach (var prop in mapping.AssignedProperties)
            {
                var declaringClass = prop.DeclaringType?.Name ?? "Unknown";
                Console.WriteLine($"      → {declaringClass}.{prop.Name}");
            }
        }

        foreach (var baseCandidate in mapping.DirectBaseMappings)
        {
            Console.WriteLine(
                $"    ? Correlates with direct-base parameter [{baseCandidate.ParameterIndex}] " +
                $"{baseCandidate.ParameterName} ({baseCandidate.Confidence}, {baseCandidate.Outcome})");
        }

        if (mapping.DirectBaseOutcome == ConstructorAnalysis.Models.ParameterInferenceOutcome.Ambiguous &&
            mapping.DirectBaseMappings.Count == 0)
        {
            Console.WriteLine($"    ? Direct-base flow is ambiguous: {mapping.DirectBaseDetail}");
        }
    }

    Console.WriteLine("\nProperties set in constructor:");
    foreach (var prop in analysis.PropertiesSetInConstructor)
    {
        var declaringClass = prop.DeclaringType?.Name ?? "Unknown";
        Console.WriteLine($"  - {declaringClass}.{prop.Name}");
    }
}
