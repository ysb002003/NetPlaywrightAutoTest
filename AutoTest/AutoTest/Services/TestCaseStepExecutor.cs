using AutoTest.Models;
using Microsoft.Playwright;

namespace AutoTest.Services;

/// <summary>
/// One step per line. Format: verb|arg1|arg2...
/// Placeholders in args: {url}, {username}, {password}
/// </summary>
public static class TestCaseStepExecutor
{
    public static async Task ExecuteAsync(
        IPage page,
        string stepsRaw,
        TestCaseRunOptions options,
        CancellationToken cancellationToken = default)
    {
        var lines = SplitStepLines(stepsRaw, options);
        if (lines.Count == 0)
        {
            await GotoAsync(page, options.BaseUrl, options, cancellationToken);
            return;
        }

        for (var i = 0; i < lines.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = lines[i];
            var stepNo = i + 1;
            try
            {
                await ExecuteLineAsync(page, line, options, cancellationToken);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Step {stepNo} failed: {line}{Environment.NewLine}{ex.Message}", ex);
            }
        }
    }

    private static List<string> SplitStepLines(string stepsRaw, TestCaseRunOptions options)
    {
        var list = new List<string>();
        if (string.IsNullOrWhiteSpace(stepsRaw))
        {
            return list;
        }

        foreach (var raw in stepsRaw.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            if (TryExpandRawPlaywright(line, options, out var expanded))
            {
                list.AddRange(expanded);
                continue;
            }

            if (TryTranslateRawPlaywrightLine(line, options, out var translated))
            {
                list.AddRange(translated);
                continue;
            }

            if (IsRawPlaywrightScaffoldingLine(line))
            {
                continue;
            }

            list.Add(line);
        }

        return list;
    }

    private static bool TryExpandRawPlaywright(string line, TestCaseRunOptions options, out IReadOnlyList<string> expanded)
    {
        expanded = [];
        var parts = line.Split('|', 2, StringSplitOptions.None);
        if (parts.Length != 2)
        {
            return false;
        }

        if (!parts[0].Equals("raw_playwright", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        expanded = RawPlaywrightStepTranslator.TranslateFromBase64(parts[1], options);
        return true;
    }

    private static bool TryTranslateRawPlaywrightLine(string line, TestCaseRunOptions options, out IReadOnlyList<string> translated)
    {
        translated = [];
        if (!line.StartsWith("await ", StringComparison.Ordinal))
        {
            return false;
        }

        translated = RawPlaywrightStepTranslator.TranslateFromScript(line, options);
        return true;
    }

    private static bool IsRawPlaywrightScaffoldingLine(string line)
    {
        if (line.Equals("{", StringComparison.Ordinal)
            || line.Equals("}", StringComparison.Ordinal)
            || line.Equals("});", StringComparison.Ordinal))
        {
            return true;
        }

        return line.StartsWith("var ", StringComparison.Ordinal)
            && line.Contains("RunAndWaitForPopupAsync", StringComparison.Ordinal);
    }

    private static async Task ExecuteLineAsync(
        IPage page,
        string line,
        TestCaseRunOptions options,
        CancellationToken cancellationToken)
    {
        var parts = line.Split('|', StringSplitOptions.None)
            .Select(p => ApplyPlaceholders(p.Trim(), options))
            .ToArray();
        if (parts.Length == 0)
        {
            return;
        }

        var verb = parts[0].ToLowerInvariant();
        switch (verb)
        {
            case "goto":
            case "navigate":
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException($"{verb} requires URL.");
                }

                await GotoAsync(page, parts[1], options, cancellationToken);
                break;
            case "click":
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException("click requires selector.");
                }

                // Rejoin if selector contained '|' (e.g. inside quoted CSS).
                var clickSelector = string.Join("|", parts.Skip(1));
                try
                {
                    var clickLoc = await WaitForFirstVisibleLocatorAsync(
                        page,
                        clickSelector,
                        options.DefaultTimeoutMs,
                        cancellationToken);
                    await clickLoc.ClickAsync(new LocatorClickOptions
                    {
                        Timeout = options.DefaultTimeoutMs
                    });
                }
                catch (TimeoutException)
                {
                    // Fallback for menu items that are present but never reported visible by layout/overlay quirks.
                    var fallback = page.Locator(clickSelector).First;
                    await fallback.ClickAsync(new LocatorClickOptions
                    {
                        Timeout = options.DefaultTimeoutMs,
                        Force = true
                    });

                    if (clickSelector.Contains("group-item", StringComparison.OrdinalIgnoreCase))
                    {
                        // Some menu groups only expand when JS click handlers on wrapper/arrow are triggered.
                        await fallback.EvaluateAsync(
                            @"el => {
                                const arrow = el.querySelector('.moaui-menu-icon-arrow-wrap, .moaui-icon-arrow-wrap-click');
                                if (arrow) {
                                    arrow.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
                                }
                                el.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true }));
                            }");
                    }
                }
                break;
            case "click_force":
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException("click_force requires selector.");
                }

                var forceSel = string.Join("|", parts.Skip(1));
                await page.Locator(forceSel).First.ClickAsync(new LocatorClickOptions
                {
                    Timeout = options.DefaultTimeoutMs,
                    Force = true
                });
                break;
            case "scroll_into_view":
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException("scroll_into_view requires selector.");
                }

                var scrollSel = string.Join("|", parts.Skip(1));
                var scrollLoc = await WaitForFirstVisibleLocatorAsync(
                    page,
                    scrollSel,
                    options.DefaultTimeoutMs,
                    cancellationToken);
                await scrollLoc.ScrollIntoViewIfNeededAsync(new LocatorScrollIntoViewIfNeededOptions
                {
                    Timeout = options.DefaultTimeoutMs
                });
                break;
            case "dblclick":
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException("dblclick requires selector.");
                }

                var dblSelector = string.Join("|", parts.Skip(1));
                var dblLoc = await WaitForFirstVisibleLocatorAsync(
                    page,
                    dblSelector,
                    options.DefaultTimeoutMs,
                    cancellationToken);
                await dblLoc.DblClickAsync(new LocatorDblClickOptions
                {
                    Timeout = options.DefaultTimeoutMs
                });
                break;
            case "fill":
                if (parts.Length < 3)
                {
                    throw new InvalidOperationException("fill requires selector|value.");
                }

                var value = string.Join("|", parts.Skip(2));
                var fillLoc = await WaitForFirstVisibleLocatorAsync(
                    page,
                    parts[1],
                    options.DefaultTimeoutMs,
                    cancellationToken);
                await fillLoc.FillAsync(value, new LocatorFillOptions
                {
                    Timeout = options.DefaultTimeoutMs
                });
                break;
            case "press":
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException("press requires key name.");
                }

                await page.Keyboard.PressAsync(parts[1]);
                break;
            case "wait":
                if (parts.Length < 2 || !int.TryParse(parts[1], out var ms))
                {
                    throw new InvalidOperationException("wait requires milliseconds.");
                }

                await Task.Delay(ms, cancellationToken);
                break;
            case "wait_visible":
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException("wait_visible requires selector.");
                }

                var waitSelector = string.Join("|", parts.Skip(1));
                await WaitForFirstVisibleLocatorAsync(
                    page,
                    waitSelector,
                    options.DefaultTimeoutMs,
                    cancellationToken);
                break;
            case "expect_text":
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException("expect_text requires substring.");
                }

                var text = string.Join("|", parts.Skip(1));
                await Assertions.Expect(page.Locator("body")).ToContainTextAsync(text, new LocatorAssertionsToContainTextOptions
                {
                    Timeout = options.DefaultTimeoutMs
                });
                break;
            case "expect_title_contains":
                if (parts.Length < 2)
                {
                    throw new InvalidOperationException("expect_title_contains requires substring.");
                }

                var needle = string.Join("|", parts.Skip(1));
                await page.WaitForFunctionAsync(
                    "(expected) => document.title.includes(expected)",
                    needle,
                    new PageWaitForFunctionOptions { Timeout = options.DefaultTimeoutMs });
                break;
            default:
                throw new InvalidOperationException($"Unknown step verb: {verb}");
        }
    }

    /// <summary>
    /// When selector matches multiple nodes, use the first one that is visible (skips hidden menu duplicates).
    /// </summary>
    private static async Task<ILocator> WaitForFirstVisibleLocatorAsync(
        IPage page,
        string selector,
        int timeoutMs,
        CancellationToken cancellationToken)
    {
        var (baseSelector, desiredVisibleIndex) = ParseSelectorWithNth(selector);
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var root = page.Locator(baseSelector);
            var count = await root.CountAsync();
            var visibleIndex = 0;
            for (var i = 0; i < count; i++)
            {
                var loc = root.Nth(i);
                if (await loc.IsVisibleAsync())
                {
                    if (desiredVisibleIndex is null || visibleIndex == desiredVisibleIndex.Value)
                    {
                        return loc;
                    }

                    visibleIndex++;
                }
            }

            await Task.Delay(100, cancellationToken);
        }

        throw new TimeoutException(
            $"No visible element matched within {timeoutMs}ms (selector may match only hidden nodes): {selector}");
    }

    private static (string BaseSelector, int? VisibleIndex) ParseSelectorWithNth(string selector)
    {
        const string marker = "||nth=";
        var idx = selector.LastIndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
        {
            return (selector, null);
        }

        var baseSelector = selector[..idx].Trim();
        var nthRaw = selector[(idx + marker.Length)..].Trim();
        if (!int.TryParse(nthRaw, out var visibleIndex) || visibleIndex < 0 || string.IsNullOrWhiteSpace(baseSelector))
        {
            return (selector, null);
        }

        return (baseSelector, visibleIndex);
    }

    private static async Task GotoAsync(IPage page, string url, TestCaseRunOptions options, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        // Load is more reliable than NetworkIdle on sites with long-polling / analytics.
        await page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.Load,
            Timeout = options.DefaultTimeoutMs
        });
    }

    private static string ApplyPlaceholders(string value, TestCaseRunOptions options)
    {
        return value
            .Replace("{url}", options.BaseUrl, StringComparison.OrdinalIgnoreCase)
            .Replace("{username}", options.Username, StringComparison.OrdinalIgnoreCase)
            .Replace("{password}", options.Password, StringComparison.OrdinalIgnoreCase);
    }
}
