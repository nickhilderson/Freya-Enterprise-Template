using System.Text;

namespace Freya.Scoring.ReportWriters;

/// <summary>
/// Writes a maturity report to Markdown.
/// </summary>
public static class MarkdownReportWriter
{
    /// <summary>
    /// Writes the report to a file.
    /// </summary>
    public static Task WriteAsync(string path, MaturityReport report, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Freya Maturity Summary");
        sb.AppendLine();
        sb.AppendLine($"- GeneratedAt (UTC): `{report.GeneratedAt:O}`");
        sb.AppendLine($"- ToolVersion: `{report.ToolVersion}`");
        sb.AppendLine($"- Score: **{report.Score}/{report.MaxScore}**");
        sb.AppendLine($"- Level: **{report.Level}**");
        sb.AppendLine();

        if (report.MissingRequiredControls.Count > 0)
        {
            sb.AppendLine("## Missing Required Controls");
            sb.AppendLine();
            foreach (var c in report.MissingRequiredControls)
            {
                sb.AppendLine($"- ❌ {c}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("## Control Breakdown");
        sb.AppendLine();
        sb.AppendLine("| Control | Required | Evidence | Status | Points |");
        sb.AppendLine("|---|---:|---|---|---:|");

        foreach (var c in report.Controls)
        {
            var req = c.Required ? "✅" : "—";
            var pts = $"{c.PointsAwarded}/{c.PointsAvailable}";
            sb.AppendLine($"| {Escape(c.Name)} | {req} | `{Escape(c.EvidenceName)}` | `{Escape(c.Status)}` | {pts} |");
        }

        sb.AppendLine();
        sb.AppendLine("### Status meaning");
        sb.AppendLine();
        sb.AppendLine("- `ok`: evidence exists and appears non-empty");
        sb.AppendLine("- `empty`: evidence exists but is too small (often `{}`)");
        sb.AppendLine("- `missing`: evidence not found");
        sb.AppendLine("- `warning`: evidence exists but needs attention");

        return File.WriteAllTextAsync(path, sb.ToString(), cancellationToken);
    }

    private static string Escape(string value) => value.Replace("|", "\\|");
}