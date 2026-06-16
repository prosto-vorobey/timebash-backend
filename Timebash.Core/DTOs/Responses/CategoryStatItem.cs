namespace Timebash.Core.DTOs.Responses;

public record CategoryStatItem(
    Guid? CategoryId,
    string CategoryName,
    long TotalSeconds
);