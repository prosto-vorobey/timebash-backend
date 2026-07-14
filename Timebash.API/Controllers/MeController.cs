using Microsoft.AspNetCore.RateLimiting;
using Timebash.API.Extensions.Validation;
using Timebash.Application.Validators;

namespace Timebash.API.Controllers;

/// <summary>
/// Manages the current user’s profile and settings.
/// </summary>
[Authorize]
[ApiController]
[Route("api/me")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class MeController(
    ICurrentUserService currentUserService,
    IMeService meService,
    PasswordValidator passwordValidator,
    UserNameValidator nameValidator,
    UserEmailValidator emailValidator) : ControllerBase
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IMeService _meService = meService;
    private readonly PasswordValidator _passwordValidator = passwordValidator;
    private readonly UserNameValidator _nameValidator = nameValidator;
    private readonly UserEmailValidator _emailValidator = emailValidator;

    /// <summary>
    /// Retrieves a current user profile.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The current user profile.</returns>
    /// <response code="200">The user profile was found and returned.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The user was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> Get(CancellationToken cancellationToken = default)
        => Ok(await _meService.GetAsync(_currentUserService.GetCurrentUserId(), cancellationToken));

    /// <summary>
    /// Returns all journals that belong to the current user.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A collection of journals linked to the user.</returns>
    /// <response code="200">Journals were successfully retrieved.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("journals")]
    [ProducesResponseType(typeof(JournalsListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<JournalsListResponse>> GetJournals(CancellationToken cancellationToken = default)
        => Ok(await _meService.GetJournalsAsync(_currentUserService.GetCurrentUserId(), cancellationToken));

    /// <summary>
    /// Returns all categories that belong to the current user.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>A collection of categories linked to the user.</returns>
    /// <response code="200">Categories were successfully retrieved.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("categories")]
    [ProducesResponseType(typeof(CategoriesListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CategoriesListResponse>> GetCategories(CancellationToken cancellationToken = default)
        => Ok(await _meService.GetCategoriesAsync(_currentUserService.GetCurrentUserId(), cancellationToken));

    /// <summary>
    /// Returns the default journal for the current user.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The default journal of the current user.</returns>
    /// <response code="200">Default journal successfully retrieved.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The user was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("default-journal")]
    [ProducesResponseType(typeof(JournalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JournalResponse>> GetDefaultJournal(CancellationToken cancellationToken = default)
        => Ok(await _meService.GetDefaultJournalAsync(_currentUserService.GetCurrentUserId(), cancellationToken));

    /// <summary>
    /// Replaces current user name with the provided data.
    /// </summary>
    /// <param name="request">The new name.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>No content when the update is successful.</returns>
    /// <response code="204">The name was updated successfully.</response>
    /// <response code="400">Validation failed. Check the response body for detailed error codes.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The user was not found.</response>
    /// <response code="409">Another user already has the requested name.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPatch("name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> UpdateName(UserNameUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = _nameValidator.Validate(request.Name);
        if (!validationResult.IsValid) return BadRequest(validationResult.ToValidationErrorResponse(
                HttpContext.Request.Path,
                HttpContext.TraceIdentifier
            ));

        var changed = await _meService.UpdateNameAsync(request, _currentUserService.GetCurrentUserId(), cancellationToken);
        if (!changed) HttpContext.Response.Headers["X-No-Changes"] = "true";

        return NoContent();
    }

    /// <summary>
    /// Replaces current user email with the provided data.
    /// </summary>
    /// <param name="request">The new email.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>No content when the update is successful.</returns>
    /// <response code="204">The email was updated successfully.</response>
    /// <response code="400">Validation failed. Check the response body for detailed error codes.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The user was not found.</response>
    /// <response code="409">Another user already has the requested email.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPatch("email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> UpdateEmail(UserEmailUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = _emailValidator.Validate(request.Email);
        if (!validationResult.IsValid) return BadRequest(validationResult.ToValidationErrorResponse(
                HttpContext.Request.Path,
                HttpContext.TraceIdentifier
            ));
        
        var changed = await _meService.UpdateEmailAsync(request, _currentUserService.GetCurrentUserId(), cancellationToken);
        if (!changed) HttpContext.Response.Headers["X-No-Changes"] = "true";

        return NoContent();
    }

    /// <summary>
    /// Replaces current user password with the provided data.
    /// </summary>
    /// <param name="request">The password change data.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>No content when the update is successful.</returns>
    /// <response code="204">The password was updated successfully.</response>
    /// <response code="400">Validation failed. Check the response body for detailed error codes.</response>
    /// <response code="401">The current password is incorrect or authentication is required.</response>
    /// <response code="404">The user was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [EnableRateLimiting("LoginPolicy")]
    [HttpPatch("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdatePassword(PasswordUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = _passwordValidator.Validate(request.NewPassword);
        if (!validationResult.IsValid) return BadRequest(validationResult.ToValidationErrorResponse(
                HttpContext.Request.Path,
                HttpContext.TraceIdentifier
            ));

        var changed = await _meService.UpdatePasswordAsync(request, _currentUserService.GetCurrentUserId(), cancellationToken);
        if (!changed) HttpContext.Response.Headers["X-No-Changes"] = "true";

        return NoContent();
    }

    /// <summary>
    /// Replaces current user default journal with the provided data.
    /// </summary>
    /// <param name="request">The new default journal data.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>No content when the update is successful.</returns>
    /// <response code="204">The default journal was updated successfully.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The user was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPatch("default-journal")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateDefaultJournal(DefaultJournalUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var changed = await _meService.UpdateDefaultJournalAsync(request, _currentUserService.GetCurrentUserId(), cancellationToken);
        if (!changed) HttpContext.Response.Headers["X-No-Changes"] = "true";

        return NoContent();
    }

    /// <summary>
    /// Deletes the current user.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>No content when the deletion succeeds.</returns>
    /// <response code="204">The user was deleted successfully.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The user was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(CancellationToken cancellationToken = default)
    {
        await _meService.DeleteAsync(_currentUserService.GetCurrentUserId(), cancellationToken);
        return NoContent();
    }
}
