using FluentAssertions;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Core.Exceptions;

public class ActivityTimeRangeExceptionTests
{
    [Fact]
    public void Constructor_ShouldCreateWithCorrectProperties()
    {
        var exception = new ActivityTimeRangeException();

        exception.StatusCode.Should().Be(400);
        exception.Title.Should().Be("Bad request");
    }

    [Fact]
    public void Constructor_WithMessage_ShouldCreateWithCorrectProperties()
    {
        var message = "Invalid activity time range";
        var exception = new ActivityTimeRangeException(message);

        exception.StatusCode.Should().Be(400);
        exception.Title.Should().Be("Bad request");
        exception.Message.Should().Be(message);
    }
}
