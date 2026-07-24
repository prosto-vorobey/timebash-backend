using Bogus;
using FluentAssertions;
using Moq;
using Timebash.Application.Services.Access;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.Repositories;

namespace Timebash.Tests.Unit.Application.Services.Access;

public class JournalAccessServiceTests
{
    private static readonly Faker _faker = new();
    private readonly JournalAccessService _service;
    private readonly Mock<IJournalRepository> _repositoryMock;

    public JournalAccessServiceTests()
    {
        _repositoryMock = new(MockBehavior.Strict);
        _service = new(_repositoryMock.Object);
    }

    [Fact]
    public async Task EnsureJournalAccess_ValidAccess_ShouldReturnJournal()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word());
        _repositoryMock.SetupGetById(journal);

        var result = await _service.EnsureAccessAsync(journal.Id, journal.UserId, CancellationToken.None);
        result.Should().Be(journal);

        _repositoryMock.VerifyGetByIdCalled(journal.Id);
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
        _repositoryMock.SetupGetById(id);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        _repositoryMock.VerifyGetByIdCalled(id);
    }

    [Fact]
    public async Task EnsureJournalAccess_WrongUserId_ShouldThrowNotFound()
    {
        var journal = new Journal(Guid.NewGuid(), Guid.NewGuid(), _faker.Lorem.Word());
        _repositoryMock.SetupGetById(journal);

        await FluentActions
            .Awaiting(() => _service.EnsureAccessAsync(journal.Id, Guid.NewGuid(), CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        _repositoryMock.VerifyGetByIdCalled(journal.Id);
    }

    [Fact]
    public async Task ValidateJournalAccess_WhenExists_ShouldReturn()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _repositoryMock.SetupIsUserLinked(id, userId, true);

        await _service.ValidateAccessAsync(id, userId, CancellationToken.None);

        _repositoryMock.VerifyIsUserLinkedCalled(id, userId);
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
        _repositoryMock.SetupIsUserLinked(id, userId, false);

        await FluentActions
            .Awaiting(() => _service.ValidateAccessAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        _repositoryMock.VerifyIsUserLinkedCalled(id, userId);
    }
}
