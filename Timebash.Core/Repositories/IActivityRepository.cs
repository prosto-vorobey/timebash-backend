namespace Timebash.Core.Repositories;

public interface IActivityRepository : IRepository<Activity>
{
    Task<IEnumerable<Activity>> GetByJournalIdAsync(Guid journalId, CancellationToken cancellationToken);
    Task<IEnumerable<Activity>> GetByJournalIdAsync(Guid journalId, DateTime startDay, DateTime endDay, CancellationToken cancellationToken);
    Task<IEnumerable<Activity>> GetByIdsAsync(IEnumerable<Guid> activityIds, CancellationToken cancellationToken);
    Task<IEnumerable<Category>> GetCategoriesByActivityIdAsync(Guid activityId, CancellationToken cancellationToken);
    Task<IEnumerable<Guid>> GetCategoryIdsByActivityIdAsync(Guid activityId, CancellationToken cancellationToken);
    Task<bool> IsOwnedByUserAsync(Guid activityId, Guid userId, CancellationToken cancellationToken);
    Task<bool> IsCategoryLinkedAsync(Guid activityId, Guid categoryId, CancellationToken cancellationToken);
    void AddCategoryToActivity(Guid activityId, Guid categoryId);
    void AddCategoriesToActivity(Guid activityId, IEnumerable<Guid> categoryIds);
    Task RemoveCategoryFromActivityAsync(Guid activityId, Guid categoryId, CancellationToken cancellationToken);
    Task ClearActivityCategoriesAsync(Guid activityId, CancellationToken cancellationToken);
    Task<IEnumerable<Activity>> GetOverlappingActivitiesAsync(Guid journalId, DateTime start, DateTime end, CancellationToken cancellationToken);
    Task<Activity?> GetPreviousActivityAsync(Guid journalId, DateTime start, CancellationToken cancellationToken);
    Task<Activity?> GetNextActivityAsync(Guid journalId, DateTime end, CancellationToken cancellationToken);
}
