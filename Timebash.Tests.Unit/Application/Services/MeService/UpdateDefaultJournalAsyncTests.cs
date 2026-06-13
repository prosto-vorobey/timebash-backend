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
        var journal = new Journal(journalId, userId, Faker.Lorem.Word());
        var request = new DefaultJournalUpdateRequest(Guid.NewGuid());

        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(userSettings);
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journalId)).ReturnsAsync(journal);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(request.JournalId, userId)).ReturnsAsync(true);

        var result = await Service.UpdateDefaultJournalAsync(request, userId);

        result.Should().BeTrue();
        userSettings.UserId.Should().Be(userId);
        userSettings.DefaultJournalId.Should().Be(request.JournalId);

        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
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
        var journal = new Journal(journalId, userId, Faker.Lorem.Word());
        var request = new DefaultJournalUpdateRequest(userSettings.DefaultJournalId);

        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync(userSettings);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(request.JournalId, userId)).ReturnsAsync(true);

        var result = await Service.UpdateDefaultJournalAsync(request, userId);

        result.Should().BeFalse();
        userSettings.UserId.Should().Be(userId);
        userSettings.DefaultJournalId.Should().Be(journalId);

        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task UpdateDefaultJournal_JournalNotLinked_ShouldThrowNotFound()
    {
        var userSettings = new UserSettings
        {
            UserId = Guid.NewGuid(),
            DefaultJournalId = Guid.NewGuid()
        };
        var request = new DefaultJournalUpdateRequest(Guid.NewGuid());

        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(request.JournalId, userSettings.UserId)).ReturnsAsync(false);

        await FluentActions
            .Invoking(() => Service.UpdateDefaultJournalAsync(request, userSettings.UserId))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateDefaultJournal_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var request = new DefaultJournalUpdateRequest(Guid.NewGuid());
        
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(request.JournalId, userId)).ReturnsAsync(true);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId)).ReturnsAsync((UserSettings?)null);

        await FluentActions
            .Invoking(() => Service.UpdateDefaultJournalAsync(request, userId))
            .Should().ThrowAsync<NotFoundException>();
    }
}
