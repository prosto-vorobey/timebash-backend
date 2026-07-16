using Bogus;
using FluentAssertions;
using Moq;
using Timebash.Application.Services.Access;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;

namespace Timebash.Tests.Unit.Application.Services.Access;

public class JournalAccessServiceTests
{
    private static readonly Faker _faker = new();
    private readonly JournalAccessService _service;
    private readonly Mock<IJournalRepository> _repositoryMock;

    public JournalAccessServiceTests()
    {
        _repositoryMock = new();
        _service = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task EnsureJournalAccess_ValidAccess_ShouldReturnJournal()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word());
        _repositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id, It.IsAny<CancellationToken>())).ReturnsAsync(journal);

        var result = await _service.EnsureAccessAsync(journal.Id, journal.UserId, CancellationToken.None);
        result.Should().Be(journal);
    }

    [Fact]
    public async Task EnsureJournalAccess_EmptyJournalId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task EnsureJournalAccess_NonexistentJournalId_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        _repositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task EnsureJournalAccess_WrongUserId_ShouldThrowNotFound()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word());
        _repositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id, It.IsAny<CancellationToken>())).ReturnsAsync(journal);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(journal.Id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task ValidateJournalAccess_WhenExists_ShouldReturn()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        await _service.ValidateAccessAsync(id, userId, CancellationToken.None);
    }

    [Fact]
    public async Task ValidateJournalAccess_EmptyJournalId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => _service.ValidateAccessAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task ValidateJournalAccess_NotUserLinked_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _repositoryMock.Setup(repository => repository.IsUserLinkedAsync(id, userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => _service.ValidateAccessAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
