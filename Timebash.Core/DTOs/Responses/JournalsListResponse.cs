namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents a response that contains a collection of journals.
/// </summary>
/// <param name="Journals">The list of journal responses.</param>
public record JournalsListResponse(IReadOnlyList<JournalResponse> Journals);
