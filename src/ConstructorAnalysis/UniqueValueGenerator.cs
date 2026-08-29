using System.Reflection;
using ConstructorAnalysis.Models;

namespace ConstructorAnalysis;

internal sealed class UniqueValueGenerator
{
    private readonly SentinelValueFactory _valueFactory;
    private readonly SentinelDistinctnessValidator _distinctnessValidator;

    public UniqueValueGenerator(
        SentinelValueFactory valueFactory,
        SentinelDistinctnessValidator distinctnessValidator)
    {
        _valueFactory = valueFactory;
        _distinctnessValidator = distinctnessValidator;
    }

    public IReadOnlyList<SentinelArgument> CreateArguments(IReadOnlyList<ParameterInfo> parameters)
    {
        var typeCounts = parameters
            .GroupBy(parameter => parameter.ParameterType)
            .ToDictionary(group => group.Key, group => group.Count());
        var arguments = parameters
            .Select((parameter, index) => CreateArgument(
                parameters,
                parameter,
                index,
                typeCounts[parameter.ParameterType]))
            .ToArray();

        return _distinctnessValidator.MarkCollisions(arguments);
    }

    private SentinelArgument CreateArgument(
        IReadOnlyList<ParameterInfo> parameters,
        ParameterInfo parameter,
        int index,
        int sameTypeCount)
    {
        var occurrence = parameters
            .Take(index)
            .Count(previous => previous.ParameterType == parameter.ParameterType);
        var value = _valueFactory.Create(parameter.ParameterType, index, occurrence, sameTypeCount);

        return new SentinelArgument
        {
            Parameter = parameter,
            Value = value.Value,
            Status = value.Status,
            Detail = value.Detail
        };
    }
}
