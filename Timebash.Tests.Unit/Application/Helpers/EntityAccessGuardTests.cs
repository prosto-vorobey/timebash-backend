using Bogus;
using FluentAssertions;
using Moq;
using Timebash.Application.Helpers;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;

namespace Timebash.Tests.Unit.Application.Helpers;

public class EntityAccessGuardTests
{
    private static readonly Faker _faker = new();
    private readonly Mock<IJournalRepository> _journalRepositoryMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<IActivityRepository> _activityRepositoryMock = new();

    [Fact]
    public async Task EnsureJournalAccess_ValidAccess_ShouldReturnJournal()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word());
        _journalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);

        var result = await EntityAccessGuard.EnsureJournalAccessAsync(_journalRepositoryMock.Object, journal.Id, journal.UserId);
        result.Should().Be(journal);
    }

    [Fact]
    public async Task EnsureJournalAccess_EmptyJournalId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureJournalAccessAsync(_journalRepositoryMock.Object, Guid.Empty, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task EnsureJournalAccess_NonexistentJournalId_ShouldThrowNotFound()
    {
        var journalId = Guid.NewGuid();
        _journalRepositoryMock.Setup(repository => repository.GetByIdAsync(journalId)).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureJournalAccessAsync(_journalRepositoryMock.Object, journalId, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureJournalAccess_WrongUserId_ShouldThrowNotFound()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word());
        _journalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureJournalAccessAsync(_journalRepositoryMock.Object, journal.Id, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureCategoryAccess_ValidAccess_ShouldReturnCategory()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id)).ReturnsAsync(category);

        var result = await EntityAccessGuard.EnsureCategoryAccessAsync(_categoryRepositoryMock.Object, category.Id, category.UserId);
        result.Should().Be(category);
    }

    [Fact]
    public async Task EnsureCategoryAccess_EmptyCategoryId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureCategoryAccessAsync(_categoryRepositoryMock.Object, Guid.Empty, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task EnsureCategoryAccess_NonexistentCategoryId_ShouldThrowNotFound()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(categoryId)).ReturnsAsync((Category?)null);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureCategoryAccessAsync(_categoryRepositoryMock.Object, categoryId, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureCategoryAccess_WrongUserId_ShouldThrowNotFound()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id)).ReturnsAsync(category);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureCategoryAccessAsync(_categoryRepositoryMock.Object, category.Id, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureActivityAccess_ValidAccess_ShouldReturnActivity()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();

        _activityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        _journalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);

        var result = await EntityAccessGuard.EnsureActivityAccessAsync(_activityRepositoryMock.Object, _journalRepositoryMock.Object, activity.Id, userId);
        result.Should().Be(activity);
    }

    [Fact]
    public async Task EnsureActivityAccess_EmptyActivityId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureActivityAccessAsync(_activityRepositoryMock.Object, _journalRepositoryMock.Object, Guid.Empty, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task EnsureActivityAccess_NonexistentActivityId_ShouldThrowNotFound()
    {
        var activityId = Guid.NewGuid();
        _activityRepositoryMock.Setup(repository => repository.GetByIdAsync(activityId)).ReturnsAsync((Activity?)null);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureActivityAccessAsync(_activityRepositoryMock.Object, _journalRepositoryMock.Object, activityId, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureActivityAccess_WrongUserId_ShouldThrowNotFound()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();

        _activityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        _journalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureActivityAccessAsync(_activityRepositoryMock.Object, _journalRepositoryMock.Object, activity.Id, userId))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
