namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents the response data after creating an activity with time correction.
/// </summary>
/// <param name="MainActivity">The primary activity that was created.</param>
/// <param name="AdditionalActivities">Additional activities generated as a result of time correction.</param>
public record ActivityWithCorrectionResponse(
    ActivityResponse MainActivity,
    IEnumerable<ActivityResponse> AdditionalActivities
);
