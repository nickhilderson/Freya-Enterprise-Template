using Freya.Evidence.Abstractions;

namespace Freya.Evidence.Collectors;

/// <summary>
/// Copies reproducibility evidence (sha256 snapshots) into the evidence pack.
/// </summary>
public sealed class ReproducibilityCollector : IEvidenceCollector
{
    /// <inheritdoc />
    public string Name => "Reproducibility Collector";

    /// <inheritdoc />
    public async Task CollectAsync(EvidenceContext context, CancellationToken cancellationToken)
    {
        var candidates = new[]
        {
            Path.Combine(context.ArtifactsDirectory, "release1.sha256"),
            Path.Combine(context.ArtifactsDirectory, "release2.sha256")
        };

        var existing = candidates.Where(File.Exists).ToList();
        if (existing.Count == 0)
        {
            Console.WriteLine("[Freya] No reproducibility sha256 files found, skipping.");
            if (context.FailOnMissingRequiredEvidence)
            {
                throw new FileNotFoundException("Required reproducibility evidence not found (release1.sha256/release2.sha256).");
            }
            return;
        }

        var destinationDir = Path.Combine(context.OutputDirectory, "reproducibility");
        Directory.CreateDirectory(destinationDir);

        foreach (var file in existing)
        {
            var dest = Path.Combine(destinationDir, Path.GetFileName(file));
            await using var src = File.OpenRead(file);
            await using var dst = File.Create(dest);
            await src.CopyToAsync(dst, cancellationToken).ConfigureAwait(false);
        }

        Console.WriteLine($"[Freya] Reproducibility evidence copied ({existing.Count} file(s)).");
    }
}