using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Application.Extensions.Requests;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Application.Services.ActivityService.TestData;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public class CreateAsyncTests : ActivityServiceTestsBase
{
    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    [ClassData(typeof(CategoriesWithEmptyGuidsData))]
    [ClassData(typeof(CategoryDuplicateIdsData))]
    public async Task Create_WithCategories_ShouldReturnResponse(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        var currentUpdatedTime = journal.UpdatedAt;
        var request = new ActivityRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, categoryIds);
        Activity? capturedActivity = null;

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        ActivityRepositoryMock
            .Setup(repository => repository.Add(It.IsAny<Activity>()))
            .Callback<Activity>(activity => capturedActivity = activity);
        SetupCategoryGetByIds(clearedCategoryIds, categories);
        SetupAddCategoriesToActivity(clearedCategoryIds);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.CreateAsync(request, userId, CancellationToken.None);

        capturedActivity.Should().NotBeNull();
        capturedActivity.Id.Should().NotBeEmpty();
        capturedActivity.Should().BeEquivalentTo(
            request.ToActivity(capturedActivity.Id),
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        result.Should().BeEquivalentTo(capturedActivity.ToResponse());
        journal.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
        VerifyAddCategoriesToActivityCalled(capturedActivity.Id, clearedCategoryIds);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Theory]
    [ClassData(typeof(EmptyOrEmptyGuidListData))]
    public async Task Create_WithoutCategories_ShouldReturnResponse(List<Guid> categoryIds)
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var currentUpdatedTime = journal.UpdatedAt;
        var request = new ActivityRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, categoryIds);
        Activity? capturedActivity = null;

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        ActivityRepositoryMock
            .Setup(repository => repository.Add(It.IsAny<Activity>()))
            .Callback<Activity>(activity => capturedActivity = activity);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.CreateAsync(request, journal.UserId, CancellationToken.None);

        capturedActivity.Should().NotBeNull();
        capturedActivity.Id.Should().NotBeEmpty();
        capturedActivity.Should().BeEquivalentTo(
            request.ToActivity(capturedActivity.Id),
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        result.Should().BeEquivalentTo(capturedActivity.ToResponse());
        journal.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Create_EmptyJournalId_ShouldThrowBadRequest()
    {
        var journalId = Guid.Empty;
        var userId = Guid.NewGuid();

        JournalAccessServiceMock.SetupEnsureAccessThrowsBadRequest(journalId, userId);

        await FluentActions
            .Awaiting(() => Service.CreateAsync(
                new ActivityRequest(journalId, string.Empty, DateTime.MinValue, DateTime.MaxValue, []), userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journalId, userId);
    }

    [Fact]
    public async Task Create_JournalNotFound_ShouldThrowNotFound()
    {
        var journalId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        JournalAccessServiceMock.SetupEnsureAccessThrowsNotFound(journalId, userId);

        await FluentActions
            .Awaiting(() => Service.CreateAsync(
                new ActivityRequest(journalId, string.Empty, DateTime.MinValue, DateTime.MaxValue, []), userId,
                CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journalId, userId);
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task Create_CategoryNotFound_ShouldThrowBadRequest(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        var request = new ActivityRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, categoryIds);

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        CategoryAccessServiceMock.SetupValidateAccessThrowsBadRequest(journal.Id, userId);
        SetupCategoryGetByIds(clearedCategoryIds, categories.Skip(1));

        await FluentActions
            .Awaiting(() => Service.CreateAsync(request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, userId);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task Create_CategoryNotLinkedToUser_ShouldThrowNotFound(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var clearedCategoryIds = GetClearedCategoryIds(categoryIds);
        var request = new ActivityRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, categoryIds);
        categories[0] = new Category(categories[0].Id, Guid.NewGuid(), Faker.Lorem.Word(), "#000000");

        JournalAccessServiceMock.SetupEnsureAccess(journal);
        CategoryAccessServiceMock.SetupValidateAccessThrowsBadRequest(journal.Id, userId);
        SetupCategoryGetByIds(clearedCategoryIds, categories);

        await FluentActions
            .Awaiting(() => Service.CreateAsync(request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        JournalAccessServiceMock.VerifyEnsureAccessCalled(journal.Id, userId);
        VerifyCategoryGetByIdsCalled(clearedCategoryIds);
    }
}
