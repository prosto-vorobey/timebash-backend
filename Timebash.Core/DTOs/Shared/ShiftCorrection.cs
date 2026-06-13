namespace Timebash.Core.DTOs.Shared;

/// <summary>
/// Represents a correction that shifts an activity to new start and end times.
/// </summary>
/// <param name="NewStartTime">The new start time for the activity (UTC).</param>
/// <param name="NewEndTime">The new end time for the activity (UTC).</param>
public record ShiftCorrection(DateTime NewStartTime, DateTime NewEndTime) : CorrectionOptionBase;
