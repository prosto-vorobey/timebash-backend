namespace Timebash.Core.DTOs.Shared;

/// <summary>
/// Represents a correction that splits a conflicting activity into multiple parts.
/// </summary>
/// <param name="Parts">The time intervals defining the split parts.</param>
public record SplitCorrection(List<TimeInterval> Parts) : CorrectionOptionBase;
