namespace Timebash.Core.Repositories;

public interface IJournalRepository : IRepository<Journal>
{
    Task<IEnumerable<Journal>> GetByUserIdAsync(Guid userId);
    Task<bool> IsUserLinkedAsync(Guid journalId, Guid userId);
}
