using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

internal static class AggregationScenarioBuilder
{
    internal static (List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) GetDataWithoutActivities()
    {
        var expectedTime = 0L;
        var expectedStats = new List<CategoryStatItem>();

        return (new List<Activity>(), expectedTime, expectedStats);
    }

    internal static (List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) GetDataWithZeroDurationActivity(
        Func<DateTime, long, Activity> createActivity,
        Guid userId)
    {
        var activity = createActivity(DateTime.MinValue, 0);
        var category = StatisticsTestDataFactory.CreateCategory(userId);
        StatisticsTestDataFactory.AssignCategoryTo(activity, category);

        var expectedTime = 0;
        var expectedStats = new List<CategoryStatItem>();

        return (new List<Activity>(), expectedTime, expectedStats);
    }

    internal static (List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) GetDataWithActivityWithoutCategories(
        Func<DateTime, long, Activity> createActivity,
        long durationSecond)
    {
        var activity = createActivity(DateTime.MinValue, durationSecond);

        var expectedTime = durationSecond;
        var expectedStats = new List<CategoryStatItem> { new(null, "Uncategorized", durationSecond) };

        return (new List<Activity>(), expectedTime, expectedStats);
    }

    internal static (List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) GetDataWithSingleActivityAndCategory(
        Func<DateTime, long, Activity> createActivity,
        Guid userId,
        long durationSecond)
    {
        var activity = createActivity(DateTime.MinValue, durationSecond);
        var category = StatisticsTestDataFactory.CreateCategory(userId);
        StatisticsTestDataFactory.AssignCategoryTo(activity, category);

        var expectedTime = durationSecond;
        var expectedStats = new List<CategoryStatItem> { new(category.Id, category.Name, expectedTime) };

        return (new List<Activity>(), expectedTime, expectedStats);
    }

    internal static (List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) GetDataWithSingleActivityAndSomeCategories(
        Func<DateTime, long, Activity> createActivity,
        Guid userId,
        long durationSecond)
    {
        var activity = createActivity(DateTime.MinValue, durationSecond);
        var categories = new List<Category>
        {
            StatisticsTestDataFactory.CreateCategory(userId),
            StatisticsTestDataFactory.CreateCategory(userId),
            StatisticsTestDataFactory.CreateCategory(userId)
        };
        categories.ForEach(category => StatisticsTestDataFactory.AssignCategoryTo(activity, category));

        var expectedTime = durationSecond;
        var expectedStats = categories.Select(category => new CategoryStatItem(category.Id, category.Name, expectedTime)).ToList();

        return (new List<Activity>(), expectedTime, expectedStats);
    }

    internal static (List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) GetDataWithSomeActivitiesAndSingleCategory(
        Func<DateTime, long, Activity> createActivity,
        Guid userId, long durationSecond)
    {
        var activities = new List<Activity>
        {
            createActivity(DateTime.MinValue, durationSecond),
            createActivity(new DateTime(2026, 06, 25, 0, 0, 0), durationSecond),
            createActivity(new DateTime(2026, 12, 31, 0, 0, 10), durationSecond)
        };
        var category = StatisticsTestDataFactory.CreateCategory(userId);
        activities.ForEach(activity => StatisticsTestDataFactory.AssignCategoryTo(activity, category));

        var expectedTime = durationSecond * activities.Count;
        var expectedStats = new List<CategoryStatItem> { new(category.Id, category.Name, expectedTime) };

        return (activities, expectedTime, expectedStats);
    }

    internal static (List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) GetDataWithSomeActivitiesAndCategories(
        Func<DateTime, long, Activity> createActivity,
        Guid userId,
        long durationSecond)
    {
        var activities = new List<Activity>
        {
            createActivity(DateTime.MinValue, durationSecond),
            createActivity(new DateTime(2026, 06, 25, 0, 0, 0), durationSecond),
            createActivity(new DateTime(2026, 12, 31, 0, 0, 10), durationSecond)
        };
        var categories = new List<Category>
        {
            StatisticsTestDataFactory.CreateCategory(userId),
            StatisticsTestDataFactory.CreateCategory(userId)
        };

        StatisticsTestDataFactory.AssignCategoryTo(activities[0], categories[0]);
        StatisticsTestDataFactory.AssignCategoryTo(activities[1], categories[0]);
        StatisticsTestDataFactory.AssignCategoryTo(activities[1], categories[1]);

        var expectedTime = durationSecond * activities.Count;
        var expectedStats = new List<CategoryStatItem>
        {
            new(categories[0].Id, categories[0].Name, durationSecond * 2),
            new(categories[1].Id, categories[1].Name, durationSecond),
            new(null, "Uncategorized", durationSecond)
        };

        return (activities, expectedTime, expectedStats);
    }

    internal static (List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) GetDataWithStartDate(
        Func<DateTime, long, Activity> createActivity,
        Guid userId,
        DateTime startDate,
        long durationSecond)
    {
        var activities = new List<Activity>
        {
            createActivity(startDate.AddSeconds(-durationSecond), durationSecond / 2),
            createActivity(startDate.AddSeconds(-durationSecond), durationSecond),
            createActivity(startDate.AddSeconds(-durationSecond), durationSecond * 2),
            createActivity(startDate.AddSeconds(durationSecond), durationSecond),
        };
        var categories = new List<Category>
        {
            StatisticsTestDataFactory.CreateCategory(userId),
            StatisticsTestDataFactory.CreateCategory(userId),
        };
        StatisticsTestDataFactory.AssignCategoryTo(activities[0], categories[0]);
        StatisticsTestDataFactory.AssignCategoryTo(activities[1], categories[0]);
        StatisticsTestDataFactory.AssignCategoryTo(activities[2], categories[1]);
        StatisticsTestDataFactory.AssignCategoryTo(activities[3], categories[1]);

        var expectedTime = durationSecond * 2;
        var expectedStats = new List<CategoryStatItem> { new(categories[1].Id, categories[1].Name, durationSecond * 2) };

        return (activities, expectedTime, expectedStats);
    }

    internal static (List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) GetDataWithEndDate(
        Func<DateTime, long, Activity> createActivity,
        Guid userId,
        DateTime endDate,
        long durationSecond)
    {
        var activities = new List<Activity>
        {
            createActivity(endDate.AddSeconds(durationSecond / 2), durationSecond / 2),
            createActivity(endDate, durationSecond),
            createActivity(endDate.AddSeconds(-durationSecond), durationSecond * 2),
            createActivity(endDate.AddSeconds(-durationSecond * 2), durationSecond),
        };
        var categories = new List<Category>
        {
            StatisticsTestDataFactory.CreateCategory(userId),
            StatisticsTestDataFactory.CreateCategory(userId),
        };
        StatisticsTestDataFactory.AssignCategoryTo(activities[0], categories[0]);
        StatisticsTestDataFactory.AssignCategoryTo(activities[1], categories[0]);
        StatisticsTestDataFactory.AssignCategoryTo(activities[2], categories[1]);
        StatisticsTestDataFactory.AssignCategoryTo(activities[3], categories[1]);

        var expectedTime = durationSecond * 2;
        var expectedStats = new List<CategoryStatItem> { new(categories[1].Id, categories[1].Name, durationSecond * 2) };

        return (activities, expectedTime, expectedStats);
    }

    internal static (List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) GetDataWithStartAndEndDate(
        Func<DateTime, long, Activity> createActivity,
        Guid userId,
        DateTime startDate,
        DateTime endDate,
        long durationSecond)
    {
        var activities = new List<Activity>
        {
            createActivity(startDate.AddSeconds(-durationSecond), durationSecond / 2),
            createActivity(startDate.AddSeconds(-durationSecond), durationSecond),
            createActivity(startDate.AddSeconds(-durationSecond), durationSecond * 2),
            createActivity(startDate.AddSeconds(durationSecond), durationSecond),
            createActivity(endDate.AddSeconds(-durationSecond * 2), durationSecond),
            createActivity(endDate.AddSeconds(-durationSecond), durationSecond * 2),
            createActivity(endDate, durationSecond),
            createActivity(endDate.AddSeconds(durationSecond / 2), durationSecond / 2),
        };
        var categories = new List<Category>
        {
            StatisticsTestDataFactory.CreateCategory(userId),
            StatisticsTestDataFactory.CreateCategory(userId),
        };
        StatisticsTestDataFactory.AssignCategoryTo(activities[0], categories[0]);
        StatisticsTestDataFactory.AssignCategoryTo(activities[1], categories[0]);
        StatisticsTestDataFactory.AssignCategoryTo(activities[2], categories[1]);
        StatisticsTestDataFactory.AssignCategoryTo(activities[3], categories[1]);

        var expectedTime = durationSecond * 4;
        var expectedStats = new List<CategoryStatItem>
        {
            new(categories[1].Id, categories[1].Name, durationSecond * 2),
            new(null, "Uncategorized", durationSecond * 2),
        };

        return (activities, expectedTime, expectedStats);
    }
}
