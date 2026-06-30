using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.JournalService;

public class GetActivitiesByJournalIdAsyncTests : JournalServiceTestsBase
{
    [Fact]
    public async Task GetActivitiesByJournalId_WithoutDate_ShouldReturnResponse()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var activities = new List<Activity>
        {new(Guid.NewGuid(), Guid.NewGuid(), DateTime.MaxValue, DateTime.MaxValue),
            new(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue),
            new(Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow, DateTime.MaxValue)
        };
        var expected = new ActivitiesListResponse(
            [.. activities
                .OrderBy(activity => activity.StartTime)
                .Select(activity => activity.ToResponse())]);

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);
        ActivityRepositoryMock.Setup(repository => repository.GetByJournalIdAsync(journal.Id)).ReturnsAsync(activities);

        var result = await Service.GetActivitiesByJournalIdAsync(journal.Id, null, null, journal.UserId);

        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetActivitiesByJournalId_WithDate_ShouldCallRepositoryWithCorrectRange()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var date = DateTime.UtcNow;
        var activities = new List<Activity>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), date, DateTime.MaxValue)
        };

        var expectedStartTime = date;
        var expectedEndTime = date.AddDays(1);
        var expected = new ActivitiesListResponse(
            [.. activities
                .OrderBy(activity => activity.StartTime)
                .Select(activity => activity.ToResponse())]);

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);
        ActivityRepositoryMock
            .Setup(repository => repository.GetByJournalIdAsync(journal.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(activities);

        var result = await Service.GetActivitiesByJournalIdAsync(journal.Id, date, null, journal.UserId);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(
            repository => repository.GetByJournalIdAsync(journal.Id, expectedStartTime, expectedEndTime),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(180)]
    [InlineData(-60)]
    public async Task GetActivitiesByJournalId_WithOffset_ShouldCallRepositoryWithAdjustedRange(int offsetMinutes)
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var date = DateTime.UtcNow;
        var activities = new List<Activity>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), date, DateTime.MaxValue)
        };

        var expectedStartTime = date.AddMinutes(offsetMinutes);
        var expectedEndTime = expectedStartTime.AddDays(1);
        var expected = new ActivitiesListResponse(
            [.. activities
                .OrderBy(activity => activity.StartTime)
                .Select(activity => activity.ToResponse())]);

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);
        ActivityRepositoryMock
            .Setup(repository => repository.GetByJournalIdAsync(journal.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
            .ReturnsAsync(activities);

        var result = await Service.GetActivitiesByJournalIdAsync(journal.Id, date, offsetMinutes, journal.UserId);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(
            repository => repository.GetByJournalIdAsync(journal.Id, expectedStartTime, expectedEndTime),
            Times.Once);
    }

    [Fact]
    public async Task GetActivitiesByJournalId_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetActivitiesByJournalIdAsync(Guid.Empty, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetActivitiesByJournalId_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => Service.GetActivitiesByJournalIdAsync(id, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
