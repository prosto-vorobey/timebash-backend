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

        CategoryRepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id)).ReturnsAsync(category);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForCategoryAsync(category.Id, null, null))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetCategoryStatisticAsync(category.Id, null, null, category.UserId);
        result.Should().BeEquivalentTo(expected);

        CategoryRepositoryMock.Verify(repository => repository.GetByIdAsync(category.Id), Times.Once);
        ActivityQueryServiceMock.Verify(Service => Service.GetActivitiesForCategoryAsync(category.Id, null, null), Times.Once);
    }

    [Fact]
    public async Task GetCategoryStatistic_WithStartDate_ShouldReturnCorrectStatistic()
    {
        var category = StatisticsTestDataFactory.CreateCategory();
        var startDate = DateTime.MinValue.AddSeconds(DurationSecond);

        var (activities, expectedTime) = CategoryStatisticScenarioBuilder.GetDataWithStartDate(category, startDate, DurationSecond);
        var expected = new CategoryStatisticResponse(expectedTime);
        
        CategoryRepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id)).ReturnsAsync(category);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForCategoryAsync(category.Id, startDate, null))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetCategoryStatisticAsync(category.Id, startDate, null, category.UserId);
        result.Should().BeEquivalentTo(expected);

        CategoryRepositoryMock.Verify(repository => repository.GetByIdAsync(category.Id), Times.Once);
        ActivityQueryServiceMock.Verify(Service => Service.GetActivitiesForCategoryAsync(category.Id, startDate, null), Times.Once);
    }

    [Fact]
    public async Task GetCategoryStatistic_WithEndDate_ShouldReturnCorrectStatistic()
    {
        var category = StatisticsTestDataFactory.CreateCategory();
        var endDate = DateTime.MaxValue.AddSeconds(-DurationSecond);

        var (activities, expectedTime) = CategoryStatisticScenarioBuilder.GetDataWithEndDate(category, endDate, DurationSecond);
        var expected = new CategoryStatisticResponse(expectedTime);
        
        CategoryRepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id)).ReturnsAsync(category);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForCategoryAsync(category.Id, null, endDate))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetCategoryStatisticAsync(category.Id, null, endDate, category.UserId);
        result.Should().BeEquivalentTo(expected);

        CategoryRepositoryMock.Verify(repository => repository.GetByIdAsync(category.Id), Times.Once);
        ActivityQueryServiceMock.Verify(Service => Service.GetActivitiesForCategoryAsync(category.Id, null, endDate), Times.Once);
    }

    [Fact]
    public async Task GetCategoryStatistic_WithStartAndEndDate_ShouldReturnCorrectStatistic()
    {
        var category = StatisticsTestDataFactory.CreateCategory();
        var startDate = DateTime.MinValue.AddSeconds(DurationSecond);
        var endDate = DateTime.MaxValue.AddSeconds(-DurationSecond);

        var (activities, expectedTime) = CategoryStatisticScenarioBuilder.GetDataWithStartAndEndDate(category, startDate, endDate, DurationSecond);
        var expected = new CategoryStatisticResponse(expectedTime);
        
        CategoryRepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id)).ReturnsAsync(category);
        ActivityQueryServiceMock
            .Setup(service => service.GetActivitiesForCategoryAsync(category.Id, startDate, endDate))
            .Returns(activities.ToAsyncEnumerable());

        var result = await Service.GetCategoryStatisticAsync(category.Id, startDate, endDate, category.UserId);
        result.Should().BeEquivalentTo(expected);

        CategoryRepositoryMock.Verify(repository => repository.GetByIdAsync(category.Id), Times.Once);
        ActivityQueryServiceMock.Verify(Service => Service.GetActivitiesForCategoryAsync(category.Id, startDate, endDate), Times.Once);
    }

    [Fact]
    public async Task GetCategoryStatistic_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetCategoryStatisticAsync(Guid.Empty, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task GetCategoryStatistic_CategoryNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        CategoryRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Category?)null);

        await FluentActions
            .Awaiting(() => Service.GetCategoryStatisticAsync(id, null, null, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
