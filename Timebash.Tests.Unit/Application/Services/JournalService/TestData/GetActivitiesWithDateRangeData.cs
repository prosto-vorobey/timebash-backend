using System.Collections;

namespace Timebash.Tests.Unit.Application.Services.JournalService.TestData;

public class GetActivitiesWithDateRangeData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var journalId = Guid.NewGuid();
        var start = new DateTime(2026, 07, 10, 00, 00, 00);
        var end = new DateTime(2026, 07, 11, 00, 00, 00);

        yield return [journalId, start, end, GetActivitiesScenarioBuilder.GetDataWithNoActivities()];
        yield return [journalId, start, end, GetActivitiesScenarioBuilder.GetDataWithActivitiesExcludedByRange(start, end, journalId)];
        yield return [journalId, start, end, GetActivitiesScenarioBuilder.GetDataWithActivitiesIncludedByRange(start, end, journalId)];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
