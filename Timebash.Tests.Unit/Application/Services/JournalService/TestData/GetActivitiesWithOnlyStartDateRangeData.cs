using System.Collections;

namespace Timebash.Tests.Unit.Application.Services.JournalService.TestData;

public class GetActivitiesWithOnlyStartDateRangeData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var journalId = Guid.NewGuid();
        var start = new DateTime(2026, 07, 10, 00, 00, 00);

        yield return [journalId, start, GetActivitiesScenarioBuilder.GetDataWithNoActivities()];
        yield return [journalId, start, GetActivitiesScenarioBuilder.GetDataWithActivitiesExcludedByStartRange(start, journalId)];
        yield return [journalId, start, GetActivitiesScenarioBuilder.GetDataWithActivitiesIncludedByStartRange(start, journalId)];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
