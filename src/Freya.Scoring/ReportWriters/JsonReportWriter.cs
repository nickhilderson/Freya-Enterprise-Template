using System.Text.Json;

namespace Freya.Scoring.ReportWriters;

/// <summary>
/// Writes a maturity report to JSON.
/// </summary>
public static class JsonReportWriter
{
    /// <summary>
    /// Writes the report to a file.
    /// </summary>
    public static Task WriteAsync(string path, MaturityReport report, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(report, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return File.WriteAllTextAsync(path, json, cancellationToken);
    }
}