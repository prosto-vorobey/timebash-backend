namespace Timebash.Tests.Unit.Application.Services.JournalService.TestData;

public class OverlapAtEndData : TheoryData<DateTime, DateTime>
{
    public OverlapAtEndData()
    {
        var start = DateTime.MinValue;

        Add(start, start.AddDays(1));
        Add(start, start);
    }
}
