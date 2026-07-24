using FluentAssertions;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Application.Services.ActivityService.TestData;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

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

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories);
        SetupAddCategoriesToActivity(activity.Id, clearedCategoryIds);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        VerifyAddCategoriesToActivityCalled(activity.Id, clearedCategoryIds);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task UpdateActivityCategories_WithClearedCategories_ShouldReturnTrueAndUpdate()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var currentUpdatedTime = activity.UpdatedAt;
        var request = new ActivityCategoriesRequest([]);

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, [Guid.NewGuid()]);
        SetupClearActivityCategories(activity.Id);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyClearActivityCategoriesCalled(activity.Id);
        UnitOfWorkMock.VerifySaveChangesCalled();
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

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, clearedCategoryIds);

        var result = await Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeFalse();
        activity.UpdatedAt.Should().Be(currentUpdateTime);

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
    }

    [Fact]
    public async Task UpdateActivityCategories_EmptyActivityId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock.SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(id, new([]), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task UpdateActivityCategories_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock.SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(id, new([]), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(id, userId);
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

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, []);

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
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

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories.Skip(1));

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
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

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories);

        await FluentActions
            .Awaiting(() => Service.UpdateActivityCategoriesAsync(activity.Id, request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
    }
}
