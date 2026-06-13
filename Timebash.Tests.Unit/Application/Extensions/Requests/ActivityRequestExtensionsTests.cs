using FluentAssertions;
using Timebash.Application.Extensions.Requests;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Extensions;

namespace Timebash.Tests.Unit.Application.Extensions.Requests;

public class ActivityRequestExtensionsTests
{
    [Fact]
    public void ToActivity_FromActivityRequest_ShouldMapAllPropertiesCorrectly()
    {
        var id = Guid.NewGuid();
        var request = new ActivityRequest(Guid.NewGuid(), string.Empty, DateTime.MinValue, DateTime.MaxValue, []);

        var result = request.ToActivity(id);
        AssertActivityFields(result, id, request.JournalId, request.Name, request.StartTime.TruncateToSecond(), request.EndTime.TruncateToSecond());
    }

    [Fact]
    public void ToActivity_FromActivityWithCorrectionRequest_ShouldMapAllPropertiesCorrectly()
    {
        var id = Guid.NewGuid();
        var request = new ActivityWithCorrectionRequest(Guid.NewGuid(), string.Empty, DateTime.MinValue, DateTime.MaxValue, [], [], []);

        var result = request.ToActivity(id);
        AssertActivityFields(result, id, request.JournalId, request.Name, request.StartTime.TruncateToSecond(), request.EndTime.TruncateToSecond());
    }

    private static void AssertActivityFields(Activity activity, Guid id, Guid journalId, string name, DateTime start, DateTime end)
    {
        activity.Id.Should().Be(id);
        activity.JournalId.Should().Be(journalId);
        activity.Name.Should().Be(name);
        activity.StartTime.Should().Be(start);
        activity.EndTime.Should().Be(end);
    }
}
