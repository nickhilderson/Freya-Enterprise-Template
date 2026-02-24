namespace Freya.Evidence;

/// <summary>
/// Machine-readable index of evidence items produced or copied into an evidence pack.
/// </summary>
public sealed class EvidenceIndex
{
    /// <summary>
    /// When the index was generated (UTC).
    /// </summary>
    public required DateTimeOffset GeneratedAt { get; init; }

    /// <summary>
    /// The tool version that produced the evidence pack.
    /// </summary>
    public required string ToolVersion { get; init; }

    /// <summary>
    /// Evidence items included in the pack.
    /// </summary>
    public required IReadOnlyCollection<EvidenceItem> Items { get; init; }
}