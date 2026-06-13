namespace Timebash.Tests.Unit.Core.TestData;

public class TruncateToSecondData : TheoryData<DateTime, DateTime>
{
    public TruncateToSecondData()
    {
        var expected = new DateTime(2026, 5, 20, 10, 30, 45, 0, DateTimeKind.Utc);

        Add(expected.AddMilliseconds(1), expected);
        Add(expected.AddMicroseconds(1), expected);
        Add(expected.AddMicroseconds(-1), expected.AddSeconds(-1));
    }
}
