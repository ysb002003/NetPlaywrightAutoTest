namespace AutoTest.Models;

public sealed record TestCaseRunOptions(
    string BaseUrl,
    string Username,
    string Password,
    bool EnableVideoOnFailure,
    bool ShowBrowser,
    int DefaultTimeoutMs = 30000);
