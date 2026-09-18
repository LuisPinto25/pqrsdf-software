using FluentAssertions;
using Pqrsdf.Domain.Shared.ValueObjects;
using Xunit;

namespace Pqrsdf.Domain.UnitTests.Shared.ValueObjects;

public sealed record SampleRadicado(string Value) : ValueObject;

public class ValueObjectTests
{
    [Fact]
    public void Records_WithIdenticalValues_ShouldBeEqual()
    {
        // Arrange
        var r1 = new SampleRadicado("PQ-2026-001");
        var r2 = new SampleRadicado("PQ-2026-001");

        // Act & Assert
        r1.Should().Be(r2);
        (r1 == r2).Should().BeTrue();
        (r1 != r2).Should().BeFalse();
        r1.GetHashCode().Should().Be(r2.GetHashCode());
    }

    [Fact]
    public void Records_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var r1 = new SampleRadicado("PQ-2026-001");
        var r2 = new SampleRadicado("PQ-2026-002");

        // Act & Assert
        r1.Should().NotBe(r2);
        (r1 == r2).Should().BeFalse();
        (r1 != r2).Should().BeTrue();
    }
}
