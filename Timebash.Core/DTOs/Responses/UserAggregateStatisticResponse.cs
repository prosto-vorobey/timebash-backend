namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents aggregated statistics for all journals of a user.
/// </summary>
/// <param name="TotalTimeSeconds">
/// The total time in seconds spent across all activities of the user.
/// </param>
/// <param name="ByCategory">
/// A collection of <see cref="CategoryStatItem"/> objects, each representing the total time for a specific category.
/// </param>
public record UserAggregateStatisticResponse
(
    long TotalTimeSeconds,
    IReadOnlyList<CategoryStatItem> ByCategory
);
