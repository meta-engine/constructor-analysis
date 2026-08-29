using ConstructorAnalysis.Models;
using Xunit;

namespace ConstructorAnalysis.Tests;

public sealed class ScalarSentinelFactoryTests
{
    [Theory]
    [InlineData(typeof(byte), 255)]
    [InlineData(typeof(sbyte), 127)]
    [InlineData(typeof(short), 32767)]
    [InlineData(typeof(ushort), 65535)]
    [InlineData(typeof(char), 65535)]
    public void ExhaustedNarrowScalarDoesNotRemainSupported(Type type, int index)
    {
        var factory = new SentinelValueFactory();

        var result = factory.Create(type, index, 0, 1);

        Assert.NotEqual(SentinelGenerationStatus.Supported, result.Status);
        Assert.Contains("distinct", result.Detail, StringComparison.OrdinalIgnoreCase);
    }
}
