using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Services;
using Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

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

        SetupJournalAccess(journal.Id, userId);
        SetupGetActivitiesForJournal(journal.Id, null, null, activities);

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, null, null, userId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyJournalAccessCalled(journal.Id, userId);
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

        SetupJournalAccess(journal.Id, userId);
        SetupGetActivitiesForJournal(journal.Id, startDate, null, activities);

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, startDate, null, userId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyJournalAccessCalled(journal.Id, userId);
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

        SetupJournalAccess(journal.Id, userId);
        SetupGetActivitiesForJournal(journal.Id, null, endDate, activities);

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, null, endDate, userId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyJournalAccessCalled(journal.Id, userId);
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

        SetupJournalAccess(journal.Id, userId);
        SetupGetActivitiesForJournal(journal.Id, startDate, endDate, activities);

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, startDate, endDate, userId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyJournalAccessCalled(journal.Id, userId);
        VerifyGetActivitiesForJournalCalled(journal.Id, startDate, endDate);
    }

    [Fact]
    public async Task GetJournalAggregateStatistic_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();
        
        JournalAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

        await FluentActions
            .Awaiting(() => Service.GetJournalAggregateStatisticAsync(id, null, null, userId, It.IsAny<CancellationToken>()))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyJournalAccessCalled(id, userId);
        VerifyGetActivitiesForJournalNotCalled();
    }

    [Fact]
    public async Task GetJournalAggregateStatistic_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        
        JournalAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        await FluentActions
            .Awaiting(() => Service.GetJournalAggregateStatisticAsync(id, null, null, userId, It.IsAny<CancellationToken>()))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyJournalAccessCalled(id, userId);
        VerifyGetActivitiesForJournalNotCalled();
    }

    private static Func<DateTime, long, Activity> CreateActivity(Guid journalId) => (start, duration)
        => StatisticsTestDataFactory.CreateActivity(journalId, start, duration);

    private void SetupJournalAccess(Guid id, Guid userId)
        => JournalAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    private void SetupGetActivitiesForJournal(Guid id, DateTime? startDate, DateTime? endDate, List<Activity> activities)
        => ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForJournalAsync(id, startDate, endDate, ActivityDateFilterMode.Overlap))
            .Returns(activities.ToAsyncEnumerable());

    private void VerifyJournalAccessCalled(Guid id, Guid userId)
        => JournalAccessServiceMock.Verify(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyGetActivitiesForJournalCalled(Guid id, DateTime? startDate, DateTime? endDate)
        => ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(id, startDate, endDate, ActivityDateFilterMode.Overlap),
            Times.Once);

    private void VerifyGetActivitiesForJournalNotCalled()
        => ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime?>(),
                It.IsAny<DateTime?>(),
                It.IsAny<ActivityDateFilterMode>()),
            Times.Never);
}
