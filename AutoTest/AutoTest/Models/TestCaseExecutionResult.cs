namespace AutoTest.Models;

public sealed record TestCaseExecutionResult(
    string CaseName,
    DateTime ExecutedAt,
    string Status,
    string Message,
    string ExpectedResult,
    string ActualResult,
    string? ScreenshotPath,
    string? VideoPath,
    long DurationMs);
