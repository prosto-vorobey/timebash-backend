namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents a single validation error.
/// </summary>
/// <param name="ErrorMessage">The descriptive message explaining the validation failure.</param>
/// <param name="ErrorCode">The machine-readable error code.</param>
public record ValidationErrorItem(
    string ErrorMessage,
    string ErrorCode
);
