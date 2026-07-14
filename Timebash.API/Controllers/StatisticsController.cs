namespace Timebash.API.Controllers;

/// <summary>
/// Manages user activity statistics.
/// </summary>
[Authorize]
[ApiController]
[Route("api/statistics")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StatisticsController(ICurrentUserService currentUserService, IStatisticService statisticService) : ControllerBase
{
    private readonly ICurrentUserService _currentUserService = currentUserService;
    private readonly IStatisticService _statisticService = statisticService;

    /// <summary>
    /// Retrieves aggregated statistics for all user's activitities across all journals.
    /// </summary>
    /// <param name="startTime">Optional start date (UTC). If <c>null</c>, activities from the earliest available moment are included.</param>
    /// <param name="endTime">Optional end date (UTC). If <c>null</c>, activities up to the latest available moment are included.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The aggregated statistics data for the user.</returns>
    /// <response code="200">Statistics were successfully retrieved.</response>
    /// <response code="400">The provided time range is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The user was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("me/aggregate")]
    [ProducesResponseType(typeof(UserAggregateStatisticResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserAggregateStatisticResponse>> GetUserAggregateStatistic(
        DateTime? startTime = null, 
        DateTime? endTime = null, 
        CancellationToken cancellationToken = default)
        => Ok(await _statisticService.GetUserAggregateStatisticAsync(_currentUserService.GetCurrentUserId(), startTime, endTime, cancellationToken));

    /// <summary>
    /// Retrieves aggregated statistics for a specific journal, optionally filtered by date range.
    /// </summary>
    /// <param name="id">The journal ID.</param>
    /// <param name="startTime">Optional start date (UTC). If <c>null</c>, activities from the journal's earliest record are included.</param>
    /// <param name="endTime">Optional end date (UTC). If <c>null</c>, activities up to the journal's latest record are included.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The journal statistics data.</returns>
    /// <response code="200">Statistics were successfully retrieved.</response>
    /// <response code="400">The provided ID or time is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The journal was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("journal/{id:guid}")]
    [ProducesResponseType(typeof(JournalAggregateStatisticResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<JournalAggregateStatisticResponse>> GetJournalAggregateStatistic(
        Guid id, 
        DateTime? startTime = null, 
        DateTime? endTime = null, 
        CancellationToken cancellationToken = default)
        => Ok(await _statisticService.GetJournalAggregateStatisticAsync(
            id, 
            startTime, 
            endTime, 
            _currentUserService.GetCurrentUserId(), 
            cancellationToken));
    
    /// <summary>
    /// Retrieves statistics for a specific category, optionally filtered by date range.
    /// </summary>
    /// <param name="id">The category ID.</param>
    /// <param name="startTime">Optional start date (UTC). If <c>null</c>, activities from the category's earliest usage are included.</param>
    /// <param name="endTime">Optional end date (UTC). If <c>null</c>, activities up to the category's latest usage are included.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The category statistics data.</returns>
    /// <response code="200">Statistics were successfully retrieved.</response>
    /// <response code="400">The provided ID or time is invalid.</response>
    /// <response code="401">Authentication is required.</response>
    /// <response code="404">The category was not found.</response>
    /// <response code="500">Internal server error. Check logs for details.</response>
    [HttpGet("category/{id:guid}")]
    [ProducesResponseType(typeof(CategoryStatisticResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryStatisticResponse>> GetCategoryStatistic(
        Guid id, 
        DateTime? startTime = null, 
        DateTime? endTime = null, 
        CancellationToken cancellationToken = default)
        =>  Ok(await _statisticService.GetCategoryStatisticAsync(id, startTime, endTime, _currentUserService.GetCurrentUserId(), cancellationToken));
}
