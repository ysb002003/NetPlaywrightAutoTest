using AutoTest.Models;
using Microsoft.Playwright;

namespace AutoTest.Services;

public sealed class SmokeTestRunner
{
    public async Task<SmokeTestResult> RunAsync(SmokeTestRequest request)
    {
        var executedAt = DateTime.Now;
        var runId = $"{executedAt:yyyyMMdd_HHmmss}";
        var evidenceDirectory = Path.Combine(AppContext.BaseDirectory, "artifacts", runId);
        Directory.CreateDirectory(evidenceDirectory);
        var videoDirectory = Path.Combine(evidenceDirectory, "video");
        if (request.EnableVideoOnFailure)
        {
            Directory.CreateDirectory(videoDirectory);
        }

        if (string.IsNullOrWhiteSpace(request.Url))
        {
            return new SmokeTestResult(executedAt, "Failed", "URL is required.", null, null);
        }

        string status = "Passed";
        string message = "Run smoke success.";
        string? screenshotPath = null;
        string? videoPath = null;

        IPlaywright? playwright = null;
        IBrowser? browser = null;
        IBrowserContext? context = null;
        IPage? page = null;

        try
        {
            playwright = await Playwright.CreateAsync();
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = !request.ShowBrowser
            });

            var contextOptions = new BrowserNewContextOptions
            {
                IgnoreHTTPSErrors = true
            };
            if (request.EnableVideoOnFailure)
            {
                contextOptions.RecordVideoDir = videoDirectory;
                contextOptions.RecordVideoSize = new RecordVideoSize
                {
                    Width = 1280,
                    Height = 720
                };
            }

            context = await browser.NewContextAsync(contextOptions);
            page = await context.NewPageAsync();
            await page.GotoAsync(request.Url, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.Load,
                Timeout = 30000
            });

            var title = await page.TitleAsync();
            message = $"Open page success. Title: {title}";
        }
        catch (Exception ex)
        {
            status = "Failed";
            message = ex.Message;
            if (page is not null)
            {
                screenshotPath = Path.Combine(evidenceDirectory, "failure.png");
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
            if (request.EnableVideoOnFailure && page?.Video is not null)
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
            if (browser is not null)
            {
                await browser.DisposeAsync();
            }
            playwright?.Dispose();
        }

        if (status == "Passed" && !string.IsNullOrWhiteSpace(videoPath) && File.Exists(videoPath))
        {
            File.Delete(videoPath);
            videoPath = null;
        }

        return new SmokeTestResult(
            executedAt,
            status,
            message,
            screenshotPath,
            status == "Failed" ? videoPath : null);
    }
}
