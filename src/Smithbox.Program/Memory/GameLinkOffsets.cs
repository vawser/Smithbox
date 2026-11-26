using StudioCore.Application;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace StudioCore.Memory;

/// <summary>
/// What GameLink needs for a game: the executable to attach to, and the game actions it offers (quit to title,
/// reload the world), one group per executable version. Read from Assets/Game Link/{game}/*.json.
/// Every address is found by AOB, so a group usually keeps working on later versions; each action tries the group
/// for the running version first, then the others, newest first.
/// </summary>
public class GameLinkOffsets
{
    public List<GameLinkOffsetEntry> Groups { get; set; } = new();

    /// <summary>
    /// Reads this game's offsets. Games without any have no game link.
    /// </summary>
    public static GameLinkOffsets Load(ProjectEntry project)
    {
        var offsets = new GameLinkOffsets();
        var srcDir = Path.Combine(AppContext.BaseDirectory, "Assets", "Game Link", ProjectUtils.GetGameDirectory(project));

        if (!Directory.Exists(srcDir))
            return offsets;

        foreach (var filepath in Directory.EnumerateFiles(srcDir, "*.json"))
        {
            try
            {
                var text = File.ReadAllText(filepath);
                var item = JsonSerializer.Deserialize(text, GameLinkJsonSerializerContext.Default.GameLinkOffsetEntry);

                if (item != null && !offsets.Groups.Any(e => e.exeVersion == item.exeVersion))
                    offsets.Groups.Add(item);
            }
            catch (Exception e)
            {
                Smithbox.LogError(typeof(GameLinkOffsets), LOC.Get("GAMELINK_Offsets_Read_Failed", filepath), e);
            }
        }

        return offsets;
    }

    /// <summary>
    /// The executable to attach to (eldenring.exe), from the first group that names one
    /// </summary>
    public string ExeName => Groups.Select(e => e.exeName).FirstOrDefault(e => !string.IsNullOrWhiteSpace(e)) ?? "";

    /// <summary>
    /// The groups to try for this executable version: its own group first, then the rest, newest first
    /// </summary>
    public List<GameLinkOffsetEntry> GroupsFor(string exeVersion) => OrderFor(Groups, exeVersion, e => e.exeVersion);

    /// <summary>
    /// Version-ordered groups of any offsets file: the running version's own group first, then newest first
    /// </summary>
    public static List<T> OrderFor<T>(IEnumerable<T> groups, string exeVersion, Func<T, string> versionOf)
    {
        return groups
            .OrderByDescending(e => versionOf(e) == exeVersion)
            .ThenByDescending(e => Version.TryParse(versionOf(e), out var v) ? v : new Version(0, 0))
            .ToList();
    }

    /// <summary>
    /// Parse a hex value, with or without a 0x prefix. Null if missing or invalid.
    /// </summary>
    public static int? ParseHex(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return null;

        var hex = str.Trim();
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hex = hex[2..];

        return int.TryParse(hex, NumberStyles.HexNumber, null, out var value) ? value : null;
    }

    /// <summary>
    /// Parse a "start/end" pair of hex offsets ("0x3/0x7"): the rel32 sits at start, and is relative to end.
    /// Null if missing or invalid.
    /// </summary>
    public static (int Start, int End)? ParseRelative(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return null;

        var parts = str.Split('/');
        if (parts.Length != 2)
            return null;

        var start = ParseHex(parts[0]);
        var end = ParseHex(parts[1]);

        return start != null && end != null ? (start.Value, end.Value) : null;
    }
}

public class GameLinkOffsetEntry
{
    public string exeName { get; set; }
    public string exeVersion { get; set; }

    // AOBs are space-separated bytes, ?? = any byte. Relative offsets are "start/end" of a rel32 in the AOB.

    // Optional: quit to title. The request function starts quitOutAobOffset bytes before the match and begins with
    // quitOutPrologue (an AOB); quitOutMenuManRelativeOffset is the menu manager global's rel32, from the function
    // start. The request sets byte [[menuMan] + quitOutMenuOffset] + quitOutFlagOffset = 1.
    public string quitOutAob { get; set; }
    public string quitOutAobOffset { get; set; }
    public string quitOutPrologue { get; set; }
    public string quitOutMenuManRelativeOffset { get; set; }
    public string quitOutMenuOffset { get; set; }
    public string quitOutFlagOffset { get; set; }

    // Optional: reload the world from the save (Memory/WorldReload). "LoadSlot" = (save request, then) the world
    // step's load-slot request. Empty = not available.
    // Each field is found by a GameMan getter AOB: GameMan's rel32 ("start/end") and where the i32 field offset sits
    // in the match.
    public string worldReloadMethod { get; set; }

    // The load slot getter: mov rax,[GameMan]; mov eax,[rax+LoadSlot]; ret. Required.
    public string worldLoadSlotAob { get; set; }
    public string worldLoadSlotGameManRelativeOffset { get; set; }
    public string worldLoadSlotFieldOffset { get; set; }

    // Optional: the save request getter (mov rax,[GameMan]; movzx ebx,byte ptr [rax+SaveRequest]; ...). With it the
    // reload saves first; without it (AC6) it loads the last save as it is.
    public string worldSaveRequestAob { get; set; }
    public string worldSaveRequestGameManRelativeOffset { get; set; }
    public string worldSaveRequestFieldOffset { get; set; }

    // The slot to load: the current save slot getter (mov rax,[GameMan]; mov eax,[rax+CurrentSlot]; ret), or else
    // worldFixedSlot (AC6: 0, the only slot its RequestLoad accepts)
    public string worldCurrentSlotAob { get; set; }
    public string worldCurrentSlotGameManRelativeOffset { get; set; }
    public string worldCurrentSlotFieldOffset { get; set; }
    public string worldFixedSlot { get; set; }

    // Optional: the event flags, for the Event Flags tool (EventFlagMonitor; see
    // Claude/Documentation/Event Flag Viewer.md). eventFlagManAob finds a load of the flag manager global
    // (eventFlagManRelativeOffset = its rel32, "start/end"); several matches are fine if they all load the same global.
    // eventFlagLayout picks how the flags are stored:
    //   "VirtualMemory" (ER, NR, AC6): the manager holds a CSFD4VirtualMemoryFlag: a slot buffer and a block -> entry
    //     tree (std::map). Offsets: the buffer pointer, the slot count, the bytes per block, the flags per block and
    //     the tree's head node; optionally the layout mode (a change re-reads the tree).
    //   "Sprj" (DS3, SDT): the manager holds group tables (pointer at eventFlagGroupsOffset) of per-world-block records,
    //     ready byte at eventFlagReadyOffset; the world blocks come from FieldArea (eventFlagFieldAreaAob / rel32), its
    //     world info at eventFlagWorldInfoOffset, with eventFlagWorldBlockStride bytes per block.
    //     DS1R: also eventFlagWorldAreaStride (default 0x38).
    //   "Hashed" (DS2S): blocks of 10,000 flags in eventFlagBucketCount bucket lists at eventFlagBucketsOffset
    //     (the count is hex like every value here: 31 buckets = "0x1F").
    // eventFlagManPath (optional): offsets to follow from the global to the manager, "0x70/0x20".
    // eventFlagAlwaysOn / eventFlagAlwaysOff (optional): flags that are ON / OFF whenever a character is loaded.
    public string eventFlagLayout { get; set; }
    public string eventFlagManAob { get; set; }
    public string eventFlagManRelativeOffset { get; set; }
    public string eventFlagManPath { get; set; }
    public string eventFlagWorldAreaStride { get; set; }
    public string eventFlagBucketsOffset { get; set; }
    public string eventFlagBucketCount { get; set; }
    public string eventFlagBufferOffset { get; set; }
    public string eventFlagSlotCountOffset { get; set; }
    public string eventFlagBlockBytesOffset { get; set; }
    public string eventFlagBlockFlagsOffset { get; set; }
    public string eventFlagTreeOffset { get; set; }
    public string eventFlagLayoutModeOffset { get; set; }
    public string eventFlagGroupsOffset { get; set; }
    public string eventFlagReadyOffset { get; set; }
    public string eventFlagFieldAreaAob { get; set; }
    public string eventFlagFieldAreaRelativeOffset { get; set; }
    public string eventFlagWorldInfoOffset { get; set; }
    public string eventFlagWorldBlockStride { get; set; }
    public string eventFlagAlwaysOn { get; set; }
    public string eventFlagAlwaysOff { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true, GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(GameLinkOffsetEntry))]
internal partial class GameLinkJsonSerializerContext : JsonSerializerContext
{
}
