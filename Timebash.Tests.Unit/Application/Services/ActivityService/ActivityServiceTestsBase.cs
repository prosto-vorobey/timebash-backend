using Bogus;
using Moq;
using Timebash.Core.Contracts;
using Timebash.Core.Entities;
using Timebash.Core.Repositories;
using Timebash.Core.Services.Access;
using Service = Timebash.Application.Services.ActivityService;

namespace Timebash.Tests.Unit.Application.Services.ActivityService;

public abstract class ActivityServiceTestsBase
{
    public ActivityServiceTestsBase()
    {
        UnitOfWorkMock = new(MockBehavior.Strict);
        ActivityRepositoryMock = new(MockBehavior.Strict);
        CategoryRepositoryMock = new(MockBehavior.Strict);
        JournalRepositoryMock = new(MockBehavior.Strict);
        ActivityAccessServiceMock = new(MockBehavior.Strict);
        JournalAccessServiceMock = new(MockBehavior.Strict);
        CategoryAccessServiceMock = new(MockBehavior.Strict);

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

    protected void SetupGetCategoryIdsByActivityId(Guid id, IEnumerable<Guid> categoryIds)
        => ActivityRepositoryMock
            .Setup(repository => repository.GetCategoryIdsByActivityIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryIds);

    protected void SetupActivityIsCategoryLinked(Guid activityId, Guid categoryId, bool isLinked)
        => ActivityRepositoryMock
            .Setup(repository => repository.IsCategoryLinkedAsync(activityId, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(isLinked);

    protected void SetupAddCategoriesToActivity(Guid activityId, IEnumerable<Guid> categoryIds)
        => ActivityRepositoryMock.Setup(repository => repository.AddCategoriesToActivity(activityId, categoryIds));

    protected void SetupAddCategoriesToActivity(IEnumerable<Guid> categoryIds)
        => ActivityRepositoryMock.Setup(repository => repository.AddCategoriesToActivity(It.IsAny<Guid>(), categoryIds));

    protected void SetupActivityDelete(Activity activity)
        => ActivityRepositoryMock.Setup(repository => repository.Delete(activity));

    protected void SetupClearActivityCategories(Guid id)
        => ActivityRepositoryMock
            .Setup(repository => repository.ClearActivityCategoriesAsync(id, CancellationToken.None))
            .Returns(Task.CompletedTask);

    protected void SetupCategoryGetByIds(IEnumerable<Guid> categoryIds, IEnumerable<Category> categories)
        => CategoryRepositoryMock
            .Setup(repository => repository.GetByIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.OrderBy(id => id).SequenceEqual(categoryIds.OrderBy(id => id))),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

    protected void VerifyGetCategoryIdsByActivityIdCalled(Guid id)
        => ActivityRepositoryMock.Verify(
            repository => repository.GetCategoryIdsByActivityIdAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void VerifyActivityIsCategoryLinkedCalled(Guid activityId, Guid categoryId)
        => ActivityRepositoryMock.Verify(
            repository => repository.IsCategoryLinkedAsync(activityId, categoryId, It.IsAny<CancellationToken>()),
            Times.Once);

    protected void VerifyAddCategoriesToActivityCalled(Guid id, IEnumerable<Guid> categoryIds)
        => ActivityRepositoryMock.Verify(
            repository => repository.AddCategoriesToActivity(id, It.Is<IEnumerable<Guid>>(ids =>
                ids.OrderBy(id => id).SequenceEqual(categoryIds.OrderBy(id => id)))),
            Times.Once);

    protected void VerifyActivityDeleteCalled(Activity activity) => ActivityRepositoryMock.Verify(repository => repository.Delete(activity), Times.Once);

    protected void VerifyClearActivityCategoriesCalled(Guid id)
        => ActivityRepositoryMock.Verify(repository => repository.ClearActivityCategoriesAsync(id, It.IsAny<CancellationToken>()), Times.Once);

    protected void VerifyCategoryGetByIdsCalled(IEnumerable<Guid> categoryIds)
        => CategoryRepositoryMock.Verify(
            repository => repository.GetByIdsAsync(
                It.Is<IEnumerable<Guid>>(ids => ids.OrderBy(id => id).SequenceEqual(categoryIds.OrderBy(id => id))),
                It.IsAny<CancellationToken>()),
            Times.Once);
}
