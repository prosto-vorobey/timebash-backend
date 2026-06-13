namespace Timebash.API.Controllers;

/// <summary>
/// Manages user-defined activity categories.
/// </summary>
[Authorize]
[ApiController]
[Route("api/categories")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class CategoriesController(ICurrentUserService currentUserService, ICategoryService categoryService) : ControllerBase
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly ICategoryService _categoryService = categoryService;

    /// <summary>
    /// Retrieves a category by its unique identifier.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>The requested category.</returns>
    /// <response code="200">The category was found and returned.</response>
    /// <response code="400">The provided ID is invalid, such as empty GUID.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The category was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryResponse>> GetById(Guid id)
        => Ok(await _categoryService.GetByIdAsync(id, _currentUserService.GetCurrentUserId()));

    /// <summary>
    /// Returns all activities that belong to the specified category.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <returns>A collection of activities linked to the category.</returns>
    /// <response code="200">Activities were successfully retrieved.</response>
    /// <response code="400">The provided ID is invalid, such as empty GUID.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The category was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("{id:guid}/activities")]
    [ProducesResponseType(typeof(ActivitiesListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivitiesListResponse>> GetActivitiesByCategoryId(Guid id)
        => Ok(await _categoryService.GetActivitiesByCategoryIdAsync(id, _currentUserService.GetCurrentUserId()));

    /// <summary>
    /// Creates a new category.
    /// </summary>
    /// <param name="categoryRequest">The category data.</param>
    /// <returns>The newly created category.</returns>
    /// <response code="201">The category was created successfully. The response body contains the category data.</response>
    /// <response code="400">The request body is invalid or validation failed.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(CategoryRequest categoryRequest)
    {
        var response = await _categoryService.CreateAsync(categoryRequest, _currentUserService.GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Replaces an existing category with the provided data.
    /// </summary>
    /// <param name="id">The category ID to update.</param>
    /// <param name="categoryRequest">The new category data.</param>
    /// <returns>No content when the update is successful.</returns>
    /// <response code="204">The category was updated successfully.</response>
    /// <response code="400">Invalid ID or request body.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The category was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, CategoryRequest categoryRequest)
    {
        var changed = await _categoryService.UpdateAsync(id, categoryRequest, _currentUserService.GetCurrentUserId());
        if (!changed) HttpContext.Response.Headers["X-No-Changes"] = "true";

        return NoContent();
    }

    /// <summary>
    /// Deletes the specified category.
    /// </summary>
    /// <param name="id">The category ID to delete.</param>
    /// <returns>No content when the deletion succeeds.</returns>
    /// <response code="204">The category was deleted successfully.</response>
    /// <response code="400">The provided ID is invalid, such as empty GUID.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The category was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _categoryService.DeleteAsync(id, _currentUserService.GetCurrentUserId());
        return NoContent();
    }
}
