namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents the response data for a category.
/// </summary>
/// <param name="Id">The unique identifier of the category.</param>
/// <param name="UserId">The identifier of the user who owns the category.</param>
/// <param name="Name">The name of the category.</param>
/// <param name="Color">The display color associated with the category.</param>
/// <param name="Keywords">A collection of keywords associated with the category.</param>
/// <param name="CreatedAt">The timestamp when the category was created (UTC).</param>
/// <param name="UpdatedAt">The timestamp when the category was last updated (UTC).</param>
public record CategoryResponse
(
    Guid Id,
    Guid UserId,
    string Name,
    string Color,
    List<string> Keywords,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
