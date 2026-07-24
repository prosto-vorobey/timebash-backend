using Timebash.Core.Exceptions;

namespace Timebash.Core.Services.Access;

/// <summary>
/// Provides access verification for <see cref="Journal"/> entities.
/// </summary>
public interface IJournalAccessService
{
    /// <summary>
    /// Ensures that a journal with the specified ID exists and belongs to the given user, then returns it.
    /// </summary>
    /// <param name="id">The journal ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The journal if found and owned by <paramref name="userId"/>.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the journal does not exist or does not belong to the specified user.</exception>
    public Task<Journal> EnsureAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates that a journal with the specified ID exists and belongs to the given user.
    /// </summary>
    /// <param name="id">The journal ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A task that completes successfully if the journal exists and is owned by the user; otherwise, an exception is thrown.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the journal does not exist or does not belong to the specified user.</exception>
    public Task ValidateAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
