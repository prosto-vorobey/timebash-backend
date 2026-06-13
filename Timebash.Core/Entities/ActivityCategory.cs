namespace Timebash.Core.Entities;

public class ActivityCategory
{
    public required Guid ActivityId { get; init; }
    public Activity Activity { get; set; } = null!;

    public required Guid CategoryId { get; init; }
    public Category Category { get; set; } = null!;
}