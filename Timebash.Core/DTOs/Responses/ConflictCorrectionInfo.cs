using Timebash.Core.DTOs.Shared;

namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents information about a time conflict with a proposed correction.
/// </summary>
/// <param name="ActivityId">The identifier of the conflicting activity.</param>
/// <param name="Name">The description of the conflicting activity.</param>
/// <param name="CurrentStartTime">The current start time of the conflicting activity (UTC).</param>
/// <param name="CurrentEndTime">The current end time of the conflicting activity (UTC).</param>
/// <param name="CurrentActivityCorrection">The suggested correction for the conflicting activity.</param>
/// <param name="AddedActivityCorrection">The suggested correction for the new activity.</param>
public record ConflictCorrectionInfo
(
    Guid ActivityId,
    string Name,
    DateTime CurrentStartTime,
    DateTime CurrentEndTime,
    CorrectionOptionBase CurrentActivityCorrection,
    CorrectionOptionBase AddedActivityCorrection
);
