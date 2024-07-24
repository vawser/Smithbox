using Octokit;
using StudioCore.Banks.AliasBank;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace StudioCore.Localization;

public enum LocalizationType
{
    English,
    Chinese
}

/// <summary>
/// Handles the localization system used in Smithbox to switch the UI interface language.
/// </summary>
public static class LOC
{
    public static LocalizationType Language { get; set; }

    public static Dictionary<string, string> Words { get; set; }

    /// <summary>
    /// Grab the localized text for the passed key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string Get(string key)
    {
        if(Words.ContainsKey(key))
        {
            return Words[key];
        }
        else
        {
            TaskLogs.AddLog($"Missing localization key: {key}");
            return key;
        }
    }

    /// <summary>
    /// Setup the current language based on the stored localization type.
    /// </summary>
    public static void Setup()
    {
        Words = new();
        Language = CFG.Current.CurrentLocalizationType;

        if(Language == LocalizationType.English )
        {
            ReadLanguageFile("English");
        }
        else if (Language == LocalizationType.Chinese)
        {
            ReadLanguageFile("Chinese");
        }
    }

    /// <summary>
    /// Read the localization json file and populate the Words dictionary
    /// </summary>
    /// <param name="name"></param>
    private static void ReadLanguageFile(string name)
    {
        var filePath = $"{AppContext.BaseDirectory}\\Localization\\Languages\\{name}.json";

        if (File.Exists(filePath))
        {
            var jsonString = File.ReadAllText(filePath);
            var newResource = JsonSerializer.Deserialize(jsonString, LanguageSerializationContext.Default.LanguageList);

            foreach (var entry in newResource.List)
            {
                Words.Add(entry.ID, entry.Text);
            }
        }
    }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true,
    ReadCommentHandling = JsonCommentHandling.Skip)
]
[JsonSerializable(typeof(LanguageList))]
[JsonSerializable(typeof(LanguageWord))]
public partial class LanguageSerializationContext
    : JsonSerializerContext
{ }

public class LanguageList
{
    public List<LanguageWord> List { get; set; }
}

public class LanguageWord
{
    public string ID { get; set; }
    public string Text { get; set; }
}