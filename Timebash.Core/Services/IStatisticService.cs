using Timebash.Core.DTOs.Responses;
using Timebash.Core.Exceptions;

namespace Timebash.Core.Services;

/// <summary>
/// Provides statistical data and analytics for user activities.
/// </summary>
public interface IStatisticService
{
    /// <summary>
    /// Returns aggregated statistics for all user's activities across all journals.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="start">Optional start date (UTC). If <c>null</c>, activities from the earliest available moment are included.</param>
    /// <param name="end">Optional end date (UTC). If <c>null</c>, activities up to the latest available moment are included.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The aggregated statistics data for the user.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="userId"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the user does not exist.</exception>
    Task<UserAggregateStatisticResponse> GetUserAggregateStatisticAsync(
        Guid userId, 
        DateTime? start, 
        DateTime? end, 
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Returns aggregated statistics for a specific journal, optionally filtered by date range.
    /// </summary>
    /// <param name="journalId">The journal ID.</param>
    /// <param name="start">Optional start date (UTC). If <c>null</c>, activities from the journal's earliest record are included.</param>
    /// <param name="end">Optional end date (UTC). If <c>null</c>, activities up to the journal's latest record are included.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The journal statistics data.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="journalId"/> or <paramref name="userId"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the journal does not exist or does not belong to the user.</exception>
    Task<JournalAggregateStatisticResponse> GetJournalAggregateStatisticAsync(
        Guid journalId, 
        DateTime? start, 
        DateTime? end, 
        Guid userId, 
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Returns statistics for a specific category, optionally filtered by date range.
    /// </summary>
    /// <param name="categoryId">The category ID.</param>
    /// <param name="start">Optional start date (UTC). If <c>null</c>, activities from the category's earliest usage are included.</param>
    /// <param name="end">Optional end date (UTC). If <c>null</c>, activities up to the category's latest usage are included.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="cancellationToken">A token to cancel the request if the client disconnects.</param>
    /// <returns>The category statistics data.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="categoryId"/> or <paramref name="userId"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the category does not exist or does not belong to the user.</exception>
    Task<CategoryStatisticResponse> GetCategoryStatisticAsync(
        Guid categoryId, 
        DateTime? start, 
        DateTime? end, 
        Guid userId, 
        CancellationToken cancellationToken);
}
