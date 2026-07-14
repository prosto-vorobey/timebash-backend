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
    private readonly Mock<IUserRepository> _userRepositoryMock = new();
    private readonly Mock<IJournalRepository> _journalRepositoryMock = new();
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock = new();
    private readonly Mock<IActivityRepository> _activityRepositoryMock = new();

    [Fact]
    public async Task EnsureUserAccess_ValidAccess_ShouldReturnUser()
    {
        var user = new User(Guid.NewGuid(), _faker.Internet.UserName(), _faker.Internet.Email());
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await EntityAccessGuard.EnsureUserAccessAsync(_userRepositoryMock.Object, user.Id, CancellationToken.None);
        result.Should().Be(user);
    }

    [Fact]
    public async Task EnsureUserAccess_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureUserAccessAsync(_userRepositoryMock.Object, Guid.Empty, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task EnsureUserAccess_NonexistentId_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _userRepositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureUserAccessAsync(_userRepositoryMock.Object, id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateUserExists_WhenExists_ShouldReturn()
    {
        var id = Guid.NewGuid();
        _userRepositoryMock.Setup(repository => repository.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await EntityAccessGuard.ValidateUserExistsAsync(_userRepositoryMock.Object, id, CancellationToken.None);
    }

    [Fact]
    public async Task ValidateUserExists_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => EntityAccessGuard.ValidateUserExistsAsync(_userRepositoryMock.Object, Guid.Empty, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task ValidateUserExists_NonexistentId_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _userRepositoryMock.Setup(repository => repository.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.ValidateUserExistsAsync(_userRepositoryMock.Object, id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureJournalAccess_ValidAccess_ShouldReturnJournal()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word());
        _journalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id, It.IsAny<CancellationToken>())).ReturnsAsync(journal);

        var result = await EntityAccessGuard.EnsureJournalAccessAsync(
            _journalRepositoryMock.Object, 
            journal.Id, 
            journal.UserId, 
            CancellationToken.None);
        result.Should().Be(journal);
    }

    [Fact]
    public async Task EnsureJournalAccess_EmptyJournalId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureJournalAccessAsync(
                _journalRepositoryMock.Object, 
                Guid.Empty, 
                Guid.NewGuid(), 
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task EnsureJournalAccess_NonexistentJournalId_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _journalRepositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureJournalAccessAsync(_journalRepositoryMock.Object, id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureJournalAccess_WrongUserId_ShouldThrowNotFound()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word());
        _journalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id, It.IsAny<CancellationToken>())).ReturnsAsync(journal);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureJournalAccessAsync(
                _journalRepositoryMock.Object, 
                journal.Id, 
                Guid.NewGuid(), 
                CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateJournalAccess_WhenExists_ShouldReturn()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _journalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await EntityAccessGuard.ValidateJournalAccessAsync(_journalRepositoryMock.Object, id, userId, CancellationToken.None);
    }

    [Fact]
    public async Task ValidateJournalAccess_EmptyJournalId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => EntityAccessGuard.ValidateJournalAccessAsync(
                _journalRepositoryMock.Object, 
                Guid.Empty, 
                Guid.NewGuid(), 
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task ValidateJournalAccess_NotUserLinked_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _journalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.ValidateJournalAccessAsync(_journalRepositoryMock.Object, id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureCategoryAccess_ValidAccess_ShouldReturnCategory()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        var result = await EntityAccessGuard.EnsureCategoryAccessAsync(
            _categoryRepositoryMock.Object, 
            category.Id, 
            category.UserId, 
            CancellationToken.None);
        result.Should().Be(category);
    }

    [Fact]
    public async Task EnsureCategoryAccess_EmptyCategoryId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureCategoryAccessAsync(
                _categoryRepositoryMock.Object, 
                Guid.Empty, 
                Guid.NewGuid(), 
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task EnsureCategoryAccess_NonexistentCategoryId_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureCategoryAccessAsync(_categoryRepositoryMock.Object, id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureCategoryAccess_WrongUserId_ShouldThrowNotFound()
    {
        var category = new Category(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word(), "#000000");
        _categoryRepositoryMock.Setup(repository => repository.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureCategoryAccessAsync(
                _categoryRepositoryMock.Object, 
                category.Id, 
                Guid.NewGuid(), 
                CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateCategoryAccess_WhenExists_ShouldReturn()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _categoryRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await EntityAccessGuard.ValidateCategoryAccessAsync(_categoryRepositoryMock.Object, id, userId, CancellationToken.None);
    }

    [Fact]
    public async Task ValidateCategoryAccess_EmptyCategoryId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => EntityAccessGuard.ValidateCategoryAccessAsync(
                _categoryRepositoryMock.Object, 
                Guid.Empty, 
                Guid.NewGuid(), 
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task ValidateCategoryAccess_NotUserLinked_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _categoryRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.ValidateCategoryAccessAsync(_categoryRepositoryMock.Object, id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureActivityAccess_ValidAccess_ShouldReturnActivity()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();

        _activityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        _journalRepositoryMock
            .Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await EntityAccessGuard.EnsureActivityAccessAsync(
            _activityRepositoryMock.Object, 
            _journalRepositoryMock.Object, 
            activity.Id, 
            userId, 
            CancellationToken.None);
        result.Should().Be(activity);
    }

    [Fact]
    public async Task EnsureActivityAccess_EmptyActivityId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureActivityAccessAsync(
                _activityRepositoryMock.Object, 
                _journalRepositoryMock.Object, 
                Guid.Empty, 
                Guid.NewGuid(), 
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task EnsureActivityAccess_NonexistentActivityId_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _activityRepositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Activity?)null);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureActivityAccessAsync(
                _activityRepositoryMock.Object, 
                _journalRepositoryMock.Object, 
                id, 
                Guid.NewGuid(), 
                CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureActivityAccess_WrongUserId_ShouldThrowNotFound()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();

        _activityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id, It.IsAny<CancellationToken>())).ReturnsAsync(activity);
        _journalRepositoryMock
            .Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.EnsureActivityAccessAsync(
                _activityRepositoryMock.Object, 
                _journalRepositoryMock.Object, 
                activity.Id, 
                userId, 
                CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateActivityAccess_WhenExists_ShouldReturn()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _activityRepositoryMock.Setup(repository => repository.IsOwnedByUserAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await EntityAccessGuard.ValidateActivityAccessAsync(_activityRepositoryMock.Object, id, userId, CancellationToken.None);
    }

    [Fact]
    public async Task ValidateActivityAccess_EmptyActivityId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => EntityAccessGuard.ValidateActivityAccessAsync(
                _activityRepositoryMock.Object, 
                Guid.Empty, 
                Guid.NewGuid(), 
                CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task ValidateActivityAccess_NotUserLinked_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _activityRepositoryMock.Setup(repository => repository.IsOwnedByUserAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => EntityAccessGuard.ValidateActivityAccessAsync(_activityRepositoryMock.Object, id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
