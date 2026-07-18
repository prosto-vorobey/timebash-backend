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
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        var currentUpdatedTime = activity.UpdatedAt;
        var request = new ActivityCategoriesRequest(categoryIds);

        SetupActivityEnsureAccess(activity, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories);

        var result = await Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        VerifyActivityAddCategoriesToActivityCalled(activity.Id, clearedCategoryIds);
        VerifySaveChangesCalled();
        VerifyClearActivityCategoriesNotCalled();
    }

    [Fact]
    public async Task UpdateActivityCategories_WithClearedCategories_ShouldReturnTrueAndUpdate()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var currentUpdatedTime = activity.UpdatedAt;
        var request = new ActivityCategoriesRequest([]);

        SetupActivityEnsureAccess(activity, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, [Guid.NewGuid()]);

        var result = await Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyClearActivityCategoriesCalled(activity.Id);
        VerifySaveChangesCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
    }

    [Theory]
    [ClassData(typeof(ValidCategoryIdsData))]
    [ClassData(typeof(EmptyCategoryIdsData))]
    public async Task UpdateActivityCategories_WhenAllCategoriesAlreadyLinked_ShouldReturnFalse(Guid userId, List<Guid> categoryIds)
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        var currentUpdateTime = activity.UpdatedAt;
        var request = new ActivityCategoriesRequest(categoryIds);

        SetupActivityEnsureAccess(activity, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, clearedCategoryIds);

        var result = await Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeFalse();
        activity.UpdatedAt.Should().Be(currentUpdateTime);

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyClearActivityCategoriesNotCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task UpdateActivityCategories_EmptyActivityId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(id, new([]), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(id, userId);
        VerifyActivityGetCategoryIdsByActivityIdNotCalled();
        VerifyClearActivityCategoriesNotCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task UpdateActivityCategories_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(id, new([]), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityEnsureAccessCalled(id, userId);
        VerifyActivityGetCategoryIdsByActivityIdNotCalled();
        VerifyClearActivityCategoriesNotCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
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

        SetupActivityEnsureAccess(activity, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, []);

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyClearActivityCategoriesNotCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task UpdateActivityCategories_CategoryNotFound_ShouldThrowBadRequest(
        Guid userId,
        List<Guid> categoryIds,
        List<Category> categories)
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        var request = new ActivityCategoriesRequest(categoryIds);

        SetupActivityEnsureAccess(activity, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories.Skip(1));

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        VerifyClearActivityCategoriesNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task UpdateActivityCategories_CategoryNotLinkedToUser_ShouldThrowNotFound(
        Guid userId,
        List<Guid> categoryIds,
        List<Category> categories)
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        categories[0] = new Category(categories[0].Id, Guid.NewGuid(), Faker.Lorem.Word(), "#000000");
        var request = new ActivityCategoriesRequest(categoryIds);

        SetupActivityEnsureAccess(activity, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories);

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        VerifyClearActivityCategoriesNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }
}
