namespace Timebash.Core.Repositories;

public interface IJournalRepository : IRepository<Journal>
{
    Task<IEnumerable<Journal>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<bool> IsUserLinkedAsync(Guid journalId, Guid userId, CancellationToken cancellationToken);
}
