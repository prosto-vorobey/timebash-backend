namespace Timebash.API.Controllers;

/// <summary>
/// Manages user-defined journals.
/// </summary>
[Authorize]
[ApiController]
[Route("api/journals")]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class JournalsController(ICurrentUserService currentUserService, IJournalService journalService) : ControllerBase
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IJournalService _journalService = journalService;

    /// <summary>
    /// Retrieves a journal by its unique identifier.
    /// </summary>
    /// <param name="id">The journal ID.</param>
    /// <returns>The requested journal.</returns>
    /// <response code="200">The journal was found and returned.</response>
    /// <response code="400">The provided ID is invalid, such as empty GUID.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The journal was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JournalResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JournalResponse>> GetById(Guid id)
        => Ok(await _journalService.GetByIdAsync(id, _currentUserService.GetCurrentUserId()));

    /// <summary>
    /// Returns all activities that belong to the specified journal.
    /// </summary>
    /// <param name="id">The journal ID.</param>
    /// <param name="date">
    /// Optional date (UTC) to filter activities. If <c>null</c>, all activities are returned.
    /// If provided, only activities that occurred on that specific day (in the timezone specified by <paramref name="offsetMinutes"/>) are included.
    /// </param>
    /// <param name="offsetMinutes">
    /// Optional timezone offset in minutes. If <c>null</c>, UTC (0) is used.
    /// This offset is applied to <paramref name="date"/> to determine the start and end of the day in the user's local time.
    /// </param>
    /// <returns>A collection of activities linked to the journal.</returns>
    /// <response code="200">Activities were successfully retrieved.</response>
    /// <response code="400">The provided ID or time is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The journal was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("{id:guid}/activities")]
    [ProducesResponseType(typeof(ActivitiesListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ActivitiesListResponse>> GetActivitiesByJournalId(Guid id,
        [FromQuery] DateTime? date = null, [FromQuery] int? offsetMinutes = null)
        => Ok(await _journalService.GetActivitiesByJournalIdAsync(id, date, offsetMinutes, _currentUserService.GetCurrentUserId()));

    /// <summary>
    /// Creates a new journal.
    /// </summary>
    /// <param name="journalRequest">The journal data.</param>
    /// <returns>The newly created journal.</returns>
    /// <response code="201">The journal was created successfully. The response body contains the journal data.</response>
    /// <response code="400">The request body is invalid or validation failed.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPost]
    [ProducesResponseType(typeof(JournalResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(JournalRequest journalRequest)
    {
        var response = await _journalService.CreateAsync(journalRequest, _currentUserService.GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Replaces an existing journal with the provided data.
    /// </summary>
    /// <param name="id">The journal ID to update.</param>
    /// <param name="journalRequest">The new journal data.</param>
    /// <returns>No content when the update is successful.</returns>
    /// <response code="204">The journal was updated successfully.</response>
    /// <response code="400">The provided ID or request body is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The journal was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, JournalRequest journalRequest)
    {
        var changed = await _journalService.UpdateAsync(id, journalRequest, _currentUserService.GetCurrentUserId());
        if (!changed) HttpContext.Response.Headers["X-No-Changes"] = "true";

        return NoContent();
    }

    /// <summary>
    /// Deletes the specified journal.
    /// </summary>
    /// <param name="id">The journal ID to delete.</param>
    /// <returns>No content when the deletion succeeds.</returns>
    /// <response code="204">The journal was deleted successfully.</response>
    /// <response code="400">The provided ID is invalid, such as empty GUID.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The journal was not found.</response>
    /// <response code="409">The journal cannot be deleted because it is the default journal.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Delete(Guid id)
    {
        await _journalService.DeleteAsync(id, _currentUserService.GetCurrentUserId());
        return NoContent();
    }

    /// <summary>
    /// Finds activities in the journal that overlap the specified time range and suggests correction options for each conflict.
    /// </summary>
    /// <param name="id">The journal ID.</param>
    /// <param name="startTime">The start of the time range to check against existing activities (UTC).</param>
    /// <param name="endTime">The end of the time range to check against existing activities (UTC).</param>
    /// <returns>A collection of conflict correction suggestions for each overlapping activity.</returns>
    /// <response code="200">Conflict suggestions successfully generated.</response>
    /// <response code="400">The provided ID or time range is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The journal was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("{id:guid}/time-correction/conflicts")]
    [ProducesResponseType(typeof(ConflictCorrectionsListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConflictCorrectionsListResponse>> GetTimeCorrectionConflicts(
        Guid id,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime)
        => Ok(await _journalService.GetTimeCorrectionConflictsAsync(id, startTime, endTime, _currentUserService.GetCurrentUserId()));
}
