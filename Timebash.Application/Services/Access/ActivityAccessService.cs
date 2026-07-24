using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;
using Timebash.Core.Services.Access;

namespace Timebash.Application.Services.Access;

public class ActivityAccessService(IActivityRepository activityRepository, IJournalRepository journalRepository) : IActivityAccessService
{
    private readonly IActivityRepository _activityRepository = activityRepository;
    private readonly IJournalRepository _journalRepository = journalRepository;

    public async Task<Activity> EnsureAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        CheckInvalidId(id);

        var activity = await _activityRepository.GetByIdAsync(id, cancellationToken);
        if (activity is null || !await _journalRepository.IsUserLinkedAsync(activity.JournalId, userId, cancellationToken)) throw new NotFoundException();

        return activity;
    }

    public async Task ValidateAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        CheckInvalidId(id);
        if (!await _activityRepository.IsOwnedByUserAsync(id, userId, cancellationToken)) throw new NotFoundException();
    }

    private static void CheckInvalidId(Guid id)
    {
        if (id == Guid.Empty) throw new BadRequestException("Invalid id");
    }
}
