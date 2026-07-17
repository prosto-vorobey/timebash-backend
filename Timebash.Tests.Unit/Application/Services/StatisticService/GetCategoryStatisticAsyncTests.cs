using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

namespace Timebash.Tests.Unit.Application.Services.StatisticService;

public class GetCategoryStatisticAsyncTests : StatisticServiceTestsBase
{
    [Theory]
    [ClassData(typeof(CategoryStatisticData))]
    public async Task GetCategoryStatistic_WithoutDateRange_ShouldReturnAllStatistic(
        Category category,
        List<Activity> activities,
        long expectedTime)
    {
        var expected = new CategoryStatisticResponse(expectedTime);

        SetupCategoryAccess(category.Id, category.UserId);
        SetupGetActivitiesForCategory(category.Id, null, null, activities);

        var result = await Service.GetCategoryStatisticAsync(category.Id, null, null, category.UserId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyCategoryAccessCalled(category.Id, category.UserId);
        VerifyGetActivitiesForCategoryCalled(category.Id, null, null);
    }

    [Fact]
    public async Task GetCategoryStatistic_WithStartDate_ShouldReturnCorrectStatistic()
    {
        var category = StatisticsTestDataFactory.CreateCategory();
        var startDate = DateTime.MinValue.AddSeconds(DurationSecond);

        var (activities, expectedTime) = CategoryStatisticScenarioBuilder.GetDataWithStartDate(category, startDate, DurationSecond);
        var expected = new CategoryStatisticResponse(expectedTime);

        SetupCategoryAccess(category.Id, category.UserId);
        SetupGetActivitiesForCategory(category.Id, startDate, null, activities);

        var result = await Service.GetCategoryStatisticAsync(category.Id, startDate, null, category.UserId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyCategoryAccessCalled(category.Id, category.UserId);
        VerifyGetActivitiesForCategoryCalled(category.Id, startDate, null);
    }

    [Fact]
    public async Task GetCategoryStatistic_WithEndDate_ShouldReturnCorrectStatistic()
    {
        var category = StatisticsTestDataFactory.CreateCategory();
        var endDate = DateTime.MaxValue.AddSeconds(-DurationSecond);

        var (activities, expectedTime) = CategoryStatisticScenarioBuilder.GetDataWithEndDate(category, endDate, DurationSecond);
        var expected = new CategoryStatisticResponse(expectedTime);

        SetupCategoryAccess(category.Id, category.UserId);
        SetupGetActivitiesForCategory(category.Id, null, endDate, activities);

        var result = await Service.GetCategoryStatisticAsync(category.Id, null, endDate, category.UserId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyCategoryAccessCalled(category.Id, category.UserId);
        VerifyGetActivitiesForCategoryCalled(category.Id, null, endDate);
    }

    [Fact]
    public async Task GetCategoryStatistic_WithStartAndEndDate_ShouldReturnCorrectStatistic()
    {
        var category = StatisticsTestDataFactory.CreateCategory();
        var startDate = DateTime.MinValue.AddSeconds(DurationSecond);
        var endDate = DateTime.MaxValue.AddSeconds(-DurationSecond);

        var (activities, expectedTime) = CategoryStatisticScenarioBuilder.GetDataWithStartAndEndDate(category, startDate, endDate, DurationSecond);
        var expected = new CategoryStatisticResponse(expectedTime);

        SetupCategoryAccess(category.Id, category.UserId);
        SetupGetActivitiesForCategory(category.Id, startDate, endDate, activities);

        var result = await Service.GetCategoryStatisticAsync(category.Id, startDate, endDate, category.UserId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        VerifyCategoryAccessCalled(category.Id, category.UserId);
        VerifyGetActivitiesForCategoryCalled(category.Id, startDate, endDate);
    }

    [Fact]
    public async Task GetCategoryStatistic_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        CategoryAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

        await FluentActions
            .Awaiting(() => Service.GetCategoryStatisticAsync(id, null, null, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyCategoryAccessCalled(id, userId);
        VerifyGetActivitiesForCategoryNotCalled();
    }

    [Fact]
    public async Task GetCategoryStatistic_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        CategoryAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        await FluentActions
            .Awaiting(() => Service.GetCategoryStatisticAsync(id, null, null, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyCategoryAccessCalled(id, userId);
        VerifyGetActivitiesForCategoryNotCalled();
    }

    private void SetupCategoryAccess(Guid id, Guid userId)
        => CategoryAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    private void SetupGetActivitiesForCategory(Guid id, DateTime? startDate, DateTime? endDate, List<Activity> activities)
        => ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForCategoryAsync(id, startDate, endDate))
            .Returns(activities.ToAsyncEnumerable());

    private void VerifyCategoryAccessCalled(Guid id, Guid userId)
        => CategoryAccessServiceMock.Verify(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyGetActivitiesForCategoryCalled(Guid id, DateTime? startDate, DateTime? endDate)
        => ActivityQueryServiceMock.Verify(service => service.GetActivitiesForCategoryAsync(id, startDate, endDate), Times.Once);

    private void VerifyGetActivitiesForCategoryNotCalled()
        => ActivityQueryServiceMock.Verify(
            service => service.GetActivitiesForCategoryAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()),
            Times.Never);
}
