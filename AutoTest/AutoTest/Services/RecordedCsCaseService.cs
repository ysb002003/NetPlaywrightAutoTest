using System.Text;
using System.Text.RegularExpressions;
using AutoTest.Models;

namespace AutoTest.Services;

public sealed class RecordedCsCaseService
{
    public string GetCasesDirectory()
    {
        var dir = Path.Combine(AppContext.BaseDirectory, "TestCasesCs");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public void SaveFromCodegenText(string content, string outputFilePath)
    {
        var steps = ExtractAwaitLines(content);
        if (steps.Count == 0)
        {
            throw new InvalidOperationException("No 'await page....' lines found in recorded content.");
        }

        var className = BuildSafeIdentifier(Path.GetFileNameWithoutExtension(outputFilePath));
        var source = BuildCaseSource(className, steps);
        var dir = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrWhiteSpace(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(outputFilePath, source, Encoding.UTF8);
    }

    public IReadOnlyList<TestCaseItem> LoadCases()
    {
        var dir = GetCasesDirectory();
        var files = Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();
        var list = new List<TestCaseItem>();
        foreach (var file in files)
        {
            var raw = File.ReadAllText(file);
            var steps = ExtractAwaitLines(raw);
            if (steps.Count == 0)
            {
                continue;
            }

            list.Add(new TestCaseItem(
                Path.GetFileNameWithoutExtension(file),
                "Recorded Playwright .cs case",
                string.Join(Environment.NewLine, steps),
                string.Empty,
                string.Empty,
                string.Empty,
                string.Empty,
                "RecordedCs",
                string.Empty));
        }

        return list;
    }

    private static List<string> ExtractAwaitLines(string source)
    {
        return source
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => x.StartsWith("await page.", StringComparison.Ordinal))
            .ToList();
    }

    private static string BuildCaseSource(string className, IReadOnlyList<string> steps)
    {
        var sb = new StringBuilder();
        sb.AppendLine("using AutoTest.Models;");
        sb.AppendLine("using Microsoft.Playwright;");
        sb.AppendLine();
        sb.AppendLine("namespace AutoTest.RecordedCases;");
        sb.AppendLine();
        sb.AppendLine($"public static class {className}");
        sb.AppendLine("{");
        sb.AppendLine("    public static async Task ExecuteAsync(IPage page, TestCaseRunOptions options, CancellationToken ct)");
        sb.AppendLine("    {");
        sb.AppendLine("        // Recorded by Playwright codegen");
        foreach (var step in steps)
        {
            sb.AppendLine($"        {step}");
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string BuildSafeIdentifier(string raw)
    {
        var compact = Regex.Replace(raw, @"[^\w]", "_");
        if (string.IsNullOrWhiteSpace(compact))
        {
            return "RecordedCase";
        }

        if (char.IsDigit(compact[0]))
        {
            compact = $"Case_{compact}";
        }

        return compact;
    }
}
