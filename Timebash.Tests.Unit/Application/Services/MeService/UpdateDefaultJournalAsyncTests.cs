using FluentAssertions;
using Timebash.Core.DTOs.Requests;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class UpdateDefaultJournalAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task UpdateDefaultJournal_ValidRequest_ShouldReturnTrueAndUpdate()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var userSettings = new UserSettings
        {
            UserId = userId,
            DefaultJournalId = journalId
        };
        var request = new DefaultJournalUpdateRequest(Guid.NewGuid());

        UserAccessServiceMock.SetupValidateExists(userId);
        JournalAccessServiceMock.SetupValidateAccess(request.JournalId, userId);
        SettingsRepositoryMock.SetupGetById(userSettings);
        UnitOfWorkMock.SetupSaveChanges();

        var result = await Service.UpdateDefaultJournalAsync(request, userId, CancellationToken.None);

        result.Should().BeTrue();
        userSettings.UserId.Should().Be(userId);
        userSettings.DefaultJournalId.Should().Be(request.JournalId);

        UserAccessServiceMock.VerifyValidateExistsCalled(userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(request.JournalId, userId);
        SettingsRepositoryMock.VerifyGetByIdCalled(userId);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task UpdateDefaultJournal_NoChanges_ShouldReturnFalse()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var userSettings = new UserSettings
        {
            UserId = userId,
            DefaultJournalId = journalId
        };
        var request = new DefaultJournalUpdateRequest(userSettings.DefaultJournalId);

        UserAccessServiceMock.SetupValidateExists(userId);
        JournalAccessServiceMock.SetupValidateAccess(request.JournalId, userId);
        SettingsRepositoryMock.SetupGetById(userSettings);

        var result = await Service.UpdateDefaultJournalAsync(request, userId, CancellationToken.None);

        result.Should().BeFalse();
        userSettings.UserId.Should().Be(userId);
        userSettings.DefaultJournalId.Should().Be(journalId);

        UserAccessServiceMock.VerifyValidateExistsCalled(userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(request.JournalId, userId);
        SettingsRepositoryMock.VerifyGetByIdCalled(userId);
    }

    [Fact]
    public async Task UpdateDefaultJournal_EmptyUserId_ShouldThrowBadRequest()
    {
        var userId = Guid.Empty;
        UserAccessServiceMock.SetupValidateExistsThrowsBadRequest(userId);

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(Guid.NewGuid()), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        UserAccessServiceMock.VerifyValidateExistsCalled(userId);
    }

    [Fact]
    public async Task UpdateDefaultJournal_UserNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        UserAccessServiceMock.SetupValidateExistsThrowsNotFound(userId);

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(Guid.NewGuid()), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        UserAccessServiceMock.VerifyValidateExistsCalled(userId);
    }

    [Fact]
    public async Task UpdateDefaultJournal_EmptyJournalId_ShouldThrowBasRequest()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.Empty;

        UserAccessServiceMock.SetupValidateExists(userId);
        JournalAccessServiceMock.SetupValidateAccessThrowsBadRequest(journalId, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(journalId), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        UserAccessServiceMock.VerifyValidateExistsCalled(userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journalId, userId);
    }

    [Fact]
    public async Task UpdateDefaultJournal_JournalNotLinked_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();

        UserAccessServiceMock.SetupValidateExists(userId);
        JournalAccessServiceMock.SetupValidateAccessThrowsNotFound(journalId, userId);

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(journalId), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        UserAccessServiceMock.VerifyValidateExistsCalled(userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journalId, userId);
    }

    [Fact]
    public async Task UpdateDefaultJournal_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();

        UserAccessServiceMock.SetupValidateExists(userId);
        JournalAccessServiceMock.SetupValidateAccess(journalId, userId);
        SettingsRepositoryMock.SetupGetById(userId);

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(journalId), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        UserAccessServiceMock.VerifyValidateExistsCalled(userId);
        JournalAccessServiceMock.VerifyValidateAccessCalled(journalId, userId);
        SettingsRepositoryMock.VerifyGetByIdCalled(userId);
    }
}
