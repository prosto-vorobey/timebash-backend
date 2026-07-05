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
        var clearedCategoryIds = categoryIds.Where(id => id != Guid.Empty).Distinct().ToList();
        var request = new ActivityRequest(
            newJournalId,
            Faker.Lorem.Sentence(),
            activity.StartTime.AddHours(1),
            activity.EndTime.AddHours(-1),
            categoryIds);

        var currentUpdatedTime = activity.UpdatedAt;
        var expected = new Activity(activity.Id, activity.JournalId, activity.StartTime, activity.EndTime);
        expected.ApplyUpdate(request);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(newJournalId, userId)).ReturnsAsync(true);
        ActivityRepositoryMock.Setup(repository => repository.GetCategoryIdsByActivityIdAsync(activity.Id)).ReturnsAsync(clearedCategoryIds);

        var result = await Service.UpdateAsync(activity.Id, request, userId);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
        ActivityRepositoryMock.Verify(
            repository => repository.AddCategoriesToActivity(activity.Id, It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
        ActivityRepositoryMock.Verify(repository => repository.ClearActivityCategoriesAsync(activity.Id), Times.Never);
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    [ClassData(typeof(CategoriesWithEmptyGuidsData))]
    [ClassData(typeof(CategoryDuplicateIdsData))]
    public async Task Update_WithCategoriesChanges_ShouldReturnTrueAndUpdate(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var activity = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = categoryIds.Where(id => id != Guid.Empty).Distinct().ToList();
        var request = new ActivityRequest(activity.JournalId, activity.Name, activity.StartTime, activity.EndTime, categoryIds);

        var currentUpdatedTime = activity.UpdatedAt;
        var expected = new Activity(activity.Id, activity.JournalId, activity.StartTime, activity.EndTime, activity.Name);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, userId)).ReturnsAsync(true);
        ActivityRepositoryMock.Setup(repository => repository.GetCategoryIdsByActivityIdAsync(activity.Id)).ReturnsAsync([]);
        CategoryRepositoryMock.Setup(repository => repository.GetByIdsAsync(It.Is<IEnumerable<Guid>>(ids =>
                ids.OrderBy(id => id).SequenceEqual(clearedCategoryIds.OrderBy(id => id))))).ReturnsAsync(categories);

        var result = await Service.UpdateAsync(activity.Id, request, userId);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        ActivityRepositoryMock.Verify(repository => repository.AddCategoriesToActivity(activity.Id, It.Is<IEnumerable<Guid>>(ids =>
                ids.OrderBy(id => id).SequenceEqual(clearedCategoryIds.OrderBy(id => id)))), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
        ActivityRepositoryMock.Verify(repository => repository.ClearActivityCategoriesAsync(activity.Id), Times.Never);
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

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journal.Id, userId)).ReturnsAsync(true);
        ActivityRepositoryMock.Setup(repository => repository.GetCategoryIdsByActivityIdAsync(activity.Id)).ReturnsAsync([Guid.NewGuid()]);

        var result = await Service.UpdateAsync(activity.Id, request, userId);

        result.Should().BeTrue();
        activity.UpdatedAt.Should().BeAfter(currentUpdatedTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        ActivityRepositoryMock.Verify(repository => repository.ClearActivityCategoriesAsync(activity.Id), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
        ActivityRepositoryMock.Verify(
            repository => repository.AddCategoriesToActivity(activity.Id, It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    [Theory]
    [ClassData(typeof(ValidCategoryIdsData))]
    [ClassData(typeof(EmptyCategoryIdsData))]
    public async Task Update_NoChanges_ShouldReturnFalse(Guid userId, List<Guid> categoryIds)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var activity = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        var clearedCategoryIds = categoryIds.Where(id => id != Guid.Empty).Distinct().ToList();
        var currentUpdateTime = activity.UpdatedAt;
        var request = new ActivityRequest(activity.JournalId, activity.Name, activity.StartTime, activity.EndTime, categoryIds);

        var expected = new Activity(activity.Id, activity.JournalId, activity.StartTime, activity.EndTime, activity.Name);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        ActivityRepositoryMock.Setup(repository => repository.GetCategoryIdsByActivityIdAsync(activity.Id)).ReturnsAsync(clearedCategoryIds);

        var result = await Service.UpdateAsync(activity.Id, request, userId);

        result.Should().BeFalse();
        activity.UpdatedAt.Should().Be(currentUpdateTime);
        activity.Should().BeEquivalentTo(
            expected,
            options => options
                .Excluding(activity => activity.CreatedAt)
                .Excluding(activity => activity.UpdatedAt));

        ActivityRepositoryMock.Verify(
            repository => repository.AddCategoriesToActivity(activity.Id, It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
        ActivityRepositoryMock.Verify(repository => repository.ClearActivityCategoriesAsync(activity.Id), Times.Never);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Update_EmptyActivityId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                Guid.Empty,
                new ActivityRequest(Guid.NewGuid(), string.Empty, DateTime.MinValue, DateTime.MaxValue, []), Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task Update_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Activity?)null);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(
                id,
                new ActivityRequest(Guid.NewGuid(), string.Empty, DateTime.MinValue, DateTime.MaxValue, []), Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_EmptyJournalId_ShouldThrowBadRequest()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();
        var request = new ActivityRequest(Guid.Empty, string.Empty, activity.StartTime, activity.EndTime, []);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(activity.Id, request, userId))
            .Should()
            .ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task Update_JournalNotFound_ShouldThrowNotFound()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var newJournalId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new ActivityRequest(newJournalId, string.Empty, activity.StartTime, activity.EndTime, []);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(newJournalId, userId)).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(activity.Id, request, userId))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Update_CategoryEmptyGuids_ShouldThrowBadRequest()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var activity = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        var categoryIds = Enumerable.Range(0, Faker.Random.Number(2, 5))
                .Select(_ => Guid.Empty)
                .ToList();
        var request = new ActivityRequest(activity.JournalId, string.Empty, activity.StartTime, activity.EndTime, categoryIds);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, journal.UserId)).ReturnsAsync(true);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(activity.Id, request, journal.UserId))
            .Should()
            .ThrowAsync<BadRequestException>();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task Update_CategoryNotFound_ShouldThrowBadRequest(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var activity = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        var request = new ActivityRequest(activity.JournalId, string.Empty, activity.StartTime, activity.EndTime, categoryIds);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        CategoryRepositoryMock.Setup(repository => repository.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync(categories.Skip(1));

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(activity.Id, request, userId))
            .Should()
            .ThrowAsync<BadRequestException>();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task Update_CategoryNotLinkedToUser_ShouldThrowNotFound(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var activity = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        categories[0] = new Category(categories[0].Id, Guid.NewGuid(), Faker.Lorem.Word(), "#000000");
        var request = new ActivityRequest(activity.JournalId, string.Empty, activity.StartTime, activity.EndTime, categoryIds);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        CategoryRepositoryMock.Setup(repository => repository.GetByIdsAsync(It.IsAny<List<Guid>>())).ReturnsAsync(categories);

        await FluentActions
            .Awaiting(() => Service.UpdateAsync(activity.Id, request, userId))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
