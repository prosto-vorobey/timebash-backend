using FluentAssertions;
using Moq;
using Timebash.Application.Services.Access;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;

namespace Timebash.Tests.Unit.Application.Services.Access;

public class ActivityAccessServiceTests
{
    private readonly ActivityAccessService _service;
    private readonly Mock<IActivityRepository> _activityRepositoryMock;
    private readonly Mock<IJournalRepository> _journalRepositoryMock;

    public ActivityAccessServiceTests()
    {
        _activityRepositoryMock = new();
        _journalRepositoryMock = new();
        _service = new(_activityRepositoryMock.Object, _journalRepositoryMock.Object);
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

        var result = await _service.EnsureAccessAsync(activity.Id, userId, CancellationToken.None);
        result.Should().Be(activity);
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
        _activityRepositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Activity?)null);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(id, Guid.NewGuid(), CancellationToken.None))
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
            .Awaiting(() => _service.EnsureAccessAsync(activity.Id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateActivityAccess_WhenExists_ShouldReturn()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _activityRepositoryMock.Setup(repository => repository.IsOwnedByUserAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _service.ValidateAccessAsync(id, userId, CancellationToken.None);
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
        _activityRepositoryMock.Setup(repository => repository.IsOwnedByUserAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => _service.ValidateAccessAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
