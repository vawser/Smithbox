using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// A function the Scaleform Reloader calls or hooks: offset from the exe base, and its first bytes as a version check.
/// </summary>
public class ScaleformReloadCode
{
    public string Rva { get; set; } = "0";
    public string Prologue { get; set; } = "";

    public ulong RvaValue => ScaleformReloadOffsets.Hex(Rva);
    public byte[] PrologueBytes => Prologue.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(b => Convert.ToByte(b, 16)).ToArray();
}

/// <summary>
/// One game version's addresses and layouts for the Scaleform Reloader, from
/// Assets/Scaleform Reloader/&lt;GAME&gt;/&lt;exe version&gt;.json. Ported from gfx-reloader (MIT, lugia19), dll/src/game.rs.
/// See Claude/Documentation/Scaleform.md §5 Scaleform Reloader.
/// </summary>
public class ScaleformReloadOffsets
{
    public string Source { get; set; } = "";
    public string Game { get; set; } = "";
    public string ExeName { get; set; } = "";
    public string ExeVersion { get; set; } = "";

    /// <summary>16-byte entries {int layer, byte resident (+4), wchar* name (+8)}.</summary>
    public string MenuTable { get; set; } = "0";
    public int MenuCount { get; set; }

    /// <summary>[[base + holder] + 8] = the Scaleform system.</summary>
    public string ScaleformHolder { get; set; } = "0";

    /// <summary>The movie cache std::map's head pointer, in the Scaleform system.</summary>
    public string CacheMapHead { get; set; } = "0";

    /// <summary>"PointerAndLength" (wchar*, length) or "WString" (std::wstring, inline while capacity &lt; 8).</summary>
    public string MapKeyKind { get; set; } = "";
    public string MapKeyText { get; set; } = "0";
    public string MapKeyLength { get; set; } = "0";
    public string MapKeyCapacity { get; set; } = "0";

    /// <summary>The cache node's movie wrapper (refcount +8, vtable slot 0 destroys).</summary>
    public string NodeValue { get; set; } = "0";

    public string WindowName { get; set; } = "0";
    public string WindowFactory { get; set; } = "0";
    public string WindowCloseCheck { get; set; } = "0";
    public string WindowController { get; set; } = "0";

    public ScaleformReloadCode WindowStep { get; set; } = new();
    public ScaleformReloadCode WindowClose { get; set; } = new();
    public ScaleformReloadCode FileOpen { get; set; } = new();
    public ScaleformReloadCode DefaultOpen { get; set; } = new();
    public ScaleformReloadCode ModifyTime { get; set; } = new();

    /// <summary>The file it was read from.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public string FilePath { get; set; } = "";

    public IEnumerable<(string Name, ScaleformReloadCode Code)> Functions()
    {
        yield return (nameof(WindowStep), WindowStep);
        yield return (nameof(WindowClose), WindowClose);
        yield return (nameof(FileOpen), FileOpen);
        yield return (nameof(DefaultOpen), DefaultOpen);
        yield return (nameof(ModifyTime), ModifyTime);
    }

    public static ulong Hex(string value)
    {
        value = (value ?? "0").Trim();
        if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            value = value[2..];
        return ulong.Parse(value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
    }

    public static string Folder(ProjectEntry project)
    {
        return Path.Combine(AppContext.BaseDirectory, "Assets", "Scaleform Reloader", ProjectUtils.GetGameDirectory(project));
    }

    /// <summary>Every version file for the project's game (none: the game has no Scaleform Reloader).</summary>
    public static List<ScaleformReloadOffsets> Load(ProjectEntry project)
    {
        var list = new List<ScaleformReloadOffsets>();
        var folder = Folder(project);
        if (!Directory.Exists(folder))
            return list;

        foreach (var file in Directory.EnumerateFiles(folder, "*.json"))
        {
            try
            {
                var offsets = JsonSerializer.Deserialize<ScaleformReloadOffsets>(File.ReadAllText(file));
                if (offsets != null && !string.IsNullOrWhiteSpace(offsets.ExeName))
                {
                    offsets.FilePath = file;
                    list.Add(offsets);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(ScaleformReloadOffsets), LOC.Get("TEXVIEW_ScaleformReloader_Log_Offsets_Failed", file), e);
            }
        }

        return list;
    }
}
