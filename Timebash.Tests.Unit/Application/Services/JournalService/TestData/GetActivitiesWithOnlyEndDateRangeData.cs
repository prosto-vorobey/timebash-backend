using System.Collections;

namespace Timebash.Tests.Unit.Application.Services.JournalService.TestData;

public class GetActivitiesWithOnlyEndDateRangeData : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        var journalId = Guid.NewGuid();
        var end = new DateTime(2026, 07, 10, 00, 00, 00);

        yield return [journalId, end, GetActivitiesScenarioBuilder.GetDataWithNoActivities()];
        yield return [journalId, end, GetActivitiesScenarioBuilder.GetDataWithActivitiesExcludedByEndRange(end, journalId)];
        yield return [journalId, end, GetActivitiesScenarioBuilder.GetDataWithActivitiesIncludedByEndRange(end, journalId)];
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
