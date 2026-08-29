using System.Reflection;
using ConstructorAnalysis.Models;

namespace ConstructorAnalysis;

internal sealed class InstanceStateCreator
{
    private readonly UniqueValueGenerator _valueGenerator;

    public InstanceStateCreator(UniqueValueGenerator valueGenerator)
    {
        _valueGenerator = valueGenerator;
    }

    public InstanceStateResult Execute(ConstructorInfo constructor)
    {
        var parameters = constructor.GetParameters();
        var arguments = _valueGenerator.CreateArguments(parameters);

        try
        {
            var instanceValue = constructor.Invoke(arguments.Select(argument => argument.Value).ToArray());
            if (instanceValue is null)
            {
                return Failed(arguments, "Constructor invocation returned no instance.");
            }

            return new InstanceStateResult
            {
                InstanceValue = instanceValue,
                Arguments = arguments
            };
        }
        catch (TargetInvocationException exception)
        {
            var cause = exception.InnerException ?? exception;
            return Failed(arguments, $"Constructor invocation failed with {cause.GetType().Name}: {cause.Message}");
        }
        catch (ArgumentException exception)
        {
            return Failed(arguments, $"Constructor arguments could not be bound: {exception.Message}");
        }
        catch (MemberAccessException exception)
        {
            return Failed(arguments, $"Constructor could not be accessed: {exception.Message}");
        }
        catch (NotSupportedException exception)
        {
            return Failed(arguments, $"Constructor invocation is not supported: {exception.Message}");
        }
    }

    private InstanceStateResult Failed(IReadOnlyList<SentinelArgument> arguments, string detail)
    {
        return new InstanceStateResult
        {
            Arguments = arguments,
            Failure = new InstanceCreationFailure(detail)
        };
    }
}
