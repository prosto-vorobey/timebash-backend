namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents a statistic for a single category.
/// </summary>
/// <param name="CategoryId">
/// The unique identifier of the category.
/// <c>null</c> for activities that do not belong to any category.
/// </param>
/// <param name="CategoryName">
/// The name of the category. If <paramref name="CategoryId"/> is <c>null</c>, this value should be "Uncategorized" (or a localized equivalent).
/// </param>
/// <param name="TotalSeconds">The total time in seconds spent on activities in this category.</param>
public record CategoryStatItem
(
    Guid? CategoryId,
    string CategoryName,
    long TotalSeconds
);