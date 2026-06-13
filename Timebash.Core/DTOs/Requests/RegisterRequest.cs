namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a request to register a new user account.
/// The provided name and email will be used for authentication.
/// </summary>
/// <param name="Name">The desired display name.</param>
/// <param name="Email">The user's email address.</param>
/// <param name="Password">The password for the account.</param>
public record RegisterRequest
(
    string Name,
    string Email,
    string Password
);
