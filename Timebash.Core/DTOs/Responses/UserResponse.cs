namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents the response data for a user.
/// </summary>
/// <param name="Id">The unique identifier of the user.</param>
/// <param name="Name">The display name of the user.</param>
/// <param name="Email">The user’s email address.</param>
/// <param name="CreatedAt">The timestamp when the user account was created (UTC).</param>
public record UserResponse
(
    Guid Id,
    string Name,
    string Email,
    DateTime CreatedAt
);
