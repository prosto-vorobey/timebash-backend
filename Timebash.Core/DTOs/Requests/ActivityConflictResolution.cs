using Timebash.Core.DTOs.Shared;

namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a conflict resolution request for a specific activity.
/// </summary>
/// <param name="ActivityId">The identifier of the activity to correct.</param>
/// <param name="Correction">The correction strategy to apply to the activity.</param>
public record ActivityConflictResolution
(
    Guid ActivityId,
    CorrectionOptionBase Correction
);
