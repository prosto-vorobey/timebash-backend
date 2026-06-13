namespace Timebash.Core.DTOs.Responses;

/// <summary>
/// Represents a response containing a collection of conflict corrections.
/// </summary>
/// <param name="Corrections">The list of conflict corrections.</param>
public record ConflictCorrectionsListResponse(IReadOnlyList<ConflictCorrectionInfo> Corrections);
