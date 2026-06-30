using Timebash.Core.Exceptions;
using Timebash.Core.Repositories;

namespace Timebash.Application.Helpers;

public static class EntityAccessGuard
{
    public static async Task<User> EnsureUserAccessAsync(IUserRepository repository, Guid id)
    {
        if (id == Guid.Empty) throw new BadRequestException("Invalid id");
        var user = await repository.GetByIdAsync(id) ?? throw new NotFoundException();
        
        return user;
    }

    public static async Task<Journal> EnsureJournalAccessAsync(IJournalRepository repository, Guid id, Guid userId)
    {
        if (id == Guid.Empty) throw new BadRequestException("Invalid id");

        var journal = await repository.GetByIdAsync(id);
        if (journal is null || journal.UserId != userId) throw new NotFoundException();

        return journal;
    }

    public static async Task<Category> EnsureCategoryAccessAsync(ICategoryRepository repository, Guid id, Guid userId)
    {
        if (id == Guid.Empty) throw new BadRequestException("Invalid id");

        var category = await repository.GetByIdAsync(id);
        if (category is null || category.UserId != userId) throw new NotFoundException();

        return category;
    }

    public static async Task<Activity> EnsureActivityAccessAsync(
        IActivityRepository activityRepository, 
        IJournalRepository journalRepository,
        Guid id, 
        Guid userId)
    {
        if (id == Guid.Empty) throw new BadRequestException("Invalid id");

        var activity = await activityRepository.GetByIdAsync(id);
        if (activity is null || !await journalRepository.IsUserLinkedAsync(activity.JournalId, userId)) throw new NotFoundException();

        return activity;
    }
}
