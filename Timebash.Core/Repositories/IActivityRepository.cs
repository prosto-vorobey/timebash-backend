namespace Timebash.Core.Repositories;

public interface IActivityRepository : IRepository<Activity>
{
    Task<IEnumerable<Activity>> GetByJournalIdAsync(Guid journalId);
    Task<IEnumerable<Activity>> GetByJournalIdAsync(Guid journalId, DateTime startDay, DateTime endDay);
    Task<IEnumerable<Activity>> GetByIdsAsync(IEnumerable<Guid> activityIds);
    Task<IEnumerable<Category>> GetCategoriesByActivityIdAsync(Guid activityId);
    Task<IEnumerable<Guid>> GetCategoryIdsByActivityIdAsync(Guid activityId);
    Task<bool> IsCategoryLinkedAsync(Guid activityId, Guid categoryId);
    void AddCategoryToActivity(Guid activityId, Guid categoryId);
    void AddCategoriesToActivity(Guid activityId, IEnumerable<Guid> categoryIds);
    Task RemoveCategoryFromActivityAsync(Guid activityId, Guid categoryId);
    Task ClearActivityCategoriesAsync(Guid activityId);
    Task<IEnumerable<Activity>> GetOverlappingActivitiesAsync(Guid journalId, DateTime start, DateTime end);
    Task<Activity?> GetPreviousActivityAsync(Guid journalId, DateTime start);
    Task<Activity?> GetNextActivityAsync(Guid journalId, DateTime end);
}
