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

        UserRepositoryMock.Setup(repository => repository.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(userSettings);
        JournalRepositoryMock
            .Setup(repository => repository.IsUserLinkedAsync(request.JournalId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journalId, It.IsAny<CancellationToken>())).ReturnsAsync(journal);

        var result = await Service.UpdateDefaultJournalAsync(request, userId, CancellationToken.None);

        result.Should().BeTrue();
        userSettings.UserId.Should().Be(userId);
        userSettings.DefaultJournalId.Should().Be(request.JournalId);

        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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

        UserRepositoryMock.Setup(repository => repository.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        JournalRepositoryMock
            .Setup(repository => repository.IsUserLinkedAsync(request.JournalId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(userSettings);

        var result = await Service.UpdateDefaultJournalAsync(request, userId, CancellationToken.None);

        result.Should().BeFalse();
        userSettings.UserId.Should().Be(userId);
        userSettings.DefaultJournalId.Should().Be(journalId);

        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateDefaultJournal_EmptyUserId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(Guid.NewGuid()), Guid.Empty, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");

    [Fact]
    public async Task UpdateDefaultJournal_UserNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(Guid.NewGuid()), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateDefaultJournal_EmptyJournalId_ShouldThrowBasRequest()
    {
        var userId = Guid.NewGuid();
        UserRepositoryMock.Setup(repository => repository.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        
        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(new(Guid.Empty), userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Invalid id");
    }

    [Fact]
    public async Task UpdateDefaultJournal_JournalNotLinked_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var userSettings = new UserSettings
        {
            UserId = userId,
            DefaultJournalId = Guid.NewGuid()
        };
        var request = new DefaultJournalUpdateRequest(Guid.NewGuid());

        UserRepositoryMock.Setup(repository => repository.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        JournalRepositoryMock
            .Setup(repository => repository.IsUserLinkedAsync(request.JournalId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateDefaultJournal_UserSettingsNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var request = new DefaultJournalUpdateRequest(Guid.NewGuid());
        
        UserRepositoryMock.Setup(repository => repository.ExistsAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        JournalRepositoryMock
            .Setup(repository => repository.IsUserLinkedAsync(request.JournalId, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        SettingsRepositoryMock.Setup(repository => repository.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((UserSettings?)null);

        await FluentActions
            .Awaiting(() => Service.UpdateDefaultJournalAsync(request, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
