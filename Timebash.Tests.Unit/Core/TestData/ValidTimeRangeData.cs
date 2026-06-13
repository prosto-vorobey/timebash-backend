namespace Timebash.Tests.Unit.Core.TestData;

public class ValidTimeRangeData : TheoryData<DateTime, DateTime>
{
    public ValidTimeRangeData()
    {
        Add(DateTime.MinValue, DateTime.MinValue);
        Add(DateTime.MinValue.AddMilliseconds(1), DateTime.MinValue);
        Add(DateTime.MinValue, DateTime.MinValue.AddMilliseconds(1));
        Add(DateTime.MinValue, DateTime.MinValue.AddSeconds(1));
        Add(DateTime.MinValue.AddDays(1), DateTime.MaxValue.AddDays(-1));
    }
}
