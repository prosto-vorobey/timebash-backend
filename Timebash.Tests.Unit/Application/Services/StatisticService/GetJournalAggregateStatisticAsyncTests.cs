using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Services;
using Timebash.Tests.Unit.Application.Services.StatisticService.TestData;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

namespace Timebash.Tests.Unit.Application.Services.StatisticService;

public class GetJournalAggregateStatisticAsyncTests : StatisticServiceTestsBase
{
    [Theory]
    [ClassData(typeof(JournalAggregateStatisticData))]
    public async Task GetJournalAggregateStatistic_WithoutDateRange_ShouldReturnAllStatistic(
        Guid journalId,
        Guid userId,
        List<Activity> activities,
        long expectedTime,
        List<CategoryStatItem> expectedStats)
    {
        var journal = new Journal(journalId, userId, Faker.Lorem.Word());
        var expected = new JournalAggregateStatisticResponse(expectedTime, expectedStats);

        JournalAccessServiceMock.SetupValidateAccess(journal.Id, userId);
        SetupGetActivitiesForJournal(journal.Id, null, null, activities);

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, null, null, userId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journal.Id, userId);
        VerifyGetActivitiesForJournalCalled(journal.Id, null, null);
    }

    [Fact]
    public async Task GetJournalAggregateStatistic_WithStartDate_ShouldReturnCorrectStatistic()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var startDate = DateTime.MinValue.AddSeconds(DurationSecond);

        var (activities, expectedTime, expectedStats) = AggregationStatisticScenarioBuilder.GetDataWithStartDate(
            CreateActivity(journal.Id),
            userId,
            startDate,
            DurationSecond
        );
        var expected = new JournalAggregateStatisticResponse(expectedTime, expectedStats);

        JournalAccessServiceMock.SetupValidateAccess(journal.Id, userId);
        SetupGetActivitiesForJournal(journal.Id, startDate, null, activities);

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, startDate, null, userId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journal.Id, userId);
        VerifyGetActivitiesForJournalCalled(journal.Id, startDate, null);
    }

    [Fact]
    public async Task GetJournalAggregateStatistic_WithEndDate_ShouldReturnCorrectStatistic()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var endDate = DateTime.MaxValue.AddSeconds(-DurationSecond);

        var (activities, expectedTime, expectedStats) = AggregationStatisticScenarioBuilder.GetDataWithEndDate(
            CreateActivity(journal.Id),
            userId,
            endDate,
            DurationSecond
        );
        var expected = new JournalAggregateStatisticResponse(expectedTime, expectedStats);

        JournalAccessServiceMock.SetupValidateAccess(journal.Id, userId);
        SetupGetActivitiesForJournal(journal.Id, null, endDate, activities);

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, null, endDate, userId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journal.Id, userId);
        VerifyGetActivitiesForJournalCalled(journal.Id, null, endDate);
    }

    [Fact]
    public async Task GetJournalAggregateStatistic_WithStartAndEndDate_ShouldReturnCorrectStatistic()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var startDate = DateTime.MinValue.AddSeconds(DurationSecond);
        var endDate = DateTime.MaxValue.AddSeconds(-DurationSecond);

        var (activities, expectedTime, expectedStats) = AggregationStatisticScenarioBuilder.GetDataWithStartAndEndDate(
            CreateActivity(journal.Id),
            userId,
            startDate,
            endDate,
            DurationSecond
        );
        var expected = new JournalAggregateStatisticResponse(expectedTime, expectedStats);

        JournalAccessServiceMock.SetupValidateAccess(journal.Id, userId);
        SetupGetActivitiesForJournal(journal.Id, startDate, endDate, activities);

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, startDate, endDate, userId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journal.Id, userId);
        VerifyGetActivitiesForJournalCalled(journal.Id, startDate, endDate);
    }

    [Fact]
    public async Task GetJournalAggregateStatistic_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        JournalAccessServiceMock.SetupValidateAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetJournalAggregateStatisticAsync(id, null, null, userId, It.IsAny<CancellationToken>()))
            .Should()
            .ThrowAsync<BadRequestException>();

        JournalAccessServiceMock.VerifyValidateAccessCalled(id, userId);
    }

    [Fact]
    public async Task GetJournalAggregateStatistic_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        JournalAccessServiceMock.SetupValidateAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetJournalAggregateStatisticAsync(id, null, null, userId, It.IsAny<CancellationToken>()))
            .Should()
            .ThrowAsync<NotFoundException>();

        JournalAccessServiceMock.VerifyValidateAccessCalled(id, userId);
    }

    private static Func<DateTime, long, Activity> CreateActivity(Guid journalId) => (start, duration)
        => StatisticsTestDataFactory.CreateActivity(journalId, start, duration);

    private void SetupGetActivitiesForJournal(Guid id, DateTime? startDate, DateTime? endDate, List<Activity> activities)
        => ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForJournalAsync(id, startDate, endDate, ActivityDateFilterMode.Overlap))
            .Returns(activities.ToAsyncEnumerable());

    private void VerifyGetActivitiesForJournalCalled(Guid id, DateTime? startDate, DateTime? endDate)
        => ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(id, startDate, endDate, ActivityDateFilterMode.Overlap),
            Times.Once);
}
