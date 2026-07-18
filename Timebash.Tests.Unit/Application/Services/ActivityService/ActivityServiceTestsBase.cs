using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Entities;
using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services.Access;
using Service = Timebash.Application.Services.ActivityService;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public abstract partial class ActivityServiceTestsBase
{
    public ActivityServiceTestsBase()
    {
        UnitOfWorkMock = new();
        ActivityRepositoryMock = new();
        CategoryRepositoryMock = new();
        JournalRepositoryMock = new();
        ActivityAccessServiceMock = new();
        JournalAccessServiceMock = new();
        CategoryAccessServiceMock = new();

        Service = new(
            UnitOfWorkMock.Object,
            ActivityRepositoryMock.Object,
            CategoryRepositoryMock.Object,
            JournalRepositoryMock.Object,
            ActivityAccessServiceMock.Object,
            JournalAccessServiceMock.Object,
            CategoryAccessServiceMock.Object);
    }

    protected static Faker Faker { get; } = new();
    protected Service Service { get; }
    protected Mock<IUnitOfWork> UnitOfWorkMock { get; }
    protected Mock<IActivityRepository> ActivityRepositoryMock { get; }
    protected Mock<ICategoryRepository> CategoryRepositoryMock { get; }
    protected Mock<IJournalRepository> JournalRepositoryMock { get; }
    protected Mock<IActivityAccessService> ActivityAccessServiceMock { get; }
    protected Mock<IJournalAccessService> JournalAccessServiceMock { get; }
    protected Mock<ICategoryAccessService> CategoryAccessServiceMock { get; }

    protected static List<Guid> GetClearedCategoryIds(List<Guid> categoryIds) => [.. categoryIds.Where(id => id != Guid.Empty).Distinct()];

    protected void SetupActivityEnsureAccess(Activity activity, Guid userId)
        => ActivityAccessServiceMock
            .Setup(service => service.EnsureAccessAsync(activity.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activity);

    protected void SetupActivityEnsureAccessThrowsBadRequest(Guid id, Guid userId)
        => ActivityAccessServiceMock
            .Setup(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    protected void SetupActivityEnsureAccessThrowsNotFound(Guid id, Guid userId)
        => ActivityAccessServiceMock
            .Setup(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    protected void SetupJournalEnsureAccess(Journal journal)
        => JournalAccessServiceMock
            .Setup(service => service.EnsureAccessAsync(journal.Id, journal.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(journal);

    protected void SetupJournalEnsureAccessThrowsBadRequest(Guid id, Guid userId)
        => JournalAccessServiceMock
            .Setup(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    protected void SetupJournalEnsureAccessThrowsNotFound(Guid id, Guid userId)
        => JournalAccessServiceMock
            .Setup(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    protected void SetupCategoryValidateAccess(Guid id, Guid userId)
        => CategoryAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

    protected void SetupCategoryValidateAccessThrowsBadRequest(Guid id, Guid userId)
        => CategoryAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new BadRequestException());

    protected void SetupCategoryValidateAccessThrowsNotFound(Guid id, Guid userId)
        => CategoryAccessServiceMock
            .Setup(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException());

    protected void SetupActivityGetCategoryIdsByActivityId(Guid id, IEnumerable<Guid> categoryIds)
    => ActivityRepositoryMock
        .Setup(repository => repository.GetCategoryIdsByActivityIdAsync(id, It.IsAny<CancellationToken>()))
        .ReturnsAsync(categoryIds);

    protected void SetupCategoryGetByIds(IEnumerable<Guid> categoryIds, IEnumerable<Category> categories)
        => CategoryRepositoryMock
            .Setup(repository => repository.GetByIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.OrderBy(id => id).SequenceEqual(categoryIds.OrderBy(id => id))),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

    protected void SetupActivityIsCategoryLinked(Guid activityId, Guid categoryId, bool isLinked)
        => ActivityRepositoryMock
            .Setup(repository => repository.IsCategoryLinkedAsync(activityId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(isLinked);

    protected void VerifyActivityEnsureAccessCalled(Guid id, Guid userId)
        => ActivityAccessServiceMock.Verify(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifyJournalEnsureAccessCalled(Guid id, Guid userId)
        => JournalAccessServiceMock.Verify(service => service.EnsureAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifyCategoryValidateAccessCalled(Guid id, Guid userId)
        => CategoryAccessServiceMock.Verify(service => service.ValidateAccessAsync(id, userId, It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifyActivityGetCategoryIdsByActivityIdCalled(Guid id)
        => ActivityRepositoryMock.Verify(
            repository => repository.GetCategoryIdsByActivityIdAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void VerifyCategoryGetByIdsCalled(IEnumerable<Guid> categoryIds)
        => CategoryRepositoryMock.Verify(
            repository => repository.GetByIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.OrderBy(id => id).SequenceEqual(categoryIds.OrderBy(id => id))),
                It.IsAny<CancellationToken>()),
            Times.Once);

    protected void VerifyActivityAddCategoriesToActivityCalled(Guid id, IEnumerable<Guid> categoryIds)
        => ActivityRepositoryMock.Verify(
            repository => repository.AddCategoriesToActivity(id, It.Is<IEnumerable<Guid>>(ids =>
                ids.OrderBy(id => id).SequenceEqual(categoryIds.OrderBy(id => id)))),
            Times.Once);

    protected void VerifyClearActivityCategoriesCalled(Guid id)
        => ActivityRepositoryMock.Verify(repository => repository.ClearActivityCategoriesAsync(id, It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifyActivityIsCategoryLinkedCalled(Guid activityId, Guid categoryId)
        => ActivityRepositoryMock.Verify(
            repository => repository.IsCategoryLinkedAsync(activityId, categoryId, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void VerifyActivityDeleteCalled(Activity activity) => ActivityRepositoryMock.Verify(repository => repository.Delete(activity), Times.Once);

    protected void VerifySaveChangesCalled()
        => UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifyCategoryValidateAccessNotCalled()
        => CategoryAccessServiceMock.Verify(
            service => service.ValidateAccessAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void VerifyActivityGetCategoryIdsByActivityIdNotCalled()
        => ActivityRepositoryMock.Verify(
            repository => repository.GetCategoryIdsByActivityIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void VerifyCategoryGetByIdsNotCalled()
        => CategoryRepositoryMock.Verify(
            repository => repository.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void VerifyActivityAddCategoriesToActivityNotCalled()
        => ActivityRepositoryMock.Verify(
            repository => repository.AddCategoriesToActivity(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>()),
            Times.Never);

    protected void VerifyClearActivityCategoriesNotCalled()
        => ActivityRepositoryMock.Verify(
            repository => repository.ClearActivityCategoriesAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void VerifyActivityIsCategoryLinkedNotCalled()
        => ActivityRepositoryMock.Verify(
            repository => repository.IsCategoryLinkedAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);

    protected void VerifyActivityDeleteNotCalled() => ActivityRepositoryMock.Verify(repository => repository.Delete(It.IsAny<Activity>()), Times.Never);

    protected void VerifySaveChangesNotCalled()
        => UnitOfWorkMock.Verify(unit => unit.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
}
