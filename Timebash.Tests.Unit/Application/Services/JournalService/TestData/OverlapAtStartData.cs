namespace Timebash.Tests.Unit.Application.Services.JournalService.TestData;

public class OverlapAtStartData : TheoryData<DateTime, DateTime>
{
    public OverlapAtStartData()
    {
        var end = DateTime.MaxValue;

        Add(end, end.AddDays(-1));
        Add(end, end);
    }
}
