using Timebash.Core.Entities;

namespace Timebash.Tests.Unit.Application.Services.JournalService.TestData;

internal static class GetActivitiesScenarioBuilder
{
    internal static List<Activity> GetDataWithNoActivities() => [];

    internal static List<Activity> GetDataWithActivitiesExcludedByStartRange(DateTime startRange, Guid journalId)
    {
        var timeBeforeStart = startRange.AddSeconds(-1);

        return
        [
            new(Guid.NewGuid(), journalId, timeBeforeStart, timeBeforeStart),
            new(Guid.NewGuid(), journalId, timeBeforeStart, startRange.AddSeconds(1)),
        ];
    }

    internal static List<Activity> GetDataWithActivitiesIncludedByStartRange(DateTime startRange, Guid journalId)
        =>
        [
            new(Guid.NewGuid(), journalId, startRange, DateTime.MaxValue),
            new(Guid.NewGuid(), journalId, startRange.AddSeconds(1), DateTime.MaxValue),
        ];

    internal static List<Activity> GetDataWithActivitiesExcludedByEndRange(DateTime endRange, Guid journalId)
    {
        var timeAfterEnd = endRange.AddSeconds(1);

        return
        [
            new(Guid.NewGuid(), journalId, timeAfterEnd, timeAfterEnd),
        ];
    }

    internal static List<Activity> GetDataWithActivitiesIncludedByEndRange(DateTime endRange, Guid journalId)
        =>
        [
            new(Guid.NewGuid(), journalId, endRange.AddSeconds(-1), endRange),
            new(Guid.NewGuid(), journalId, endRange, endRange.AddSeconds(1)),
            new(Guid.NewGuid(), journalId, endRange.AddSeconds(-1), endRange.AddSeconds(1)),
        ];

    internal static List<Activity> GetDataWithActivitiesExcludedByRange(DateTime startRange, DateTime endRange, Guid journalId)
    {
        var timeBeforeStart = startRange.AddSeconds(-1);
        var timeAfterEnd = endRange.AddSeconds(1);

        return
        [
            new(Guid.NewGuid(), journalId, timeBeforeStart, timeBeforeStart),
            new(Guid.NewGuid(), journalId, timeBeforeStart, endRange),
            new(Guid.NewGuid(), journalId, timeAfterEnd, timeAfterEnd),
        ];
    }

    internal static List<Activity> GetDataWithActivitiesIncludedByRange(DateTime startRange, DateTime endRange, Guid journalId)
        =>
        [
            new(Guid.NewGuid(), journalId, startRange, endRange),
            new(Guid.NewGuid(), journalId, startRange, endRange.AddSeconds(1)),
            new(Guid.NewGuid(), journalId, startRange.AddSeconds(1), endRange),
            new(Guid.NewGuid(), journalId, startRange.AddSeconds(1), endRange.AddSeconds(1)),
        ];
}
