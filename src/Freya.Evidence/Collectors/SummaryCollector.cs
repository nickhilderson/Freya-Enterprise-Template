using System.Text;
using System.Text.Json;
using Freya.Core.IO;
using Freya.Evidence.Abstractions;

namespace Freya.Evidence.Collectors;

/// <summary>
/// Generates a human-readable Markdown summary and a machine-readable JSON index
/// for the evidence pack.
/// </summary>
public sealed class SummaryCollector : IEvidenceCollector
{
    /// <inheritdoc />
    public string Name => "Summary Collector";

    /// <inheritdoc />
    public async Task CollectAsync(EvidenceContext context, CancellationToken cancellationToken)
    {
        // Known evidence locations in the output directory.
        var expected = new[]
        {
            ("SBOM", Path.Combine(context.OutputDirectory, "sbom", "bom.json")),
            ("Dependencies", Path.Combine(context.OutputDirectory, "dependencies", "dependencies.json")),
            ("Licenses", Path.Combine(context.OutputDirectory, "licenses", "packages.json")),
            ("Reproducibility Hash #1", Path.Combine(context.OutputDirectory, "..", "artifacts", "release1.sha256")), // usually not copied; kept for future
            ("Manifest", Path.Combine(context.OutputDirectory, "evidence-manifest.json"))
        };

        var items = new List<EvidenceItem>();

        foreach (var (name, fullPath) in expected)
        {
            // Only index files inside the output directory by default.
            // For external paths, mark as warning and skip unless needed later.
            if (!fullPath.StartsWith(context.OutputDirectory, StringComparison.Ordinal))
            {
                continue;
            }

            var exists = File.Exists(fullPath);
            var size = exists ? new FileInfo(fullPath).Length : 0;

            var status = exists
                ? (size < context.MinimumJsonBytes && fullPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase) ? "empty" : "ok")
                : "missing";

            items.Add(new EvidenceItem
            {
                Name = name,
                RelativePath = Paths.GetRelativePath(context.OutputDirectory, fullPath),
                Exists = exists,
                SizeBytes = size,
                Status = status
            });
        }

        var toolVersion = typeof(EvidencePackBuilder).Assembly.GetName().Version?.ToString() ?? "unknown";

        var index = new EvidenceIndex
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            ToolVersion = toolVersion,
            Items = items
        };

        await WriteIndexJsonAsync(context, index, cancellationToken).ConfigureAwait(false);
        await WriteSummaryMarkdownAsync(context, index, cancellationToken).ConfigureAwait(false);

        Console.WriteLine("[Freya] Summary generated (markdown + json).");
    }

    private static async Task WriteIndexJsonAsync(EvidenceContext context, EvidenceIndex index, CancellationToken cancellationToken)
    {
        var path = Path.Combine(context.OutputDirectory, "evidence-index.json");

        var json = JsonSerializer.Serialize(index, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(path, json, cancellationToken).ConfigureAwait(false);
    }

    private static async Task WriteSummaryMarkdownAsync(EvidenceContext context, EvidenceIndex index, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Freya Evidence Pack Summary");
        sb.AppendLine();
        sb.AppendLine($"- GeneratedAt (UTC): `{index.GeneratedAt:O}`");
        sb.AppendLine($"- ToolVersion: `{index.ToolVersion}`");
        sb.AppendLine();
        sb.AppendLine("## Evidence Items");
        sb.AppendLine();
        sb.AppendLine("| Name | Path | Exists | Size (bytes) | Status |");
        sb.AppendLine("|---|---|---:|---:|---|");

        foreach (var item in index.Items)
        {
            sb.AppendLine($"| {Escape(item.Name)} | `{Escape(item.RelativePath)}` | {(item.Exists ? "✅" : "❌")} | {item.SizeBytes} | `{Escape(item.Status)}` |");
        }

        sb.AppendLine();
        sb.AppendLine("## Notes");
        sb.AppendLine();
        sb.AppendLine("- `ok`: file exists and appears non-empty");
        sb.AppendLine("- `empty`: file exists but is too small (often `{}`)");
        sb.AppendLine("- `missing`: file not found in evidence output");

        var path = Path.Combine(context.OutputDirectory, "EvidenceSummary.md");
        await File.WriteAllTextAsync(path, sb.ToString(), cancellationToken).ConfigureAwait(false);
    }

    private static string Escape(string value)
        => value.Replace("|", "\\|");
}