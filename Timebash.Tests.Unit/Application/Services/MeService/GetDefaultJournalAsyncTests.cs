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

        UserRepositoryMock.Setup(repository => repository.ExistsAsync(userId)).ReturnsAsync(true);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(userSettings);
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);

        var result = await Service.GetDefaultJournalAsync(userId);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetDefaultJournal_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(Guid.Empty))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task GetDefaultJournal_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.ExistsAsync(id)).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetDefaultJournal_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.ExistsAsync(id)).ReturnsAsync(true);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((UserSettings?)null);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id))
            .Should()
            .ThrowAsync<NotFoundException>();
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

        UserRepositoryMock.Setup(repository => repository.ExistsAsync(id)).ReturnsAsync(true);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync(userSettings);
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(userSettings.DefaultJournalId)).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
