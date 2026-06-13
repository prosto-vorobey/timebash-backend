namespace Timebash.Core.Services;

/// <summary>
/// Provides password hashing and verification.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Verifies the specified password against the user's stored hash.
    /// </summary>
    /// <param name="user">The user entity whose password needs to be verified.</param>
    /// <param name="password">The password to verified.</param>
    /// <returns>True if the password matches, otherwise false.</returns>
    bool VerifyPassword(User user, string password);

    /// <summary>
    /// Hashes a password for the specified user.
    /// </summary>
    /// <param name="user">The user entity for which the password is being hashed.</param>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>A password hash string.</returns>
    string HashPassword(User user, string password);
}
