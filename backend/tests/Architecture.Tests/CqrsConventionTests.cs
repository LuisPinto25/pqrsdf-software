using FluentAssertions;
using MediatR;
using NetArchTest.Rules;
using Pqrsdf.Application.Features.System.UseCases.GetSystemStatus;
using Xunit;

namespace Pqrsdf.Architecture.Tests;

public class CqrsConventionTests
{
    [Fact]
    public void CommandAndQueryHandlers_ShouldResideInFeaturesNamespace()
    {
        // Arrange
        var applicationAssembly = typeof(GetSystemStatusQuery).Assembly;

        // Act
        var testResult = Types
            .InAssembly(applicationAssembly)
            .That()
            .ImplementInterface(typeof(IRequestHandler<,>))
            .Should()
            .ResideInNamespace("Pqrsdf.Application.Features")
            .GetResult();

        // Assert
        testResult.IsSuccessful.Should().BeTrue();
    }
}
