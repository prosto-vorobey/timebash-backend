namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents statistics for a specific category.
/// </summary>
/// <param name="TotalTimeSeconds">
/// The total time in seconds spent on activities that belong to this category, across all journals.
/// </param>
public record CategoryStatisticResponse
(
    long TotalTimeSeconds
);
