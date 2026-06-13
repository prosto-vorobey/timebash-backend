using System.ComponentModel.DataAnnotations.Schema;
using Timebash.Core.Exceptions;
using Timebash.Core.Extensions;
using Timebash.Core.Utilities;

namespace Timebash.Core.Entities;

public class Activity : EntityBase
{
    public Activity(Guid id, Guid journalId, DateTime startTime, DateTime endTime, string name = "") : base(id)
    {
        JournalId = journalId;
        SetTimeRange(startTime, endTime);
        Name = name;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid JournalId
    {
        get;
        set => field = Ensure.NotEmpty(value, nameof(JournalId));
    }
    public Journal Journal { get; private set; } = null!;
    
    public string Name
    {
        get;
        set => field = value ?? string.Empty;
    } = string.Empty;

    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

    [NotMapped]
    public TimeSpan Duration => EndTime - StartTime;

    public ICollection<ActivityCategory> ActivityCategories { get; private set; } = new HashSet<ActivityCategory>();

    public bool UpdateTimeRange(DateTime start, DateTime end)
    {
        var truncatedStart = start.TruncateToSecond();
        var truncatedEnd = end.TruncateToSecond();

        if (truncatedStart > truncatedEnd) throw new ActivityTimeRangeException("Start time cannot be later than end time.");
        if (StartTime == truncatedStart && EndTime == truncatedEnd) return false;

        StartTime = truncatedStart;
        EndTime = truncatedEnd;

        return true;
    }

    private void SetTimeRange(DateTime start, DateTime end)
    {
        var truncatedStart = start.TruncateToSecond();
        var truncatedEnd = end.TruncateToSecond();
        if (truncatedStart > truncatedEnd) throw new ActivityTimeRangeException("Start time cannot be later than end time.");

        StartTime = truncatedStart;
        EndTime = truncatedEnd;
    }
}
