using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.JournalService;

public class DeleteAsyncTests : JournalServiceTestsBase
{
    [Fact]
    public async Task Delete_ValidAccess_ShouldDeleteJournal()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var userSettings = new UserSettings
        {
            UserId = userId,
            DefaultJournalId = Guid.NewGuid()
        };

        SetupEnsureAccess(journal);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(userSettings);

        await Service.DeleteAsync(journal.Id, userId, CancellationToken.None);

        VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        SettingsRepositoryMock.Verify(repository => repository.GetByIdAsync(journal.UserId, It.IsAny<CancellationToken>()), Times.Once);
        JournalRepositoryMock.Verify(repository => repository.Delete(journal), Times.Once);
        VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
        .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
        .Should()
        .ThrowAsync<BadRequestException>();

        VerifyEnsureAccessCalled(id, userId);
        VerifyJournalDeleteNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task Delete_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyEnsureAccessCalled(id, userId);
        VerifyJournalDeleteNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task Delete_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());

        SetupEnsureAccess(journal);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((UserSettings?)null);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(journal.Id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyJournalDeleteNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task Delete_DefaultJournalId_ShouldThrowConflict()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var userSettings = new UserSettings
        {
            UserId = userId,
            DefaultJournalId = journal.Id
        };

        SetupEnsureAccess(journal);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(userSettings);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(journal.Id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<ConflictException>();

        VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        VerifyJournalDeleteNotCalled();
        VerifySaveChangesNotCalled();
    }

    private void VerifyJournalDeleteNotCalled()
        => JournalRepositoryMock.Verify(repository => repository.Delete(It.IsAny<Journal>()), Times.Never);
}
