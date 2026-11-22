using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Builds the rows of an AITH file (AC6 "AI thread info", one per mission) from the maps it covers.
/// A row refers to an Enemy part by its index in its map's MSB and gives it a worker slot out of 5, 6 and 12.
/// See Claude/Documentation/AITH Format.md.
/// </summary>
public static partial class AiThreadInfoBuilder
{
    /// <summary>
    /// A map an AITH file covers: its ID and its slot in the game's map list, which every row of the map carries.
    /// </summary>
    public record MapRef(string MapID, byte BlockIndex);

    /// <summary>
    /// The maps the rows of a file refer to, in the order they first appear.
    /// </summary>
    public static List<MapRef> GetMaps(AITH aith)
    {
        var maps = new List<MapRef>();

        foreach (var entry in aith.Entries)
        {
            if (!maps.Any(e => e.MapID == entry.MapName))
                maps.Add(new MapRef(entry.MapName, entry.BlockIndex));
        }

        return maps;
    }

    /// <summary>
    /// The Enemy parts of a loaded map, in the order they are written to the MSB: a row's index is a position in this list.
    /// </summary>
    public static List<Entity> GetEnemies(MapContainer map)
    {
        return map.Objects.Where(e => e.WrappedObject is MSB_AC6.Part.Enemy).ToList();
    }

    /// <summary>
    /// One row for each Enemy of each map, in map order, with slots handed out in turn (0,0,0), (1,1,1), ...
    /// as the vanilla files of small missions have them.
    /// </summary>
    public static List<AITH.Entry> Build(IEnumerable<(MapRef Map, int EnemyCount)> maps)
    {
        var entries = new List<AITH.Entry>();

        foreach (var (map, enemyCount) in maps)
        {
            var id = ParseMapID(map.MapID);

            for (int i = 0; i < enemyCount; i++)
            {
                entries.Add(new AITH.Entry
                {
                    Index = i,
                    BlockIndex = map.BlockIndex,
                    MapAA = id[0],
                    MapBB = id[1],
                    MapCC = id[2],
                    MapDD = id[3],
                });
            }
        }

        AssignSlots(entries);
        return entries;
    }

    /// <summary>
    /// Gives each row the next slot in turn under each slot count.
    /// </summary>
    public static void AssignSlots(List<AITH.Entry> entries)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            entries[i].Slot5 = (byte)(i % 5);
            entries[i].Slot6 = (byte)(i % 6);
            entries[i].Slot12 = (byte)(i % 12);
        }
    }

    /// <summary>
    /// Whether a file's rows for a map are what Build would give for the map's enemies (slots aside).
    /// </summary>
    public static bool MatchesMap(AITH aith, string mapID, int enemyCount)
    {
        var indices = aith.Entries.Where(e => e.MapName == mapID).Select(e => e.Index).ToList();
        return indices.SequenceEqual(Enumerable.Range(0, enemyCount));
    }

    /// <summary>
    /// The four parts of mAA_BB_CC_DD, or null if the text isn't a map ID.
    /// </summary>
    public static byte[] ParseMapID(string mapID)
    {
        var m = MapIDRegex().Match(mapID ?? "");

        if (!m.Success)
            return null;

        return [byte.Parse(m.Groups[1].Value), byte.Parse(m.Groups[2].Value), byte.Parse(m.Groups[3].Value), byte.Parse(m.Groups[4].Value)];
    }

    [GeneratedRegex(@"^m(\d{2})_(\d{2})_(\d{2})_(\d{2})$")]
    private static partial Regex MapIDRegex();
}
