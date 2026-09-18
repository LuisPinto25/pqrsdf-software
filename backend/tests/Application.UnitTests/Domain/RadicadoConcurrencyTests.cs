using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Pqrsdf.Infrastructure.Persistence;
using Pqrsdf.Infrastructure.Services;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Domain;

public class RadicadoConcurrencyTests
{
    [Fact]
    public async Task NextAsync_FiftySimultaneousRequests_ProducesZeroDuplicatesAndExactSequence()
    {
        // Arrange: Shared in-memory database for concurrent workers
        var databaseName = $"RadicadoConcurrencyTest_{Guid.NewGuid()}";
        var options = new DbContextOptionsBuilder<PqrsdfDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        const int concurrencyLevel = 50;
        const int year = 2026;
        var generatedRadicados = new ConcurrentBag<string>();

        // Act: Launch 50 simultaneous asynchronous tasks
        var tasks = Enumerable.Range(0, concurrencyLevel).Select(async _ =>
        {
            await using var dbContext = new PqrsdfDbContext(options);
            var generator = new RadicadoSequenceGenerator(dbContext);
            var radicado = await generator.NextAsync(year);
            generatedRadicados.Add(radicado.Value);
        });

        await Task.WhenAll(tasks);

        // Assert:
        generatedRadicados.Should().HaveCount(concurrencyLevel);

        // 1. Zero duplicate numbers
        var distinctRadicados = generatedRadicados.Distinct().ToList();
        distinctRadicados.Should().HaveCount(concurrencyLevel, "no two concurrent tasks should ever receive the same radicado");

        // 2. Format matches YYYY-NNNNNNNN
        foreach (var radicado in generatedRadicados)
        {
            radicado.Should().MatchRegex(@"^2026-\d{8}$");
        }

        // 3. Complete sequence from 1 to 50
        var expectedRadicados = Enumerable.Range(1, concurrencyLevel)
            .Select(i => $"2026-{i:D8}")
            .ToHashSet();

        distinctRadicados.ToHashSet().Should().BeEquivalentTo(expectedRadicados);
    }
}
