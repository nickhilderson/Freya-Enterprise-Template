using Freya.Core.IO;
using Freya.Evidence.Abstractions;

namespace Freya.Evidence.Collectors;

/// <summary>
/// Copies the dependency snapshot artifact into the evidence pack.
/// </summary>
public sealed class DependencyCollector : IEvidenceCollector
{
    /// <inheritdoc />
    public string Name => "Dependency Collector";

    /// <inheritdoc />
    public async Task CollectAsync(EvidenceContext context, CancellationToken cancellationToken)
    {
        var sourceFile = Path.Combine(context.ArtifactsDirectory, "dependencies", "dependencies.json");
        var destinationFile = Path.Combine(context.OutputDirectory, "dependencies", "dependencies.json");

        if (!File.Exists(sourceFile))
        {
            var msg = "[Freya] No dependency snapshot found, skipping.";
            Console.WriteLine(msg);

            if (context.FailOnMissingRequiredEvidence)
            {
                throw new FileNotFoundException("Required dependency snapshot file was not found.", sourceFile);
            }

            return;
        }

        var size = FileCopy.GetSizeOrZero(sourceFile);
        if (size < context.MinimumJsonBytes)
        {
            var msg = $"[Freya] Dependency snapshot found but appears empty (size={size} bytes).";
            Console.WriteLine(msg);

            if (context.FailOnMissingRequiredEvidence)
            {
                throw new InvalidOperationException("Required dependency snapshot appears empty.");
            }
        }

        await FileCopy.CopyAsync(sourceFile, destinationFile, cancellationToken).ConfigureAwait(false);
        Console.WriteLine("[Freya] Dependencies copied.");
    }
}