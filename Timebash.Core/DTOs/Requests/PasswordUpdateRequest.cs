namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a request to update the current user's password.
/// </summary>
/// <param name="CurrentPassword">The user's current password.</param>
/// <param name="NewPassword">The new password to set.</param>
public record PasswordUpdateRequest
(
    string CurrentPassword,
    string NewPassword
);
