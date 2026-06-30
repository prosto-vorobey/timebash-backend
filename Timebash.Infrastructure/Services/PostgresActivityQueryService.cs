using Microsoft.EntityFrameworkCore;
using Timebash.Core.Services;

namespace Timebash.Infrastructure.Services;

public class PostgresActivityQueryService(TimebashDbContext context) : IActivityQueryService
{
    private readonly TimebashDbContext _context = context;

    public IAsyncEnumerable<Activity> GetActivitiesForUserAsync(Guid userId, DateTime? start, DateTime? end)
    {
        var result = _context.Activities
            .Where(activity => activity.Journal.UserId == userId);
        
        result = FilterByDateRange(result, start, end);
        result = ApplyIncludes(result);
        
        return result.AsNoTracking().AsAsyncEnumerable();
    }

    public IAsyncEnumerable<Activity> GetActivitiesForJournalAsync(Guid journalId, DateTime? start, DateTime? end)
    {
        var result = _context.Activities
            .Where(activity => activity.JournalId == journalId);

        result = FilterByDateRange(result, start, end);
        result = ApplyIncludes(result);

        return result.AsNoTracking().AsAsyncEnumerable();
    }

    public IAsyncEnumerable<Activity> GetActivitiesForCategoryAsync(Guid categoryId, DateTime? start, DateTime? end)
    {
        var result = _context.ActivityCategories
            .Where(pair => pair.CategoryId == categoryId)
            .Select(pair => pair.Activity);

        result = FilterByDateRange(result, start, end);

        return result.AsNoTracking().AsAsyncEnumerable();
    }

    private static IQueryable<Activity> ApplyIncludes(IQueryable<Activity> query)
        => query
            .Include(a => a.ActivityCategories)
                .ThenInclude(ac => ac.Category);

    private static IQueryable<Activity> FilterByDateRange(IQueryable<Activity> query, DateTime? start, DateTime? end)
    {
        var result = query;
        if (start.HasValue)
            result = result.Where(activity => activity.EndTime > start.Value);
        if (end.HasValue)
            result = result.Where(activity => activity.StartTime < end.Value);

        return result;
    }
}
