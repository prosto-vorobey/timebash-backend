using Microsoft.EntityFrameworkCore;

namespace Timebash.Infrastructure.Repositories;

public class PostgresJournalRepository(TimebashDbContext context) : PostgresRepositoryBase<Journal>(context), IJournalRepository
{
    public async Task<IEnumerable<Journal>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        => await Context.Journals
            .Where(journal => journal.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsUserLinkedAsync(Guid journalId, Guid userId, CancellationToken cancellationToken)
        => await Context.Journals.AnyAsync(journal => journal.Id == journalId && journal.User.Id == userId, cancellationToken);
}
