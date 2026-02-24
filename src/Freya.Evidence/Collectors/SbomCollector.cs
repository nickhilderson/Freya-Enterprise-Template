using Freya.Core.IO;
using Freya.Evidence.Abstractions;

namespace Freya.Evidence.Collectors;

/// <summary>
/// Copies the CycloneDX SBOM artifact into the evidence pack.
/// </summary>
public sealed class SbomCollector : IEvidenceCollector
{
    /// <inheritdoc />
    public string Name => "SBOM Collector";

    /// <inheritdoc />
    public async Task CollectAsync(EvidenceContext context, CancellationToken cancellationToken)
    {
        var sourceFile = Path.Combine(context.ArtifactsDirectory, "sbom", "bom.json");
        var destinationFile = Path.Combine(context.OutputDirectory, "sbom", "bom.json");

        if (!File.Exists(sourceFile))
        {
            var msg = "[Freya] No SBOM found, skipping.";
            Console.WriteLine(msg);

            if (context.FailOnMissingRequiredEvidence)
            {
                throw new FileNotFoundException("Required SBOM file was not found.", sourceFile);
            }

            return;
        }

        var size = FileCopy.GetSizeOrZero(sourceFile);
        if (size < context.MinimumJsonBytes)
        {
            var msg = $"[Freya] SBOM found but appears empty (size={size} bytes).";
            Console.WriteLine(msg);

            if (context.FailOnMissingRequiredEvidence)
            {
                throw new InvalidOperationException("Required SBOM file appears empty.");
            }

            // Still copy for evidence, but mark it in logs.
        }

        await FileCopy.CopyAsync(sourceFile, destinationFile, cancellationToken).ConfigureAwait(false);
        Console.WriteLine("[Freya] SBOM copied.");
    }
}