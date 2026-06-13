namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents a response that contains a collection of activities.
/// </summary>
/// <param name="Activities">The list of activity responses.</param>
public record ActivitiesListResponse(IReadOnlyList<ActivityResponse> Activities);
