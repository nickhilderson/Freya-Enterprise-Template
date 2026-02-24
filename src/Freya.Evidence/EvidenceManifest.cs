namespace Freya.Evidence;

/// <summary>
/// Metadata describing the generated evidence pack.
/// </summary>
public sealed class EvidenceManifest
{
    /// <summary>
    /// When the evidence pack was generated (UTC).
    /// </summary>
    public required DateTimeOffset GeneratedAt { get; init; }

    /// <summary>
    /// The version of the Freya evidence engine.
    /// </summary>
    public required string ToolVersion { get; init; }

    /// <summary>
    /// The collectors that ran during this evidence export.
    /// </summary>
    public required IReadOnlyCollection<string> Collectors { get; init; }
}