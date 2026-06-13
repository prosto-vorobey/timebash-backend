namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a request to update the current user's default journal.
/// </summary>
/// <param name="JournalId">The ID of the journal to set as default..</param>
public record DefaultJournalUpdateRequest
(
    Guid JournalId
);
