using Microsoft.EntityFrameworkCore;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Services;

namespace Timebash.Infrastructure.Services;

public class PostgresStatisticsQueryService(TimebashDbContext context) : IStatisticsQueryService
{
    private readonly TimebashDbContext _context = context;

    public async Task<UserAggregateStatisticResponse> GetUserStatisticsAsync(Guid userId, DateTime? start, DateTime? end)
    {
        var query = _context.Activities
            .Where(activity => _context.Journals
                .Any(journal => journal.Id == activity.JournalId && journal.UserId == userId));

        if (start.HasValue)
            query = query.Where(activity => activity.EndTime > start.Value);
        if (end.HasValue)
            query = query.Where(activity => activity.StartTime < end.Value);

        query = query
            .Include(activity => activity.ActivityCategories)
                .ThenInclude(pair => pair.Category);

        var activities = await query.ToListAsync();

        var totalTime = 0L;
        var timeWithoutCategory = 0L;
        var categoryTimes = new Dictionary<Guid, long>();
        foreach (var activity in activities)
        {
            var currentStart = activity.StartTime;
            var currentEnd = activity.EndTime;

            if (start.HasValue && currentStart < start.Value) currentStart = start.Value;
            if (end.HasValue && currentEnd > end.Value) currentEnd = end.Value;
            if (currentStart >= currentEnd) continue;

            var time = (long)(currentEnd - currentStart).TotalSeconds;
            totalTime += time;

            if (activity.ActivityCategories.Count == 0)
            {
                timeWithoutCategory += time;
                continue;
            }

            foreach (var pair in activity.ActivityCategories)
            {
                categoryTimes.TryGetValue(pair.CategoryId, out var current);
                categoryTimes[pair.CategoryId] = current + time;
            }
        }

        var statItems = activities
            .SelectMany(activity => activity.ActivityCategories.Select(pair => pair.Category))
            .Distinct()
            .Select(category => new CategoryStatItem(category.Id, category.Name, categoryTimes.GetValueOrDefault(category.Id, 0)))
            .ToList();
        statItems.Add(new(null, "Uncategorized", timeWithoutCategory));

        return new UserAggregateStatisticResponse(totalTime, statItems);
    }

    public async Task<JournalStatisticResponse> GetJournalStatisticsAsync(Guid journalId, DateTime? start, DateTime? end)
    {
        var query = _context.Activities
            .Where(activity => activity.JournalId == journalId);

        if (start.HasValue)
            query = query.Where(activity => activity.EndTime > start.Value);
        if (end.HasValue)
            query = query.Where(activity => activity.StartTime < end.Value);

        query = query
            .Include(activity => activity.ActivityCategories)
                .ThenInclude(pair => pair.Category);

        var activities = await query.ToListAsync();

        var totalTime = 0L;
        var timeWithoutCategory = 0L;
        var categoryTimes = new Dictionary<Guid, long>();
        foreach (var activity in activities)
        {
            var currentStart = activity.StartTime;
            var currentEnd = activity.EndTime;

            if (start.HasValue && currentStart < start.Value) currentStart = start.Value;
            if (end.HasValue && currentEnd > end.Value) currentEnd = end.Value;
            if (currentStart >= currentEnd) continue;

            var time = (long)(currentEnd - currentStart).TotalSeconds;
            totalTime += time;

            if (activity.ActivityCategories.Count == 0)
            {
                timeWithoutCategory += time;
                continue;
            }

            foreach (var pair in activity.ActivityCategories)
            {
                categoryTimes.TryGetValue(pair.CategoryId, out var current);
                categoryTimes[pair.CategoryId] = current + time;
            }
        }

        var statItems = activities
            .SelectMany(activity => activity.ActivityCategories.Select(pair => pair.Category))
            .Distinct()
            .Select(category => new CategoryStatItem(category.Id, category.Name, categoryTimes.GetValueOrDefault(category.Id, 0)))
            .ToList();
        statItems.Add(new(null, "Uncategorized", timeWithoutCategory));

        return new JournalStatisticResponse(totalTime, statItems);
    }

    public async Task<CategoryStatisticResponse> GetCategoryStatisticsAsync(Guid categoryId, DateTime? start, DateTime? end)
    {
        var query = _context.ActivityCategories
            .Where(pair => pair.CategoryId == categoryId)
            .Select(pair => pair.Activity);

        if (start.HasValue)
            query = query.Where(activity => activity.EndTime > start.Value);
        if (end.HasValue)
            query = query.Where(activity => activity.StartTime < end.Value);

        var activities = await query.ToListAsync();

        long totalTime = 0;
        foreach (var activity in activities)
        {
            var currentStart = activity.StartTime;
            var currentEnd = activity.EndTime;

            if (start.HasValue && currentStart < start.Value) currentStart = start.Value;
            if (end.HasValue && currentEnd > end.Value) currentEnd = end.Value;

            if (currentStart < currentEnd)
                totalTime += (long)(currentEnd - currentStart).TotalSeconds;
        }

        return new CategoryStatisticResponse(totalTime);
    }
}
