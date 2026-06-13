namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents the response data for an activity.
/// </summary>
/// <param name="Id">The unique identifier of the activity.</param>
/// <param name="JournalId">The identifier of the journal that contains the activity.</param>
/// <param name="Name">A short description of the activity.</param>
/// <param name="StartTime">The start time of the activity (UTC).</param>
/// <param name="EndTime">The end time of the activity (UTC).</param>
/// <param name="CreatedAt">The timestamp when the activity was created (UTC).</param>
/// <param name="UpdatedAt">The timestamp when the activity was last updated (UTC).</param>
/// <param name="Duration">The calculated duration of the activity.</param>
public record ActivityResponse
(
    Guid Id,
    Guid JournalId,
    string Name,
    DateTime StartTime,
    DateTime EndTime,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    TimeSpan Duration
);
