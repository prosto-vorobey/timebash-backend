using FluentAssertions;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Core.Exceptions;

public class ResourceConflictExceptionTests
{
    [Fact]
    public void Constructor_ShouldCreateWithCorrectProperties()
    {
        var exception = new ResourceConflictException("Name");

        exception.StatusCode.Should().Be(409);
        exception.Title.Should().Be("Conflict");
        exception.Field.Should().Be("Name");
    }

    [Fact]
    public void Constructor_WithMessage_ShouldCreateWithCorrectProperties()
    {
        var message = "Resource conflict";
        var exception = new ResourceConflictException("Name", message);

        exception.StatusCode.Should().Be(409);
        exception.Title.Should().Be("Conflict");
        exception.Field.Should().Be("Name");
        exception.Message.Should().Be(message);
    }
}
