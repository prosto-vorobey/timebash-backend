using Timebash.Core.Utilities;

namespace Timebash.Core.Entities;

public class User : EntityBase
{
    public User(Guid id, string name, string email) : base(id)
    {
        Name = name;
        Email = email;
    }

    public string Name
    {
        get;
        set => field = Ensure.NotNullOrWhiteSpace(value, nameof(Name));
    }

    public string Email
    {
        get;
        set => field = Ensure.NotNullOrWhiteSpace(value, nameof(Email));
    }

    public string PasswordHash
    {
        get;
        set => field = Ensure.NotNullOrWhiteSpace(value, nameof(PasswordHash));
    } = null!;
    
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public ICollection<Journal> Journals { get; private set; } = new HashSet<Journal>();
    public ICollection<Category> Categories { get; private set; } = new HashSet<Category>();
}
