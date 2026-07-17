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

        SetupUserExists(user.Id);
        SetupGetActivitiesForUser(user.Id, null, null, activities);

        var result = await Service.GetUserAggregateStatisticAsync(user.Id, null, null, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyUserExistsCalled(user.Id);
        VerifyGetActivitiesForUserCalled(user.Id, null, null);
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

        SetupUserExists(user.Id);
        SetupGetActivitiesForUser(user.Id, startDate, null, activities);

        var result = await Service.GetUserAggregateStatisticAsync(user.Id, startDate, null, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyUserExistsCalled(user.Id);
        VerifyGetActivitiesForUserCalled(user.Id, startDate, null);
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

        SetupUserExists(user.Id);
        SetupGetActivitiesForUser(user.Id, null, endDate, activities);

        var result = await Service.GetUserAggregateStatisticAsync(user.Id, null, endDate, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyUserExistsCalled(user.Id);
        VerifyGetActivitiesForUserCalled(user.Id, null, endDate);
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

        SetupUserExists(user.Id);
        SetupGetActivitiesForUser(user.Id, startDate, endDate, activities);

        var result = await Service.GetUserAggregateStatisticAsync(user.Id, startDate, endDate, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyUserExistsCalled(user.Id);
        VerifyGetActivitiesForUserCalled(user.Id, startDate, endDate);
    }

    [Fact]
    public async Task GetUserAggregateStatistic_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        UserAccessServiceMock
            .Setup(service => service.ValidateExistsAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

        await FluentActions
            .Awaiting(() => Service.GetUserAggregateStatisticAsync(id, null, null, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyUserExistsCalled(id);
        VerifyGetActivitiesForUserNotCalled();
    }

    [Fact]
    public async Task GetUserAggregateStatistic_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserAccessServiceMock
            .Setup(service => service.ValidateExistsAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        await FluentActions
            .Awaiting(() => Service.GetUserAggregateStatisticAsync(id, null, null, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyUserExistsCalled(id);
        VerifyGetActivitiesForUserNotCalled();
    }

    private void SetupUserExists(Guid userId)
        => UserAccessServiceMock
            .Setup(service => service.ValidateExistsAsync(userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    private void SetupGetActivitiesForUser(Guid id, DateTime? startDate, DateTime? endDate, List<Activity> activities)
        => ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForUserAsync(id, startDate, endDate))
            .Returns(activities.ToAsyncEnumerable());

    private void VerifyUserExistsCalled(Guid userId)
        => UserAccessServiceMock.Verify(service => service.ValidateExistsAsync(userId, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyGetActivitiesForUserCalled(Guid id, DateTime? startDate, DateTime? endDate)
        => ActivityQueryServiceMock.Verify(service => service.GetActivitiesForUserAsync(id, startDate, endDate), Times.Once);

    private void VerifyGetActivitiesForUserNotCalled()
        => ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForUserAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()),
            Times.Never);
}
