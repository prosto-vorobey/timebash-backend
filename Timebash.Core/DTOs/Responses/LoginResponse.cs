namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents an authentication response containing a JSON Web Token.
/// </summary>
/// <param name="Token">The access token returned after successful login.</param>
public record LoginResponse(string Token);
