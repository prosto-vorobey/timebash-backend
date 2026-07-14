using FluentValidation;
using Microsoft.AspNetCore.RateLimiting;
using Timebash.API.Extensions.Validation;

namespace Timebash.API.Controllers;

/// <summary>
/// Handles user registration and authentication.
/// </summary>
[ApiController]
[Route("api/auth")]
[AllowAnonymous]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class AuthController(IAuthService service, IValidator<RegisterRequest> registerValidator) : ControllerBase
{
    private readonly IAuthService _service = service;
    private readonly IValidator<RegisterRequest> _registerValidator = registerValidator;

    /// <summary>
    /// Registers a new user account and returns the created profile.
    /// </summary>
    /// <param name="registerRequest">The registration data.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The newly created user profile.</returns>
    /// <response code="201">User registered successfully. The response body contains the user profile.</response>
    /// <response code="400">Validation failed. Check the response body for detailed error codes.</response>
    /// <response code="409">Another user already has the requested name or email.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Register(RegisterRequest registerRequest, CancellationToken cancellationToken = default)
    {
        var validationResult = _registerValidator.Validate(registerRequest);
        if (!validationResult.IsValid) return BadRequest(validationResult.ToValidationErrorResponse(
                HttpContext.Request.Path,
                HttpContext.TraceIdentifier
            ));

        var response = await _service.RegisterAsync(registerRequest, cancellationToken);

        return CreatedAtAction(nameof(MeController.Get), "Me", null, response);
    }

    /// <summary>
    /// Authenticates a user and returns a JWT access token.
    /// </summary>
    /// <param name="loginRequest">Login credentials.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A JWT token for use in authorized requests.</returns>
    /// <response code="200">Login successful. Returns the access token.</response>
    /// <response code="400">Invalid request format, such as malformed JSON.</response>
    /// <response code="401">Authentication failed – incorrect login or password.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [EnableRateLimiting("LoginPolicy")]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest loginRequest, CancellationToken cancellationToken = default)
        => Ok(await _service.LoginAsync(loginRequest, cancellationToken));
}
