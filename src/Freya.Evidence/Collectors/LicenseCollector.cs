using Freya.Core.IO;
using Freya.Evidence.Abstractions;

namespace Freya.Evidence.Collectors;

/// <summary>
/// Copies the license snapshot artifact into the evidence pack.
/// </summary>
public sealed class LicenseCollector : IEvidenceCollector
{
    /// <inheritdoc />
    public string Name => "License Collector";

    /// <inheritdoc />
    public async Task CollectAsync(EvidenceContext context, CancellationToken cancellationToken)
    {
        var sourceFile = Path.Combine(context.ArtifactsDirectory, "licenses", "packages.json");
        var destinationFile = Path.Combine(context.OutputDirectory, "licenses", "packages.json");

        if (!File.Exists(sourceFile))
        {
            Console.WriteLine("[Freya] No license snapshot found, skipping.");

            if (context.FailOnMissingRequiredEvidence)
            {
                throw new FileNotFoundException("Required license snapshot file was not found.", sourceFile);
            }

            return;
        }

        var size = FileCopy.GetSizeOrZero(sourceFile);
        if (size < context.MinimumJsonBytes)
        {
            Console.WriteLine($"[Freya] License snapshot found but appears empty (size={size} bytes).");

            if (context.FailOnMissingRequiredEvidence)
            {
                throw new InvalidOperationException("Required license snapshot appears empty.");
            }
        }

        await FileCopy.CopyAsync(sourceFile, destinationFile, cancellationToken).ConfigureAwait(false);
        Console.WriteLine("[Freya] Licenses copied.");
    }
}