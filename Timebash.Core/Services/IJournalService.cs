using Timebash.Core.DTOs.Requests;
using Timebash.Core.DTOs.Responses;
using Timebash.Core.Exceptions;

namespace Timebash.Core.Services;

/// <summary>
/// Provides operations for managing journals.
/// </summary>
public interface IJournalService
{
    /// <summary>
    /// Retrieves a journal by its identifier.
    /// </summary>
    /// <param name="id">The journal ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>The journal data.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the journal does not exist or does not belong to the user.</exception>
    Task<JournalResponse> GetByIdAsync(Guid id, Guid userId);

    /// <summary>
    /// Returns activities for a journal, optionally filtered by date.
    /// </summary>
    /// <param name="id">The journal ID.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <param name="date">
    /// Optional date (UTC) to filter activities. If <c>null</c>, all activities are returned.
    /// If provided, only activities that occurred on that specific day (in the timezone specified by <paramref name="offsetMinutes"/>) are included.
    /// </param>
    /// <param name="offsetMinutes">
    /// Optional timezone offset in minutes. If <c>null</c>, UTC (0) is used.
    /// This offset is applied to <paramref name="date"/> to determine the start and end of the day in the user's local time.
    /// </param>
    /// <returns>A collection of activities ordered by start time.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the journal does not exist or does not belong to the user.</exception>
    Task<ActivitiesListResponse> GetActivitiesByJournalIdAsync(Guid id, Guid userId, DateTime? date = null, int? offsetMinutes = null);

    /// <summary>
    /// Creates a new journal.
    /// </summary>
    /// <param name="journalRequest">The journal data.</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>The newly created journal.</returns>
    Task<JournalResponse> CreateAsync(JournalRequest journalRequest, Guid userId);

    /// <summary>
    /// Replaces an existing journal with the provided data.
    /// </summary>
    /// <param name="id">The journal ID.</param>
    /// <param name="journalRequest">The new journal data.</param>
    /// <param name="userId">The owner user ID to update.</param>
    /// <returns>True if any changes were applied, otherwise false.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the journal does not exist or does not belong to the user.</exception>
    Task<bool> UpdateAsync(Guid id, JournalRequest journalRequest, Guid userId);

    /// <summary>
    /// Deletes the specified journal.
    /// </summary>
    /// <param name="id">The journal ID.</param>
    /// <param name="userId">The owner user ID to delete.</param>
    /// <returns>A task representing the asynchronous delete operation.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the journal does not exist or does not belong to the user.</exception>
    /// <exception cref="ConflictException">Thrown when <paramref name="id"/> belongs to the default journal.</exception>
    Task DeleteAsync(Guid id, Guid userId);

    /// <summary>
    /// Finds activities in the journal that overlap the specified time range and suggests correction options for each conflict.
    /// </summary>
    /// <param name="id">The journal ID.</param>
    /// <param name="startTime">The start of the time range to check against existing activities (UTC).</param>
    /// <param name="endTime">The end of the time range to check against existing activities (UTC).</param>
    /// <param name="userId">The owner user ID.</param>
    /// <returns>A collection of conflict correction suggestions.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="id"/> is empty.</exception>
    /// <exception cref="NotFoundException">Thrown when the journal does not exist or does not belong to the user.</exception>
    Task<ConflictCorrectionsListResponse> GetTimeCorrectionConflictsAsync(Guid id, DateTime startTime, DateTime endTime, Guid userId);
}
