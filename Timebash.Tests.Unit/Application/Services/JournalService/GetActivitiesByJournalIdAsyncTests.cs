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

        SetupValidateAccess(journal.Id, journal.UserId);
        ActivityRepositoryMock.Setup(repository => repository.GetByJournalIdAsync(journal.Id, It.IsAny<CancellationToken>())).ReturnsAsync(activities);

        var result = await Service.GetActivitiesByJournalIdAsync(journal.Id, null, null, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        VerifyValidateAccessCalled(journal.Id, journal.UserId);
        ActivityRepositoryMock.Verify(repository => repository.GetByJournalIdAsync(journal.Id, It.IsAny<CancellationToken>()), Times.Once);
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

        SetupValidateAccess(journal.Id, journal.UserId);
        ActivityQueryServiceMock.Setup(service => service.GetActivitiesForJournalAsync(journal.Id, start, null, ActivityDateFilterMode.ByStartTime))
            .Returns(expectedActivities.ToAsyncEnumerable());

        var result = await Service.GetActivitiesByJournalIdAsync(journal.Id, start, null, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        VerifyValidateAccessCalled(journal.Id, journal.UserId);
        VerifyGetActivitiesForJournalCalled(journal.Id, start, null);
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

        SetupValidateAccess(journal.Id, journal.UserId);
        ActivityQueryServiceMock.Setup(service => service.GetActivitiesForJournalAsync(journal.Id, null, end, ActivityDateFilterMode.ByStartTime))
            .Returns(expectedActivities.ToAsyncEnumerable());

        var result = await Service.GetActivitiesByJournalIdAsync(journal.Id, null, end, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        VerifyValidateAccessCalled(journal.Id, journal.UserId);
        VerifyGetActivitiesForJournalCalled(journal.Id, null, end);
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

        SetupValidateAccess(journal.Id, journal.UserId);
        ActivityQueryServiceMock.Setup(service => service.GetActivitiesForJournalAsync(journal.Id, start, end, ActivityDateFilterMode.ByStartTime))
            .Returns(expectedActivities.ToAsyncEnumerable());

        var result = await Service.GetActivitiesByJournalIdAsync(journal.Id, start, end, journal.UserId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        VerifyValidateAccessCalled(journal.Id, journal.UserId);
        VerifyGetActivitiesForJournalCalled(journal.Id, start, end);
    }

    [Fact]
    public async Task GetActivitiesByJournalId_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        SetupValidateAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetActivitiesByJournalIdAsync(id, null, null, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyValidateAccessCalled(id, userId);
        VerifyGetActivitiesByJournalIdNotCalled();
        VerifyGetActivitiesForJournalNotCalled();
    }

    [Fact]
    public async Task GetActivitiesByJournalId_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupValidateAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetActivitiesByJournalIdAsync(id, null, null, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyValidateAccessCalled(id, userId);
        VerifyGetActivitiesByJournalIdNotCalled();
        VerifyGetActivitiesForJournalNotCalled();
    }

    private void VerifyGetActivitiesForJournalCalled(Guid id, DateTime? start, DateTime? end)
        => ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(id, start, end, It.IsAny<ActivityDateFilterMode>()),
            Times.Once);

    private void VerifyGetActivitiesByJournalIdNotCalled()
        => ActivityRepositoryMock.Verify(repository => repository.GetByJournalIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

    private void VerifyGetActivitiesForJournalNotCalled() 
        => ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<ActivityDateFilterMode>()),
            Times.Never);
}
