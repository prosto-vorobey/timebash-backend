using FluentAssertions;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Core.Exceptions;

public class ConflictExceptionTests
{
    [Fact]
    public void Constructor_ShouldCreateWithCorrectProperties()
    {
        var exception = new ConflictException();

        exception.StatusCode.Should().Be(409);
        exception.Title.Should().Be("Conflict");
    }

    [Fact]
    public void Constructor_WithMessage_ShouldCreateWithCorrectProperties()
    {
        var message = "Operation conflict";
        var exception = new ConflictException(message);

        exception.StatusCode.Should().Be(409);
        exception.Title.Should().Be("Conflict");
        exception.Message.Should().Be(message);
    }
}
