using System.Collections;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

public class UserAggregateStatisticData : IEnumerable<object[]>
{
    private static readonly long durationSecond = 86_400L;

    public IEnumerator<object[]> GetEnumerator()
    {
        var userId = Guid.NewGuid();

        yield return [.. GetDataWithoutActivities().Prepend(userId)];
        yield return [.. GetDataWithZeroDurationActivity(userId).Prepend(userId)];
        yield return [.. GetDataWithActivityWithoutCategories().Prepend(userId)];
        yield return [.. GetDataWithSingleActivityAndCategory(userId).Prepend(userId)];
        yield return [.. GetDataWithSingleActivityAndSomeCategories(userId).Prepend(userId)];
        yield return [.. GetDataWithSomeActivitiesAndSingleCategory(userId).Prepend(userId)];
        yield return [.. GetDataWithSomeActivitiesAndCategories(userId).Prepend(userId)];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private static object[] GetDataWithoutActivities()
    {
        var expectedTime = 0L;
        var expectedStats = new List<CategoryStatItem>();

        return [new List<Activity>(), expectedTime, expectedStats];
    }

    private static object[] GetDataWithZeroDurationActivity(Guid userId)
    {
        var activity = ActivityTestDataFactory.GetNewActivity(DateTime.MinValue, 0);
        var category = ActivityTestDataFactory.GetNewCategory(userId);
        ActivityTestDataFactory.AddCategoryToActivity(activity, category);

        var expectedTime = 0;
        var expectedStats = new List<CategoryStatItem>();

        return [new List<Activity> { activity }, expectedTime, expectedStats];
    }

    private static object[] GetDataWithActivityWithoutCategories()
    {
        var activity = ActivityTestDataFactory.GetNewActivity(DateTime.MinValue, durationSecond);

        var expectedTime = durationSecond;
        var expectedStats = new List<CategoryStatItem> { new(null, "Uncategorized", durationSecond) };

        return [new List<Activity> { activity }, expectedTime, expectedStats];
    }

    private static object[] GetDataWithSingleActivityAndCategory(Guid userId)
    {
        var activity = ActivityTestDataFactory.GetNewActivity(DateTime.MinValue, durationSecond);
        var category = ActivityTestDataFactory.GetNewCategory(userId);
        ActivityTestDataFactory.AddCategoryToActivity(activity, category);

        var expectedTime = durationSecond;
        var expectedStats = new List<CategoryStatItem> { new(category.Id, category.Name, expectedTime) };

        return [new List<Activity> { activity }, expectedTime, expectedStats];
    }

    private static object[] GetDataWithSomeActivitiesAndSingleCategory(Guid userId)
    {
        var activities = new List<Activity>
        {
            ActivityTestDataFactory.GetNewActivity(DateTime.MinValue, durationSecond),
            ActivityTestDataFactory.GetNewActivity(new DateTime(2026, 06, 25, 0, 0, 0), durationSecond),
            ActivityTestDataFactory.GetNewActivity(new DateTime(2026, 12, 31, 0, 0, 10), durationSecond)
        };
        var category = ActivityTestDataFactory.GetNewCategory(userId);
        activities.ForEach(activity => ActivityTestDataFactory.AddCategoryToActivity(activity, category));

        var expectedTime = durationSecond * activities.Count;
        var expectedStats = new List<CategoryStatItem> { new(category.Id, category.Name, expectedTime) };

        return [activities, expectedTime, expectedStats];
    }

    private static object[] GetDataWithSingleActivityAndSomeCategories(Guid userId)
    {
        var activity = ActivityTestDataFactory.GetNewActivity(DateTime.MinValue, durationSecond);
        var categories = new List<Category>
        {
            ActivityTestDataFactory.GetNewCategory(userId),
            ActivityTestDataFactory.GetNewCategory(userId),
            ActivityTestDataFactory.GetNewCategory(userId)
        };
        categories.ForEach(category => ActivityTestDataFactory.AddCategoryToActivity(activity, category));

        var expectedTime = durationSecond;
        var expectedStats = categories.Select(category => new CategoryStatItem(category.Id, category.Name, expectedTime)).ToList();

        return [new List<Activity> { activity }, expectedTime, expectedStats];
    }

    private static object[] GetDataWithSomeActivitiesAndCategories(Guid userId)
    {
        var activities = new List<Activity>
        {
            ActivityTestDataFactory.GetNewActivity(DateTime.MinValue, durationSecond),
            ActivityTestDataFactory.GetNewActivity(new DateTime(2026, 06, 25, 0, 0, 0), durationSecond),
            ActivityTestDataFactory.GetNewActivity(new DateTime(2026, 12, 31, 0, 0, 10), durationSecond)
        };
        var categories = new List<Category>
        {
            ActivityTestDataFactory.GetNewCategory(userId),
            ActivityTestDataFactory.GetNewCategory(userId)
        };

        ActivityTestDataFactory.AddCategoryToActivity(activities[0], categories[0]);
        ActivityTestDataFactory.AddCategoryToActivity(activities[1], categories[0]);
        ActivityTestDataFactory.AddCategoryToActivity(activities[1], categories[1]);

        var expectedTime = durationSecond * activities.Count;
        var expectedStats = new List<CategoryStatItem>
        {
            new (categories[0].Id, categories[0].Name, durationSecond * 2),
            new (categories[1].Id, categories[1].Name, durationSecond),
            new (null, "Uncategorized", durationSecond)
        };

        return [activities, expectedTime, expectedStats];
    }


}
