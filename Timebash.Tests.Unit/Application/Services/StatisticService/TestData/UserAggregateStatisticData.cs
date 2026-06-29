using System.Collections;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

public class UserAggregateStatisticData : IEnumerable<object[]>
{
    private static readonly long durationSecond = 86_400L;

    public IEnumerator<object[]> GetEnumerator()
    {
        var userId = Guid.NewGuid();
        Func<DateTime, long, Activity> createActivity = StatisticsTestDataFactory.CreateActivity;

        yield return PrependData(AggregationScenarioBuilder.GetDataWithoutActivities());
        yield return PrependData(AggregationScenarioBuilder.GetDataWithZeroDurationActivity(createActivity, userId));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithActivityWithoutCategories(createActivity, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSingleActivityAndCategory(createActivity, userId, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSingleActivityAndSomeCategories(createActivity, userId, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSomeActivitiesAndSingleCategory(createActivity, userId, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSomeActivitiesAndCategories(createActivity, userId, durationSecond));

        object[] PrependData(object[] data) => [.. data.Prepend(userId)];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
