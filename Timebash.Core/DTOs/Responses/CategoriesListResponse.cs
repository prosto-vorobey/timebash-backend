namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents a response that contains a collection of categories.
/// </summary>
/// <param name="Categories">The list of category responses.</param>
public record CategoriesListResponse(IReadOnlyList<CategoryResponse> Categories);
