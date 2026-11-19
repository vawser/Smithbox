using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The project's Scaleform Configuration (Texture Viewer Tools window): images to show in the GFx mode's preview in
/// place of a movie's placeholder images, which the game fills in at runtime (MENU_Icon_00000, AC6 EquipPanel_*) or
/// which no atlas has. Preview only: the movies are not changed. Kept in .smithbox/Project/Scaleform Configuration.json.
/// </summary>
public class MovieConfiguration
{
    public class Entry
    {
        /// <summary>The placeholder image name, by stem (MENU_Icon_00000); '*' matches any run of characters.</summary>
        public string Placeholder { get; set; } = "";

        /// <summary>The image to show instead: a shoebox cell or menu texture name.</summary>
        public string Substitute { get; set; } = "";
    }

    private class FileData
    {
        public int Version { get; set; } = 1;
        public List<Entry> Placeholders { get; set; } = new();
    }

    private readonly ProjectEntry Project;

    public List<Entry> Placeholders { get; private set; } = new();

    /// <summary>Bumped on every change, so the image lookups are made again.</summary>
    public int Version { get; private set; }

    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    public MovieConfiguration(ProjectEntry project)
    {
        Project = project;
    }

    public string FilePath => Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "Project", "Scaleform Configuration.json");

    public void Load()
    {
        Placeholders = new();
        try
        {
            if (File.Exists(FilePath))
            {
                var data = JsonSerializer.Deserialize<FileData>(File.ReadAllText(FilePath), Json);
                Placeholders = data?.Placeholders?.Where(e => !string.IsNullOrWhiteSpace(e.Placeholder)).ToList() ?? new();
            }
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_ScaleformConfig_Log_Load_Failed", FilePath), e);
        }
        Changed();
    }

    private void SaveFile()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath, JsonSerializer.Serialize(new FileData { Placeholders = Placeholders }, Json));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TEXVIEW_ScaleformConfig_Log_Save_Failed", FilePath), e);
        }
    }

    private void Changed()
    {
        Version++;
        Patterns = null;
    }

    /// <summary>Adds an entry, or replaces the one for the same placeholder. Written at once.</summary>
    public void Set(string placeholder, string substitute)
    {
        placeholder = placeholder.Trim();
        substitute = substitute.Trim();
        var existing = Placeholders.FirstOrDefault(e => e.Placeholder.Equals(placeholder, StringComparison.OrdinalIgnoreCase));
        if (existing != null)
            existing.Substitute = substitute;
        else
            Placeholders.Add(new Entry { Placeholder = placeholder, Substitute = substitute });
        Changed();
        SaveFile();
    }

    public void Remove(Entry entry)
    {
        if (Placeholders.Remove(entry))
        {
            Changed();
            SaveFile();
        }
    }

    private List<(Regex Pattern, Entry Entry)> Patterns;

    /// <summary>
    /// The substitute for an image name, or null. An exact placeholder wins over a pattern; among patterns, the one
    /// with the most literal characters (the most specific) wins.
    /// </summary>
    public string FindSubstitute(string fileName)
    {
        if (Placeholders.Count == 0)
            return null;

        var stem = MovieImages.Stem(fileName);

        foreach (var e in Placeholders)
        {
            if (!e.Placeholder.Contains('*') && MovieImages.Stem(e.Placeholder) == stem)
                return e.Substitute;
        }

        Patterns ??= Placeholders
            .Where(e => e.Placeholder.Contains('*'))
            .OrderByDescending(e => e.Placeholder.Count(c => c != '*'))
            .Select(e => (new Regex("^" + string.Join(".*", MovieImages.Stem(e.Placeholder).Split('*').Select(Regex.Escape)) + "$",
                RegexOptions.CultureInvariant | RegexOptions.IgnoreCase), e))
            .ToList();

        foreach (var (pattern, entry) in Patterns)
        {
            if (pattern.IsMatch(stem))
                return entry.Substitute;
        }

        return null;
    }
}
