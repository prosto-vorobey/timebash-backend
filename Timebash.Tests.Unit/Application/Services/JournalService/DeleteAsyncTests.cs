using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.AccessServices;
using Timebash.Tests.Unit.TestInfrastructure.MockExtensions.Repositories;

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

        AccessServiceMock.SetupEnsureAccess(journal);
        SettingsRepositoryMock.SetupGetById(userSettings);
        JournalRepositoryMock.Setup(repository => repository.Delete(journal));
        UnitOfWorkMock.SetupSaveChanges();

        await Service.DeleteAsync(journal.Id, userId, CancellationToken.None);

        AccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        SettingsRepositoryMock.VerifyGetByIdCalled(journal.UserId);
        JournalRepositoryMock.Verify(repository => repository.Delete(journal), Times.Once);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
        .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
        .Should()
        .ThrowAsync<BadRequestException>();

        AccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task Delete_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        AccessServiceMock.SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        AccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task Delete_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());

        AccessServiceMock.SetupEnsureAccess(journal);
        SettingsRepositoryMock.SetupGetById(userId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(journal.Id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        AccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        SettingsRepositoryMock.VerifyGetByIdCalled(userId);
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

        AccessServiceMock.SetupEnsureAccess(journal);
        SettingsRepositoryMock.SetupGetById(userSettings);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(journal.Id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<ConflictException>();

        AccessServiceMock.VerifyEnsureAccessCalled(journal.Id, journal.UserId);
        SettingsRepositoryMock.VerifyGetByIdCalled(userId);
    }
}
