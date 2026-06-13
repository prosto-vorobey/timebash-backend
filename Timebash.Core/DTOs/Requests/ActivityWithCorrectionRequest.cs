using Timebash.Core.DTOs.Shared;

namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a request to create an activity with automatic time conflict resolution.
/// </summary>
/// <param name="JournalId">The ID of the journal to which the activity belongs.</param>
/// <param name="Name">A short description of the activity.</param>
/// <param name="StartTime">The start time of the activity (UTC).</param>
/// <param name="EndTime">The end time of the activity (UTC).</param>
/// <param name="CategoryIds">A list of category IDs associated with the activity.</param>
/// <param name="ConflictResolutions">A list of conflict resolutions to apply to existing activities.</param>
/// <param name="AdditionalPartIntervals">>Additional time intervals into which the new activity will be split.</param>
public record ActivityWithCorrectionRequest
(
    Guid JournalId,
    string Name,
    DateTime StartTime,
    DateTime EndTime,
    List<Guid> CategoryIds,
    List<ActivityConflictResolution> ConflictResolutions,
    List<TimeInterval> AdditionalPartIntervals
);
