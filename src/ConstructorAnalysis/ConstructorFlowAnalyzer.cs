using System.Reflection;
using ConstructorAnalysis.Models;

namespace ConstructorAnalysis;

public sealed class ConstructorFlowAnalyzer
{
    private readonly ConstructorSelector _constructorSelector;
    private readonly InstanceStateCreator _instanceStateCreator;
    private readonly PropertyFlowMatcher _propertyFlowMatcher;
    private readonly DirectBaseFlowAnalyzer _directBaseFlowAnalyzer;

    public ConstructorFlowAnalyzer()
    {
        _constructorSelector = new ConstructorSelector();
        var valueGenerator = new UniqueValueGenerator(
            new SentinelValueFactory(),
            new SentinelDistinctnessValidator());
        _instanceStateCreator = new InstanceStateCreator(valueGenerator);
        _propertyFlowMatcher = new PropertyFlowMatcher(new PropertyStateReader());
        _directBaseFlowAnalyzer = new DirectBaseFlowAnalyzer(
            _constructorSelector,
            _instanceStateCreator,
            _propertyFlowMatcher,
            new DirectBaseCandidateFactory(new ReadOnlyAutoPropertyDetector()));
    }

    internal ConstructorFlowAnalyzer(
        ConstructorSelector constructorSelector,
        InstanceStateCreator instanceStateCreator,
        PropertyFlowMatcher propertyFlowMatcher,
        DirectBaseFlowAnalyzer directBaseFlowAnalyzer)
    {
        _constructorSelector = constructorSelector;
        _instanceStateCreator = instanceStateCreator;
        _propertyFlowMatcher = propertyFlowMatcher;
        _directBaseFlowAnalyzer = directBaseFlowAnalyzer;
    }

    public ConstructorFlowAnalysis? Analyze(Type type)
    {
        var constructor = _constructorSelector.Select(type);
        if (constructor is null)
        {
            return null;
        }

        return AnalyzeConstructor(type, constructor);
    }

    public ConstructorFlowAnalysis AnalyzeConstructor(Type type, ConstructorInfo constructor)
    {
        var state = _instanceStateCreator.Execute(constructor);
        var mappings = _propertyFlowMatcher.Match(type, state);
        _directBaseFlowAnalyzer.Apply(type, mappings);

        var properties = mappings
            .SelectMany(mapping => mapping.AssignedProperties)
            .Distinct()
            .OrderBy(property => property.DeclaringType?.FullName, StringComparer.Ordinal)
            .ThenBy(property => property.MetadataToken)
            .ToArray();

        return new ConstructorFlowAnalysis
        {
            Constructor = constructor,
            ParameterMappings = mappings,
            PropertiesSetInConstructor = properties
        };
    }
}
