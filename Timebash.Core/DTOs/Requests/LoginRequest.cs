namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a request to authenticate a user.
/// </summary>
/// <param name="Login">The user's name or email address.</param>
/// <param name="Password">The user's password.</param>
public record LoginRequest(
    string Login,
    string Password
);
