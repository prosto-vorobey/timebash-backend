using FluentAssertions;
using Moq;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public class DeleteAsyncTests : ActivityServiceTestsBase
{
    [Fact]
    public async Task Delete_ValidAccess_ShouldDeleteActivity()
    {
        var userId = Guid.NewGuid();
        var journal = new Journal(Guid.NewGuid(), userId, Faker.Lorem.Word());
        var activity = new Activity(Guid.NewGuid(), journal.Id, DateTime.MinValue, DateTime.MaxValue);
        var currentJournalUpdatedTime = journal.UpdatedAt;

        SetupActivityEnsureAccess(activity, userId);
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.JournalId, It.IsAny<CancellationToken>())).ReturnsAsync(journal);

        await Service.DeleteAsync(activity.Id, userId, CancellationToken.None);

        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyJournalGetByIdAsyncCalled(activity.JournalId);
        VerifyActivityDeleteCalled(activity);
        VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        VerifyActivityEnsureAccessCalled(id, userId);
        VerifyJournalGetByIdAsyncNotCalled();
        VerifyActivityDeleteNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task Delete_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        SetupActivityEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityEnsureAccessCalled(id, userId);
        VerifyJournalGetByIdAsyncNotCalled();
        VerifyActivityDeleteNotCalled();
        VerifySaveChangesNotCalled();
    }

    [Fact]
    public async Task Delete_JournalNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);

        SetupActivityEnsureAccess(activity, userId);
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journalId, It.IsAny<CancellationToken>())).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(activity.Id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        VerifyActivityEnsureAccessCalled(activity.Id, userId);
        VerifyJournalGetByIdAsyncCalled(journalId);
        VerifyActivityDeleteNotCalled();
        VerifySaveChangesNotCalled();
    }

    private void VerifyJournalGetByIdAsyncCalled(Guid id)
        => JournalRepositoryMock.Verify(repository => repository.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);

    private void VerifyJournalGetByIdAsyncNotCalled()
        => JournalRepositoryMock.Verify(repository => repository.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
}
