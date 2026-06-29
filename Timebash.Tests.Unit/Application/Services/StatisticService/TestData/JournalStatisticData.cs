using System.Collections;

namespace Timebash.Tests.Unit.Application.Services.StatisticService.TestData;

public class JournalStatisticData : IEnumerable<object[]>
{
    private static readonly long durationSecond = 86_400L;

    public IEnumerator<object[]> GetEnumerator()
    {
        var userId = Guid.NewGuid();
        var journalId = Guid.NewGuid();
        var createActivity = (DateTime start, long duration) => ActivityTestDataFactory.GetNewActivity(journalId, start, duration);

        yield return PrependData(AggregationScenarioBuilder.GetDataWithoutActivities());
        yield return PrependData(AggregationScenarioBuilder.GetDataWithZeroDurationActivity(createActivity, userId));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithActivityWithoutCategories(createActivity, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSingleActivityAndCategory(createActivity, userId, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSingleActivityAndSomeCategories(createActivity, userId, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSomeActivitiesAndSingleCategory(createActivity, userId, durationSecond));
        yield return PrependData(AggregationScenarioBuilder.GetDataWithSomeActivitiesAndCategories(createActivity, userId, durationSecond));

        object[] PrependData(object[] data) => [.. data.Prepend(userId).Prepend(journalId)];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
