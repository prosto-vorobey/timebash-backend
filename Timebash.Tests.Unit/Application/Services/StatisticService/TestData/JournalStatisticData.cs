using System.Collections;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

public class JournalStatisticData : IEnumerable<object[]>
{
    private static readonly long durationSecond = 86_400L;

    public IEnumerator<object[]> GetEnumerator()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var createActivity = (DateTime start, long duration) => StatisticsTestDataFactory.CreateActivity(journalId, start, duration);

        yield return PrependData(AggregationScenarioBuilder.GetDataWithoutActivities());
        yield return PrependData(AggregationScenarioBuilder.GetDataWithZeroDurationActivity(createActivity, userId));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithActivityWithoutCategories(createActivity, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSingleActivityAndCategory(createActivity, userId, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSingleActivityAndSomeCategories(createActivity, userId, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSomeActivitiesAndSingleCategory(createActivity, userId, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSomeActivitiesAndCategories(createActivity, userId, durationSecond));

        object[] PrependData((List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) data) 
            => [journalId, userId, data.Activities, data.ExpectedTime, data.ExpectedStats];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
