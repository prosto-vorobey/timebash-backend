using FluentAssertions;
using Moq;
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

        SetupUserValidateExists(userId);
        SetupJournalValidateAccess(journalId, userId);
        SetupSettingsGetById(userId, userSettings);

        var result = await Service.UpdateDefaultJournalAsync(request, userId, CancellationToken.None);

        result.Should().BeTrue();
        userSettings.UserId.Should().Be(userId);
        userSettings.DefaultJournalId.Should().Be(request.JournalId);

        VerifyValidateExistsCalled(userId);
        VerifyJournalValidateAccessCalled(journalId, userId);
        VerifySettingsGetByIdCalled(userId);
        VerifySaveChangesCalled();
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

        SetupUserValidateExists(userId);
        SetupJournalValidateAccess(journalId, userId);
        SetupSettingsGetById(userId, userSettings);

        var result = await Service.UpdateDefaultJournalAsync(request, userId, CancellationToken.None);

        result.Should().BeFalse();
        userSettings.UserId.Should().Be(userId);
        userSettings.DefaultJournalId.Should().Be(journalId);

        VerifyValidateExistsCalled(userId);
        VerifyJournalValidateAccessCalled(journalId, userId);
        VerifySettingsGetByIdCalled(userId);
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task UpdateDefaultJournal_EmptyUserId_ShouldThrowBadRequest()
    {
        var userId = Guid.Empty;
        SetupUserValidateExistsThrowsBadRequest(userId);

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(Guid.NewGuid()), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyValidateExistsCalled(userId);
        VerifyJournalValidateAccessNotCalled();
        VerifySettingsGetByIdNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task UpdateDefaultJournal_UserNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        SetupUserValidateExistsThrowsNotFound(userId);

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(Guid.NewGuid()), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyValidateExistsCalled(userId);
        VerifyJournalValidateAccessNotCalled();
        VerifySettingsGetByIdNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task UpdateDefaultJournal_EmptyJournalId_ShouldThrowBasRequest()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.Empty;

        SetupUserValidateExists(userId);
        JournalAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(journalId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(journalId), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyValidateExistsCalled(userId);
        VerifyJournalValidateAccessCalled(journalId, userId);
        VerifySettingsGetByIdNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task UpdateDefaultJournal_JournalNotLinked_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();

        SetupUserValidateExists(userId);
        JournalAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(journalId, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(journalId), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyValidateExistsCalled(userId);
        VerifyJournalValidateAccessCalled(journalId, userId);
        VerifySettingsGetByIdNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task UpdateDefaultJournal_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();

        SetupUserValidateExists(userId);
        SetupJournalValidateAccess(journalId, userId);
        SetupSettingsGetById(userId, null);

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(journalId), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyValidateExistsCalled(userId);
        VerifyJournalValidateAccessCalled(journalId, userId);
        VerifySettingsGetByIdCalled(userId);
        VerifySaveChangesNotCalled();
    }

    private void SetupJournalValidateAccess(Guid id, Guid userId)
        => JournalAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    private void SetupSettingsGetById(Guid userId, UserSettings? userSettings)
        => SettingsRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userSettings);

    private void VerifyJournalValidateAccessCalled(Guid id, Guid userId)
        => JournalAccessServiceMock.Verify(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifySettingsGetByIdCalled(Guid userId)
        => SettingsRepositoryMock.Verify(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyJournalValidateAccessNotCalled()
        => JournalAccessServiceMock.Verify(
            service => service.ValidateAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

    private void VerifySettingsGetByIdNotCalled()
        => SettingsRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
}
