namespace Timebash.Core.DTOs.Requests;

/// <summary>
/// Represents a request to create or update a journal.
/// </summary>
/// <param name="Name">The name of the journal.</param>
public record JournalRequest
(
    string Name
);
