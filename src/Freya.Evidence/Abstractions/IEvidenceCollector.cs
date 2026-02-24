namespace Freya.Evidence.Abstractions;

/// <summary>
/// Represents a unit of evidence collection that copies or generates a specific artifact
/// into the evidence pack output.
/// </summary>
public interface IEvidenceCollector
{
    /// <summary>
    /// The human-readable collector name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Collects evidence into the configured output directory.
    /// </summary>
    /// <param name="context">Evidence context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task CollectAsync(EvidenceContext context, CancellationToken cancellationToken);
}