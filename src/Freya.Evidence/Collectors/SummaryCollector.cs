using System.Text;
using System.Text.Json;
using Freya.Evidence.Abstractions;

namespace Freya.Evidence.Collectors;

/// <summary>
/// Generates a Markdown summary and machine-readable evidence index.
/// </summary>
public sealed class SummaryCollector : IEvidenceCollector
{
    /// <inheritdoc />
    public string Name => "Summary";

    /// <inheritdoc />
    public async Task CollectAsync(EvidenceContext context, CancellationToken cancellationToken)
    {
        var toolVersion = typeof(EvidencePackBuilder).Assembly.GetName().Version?.ToString() ?? "unknown";

        // First, write a summary Markdown
        var summaryPath = Path.Combine(context.OutputDirectory, "EvidenceSummary.md");
        await WriteSummaryAsync(summaryPath, cancellationToken).ConfigureAwait(false);

        // Build evidence items AFTER summary exists
        var items = new List<EvidenceItem>
        {
            CreateItem("SBOM", context, "sbom/bom.json"),
            CreateItem("Dependencies", context, "dependencies/dependencies.json"),
            CreateItem("Licenses", context, "licenses/packages.json"),
            CreateItem("Manifest", context, "evidence-manifest.json"),
            CreateItem("Summary", context, "EvidenceSummary.md"),
            CreateDirectoryItem("TestResults", context, "tests"),
            CreateDirectoryItem("Reproducibility", context, "reproducibility"),
            CreateDirectoryItem("Provenance", context, "provenance")
        };

        var index = new EvidenceIndex
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            ToolVersion = toolVersion,
            Items = items
        };

        var indexPath = Path.Combine(context.OutputDirectory, "evidence-index.json");

        var json = JsonSerializer.Serialize(index, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(indexPath, json, cancellationToken).ConfigureAwait(false);

        Console.WriteLine("[Freya] Summary and index generated.");
    }

    private static EvidenceItem CreateItem(string name, EvidenceContext context, string relativePath)
    {
        var fullPath = Path.Combine(context.OutputDirectory, relativePath);
        var exists = File.Exists(fullPath);
        var size = exists ? new FileInfo(fullPath).Length : 0;

        var status = exists
            ? (size < context.MinimumJsonBytes && fullPath.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? "empty"
                : "ok")
            : "missing";

        return new EvidenceItem
        {
            Name = name,
            RelativePath = relativePath,
            Exists = exists,
            SizeBytes = size,
            Status = status
        };
    }
    
    private static EvidenceItem CreateDirectoryItem(string name, EvidenceContext context, string relativePath)
    {
        var fullPath = Path.Combine(context.OutputDirectory, relativePath);
        var exists = Directory.Exists(fullPath);

        long size = 0;
        if (exists)
        {
            size = Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f).Length)
                .Sum();
        }

        var status = exists ? (size > 0 ? "ok" : "empty") : "missing";

        return new EvidenceItem
        {
            Name = name,
            RelativePath = relativePath,
            Exists = exists,
            SizeBytes = size,
            Status = status
        };
    }

    private static Task WriteSummaryAsync(string path, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Freya Evidence Summary");
        sb.AppendLine();
        sb.AppendLine("This evidence pack contains CI-generated compliance artifacts.");
        sb.AppendLine();

        return File.WriteAllTextAsync(path, sb.ToString(), cancellationToken);
    }
}