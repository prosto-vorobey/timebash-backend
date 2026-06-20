using Timebash.Core.DTOs.Responses;

namespace Timebash.Core.Services;

/// <summary>
/// Provides aggregated statistical data for user activities.
/// </summary>
public interface IStatisticsQueryService
{
    /// <summary>
    /// Returns aggregated statistics for all user's activities across all journals.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="start">Optional start date (UTC). If <c>null</c>, activities from the earliest available moment are included.</param>
    /// <param name="end">Optional end date (UTC). If <c>null</c>, activities up to the latest available moment are included.</param>
    /// <returns>The aggregated statistics data for the user.</returns>
    public Task<UserAggregateStatisticResponse> GetUserStatisticsAsync(Guid userId, DateTime? start, DateTime? end);

    /// <summary>
    /// Returns statistics for a specific journal, optionally filtered by date range.
    /// </summary>
    /// <param name="journalId">The journal ID.</param>
    /// <param name="start">Optional start date (UTC). If <c>null</c>, activities from the journal's earliest record are included.</param>
    /// <param name="end">Optional end date (UTC). If <c>null</c>, activities up to the journal's latest record are included.</param>
    /// <returns>The journal statistics data.</returns>
    public Task<JournalStatisticResponse> GetJournalStatisticsAsync(Guid journalId, DateTime? start, DateTime? end);

    /// <summary>
    /// Returns statistics for a specific category, optionally filtered by date range.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <param name="start">Optional start date (UTC). If <c>null</c>, activities from the category's earliest usage are included.</param>
    /// <param name="end">Optional end date (UTC). If <c>null</c>, activities up to the category's latest usage are included.</param>
    /// <returns>The category statistics data.</returns>
    public Task<CategoryStatisticResponse> GetCategoryStatisticsAsync(Guid categoryId, DateTime? start, DateTime? end);
}
