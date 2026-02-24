using Freya.Evidence.Abstractions;

namespace Freya.Evidence.Collectors;

/// <summary>
/// Copies test result artifacts (TRX) into the evidence pack.
/// </summary>
public sealed class TestResultsCollector : IEvidenceCollector
{
    /// <inheritdoc />
    public string Name => "TestResults Collector";

    /// <inheritdoc />
    public async Task CollectAsync(EvidenceContext context, CancellationToken cancellationToken)
    {
        var sourceDir = Path.Combine(context.ArtifactsDirectory, "test-results");
        if (!Directory.Exists(sourceDir))
        {
            Console.WriteLine("[Freya] No test results directory found, skipping.");
            if (context.FailOnMissingRequiredEvidence)
            {
                throw new DirectoryNotFoundException($"Required test results directory not found: {sourceDir}");
            }
            return;
        }

        var trxFiles = Directory.EnumerateFiles(sourceDir, "*.trx", SearchOption.AllDirectories).ToList();
        if (trxFiles.Count == 0)
        {
            Console.WriteLine("[Freya] No TRX files found, skipping.");
            if (context.FailOnMissingRequiredEvidence)
            {
                throw new InvalidOperationException("Required test results are missing (.trx not found).");
            }
            return;
        }

        var destinationDir = Path.Combine(context.OutputDirectory, "tests");
        Directory.CreateDirectory(destinationDir);

        foreach (var file in trxFiles)
        {
            var dest = Path.Combine(destinationDir, Path.GetFileName(file));
            await using var src = File.OpenRead(file);
            await using var dst = File.Create(dest);
            await src.CopyToAsync(dst, cancellationToken).ConfigureAwait(false);
        }

        Console.WriteLine($"[Freya] Test results copied ({trxFiles.Count} file(s)).");
    }
}