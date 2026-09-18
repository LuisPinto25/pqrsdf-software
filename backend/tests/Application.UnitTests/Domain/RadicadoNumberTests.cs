using FluentAssertions;
using Pqrsdf.Domain.ValueObjects;
using Xunit;

namespace Pqrsdf.Application.UnitTests.Domain;

public class RadicadoNumberTests
{
    [Theory]
    [InlineData(2026, 1, "2026-00000001")]
    [InlineData(2026, 99999999, "2026-99999999")]
    [InlineData(2027, 42, "2027-00000042")]
    public void Create_WithYearAndSequence_ShouldReturnFormattedRadicadoNumber(int year, long sequence, string expected)
    {
        var result = RadicadoNumber.Create(year, sequence);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(expected);
        result.Value.GetYear().Should().Be(year);
        result.Value.GetSequence().Should().Be(sequence);
    }

    [Theory]
    [InlineData(1999, 1)]
    [InlineData(2101, 1)]
    [InlineData(2026, 0)]
    [InlineData(2026, -5)]
    [InlineData(2026, 100_000_000)]
    public void Create_WithInvalidYearOrSequence_ShouldReturnFailure(int year, long sequence)
    {
        var result = RadicadoNumber.Create(year, sequence);

        result.IsFailure.Should().BeTrue();
    }

    [Theory]
    [InlineData("2026-00000001")]
    [InlineData("2027-00000123")]
    public void Create_WithString_ShouldValidateFormat(string value)
    {
        var result = RadicadoNumber.Create(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("2026-1")]
    [InlineData("26-00000001")]
    [InlineData("2026-000000001")]
    [InlineData("ABCD-00000001")]
    [InlineData("2026_00000001")]
    public void Create_WithMalformedString_ShouldReturnFailure(string malformed)
    {
        var result = RadicadoNumber.Create(malformed);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Equality_TwoInstancesWithSameValue_ShouldBeEqual()
    {
        var r1 = RadicadoNumber.Create("2026-00000001").Value;
        var r2 = RadicadoNumber.Create(2026, 1).Value;

        r1.Should().Be(r2);
        (r1 == r2).Should().BeTrue();
    }
}
