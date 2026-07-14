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

        UserRepositoryMock.Setup(repository => repository.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(userSettings);
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id, It.IsAny<CancellationToken>())).ReturnsAsync(journal);

        var result = await Service.GetDefaultJournalAsync(userId, CancellationToken.None);
        result.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetDefaultJournal_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(Guid.Empty, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task GetDefaultJournal_UserNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetDefaultJournal_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((UserSettings?)null);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id, CancellationToken.None))
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

        UserRepositoryMock.Setup(repository => repository.ExistsAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(userSettings);
        JournalRepositoryMock
            .Setup(repository => repository.GetByIdAsync(userSettings.DefaultJournalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => Service.GetDefaultJournalAsync(id, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
