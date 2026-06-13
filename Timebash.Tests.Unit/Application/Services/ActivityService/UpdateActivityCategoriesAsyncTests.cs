using FluentAssertions;
using Moq;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Application.Services.ActivityService.TestData;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public class UpdateActivityCategoriesAsyncTests : ActivityServiceTestsBase
{
    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    [ClassData(typeof(CategoriesWithEmptyGuidsData))]
    [ClassData(typeof(CategoryDuplicateIdsData))]
    public async Task UpdateActivityCategories_WithNewCategories_ShouldReturnTrueAndUpdate(
        Guid userId,
        List<Guid> categoryIds,
        List<Category> categories)
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = categoryIds.Where(id => id != Guid.Empty).Distinct().ToList();
        var currentUpdatedTime = activity.UpdatedAt;
        var request = new ActivityCategoriesRequest(categoryIds);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        ActivityRepositoryMock.Setup(repository => repository.GetCategoryIdsByActivityIdAsync(activity.Id)).ReturnsAsync([]);
        CategoryRepositoryMock.Setup(repository => repository.GetByIdsAsync(It.Is<IEnumerable<Guid>>(ids =>
            ids.OrderBy(id => id).SequenceEqual(clearedCategoryIds.OrderBy(id => id))))).ReturnsAsync(categories);

        var result = await Service.UpdateActivityCategoriesAsync(activity.Id, request, userId);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        ActivityRepositoryMock.Verify(repository => repository.AddCategoriesToActivity(activity.Id, It.Is<IEnumerable<Guid>>(ids =>
            ids.OrderBy(id => id).SequenceEqual(clearedCategoryIds.OrderBy(id => id)))), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.ClearActivityCategoriesAsync(activity.Id), Times.Never);
    }

    [Fact]
    public async Task UpdateActivityCategories_WithClearedCategories_ShouldReturnTrueAndUpdate()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var currentUpdatedTime = activity.UpdatedAt;
        var request = new ActivityCategoriesRequest([]);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        ActivityRepositoryMock.Setup(repository => repository.GetCategoryIdsByActivityIdAsync(activity.Id)).ReturnsAsync([Guid.NewGuid()]);

        var result = await Service.UpdateActivityCategoriesAsync(activity.Id, request, userId);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        ActivityRepositoryMock.Verify(repository => repository.ClearActivityCategoriesAsync(activity.Id), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.AddCategoryToActivity(activity.Id, It.IsAny<Guid>()), Times.Never);
    }

    [Theory]
    [ClassData(typeof(ValidCategoryIdsData))]
    [ClassData(typeof(EmptyCategoryIdsData))]
    public async Task UpdateActivityCategories_WhenAllCategoriesAlreadyLinked_ShouldReturnFalse(Guid userId, List<Guid> categoryIds)
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = categoryIds.Where(id => id != Guid.Empty).Distinct().ToList();
        var currentUpdateTime = activity.UpdatedAt;
        var request = new ActivityCategoriesRequest(categoryIds);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        ActivityRepositoryMock.Setup(repository => repository.GetCategoryIdsByActivityIdAsync(activity.Id)).ReturnsAsync(clearedCategoryIds);

        var result = await Service.UpdateActivityCategoriesAsync(activity.Id, request, userId);

        result.Should().BeFalse();
        activity.UpdatedAt.Should().Be(currentUpdateTime);

        ActivityRepositoryMock.Verify(repository => repository.AddCategoryToActivity(activity.Id, It.IsAny<Guid>()), Times.Never);
        ActivityRepositoryMock.Verify(repository => repository.ClearActivityCategoriesAsync(activity.Id), Times.Never);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateActivityCategories_EmptyActivityId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(Guid.Empty, new ActivityCategoriesRequest([]), Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task UpdateActivityCategories_ActivityNotFound_ShouldThrowNotFound()
    {
        var activityId = Guid.NewGuid();
        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activityId)).ReturnsAsync((Activity?)null);

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(activityId, new ActivityCategoriesRequest([]), Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateActivityCategories_CategoryEmptyGuids_ShouldThrowBadRequest()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var categoryIds = Enumerable.Range(0, Faker.Random.Number(2, 5))
                .Select(_ => Guid.Empty)
                .ToList();
        var request = new ActivityCategoriesRequest(categoryIds);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(activity.Id, request, userId))
            .Should()
            .ThrowAsync<BadRequestException>();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task UpdateActivityCategories_CategoryNotFound_ShouldThrowBadRequest(
        Guid userId,
        List<Guid> categoryIds,
        List<Category> categories)
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var request = new ActivityCategoriesRequest(categoryIds);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        CategoryRepositoryMock.Setup(repository => repository.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync(categories.Skip(1));

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(activity.Id, request, userId))
            .Should()
            .ThrowAsync<BadRequestException>();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task UpdateActivityCategories_CategoryNotLinkedToUser_ShouldThrowNotFound(
        Guid userId,
        List<Guid> categoryIds,
        List<Category> categories)
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        categories[0] = new Category(categories[0].Id, Guid.NewGuid(), Faker.Lorem.Word(), "#000000");
        var request = new ActivityCategoriesRequest(categoryIds);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        CategoryRepositoryMock.Setup(repository => repository.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync(categories);

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(activity.Id, request, userId))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
