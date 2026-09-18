using FluentAssertions;
using MediatR;
using NetArchTest.Rules;
using Pqrsdf.Api.Controllers.V1;
using Pqrsdf.Application.Features.Pqrsdf.UseCases.MakePqrsdf;
using Pqrsdf.Domain.Entities;
using Pqrsdf.Domain.Repositories;
using Xunit;

namespace Pqrsdf.Architecture.Tests;

public class PqrsdfArchitectureTests
{
    private const string DomainNamespace = "Pqrsdf.Domain";
    private const string ApplicationNamespace = "Pqrsdf.Application";
    private const string InfrastructureNamespace = "Pqrsdf.Infrastructure";
    private const string ApiNamespace = "Pqrsdf.Api";

    [Fact]
    public void Pqrsdf_UseCases_Handlers_ShouldBeSealed()
    {
        var applicationAssembly = typeof(MakePqrsdfCommandHandler).Assembly;

        var testResult = Types
            .InAssembly(applicationAssembly)
            .That()
            .ResideInNamespace("Pqrsdf.Application.Features.Pqrsdf.UseCases")
            .And()
            .HaveNameEndingWith("Handler")
            .Should()
            .BeSealed()
            .GetResult();

        testResult.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Pqrsdf_UseCases_ShouldBeCoLocatedUnderFeaturesNamespace()
    {
        var applicationAssembly = typeof(MakePqrsdfCommand).Assembly;

        var testResult = Types
            .InAssembly(applicationAssembly)
            .That()
            .ImplementInterface(typeof(IRequest<>))
            .Should()
            .ResideInNamespace("Pqrsdf.Application.Features")
            .GetResult();

        testResult.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Pqrsdf_Domain_ShouldNotDependOnOuterLayers()
    {
        var domainAssembly = typeof(PqrsdfTicket).Assembly;

        var forbiddenDependencies = new[]
        {
            ApplicationNamespace,
            InfrastructureNamespace,
            ApiNamespace
        };

        var testResult = Types
            .InAssembly(domainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        testResult.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Api_Controllers_ShouldNotDependDirectlyOnRepositories()
    {
        var apiAssembly = typeof(PqrsdfController).Assembly;

        var forbiddenDependencies = new[]
        {
            typeof(IPqrsdfTicketRepository).Namespace!
        };

        var testResult = Types
            .InAssembly(apiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        testResult.IsSuccessful.Should().BeTrue();
    }
}
