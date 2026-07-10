namespace Timebash.Core.Services;

/// <summary>
/// Provides read-only queries for retrieving activity data.
/// </summary>
public interface IActivityQueryService
{
    /// <summary>
    /// Returns a stream of all activities belonging to the specified user, optionally filtered by date range.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="start">Optional start date (UTC). If <c>null</c>, activities from the earliest available moment are included.</param>
    /// <param name="end">Optional end date (UTC). If <c>null</c>, activities up to the latest available moment are included.</param>
    /// <returns>
    /// An asynchronous stream of <see cref="Activity"/> objects with their associated categories populated.
    /// </returns>
    public IAsyncEnumerable<Activity> GetActivitiesForUserAsync(Guid userId, DateTime? start, DateTime? end);

    /// <summary>
    /// Returns a stream of all activities in a specific journal, optionally filtered by date range.
    /// </summary>
    /// <param name="journalId">The journal ID.</param>
    /// <param name="start">Optional start date (UTC). If <c>null</c>, activities from the journal's earliest record are included.</param>
    /// <param name="end">Optional end date (UTC). If <c>null</c>, activities up to the journal's latest record are included.</param>
    /// <param name="filterMode">Specifies how activities are filtered by date range.</param>
    /// <returns>
    /// An asynchronous stream of <see cref="Activity"/> objects with their associated categories populated.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="filterMode"/> is not a valid enum value.</exception>
    public IAsyncEnumerable<Activity> GetActivitiesForJournalAsync(Guid journalId, DateTime? start, DateTime? end, ActivityDateFilterMode filterMode);

    /// <summary>
    /// Returns a stream of all activities that belong to a specific category, optionally filtered by date range.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <param name="start">Optional start date (UTC). If <c>null</c>, activities from the category's earliest usage are included.</param>
    /// <param name="end">Optional end date (UTC). If <c>null</c>, activities up to the category's latest usage are included.</param>
    /// <returns>
    /// An asynchronous stream of <see cref="Activity"/> objects (categories are not pre-loaded, as filtering is done by category).
    /// </returns>
    public IAsyncEnumerable<Activity> GetActivitiesForCategoryAsync(Guid categoryId, DateTime? start, DateTime? end);
}
