namespace Timebash.API.Controllers;

/// <summary>
/// Manages user-defined activities.
/// </summary>
[Authorize]
[ApiController]
[Route("api/activities")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class ActivitiesController(ICurrentUserService currentUserService, IActivityService activityService) : ControllerBase
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IActivityService _activityService = activityService;

    /// <summary>
    /// Retrieves an activity by its unique identifier.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <returns>The requested activity.</returns>
    /// <response code="200">The activity was found and returned.</response>
    /// <response code="400">The provided ID is invalid, such as empty GUID.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The activity was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ActivityResponse>> GetById(Guid id)
        => Ok(await _activityService.GetByIdAsync(id, _currentUserService.GetCurrentUserId()));

    /// <summary>
    /// Returns all categories that belong to the specified activity.
    /// </summary>
    /// <param name="id">The activity ID.</param>
    /// <returns>A collection of categories linked to the activity.</returns>
    /// <response code="200">Categories were successfully retrieved.</response>
    /// <response code="400">The provided ID is invalid, such as empty GUID.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The activity was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("{id:guid}/categories")]
    [ProducesResponseType(typeof(CategoriesListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoriesListResponse>> GetCategoriesByActivityId(Guid id)
        => Ok(await _activityService.GetCategoriesByActivityIdAsync(id, _currentUserService.GetCurrentUserId()));

    /// <summary>
    /// Creates a new activity.
    /// </summary>
    /// <remarks>
    /// This operation also updates the journal's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="request">The activity data.</param>
    /// <returns>The newly created activity.</returns>
    /// <response code="201">The activity was created successfully. The response body contains the activity data. Journal's UpdatedAt was refreshed.</response>
    /// <response code="400">The request body is invalid or validation failed.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The journal or category was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ActivityResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult> Create(ActivityRequest request)
    {
        var response = await _activityService.CreateAsync(request, _currentUserService.GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Creates a new activity with auto-time correction for existing activities.
    /// </summary>
    /// <remarks>
    /// This operation updates the UpdatedAt timestamp of the journal and any affected activities.
    /// </remarks>
    /// <param name="request">The activity data with time correction options.</param>
    /// <returns>The newly created activity and additional activities.</returns>
    /// <response code="200">The activity was created with applied time corrections. Journal's and affected activities' UpdatedAt were refreshed.</response>
    /// <response code="400">The request body is invalid or validation failed.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The journal or category was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPost("activities/create-with-correction")]
    [ProducesResponseType(typeof(ActivityWithCorrectionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ActivityWithCorrectionResponse>> CreateWithCorrection(ActivityWithCorrectionRequest request)
        => Ok(await _activityService.CreateWithCorrectionAsync(request, _currentUserService.GetCurrentUserId()));

    /// <summary>
    /// Adds a category to the specified activity.
    /// </summary>
    /// <remarks>
    /// This operation updates the activity's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="categoryId">The category ID to add.</param>
    /// <returns>No content when the addition is successful.</returns>
    /// <response code="204">The category was added to the activity successfully. Activity's UpdatedAt was refreshed.</response>
    /// <response code="400">Provided IDs are invalid, such as empty GUID.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The activity or category was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPost("{activityId:guid}/categories/{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> AddCategoryToActivity(Guid activityId, Guid categoryId)
    {
        var changed = await _activityService.AddCategoryToActivityAsync(activityId, categoryId, _currentUserService.GetCurrentUserId());
        if (!changed) HttpContext.Response.Headers["X-No-Changes"] = "true";

        return NoContent();
    }

    /// <summary>
    /// Adds a collection of categories to the specified activity.
    /// </summary>
    /// <remarks>
    /// This operation updates the activity's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="request">A collection of category IDs to add.</param>
    /// <returns>No content when the addition is successful.</returns>
    /// <response code="204">Categories were added to the activity successfully. Activity's UpdatedAt was refreshed.</response>
    /// <response code="400">The provided ID or request body is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The activity or category was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPost("{activityId:guid}/categories")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> AddCategoriesToActivity(Guid activityId, ActivityCategoriesRequest request)
    {
        var changed = await _activityService.AddCategoriesToActivityAsync(activityId, request, _currentUserService.GetCurrentUserId());
        if (!changed) HttpContext.Response.Headers["X-No-Changes"] = "true";

        return NoContent();
    }

    /// <summary>
    /// Replaces an existing activity with the provided data.
    /// </summary>
    /// <remarks>
    /// This operation updates the activity's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="id">The activity ID to update.</param>
    /// <param name="request">The new activity data.</param>
    /// <returns>No content when the update is successful.</returns>
    /// <response code="204">The activity was updated successfully. Activity's UpdatedAt was refreshed.</response>
    /// <response code="400">The provided ID or request body is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The activity was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Update(Guid id, ActivityRequest request)
    {
        var changed = await _activityService.UpdateAsync(id, request, _currentUserService.GetCurrentUserId());
        if (!changed) HttpContext.Response.Headers["X-No-Changes"] = "true";

        return NoContent();
    }

    /// <summary>
    /// Replaces the categories of the specified activity with the provided data.
    /// </summary>
    /// <remarks>
    /// This operation updates the activity's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="request">A collection of category IDs to set.</param>
    /// <returns>No content when the update is successful.</returns>
    /// <response code="204">Activity categories were updated successfully. Activity's UpdatedAt was refreshed.</response>
    /// <response code="400">The provided ID or request body is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The activity or category was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPut("{activityId:guid}/categories")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> UpdateActivityCategories(Guid activityId, ActivityCategoriesRequest request)
    {
        var changed = await _activityService.UpdateActivityCategoriesAsync(activityId, request, _currentUserService.GetCurrentUserId());
        if (!changed) HttpContext.Response.Headers["X-No-Changes"] = "true";

        return NoContent();
    }

    /// <summary>
    /// Deletes the specified activity.
    /// </summary>
    /// <remarks>
    /// This operation also updates the journal's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="id">The activity ID to delete.</param>
    /// <returns>No content when the deletion is successful.</returns>
    /// <response code="204">The activity was deleted successfully. Journal's UpdatedAt was refreshed.</response>
    /// <response code="400">The provided ID is invalid, such as empty GUID.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The activity was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _activityService.DeleteAsync(id, _currentUserService.GetCurrentUserId());
        return NoContent();
    }

    /// <summary>
    /// Removes a category from the specified activity.
    /// </summary>
    /// <remarks>
    /// This operation updates the activity's UpdatedAt timestamp.
    /// </remarks>
    /// <param name="activityId">The activity ID.</param>
    /// <param name="categoryId">The category ID to remove.</param>
    /// <returns>No content when the removal is successful.</returns>
    /// <response code="204">The category was removed from the activity successfully. Activity's UpdatedAt was refreshed.</response>
    /// <response code="400">Provided IDs are invalid, such as empty GUID.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The activity or category was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpDelete("{activityId:guid}/categories/{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> RemoveCategoryFromActivity(Guid activityId, Guid categoryId)
    {
        var changed = await _activityService.RemoveCategoryFromActivityAsync(activityId, categoryId, _currentUserService.GetCurrentUserId());
        if (!changed) HttpContext.Response.Headers["X-No-Changes"] = "true";

        return NoContent();
    }
}
