using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editor;

[JsonSourceGenerationOptions(WriteIndented = true, GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ProjectSettings))]
internal partial class ProjectSettingsSerializerContext : JsonSerializerContext
{
}

/// <summary>
///     Settings for a modding project. Gets serialized to JSON
/// </summary>
public class ProjectSettings
{
    public string ProjectName { get; set; } = "";
    public string GameRoot { get; set; } = "";
    public GameType GameType { get; set; } = GameType.Undefined;

    // JsonExtensionData stores info in config file not present in class in order to retain settings between versions.
#pragma warning disable IDE0051
    [JsonExtensionData] public IDictionary<string, JsonElement> AdditionalData { get; set; }
#pragma warning restore IDE0051

    // Params
    public List<string> PinnedParams { get; set; } = new();
    public Dictionary<string, List<int>> PinnedRows { get; set; } = new();
    public Dictionary<string, List<string>> PinnedFields { get; set; } = new();

    /// <summary>
    ///     Has different meanings depending on the game, but for supported games
    ///     (DS2 and DS3) this means that params are written as "loose" i.e. outside
    ///     the regulation file.
    /// </summary>
    public bool UseLooseParams { get; set; } = false;

    public bool PartialParams { get; set; } = false;

    // FMG editor
    public string LastFmgLanguageUsed { get; set; } = "";

    public void Serialize(string path)
    {
        var jsonString =
            JsonSerializer.SerializeToUtf8Bytes(this, ProjectSettingsSerializerContext.Default.ProjectSettings);
        File.WriteAllBytes(path, jsonString);
    }

    public static ProjectSettings Deserialize(string path)
    {
        var jsonString = File.ReadAllBytes(path);
        var readOnlySpan = new ReadOnlySpan<byte>(jsonString);
        try
        {
            ProjectSettings settings = JsonSerializer.Deserialize(readOnlySpan,
                ProjectSettingsSerializerContext.Default.ProjectSettings);
            if (settings == null)
            {
                throw new Exception("JsonConvert returned null");
            }

            return settings;
        }
        catch (Exception e)
        {
            DialogResult result = PlatformUtils.Instance.MessageBox(
                $"{e.Message}\n\nProject.json cannot be loaded. Delete project.json?",
                "Project Load Error", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                File.Delete(path);
            }

            return null;
        }
    }
}

public class NewProjectOptions
{
    public string directory = "";
    public bool loadDefaultNames = true;
    public ProjectSettings settings;
}
