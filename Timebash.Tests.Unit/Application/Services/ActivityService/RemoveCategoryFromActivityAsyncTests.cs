using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public class RemoveCategoryFromActivityAsyncTests : ActivityServiceTestsBase
{
    [Fact]
    public async Task RemoveCategoryFromActivity_ValidAccess_ShouldReturnTrueAndRemove()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var category = new Category(Guid.NewGuid(), userId, Faker.Lorem.Word(), "#000000");
        var currentUpdatedTime = activity.UpdatedAt;

        SetupActivityEnsureAccess(activity, userId);
        SetupCategoryValidateAccess(category.Id, userId);
        SetupActivityIsCategoryLinked(activity.Id, category.Id, true);

        var result = await Service.RemoveCategoryFromActivityAsync(activity.Id, category.Id, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyCategoryValidateAccessCalled(category.Id, userId);
        VerifyActivityIsCategoryLinkedCalled(activity.Id, category.Id);
        ActivityRepositoryMock.Verify(
            repository => repository.RemoveCategoryFromActivityAsync(activity.Id, category.Id, It.IsAny<CancellationToken>()), 
            Times.Once);
        VerifySaveChangesCalled();
    }

    [Fact]
    public async Task RemoveCategoryFromActivity_WhenCategoryNotLinkedToActivity_ShouldReturnFalse()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var category = new Category(Guid.NewGuid(), userId, Faker.Lorem.Word(), "#000000");
        var currentUpdateTime = activity.UpdatedAt;

        SetupActivityEnsureAccess(activity, userId);
        SetupCategoryValidateAccess(category.Id, userId);
        SetupActivityIsCategoryLinked(activity.Id, category.Id, false);

        var result = await Service.RemoveCategoryFromActivityAsync(activity.Id, category.Id, userId, CancellationToken.None);

        result.Should().BeFalse();
        activity.UpdatedAt.Should().Be(currentUpdateTime);

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyCategoryValidateAccessCalled(category.Id, userId);
        VerifyActivityIsCategoryLinkedCalled(activity.Id, category.Id);
        VerifyRemoveCategoryFromActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task RemoveCategoryFromActivity_EmptyActivityId_ShouldThrowBadRequest()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.RemoveCategoryFromActivityAsync(id, Guid.NewGuid(), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(id, userId);
        VerifyCategoryValidateAccessNotCalled();
        VerifyActivityIsCategoryLinkedNotCalled();
        VerifyRemoveCategoryFromActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task RemoveCategoryFromActivity_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.RemoveCategoryFromActivityAsync(id, Guid.NewGuid(), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityEnsureAccessCalled(id, userId);
        VerifyCategoryValidateAccessNotCalled();
        VerifyActivityIsCategoryLinkedNotCalled();
        VerifyRemoveCategoryFromActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task RemoveCategoryFromActivity_EmptyCategoryId_ShouldThrowBadRequest()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var categoryId = Guid.Empty;

        SetupActivityEnsureAccess(activity, userId);
        SetupCategoryValidateAccessThrowsBadRequest(categoryId, userId);

        await FluentActions
            .Awaiting(() => Service.RemoveCategoryFromActivityAsync(activity.Id, categoryId, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyCategoryValidateAccessCalled(categoryId, userId);
        VerifyActivityIsCategoryLinkedNotCalled();
        VerifyRemoveCategoryFromActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task RemoveCategoryFromActivity_CategoryNotFound_ShouldThrowNotFound()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        SetupActivityEnsureAccess(activity, userId);
        SetupCategoryValidateAccessThrowsNotFound(categoryId, userId);

        await FluentActions
            .Awaiting(() => Service.RemoveCategoryFromActivityAsync(activity.Id, categoryId, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyCategoryValidateAccessCalled(categoryId, userId);
        VerifyActivityIsCategoryLinkedNotCalled();
        VerifyRemoveCategoryFromActivityNotCalled();
        VerifySaveChangesNotCalled();
    }
    
    private void VerifyRemoveCategoryFromActivityNotCalled()
        => ActivityRepositoryMock.Verify(
            repository => repository.RemoveCategoryFromActivityAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
}
