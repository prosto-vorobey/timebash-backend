namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a request to update the current user's name.
/// </summary>
/// <param name="Name">The new name to set.</param>
public record UserNameUpdateRequest(string Name);
