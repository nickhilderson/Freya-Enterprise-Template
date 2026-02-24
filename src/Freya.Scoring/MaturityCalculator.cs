using System.Text.Json;
using Freya.Evidence;

namespace Freya.Scoring;

/// <summary>
/// Calculates maturity scores based on an evidence index and a scoring configuration.
/// </summary>
public sealed class MaturityCalculator
{
    /// <summary>
    /// Calculates a maturity report.
    /// </summary>
    /// <param name="evidenceIndexPath">Path to evidence-index.json.</param>
    /// <param name="configPath">Path to maturity-controls.json.</param>
    public MaturityReport Calculate(string evidenceIndexPath, string configPath)
    {
        if (!File.Exists(evidenceIndexPath))
        {
            throw new FileNotFoundException("Evidence index not found.", evidenceIndexPath);
        }

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException("Maturity config not found.", configPath);
        }

        var indexJson = File.ReadAllText(evidenceIndexPath);
        var index = JsonSerializer.Deserialize<EvidenceIndex>(indexJson)
                   ?? throw new InvalidOperationException("Failed to deserialize evidence index.");

        var cfgJson = File.ReadAllText(configPath);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
        };

        MaturityConfig cfg;
        try
        {
            cfg = JsonSerializer.Deserialize<MaturityConfig>(cfgJson, jsonOptions)
                  ?? throw new InvalidOperationException("Failed to deserialize maturity config.");
        }
        catch (JsonException je)
        {
            throw new InvalidOperationException($"Unable to parse maturity config '{configPath}': {je.Message}", je);
        }
        
        var results = new List<ControlResult>();
        var missingRequired = new List<string>();

        foreach (var control in cfg.Controls)
        {
            var evidence = index.Items.FirstOrDefault(i => string.Equals(i.Name, control.EvidenceName, StringComparison.Ordinal));
            var status = evidence?.Status ?? "missing";

            // Achieved policy: only "ok" counts as achieved.
            var achieved = string.Equals(status, "ok", StringComparison.Ordinal);

            var awarded = achieved ? control.Points : 0;

            if (control.Required && !achieved)
            {
                missingRequired.Add(control.Name);
            }

            results.Add(new ControlResult
            {
                Id = control.Id,
                Name = control.Name,
                EvidenceName = control.EvidenceName,
                Required = control.Required,
                PointsAvailable = control.Points,
                PointsAwarded = awarded,
                Status = status,
                Achieved = achieved
            });
        }

        var score = results.Sum(r => r.PointsAwarded);
        var max = results.Sum(r => r.PointsAvailable);

        var level = cfg.GetLevelForScore(score);

        return new MaturityReport
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            ToolVersion = index.ToolVersion,
            Score = score,
            MaxScore = max,
            Level = level,
            Controls = results.AsReadOnly(),
            MissingRequiredControls = missingRequired.AsReadOnly()
        };
    }

    private sealed class MaturityConfig
    {
        public required IReadOnlyCollection<LevelDefinition> Levels { get; init; }
        public required IReadOnlyCollection<ControlDefinition> Controls { get; init; }

        public string GetLevelForScore(int score)
        {
            // Highest minScore <= score wins
            var best = Levels
                .OrderByDescending(l => l.MinScore)
                .FirstOrDefault(l => score >= l.MinScore);

            return best?.Name ?? "Level 1 - Basic CI";
        }
    }

    private sealed class LevelDefinition
    {
        public required string Name { get; init; }
        public required int MinScore { get; init; }
    }
}