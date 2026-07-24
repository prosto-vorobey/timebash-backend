using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;

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

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        CategoryAccessServiceMock.SetupValidateAccess(category.Id, userId);
        SetupActivityIsCategoryLinked(activity.Id, category.Id, false);
        ActivityRepositoryMock.Setup(repository => repository.AddCategoryToActivity(activity.Id, category.Id));
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.AddCategoryToActivityAsync(activity.Id, category.Id, userId, CancellationToken.None);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        CategoryAccessServiceMock.VerifyValidateAccessCalled(category.Id, userId);
        VerifyActivityIsCategoryLinkedCalled(activity.Id, category.Id);
        ActivityRepositoryMock.Verify(repository => repository.AddCategoryToActivity(activity.Id, category.Id), Times.Once);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task AddCategoryToActivity_WhenCategoryAlreadyLinked_ShouldReturnFalse()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var category = new Category(Guid.NewGuid(), userId, Faker.Lorem.Word(), "#000000");
        var currentUpdateTime = activity.UpdatedAt;

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        CategoryAccessServiceMock.SetupValidateAccess(category.Id, userId);
        SetupActivityIsCategoryLinked(activity.Id, category.Id, true);

        var result = await Service.AddCategoryToActivityAsync(activity.Id, category.Id, userId, CancellationToken.None);

        result.Should().BeFalse();
        activity.UpdatedAt.Should().Be(currentUpdateTime);

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        CategoryAccessServiceMock.VerifyValidateAccessCalled(category.Id, userId);
        VerifyActivityIsCategoryLinkedCalled(activity.Id, category.Id);
    }


    [Fact]
    public async Task AddCategoryToActivity_EmptyActivityId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock.SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(id, Guid.NewGuid(), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task AddCategoryToActivity_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock.SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(id, Guid.NewGuid(), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task AddCategoryToActivity_EmptyCategoryId_ShouldThrowBadRequest()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var categoryId = Guid.Empty;

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        CategoryAccessServiceMock.SetupValidateAccessThrowsBadRequest(categoryId, userId);

        await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(activity.Id, categoryId, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        CategoryAccessServiceMock.VerifyValidateAccessCalled(categoryId, userId);
    }

    [Fact]
    public async Task AddCategoryToActivity_CategoryNotFound_ShouldThrowNotFound()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        CategoryAccessServiceMock.SetupValidateAccessThrowsNotFound(categoryId, userId);

        await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(activity.Id, categoryId, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        CategoryAccessServiceMock.VerifyValidateAccessCalled(categoryId, userId);
    }
}
