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

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        CategoryRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(category.Id, userId)).ReturnsAsync(true);
        ActivityRepositoryMock.Setup(repository => repository.IsCategoryLinkedAsync(activity.Id, category.Id)).ReturnsAsync(false);

        var result = await Service.AddCategoryToActivityAsync(activity.Id, category.Id, userId);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        ActivityRepositoryMock.Verify(repository => repository.AddCategoryToActivity(activity.Id, category.Id), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task AddCategoryToActivity_WhenCategoryAlreadyLinked_ShouldReturnFalse()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var category = new Category(Guid.NewGuid(), userId, Faker.Lorem.Word(), "#000000");
        var currentUpdateTime = activity.UpdatedAt;

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        CategoryRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(category.Id, userId)).ReturnsAsync(true);
        ActivityRepositoryMock.Setup(repository => repository.IsCategoryLinkedAsync(activity.Id, category.Id)).ReturnsAsync(true);

        var result = await Service.AddCategoryToActivityAsync(activity.Id, category.Id, userId);

        result.Should().BeFalse();
        activity.UpdatedAt.Should().Be(currentUpdateTime);

        ActivityRepositoryMock.Verify(repository => repository.AddCategoryToActivity(activity.Id, category.Id), Times.Never);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Never);
    }


    [Fact]
    public async Task AddCategoryToActivity_EmptyActivityId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(Guid.Empty, Guid.NewGuid(), Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task AddCategoryToActivity_ActivityNotFound_ShouldThrowNotFound()
    {
        var activityId = Guid.NewGuid();
        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activityId)).ReturnsAsync((Activity?)null);

        await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(activityId, Guid.NewGuid(), Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task AddCategoryToActivity_EmptyCategoryId_ShouldThrowBadRequest()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);

        await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(activity.Id, Guid.Empty, userId))
            .Should()
            .ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task AddCategoryToActivity_CategoryNotFound_ShouldThrowNotFound()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var categoryId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        CategoryRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(categoryId, userId)).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.AddCategoryToActivityAsync(activity.Id, categoryId, userId))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
