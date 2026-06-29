using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

namespace Timebash.Tests.Unit.Application.Services.StatisticService;

public class GetJournalStatisticAsyncTests : StatisticServiceTestsBase
{
    [Theory]
    [ClassData(typeof(JournalStatisticData))]
    public async Task GetJournalStatistic_WithoutDateRange_ShouldReturnAllStatistic(
        Guid journalId,
        Guid userId,
        List<Activity> activities,
        long expectedTime,
        List<CategoryStatItem> expectedStats)
    {
        var journal = new Journal(journalId, userId, Faker.Lorem.Word());
        var expected = new JournalStatisticResponse(expectedTime, expectedStats);

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);
        ActivityQueryServiceMock.Setup(service => service.GetActivitiesForJournalAsync(journal.Id, null, null)).Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetJournalStatisticAsync(journal.Id, null, null, userId);
        result.Should().BeEquivalentTo(expected);

        JournalRepositoryMock.Verify(repository => repository.GetByIdAsync(journal.Id), Times.Once);
        ActivityQueryServiceMock.Verify(service => service.GetActivitiesForJournalAsync(journal.Id, null, null), Times.Once);
    }

    [Fact]
    public async Task GetJournalStatistic_WithStartDate_ShouldReturnCorrectStatistic()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var startDate = DateTime.MinValue.AddSeconds(DurationSecond);

        var (activities, expectedTime, expectedStats) = AggregationScenarioBuilder.GetDataWithStartDate(
            CreateActivity(journal.Id),
            userId,
            startDate,
            DurationSecond
        );
        var expected = new JournalStatisticResponse(expectedTime, expectedStats);

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);
        ActivityQueryServiceMock.Setup(service => service.GetActivitiesForJournalAsync(journal.Id, startDate, null)).Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetJournalStatisticAsync(journal.Id, startDate, null, userId);
        result.Should().BeEquivalentTo(expected);

        JournalRepositoryMock.Verify(repository => repository.GetByIdAsync(journal.Id), Times.Once);
        ActivityQueryServiceMock.Verify(service => service.GetActivitiesForJournalAsync(journal.Id, startDate, null), Times.Once);
    }

    [Fact]
    public async Task GetJournalStatistic_WithEndDate_ShouldReturnCorrectStatistic()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var endDate = DateTime.MaxValue.AddSeconds(-DurationSecond);

        var (activities, expectedTime, expectedStats) = AggregationScenarioBuilder.GetDataWithEndDate(
            CreateActivity(journal.Id),
            userId,
            endDate,
            DurationSecond
        );
        var expected = new JournalStatisticResponse(expectedTime, expectedStats);

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);
        ActivityQueryServiceMock.Setup(service => service.GetActivitiesForJournalAsync(journal.Id, null, endDate)).Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetJournalStatisticAsync(journal.Id, null, endDate, userId);
        result.Should().BeEquivalentTo(expected);

        JournalRepositoryMock.Verify(repository => repository.GetByIdAsync(journal.Id), Times.Once);
        ActivityQueryServiceMock.Verify(service => service.GetActivitiesForJournalAsync(journal.Id, null, endDate), Times.Once);
    }

    [Fact]
    public async Task GetJournalStatistic_WithStartAndEndDate_ShouldReturnCorrectStatistic()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var startDate = DateTime.MinValue.AddSeconds(DurationSecond);
        var endDate = DateTime.MaxValue.AddSeconds(-DurationSecond);

        var (activities, expectedTime, expectedStats) = AggregationScenarioBuilder.GetDataWithStartAndEndDate(
            CreateActivity(journal.Id),
            userId,
            startDate,
            endDate,
            DurationSecond
        );
        var expected = new JournalStatisticResponse(expectedTime, expectedStats);

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);
        ActivityQueryServiceMock.Setup(service => service.GetActivitiesForJournalAsync(journal.Id, startDate, endDate)).Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetJournalStatisticAsync(journal.Id, startDate, endDate, userId);
        result.Should().BeEquivalentTo(expected);

        JournalRepositoryMock.Verify(repository => repository.GetByIdAsync(journal.Id), Times.Once);
        ActivityQueryServiceMock.Verify(service => service.GetActivitiesForJournalAsync(journal.Id, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task GetJournalStatistic_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetJournalStatisticAsync(Guid.Empty, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetJournalStatistic_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => Service.GetJournalStatisticAsync(id, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    private static Func<DateTime, long, Activity> CreateActivity(Guid journalId) => (start, duration) => StatisticsTestDataFactory.CreateActivity(journalId, start, duration);
}
