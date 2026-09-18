using FluentAssertions;
using Pqrsdf.Domain.Shared.Results;
using Xunit;

namespace Pqrsdf.Domain.UnitTests.Shared.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success("TestPayload");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be("TestPayload");
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResultWithError()
    {
        // Arrange
        var error = Error.Validation("Test.Code", "Mensaje de prueba en español");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Error.Code.Should().Be("Test.Code");
        result.Error.Message.Should().Be("Mensaje de prueba en español");
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void AccessingValue_OnFailedResult_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Failure("Code", "Fallo"));

        // Act
        var act = () => _ = result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No se puede acceder al valor de un resultado fallido.");
    }
}
