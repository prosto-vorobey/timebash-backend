namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a request containing a collection of category IDs.
/// </summary>
/// <param name="CategoryIds">The list of category identifiers.</param>
public record ActivityCategoriesRequest(List<Guid> CategoryIds);
