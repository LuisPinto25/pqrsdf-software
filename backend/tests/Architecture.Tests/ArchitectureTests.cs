using FluentAssertions;
using NetArchTest.Rules;
using Pqrsdf.Application.Features.System.UseCases.GetSystemStatus;
using Pqrsdf.Domain.Shared.Entities;
using Pqrsdf.Infrastructure.Persistence;
using Xunit;

namespace Pqrsdf.Architecture.Tests;

public class ArchitectureTests
{
    private const string DomainNamespace = "Pqrsdf.Domain";
    private const string ApplicationNamespace = "Pqrsdf.Application";
    private const string InfrastructureNamespace = "Pqrsdf.Infrastructure";
    private const string ApiNamespace = "Pqrsdf.Api";

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnOtherProjects()
    {
        // Arrange
        var domainAssembly = typeof(Entity<>).Assembly;

        var otherProjects = new[]
        {
            ApplicationNamespace,
            InfrastructureNamespace,
            ApiNamespace
        };

        // Act
        var testResult = Types
            .InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(otherProjects)
            .GetResult();

        // Assert
        testResult.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnInfrastructureOrApi()
    {
        // Arrange
        var applicationAssembly = typeof(GetSystemStatusQuery).Assembly;

        var forbiddenProjects = new[]
        {
            InfrastructureNamespace,
            ApiNamespace
        };

        // Act
        var testResult = Types
            .InAssembly(applicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenProjects)
            .GetResult();

        // Assert
        testResult.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Infrastructure_ShouldNotHaveDependencyOnApi()
    {
        // Arrange
        var infrastructureAssembly = typeof(PqrsdfDbContext).Assembly;

        // Act
        var testResult = Types
            .InAssembly(infrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApiNamespace)
            .GetResult();

        // Assert
        testResult.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Handlers_ShouldHaveNameEndingWith_Handler()
    {
        // Arrange
        var applicationAssembly = typeof(GetSystemStatusQuery).Assembly;

        // Act
        var testResult = Types
            .InAssembly(applicationAssembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        // Assert
        testResult.IsSuccessful.Should().BeTrue();
    }
}
