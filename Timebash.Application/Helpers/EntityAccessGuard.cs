using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;

namespace Timebash.Application.Helpers;

public static class EntityAccessGuard
{
    public static async Task<User> EnsureUserAccessAsync(IUserRepository repository, Guid id)
    {
        CheckInvalidId(id);
        var user = await repository.GetByIdAsync(id) ?? throw new NotFoundException();

        return user;
    }

    public static async Task ValidateUserExistsAsync(IUserRepository repository, Guid id)
    {
        CheckInvalidId(id);
        if (!await repository.ExistsAsync(id)) throw new NotFoundException();
    }

    public static async Task<Journal> EnsureJournalAccessAsync(IJournalRepository repository, Guid id, Guid userId)
    {
        CheckInvalidId(id);

        var journal = await repository.GetByIdAsync(id);
        if (journal is null || journal.UserId != userId) throw new NotFoundException();

        return journal;
    }

    public static async Task ValidateJournalAccessAsync(IJournalRepository repository, Guid id, Guid userId)
    {
        CheckInvalidId(id);
        if(!await repository.IsUserLinkedAsync(id, userId)) throw new NotFoundException();
    }

    public static async Task<Category> EnsureCategoryAccessAsync(ICategoryRepository repository, Guid id, Guid userId)
    {
        CheckInvalidId(id);

        var category = await repository.GetByIdAsync(id);
        if (category is null || category.UserId != userId) throw new NotFoundException();

        return category;
    }

    public static async Task ValidateCategoryAccessAsync(ICategoryRepository repository, Guid id, Guid userId)
    {
        CheckInvalidId(id);
        if (!await repository.IsUserLinkedAsync(id, userId)) throw new NotFoundException();
    }

    public static async Task<Activity> EnsureActivityAccessAsync(
        IActivityRepository activityRepository, 
        IJournalRepository journalRepository,
        Guid id, 
        Guid userId)
    {
        CheckInvalidId(id);

        var activity = await activityRepository.GetByIdAsync(id);
        if (activity is null || !await journalRepository.IsUserLinkedAsync(activity.JournalId, userId)) throw new NotFoundException();

        return activity;
    }

    public static async Task ValidateActivityAccessAsync(IActivityRepository repository, Guid id, Guid userId)
    {
        CheckInvalidId(id);
        if (!await repository.IsOwnedByUserAsync(id, userId)) throw new NotFoundException();
    }

    private static void CheckInvalidId(Guid id)
    {
        if (id == Guid.Empty) throw new BadRequestException("Invalid id");
    }
}
