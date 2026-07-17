using FluentAssertions;
using Moq;
using Timebash.Application.Extensions;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.MeService;

public class GetDefaultJournalAsyncTests : MeServiceTestsBase
{
    [Fact]
    public async Task GetDefaultJournal_ValidAccess_ShouldReturnResponse()
    {
        var userId = Guid.NewGuid();
        var userSettings = new UserSettings
        {
            UserId = userId,
            DefaultJournalId = Guid.NewGuid()
        };
        var journal = new Journal(userSettings.DefaultJournalId, userId, Faker.Lorem.Word());
        var expected = journal.ToResponse();

        SetupUserValidateExists(userId);
        SetupSettingsGetById(userId, userSettings);
        SetupJournalGetById(userSettings.DefaultJournalId, journal);

        var result = await Service.GetDefaultJournalAsync(userId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        
        VerifyValidateExistsCalled(userId);
        SettingsRepositoryMock.Verify(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        JournalRepositoryMock.Verify(repository => repository.GetByIdAsync(journal.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetDefaultJournal_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        SetupUserValidateExistsThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyValidateExistsCalled(id);
        VerifySettingsGetByIdNotCalled();
        VerifyJournalGetByIdNotCalled();
    }

    [Fact]
    public async Task GetDefaultJournal_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        SetupUserValidateExistsThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyValidateExistsCalled(id);
        VerifySettingsGetByIdNotCalled();
        VerifyJournalGetByIdNotCalled();
    }

    [Fact]
    public async Task GetDefaultJournal_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        SetupUserValidateExistsThrowsNotFound(id);
        SetupSettingsGetById(id, null);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyValidateExistsCalled(id);
        VerifyJournalGetByIdNotCalled();
    }

    [Fact]
    public async Task GetDefaultJournal_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userSettings = new UserSettings
        {
            UserId = id,
            DefaultJournalId = Guid.NewGuid()
        };

        SetupUserValidateExistsThrowsNotFound(id);
        SetupSettingsGetById(id, userSettings);
        SetupJournalGetById(userSettings.DefaultJournalId, null);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyValidateExistsCalled(id);
    }

    private void SetupSettingsGetById(Guid userId, UserSettings? userSettings)
        => SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(userSettings);

    private void SetupJournalGetById(Guid journalId, Journal? journal)
        => JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journalId, It.IsAny<CancellationToken>())).ReturnsAsync(journal);

    private void VerifySettingsGetByIdNotCalled()
        => SettingsRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

    private void VerifyJournalGetByIdNotCalled()
        => JournalRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
}
