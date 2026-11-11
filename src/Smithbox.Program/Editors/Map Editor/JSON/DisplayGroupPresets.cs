using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Display Groups tool's saved views, per project (.smithbox/MSB/Display Groups.json).
/// </summary>
public class DisplayGroupPresetList
{
    public List<DisplayGroupPreset> Presets { get; set; } = new();

    /// <summary>
    /// The preset applied or saved most recently, which the Apply Last Preset keybind applies.
    /// </summary>
    public string LastUsed { get; set; } = "";
}

public class DisplayGroupPreset
{
    public string Name { get; set; } = "";

    /// <summary>
    /// The shown groups, one word per 32 groups (4 words in DES/DS1/DS2, 8 later).
    /// </summary>
    public uint[] Groups { get; set; } = System.Array.Empty<uint>();
}
