using AutoTest.Models;
using Microsoft.Playwright;

namespace AutoTest.Services;

public sealed class TestCaseBatchRunner
{
    public async Task<IReadOnlyList<TestCaseExecutionResult>> RunAsync(
        IReadOnlyList<TestCaseItem> cases,
        TestCaseRunOptions options,
        CancellationToken cancellationToken = default)
    {
        if (cases.Count == 0)
        {
            return [];
        }

        var batchId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var batchRoot = Path.Combine(AppContext.BaseDirectory, "artifacts", $"batch_{batchId}");
        Directory.CreateDirectory(batchRoot);

        var results = new List<TestCaseExecutionResult>();
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = !options.ShowBrowser
        });

        foreach (var testCase in cases)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var caseDir = Path.Combine(batchRoot, SanitizeDirName(testCase.Name));
            Directory.CreateDirectory(caseDir);
            var videoDir = Path.Combine(caseDir, "video");
            if (options.EnableVideoOnFailure)
            {
                Directory.CreateDirectory(videoDir);
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            string status = "Passed";
            string message = "OK";
            string? screenshotPath = null;
            string? videoPath = null;
            string actualResult = string.Empty;

            IBrowserContext? context = null;
            IPage? page = null;

            try
            {
                var contextOptions = new BrowserNewContextOptions { IgnoreHTTPSErrors = true };
                if (options.EnableVideoOnFailure)
                {
                    contextOptions.RecordVideoDir = videoDir;
                    contextOptions.RecordVideoSize = new RecordVideoSize { Width = 1280, Height = 720 };
                }

                context = await browser.NewContextAsync(contextOptions);
                page = await context.NewPageAsync();
                await TestCaseStepExecutor.ExecuteAsync(page, testCase.Steps, options, cancellationToken);
                actualResult = await page.TitleAsync();
                message = $"Steps completed. Title: {actualResult}";
            }
            catch (Exception ex)
            {
                status = "Failed";
                message = ex.Message;
                actualResult = ex.Message;
                if (page is not null)
                {
                    screenshotPath = Path.Combine(caseDir, "failure.png");
                    try
                    {
                        await page.ScreenshotAsync(new PageScreenshotOptions
                        {
                            Path = screenshotPath,
                            FullPage = true
                        });
                    }
                    catch
                    {
                        screenshotPath = null;
                    }
                }
            }
            finally
            {
                if (context is not null)
                {
                    await context.CloseAsync();
                }

                if (options.EnableVideoOnFailure && page?.Video is not null)
                {
                    try
                    {
                        videoPath = await page.Video.PathAsync();
                    }
                    catch
                    {
                        videoPath = null;
                    }
                }
            }

            sw.Stop();
            if (status == "Passed" && !string.IsNullOrWhiteSpace(videoPath) && File.Exists(videoPath))
            {
                try
                {
                    File.Delete(videoPath);
                }
                catch
                {
                    // ignore
                }

                videoPath = null;
            }

            results.Add(new TestCaseExecutionResult(
                testCase.Name,
                DateTime.Now,
                status,
                message,
                testCase.ExpectedResult,
                actualResult,
                screenshotPath,
                status == "Failed" ? videoPath : null,
                sw.ElapsedMilliseconds));
        }

        return results;
    }

    private static string SanitizeDirName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "unnamed_case";
        }

        var invalid = Path.GetInvalidFileNameChars();
        var chars = name.Select(c => invalid.Contains(c) ? '_' : c).ToArray();
        var s = new string(chars).Trim();
        return string.IsNullOrEmpty(s) ? "unnamed_case" : s[..Math.Min(s.Length, 80)];
    }
}
