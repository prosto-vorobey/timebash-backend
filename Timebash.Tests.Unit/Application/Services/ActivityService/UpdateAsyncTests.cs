using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Application.Services.ActivityService.TestData;

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

        SetupActivityEnsureAccess(activity, userId);
        SetupJournalValidateAccess(request.JournalId, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, clearedCategoryIds);

        var result = await Service.UpdateAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyJournalValidateAccessCalled(request.JournalId, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifySaveChangesCalled();
        VerifyClearActivityCategoriesNotCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
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

        SetupActivityEnsureAccess(activity, userId);
        SetupJournalValidateAccess(request.JournalId, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories);

        var result = await Service.UpdateAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyJournalValidateAccessCalled(request.JournalId, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyActivityAddCategoriesToActivityCalled(activity.Id, clearedCategoryIds);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        VerifySaveChangesCalled();
        VerifyClearActivityCategoriesNotCalled();
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

        SetupActivityEnsureAccess(activity, userId);
        SetupJournalValidateAccess(request.JournalId, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, [Guid.NewGuid()]);

        var result = await Service.UpdateAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyJournalValidateAccessCalled(request.JournalId, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyClearActivityCategoriesCalled(activity.Id);
        VerifySaveChangesCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
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

        SetupActivityEnsureAccess(activity, userId);
        SetupJournalValidateAccess(request.JournalId, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, clearedCategoryIds);

        var result = await Service.UpdateAsync(activity.Id, request, userId, CancellationToken.None);

        result.Should().BeFalse();
        activity.UpdatedAt.Should().Be(currentUpdateTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyJournalValidateAccessCalled(request.JournalId, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyClearActivityCategoriesNotCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task Update_EmptyActivityId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                id,
                new(Guid.NewGuid(), string.Empty, DateTime.MinValue, DateTime.MaxValue, []), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(id, userId);
        VerifyJournalValidateAccessNotCalled();
        VerifyActivityGetCategoryIdsByActivityIdNotCalled();
        VerifyClearActivityCategoriesNotCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task Update_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                id,
                new(Guid.NewGuid(), string.Empty, DateTime.MinValue, DateTime.MaxValue, []), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityEnsureAccessCalled(id, userId);
        VerifyJournalValidateAccessNotCalled();
        VerifyActivityGetCategoryIdsByActivityIdNotCalled();
        VerifyClearActivityCategoriesNotCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task Update_EmptyJournalId_ShouldThrowBadRequest()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);

        SetupActivityEnsureAccess(activity, userId);
        JournalAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(journalId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                activity.Id,
                new ActivityRequest(journalId, string.Empty, activity.StartTime, activity.EndTime, []),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyJournalValidateAccessCalled(journalId, userId);
        VerifyActivityGetCategoryIdsByActivityIdNotCalled();
        VerifyClearActivityCategoriesNotCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task Update_JournalNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);

        SetupActivityEnsureAccess(activity, userId);
        JournalAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(journalId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                activity.Id,
                new ActivityRequest(activity.JournalId, string.Empty, activity.StartTime, activity.EndTime, []),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyJournalValidateAccessCalled(journalId, userId);
        VerifyActivityGetCategoryIdsByActivityIdNotCalled();
        VerifyClearActivityCategoriesNotCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
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

        SetupActivityEnsureAccess(activity, userId);
        SetupJournalValidateAccess(journalId, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, []);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                activity.Id,
                new(activity.JournalId, string.Empty, activity.StartTime, activity.EndTime, categoryIds),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyJournalValidateAccessCalled(journalId, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyClearActivityCategoriesNotCalled();
        VerifyCategoryGetByIdsNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task Update_CategoryNotFound_ShouldThrowBadRequest(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);

        SetupActivityEnsureAccess(activity, userId);
        SetupJournalValidateAccess(journalId, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories.Skip(1));

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                activity.Id,
                new(activity.JournalId, string.Empty, activity.StartTime, activity.EndTime, categoryIds),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyJournalValidateAccessCalled(journalId, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        VerifyClearActivityCategoriesNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task Update_CategoryNotLinkedToUser_ShouldThrowNotFound(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        categories[0] = new Category(categories[0].Id, Guid.NewGuid(), Faker.Lorem.Word(), "#000000");

        SetupActivityEnsureAccess(activity, userId);
        SetupJournalValidateAccess(journalId, userId);
        SetupActivityGetCategoryIdsByActivityId(activity.Id, []);
        SetupCategoryGetByIds(clearedCategoryIds, categories);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                activity.Id,
                new(activity.JournalId, string.Empty, activity.StartTime, activity.EndTime, categoryIds),
                userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyJournalValidateAccessCalled(journalId, userId);
        VerifyActivityGetCategoryIdsByActivityIdCalled(activity.Id);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        VerifyClearActivityCategoriesNotCalled();
        VerifyActivityAddCategoriesToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    private void SetupJournalValidateAccess(Guid id, Guid userId)
        => JournalAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    private void VerifyJournalValidateAccessCalled(Guid id, Guid userId)
        => JournalAccessServiceMock.Verify(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);
    
    private void VerifyJournalValidateAccessNotCalled()
        => JournalAccessServiceMock.Verify(
            service => service.ValidateAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
