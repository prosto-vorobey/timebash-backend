using FluentAssertions;
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

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        JournalRepositoryMock.SetupGetById(journal);
        SetupActivityDelete(activity);
        UnitOfWorkMock.SetupSaveChanges();

        await Service.DeleteAsync(activity.Id, userId, CancellationToken.None);

        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        JournalRepositoryMock.VerifyGetByIdCalled(activity.JournalId);
        VerifyActivityDeleteCalled(activity);
        UnitOfWorkMock.VerifySaveChangesCalled();
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
    {
        var id = Guid.Empty;
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock.SetupEnsureAccessThrowsBadRequest(id, userId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task Delete_ActivityNotFound_ShouldThrowNotFound()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        ActivityAccessServiceMock.SetupEnsureAccessThrowsNotFound(id, userId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(id, userId);
    }

    [Fact]
    public async Task Delete_JournalNotFound_ShouldThrowNotFound()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);

        ActivityAccessServiceMock.SetupEnsureAccess(activity, userId);
        JournalRepositoryMock.SetupGetById(journalId);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(activity.Id, userId, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>();

        ActivityAccessServiceMock.VerifyEnsureAccessCalled(activity.Id, userId);
        JournalRepositoryMock.VerifyGetByIdCalled(journalId);
    }

    
}
