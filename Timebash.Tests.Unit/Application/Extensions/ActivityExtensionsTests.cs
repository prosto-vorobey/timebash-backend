using Bogus;
using FluentAssertions;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Extensions;

namespace Timebash.Tests.Unit.Application.Extensions;

public class ActivityExtensionsTests
{
    private static readonly Faker _faker = new();

    [Fact]
    public void ToResponse_ShouldMapAllPropertiesCorrectly()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);

        var result = activity.ToResponse();
        AssertActivityResponseFields(
            result,
            activity.Id,
            activity.JournalId,
            activity.Name,
            activity.StartTime,
            activity.EndTime,
            activity.CreatedAt,
            activity.UpdatedAt,
            activity.Duration);
    }

    [Fact]
    public void ApplyUpdate_WhenJournalIdChanged_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var name = string.Empty;
        var startTime = DateTime.MinValue;
        var endTime = DateTime.MaxValue;
        var activity = new Activity(id, Guid.NewGuid(), startTime, endTime);
        var currentCreatedTime = activity.CreatedAt;
        var currentUpdatedTime = activity.UpdatedAt;
        var newJournalId = Guid.NewGuid();
        var request = new ActivityRequest(newJournalId, name, startTime, endTime, []);

        activity.ApplyUpdate(request).Should().BeTrue();
        AssertActivityFieldsAfterApplyUpdate(activity, id, newJournalId, name, startTime, endTime, currentCreatedTime, currentUpdatedTime);
    }

    [Fact]
    public void ApplyUpdate_WhenNameChanged_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var startTime = DateTime.MinValue;
        var endTime = DateTime.MaxValue;
        var activity = new Activity(id, journalId, startTime, endTime);
        var currentCreatedTime = activity.CreatedAt;
        var currentUpdatedTime = activity.UpdatedAt;
        var newName = _faker.Lorem.Sentence();
        var request = new ActivityRequest(journalId, newName, startTime, endTime, []);

        activity.ApplyUpdate(request).Should().BeTrue();
        AssertActivityFieldsAfterApplyUpdate(activity, id, journalId, newName, startTime, endTime, currentCreatedTime, currentUpdatedTime);
    }

    [Fact]
    public void ApplyUpdate_WhenStartTimeChanged_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var name = string.Empty;
        var endTime = DateTime.MaxValue;
        var activity = new Activity(id, journalId, DateTime.MinValue, endTime, name);
        var currentCreatedTime = activity.CreatedAt;
        var currentUpdatedTime = activity.UpdatedAt;
        var newStartTime = activity.StartTime.AddDays(1);
        var request = new ActivityRequest(journalId, name, newStartTime, endTime, []);

        activity.ApplyUpdate(request).Should().BeTrue();
        AssertActivityFieldsAfterApplyUpdate(activity, id, journalId, name, newStartTime, endTime, currentCreatedTime, currentUpdatedTime);
    }

    [Fact]
    public void ApplyUpdate_WhenEndTimeChanged_ShouldReturnTrueAndUpdate()
    {
        var id = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var name = string.Empty;
        var startTime = DateTime.MinValue;
        var activity = new Activity(id, journalId, startTime, DateTime.MaxValue, name);
        var currentCreatedTime = activity.CreatedAt;
        var currentUpdatedTime = activity.UpdatedAt;
        var newEndTime = activity.EndTime.AddDays(-1);
        var request = new ActivityRequest(journalId, name, startTime, newEndTime, []);

        activity.ApplyUpdate(request).Should().BeTrue();
        AssertActivityFieldsAfterApplyUpdate(activity, id, journalId, name, startTime, newEndTime, currentCreatedTime, currentUpdatedTime);
    }

    [Fact]
    public void ApplyUpdate_NoChanges_ShouldReturnFalse()
    {
        var id = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var name = string.Empty;
        var startTime = DateTime.MinValue;
        var endTime = DateTime.MaxValue;
        var activity = new Activity(id, journalId, startTime, endTime, name);
        var currentCreatedTime = activity.CreatedAt;
        var currentUpdatedTime = activity.UpdatedAt;
        var request = new ActivityRequest(journalId, name, startTime, endTime, []);

        activity.ApplyUpdate(request).Should().BeFalse();
        AssertActivityFieldsAfterApplyUpdate(activity, id, journalId, name, startTime, endTime, currentCreatedTime, currentUpdatedTime);
    }

    private static void AssertActivityResponseFields(
        ActivityResponse response,
        Guid id,
        Guid journalId,
        string name,
        DateTime start,
        DateTime end,
        DateTime createdAt,
        DateTime updatedAt,
        TimeSpan duration)
    {
        response.Id.Should().Be(id);
        response.JournalId.Should().Be(journalId);
        response.Name.Should().Be(name);
        response.StartTime.Should().Be(start);
        response.EndTime.Should().Be(end);
        response.CreatedAt.Should().Be(createdAt);
        response.UpdatedAt.Should().Be(updatedAt);
        response.Duration.Should().Be(duration);
    }

    private static void AssertActivityFieldsAfterApplyUpdate(
        Activity activity,
        Guid id,
        Guid journalId,
        string name,
        DateTime start,
        DateTime end,
        DateTime createdAt,
        DateTime updatedAt)
    {
        activity.Id.Should().Be(id);
        activity.JournalId.Should().Be(journalId);
        activity.Name.Should().Be(name);
        activity.StartTime.Should().Be(start.TruncateToSecond());
        activity.EndTime.Should().Be(end.TruncateToSecond());
        activity.CreatedAt.Should().Be(createdAt);
        activity.UpdatedAt.Should().Be(updatedAt);
    }
}
