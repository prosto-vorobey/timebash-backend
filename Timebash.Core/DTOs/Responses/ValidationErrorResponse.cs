namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents a validation error response.
/// </summary>
/// <param name="Title">A short description of the error.</param>
/// <param name="Status">The HTTP status code.</param>
/// <param name="Instance">The request path where the error occurred.</param>
/// <param name="TraceId">The trace identifier for the failed request.</param>
/// <param name="Errors">A dictionary of validation errors keyed by field name.</param>
public record ValidationErrorResponse(
    string Title,
    int Status,
    string Instance,
    string TraceId,
    IDictionary<string, IReadOnlyList<ValidationErrorItem>> Errors
);
