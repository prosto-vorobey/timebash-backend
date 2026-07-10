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

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, userId)).ReturnsAsync(true);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForJournalAsync(journal.Id, null, null, ActivityDateFilterMode.Overlap))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, null, null, userId);
        result.Should().BeEquivalentTo(expected);

        JournalRepositoryMock.Verify(repository => repository.IsUserLinkedAsync(journal.Id, userId), Times.Once);
        ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(journal.Id, null, null, ActivityDateFilterMode.Overlap), 
            Times.Once);
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

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, userId)).ReturnsAsync(true);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForJournalAsync(journal.Id, startDate, null, ActivityDateFilterMode.Overlap))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, startDate, null, userId);
        result.Should().BeEquivalentTo(expected);

        JournalRepositoryMock.Verify(repository => repository.IsUserLinkedAsync(journal.Id, userId), Times.Once);
        ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(journal.Id, startDate, null, ActivityDateFilterMode.Overlap), 
            Times.Once);
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

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, userId)).ReturnsAsync(true);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForJournalAsync(journal.Id, null, endDate, ActivityDateFilterMode.Overlap))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, null, endDate, userId);
        result.Should().BeEquivalentTo(expected);

        JournalRepositoryMock.Verify(repository => repository.IsUserLinkedAsync(journal.Id, userId), Times.Once);
        ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(journal.Id, null, endDate, ActivityDateFilterMode.Overlap), 
            Times.Once);
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

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, userId)).ReturnsAsync(true);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForJournalAsync(journal.Id, startDate, endDate, ActivityDateFilterMode.Overlap))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetJournalAggregateStatisticAsync(journal.Id, startDate, endDate, userId);
        result.Should().BeEquivalentTo(expected);

        JournalRepositoryMock.Verify(repository => repository.IsUserLinkedAsync(journal.Id, userId), Times.Once);
        ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForJournalAsync(journal.Id, startDate, endDate, ActivityDateFilterMode.Overlap), 
            Times.Once);
    }

    [Fact]
    public async Task GetJournalAggregateStatistic_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetJournalAggregateStatisticAsync(Guid.Empty, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetJournalAggregateStatistic_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId)).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.GetJournalAggregateStatisticAsync(id, null, null, userId))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    private static Func<DateTime, long, Activity> CreateActivity(Guid journalId) => (start, duration) 
        => StatisticsTestDataFactory.CreateActivity(journalId, start, duration);
}
