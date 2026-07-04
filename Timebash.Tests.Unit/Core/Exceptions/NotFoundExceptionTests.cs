using FluentAssertions;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Core.Exceptions;

public class NotFoundExceptionTests
{
    [Fact]
    public void Constructor_ShouldCreateWithCorrectProperties()
    {
        var exception = new NotFoundException();

        exception.StatusCode.Should().Be(404);
        exception.Title.Should().Be("Not found");
    }

    [Fact]
    public void Constructor_WithMessage_ShouldCreateWithCorrectProperties()
    {
        var message = "User not found";
        var exception = new NotFoundException(message);

        exception.StatusCode.Should().Be(404);
        exception.Title.Should().Be("Not found");
        exception.Message.Should().Be(message);
    }
}
