using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Exceptions;

namespace Timebash.Core.Services;

/// <summary>
/// Provides operations for handling user registration and authentication.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account and returns the created profile.
    /// </summary>
    /// <param name="registerRequest">The registration data.</param>
    /// <returns>The newly created user profile.</returns>
    /// <exception cref="ResourceConflictException">Thrown when another user already has the requested name or email.</exception>
    Task<UserResponse> RegisterAsync(RegisterRequest registerRequest);

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    /// <param name="loginRequest">Login credentials.</param>
    /// <returns>A JWT token for use in authorized requests.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the login or password is incorrect.</exception>
    Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
}
