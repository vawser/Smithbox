using SoulsFormats;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.Common;

public enum EventFlagValidity { Valid, Zero, Unallocated }

/// <summary>What an event flag ID is: whether the game stores it, the range it belongs to and what that range is for</summary>
public sealed class EventFlagInfo
{
    public EventFlagValidity Validity;
    /// <summary>The range the ID falls in, e.g. "Map m60_43_36_00 (open world tile)"</summary>
    public string Range;
    /// <summary>What flags of that range are typically used for in the vanilla game; null if no use is known</summary>
    public string Usage;
    /// <summary>For an unallocated flag, why the game doesn't store it</summary>
    public string Reason;

    public bool IsValid => Validity == EventFlagValidity.Valid;
}

/// <summary>
/// Elden Ring's event flag allocation: the set of 1000-flag blocks the game stores. A flag whose block (ID / 1000) isn't
/// allocated is ignored by the game: it always reads off and writes do nothing.
/// Port of the game's start-up registration (CSEventFlagManImp, eldenring.exe 1405D3B20), built from the project's
/// four eventflagalloclist files (so a mod's lists apply) and the exe's tile table (Assets/EVENTFLAG/ER/Tile Table.json).
/// See Claude/Documentation/ER Event Flags.md.
/// </summary>
public sealed class ErEventFlagAllocation
{
    private const int SlotCount = 14667;
    private static readonly int[] TileSub = { 0, 2, 4, 5, 7, 8, 9 };
    private static readonly int[] AllocSub = { 0, 1, 2, 3, 4, 5, 7, 8, 9 };

    // Load order of the game (1405D86D0); the first line for a map wins
    private static readonly string[] AllocFiles = { "legacymap", "openmap", "legacymap_dlc02", "openmap_dlc02" };

    private record AllocLine(int Index, int Area, int B, int C, int Col3);
    private record struct Tile(int T, int B, int X, int Y);

    private readonly List<AllocLine> Alloc = new();
    private readonly Dictionary<Tile, Tile> DlcDonor = new();   // m61 tile -> the m60 tile whose slots it uses
    private readonly HashSet<Tile> Excluded = new();             // m60 tiles in the table: no slots of their own
    private readonly HashSet<int> Blocks = new();

    public int BlockCount => Blocks.Count;

    /// <summary>Builds the allocation for an ER project; null (with a log entry) when its data can't be read</summary>
    public static ErEventFlagAllocation TryBuild(ProjectEntry project)
    {
        try
        {
            var a = new ErEventFlagAllocation();
            foreach (var name in AllocFiles)
            {
                var path = $"/event/eventflag/{name}.eventflagalloclist.dcx";
                var data = project.VFS.FS.ReadFile(path);
                if (data == null)
                {
                    Smithbox.LogError(typeof(ErEventFlagAllocation), LOC.Get("EVENTFLAG_Log_Missing_Alloc_List", path));
                    return null;
                }
                a.ReadAlloc(Encoding.UTF8.GetString(DCX.Decompress(data.Value).Span));
            }

            var tablePath = Path.Join(AppContext.BaseDirectory, "Assets", "EVENTFLAG", "ER", "Tile Table.json");
            using (var doc = JsonDocument.Parse(File.ReadAllText(tablePath)))
            {
                static Tile Read(JsonElement e) => new(e.GetProperty("T").GetInt32(), e.GetProperty("B").GetInt32(),
                    e.GetProperty("X").GetInt32(), e.GetProperty("Y").GetInt32());

                foreach (var row in doc.RootElement.GetProperty("Rows").EnumerateArray())
                {
                    var tile = Read(row);
                    a.Excluded.Add(tile);
                    if (row.TryGetProperty("Dlc", out var dlc))
                        a.DlcDonor.TryAdd(Read(dlc), tile);
                }
            }

            a.Build();
            return a;
        }
        catch (Exception e)
        {
            Smithbox.LogError(typeof(ErEventFlagAllocation), LOC.Get("EVENTFLAG_Log_Build_Failed"), e);
            return null;
        }
    }

    private void ReadAlloc(string text)
    {
        foreach (var line in text.Split('\n').Select(l => l.Trim()).Where(l => l.Length > 0))
        {
            var m = Regex.Match(line, @"^(\d+),m(\d\d)_(\d\d)_(\d\d)_\d\d,(\d+)$");
            if (m.Success)
                Alloc.Add(new AllocLine(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value),
                    int.Parse(m.Groups[3].Value), int.Parse(m.Groups[4].Value), int.Parse(m.Groups[5].Value)));
        }
    }

    #region Registration (port of the exe)

    private static int LegacyBlock(int area, int bb, int k) => (area * 100 + bb) * 10 + k;
    private static int OpenBlock(int t, int b, int x, int y, int k) => (((t * 10 + b) * 100 + x) * 100 + y) * 10 + k;
    private static int OpenType(int area) => area switch { 60 => 1, 61 => 2, 80 => 1, _ => 0 };

    private int AllocIndex(int area, int bb, int cc, int col3) =>
        Alloc.FirstOrDefault(a => a.Area == area && a.B == bb && a.C == cc && a.Col3 == col3)?.Index ?? -1;

    private static int Cap(int slot) => slot >= SlotCount ? -1 : slot;

    private static int AllocSlot(int index, int k)
    {
        var pos = Array.IndexOf(AllocSub, k);
        return index < 0 || pos < 0 ? -1 : Cap(11067 + index * 9 + pos);
    }

    // 1405D4B90
    private int LegacySlot(int area, int bb, int k)
    {
        if (area == 0)
        {
            if (bb == 0) return k;
            if (bb is >= 6 and <= 9) return (bb - 5) * 10 + k;
            if (bb is >= 10 and <= 99) return k == 0 ? bb + 40 : -1;
        }
        else if (area is >= 90 and <= 99)
            return Cap((area * 100 + bb) * 10 + SlotCount + k);

        return AllocSlot(AllocIndex(area, bb, 0, 0), k);
    }

    // 1405D4D80
    private int OpenSlot(int t, int b, int x, int y, int k)
    {
        if (t == 1)
        {
            var pos = Array.IndexOf(TileSub, k);
            if (b == 0 && x is >= 24 and <= 62 && y is >= 25 and <= 64)
                return pos < 0 ? -1 : Cap((x * 40 - 965 + y) * 7 + pos);

            var index = AllocIndex(60, x, y, b);
            if (index >= 0)
                return AllocSlot(index, k);

            if ((b * 100 + x) * 100 + y == 9900)   // m60_99_00
                return pos < 0 ? -1 : Cap(0x62C * 7 + pos);

            return -1;
        }

        return t == 2 ? AllocSlot(AllocIndex(61, x, y, b), k) : -1;
    }

    // 1405D4CC0: a DLC tile from the table uses its m60 tile's slots; the table's m60 tiles get none
    private int TileSlot(int t, int b, int x, int y, int k)
    {
        var tile = new Tile(t, b, x, y);
        if (DlcDonor.TryGetValue(tile, out var donor))
            return OpenSlot(donor.T, donor.B, donor.X, donor.Y, k);
        return Excluded.Contains(tile) ? -1 : OpenSlot(t, b, x, y, k);
    }

    private void Add(int block, int slot)
    {
        if (slot >= 0)
            Blocks.Add(block);
    }

    private void AddTile(int t, int b, int x, int y)
    {
        foreach (var k in TileSub)
            Add(OpenBlock(t, b, x, y, k), TileSlot(t, b, x, y, k));
    }

    // 1405D3B20
    private void Build()
    {
        for (int k = 0; k < 10; k++)
            Add(LegacyBlock(0, 0, k), k);
        for (int bb = 6; bb <= 9; bb++)
            for (int k = 0; k < 10; k++)
                Add(LegacyBlock(0, bb, k), LegacySlot(0, bb, k));
        for (int bb = 10; bb <= 99; bb++)
            Add(LegacyBlock(0, bb, 0), LegacySlot(0, bb, 0));

        for (int x = 24; x <= 62; x++)
            for (int y = 25; y <= 64; y++)
                AddTile(1, 0, x, y);
        AddTile(1, 0, 99, 0);

        foreach (var dlc in DlcDonor.Keys.Where(t => t.T == 2 && t.B == 0))
            AddTile(dlc.T, dlc.B, dlc.X, dlc.Y);

        foreach (var line in Alloc)
        {
            if (line.Area is >= 50 and <= 88)
            {
                var t = OpenType(line.Area);
                if (t == 0)
                    continue;
                foreach (var k in AllocSub)
                    Add(OpenBlock(t, line.Col3, line.B, line.C, k), TileSlot(t, line.Col3, line.B, line.C, k));
            }
            else
            {
                foreach (var k in AllocSub)
                    Add(LegacyBlock(line.Area, line.B, k), LegacySlot(line.Area, line.B, k));
            }
        }
    }

    #endregion

    /// <summary>True when the game stores the flag (0 and negative IDs are never stored)</summary>
    public bool IsValid(long flag) => flag > 0 && flag <= uint.MaxValue && Blocks.Contains((int)(flag / 1000));

    #region Description

    // Global ranges: (first, last, usage key). Only ranges with a known vanilla use are listed.
    private static readonly (long First, long Last, string Key)[] GlobalUsage =
    {
        (0, 999, "EVENTFLAG_Usage_Global_Story"),
        (1000, 2999, "EVENTFLAG_Usage_Global_System"),
        (3000, 4999, "EVENTFLAG_Usage_Global_Quests"),
        (6000, 6999, "EVENTFLAG_Usage_Global_Constants"),
        (7000, 7999, "EVENTFLAG_Usage_Global_World"),
        (9000, 9999, "EVENTFLAG_Usage_Global_Bosses"),
        (60000, 60999, "EVENTFLAG_Usage_Global_Key_Items"),
        (61000, 61999, "EVENTFLAG_Usage_Global_Emevd"),
        (62000, 62999, "EVENTFLAG_Usage_Global_Map_Display"),
        (63000, 63999, "EVENTFLAG_Usage_Global_Map_Fragments"),
        (65000, 65999, "EVENTFLAG_Usage_Global_Tears"),
        (66000, 66999, "EVENTFLAG_Usage_Global_Pots"),
        (67000, 68999, "EVENTFLAG_Usage_Global_Cookbooks"),
        (69000, 69999, "EVENTFLAG_Usage_Global_Shop_Stock"),
        (71000, 76999, "EVENTFLAG_Usage_Global_Graces"),
        (78000, 78999, "EVENTFLAG_Usage_Global_Map_Points"),
        (82000, 82999, "EVENTFLAG_Usage_Global_Map_Layers"),
        (100000, 999999, "EVENTFLAG_Usage_Global_Items"),
    };

    /// <summary>The flag's validity, range and typical use</summary>
    public EventFlagInfo Describe(long flag)
    {
        var info = new EventFlagInfo { Validity = IsValid(flag) ? EventFlagValidity.Valid : EventFlagValidity.Unallocated };
        if (flag <= 0)
        {
            info.Validity = EventFlagValidity.Zero;
            info.Reason = LOC.Get("EVENTFLAG_Reason_Zero");
            return info;
        }

        int k = (int)(flag / 1000 % 10);

        if (flag < 1000000)
        {
            info.Range = LOC.Get("EVENTFLAG_Range_Global");
            var usage = GlobalUsage.FirstOrDefault(g => flag >= g.First && flag <= g.Last);
            info.Usage = usage.Key != null ? LOC.Get(usage.Key) : null;
            if (!info.IsValid)
                info.Reason = flag < 60000
                    ? LOC.Get("EVENTFLAG_Reason_Global_Gap")
                    : LOC.Get("EVENTFLAG_Reason_Global_Thousands", flag / 10000 * 10000, flag / 10000 * 10000 + 999);
            return info;
        }

        if (flag < 10000000)
        {
            info.Range = LOC.Get("EVENTFLAG_Range_Seven_Digit");
            info.Reason = LOC.Get("EVENTFLAG_Reason_Seven_Digit");
            return info;
        }

        if (flag < 100000000)
        {
            int area = (int)(flag / 1000000), bb = (int)(flag / 10000 % 100);
            var map = $"m{area:D2}_{bb:D2}_00_00";
            info.Range = LOC.Get("EVENTFLAG_Range_Legacy", map);
            info.Usage = MapUsage(k);
            if (!info.IsValid)
                info.Reason = area >= 90 ? LOC.Get("EVENTFLAG_Reason_Area_9x")
                    : AllocIndex(area, bb, 0, 0) < 0 ? LOC.Get("EVENTFLAG_Reason_Legacy_No_Alloc", map)
                    : LOC.Get("EVENTFLAG_Reason_Legacy_Digit", k);
            return info;
        }

        if (flag >= 1000000000 && flag < 3000000000)
        {
            int t = (int)(flag / 1000000000), b = (int)(flag / 100000000 % 10);
            int x = (int)(flag / 1000000 % 100), y = (int)(flag / 10000 % 100);
            var map = $"m{(t == 1 ? 60 : 61)}_{x:D2}_{y:D2}_00";
            info.Range = b == 0 ? LOC.Get("EVENTFLAG_Range_Tile", map) : LOC.Get("EVENTFLAG_Range_Tile_Extra", map, t * 10 + b);
            info.Usage = t == 1 && x == 99 && y == 0 && b == 0 ? LOC.Get("EVENTFLAG_Usage_Open_World_Common") : MapUsage(k);
            if (!info.IsValid)
            {
                var tile = new Tile(t, b, x, y);
                bool hasSlots = TileSub.Concat(AllocSub).Any(s => TileSlot(t, b, x, y, s) >= 0);
                info.Reason = !hasSlots
                    ? b != 0 ? LOC.Get("EVENTFLAG_Reason_Tile_No_Alloc", map, b)
                    : Excluded.Contains(tile) ? LOC.Get("EVENTFLAG_Reason_Tile_Unused", map)
                    : LOC.Get("EVENTFLAG_Reason_Tile_Outside", map)
                    : LOC.Get("EVENTFLAG_Reason_Tile_Digit", k);
            }
            return info;
        }

        info.Range = LOC.Get("EVENTFLAG_Range_Other");
        info.Reason = LOC.Get("EVENTFLAG_Reason_Other");
        return info;
    }

    private static string MapUsage(int k) => k switch
    {
        0 => LOC.Get("EVENTFLAG_Usage_Map_0"),
        2 => LOC.Get("EVENTFLAG_Usage_Map_2"),
        7 => LOC.Get("EVENTFLAG_Usage_Map_7"),
        8 => LOC.Get("EVENTFLAG_Usage_Map_8"),
        9 => LOC.Get("EVENTFLAG_Usage_Map_9"),
        _ => LOC.Get("EVENTFLAG_Usage_Map_Rare")
    };

    #endregion
}
