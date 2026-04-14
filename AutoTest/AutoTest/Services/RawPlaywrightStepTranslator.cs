using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using AutoTest.Models;

namespace AutoTest.Services;

public static partial class RawPlaywrightStepTranslator
{
    public static IReadOnlyList<string> TranslateFromBase64(string payload, TestCaseRunOptions options)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new InvalidOperationException("raw_playwright requires base64 payload.");
        }

        string script;
        try
        {
            var bytes = Convert.FromBase64String(payload.Trim());
            script = Encoding.UTF8.GetString(bytes);
        }
        catch (FormatException ex)
        {
            throw new InvalidOperationException("raw_playwright payload is not valid base64.", ex);
        }

        return TranslateFromScript(script, options);
    }

    public static IReadOnlyList<string> TranslateFromScript(string script, TestCaseRunOptions options)
    {
        var result = new List<string>();
        var lines = script.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0 || line.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (!line.StartsWith("await ", StringComparison.Ordinal))
            {
                continue;
            }

            if (TryTranslateGoto(line, result))
            {
                continue;
            }

            if (TryTranslateGetByRole(line, options, result))
            {
                continue;
            }

            if (TryTranslateGetByLabel(line, options, result))
            {
                continue;
            }

            if (TryTranslateKeyboardPress(line, result))
            {
                continue;
            }

            if (TryTranslateWait(line, result))
            {
                continue;
            }

            if (TryTranslateGetByTextClick(line, result))
            {
                continue;
            }

            if (TryTranslateLocatorFilterNthClick(line, result))
            {
                continue;
            }

            throw new InvalidOperationException($"Unsupported raw playwright line: {line}");
        }

        if (result.Count == 0)
        {
            throw new InvalidOperationException("raw_playwright did not produce executable steps.");
        }

        return result;
    }

    private static bool TryTranslateGoto(string line, List<string> result)
    {
        var m = GotoRegex().Match(line);
        if (!m.Success)
        {
            return false;
        }

        var url = m.Groups["url"].Value;
        if (url.Contains("login.microsoftonline.com", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        result.Add($"goto|{url}");
        return true;
    }

    private static bool TryTranslateGetByRole(string line, TestCaseRunOptions options, List<string> result)
    {
        var m = GetByRoleRegex().Match(line);
        if (!m.Success)
        {
            return false;
        }

        var role = m.Groups["role"].Value;
        var name = m.Groups["name"].Value;
        var nthRaw = m.Groups["nth"].Value;
        var action = m.Groups["action"].Value;
        var arg = m.Groups["arg"].Value;
        var selector = BuildRoleSelector(role, name);
        selector = ApplyNth(selector, nthRaw);

        if (string.IsNullOrWhiteSpace(selector))
        {
            throw new InvalidOperationException($"Unsupported role for raw_playwright: {role}");
        }

        switch (action)
        {
            case "ClickAsync":
                if (role.Equals("Textbox", StringComparison.OrdinalIgnoreCase))
                {
                    result.Add($"wait_visible|{selector}");
                }
                else
                {
                    result.Add($"click|{selector}");
                }

                return true;
            case "FillAsync":
                var value = NormalizeSecret(arg, options);
                result.Add($"fill|{selector}|{value}");
                return true;
            case "PressAsync":
                var key = NormalizeStringArg(arg);
                result.Add($"wait_visible|{selector}");
                result.Add($"press|{key}");
                return true;
            default:
                throw new InvalidOperationException($"Unsupported action for GetByRole: {action}");
        }
    }

    private static bool TryTranslateGetByLabel(string line, TestCaseRunOptions options, List<string> result)
    {
        var m = GetByLabelRegex().Match(line);
        if (!m.Success)
        {
            return false;
        }

        var label = m.Groups["label"].Value;
        var action = m.Groups["action"].Value;
        var arg = m.Groups["arg"].Value;
        var selector = $"input[aria-label=\"{EscapeCssText(label)}\"],textarea[aria-label=\"{EscapeCssText(label)}\"]";

        switch (action)
        {
            case "ClickAsync":
                result.Add($"wait_visible|{selector}");
                return true;
            case "FillAsync":
                var value = NormalizeSecret(arg, options);
                result.Add($"fill|{selector}|{value}");
                return true;
            default:
                throw new InvalidOperationException($"Unsupported action for GetByLabel: {action}");
        }
    }

    private static bool TryTranslateKeyboardPress(string line, List<string> result)
    {
        var m = KeyboardPressRegex().Match(line);
        if (!m.Success)
        {
            return false;
        }

        result.Add($"press|{m.Groups["key"].Value}");
        return true;
    }

    private static bool TryTranslateWait(string line, List<string> result)
    {
        var m = WaitRegex().Match(line);
        if (!m.Success)
        {
            return false;
        }

        result.Add($"wait|{m.Groups["ms"].Value}");
        return true;
    }

    private static bool TryTranslateGetByTextClick(string line, List<string> result)
    {
        var m = GetByTextClickRegex().Match(line);
        if (!m.Success)
        {
            return false;
        }

        var text = m.Groups["text"].Value;
        result.Add($"click|text={text}");
        return true;
    }

    private static bool TryTranslateLocatorFilterNthClick(string line, List<string> result)
    {
        var m = LocatorFilterNthClickRegex().Match(line);
        if (!m.Success)
        {
            return false;
        }

        var hasText = m.Groups["text"].Value;
        result.Add($"click|text={hasText}");
        return true;
    }

    private static string BuildRoleSelector(string role, string name)
    {
        var escaped = EscapeCssText(name);
        if (role.Equals("Link", StringComparison.OrdinalIgnoreCase))
        {
            var cleaned = NormalizeDisplayText(name);
            var stable = ToStableLinkText(cleaned);
            var escapedStable = EscapeCssText(stable);
            return $"a.group-item[title=\"{escapedStable}\"],a.group-item:has(.item-name:has-text(\"{escapedStable}\")),a[title=\"{escapedStable}\"],a:has(.item-name:has-text(\"{escapedStable}\")),a:has-text(\"{escapedStable}\")";
        }

        if (role.Equals("Button", StringComparison.OrdinalIgnoreCase))
        {
            return $"role=button[name=\"{escaped}\"]";
        }

        if (role.Equals("Textbox", StringComparison.OrdinalIgnoreCase))
        {
            return $"input[aria-label=\"{escaped}\"],input[placeholder=\"{escaped}\"],textarea[aria-label=\"{escaped}\"],textarea[placeholder=\"{escaped}\"]";
        }

        return string.Empty;
    }

    private static string ApplyNth(string selector, string nthRaw)
    {
        if (string.IsNullOrWhiteSpace(nthRaw))
        {
            return selector;
        }

        if (!int.TryParse(nthRaw, out var nth) || nth < 0)
        {
            return selector;
        }

        return $"{selector}||nth={nth}";
    }

    private static string NormalizeSecret(string rawArg, TestCaseRunOptions options)
    {
        var arg = NormalizeStringArg(rawArg);

        if (!string.IsNullOrWhiteSpace(options.Username) && arg.Equals(options.Username, StringComparison.OrdinalIgnoreCase))
        {
            return "{username}";
        }

        if (!string.IsNullOrWhiteSpace(options.Password) && arg.Equals(options.Password, StringComparison.Ordinal))
        {
            return "{password}";
        }

        return arg;
    }

    private static string NormalizeStringArg(string rawArg)
    {
        var arg = rawArg.Trim();
        if (arg.StartsWith('"') && arg.EndsWith('"') && arg.Length >= 2)
        {
            return arg[1..^1];
        }

        return arg;
    }

    private static string EscapeCssText(string value) => value.Replace("\"", "\\\"", StringComparison.Ordinal);

    private static string NormalizeDisplayText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category is UnicodeCategory.PrivateUse or UnicodeCategory.OtherSymbol or UnicodeCategory.ModifierSymbol)
            {
                sb.Append(' ');
                continue;
            }

            sb.Append(ch);
        }

        var cleaned = Regex.Replace(sb.ToString(), @"\s+", " ").Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? value : cleaned;
    }

    private static string ToStableLinkText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var openBracket = value.IndexOf('(');
        if (openBracket > 0)
        {
            var prefix = value[..openBracket].Trim();
            if (prefix.Length >= 8)
            {
                return prefix;
            }
        }

        return value;
    }

    [GeneratedRegex(@"^await\s+\w+\.GotoAsync\(""(?<url>.+?)""\)\s*;$", RegexOptions.CultureInvariant)]
    private static partial Regex GotoRegex();

    [GeneratedRegex(@"^await\s+\w+\.GetByRole\(AriaRole\.(?<role>\w+),\s*new\(\)\s*\{\s*Name\s*=\s*""(?<name>.+?)""(?:\s*,\s*Exact\s*=\s*(?:true|false))?\s*\}\)(?:\.Nth\((?<nth>\d+)\))?\.(?<action>ClickAsync|FillAsync|PressAsync)\((?<arg>.*?)\)\s*;$", RegexOptions.CultureInvariant)]
    private static partial Regex GetByRoleRegex();

    [GeneratedRegex(@"^await\s+\w+\.GetByLabel\(""(?<label>.+?)""\)\.(?<action>ClickAsync|FillAsync)\((?<arg>.*?)\)\s*;$", RegexOptions.CultureInvariant)]
    private static partial Regex GetByLabelRegex();

    [GeneratedRegex(@"^await\s+\w+\.Keyboard\.PressAsync\(""(?<key>.+?)""\)\s*;$", RegexOptions.CultureInvariant)]
    private static partial Regex KeyboardPressRegex();

    [GeneratedRegex(@"^await\s+\w+\.WaitForTimeoutAsync\((?<ms>\d+)\)\s*;$", RegexOptions.CultureInvariant)]
    private static partial Regex WaitRegex();

    [GeneratedRegex(@"^await\s+\w+\.GetByText\(""(?<text>.+?)""(?:\s*,\s*new\(\)\s*\{\s*Exact\s*=\s*(?:true|false)\s*\})?\)\.ClickAsync\(\)\s*;$", RegexOptions.CultureInvariant)]
    private static partial Regex GetByTextClickRegex();

    [GeneratedRegex(@"^await\s+\w+\.Locator\(""[^""]*""\)\.Filter\(new\(\)\s*\{\s*HasText\s*=\s*""(?<text>.+?)""\s*\}\)\.Nth\(\d+\)\.ClickAsync\(\)\s*;$", RegexOptions.CultureInvariant)]
    private static partial Regex LocatorFilterNthClickRegex();
}
