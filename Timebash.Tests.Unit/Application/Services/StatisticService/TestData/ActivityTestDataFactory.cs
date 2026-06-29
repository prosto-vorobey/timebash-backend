using Bogus;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

internal static class ActivityTestDataFactory
{
    private static readonly Faker _faker = new();

    internal static Activity GetNewActivity(DateTime start, long durationSeconds)
        => new(Guid.NewGuid(), Guid.NewGuid(), start, start.AddSeconds(durationSeconds));

    internal static Activity GetNewActivity(Guid journalId, DateTime start, long durationSeconds)
        => new(Guid.NewGuid(), journalId, start, start.AddSeconds(durationSeconds));

    internal static Category GetNewCategory(Guid userId) => new(Guid.NewGuid(), userId, _faker.Lorem.Word(), "#000000");

    internal static void AddCategoryToActivity(Activity activity, Category category) => activity.ActivityCategories.Add(new()
    {
        ActivityId = activity.Id,
        Activity = activity,
        CategoryId = category.Id,
        Category = category
    });
}
