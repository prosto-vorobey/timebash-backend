using Timebash.Core.Exceptions;

namespace Timebash.Core.Services.Access;

/// <summary>
/// Provides access verification for <see cref="Category"/> entities.
/// </summary>
public interface ICategoryAccessService
{
    /// <summary>
    /// Ensures that a category with the specified ID exists and belongs to the given user, then returns it.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The category if found and owned by <paramref name="userId"/>.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the category does not exist or does not belong to the specified user.</exception>
    public Task<Category> EnsureAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Validates that a category with the specified ID exists and belongs to the given user.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A task that completes successfully if the category exists and is owned by the user; otherwise, an exception is thrown.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the category does not exist or does not belong to the specified user.</exception>
    public Task ValidateAccessAsync(Guid id, Guid userId, CancellationToken cancellationToken);
}
