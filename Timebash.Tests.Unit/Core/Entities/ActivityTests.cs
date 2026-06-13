using Bogus;
using FluentAssertions;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Extensions;
using Timebash.Tests.Unit.Core.TestData;

namespace Timebash.Tests.Unit.Core.Entities;

public class ActivityTests
{
    private static readonly Faker _faker = new();

    [Theory]
    [ClassData(typeof(ValidTimeRangeData))]
    public void Constructor_WithValidData_ShouldCreateActivity(DateTime startTime, DateTime endTime)
    {
        var id = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var name = _faker.Lorem.Sentence();

        var timeBeforeCreate = DateTime.UtcNow;
        var result = new Activity(id, journalId, startTime, endTime, name);

        result.Id.Should().Be(id);
        result.JournalId.Should().Be(journalId);
        result.Name.Should().Be(name);
        result.StartTime.Should().Be(startTime.TruncateToSecond());
        result.EndTime.Should().Be(endTime.TruncateToSecond());
        result.CreatedAt.Should().BeOnOrAfter(timeBeforeCreate);
        result.CreatedAt.Should().Be(result.UpdatedAt);
    }

    [Fact]
    public void Constructor_EmptyId_ShouldThrowDomainValidationException()
        => FluentActions
            .Invoking(() => new Activity(Guid.Empty, Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue))
            .Should()
            .Throw<DomainValidationException>();

    [Fact]
    public void Constructor_EmptyJournalId_ShouldThrowDomainValidationException()
    => FluentActions
        .Invoking(() => new Activity(Guid.NewGuid(), Guid.Empty, DateTime.MinValue, DateTime.MaxValue))
        .Should()
        .Throw<DomainValidationException>();

    [Fact]
    public void Constructor_StartTimeAfterEnd_ShouldThrowActivityTimeRangeException()
    {
        var startTime = DateTime.MaxValue;
        var endTime = startTime.AddSeconds(-1);

        FluentActions
            .Invoking(() => new Activity(Guid.NewGuid(), Guid.NewGuid(), startTime, endTime))
            .Should()
            .Throw<ActivityTimeRangeException>();
    }

    [Fact]
    public void SetJournalId_WithValidJournalId_ShouldUpdateName()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var newJournalId = Guid.NewGuid();

        activity.JournalId = newJournalId;

        activity.JournalId.Should().Be(newJournalId);
    }

    [Fact]
    public void SetJournalId_EmptyJournalId_ShouldThrowDomainValidationException()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        FluentActions
            .Invoking(() => activity.JournalId = Guid.Empty)
            .Should()
            .Throw<DomainValidationException>();
    }

    [Fact]
    public void SetName_WithNull_ShouldUpdateNameWithEmpty()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue, _faker.Lorem.Sentence());
        activity.Name = null!;

        activity.Name.Should().BeEmpty();
    }

    [Fact]
    public void Duration_ShouldReturnDuration()
    {
        var id = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var startTime = DateTime.UtcNow;
        var endTime = startTime.AddMilliseconds(1);

        var activtiy = new Activity(id, journalId, startTime, endTime);

        activtiy.Duration.Should().Be(endTime.TruncateToSecond() - startTime.TruncateToSecond());
    }

    [Theory]
    [ClassData(typeof(ValidTimeRangeData))]
    public void UpdateTimeRange_WithValidTimeRange_ShouldReturnTrueAndUpdate(DateTime newStartTime, DateTime newEndTime)
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);

        var result = activity.UpdateTimeRange(newStartTime, newEndTime);

        result.Should().BeTrue();
        activity.StartTime.Should().Be(newStartTime.TruncateToSecond());
        activity.EndTime.Should().Be(newEndTime.TruncateToSecond());
    }

    [Theory]
    [ClassData(typeof(ValidTimeRangeNoChangesUpdateData))]
    public void UpdateTimeRange_NoChanges_ShouldReturnFalse(DateTime startTime, DateTime endTime, DateTime newStartTime, DateTime newEndTime)
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), startTime, endTime);

        var result = activity.UpdateTimeRange(newStartTime, newEndTime);

        result.Should().BeFalse();
    }

    [Fact]
    public void UpdateTimeRange_StartTimeAfterEnd_ShouldThrowActivityTimeRangeException()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var newStartTime = DateTime.MaxValue;
        var newEndTime = newStartTime.AddSeconds(-1);

        FluentActions
            .Invoking(() => activity.UpdateTimeRange(newStartTime, newEndTime))
            .Should()
            .Throw<ActivityTimeRangeException>();
    }
}
