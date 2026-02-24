namespace Freya.Evidence;

/// <summary>
/// Describes a single evidence file within the evidence pack.
/// </summary>
public sealed class EvidenceItem
{
    /// <summary>
    /// Logical evidence name (e.g., "SBOM", "Dependencies").
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Relative path within the evidence pack output directory.
    /// </summary>
    public required string RelativePath { get; init; }

    /// <summary>
    /// Whether the evidence file exists in the output directory.
    /// </summary>
    public required bool Exists { get; init; }

    /// <summary>
    /// The size of the evidence file in bytes. Zero if missing.
    /// </summary>
    public required long SizeBytes { get; init; }

    /// <summary>
    /// A simple status label: "ok", "missing", "empty", or "warning".
    /// </summary>
    public required string Status { get; init; }
}