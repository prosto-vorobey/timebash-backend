using Timebash.Core.Utilities;

namespace Timebash.Core.Entities;

public class Category : EntityBase
{
    public Category(Guid id, Guid userId, string name, string color) : base(id)
    {
        UserId = Ensure.NotEmpty(userId, nameof(userId));
        Name = name;
        Color = color;
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

    public string Color
    {
        get;
        set => field = Ensure.ValidHexColor(value, nameof(Color));
    }

    public List<string> Keywords
    {
        get;
        set => field = value ?? [];
    } = [];
    
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ActivityCategory> ActivityCategories { get; private set; } = new HashSet<ActivityCategory>();
}
