namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents statistics for a specific journal.
/// </summary>
/// <param name="TotalTimeSeconds">
/// The total time in seconds spent on all activities within this journal.
/// </param>
/// <param name="ByCategory">
/// A collection of <see cref="CategoryStatItem"/> objects, each representing the total time for a category within this journal.
/// </param>
public record JournalStatisticResponse
(
    long TotalTimeSeconds,
    IReadOnlyList<CategoryStatItem> ByCategory
);
