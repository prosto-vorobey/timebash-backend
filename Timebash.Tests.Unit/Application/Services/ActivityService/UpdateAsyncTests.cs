using FluentAssertions;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Application.Services.ActivityService.TestData;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public class UpdateAsyncTests : ActivityServiceTestsBase
{
    [Theory]
    [ClassData(typeof(ValidCategoryIdsData))]
    [ClassData(typeof(EmptyCategoryIdsData))]
    public async Task Update_WithActivityChanges_ShouldReturnTrueAndUpdate(Guid userId, List<Guid> categoryIds)
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var newJournalId = Guid.NewGuid();
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);;
        var request = new ActivityRequest(
            newJournalId,
            Faker.Lorem.Sentence(),
            activity.StartTime.AddHours(1),
            activity.EndTime.AddHours(-1),
            categoryIds);

        var currentUpdatedTime = activity.UpdatedAt;
        var expected = new Activity(activity.Id, activity.JournalId, activity.StartTime, activity.EndTime);
        expected.ApplyUpdate(request);

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        JournalAccessServiceMock.SetupValidateAccess(request.JournalId, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, clearedCategoryIds);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.UpdateAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(request.JournalId, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    [ClassData(typeof(CategoriesWithEmptyGuidsData))]
    [ClassData(typeof(CategoryDuplicateIdsData))]
    public async Task Update_WithCategoriesChanges_ShouldReturnTrueAndUpdate(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var activity = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);;
        var request = new ActivityRequest(activity.JournalId, activity.Name, activity.StartTime, activity.EndTime, categoryIds);

        var currentUpdatedTime = activity.UpdatedAt;
        var expected = new Activity(activity.Id, activity.JournalId, activity.StartTime, activity.EndTime, activity.Name);

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        JournalAccessServiceMock.SetupValidateAccess(request.JournalId, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories);
        SetupAddCategoriesToActivity(activity.Id, clearedCategoryIds);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.UpdateAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(request.JournalId, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyAddCategoriesToActivityCalled(activity.Id, clearedCategoryIds);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Update_WithClearedCategories_ShouldReturnTrueAndUpdate()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var activity = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        var request = new ActivityRequest(activity.JournalId, activity.Name, activity.StartTime, activity.EndTime, []);

        var currentUpdatedTime = activity.UpdatedAt;
        var expected = new Activity(activity.Id, activity.JournalId, activity.StartTime, activity.EndTime, activity.Name);

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        JournalAccessServiceMock.SetupValidateAccess(request.JournalId, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, [Guid.NewGuid()]);
        SetupClearActivityCategories(activity.Id);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.UpdateAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(request.JournalId, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyClearActivityCategoriesCalled(activity.Id);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Theory]
    [ClassData(typeof(ValidCategoryIdsData))]
    [ClassData(typeof(EmptyCategoryIdsData))]
    public async Task Update_NoChanges_ShouldReturnFalse(Guid userId, List<Guid> categoryIds)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var activity = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);;
        var currentUpdateTime = activity.UpdatedAt;
        var request = new ActivityRequest(activity.JournalId, activity.Name, activity.StartTime, activity.EndTime, categoryIds);

        var expected = new Activity(activity.Id, activity.JournalId, activity.StartTime, activity.EndTime, activity.Name);

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        JournalAccessServiceMock.SetupValidateAccess(request.JournalId, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, clearedCategoryIds);

        var result = await Service.UpdateAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeFalse();
        activity.UpdatedAt.Should().Be(currentUpdateTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(request.JournalId, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
    }

    [Fact]
    public async Task Update_EmptyActivityId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock.SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                id,
                new(Guid.NewGuid(), string.Empty, DateTime.MinValue, DateTime.MaxValue, []), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task Update_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock.SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                id,
                new(Guid.NewGuid(), string.Empty, DateTime.MinValue, DateTime.MaxValue, []), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task Update_EmptyJournalId_ShouldThrowBadRequest()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        JournalAccessServiceMock.SetupValidateAccessThrowsBadRequest(journalId, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                activity.Id,
                new ActivityRequest(journalId, string.Empty, activity.StartTime, activity.EndTime, []),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journalId, userId);
    }

    [Fact]
    public async Task Update_JournalNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        JournalAccessServiceMock.SetupValidateAccessThrowsNotFound(journalId, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                activity.Id,
                new ActivityRequest(activity.JournalId, string.Empty, activity.StartTime, activity.EndTime, []),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journalId, userId);
    }

    [Fact]
    public async Task Update_CategoryEmptyGuids_ShouldThrowBadRequest()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);
        var categoryIds = Enumerable.Range(0, Faker.Random.Number(2, 5))
                .Select(_ => Guid.Empty)
                .ToList();

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        JournalAccessServiceMock.SetupValidateAccess(journalId, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, []);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                activity.Id,
                new(activity.JournalId, string.Empty, activity.StartTime, activity.EndTime, categoryIds),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journalId, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task Update_CategoryNotFound_ShouldThrowBadRequest(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        JournalAccessServiceMock.SetupValidateAccess(journalId, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories.Skip(1));

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                activity.Id,
                new(activity.JournalId, string.Empty, activity.StartTime, activity.EndTime, categoryIds),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journalId, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task Update_CategoryNotLinkedToUser_ShouldThrowNotFound(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        categories[0] = new Category(categories[0].Id, Guid.NewGuid(), Faker.Lorem.Word(), "#000000");

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        JournalAccessServiceMock.SetupValidateAccess(journalId, userId);
        SetupGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                activity.Id,
                new(activity.JournalId, string.Empty, activity.StartTime, activity.EndTime, categoryIds),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journalId, userId);
        VerifyGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
    }
}
