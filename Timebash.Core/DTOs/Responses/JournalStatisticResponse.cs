namespace Timebash.Core.DTOs.Responses;

public record JournalStatisticResponse
(
    long TotalTimeSeconds,
    IReadOnlyList<CategoryStatItem> ByCategory
);
