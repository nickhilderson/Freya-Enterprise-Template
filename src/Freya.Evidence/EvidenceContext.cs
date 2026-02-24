namespace Freya.Evidence;

/// <summary>
///     Provides configuration and runtime information for evidence collection.
/// </summary>
public sealed class EvidenceContext
{
    /// <summary>
    ///     The directory that contains CI-generated artifacts (e.g., ./artifacts).
    /// </summary>
    public required string ArtifactsDirectory { get; init; }

    /// <summary>
    ///     The directory where the evidence pack will be written.
    /// </summary>
    public required string OutputDirectory { get; init; }

    /// <summary>
    ///     The timestamp (UTC) when evidence pack generation started.
    /// </summary>
    public DateTimeOffset GeneratedAt { get; init; } = DateTimeOffset.UtcNow;

    /// <summary>
    ///     When <c>true</c>, missing or invalid required evidence will fail the run.
    ///     When <c>false</c>, collectors may skip missing evidence and continue.
    /// </summary>
    public bool FailOnMissingRequiredEvidence { get; init; } = false;

    /// <summary>
    ///     The minimum file size (bytes) considered "non-empty" for JSON evidence files.
    ///     Defaults to 2 bytes to treat "{}" as effectively empty for our purposes.
    /// </summary>
    public long MinimumJsonBytes { get; init; } = 3; // "{}" is 2 bytes; 3 means "must be more than {}"
    
    /// <summary>
    /// Optional provenance metadata to include in the evidence pack.
    /// CI typically provides this (e.g., GitHub Actions).
    /// </summary>
    public IReadOnlyDictionary<string, string> Provenance { get; init; }
        = new Dictionary<string, string>();
}