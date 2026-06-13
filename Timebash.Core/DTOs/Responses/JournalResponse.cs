namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents the response data for a journal.
/// </summary>
/// <param name="Id">The unique identifier of the journal.</param>
/// <param name="UserId">The identifier of the user who owns the journal.</param>
/// <param name="Name">The name of the journal.</param>
/// <param name="CreatedAt">The timestamp when the journal was created (UTC).</param>
/// <param name="UpdatedAt">The timestamp when the journal was last updated (UTC).</param>
public record JournalResponse
(
    Guid Id,
    Guid UserId,
    string Name,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
