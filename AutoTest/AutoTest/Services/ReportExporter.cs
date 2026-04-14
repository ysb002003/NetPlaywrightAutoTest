using System.Net;
using System.Text;
using AutoTest.Models;

namespace AutoTest.Services;

public static class ReportExporter
{
    public static void ExportHtml(IReadOnlyList<TestCaseExecutionResult> results, string outputPath)
    {
        var dir = Path.GetDirectoryName(Path.GetFullPath(outputPath));
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var passed = results.Count(r => r.Status == "Passed");
        var failed = results.Count - passed;
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html><html><head><meta charset=\"utf-8\"/><title>AutoTest Report</title>");
        sb.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:24px;} table{border-collapse:collapse;width:100%;} th,td{border:1px solid #ccc;padding:8px;text-align:left;} th{background:#f0f0f0;} .pass{color:green;} .fail{color:#c00;}</style>");
        sb.AppendLine("</head><body>");
        sb.AppendLine("<h1>AutoTest Report</h1>");
        sb.AppendLine($"<p>Generated: {WebUtility.HtmlEncode(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}</p>");
        sb.AppendLine($"<p><strong>Total:</strong> {results.Count} &nbsp; <span class=\"pass\">Passed: {passed}</span> &nbsp; <span class=\"fail\">Failed: {failed}</span></p>");
        sb.AppendLine("<table><thead><tr><th>Case</th><th>Status</th><th>Duration (ms)</th><th>Message</th><th>Expected</th><th>Actual</th><th>Screenshot</th><th>Video</th></tr></thead><tbody>");

        foreach (var r in results)
        {
            sb.AppendLine("<tr>");
            sb.AppendLine($"<td>{WebUtility.HtmlEncode(r.CaseName)}</td>");
            sb.AppendLine($"<td class=\"{(r.Status == "Passed" ? "pass" : "fail")}\">{WebUtility.HtmlEncode(r.Status)}</td>");
            sb.AppendLine($"<td>{r.DurationMs}</td>");
            sb.AppendLine($"<td>{WebUtility.HtmlEncode(r.Message)}</td>");
            sb.AppendLine($"<td>{WebUtility.HtmlEncode(r.ExpectedResult)}</td>");
            sb.AppendLine($"<td>{WebUtility.HtmlEncode(r.ActualResult)}</td>");
            sb.AppendLine($"<td>{LinkOrDash(r.ScreenshotPath)}</td>");
            sb.AppendLine($"<td>{LinkOrDash(r.VideoPath)}</td>");
            sb.AppendLine("</tr>");
        }

        sb.AppendLine("</tbody></table></body></html>");
        File.WriteAllText(outputPath, sb.ToString(), Encoding.UTF8);
    }

    private static string LinkOrDash(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return "-";
        }

        var full = Path.GetFullPath(path);
        var uri = new Uri(full).AbsoluteUri;
        var name = WebUtility.HtmlEncode(Path.GetFileName(path));
        return $"<a href=\"{WebUtility.HtmlEncode(uri)}\">{name}</a>";
    }
}
