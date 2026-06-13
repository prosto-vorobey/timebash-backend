namespace Timebash.Core.Services;

/// <summary>
/// Provides functionality for generating JWT access tokens.
/// </summary>
public interface IJwtProvider
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user entity for which the token is generated.</param>
    /// <returns>A JWT token string.</returns>
    string GenerateToken(User user);
}
