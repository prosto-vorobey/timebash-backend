using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Application.Services.StatisticService.TestData;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

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

        CategoryAccessServiceMock.SetupValidateAccess(category.Id, category.UserId);
        SetupGetActivitiesForCategory(category.Id, null, null, activities);

        var result = await Service.GetCategoryStatisticAsync(category.Id, null, null, category.UserId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        CategoryAccessServiceMock.VerifyValidateAccessCalled(category.Id, category.UserId);
        VerifyGetActivitiesForCategoryCalled(category.Id, null, null);
    }

    [Fact]
    public async Task GetCategoryStatistic_WithStartDate_ShouldReturnCorrectStatistic()
    {
        var category = StatisticsTestDataFactory.CreateCategory();
        var startDate = DateTime.MinValue.AddSeconds(DurationSecond);

        var (activities, expectedTime) = CategoryStatisticScenarioBuilder.GetDataWithStartDate(category, startDate, DurationSecond);
        var expected = new CategoryStatisticResponse(expectedTime);

        CategoryAccessServiceMock.SetupValidateAccess(category.Id, category.UserId);
        SetupGetActivitiesForCategory(category.Id, startDate, null, activities);

        var result = await Service.GetCategoryStatisticAsync(category.Id, startDate, null, category.UserId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        CategoryAccessServiceMock.VerifyValidateAccessCalled(category.Id, category.UserId);
        VerifyGetActivitiesForCategoryCalled(category.Id, startDate, null);
    }

    [Fact]
    public async Task GetCategoryStatistic_WithEndDate_ShouldReturnCorrectStatistic()
    {
        var category = StatisticsTestDataFactory.CreateCategory();
        var endDate = DateTime.MaxValue.AddSeconds(-DurationSecond);

        var (activities, expectedTime) = CategoryStatisticScenarioBuilder.GetDataWithEndDate(category, endDate, DurationSecond);
        var expected = new CategoryStatisticResponse(expectedTime);

        CategoryAccessServiceMock.SetupValidateAccess(category.Id, category.UserId);
        SetupGetActivitiesForCategory(category.Id, null, endDate, activities);

        var result = await Service.GetCategoryStatisticAsync(category.Id, null, endDate, category.UserId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        CategoryAccessServiceMock.VerifyValidateAccessCalled(category.Id, category.UserId);
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

        CategoryAccessServiceMock.SetupValidateAccess(category.Id, category.UserId);
        SetupGetActivitiesForCategory(category.Id, startDate, endDate, activities);

        var result = await Service.GetCategoryStatisticAsync(category.Id, startDate, endDate, category.UserId, CancellationToken.None);
        
        result.Should().BeEquivalentTo(expected);
        CategoryAccessServiceMock.VerifyValidateAccessCalled(category.Id, category.UserId);
        VerifyGetActivitiesForCategoryCalled(category.Id, startDate, endDate);
    }

    [Fact]
    public async Task GetCategoryStatistic_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        CategoryAccessServiceMock.SetupValidateAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetCategoryStatisticAsync(id, null, null, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        CategoryAccessServiceMock.VerifyValidateAccessCalled(id, userId);
    }

    [Fact]
    public async Task GetCategoryStatistic_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        CategoryAccessServiceMock.SetupValidateAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.GetCategoryStatisticAsync(id, null, null, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        CategoryAccessServiceMock.VerifyValidateAccessCalled(id, userId);
    }

    private void SetupGetActivitiesForCategory(Guid id, DateTime? startDate, DateTime? endDate, List<Activity> activities)
        => ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForCategoryAsync(id, startDate, endDate))
            .Returns(activities.ToAsyncEnumerable());

    private void VerifyGetActivitiesForCategoryCalled(Guid id, DateTime? startDate, DateTime? endDate)
        => ActivityQueryServiceMock.Verify(service => service.GetActivitiesForCategoryAsync(id, startDate, endDate), Times.Once);
}
