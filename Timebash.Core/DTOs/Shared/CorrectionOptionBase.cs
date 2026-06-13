using System.Text.Json.Serialization;

namespace Timebash.Core.DTOs.Shared;

/// <summary>
/// Represents a base type for strategies that resolve time conflicts.
/// </summary>
[JsonPolymorphic]
[JsonDerivedType(typeof(ShiftCorrection), "shift")]
[JsonDerivedType(typeof(SplitCorrection), "split")]
[JsonDerivedType(typeof(DeleteCorrection), "delete")]
public abstract record CorrectionOptionBase();
