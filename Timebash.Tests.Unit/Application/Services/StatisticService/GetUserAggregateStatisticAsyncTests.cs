using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

namespace Timebash.Tests.Unit.Application.Services.StatisticService;

public class GetUserAggregateStatisticAsyncTests : StatisticServiceTestsBase
{
    [Theory]
    [ClassData(typeof(UserAggregateStatisticData))]
    public async Task GetUserAggregateStatistic_WithoutDateRange_ShouldReturnAllStatistic(
        Guid userId,
        List<Activity> activities,
        long expectedTime,
        List<CategoryStatItem> expectedStats)
    {
        var user = new User(userId, Faker.Internet.UserName(), Faker.Internet.Email());
        var expected = new UserAggregateStatisticResponse(expectedTime, expectedStats);

        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForUserAsync(user.Id, null, null))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetUserAggregateStatisticAsync(user.Id, null, null);
        result.Should().BeEquivalentTo(expected);

        UserRepositoryMock.Verify(repository => repository.GetByIdAsync(user.Id), Times.Once);
        ActivityQueryServiceMock.Verify(service => service.GetActivitiesForUserAsync(user.Id, null, null), Times.Once);
    }

    [Fact]
    public async Task GetUserAggregateStatistic_WithStartDate_ShouldReturnCorrectStatistic()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());
        var startDate = DateTime.MinValue.AddSeconds(DurationSecond);

        var (activities, expectedTime, expectedStats) = AggregationStatisticScenarioBuilder.GetDataWithStartDate(
            StatisticsTestDataFactory.CreateActivity,
            user.Id,
            startDate,
            DurationSecond
        );
        var expected = new UserAggregateStatisticResponse(expectedTime, expectedStats);

        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForUserAsync(user.Id, startDate, null))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetUserAggregateStatisticAsync(user.Id, startDate, null);
        result.Should().BeEquivalentTo(expected);

        UserRepositoryMock.Verify(repository => repository.GetByIdAsync(user.Id), Times.Once);
        ActivityQueryServiceMock.Verify(service => service.GetActivitiesForUserAsync(user.Id, startDate, null), Times.Once);
    }

    [Fact]
    public async Task GetUserAggregateStatistic_WithEndDate_ShouldReturnCorrectStatistic()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());
        var endDate = DateTime.MaxValue.AddSeconds(-DurationSecond);

        var (activities, expectedTime, expectedStats) = AggregationStatisticScenarioBuilder.GetDataWithEndDate(
            StatisticsTestDataFactory.CreateActivity,
            user.Id,
            endDate,
            DurationSecond
        );
        var expected = new UserAggregateStatisticResponse(expectedTime, expectedStats);

        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForUserAsync(user.Id, null, endDate))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetUserAggregateStatisticAsync(user.Id, null, endDate);
        result.Should().BeEquivalentTo(expected);

        UserRepositoryMock.Verify(repository => repository.GetByIdAsync(user.Id), Times.Once);
        ActivityQueryServiceMock.Verify(service => service.GetActivitiesForUserAsync(user.Id, null, endDate), Times.Once);
    }

    [Fact]
    public async Task GetUserAggregateStatistic_WithStartAndEndDate_ShouldReturnCorrectStatistic()
    {
        var user = new User(Guid.NewGuid(), Faker.Internet.UserName(), Faker.Internet.Email());
        var startDate = DateTime.MinValue.AddSeconds(DurationSecond);
        var endDate = DateTime.MaxValue.AddSeconds(-DurationSecond);

        var (activities, expectedTime, expectedStats) = AggregationStatisticScenarioBuilder.GetDataWithStartAndEndDate(
            StatisticsTestDataFactory.CreateActivity,
            user.Id,
            startDate,
            endDate,
            DurationSecond
        );
        var expected = new UserAggregateStatisticResponse(expectedTime, expectedStats);

        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id)).ReturnsAsync(user);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForUserAsync(user.Id, startDate, endDate))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetUserAggregateStatisticAsync(user.Id, startDate, endDate);
        result.Should().BeEquivalentTo(expected);

        UserRepositoryMock.Verify(repository => repository.GetByIdAsync(user.Id), Times.Once);
        ActivityQueryServiceMock.Verify(service => service.GetActivitiesForUserAsync(user.Id, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task GetUserAggregateStatistic_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetUserAggregateStatisticAsync(Guid.Empty, null, null))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetUserAggregateStatistic_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((User?)null);

        await FluentActions
            .Awaiting(() => Service.GetUserAggregateStatisticAsync(id, null, null))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
