using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.DTOs.Shared;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Extensions;
using Timebash.Tests.Unit.Application.Services.JournalService.TestData;

namespace Timebash.Tests.Unit.Application.Services.JournalService;

public class GetTimeCorrectionConflictsAsyncTests : JournalServiceTestsBase
{
    [Fact]
    public async Task GetTimeCorrectionConflicts_WhenOverlapFullyInside_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.MinValue;
        var endTime = DateTime.MaxValue;
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();
        var overlap = new Activity(Guid.NewGuid(), journal.Id, startTime.AddDays(1), endTime.AddDays(-1));

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    overlap.Id,
                    string.Empty,
                    overlap.StartTime,
                    overlap.EndTime,
                    new DeleteCorrection(),
                    new SplitCorrection(
                        [
                            new(truncatedStart, overlap.StartTime),
                            new(overlap.EndTime, truncatedEnd)
                        ]))
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([overlap]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_WhenOverlapLeftAligned_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.MinValue;
        var endTime = DateTime.MaxValue;
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();
        var overlap = new Activity(Guid.NewGuid(), journal.Id, startTime, endTime.AddDays(-1));

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    overlap.Id,
                    string.Empty,
                    overlap.StartTime,
                    overlap.EndTime,
                    new ShiftCorrection(truncatedStart, startTime),
                    new ShiftCorrection(overlap.EndTime, truncatedEnd))
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([overlap]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_WhenOverlapRightAligned_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.MinValue;
        var endTime = DateTime.MaxValue;
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();
        var overlap = new Activity(Guid.NewGuid(), journal.Id, startTime.AddDays(1), endTime);

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    overlap.Id,
                    string.Empty,
                    overlap.StartTime,
                    overlap.EndTime,
                    new ShiftCorrection(truncatedEnd, truncatedEnd),
                    new ShiftCorrection(truncatedStart, overlap.StartTime))
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([overlap]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_WhenOverlapIdentical_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.MinValue;
        var endTime = DateTime.MaxValue;
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();
        var overlap = new Activity(Guid.NewGuid(), journal.Id, startTime, endTime);

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    overlap.Id,
                    string.Empty,
                    overlap.StartTime,
                    overlap.EndTime,
                    new ShiftCorrection(truncatedStart, truncatedStart),
                    new ShiftCorrection(truncatedEnd, truncatedEnd))
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([overlap]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_WhenOverlapEnclosing_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var overlap = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        var startTime = overlap.StartTime.AddDays(1);
        var endTime = overlap.EndTime.AddDays(-1);

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    overlap.Id,
                    string.Empty,
                    overlap.StartTime,
                    overlap.EndTime,
                    new SplitCorrection(
                        [
                            new(overlap.StartTime, startTime),
                            new(endTime, overlap.EndTime)
                        ]),
                    new DeleteCorrection())
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, startTime, endTime, It.IsAny<CancellationToken>()))
            .ReturnsAsync([overlap]);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, startTime, It.IsAny<CancellationToken>()), Times.Never);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, endTime, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [ClassData(typeof(OverlapAtStartData))]
    public async Task GetTimeCorrectionConflicts_WhenOverlapAtStart_ShouldReturnResponse(DateTime endTime, DateTime overlapEndTime)
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var overlap = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, overlapEndTime);
        var startTime = overlap.StartTime.AddDays(1);
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    overlap.Id,
                    string.Empty,
                    overlap.StartTime,
                    overlap.EndTime,
                    new ShiftCorrection(overlap.StartTime, truncatedStart),
                    new ShiftCorrection(overlap.EndTime, truncatedEnd))
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([overlap]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [ClassData(typeof(OverlapAtEndData))]
    public async Task GetTimeCorrectionConflicts_WhenOverlapAtEnd_ShouldReturnResponse(DateTime startTime, DateTime overlapStartTime)
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var overlap = new Activity(Guid.NewGuid(), journal.Id, overlapStartTime, DateTime.MaxValue);
        var endTime = overlap.EndTime.AddDays(-1);
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    overlap.Id,
                    string.Empty,
                    overlap.StartTime,
                    overlap.EndTime,
                    new ShiftCorrection(truncatedEnd, overlap.EndTime),
                    new ShiftCorrection(truncatedStart, overlap.StartTime))
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([overlap]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_WithMultiOverlaps_WithoutPreviousAndNextActivities_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.MaxValue;
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();
        var overlaps = new List<Activity>
        {
            new(Guid.NewGuid(), journal.Id, DateTime.MinValue, startTime.AddDays(1)),
            new(Guid.NewGuid(), journal.Id, startTime.AddDays(2), startTime.AddDays(3)),
            new(Guid.NewGuid(), journal.Id, startTime.AddDays(2), endTime.AddDays(-1)),
            new(Guid.NewGuid(), journal.Id, endTime.AddDays(-1), endTime)
        };

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    overlaps[0].Id,
                    string.Empty,
                    overlaps[0].StartTime,
                    overlaps[0].EndTime,
                    new ShiftCorrection(overlaps[0].StartTime, truncatedStart),
                    new ShiftCorrection(overlaps[0].EndTime, truncatedEnd)),
                new(
                    overlaps[1].Id,
                    string.Empty,
                    overlaps[1].StartTime,
                    overlaps[1].EndTime,
                    new DeleteCorrection(),
                    new SplitCorrection(
                        [
                            new(truncatedStart, overlaps[1].StartTime),
                            new(overlaps[1].EndTime, truncatedEnd)
                        ])),
                new(
                    overlaps[2].Id,
                    string.Empty,
                    overlaps[2].StartTime,
                    overlaps[2].EndTime,
                    new DeleteCorrection(),
                    new SplitCorrection(
                        [
                            new(truncatedStart, overlaps[2].StartTime),
                            new(overlaps[2].EndTime, truncatedEnd)
                        ])),
                new(
                    overlaps[3].Id,
                    string.Empty,
                    overlaps[3].StartTime,
                    overlaps[3].EndTime,
                    new ShiftCorrection(overlaps[3].EndTime, truncatedEnd),
                    new ShiftCorrection(truncatedStart, overlaps[3].StartTime)),
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(overlaps);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_WithMultiOverlaps_WithPreviousAndNextActivities_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.MaxValue.AddDays(-1);
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();
        var overlaps = new List<Activity>
        {
            new(Guid.NewGuid(), journal.Id, startTime, startTime.AddDays(1)),
            new(Guid.NewGuid(), journal.Id, startTime.AddDays(2), startTime.AddDays(3)),
            new(Guid.NewGuid(), journal.Id, startTime.AddDays(2), endTime.AddDays(-1)),
            new(Guid.NewGuid(), journal.Id, endTime.AddDays(-1), endTime)
        };
        var previous = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MinValue.AddDays(1));
        var next = new Activity(Guid.NewGuid(), journal.Id, DateTime.MaxValue, DateTime.MaxValue);

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    overlaps[0].Id,
                    string.Empty,
                    overlaps[0].StartTime,
                    overlaps[0].EndTime,
                    new ShiftCorrection(overlaps[0].StartTime, truncatedStart),
                    new ShiftCorrection(overlaps[0].EndTime, truncatedEnd)),
                new(
                    overlaps[1].Id,
                    string.Empty,
                    overlaps[1].StartTime,
                    overlaps[1].EndTime,
                    new DeleteCorrection(),
                    new SplitCorrection(
                        [
                            new(truncatedStart, overlaps[1].StartTime),
                            new(overlaps[1].EndTime, truncatedEnd)
                        ])),
                new(
                    overlaps[2].Id,
                    string.Empty,
                    overlaps[2].StartTime,
                    overlaps[2].EndTime,
                    new DeleteCorrection(),
                    new SplitCorrection(
                        [
                            new(truncatedStart, overlaps[2].StartTime),
                            new(overlaps[2].EndTime, truncatedEnd)
                        ])),
                new(
                    overlaps[3].Id,
                    string.Empty,
                    overlaps[3].StartTime,
                    overlaps[3].EndTime,
                    new ShiftCorrection(overlaps[3].EndTime, truncatedEnd),
                    new ShiftCorrection(truncatedStart, overlaps[3].StartTime)),
                new(
                    previous.Id,
                    string.Empty,
                    previous.StartTime,
                    previous.EndTime,
                    new ShiftCorrection(previous.StartTime, truncatedStart),
                    new ShiftCorrection(previous.EndTime, truncatedEnd)),
                new(
                    next.Id,
                    string.Empty,
                    next.StartTime,
                    next.EndTime,
                    new ShiftCorrection(truncatedEnd, next.EndTime),
                    new ShiftCorrection(truncatedStart, next.StartTime))
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(overlaps);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previous);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(next);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_WithPreviousActivity_ShouldAddPreviousCorrection()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow;
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();
        var previous = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MinValue.AddDays(1), Faker.Lorem.Sentence());

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    previous.Id,
                    previous.Name,
                    previous.StartTime,
                    previous.EndTime,
                    new ShiftCorrection(previous.StartTime, truncatedStart),
                    new ShiftCorrection(previous.EndTime, truncatedEnd)),
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previous);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, truncatedStart, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_WhenPreviousEndTimeEqualsStart_ShouldNotAddCorrection()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow;
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();
        var previous = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, startTime, Faker.Lorem.Sentence());

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previous);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Corrections.Should().BeEmpty();
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_WithNextActivity_ShouldAddNextCorrection()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow;
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();
        var next = new Activity(Guid.NewGuid(), journal.Id, DateTime.MaxValue.AddDays(-1), DateTime.MaxValue, Faker.Lorem.Sentence());

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    next.Id,
                    next.Name,
                    next.StartTime,
                    next.EndTime,
                    new ShiftCorrection(truncatedEnd, next.EndTime),
                    new ShiftCorrection(truncatedStart, next.StartTime))
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(next);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_WhenNextStartTimeEqualsEnd_ShouldNotAddCorrection()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow;
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();
        var next = new Activity(Guid.NewGuid(), journal.Id, endTime, DateTime.MaxValue, Faker.Lorem.Sentence());

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(next);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Corrections.Should().BeEmpty();
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_WhenBetweenActivities_NoOverlaps_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.UtcNow;
        var endTime = DateTime.UtcNow;
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();
        var previous = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MinValue.AddDays(1), Faker.Lorem.Sentence());
        var next = new Activity(Guid.NewGuid(), journal.Id, DateTime.MaxValue.AddDays(-1), DateTime.MaxValue, Faker.Lorem.Sentence());

        var expected = new ConflictCorrectionsListResponse(
            [
                new(
                    previous.Id,
                    previous.Name,
                    previous.StartTime,
                    previous.EndTime,
                    new ShiftCorrection(previous.StartTime, truncatedStart),
                    new ShiftCorrection(previous.EndTime, truncatedEnd)),
                new(
                    next.Id,
                    next.Name,
                    next.StartTime,
                    next.EndTime,
                    new ShiftCorrection(truncatedEnd, next.EndTime),
                    new ShiftCorrection(truncatedStart, next.StartTime))
            ]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync(previous);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync(next);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_NoActivities_ShouldNotAddCorrection()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var startTime = DateTime.MinValue;
        var endTime = DateTime.MaxValue;
        var truncatedStart = startTime.TruncateToSecond();
        var truncatedEnd = endTime.TruncateToSecond();

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        ActivityRepositoryMock
            .Setup(repository => repository.GetOverlappingActivitiesAsync(journal.Id, truncatedStart, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        ActivityRepositoryMock
            .Setup(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);
        ActivityRepositoryMock
            .Setup(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Activity?)null);

        var result = await Service.GetTimeCorrectionConflictsAsync(journal.Id, startTime, endTime, journal.UserId, CancellationToken.None);

        result.Corrections.Should().BeEmpty();
        ActivityRepositoryMock.Verify(repository => repository.GetPreviousActivityAsync(journal.Id, truncatedStart, It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.GetNextActivityAsync(journal.Id, truncatedEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTimeCorrectionConflicts_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetTimeCorrectionConflictsAsync(Guid.Empty, DateTime.MinValue, DateTime.MaxValue, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetTimeCorrectionConflicts_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.GetTimeCorrectionConflictsAsync(id, DateTime.MinValue, DateTime.MaxValue, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
