namespace Freya.Scoring;

/// <summary>
/// Defines a maturity control and how it maps to evidence.
/// </summary>
public sealed class ControlDefinition
{
    /// <summary>
    /// A stable identifier for the control (e.g., "sbom").
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Human-readable control name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Evidence item name as produced in the evidence index (e.g., "SBOM").
    /// </summary>
    public required string EvidenceName { get; init; }

    /// <summary>
    /// Whether this control is required for a compliant baseline.
    /// </summary>
    public required bool Required { get; init; }

    /// <summary>
    /// Points awarded when the control is achieved.
    /// </summary>
    public required int Points { get; init; }
}