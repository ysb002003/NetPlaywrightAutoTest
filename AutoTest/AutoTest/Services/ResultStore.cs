using System.Text.Json;
using AutoTest.Models;

namespace AutoTest.Services;

public sealed class ResultStore
{
    private readonly string _resultFilePath;

    public ResultStore()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "artifacts");
        Directory.CreateDirectory(directory);
        _resultFilePath = Path.Combine(directory, "results.jsonl");
    }

    public void Append(SmokeTestResult result)
    {
        var line = JsonSerializer.Serialize(result);
        File.AppendAllText(_resultFilePath, line + Environment.NewLine);
    }

    public IReadOnlyList<SmokeTestResult> ReadAll()
    {
        if (!File.Exists(_resultFilePath))
        {
            return [];
        }

        var results = new List<SmokeTestResult>();
        foreach (var line in File.ReadLines(_resultFilePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                var result = JsonSerializer.Deserialize<SmokeTestResult>(line);
                if (result is not null)
                {
                    results.Add(result);
                }
            }
            catch
            {
                // Ignore malformed lines to keep history usable.
            }
        }

        return results;
    }

    public void ClearAll()
    {
        if (!File.Exists(_resultFilePath))
        {
            return;
        }

        File.Delete(_resultFilePath);
    }
}
