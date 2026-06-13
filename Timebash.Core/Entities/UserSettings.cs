namespace Timebash.Core.Entities;

public class UserSettings
{
    public required Guid UserId { get; init;}
    public User User { get; set; } = null!;
    public required Guid DefaultJournalId { get; set; }
    public Journal DefaultJournal { get; set; } = null!;
}
