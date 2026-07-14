using Microsoft.EntityFrameworkCore;

namespace Timebash.Infrastructure.Repositories;

public class PostgresActivityRepository(TimebashDbContext context) : PostgresRepositoryBase<Activity>(context), IActivityRepository
{
    public async Task<IEnumerable<Activity>> GetByJournalIdAsync(Guid journalId, CancellationToken cancellationToken)
        => await Context.Activities
            .Where(activity => activity.JournalId == journalId)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Activity>> GetByJournalIdAsync(
        Guid journalId, 
        DateTime startDay, 
        DateTime endDay, 
        CancellationToken cancellationToken)
        => await Context.Activities
            .Where(activity => activity.JournalId == journalId && activity.StartTime >= startDay && activity.StartTime < endDay)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Activity>> GetByIdsAsync(IEnumerable<Guid> activityIds, CancellationToken cancellationToken)
    {
        if (activityIds == null || !activityIds.Any()) return [];

        return await Context.Activities
            .Where(activity => activityIds.Contains(activity.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Category>> GetCategoriesByActivityIdAsync(Guid activityId, CancellationToken cancellationToken)
        => await Context.ActivityCategories
            .Where(pair => pair.ActivityId == activityId)
            .Select(pair => pair.Category)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<Guid>> GetCategoryIdsByActivityIdAsync(Guid activityId, CancellationToken cancellationToken)
        => await Context.ActivityCategories.Where(pair => pair.ActivityId == activityId)
            .Select(pair => pair.CategoryId)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsOwnedByUserAsync(Guid activityId, Guid userId, CancellationToken cancellationToken)
        => await Context.Activities.AnyAsync(activity => activity.Id == activityId && activity.Journal.UserId == userId, cancellationToken);

    public Task<bool> IsCategoryLinkedAsync(Guid activityId, Guid categoryId, CancellationToken cancellationToken)
        => Context.ActivityCategories.AnyAsync(pair =>
            pair.ActivityId == activityId &&
            pair.CategoryId == categoryId, cancellationToken);

    public void AddCategoryToActivity(Guid activityId, Guid categoryId)
        => Context.ActivityCategories.Add(new ActivityCategory { ActivityId = activityId, CategoryId = categoryId });

    public void AddCategoriesToActivity(Guid activityId, IEnumerable<Guid> categoryIds)
        => Context.ActivityCategories.AddRange(categoryIds.Select(categoryId =>
            new ActivityCategory
            {
                ActivityId = activityId,
                CategoryId = categoryId
            }));

    public async Task RemoveCategoryFromActivityAsync(Guid activityId, Guid categoryId, CancellationToken cancellationToken)
    {
        var activityCategory = await Context.ActivityCategories.FindAsync([activityId, categoryId], cancellationToken);
        if (activityCategory is not null) Context.ActivityCategories.Remove(activityCategory);
    }

    public async Task ClearActivityCategoriesAsync(Guid activityId, CancellationToken cancellationToken)
        => Context.ActivityCategories.RemoveRange(await Context.ActivityCategories
            .Where(pair => pair.ActivityId == activityId)
            .ToListAsync(cancellationToken));

    public async Task<IEnumerable<Activity>> GetOverlappingActivitiesAsync(
        Guid journalId, 
        DateTime start, 
        DateTime end, 
        CancellationToken cancellationToken)
        => await Context.Activities.Where(activity => activity.JournalId == journalId 
                && ((start >= activity.StartTime && start < activity.EndTime) 
                    || (end > activity.StartTime && end <= activity.EndTime)
                    || (start < activity.StartTime && end > activity.EndTime)))
            .OrderBy(activity => activity.StartTime)
            .ToListAsync(cancellationToken);

    public async Task<Activity?> GetPreviousActivityAsync(Guid journalId, DateTime start, CancellationToken cancellationToken)
        => await Context.Activities.Where(activity => activity.JournalId == journalId && activity.EndTime <= start)
            .OrderByDescending(activity => activity.StartTime)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Activity?> GetNextActivityAsync(Guid journalId, DateTime end, CancellationToken cancellationToken)
        => await Context.Activities.Where(activity => activity.JournalId == journalId && activity.StartTime >= end)
            .OrderBy(activity => activity.StartTime)
            .FirstOrDefaultAsync(cancellationToken);
}
