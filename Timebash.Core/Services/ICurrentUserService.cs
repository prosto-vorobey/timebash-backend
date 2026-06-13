namespace Timebash.Core.Services;

/// <summary>
/// Provides access to the current user's identity.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Returns the unique identifier of the current user, extracted from the HTTP context.
    /// </summary>
    /// <returns>The current user's unique identifier.</returns>
    Guid GetCurrentUserId();
}
