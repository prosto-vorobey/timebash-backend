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

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(userSettings);

        await Service.DeleteAsync(journal.Id, userId);

        SettingsRepositoryMock.Verify(repository => repository.GetByIdAsync(journal.UserId), Times.Once);
        JournalRepositoryMock.Verify(repository => repository.Delete(journal), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
    => await FluentActions
        .Awaiting(() => Service.DeleteAsync(Guid.Empty, Guid.NewGuid()))
        .Should()
        .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task Delete_JournalNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(id)).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync((UserSettings?)null);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(journal.Id, userId))
            .Should()
            .ThrowAsync<NotFoundException>();
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

        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journal.Id)).ReturnsAsync(journal);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(userSettings);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(journal.Id, userId))
            .Should()
            .ThrowAsync<ConflictException>();
    }
}
