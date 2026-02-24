using Freya.Evidence;
using Freya.Evidence.Abstractions;
using Freya.Evidence.Collectors;
using Freya.Scoring;
using Freya.Scoring.ReportWriters;

namespace Freya.Cli;

/// <summary>
/// Freya CLI entry point.
/// </summary>
internal static class Program
{
    static string? GetEnv(string name)
        => Environment.GetEnvironmentVariable(name);
    
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
        
        var provenance = new Dictionary<string, string>();

        void AddIfSet(string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                provenance[key] = value!;
            }
        }

        AddIfSet("repo", GetEnv("FREYA_REPO"));
        AddIfSet("sha", GetEnv("FREYA_SHA"));
        AddIfSet("ref", GetEnv("FREYA_REF"));
        AddIfSet("run_id", GetEnv("FREYA_RUN_ID"));
        AddIfSet("run_attempt", GetEnv("FREYA_RUN_ATTEMPT"));
        AddIfSet("actor", GetEnv("FREYA_ACTOR"));
        AddIfSet("workflow", GetEnv("FREYA_WORKFLOW"));


        var context = new EvidenceContext
        {
            ArtifactsDirectory = artifactsDir,
            OutputDirectory = outputDir,
            FailOnMissingRequiredEvidence = strict,
            Provenance = provenance
        };
        var collectors = new List<IEvidenceCollector>
        {
            new SbomCollector(),
            new DependencyCollector(),
            new LicenseCollector(),
            new TestResultsCollector(),
            new ReproducibilityCollector(),
            new ProvenanceCollector(),
            
            // Summary should run last so it can index what earlier collectors produced.
            new SummaryCollector()
        };

        var builder = new EvidencePackBuilder(collectors);

        await builder.BuildAsync(context, CancellationToken.None).ConfigureAwait(false);
        
        // Generate a maturity report based on the evidence index
        var indexPath = Path.Combine(outputDir, "evidence-index.json");
        var configPath = Path.Combine("build", "maturity-controls.json");

        var calculator = new MaturityCalculator();
        var report = calculator.Calculate(indexPath, configPath);

        await JsonReportWriter.WriteAsync(Path.Combine(outputDir, "maturity-report.json"), report, CancellationToken.None)
            .ConfigureAwait(false);

        await MarkdownReportWriter.WriteAsync(Path.Combine(outputDir, "MaturitySummary.md"), report, CancellationToken.None)
            .ConfigureAwait(false);

        Console.WriteLine($"[Freya] Maturity: {report.Score}/{report.MaxScore} - {report.Level}");
    }
}