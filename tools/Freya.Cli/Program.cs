using Freya.Evidence;
using Freya.Evidence.Abstractions;
using Freya.Evidence.Collectors;

namespace Freya.Cli;

/// <summary>
/// Freya CLI entry point.
/// </summary>
internal static class Program
{
    /// <summary>
    /// Runs the Freya CLI.
    /// </summary>
    /// <param name="args">
    /// args[0] = artifacts directory (default: ./artifacts)
    /// args[1] = output directory (default: ./evidence-pack)
    /// args[2] = strict mode (optional: "true"/"false", default: false)
    /// </param>
    private static async Task Main(string[] args)
    {
        var artifactsDir = args.Length > 0 ? args[0] : "./artifacts";
        var outputDir = args.Length > 1 ? args[1] : "./evidence-pack";
        var strict = args.Length > 2 && bool.TryParse(args[2], out var s) && s;

        var context = new EvidenceContext
        {
            ArtifactsDirectory = artifactsDir,
            OutputDirectory = outputDir,
            FailOnMissingRequiredEvidence = strict
        };

        var collectors = new List<IEvidenceCollector>
        {
            new SbomCollector(),
            new DependencyCollector(),
            new LicenseCollector(),
            
            // Summary should run last so it can index what earlier collectors produced.
            new SummaryCollector()
        };

        var builder = new EvidencePackBuilder(collectors);

        await builder.BuildAsync(context, CancellationToken.None).ConfigureAwait(false);
    }
}