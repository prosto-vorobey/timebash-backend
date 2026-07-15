using Timebash.Core.Exceptions;

namespace Timebash.Core.Services.Access;

/// <summary>
/// Provides access verification for <see cref="Activity"/> entities.
/// </summary>
public interface IActivityAccessService
{
    /// <summary>
    /// Ensures that an activity with the specified ID exists, belongs to a journal owned by the given user, and returns it.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The activity if found and associated with a journal owned by <paramref name="userId"/>.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the activity does not exist or its journal does not belong to the specified user.</exception>
    public Task<Activity> EnsureAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates that an activity with the specified ID exists and is owned by the given user (through its journal).
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A task that completes successfully if the activity exists and belongs to the user; otherwise, an exception is thrown.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the activity does not exist or its journal does not belong to the specified user.</exception>
    public Task ValidateAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}