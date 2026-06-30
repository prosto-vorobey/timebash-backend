using System.Collections;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

public class JournalAggregateStatisticData : IEnumerable<object[]>
{
    private static readonly long durationSecond = 86_400L;

    public IEnumerator<object[]> GetEnumerator()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var createActivity = (DateTime start, long duration) => StatisticsTestDataFactory.CreateActivity(journalId, start, duration);

        yield return PrependData(AggregationStatisticScenarioBuilder.GetDataWithoutActivities());
        yield return PrependData(AggregationStatisticScenarioBuilder.GetDataWithZeroDurationActivity(createActivity, userId));
        yield return PrependData(AggregationStatisticScenarioBuilder.GetDataWithActivityWithoutCategories(createActivity, durationSecond));
        yield return PrependData(AggregationStatisticScenarioBuilder.GetDataWithSingleActivityAndCategory(createActivity, userId, durationSecond));
        yield return PrependData(AggregationStatisticScenarioBuilder.GetDataWithSingleActivityAndSomeCategories(createActivity, userId, durationSecond));
        yield return PrependData(AggregationStatisticScenarioBuilder.GetDataWithSomeActivitiesAndSingleCategory(createActivity, userId, durationSecond));
        yield return PrependData(AggregationStatisticScenarioBuilder.GetDataWithSomeActivitiesAndCategories(createActivity, userId, durationSecond));

        object[] PrependData((List<Activity> Activities, long ExpectedTime, List<CategoryStatItem> ExpectedStats) data) 
            => [journalId, userId, data.Activities, data.ExpectedTime, data.ExpectedStats];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
