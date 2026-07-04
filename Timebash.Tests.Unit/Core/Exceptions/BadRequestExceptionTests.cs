using FluentAssertions;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Core.Exceptions;

public class BadRequestExceptionTests
{
    [Fact]
    public void Constructor_ShouldCreateWithCorrectProperties()
    {
        var exception = new BadRequestException();

        exception.StatusCode.Should().Be(400);
        exception.Title.Should().Be("Bad request");
    }

    [Fact]
    public void Constructor_WithMessage_ShouldCreateWithCorrectProperties()
    {
        var message = "Invalid request";
        var exception = new BadRequestException(message);

        exception.StatusCode.Should().Be(400);
        exception.Title.Should().Be("Bad request");
        exception.Message.Should().Be(message);
    }
}
