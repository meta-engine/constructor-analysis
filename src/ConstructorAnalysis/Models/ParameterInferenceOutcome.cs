namespace ConstructorAnalysis.Models;

public enum ParameterInferenceOutcome
{
    Inferred,
    Unmatched,
    Ambiguous,
    Unsupported,
    InstantiationFailed,
    InspectionFailed
}
