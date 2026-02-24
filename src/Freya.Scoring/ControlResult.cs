namespace Freya.Scoring;

/// <summary>
/// The result of evaluating a single control against evidence.
/// </summary>
public sealed class ControlResult
{
    /// <summary>
    /// Control identifier.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Control name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The mapped evidence name.
    /// </summary>
    public required string EvidenceName { get; init; }

    /// <summary>
    /// Whether the control is required.
    /// </summary>
    public required bool Required { get; init; }

    /// <summary>
    /// Points available for this control.
    /// </summary>
    public required int PointsAvailable { get; init; }

    /// <summary>
    /// Points awarded after evaluation.
    /// </summary>
    public required int PointsAwarded { get; init; }

    /// <summary>
    /// Evidence status ("ok", "empty", "missing", "warning").
    /// </summary>
    public required string Status { get; init; }

    /// <summary>
    /// True when the control is considered achieved.
    /// </summary>
    public required bool Achieved { get; init; }
}