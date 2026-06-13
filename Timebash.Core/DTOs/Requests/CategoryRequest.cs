namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a request to create or update a category.
/// </summary>
/// <param name="Name">The name of the category.</param>
/// <param name="Color">The display color associated with the category.</param>
/// <param name="Keywords">A collection of keywords linked to the category.</param>
public record CategoryRequest
(
    string Name,
    string Color,
    List<string>? Keywords
);
