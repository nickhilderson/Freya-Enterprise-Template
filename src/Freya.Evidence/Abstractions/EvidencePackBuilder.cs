using System.Text.Json;

namespace Freya.Evidence.Abstractions;

/// <summary>
/// Builds an evidence pack by executing a set of collectors and writing a manifest.
/// </summary>
public sealed class EvidencePackBuilder
{
    private readonly IReadOnlyCollection<IEvidenceCollector> _collectors;

    /// <summary>
    /// Creates a new <see cref="EvidencePackBuilder"/>.
    /// </summary>
    /// <param name="collectors">Collectors to execute in order.</param>
    public EvidencePackBuilder(IEnumerable<IEvidenceCollector> collectors)
    {
        _collectors = collectors.ToList().AsReadOnly();
    }

    /// <summary>
    /// Executes all configured collectors and writes an evidence manifest.
    /// </summary>
    /// <param name="context">Evidence context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task BuildAsync(EvidenceContext context, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(context.OutputDirectory);

        var executedCollectors = new List<string>();

        foreach (var collector in _collectors)
        {
            Console.WriteLine($"[Freya] Running collector: {collector.Name}");
            await collector.CollectAsync(context, cancellationToken).ConfigureAwait(false);
            executedCollectors.Add(collector.Name);
        }

        var manifest = new EvidenceManifest
        {
            GeneratedAt = context.GeneratedAt,
            ToolVersion = typeof(EvidencePackBuilder).Assembly.GetName().Version?.ToString() ?? "unknown",
            Collectors = executedCollectors
        };

        var manifestPath = Path.Combine(context.OutputDirectory, "evidence-manifest.json");

        var json = JsonSerializer.Serialize(manifest, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(manifestPath, json, cancellationToken).ConfigureAwait(false);

        Console.WriteLine("[Freya] Evidence pack generation complete.");
    }
}