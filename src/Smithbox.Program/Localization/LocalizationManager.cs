using System;
using System.Collections.Generic;
using System.IO;
using StudioCore.Application;

namespace StudioCore;

/// <summary>
/// Simple localization manager (singleton).
/// Convention: language files are placed under Assets/I18n in the program directory.
/// Example: Assets/I18n/simple-chinese.txt
/// File format: even lines are keys, the following odd lines are their translations.
/// Example:
/// Help
/// Help (translated)
/// </summary>
public sealed class LocalizationManager
{
    private static readonly Lazy<LocalizationManager> _instance =
        new(() => new LocalizationManager());

    public static LocalizationManager Instance => _instance.Value;

    private readonly Dictionary<string, string> _translations =
        new(StringComparer.OrdinalIgnoreCase);

    public string CurrentLanguageKey { get; private set; } = DefaultLanguageKey;

    public const string DefaultLanguageKey = "english";

    private LocalizationManager()
    {
        // Read initial language from configuration; fall back to default if not set
        try
        {
            if (CFG.Current is not null && !string.IsNullOrWhiteSpace(CFG.Current.System_Language))
            {
                CurrentLanguageKey = CFG.Current.System_Language;
            }
        }
        catch
        {
            // ignore config exceptions and fall back to English
        }

        LoadLanguage(CurrentLanguageKey);
    }

    public void SetLanguage(string languageKey)
    {
        if (string.IsNullOrWhiteSpace(languageKey))
        {
            languageKey = DefaultLanguageKey;
        }

        if (languageKey == CurrentLanguageKey)
        {
            return;
        }

        LoadLanguage(languageKey);
        CurrentLanguageKey = languageKey;

        // Persist current language back to configuration so it is restored next launch
        if (CFG.Current is not null)
        {
            CFG.Current.System_Language = languageKey;
        }
    }

    public string Get(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return key;
        }

        if (_translations.TryGetValue(key, out var value))
        {
            return value;
        }

        return key;
    }

    private void LoadLanguage(string languageKey)
    {
        _translations.Clear();

        // Default language uses keys directly without loading any external file
        if (string.Equals(languageKey, DefaultLanguageKey, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var langFileName = GetLanguageFileName(languageKey);

        if (langFileName is null)
        {
            return;
        }

        var relativePath = Path.Join("Assets", "I18n", langFileName);
        var fullPath = Path.Combine(AppContext.BaseDirectory, relativePath);

        if (!File.Exists(fullPath))
        {
            return;
        }

        var lines = File.ReadAllLines(fullPath);

        for (var i = 0; i + 1 < lines.Length; i += 2)
        {
            var key = lines[i].Trim();
            var value = lines[i + 1].Trim();

            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            _translations[key] = value;
        }
    }

    private static string? GetLanguageFileName(string languageKey)
    {
        if (string.IsNullOrWhiteSpace(languageKey))
            return null;

        // Convention: languageKey is the file name (without extension)
        return languageKey.ToLowerInvariant() + ".txt";
    }

    public IReadOnlyList<(string Key, string Label)> GetAvailableLanguages()
    {
        var result = new List<(string, string)>
        {
            (DefaultLanguageKey, "Default Language")
        };

        var relativeDir = Path.Join("Assets", "I18n");
        var fullDir = Path.Combine(AppContext.BaseDirectory, relativeDir);

        if (!Directory.Exists(fullDir))
            return result;

        foreach (var file in Directory.GetFiles(fullDir, "*.txt"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var key = name.ToLowerInvariant();

            // Skip english.txt since the default language is built-in and does not rely on a file
            if (key == DefaultLanguageKey)
                continue;

            // Build a user-friendly label from the key, e.g. "simple-chinese" -> "Simple Chinese"
            var labelParts = key.Split('-', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < labelParts.Length; i++)
            {
                var part = labelParts[i];
                if (part.Length == 0) continue;
                labelParts[i] = char.ToUpper(part[0]) + (part.Length > 1 ? part[1..] : string.Empty);
            }

            var label = string.Join(' ', labelParts);

            result.Add((key, label));
        }

        return result;
    }
}

