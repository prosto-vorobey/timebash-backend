using FluentAssertions;
using Timebash.Core.Extensions;
using Timebash.Tests.Unit.Core.TestData;

namespace Timebash.Tests.Unit.Core.Extensions;

public class DateTimeExtensionsTests
{
    [Fact]
    public void TruncateToSecond_WhenAlreadyRounded_ShouldReturnSameValue()
    {
        var dateTime = new DateTime(2026, 5, 20, 10, 30, 45, 0, DateTimeKind.Utc);
        var expected = dateTime;

        dateTime.TruncateToSecond().Should().Be(expected);
    }

    [Theory]
    [ClassData(typeof(TruncateToSecondData))]
    public void TruncateToSecond_WithSubsecondPrecision_ShouldTruncateToSecond(DateTime dateTime, DateTime expected)
      => dateTime.TruncateToSecond().Should().Be(expected);

    [Fact]
    public void TruncateToSecond_WithDifferentKinds_ShouldPreserveKind()
    {
        var dateTime = new DateTime(2026, 5, 20, 10, 30, 45, 123, DateTimeKind.Local);
        var expected = new DateTime(2026, 5, 20, 10, 30, 45, 0, DateTimeKind.Local);

        dateTime.TruncateToSecond().Should().Be(expected);
    }
}
