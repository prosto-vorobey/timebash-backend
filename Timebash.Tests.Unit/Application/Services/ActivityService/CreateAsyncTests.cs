using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Application.Extensions.Requests;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.Application.Services.ActivityService.TestData;

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
        var clearedCategoryIds = categoryIds.Where(id => id != Guid.Empty).Distinct().ToList();
        var currentUpdatedTime = journal.UpdatedAt;
        var request = new ActivityRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, categoryIds);
        Activity? capturedActivity = null;

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(request.JournalId, It.IsAny<CancellationToken>())).ReturnsAsync(journal);
        ActivityRepositoryMock
            .Setup(repository => repository.Add(It.IsAny<Activity>()))
            .Callback<Activity>(activity => capturedActivity = activity);
        CategoryRepositoryMock
            .Setup(repository => repository.GetByIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.OrderBy(id => id).SequenceEqual(clearedCategoryIds.OrderBy(id => id))),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

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

        ActivityRepositoryMock.Verify(repository => repository.AddCategoriesToActivity(capturedActivity.Id, It.Is<IEnumerable<Guid>>(ids =>
            ids.OrderBy(id => id).SequenceEqual(clearedCategoryIds.OrderBy(id => id)))), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [ClassData(typeof(EmptyOrEmptyGuidListData))]
    public async Task Create_WithoutCategories_ShouldReturnResponse(List<Guid> categoryIds)
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), Faker.Lorem.Word());
        var currentUpdatedTime = journal.UpdatedAt;
        var request = new ActivityRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, categoryIds);
        Activity? capturedActivity = null;

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(request.JournalId, It.IsAny<CancellationToken>())).ReturnsAsync(journal);
        ActivityRepositoryMock
            .Setup(repository => repository.Add(It.IsAny<Activity>()))
            .Callback<Activity>(activity => capturedActivity = activity);

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

        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        ActivityRepositoryMock.Verify(
            repository => repository.AddCategoriesToActivity(capturedActivity.Id, It.IsAny<IEnumerable<Guid>>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_EmptyJournalId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.CreateAsync(
                new ActivityRequest(Guid.Empty, string.Empty, DateTime.MinValue, DateTime.MaxValue, []), Guid.NewGuid(),
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task Create_JournalNotFound_ShouldThrowNotFound()
    {
        var request = new ActivityRequest(Guid.NewGuid(), string.Empty, DateTime.MinValue, DateTime.MaxValue, []);
        var userId = Guid.NewGuid();

        JournalRepositoryMock
            .Setup(repository => repository.GetByIdAsync(request.JournalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => Service.CreateAsync(request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task Create_CategoryNotFound_ShouldThrowBadRequest(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var request = new ActivityRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, categoryIds);

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(request.JournalId, It.IsAny<CancellationToken>())).ReturnsAsync(journal);
        CategoryRepositoryMock
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories.Skip(1));

        await FluentActions
            .Awaiting(() => Service.CreateAsync(request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();
    }

    [Theory]
    [ClassData(typeof(ValidCategoriesData))]
    public async Task Create_CategoryNotLinkedToUser_ShouldThrowNotFound(Guid userId, List<Guid> categoryIds, List<Category> categories)
    {
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var request = new ActivityRequest(journal.Id, string.Empty, DateTime.MinValue, DateTime.MaxValue, categoryIds);
        categories[0] = new Category(categories[0].Id, Guid.NewGuid(), Faker.Lorem.Word(), "#000000");

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(request.JournalId, It.IsAny<CancellationToken>())).ReturnsAsync(journal);
        CategoryRepositoryMock
            .Setup(repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        await FluentActions
            .Awaiting(() => Service.CreateAsync(request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
