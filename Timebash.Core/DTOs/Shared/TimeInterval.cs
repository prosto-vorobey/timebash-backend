namespace Timebash.Core.DTOs.Shared;

/// <summary>
/// Represents a time interval with a start and end time.
/// </summary>
/// <param name="StartTime">The start time of the interval (UTC).</param>
/// <param name="EndTime">The end time of the interval (UTC).</param>
public record TimeInterval(DateTime StartTime, DateTime EndTime);
