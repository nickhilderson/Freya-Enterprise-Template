using System.Text.Json;
using Freya.Evidence.Abstractions;

namespace Freya.Evidence.Collectors;

/// <summary>
/// Writes CI provenance metadata into the evidence pack.
/// </summary>
public sealed class ProvenanceCollector : IEvidenceCollector
{
    /// <inheritdoc />
    public string Name => "Provenance Collector";

    /// <inheritdoc />
    public async Task CollectAsync(EvidenceContext context, CancellationToken cancellationToken)
    {
        if (context.Provenance.Count == 0)
        {
            Console.WriteLine("[Freya] No provenance metadata provided, skipping.");
            if (context.FailOnMissingRequiredEvidence)
            {
                throw new InvalidOperationException("Required provenance metadata was not provided.");
            }
            return;
        }

        var destinationDir = Path.Combine(context.OutputDirectory, "provenance");
        Directory.CreateDirectory(destinationDir);

        var destinationFile = Path.Combine(destinationDir, "provenance.json");

        var json = JsonSerializer.Serialize(context.Provenance, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(destinationFile, json, cancellationToken).ConfigureAwait(false);

        Console.WriteLine("[Freya] Provenance metadata written.");
    }
}