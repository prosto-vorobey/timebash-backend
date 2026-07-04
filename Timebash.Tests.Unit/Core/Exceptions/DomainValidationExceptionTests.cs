using FluentAssertions;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Core.Exceptions;

public class DomainValidationExceptionTests
{
    [Fact]
    public void Constructor_ShouldCreateWithCorrectProperties()
    {
        var exception = new DomainValidationException();

        exception.StatusCode.Should().Be(400);
        exception.Title.Should().Be("Bad request");
    }

    [Fact]
    public void Constructor_WithMessage_ShouldCreateWithCorrectProperties()
    {
        var message = "Domain validation failed";
        var exception = new DomainValidationException(message);

        exception.StatusCode.Should().Be(400);
        exception.Title.Should().Be("Bad request");
        exception.Message.Should().Be(message);
    }
}
