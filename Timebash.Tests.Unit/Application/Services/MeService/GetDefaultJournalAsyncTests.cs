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
        var userSettings = new UserSettings
        {
            UserId = Guid.NewGuid(),
            DefaultJournalId = Guid.NewGuid()
        };
        var journal = new Journal(userSettings.DefaultJournalId, userSettings.UserId, Faker.Lorem.Word());
        var expected = journal.ToResponse();

        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userSettings.UserId)).ReturnsAsync(userSettings);
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);

        var result = await Service.GetDefaultJournalAsync(userSettings.UserId);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetDefaultJournal_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((UserSettings?)null);

        await FluentActions
            .Invoking(() => Service.GetDefaultJournalAsync(id))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetDefaultJournal_JournalNotFound_ShouldThrowNotFound()
    {
        var userSettings = new UserSettings
        {
            UserId = Guid.NewGuid(),
            DefaultJournalId = Guid.NewGuid()
        };

        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userSettings.UserId)).ReturnsAsync(userSettings);
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(userSettings.DefaultJournalId)).ReturnsAsync((Journal?)null);

        await FluentActions
            .Invoking(() => Service.GetDefaultJournalAsync(userSettings.UserId))
            .Should().ThrowAsync<NotFoundException>();
    }
}
