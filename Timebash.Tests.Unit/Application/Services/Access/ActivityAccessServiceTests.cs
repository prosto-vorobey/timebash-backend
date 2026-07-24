using FluentAssertions;
using Moq;
using Timebash.Application.Services.Access;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.Repositories;

namespace Timebash.Tests.Unit.Application.Services.Access;

public class ActivityAccessServiceTests
{
    private readonly ActivityAccessService _service;
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly Mock<IJournalRepository> _journalRepositoryMock;

    public ActivityAccessServiceTests()
    {
        _activityRepositoryMock = new(MockBehavior.Strict);
        _journalRepositoryMock = new(MockBehavior.Strict);
        _service = new(_activityRepositoryMock.Object, _journalRepositoryMock.Object);
    }

    [Fact]
    public async Task EnsureActivityAccess_ValidAccess_ShouldReturnActivity()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();

        SetupActivityGetById(activity.Id, activity);
        _journalRepositoryMock.SetupIsUserLinked(activity.JournalId, userId, true);

        var result = await _service.EnsureAccessAsync(activity.Id, userId, CancellationToken.None);
        result.Should().Be(activity);

        VerifyActivityGetByIdCalled(activity.Id);
        _journalRepositoryMock.VerifyIsUserLinkedCalled(activity.JournalId, userId);
    }

    [Fact]
    public async Task EnsureActivityAccess_EmptyActivityId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task EnsureActivityAccess_NonexistentActivityId_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        SetupActivityGetById(id, null);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityGetByIdCalled(id);
    }

    [Fact]
    public async Task EnsureActivityAccess_WrongUserId_ShouldThrowNotFound()
    {
        var activity = new Activity(Guid.NewGuid(), Guid.NewGuid(), DateTime.MinValue, DateTime.MaxValue);
        var userId = Guid.NewGuid();

        SetupActivityGetById(activity.Id, activity);
        _journalRepositoryMock.SetupIsUserLinked(activity.JournalId, userId, false);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(activity.Id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityGetByIdCalled(activity.Id);
        _journalRepositoryMock.VerifyIsUserLinkedCalled(activity.JournalId, userId);
    }

    [Fact]
    public async Task ValidateActivityAccess_WhenExists_ShouldReturn()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupActivityIsOwnedByUserAsync(id, userId, true);

        await _service.ValidateAccessAsync(id, userId, CancellationToken.None);

        VerifyActivityIsOwnedByUserAsync(id, userId);
    }

    [Fact]
    public async Task ValidateActivityAccess_EmptyActivityId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => _service.ValidateAccessAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task ValidateActivityAccess_NotUserLinked_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        SetupActivityIsOwnedByUserAsync(id, userId, false);

        await FluentActions
            .Awaiting(() => _service.ValidateAccessAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityIsOwnedByUserAsync(id, userId);
    }

    private void SetupActivityGetById(Guid id, Activity? activity)
        => _activityRepositoryMock
            .Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

    private void SetupActivityIsOwnedByUserAsync(Guid id, Guid userId, bool isOwned)
        => _activityRepositoryMock.Setup(repository => repository.IsOwnedByUserAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(isOwned);

    private void VerifyActivityGetByIdCalled(Guid id)
        => _activityRepositoryMock.Verify(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyActivityIsOwnedByUserAsync(Guid id, Guid userId)
        => _activityRepositoryMock.Verify(repository => repository.IsOwnedByUserAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);
}
