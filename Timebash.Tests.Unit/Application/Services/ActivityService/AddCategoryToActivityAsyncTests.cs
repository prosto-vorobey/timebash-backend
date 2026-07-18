using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public class AddCategoryToActivityAsyncTests : ActivityServiceTestsBase
{
    [Fact]
    public async Task AddCategoryToActivity_ValidAccess_ShouldReturnTrueAndAdd()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var category = new Category(Guid.NewGuid(), userId, Faker.Lorem.Word(), "#000000");
        var currentUpdatedTime = activity.UpdatedAt;

        SetupActivityEnsureAccess(activity, userId);
        SetupCategoryValidateAccess(category.Id, userId);
        SetupActivityIsCategoryLinked(activity.Id, category.Id, false);

        var result = await Service.AddCategoryToActivityAsync(activity.Id, category.Id, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyCategoryValidateAccessCalled(category.Id, userId);
        VerifyActivityIsCategoryLinkedCalled(activity.Id, category.Id);
        ActivityRepositoryMock.Verify(repository => repository.AddCategoryToActivity(activity.Id, category.Id), Times.Once);
        VerifySaveChangesCalled();
    }

    [Fact]
    public async Task AddCategoryToActivity_WhenCategoryAlreadyLinked_ShouldReturnFalse()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var category = new Category(Guid.NewGuid(), userId, Faker.Lorem.Word(), "#000000");
        var currentUpdateTime = activity.UpdatedAt;

        SetupActivityEnsureAccess(activity, userId);
        SetupCategoryValidateAccess(category.Id, userId);
        SetupActivityIsCategoryLinked(activity.Id, category.Id, true);

        var result = await Service.AddCategoryToActivityAsync(activity.Id, category.Id, userId, CancellationToken.None);

        result.Should().BeFalse();
        activity.UpdatedAt.Should().Be(currentUpdateTime);

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyCategoryValidateAccessCalled(category.Id, userId);
        VerifyActivityIsCategoryLinkedCalled(activity.Id, category.Id);
        VerifyAddCategoryToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }


    [Fact]
    public async Task AddCategoryToActivity_EmptyActivityId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(id, Guid.NewGuid(), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(id, userId);
        VerifyCategoryValidateAccessNotCalled();
        VerifyActivityIsCategoryLinkedNotCalled();
        VerifyAddCategoryToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task AddCategoryToActivity_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(id, Guid.NewGuid(), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityEnsureAccessCalled(id, userId);
        VerifyCategoryValidateAccessNotCalled();
        VerifyActivityIsCategoryLinkedNotCalled();
        VerifyAddCategoryToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task AddCategoryToActivity_EmptyCategoryId_ShouldThrowBadRequest()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var categoryId = Guid.Empty;

        SetupActivityEnsureAccess(activity, userId);
        SetupCategoryValidateAccessThrowsBadRequest(categoryId, userId);

        await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(activity.Id, categoryId, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyCategoryValidateAccessCalled(categoryId, userId);
        VerifyActivityIsCategoryLinkedNotCalled();
        VerifyAddCategoryToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task AddCategoryToActivity_CategoryNotFound_ShouldThrowNotFound()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        SetupActivityEnsureAccess(activity, userId);
        SetupCategoryValidateAccessThrowsNotFound(categoryId, userId);

        await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(activity.Id, categoryId, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyCategoryValidateAccessCalled(categoryId, userId);
        VerifyActivityIsCategoryLinkedNotCalled();
        VerifyAddCategoryToActivityNotCalled();
        VerifySaveChangesNotCalled();
    }

    private void VerifyAddCategoryToActivityNotCalled()
        => ActivityRepositoryMock.Verify(repository => repository.AddCategoryToActivity(It.IsAny<Guid>(), It.IsAny<Guid>()), Times.Never);
}
