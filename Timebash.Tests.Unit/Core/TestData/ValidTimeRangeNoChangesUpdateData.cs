namespace Timebash.Tests.Unit.Core.TestData;

public class ValidTimeRangeNoChangesUpdateData : TheoryData<DateTime, DateTime, DateTime, DateTime>
{
    public ValidTimeRangeNoChangesUpdateData()
    {
        var currentStart = DateTime.MinValue;
        var currentEnd = DateTime.MaxValue;

        Add(currentStart, currentEnd, currentStart, currentEnd);
        Add(currentStart, currentEnd, currentStart.AddMilliseconds(1), currentEnd);
        Add(currentStart, currentEnd, currentStart, currentEnd.AddMilliseconds(-1));
    }
}
