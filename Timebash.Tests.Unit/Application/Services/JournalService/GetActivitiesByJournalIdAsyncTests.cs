using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Services;
using Timebash.Tests.Unit.Application.Services.JournalService.TestData;

namespace Timebash.Tests.Unit.Application.Services.JournalService;

public class GetActivitiesByJournalIdAsyncTests : JournalServiceTestsBase
{
    [Fact]
    public async Task GetActivitiesByJournalId_WithoutDateRange_ShouldReturnAllActivities()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var activities = new List<Activity>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue),
            new(Guid.NewGuid(), Guid.NewGuid(), new DateTime(2026, 07, 10), DateTime.MaxValue),
            new(Guid.NewGuid(), Guid.NewGuid(), DateTime.MaxValue, DateTime.MaxValue),
        };
        var expected = new ActivitiesListResponse(
            [.. activities
                .OrderBy(activity => activity.StartTime)
                .Select(activity => activity.ToResponse())]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId)).ReturnsAsync(true);
        ActivityRepositoryMock.Setup(repository => repository.GetByJournalIdAsync(journal.Id)).ReturnsAsync(activities);

        var result = await Service.GetActivitiesByJournalIdAsync(journal.Id, null, null, journal.UserId);

        result.Should().BeEquivalentTo(expected);
        ActivityRepositoryMock.Verify(repository => repository.GetByJournalIdAsync(journal.Id), Times.Once);
    }

    [Theory]
    [ClassData(typeof(GetActivitiesWithOnlyStartDateRangeData))]
    public async Task GetActivitiesByJournalId_WithDate_WithOnlyStartDateRange_ShouldReturnCorrectActivities(
        Guid journalId, 
        DateTime start, 
        List<Activity> expectedActivities)
    {
        var journal = new Journal(journalId, Guid.NewGuid(), Faker.Lorem.Word());
        var expected = new ActivitiesListResponse(
            [.. expectedActivities
                .OrderBy(activity => activity.StartTime)
                .Select(activity => activity.ToResponse())]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId)).ReturnsAsync(true);
        ActivityQueryServiceMock.Setup(service => service.GetActivitiesForJournalAsync(journal.Id, start, null, ActivityDateFilterMode.ByStartTime))
            .Returns(expectedActivities.ToAsyncEnumerable());

        var result = await Service.GetActivitiesByJournalIdAsync(journal.Id, start, null, journal.UserId);
        
        result.Should().BeEquivalentTo(expected);
        ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(journal.Id, start, null, ActivityDateFilterMode.ByStartTime), 
            Times.Once);
    }

    [Theory]
    [ClassData(typeof(GetActivitiesWithOnlyEndDateRangeData))]
    public async Task GetActivitiesByJournalId_WithDate_WithOnlyEndDateRange_ShouldReturnCorrectActivities(
        Guid journalId, 
        DateTime end, 
        List<Activity> expectedActivities)
    {
        var journal = new Journal(journalId, Guid.NewGuid(), Faker.Lorem.Word());
        var expected = new ActivitiesListResponse(
            [.. expectedActivities
                .OrderBy(activity => activity.StartTime)
                .Select(activity => activity.ToResponse())]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId)).ReturnsAsync(true);
        ActivityQueryServiceMock.Setup(service => service.GetActivitiesForJournalAsync(journal.Id, null, end, ActivityDateFilterMode.ByStartTime))
            .Returns(expectedActivities.ToAsyncEnumerable());

        var result = await Service.GetActivitiesByJournalIdAsync(journal.Id, null, end, journal.UserId);
        
        result.Should().BeEquivalentTo(expected);
        ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(journal.Id, null, end, ActivityDateFilterMode.ByStartTime), 
            Times.Once);
    }

    [Theory]
    [ClassData(typeof(GetActivitiesWithDateRangeData))]
    public async Task GetActivitiesByJournalId_WithDate_WithDateRange_ShouldReturnCorrectActivities(
        Guid journalId, 
        DateTime start, 
        DateTime end, 
        List<Activity> expectedActivities)
    {
        var journal = new Journal(journalId, Guid.NewGuid(), Faker.Lorem.Word());
        var expected = new ActivitiesListResponse(
            [.. expectedActivities
                .OrderBy(activity => activity.StartTime)
                .Select(activity => activity.ToResponse())]);

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, journal.UserId)).ReturnsAsync(true);
        ActivityQueryServiceMock.Setup(service => service.GetActivitiesForJournalAsync(journal.Id, start, end, ActivityDateFilterMode.ByStartTime))
            .Returns(expectedActivities.ToAsyncEnumerable());

        var result = await Service.GetActivitiesByJournalIdAsync(journal.Id, start, end, journal.UserId);
        
        result.Should().BeEquivalentTo(expected);
        ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(journal.Id, start, end, ActivityDateFilterMode.ByStartTime), 
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
        var userId = Guid.NewGuid();
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId)).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.GetActivitiesByJournalIdAsync(id, null, null, userId))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
