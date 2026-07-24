using FluentAssertions;
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

        UserAccessServiceMock.SetupValidateExists(userId);
        SettingsRepositoryMock.SetupGetById(userSettings);
        JournalRepositoryMock.SetupGetById(journal);

        var result = await Service.GetDefaultJournalAsync(userId, CancellationToken.None);

        result.Should().BeEquivalentTo(expected);
        
        UserAccessServiceMock.VerifyValidateExistsCalled(userId);
        SettingsRepositoryMock.VerifyGetByIdCalled(userId);
        JournalRepositoryMock.VerifyGetByIdCalled(journal.Id);
    }

    [Fact]
    public async Task GetDefaultJournal_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        UserAccessServiceMock.SetupValidateExistsThrowsBadRequest(id);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        UserAccessServiceMock.VerifyValidateExistsCalled(id);
    }

    [Fact]
    public async Task GetDefaultJournal_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserAccessServiceMock.SetupValidateExistsThrowsNotFound(id);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        UserAccessServiceMock.VerifyValidateExistsCalled(id);
    }

    [Fact]
    public async Task GetDefaultJournal_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserAccessServiceMock.SetupValidateExists(id);
        SettingsRepositoryMock.SetupGetById(id);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        UserAccessServiceMock.VerifyValidateExistsCalled(id);
        SettingsRepositoryMock.VerifyGetByIdCalled(id);
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

        UserAccessServiceMock.SetupValidateExistsThrowsNotFound(id);
        SettingsRepositoryMock.SetupGetById(id);
        JournalRepositoryMock.SetupGetById(userSettings.DefaultJournalId);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        UserAccessServiceMock.VerifyValidateExistsCalled(id);
    }
}
