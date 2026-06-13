using Microsoft.EntityFrameworkCore;

namespace Timebash.Infrastructure.Repositories;

public class PostgresActivityRepository(TimebashDbContext context) : PostgresRepositoryBase<Activity>(context), IActivityRepository
{
    public async Task<IEnumerable<Activity>> GetByJournalIdAsync(Guid journalId)
        => await Context.Activities
            .Where(activity => activity.JournalId == journalId)
            .ToListAsync();

    public async Task<IEnumerable<Activity>> GetByJournalIdAsync(Guid journalId, DateTime startDay, DateTime endDay)
        => await Context.Activities
            .Where(activity => activity.JournalId == journalId && activity.StartTime >= startDay && activity.StartTime < endDay)
            .ToListAsync();

    public async Task<IEnumerable<Activity>> GetByIdsAsync(IEnumerable<Guid> activityIds)
    {
        if (activityIds == null || !activityIds.Any()) return [];

        return await Context.Activities
            .Where(activity => activityIds.Contains(activity.Id))
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetCategoriesByActivityIdAsync(Guid activityId)
        => await Context.ActivityCategories
            .Where(pair => pair.ActivityId == activityId)
            .Select(pair => pair.Category)
            .ToListAsync();

    public async Task<IEnumerable<Guid>> GetCategoryIdsByActivityIdAsync(Guid activityId)
        => await Context.ActivityCategories.Where(pair => pair.ActivityId == activityId)
            .Select(pair => pair.CategoryId)
            .ToListAsync();

    public Task<bool> IsCategoryLinkedAsync(Guid activityId, Guid categoryId)
        => Context.ActivityCategories.AnyAsync(pair =>
            pair.ActivityId == activityId &&
            pair.CategoryId == categoryId);

    public void AddCategoryToActivity(Guid activityId, Guid categoryId)
        => Context.ActivityCategories.Add(new ActivityCategory { ActivityId = activityId, CategoryId = categoryId });

    public void AddCategoriesToActivity(Guid activityId, IEnumerable<Guid> categoryIds)
        => Context.ActivityCategories.AddRange(categoryIds.Select(categoryId =>
                new ActivityCategory
                {
                    ActivityId = activityId,
                    CategoryId = categoryId
                }));

    public async Task RemoveCategoryFromActivityAsync(Guid activityId, Guid categoryId)
    {
        var activityCategory = await Context.ActivityCategories.FindAsync(activityId, categoryId);
        if (activityCategory is not null) Context.ActivityCategories.Remove(activityCategory);
    }

    public async Task ClearActivityCategoriesAsync(Guid activityId)
        => Context.ActivityCategories.RemoveRange(await Context.ActivityCategories
            .Where(pair => pair.ActivityId == activityId)
            .ToListAsync());

    public async Task<IEnumerable<Activity>> GetOverlappingActivitiesAsync(Guid journalId, DateTime start, DateTime end)
        => await Context.Activities.Where(activity => activity.JournalId == journalId 
                && ((start >= activity.StartTime && start < activity.EndTime) 
                    || (end > activity.StartTime && end <= activity.EndTime)
                    || (start < activity.StartTime && end > activity.EndTime)))
            .OrderBy(activity => activity.StartTime)
            .ToListAsync();

    public async Task<Activity?> GetPreviousActivityAsync(Guid journalId, DateTime start)
        => await Context.Activities.Where(activity => activity.JournalId == journalId && activity.EndTime <= start)
            .OrderByDescending(activity => activity.StartTime)
            .FirstOrDefaultAsync();

    public async Task<Activity?> GetNextActivityAsync(Guid journalId, DateTime end)
        => await Context.Activities.Where(activity => activity.JournalId == journalId && activity.StartTime >= end)
            .OrderBy(activity => activity.StartTime)
            .FirstOrDefaultAsync();
}
