using Timebash.Core.Utilities;

namespace Timebash.Core.Entities;

public class Journal : EntityBase
{
    public Journal(Guid id, Guid userId, string name) : base(id)
    {
        UserId = Ensure.NotEmpty(userId, nameof(userId));
        Name = name;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public Guid UserId { get; init; }
    public User User { get; private set; } = null!;
    
    public string Name
    {
        get;
        set => field = Ensure.NotNullOrWhiteSpace(value, nameof(Name));
    }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Activity> Activities { get; private set; } = new HashSet<Activity>();
}
