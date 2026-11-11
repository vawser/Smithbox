using StudioCore.Application;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Loads and saves the Display Groups tool's presets for a project, in .smithbox/MSB/Display Groups.json.
/// The file is small, so it's read on first use and rewritten after every change.
/// </summary>
public class DisplayGroupPresetStore
{
    private readonly ProjectEntry Project;
    private DisplayGroupPresetList _data;

    public DisplayGroupPresetStore(ProjectEntry project)
    {
        Project = project;
    }

    private string FilePath => Path.Combine(Project.Descriptor.ProjectPath, ".smithbox", "MSB", "Display Groups.json");

    public DisplayGroupPresetList Data
    {
        get
        {
            _data ??= Load();
            return _data;
        }
    }

    public DisplayGroupPreset Find(string name)
    {
        return Data.Presets.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Saves the groups under a name, replacing a preset of the same name.
    /// </summary>
    public void Store(string name, uint[] groups)
    {
        var preset = Find(name);
        if (preset == null)
        {
            preset = new DisplayGroupPreset { Name = name };
            Data.Presets.Add(preset);
        }

        preset.Groups = (uint[])groups.Clone();
        Data.LastUsed = preset.Name;
        Save();
    }

    public void Remove(DisplayGroupPreset preset)
    {
        Data.Presets.Remove(preset);
        if (string.Equals(Data.LastUsed, preset.Name, StringComparison.OrdinalIgnoreCase))
            Data.LastUsed = "";
        Save();
    }

    public void MarkUsed(DisplayGroupPreset preset)
    {
        if (Data.LastUsed == preset.Name)
            return;

        Data.LastUsed = preset.Name;
        Save();
    }

    private DisplayGroupPresetList Load()
    {
        var path = FilePath;
        if (!File.Exists(path))
            return new DisplayGroupPresetList();

        try
        {
            var text = File.ReadAllText(path);
            var data = JsonSerializer.Deserialize(text, MapEditorJsonSerializerContext.Default.DisplayGroupPresetList);
            data ??= new DisplayGroupPresetList();
            data.Presets ??= new();
            data.Presets.RemoveAll(p => p == null || p.Groups == null);
            data.LastUsed ??= "";
            return data;
        }
        catch (Exception e)
        {
            Smithbox.LogError<DisplayGroupPresetStore>(string.Format(LOC.Get("MAP_DisplayGroups_Presets_Read_Failed"), path), e);
            return new DisplayGroupPresetList();
        }
    }

    private void Save()
    {
        var path = FilePath;

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var text = JsonSerializer.Serialize(Data, MapEditorJsonSerializerContext.Default.DisplayGroupPresetList);
            File.WriteAllText(path, text);
        }
        catch (Exception e)
        {
            Smithbox.LogError<DisplayGroupPresetStore>(string.Format(LOC.Get("MAP_DisplayGroups_Presets_Write_Failed"), path), e);
        }
    }
}
