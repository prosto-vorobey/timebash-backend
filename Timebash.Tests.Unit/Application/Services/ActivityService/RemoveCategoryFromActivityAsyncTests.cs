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

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        CategoryRepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id)).ReturnsAsync(category);
        ActivityRepositoryMock.Setup(repository => repository.IsCategoryLinkedAsync(activity.Id, category.Id)).ReturnsAsync(true);

        var result = await Service.RemoveCategoryFromActivityAsync(activity.Id, category.Id, userId);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);

        ActivityRepositoryMock.Verify(repository => repository.RemoveCategoryFromActivityAsync(activity.Id, category.Id), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task RemoveCategoryFromActivity_WhenCategoryNotLinkedToActivity_ShouldReturnFalse()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var category = new Category(Guid.NewGuid(), userId, Faker.Lorem.Word(), "#000000");
        var currentUpdateTime = activity.UpdatedAt;

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        CategoryRepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id)).ReturnsAsync(category);
        ActivityRepositoryMock.Setup(repository => repository.IsCategoryLinkedAsync(activity.Id, category.Id)).ReturnsAsync(false);

        var result = await Service.RemoveCategoryFromActivityAsync(activity.Id, category.Id, userId);

        result.Should().BeFalse();
        activity.UpdatedAt.Should().Be(currentUpdateTime);

        ActivityRepositoryMock.Verify(repository => repository.RemoveCategoryFromActivityAsync(activity.Id, category.Id), Times.Never);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task RemoveCategoryFromActivity_EmptyActivityId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.RemoveCategoryFromActivityAsync(Guid.Empty, Guid.NewGuid(), Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task RemoveCategoryFromActivity_ActivityNotFound_ShouldThrowNotFound()
    {
        var activityId = Guid.NewGuid();
        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activityId)).ReturnsAsync((Activity?)null);

        await FluentActions
            .Awaiting(() => Service.RemoveCategoryFromActivityAsync(activityId, Guid.NewGuid(), Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task RemoveCategoryFromActivity_EmptyCategoryId_ShouldThrowBadRequest()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);

        await FluentActions
            .Awaiting(() => Service.RemoveCategoryFromActivityAsync(activity.Id, Guid.Empty, userId))
            .Should()
            .ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task RemoveCategoryFromActivity_CategoryNotFound_ShouldThrowNotFound()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        CategoryRepositoryMock.Setup(repository => repository.GetByIdAsync(categoryId)).ReturnsAsync((Category?)null);

        await FluentActions
            .Awaiting(() => Service.RemoveCategoryFromActivityAsync(activity.Id, categoryId, userId))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
