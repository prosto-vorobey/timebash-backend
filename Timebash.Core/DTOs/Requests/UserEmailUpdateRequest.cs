namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a request to update the current user's email.
/// </summary>
/// <param name="Email">The new email address to set.</param>
public record UserEmailUpdateRequest(string Email);
