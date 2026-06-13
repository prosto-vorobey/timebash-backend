using Timebash.Core.DTOs.Shared;

namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Contains information about an activity adjustment required to resolve a scheduling conflict.
/// </summary>
/// <param name="ActivityId">The identifier of the activity being adjusted.</param>
/// <param name="Name">The name of the activity.</param>
/// <param name="CurrentStartTime">The current start time of the activity before adjustment.</param>
/// <param name="CurrentEndTime">The current end time of the activity before adjustment.</param>
/// <param name="Correction">The correction option selected for the activity.</param>
public record AdjustmentCorrectionInfo
(
    Guid ActivityId,
    string Name,
    DateTime CurrentStartTime,
    DateTime CurrentEndTime,
    CorrectionOptionBase Correction
);
