using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

internal static class CategoryStatisticScenarioBuilder
{
    internal static (Category Category, List<Activity> Activities, long ExpectedTime) GetDataWithoutActivities()
    {
        var category = StatisticsTestDataFactory.CreateCategory(Guid.NewGuid());
        var expectedTime = 0L;

        return (category, new List<Activity>(), expectedTime);
    }

    internal static (Category Category, List<Activity> Activities, long ExpectedTime) GetDataWithZeroDurationActivity()
    {
        var activity = StatisticsTestDataFactory.CreateActivity(DateTime.MinValue, 0);
        var category = StatisticsTestDataFactory.CreateCategory();
        StatisticsTestDataFactory.AssignCategoryTo(activity, category);

        var expectedTime = 0;

        return (category, new List<Activity>{ activity }, expectedTime);
    }

    internal static (Category Category, List<Activity> Activities, long ExpectedTime) GetDataWithSingleActivity(long durationSecond)
    {
        var activity = StatisticsTestDataFactory.CreateActivity(DateTime.MinValue, durationSecond);
        var category = StatisticsTestDataFactory.CreateCategory();
        StatisticsTestDataFactory.AssignCategoryTo(activity, category);

        var expectedTime = durationSecond;

        return (category, new List<Activity>{ activity }, expectedTime);
    }

    internal static (Category Category, List<Activity> Activities, long ExpectedTime) GetDataWithActivityHavingMultipleCategories(long durationSecond)
    {
        var activity = StatisticsTestDataFactory.CreateActivity(DateTime.MinValue, durationSecond);
        var category = StatisticsTestDataFactory.CreateCategory();
        var otherCategory = StatisticsTestDataFactory.CreateCategory();
        StatisticsTestDataFactory.AssignCategoryTo(activity, category);
        StatisticsTestDataFactory.AssignCategoryTo(activity, otherCategory);

        var expectedTime = durationSecond;

        return (category, new List<Activity> { activity }, expectedTime);
    }

    internal static (Category Category, List<Activity> Activities, long ExpectedTime) GetDataWithSomeActivities(long durationSecond)
    {
        var activities = new List<Activity>
        {
            StatisticsTestDataFactory.CreateActivity(DateTime.MinValue, durationSecond),
            StatisticsTestDataFactory.CreateActivity(new DateTime(2026, 06, 25, 0, 0, 0), durationSecond),
            StatisticsTestDataFactory.CreateActivity(new DateTime(2026, 12, 31, 0, 0, 10), durationSecond)
        };
        var category = StatisticsTestDataFactory.CreateCategory();
        activities.ForEach(activity => StatisticsTestDataFactory.AssignCategoryTo(activity, category));

        var expectedTime = durationSecond * activities.Count;

        return (category, activities, expectedTime);
    }

    internal static (List<Activity> Activities, long ExpectedTime) GetDataWithStartDate(Category category, DateTime startDate, long durationSecond)
    {
        var activities = new List<Activity>
        {
            StatisticsTestDataFactory.CreateActivity(startDate.AddSeconds(-durationSecond), durationSecond / 2),
            StatisticsTestDataFactory.CreateActivity(startDate.AddSeconds(-durationSecond), durationSecond),
            StatisticsTestDataFactory.CreateActivity(startDate.AddSeconds(-durationSecond), durationSecond * 2),
            StatisticsTestDataFactory.CreateActivity(startDate.AddSeconds(durationSecond), durationSecond),
        };
        activities.ForEach(activity => StatisticsTestDataFactory.AssignCategoryTo(activity, category));

        var expectedTime = durationSecond * 2;

        return (activities, expectedTime);
    }

    internal static (List<Activity> Activities, long ExpectedTime) GetDataWithEndDate(Category category, DateTime endDate, long durationSecond)
    {
        var activities = new List<Activity>
        {
            StatisticsTestDataFactory.CreateActivity(endDate.AddSeconds(durationSecond / 2), durationSecond / 2),
            StatisticsTestDataFactory.CreateActivity(endDate, durationSecond),
            StatisticsTestDataFactory.CreateActivity(endDate.AddSeconds(-durationSecond), durationSecond * 2),
            StatisticsTestDataFactory.CreateActivity(endDate.AddSeconds(-durationSecond * 2), durationSecond),
        };
        activities.ForEach(activity => StatisticsTestDataFactory.AssignCategoryTo(activity, category));

        var expectedTime = durationSecond * 2;

        return (activities, expectedTime);
    }

    internal static (List<Activity> Activities, long ExpectedTime) GetDataWithStartAndEndDate(
        Category category, 
        DateTime startDate, 
        DateTime endDate,
        long durationSecond)
    {
        var activities = new List<Activity>
        {
            StatisticsTestDataFactory.CreateActivity(startDate.AddSeconds(-durationSecond), durationSecond / 2),
            StatisticsTestDataFactory.CreateActivity(startDate.AddSeconds(-durationSecond), durationSecond),
            StatisticsTestDataFactory.CreateActivity(startDate.AddSeconds(-durationSecond), durationSecond * 2),
            StatisticsTestDataFactory.CreateActivity(startDate.AddSeconds(durationSecond), durationSecond),
            StatisticsTestDataFactory.CreateActivity(endDate.AddSeconds(-durationSecond * 2), durationSecond),
            StatisticsTestDataFactory.CreateActivity(endDate.AddSeconds(-durationSecond), durationSecond * 2),
            StatisticsTestDataFactory.CreateActivity(endDate, durationSecond),
            StatisticsTestDataFactory.CreateActivity(endDate.AddSeconds(durationSecond / 2), durationSecond / 2),
        };
        activities.ForEach(activity => StatisticsTestDataFactory.AssignCategoryTo(activity, category));

        var expectedTime = durationSecond * 4;

        return (activities, expectedTime);
    }
} 
