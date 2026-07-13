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

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(activity.JournalId, userId)).ReturnsAsync(true);
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.JournalId)).ReturnsAsync(journal);

        await Service.DeleteAsync(activity.Id, userId);

        journal.UpdatedAt.Should().BeAfter(currentJournalUpdatedTime);

        ActivityRepositoryMock.Verify(repository => repository.Delete(activity), Times.Once);
        UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_EmptyId_ShouldThrowBadRequest()
        => await FluentActions
            .Awaiting(() => Service.DeleteAsync(Guid.Empty, Guid.NewGuid()))
            .Should()
            .ThrowAsync<BadRequestException>();

    [Fact]
    public async Task Delete_ActivityNotFound_ShouldThrowNotFound()
    {
        var activityId = Guid.NewGuid();
        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activityId)).ReturnsAsync((Activity?)null);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(activityId, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_JournalNotLinkedToUser_ShouldThrowNotFound()
    {
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journalId, Guid.NewGuid())).ReturnsAsync(false);
        
        await FluentActions
            .Awaiting(() => Service.DeleteAsync(activity.Id, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Delete_JournalNotFound_ShouldThrowNotFound()
    {
        var journalId = Guid.NewGuid();
        var activity = new Activity(Guid.NewGuid(), journalId, DateTime.MinValue, DateTime.MaxValue);

        ActivityRepositoryMock.Setup(repository => repository.GetByIdAsync(activity.Id)).ReturnsAsync(activity);
        JournalRepositoryMock.Setup(repository => repository.IsUserLinkedAsync(journalId, Guid.NewGuid())).ReturnsAsync(true);
        JournalRepositoryMock.Setup(repository => repository.GetByIdAsync(journalId)).ReturnsAsync((Journal?)null);

        await FluentActions
            .Awaiting(() => Service.DeleteAsync(activity.Id, Guid.NewGuid()))
            .Should()
            .ThrowAsync<NotFoundException>();
    }
}
