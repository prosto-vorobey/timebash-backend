using Timebash.Application.Helpers;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Repositories;
using Timebash.Core.Services;

namespace Timebash.Application.Services;

public class StatisticService(
    IUserRepository userRepository,
    IJournalRepository journalRepository,
    ICategoryRepository categoryRepository,
    IActivityQueryService queryService) : IStatisticService
{
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IJournalRepository _journalRepository = journalRepository;
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    private readonly IActivityQueryService _queryService = queryService;

    public async Task<UserAggregateStatisticResponse> GetUserAggregateStatisticAsync(Guid userId, DateTime? start, DateTime? end)
    {
        await EntityAccessGuard.ValidateUserExistsAsync(_userRepository, userId);

        var (totalTime, statItems) = await AggregateCategoryStatisticAsync(_queryService.GetActivitiesForUserAsync(userId, start, end), start, end);

        return new UserAggregateStatisticResponse(totalTime, statItems);
    }

    public async Task<JournalAggregateStatisticResponse> GetJournalAggregateStatisticAsync(Guid journalId, DateTime? start, DateTime? end, Guid userId)
    {
        await EntityAccessGuard.ValidateJournalAccessAsync(_journalRepository, journalId, userId);

        var (totalTime, statItems) = await AggregateCategoryStatisticAsync(
            _queryService.GetActivitiesForJournalAsync(journalId, start, end, ActivityDateFilterMode.Overlap), 
            start, 
            end);

        return new JournalAggregateStatisticResponse(totalTime, statItems);
    }

    public async Task<CategoryStatisticResponse> GetCategoryStatisticAsync(Guid categoryId, DateTime? start, DateTime? end, Guid userId)
    {
        await EntityAccessGuard.ValidateCategoryAccessAsync(_categoryRepository, categoryId, userId);
        
        long totalTime = 0;
        await foreach (var activity in _queryService.GetActivitiesForCategoryAsync(categoryId, start, end))
        {
            totalTime += GetIntersectingTimeSeconds(activity, start, end);
        }

        return new CategoryStatisticResponse(totalTime);
    }

    private static async Task<(long TotalTime, List<CategoryStatItem> Items)> AggregateCategoryStatisticAsync(
        IAsyncEnumerable<Activity> activities, 
        DateTime? start,
        DateTime? end)
    {
        var totalTime = 0L;
        var timeWithoutCategory = 0L;
        var categoryTimes = new Dictionary<Guid, long>();
        var categoriesData = new Dictionary<Guid, Category>();

        await foreach (var activity in activities)
        {
            var time = GetIntersectingTimeSeconds(activity, start, end);
            if (time == 0L) continue;

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
                
                categoriesData.TryAdd(pair.CategoryId, pair.Category);
            }
        }

        var statItems = categoryTimes.Keys
            .Select(categoryId => new CategoryStatItem(categoryId, categoriesData[categoryId].Name, categoryTimes[categoryId]))
            .ToList();

        if (timeWithoutCategory > 0)
            statItems.Add(new(null, "Uncategorized", timeWithoutCategory));

        return (totalTime, statItems);
    }

    private static long GetIntersectingTimeSeconds(Activity activity, DateTime? start, DateTime? end)
    {
        var currentStart = activity.StartTime;
        var currentEnd = activity.EndTime;

        if (start.HasValue && currentStart < start.Value) currentStart = start.Value;
        if (end.HasValue && currentEnd > end.Value) currentEnd = end.Value;

        if (currentStart >= currentEnd) return 0L;

        return (long)(currentEnd - currentStart).TotalSeconds;
    }
}
