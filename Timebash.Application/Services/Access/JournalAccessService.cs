using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services.Access;

namespace Timebash.Application.Services.Access;

public class JournalAccessService(IJournalRepository repository) : IJournalAccessService
{
    private readonly IJournalRepository _repository = repository;

    public async Task<Journal> EnsureAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        CheckInvalidId(id);

        var journal = await _repository.GetByIdAsync(id, cancellationToken);
        if (journal is null || journal.UserId != userId) throw new NotFoundException();

        return journal;
    }

    public async Task ValidateAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        CheckInvalidId(id);
        if(!await _repository.IsUserLinkedAsync(id, userId, cancellationToken)) throw new NotFoundException();
    }

    private static void CheckInvalidId(Guid id)
    {
        if (id == Guid.Empty) throw new BadRequestException("Invalid id");
    }
}
