using FluentAssertions;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Core.Exceptions;

public class UnauthorizedExceptionTests
{
    [Fact]
    public void Constructor_ShouldCreateWithCorrectProperties()
    {
        var exception = new UnauthorizedException();

        exception.StatusCode.Should().Be(401);
        exception.Title.Should().Be("Unauthorized");
    }
    
    [Fact]
    public void Constructor_WithMessage_ShouldCreateWithCorrectProperties()
    {
        var message = "Authentication required";
        var exception = new UnauthorizedException(message);

        exception.StatusCode.Should().Be(401);
        exception.Title.Should().Be("Unauthorized");
        exception.Message.Should().Be(message);
    }
}
