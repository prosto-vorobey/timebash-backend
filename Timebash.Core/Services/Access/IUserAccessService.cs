using Timebash.Core.Exceptions;

namespace Timebash.Core.Services.Access;

/// <summary>
/// Provides access verification for <see cref="User"/> entities.
/// </summary>
public interface IUserAccessService
{
    /// <summary>
    /// Ensures that a user with the specified ID exists and returns it.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The user if found.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when no user with the given <paramref name="id"/> exists.</exception>
    public Task<User> EnsureAccessAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Validates that a user with the specified ID exists.
    /// </summary>
    /// <param name="id">The user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A task that completes successfully if the user exists; otherwise, an exception is thrown.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when no user with the given <paramref name="id"/> exists.</exception>
    public Task ValidateExistsAsync(Guid id, CancellationToken cancellationToken);
}
