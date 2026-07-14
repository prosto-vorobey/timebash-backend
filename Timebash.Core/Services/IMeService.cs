using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Exceptions;

namespace Timebash.Core.Services;

/// <summary>
/// Provides operations for managing current user’s profile and settings.
/// </summary>
public interface IMeService
{
    /// <summary>
    /// Retrieves a current user profile.
    /// </summary>
    /// <param name="userId">The current user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The current user profile.</returns>
    /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
    Task<UserResponse> GetAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns all journals that belong to the current user.
    /// </summary>
    /// <param name="userId">The current user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A collection of journals linked to the user.</returns>
    Task<JournalsListResponse> GetJournalsAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns all categories that belong to the current user.
    /// </summary>
    /// <param name="userId">The current user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A collection of categories linked to the user.</returns>
    Task<CategoriesListResponse> GetCategoriesAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the default journal for the current user.
    /// </summary>
    /// <param name="userId">The current user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The default journal of the current user.</returns>
    /// <exception cref="NotFoundException">Thrown when the user or the associated default journal does not exist.</exception>
    Task<JournalResponse> GetDefaultJournalAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces current user name with the provided data.
    /// </summary>
    /// <param name="request">The new name.</param>
    /// <param name="userId">The current user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>True if any changes were applied, otherwise false.</returns>
    /// <exception cref="ResourceConflictException">Thrown when another user already has the requested name.</exception>
    /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
    Task<bool> UpdateNameAsync(UserNameUpdateRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces current user email with the provided data.
    /// </summary>
    /// <param name="request">The new email.</param>
    /// <param name="userId">The current user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>True if any changes were applied, otherwise false.</returns>
    /// <exception cref="ResourceConflictException">Thrown when another user already has the requested email.</exception>
    /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
    Task<bool> UpdateEmailAsync(UserEmailUpdateRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces current user password with the provided data.
    /// </summary>
    /// <param name="request">The password change data.</param>
    /// <param name="userId">The current user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>True if any changes were applied, otherwise false.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the current password is incorrect.</exception>
    /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
    Task<bool> UpdatePasswordAsync(PasswordUpdateRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces current user default journal with the provided data.
    /// </summary>
    /// <param name="request">The new default journal data.</param>
    /// <param name="userId">The current user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>True if any changes were applied, otherwise false.</returns>
    /// <exception cref="NotFoundException">Thrown when the user or the associated default journal does not exist.</exception>
    Task<bool> UpdateDefaultJournalAsync(DefaultJournalUpdateRequest request, Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the current user.
    /// </summary>
    /// <param name="userId">The current user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
    Task DeleteAsync(Guid userId, CancellationToken cancellationToken);
}
