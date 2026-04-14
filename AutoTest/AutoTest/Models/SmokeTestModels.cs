namespace AutoTest.Models;

public sealed record SmokeTestRequest(
    string Url,
    string Username,
    string Password,
    bool EnableVideoOnFailure,
    bool ShowBrowser);

public sealed record SmokeTestResult(
    DateTime ExecutedAt,
    string Status,
    string Message,
    string? ScreenshotPath,
    string? VideoPath,
    string? CaseName = null,
    string? ExpectedResult = null,
    string? ActualResult = null,
    long? DurationMs = null);
