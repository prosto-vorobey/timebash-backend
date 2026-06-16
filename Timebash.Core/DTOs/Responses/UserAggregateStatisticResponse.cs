namespace Timebash.Core.DTOs.Responses;

public record UserAggregateStatisticResponse
(
    long TotalTimeSeconds,
    IReadOnlyList<CategoryStatItem> ByCategory
);
