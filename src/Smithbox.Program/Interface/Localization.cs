using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Interface;

// Short name for readability in usage
public static class LOC
{
    public static LocalizationLanguages Languages;

    public static Dictionary<string, string> Localization = new();
    public static Dictionary<string, string> FallbackLocalization = new();

    public static void Setup()
    {
        var languageDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Localization");
        var file = Path.Combine(languageDir, "Languages.json");

        if (!File.Exists(file))
        {
            Languages = new LocalizationLanguages();
        }
        else
        {
            try
            {
                var filestring = File.ReadAllText(file);
                Languages = JsonSerializer.Deserialize(filestring, LOCSerializerContext.Default.LocalizationLanguages);
            }
            catch (Exception)
            {
                //Smithbox.LogError(typeof(LOC), "Failed to load localization languages.", e);
            }
        }
    }

    // We do it like this for ease of reading the json, then we place the key and text in a dictionary for speed
    public static void Load()
    {
        Localization.Clear();

        // Selected Language
        var curLang = Languages.Languages.FirstOrDefault(e => e.Name == Startup.Current.Program_Language);

        if (curLang == null)
        {
            //Smithbox.LogError(typeof(LOC), "Failed to find program language.");
            return;
        }

        var languageDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Localization", curLang.Folder);

        if(!Directory.Exists(languageDir))
        {
            Directory.CreateDirectory(languageDir);
        }

        foreach(var file in Directory.EnumerateFiles(languageDir))
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var locEntry = JsonSerializer.Deserialize(filestring, LOCSerializerContext.Default.LocalizationEntry);

                foreach (var entry in locEntry.Entries)
                {
                    Localization.TryAdd(entry.Key, entry.Text);
                }
            }
            catch (Exception)
            {
                //Smithbox.LogError(typeof(LOC), "Failed to read localization file..", e);
            }
        }

        // Fallback
        var fallbackLanguageDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Localization", "English");

        if (!Directory.Exists(fallbackLanguageDir))
        {
            Directory.CreateDirectory(fallbackLanguageDir);
        }

        foreach (var file in Directory.EnumerateFiles(fallbackLanguageDir))
        {
            try
            {
                var filestring = File.ReadAllText(file);
                var locEntry = JsonSerializer.Deserialize(filestring, LOCSerializerContext.Default.LocalizationEntry);

                foreach (var entry in locEntry.Entries)
                {
                    FallbackLocalization.TryAdd(entry.Key, entry.Text);
                }
            }
            catch (Exception)
            {
                //Smithbox.LogError(typeof(LOC), "Failed to read localization file..", e);
            }
        }
    }

    public static string Get(string locKey, params object[] parameters)
    {
        if (!Localization.TryGetValue(locKey, out var template))
        {
            // If non-English fails, try fallback English localization
            if (!FallbackLocalization.TryGetValue(locKey, out var fallbackTemplate))
            {
                #if DEBUG
                Smithbox.LogError<Smithbox>($"Missing Localization: {locKey}");
                #endif
                return locKey;
            }
            else
            {
                return parameters.Length == 0
                    ? template
                    : string.Format(template, parameters);
            }
        }
        else
        {
            return parameters.Length == 0
                ? template
                : string.Format(template, parameters);
        }
    }
}

#region JSON
[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]

[JsonSerializable(typeof(LocalizationLanguages))]
[JsonSerializable(typeof(LocalizationLanguageEntry))]

[JsonSerializable(typeof(LocalizationEntry))]

internal partial class LOCSerializerContext : JsonSerializerContext
{
}

public class LocalizationLanguages
{
    public List<LocalizationLanguageEntry> Languages { get; set; } = new();
}

public class LocalizationLanguageEntry
{
    public string Key { get; set; }
    public string Name { get; set; }
    public string Folder { get; set; }
}

public class LocalizationEntry
{
    public List<LocalizationTextEntry> Entries { get; set; }
}
public class LocalizationTextEntry
{
    public string Key { get; set; }
    public string Text { get; set; }
}

#endregion