using StudioCore.Application;
using StudioCore.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The memory offsets used by the FX Reloader (FxReloader), one group per game executable version.
/// Read from Assets/FX Reloader/{game}/*.json. The connection itself (which exe, quit out, reload world) is GameLink's.
/// The reloader tries the group for the running version first, then the others, newest first.
/// See Claude/Documentation/FX.md (FX Reloader).
/// </summary>
public class FxReloaderOffsets
{
    public List<FxReloaderOffsetEntry> Groups { get; set; } = new();

    /// <summary>Reads this game's offsets. Games without any have no FX Reloader.</summary>
    public static FxReloaderOffsets Load(ProjectEntry project)
    {
        var offsets = new FxReloaderOffsets();
        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "FX Reloader", ProjectUtils.GetGameDirectory(project));

        if (!Directory.Exists(srcDir))
            return offsets;

        foreach (var filepath in Directory.EnumerateFiles(srcDir, "*.json"))
        {
            try
            {
                var text = File.ReadAllText(filepath);
                var item = JsonSerializer.Deserialize(text, FxEditorJsonSerializerContext.Default.FxReloaderOffsetEntry);

                if (item != null && !offsets.Groups.Any(e => e.exeVersion == item.exeVersion))
                    offsets.Groups.Add(item);
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(FxReloaderOffsets), LOC.Get("FX_Reloader_Offsets_Read_Failed", filepath), e);
            }
        }

        return offsets;
    }

    /// <summary>The groups to try for this executable version: its own group first, then the rest, newest first</summary>
    public List<FxReloaderOffsetEntry> GroupsFor(string exeVersion) =>
        GameLinkOffsets.OrderFor(Groups, exeVersion, e => e.exeVersion);
}

public class FxReloaderOffsetEntry
{
    public string exeName { get; set; }
    public string exeVersion { get; set; }

    // AOBs are space-separated bytes, ?? = any byte. Relative offsets are "start/end" of a rel32, from the match.

    // The SFX singleton's global (CSSfxImp in ER, AC6, NR; SprjSfxImp in DS3, SDT), from a singleton access in
    // FfxResCap::Init (or, in DS3, CSSfx's effect registration): mov reg,[global] at the match.
    public string sfxGlobalAob { get; set; }
    public string sfxGlobalRelativeOffset { get; set; }

    // Optional: the access names the singleton with a lea to an ASCII string (DS3: "SprjSfx"); the match is refused
    // when the string differs
    public string sfxNameRelativeOffset { get; set; }
    public string sfxName { get; set; }

    // From the singleton to the effect resource container, comma-separated: [[[sfx+a]+b]+c] (scene control, graphics
    // resource manager, resource container). ER: "0x60,0x28,0x160".
    public string managerPath { get; set; }

    // LoadBinaryFxr(bytes, size): copies an FXR into the effect heap and returns its definition wrapper.
    // Starts loadFxrAobOffset bytes before the match and must begin with loadFxrPrologue (an AOB).
    public string loadFxrAob { get; set; }
    public string loadFxrAobOffset { get; set; }
    public string loadFxrPrologue { get; set; }

    // The manager's definition list: manager + head = the list's sentinel pointer, + count = its length.
    // Nodes: +0 next, id, wrapper. Wrapper: the FXR copy, and a count of the effect's instances playing now.
    public string definitionsHeadOffset { get; set; }
    public string definitionsCountOffset { get; set; }
    public string nodeIdOffset { get; set; }
    public string nodeWrapperOffset { get; set; }
    public string wrapperDataOffset { get; set; }
    public string wrapperUseCountOffset { get; set; }

    /// <summary>The FXR version the game's loader accepts (DS3: 4; SDT, ER, AC6, NR: 5)</summary>
    public int fxrVersion { get; set; }
}

[JsonSourceGenerationOptions(
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata,
    IncludeFields = true)]
[JsonSerializable(typeof(FxReloaderOffsetEntry))]
internal partial class FxEditorJsonSerializerContext : JsonSerializerContext
{
}
