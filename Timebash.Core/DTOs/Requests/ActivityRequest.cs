namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a request to create or update an activity.
/// </summary>
/// <param name="JournalId">The ID of the journal to which the activity belongs.</param>
/// <param name="Name">A short description of the activity.</param>
/// <param name="StartTime">The start time of the activity (UTC).</param>
/// <param name="EndTime">The end time of the activity (UTC).</param>
/// <param name="CategoryIds">A list of category IDs associated with the activity.</param>
public record ActivityRequest
(
    Guid JournalId,
    string Name,
    DateTime StartTime,
    DateTime EndTime,
    List<Guid> CategoryIds
);
