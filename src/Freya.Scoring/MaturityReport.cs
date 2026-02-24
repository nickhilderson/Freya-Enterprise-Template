namespace Freya.Scoring;

/// <summary>
/// Represents a maturity evaluation report.
/// </summary>
public sealed class MaturityReport
{
    /// <summary>
    /// Time when the report was generated (UTC).
    /// </summary>
    public required DateTimeOffset GeneratedAt { get; init; }

    /// <summary>
    /// Evidence tool version used.
    /// </summary>
    public required string ToolVersion { get; init; }

    /// <summary>
    /// Total achieved score.
    /// </summary>
    public required int Score { get; init; }

    /// <summary>
    /// Maximum possible score based on config.
    /// </summary>
    public required int MaxScore { get; init; }

    /// <summary>
    /// The derived maturity level.
    /// </summary>
    public required string Level { get; init; }

    /// <summary>
    /// Individual control results.
    /// </summary>
    public required IReadOnlyCollection<ControlResult> Controls { get; init; }

    /// <summary>
    /// Any required controls that are missing/failed.
    /// </summary>
    public required IReadOnlyCollection<string> MissingRequiredControls { get; init; }
}