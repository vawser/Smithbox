using Andre.Formats;
using StudioCore.ParamEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.MsbEditor;

/// <summary>
///     Business logic for cross-map connections in a game.
/// </summary>
internal class SpecialMapConnections
{
    public enum RelationType
    {
        Unknown,
        Ancestor,
        Parent,
        Child,
        Descendant,
        Connection
    }

    private static Dictionary<string, DungeonOffset> eldenRingOffsets;

    public static Transform? GetEldenMapTransform(
        string mapid,
        IReadOnlyDictionary<string, ObjectContainer> loadedMaps)
    {
        if (!TryInitializeEldenOffsets())
        {
            return null;
        }

        if (!TryParseMap(mapid, out var target) ||
            !ToEldenGlobalCoords(target, Vector3.Zero, 0, 0, out Vector3 targetGlobal))
        {
            return null;
        }

        (var originX, var originZ) = GetClosestTile(targetGlobal, 0, 0);
        // Recenter target in terms of closest tile center, for maximum precision
        if (!ToEldenGlobalCoords(target, Vector3.Zero, originX, originZ, out targetGlobal))
        {
            return null;
        }

        var closestDistSq = float.PositiveInfinity;
        Vector3 closestOriginGlobal = Vector3.Zero;
        ObjectContainer closestMap = null;
        foreach (KeyValuePair<string, ObjectContainer> entry in loadedMaps)
        {
            if (entry.Value == null
                || !entry.Value.RootObject.HasTransform
                || !TryParseMap(entry.Key, out var origin)
                || !ToEldenGlobalCoords(origin, Vector3.Zero, originX, originZ, out Vector3 originGlobal))
            {
                continue;
            }

            var distSq = Vector3.DistanceSquared(targetGlobal, originGlobal);
            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closestOriginGlobal = originGlobal;
                closestMap = entry.Value;
            }
        }

        if (closestMap == null)
        {
            return null;
        }

        Vector3 targetOffset = targetGlobal - closestOriginGlobal;
        return closestMap.RootObject.GetLocalTransform() + targetOffset;
    }

    public static IReadOnlyDictionary<string, RelationType> GetRelatedMaps(
        GameType gameType,
        string mapid,
        IReadOnlyCollection<string> allMapIds,
        List<byte[]> connectColMaps = null)
    {
        connectColMaps ??= new List<byte[]>();
        SortedDictionary<string, RelationType> relations = new();
        if (!TryParseMap(mapid, out var parts))
        {
            return relations;
        }

        if (gameType == GameType.EldenRing && parts[0] == 60 && parts[1] > 0 && parts[2] > 0)
        {
            var scale = parts[3] % 10;
            if (scale < 2)
            {
                var tileX = parts[1];
                var tileZ = parts[2];
                tileX /= 2;
                tileZ /= 2;
                var parent = FormatMap(new byte[] { 60, tileX, tileZ, (byte)(parts[3] + 1) });
                if (allMapIds.Contains(parent))
                {
                    relations[parent] = RelationType.Parent;
                    if (scale == 0)
                    {
                        tileX /= 2;
                        tileZ /= 2;
                        var ancestor = FormatMap(new byte[] { 60, tileX, tileZ, (byte)(parts[3] + 2) });
                        if (allMapIds.Contains(ancestor))
                        {
                            relations[ancestor] = RelationType.Ancestor;
                        }
                    }
                }
            }

            if (scale > 0)
            {
                // Order: Southwest, Northwest, Southeast, Northeast
                var tileX = parts[1];
                var tileZ = parts[2];
                for (var x = 0; x <= 1; x++)
                {
                    for (var z = 0; z <= 1; z++)
                    {
                        var childX = (byte)((tileX * 2) + x);
                        var childZ = (byte)((tileZ * 2) + z);
                        var child = FormatMap(new byte[] { 60, childX, childZ, (byte)(parts[3] - 1) });
                        if (allMapIds.Contains(child))
                        {
                            relations[child] = RelationType.Child;
                            if (scale != 2)
                            {
                                continue;
                            }

                            for (var cx = 0; cx <= 1; cx++)
                            {
                                for (var cz = 0; cz <= 1; cz++)
                                {
                                    var descX = (byte)((childX * 2) + cx);
                                    var descZ = (byte)((childZ * 2) + cz);
                                    var desc = FormatMap(new byte[] { 60, descX, descZ, (byte)(parts[3] - 2) });
                                    if (allMapIds.Contains(desc))
                                    {
                                        relations[desc] = RelationType.Descendant;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else if (gameType == GameType.ArmoredCoreVI)
        {
            //TODO AC6
        }

        Dictionary<string, string> colPatterns = new();
        foreach (var connectParts in connectColMaps)
        {
            var connectMapId = FormatMap(connectParts);
            if (connectParts.Length != 4 || colPatterns.ContainsKey(connectMapId))
            {
                continue;
            }

            colPatterns[connectMapId] = null;
            // DeS, DS1 use wildcards in the last two digits
            // DS2, DS3 use full map ids
            // Bloodborne, Sekiro use wildcards in 0-3 final positions
            // Elden Ring uses wildcards in the final position, and also has the alternate map system (_10 tiles)
            // Not all connections are valid in-game, but include all matches nonetheless, with a few exceptions.
            var firstWildcard = Array.IndexOf(connectParts, (byte)0xFF);
            if (firstWildcard == -1)
            {
                if (allMapIds.Contains(connectMapId))
                {
                    relations[connectMapId] = RelationType.Connection;
                }

                continue;
            }

            if (firstWildcard == 0)
            {
                // Full wildcards are no-ops
                continue;
            }

            if (connectParts.Skip(firstWildcard).Any(p => p != 0xFF))
            {
                // Sanity check for no non-wildcards after wildcards
                continue;
            }

            // Avoid putting in tons of maps. These types of cols are not used in the vanilla game.
            if (gameType == GameType.EldenRing && connectParts[0] == 60 && firstWildcard < 3)
            {
                continue;
            }

            if (gameType == GameType.Bloodborne && connectParts[0] == 29)
            {
                continue;
            }

            if (gameType == GameType.ArmoredCoreVI)
            {
                //TODO AC6
            }

            colPatterns[connectMapId] =
                "^m" + string.Join("_", connectParts.Select(p => p == 0xFF ? @"\d\d" : $"{p:d2}")) + "$";
        }

        if (colPatterns.Count > 0)
        {
            var pattern = string.Join("|", colPatterns.Select(e => e.Value).Where(v => v != null));
            if (pattern.Length > 1)
            {
                Regex re = new(pattern);
                // Add all matching maps, aside from skyboxes
                foreach (var matchingMap in allMapIds.Where(m => re.IsMatch(m) && !m.EndsWith("_99")))
                {
                    relations[matchingMap] = RelationType.Connection;
                }
            }
        }

        return relations;
    }

    private static bool TryInitializeEldenOffsets()
    {
        if (eldenRingOffsets != null)
        {
            return eldenRingOffsets.Count > 0;
        }

        IReadOnlyDictionary<string, Param> loadedParams = ParamBank.PrimaryBank.Params;
        // Do not explicitly check ParamBank's game type here, but fail gracefully if the param does not exist
        if (loadedParams == null || !loadedParams.TryGetValue("WorldMapLegacyConvParam", out Param convParam))
        {
            return false;
        }

        // Now, attempt to populate the offset dictionary. This relies on param field names matching official Paramdex names.
        List<string> srcPartFields = new() { "srcAreaNo", "srcGridXNo", "srcGridZNo" };
        List<string> dstPartFields = new() { "dstAreaNo", "dstGridXNo", "dstGridZNo" };
        // Some maps have multiple incompatible connections with no clear distinguishing characteristics,
        // so these are the authoritative connections, based on manual testing.
        Dictionary<string, string> correctConnects = new()
        {
            // Farum Azula from Bestial Sanctum - not Forge of the Giants (m60_54_53_00) oddly enough
            ["m13_00_00_00"] = "m60_51_43_00",
            // Haligtree from Ordina
            ["m15_00_00_00"] = "m60_48_57_00"
        };
        Dictionary<string, DungeonOffset> dungeonOffsets = new();
        foreach (Param.Row row in convParam.Rows)
        {
            // Dungeon -> World conversions
            // Calculating source (legacy) in terms of destination (overworld)
            if ((byte)row.GetCellHandleOrThrow("isBasePoint").Value == 0)
            {
                continue;
            }

            var dstParts = GetRowMapParts(row, dstPartFields).ToArray();
            if (dstParts[0] != 60)
            {
                continue;
            }

            var srcId = FormatMap(GetRowMapParts(row, srcPartFields));
            var dstId = FormatMap(dstParts);
            if (dungeonOffsets.ContainsKey(srcId)
                || (correctConnects.TryGetValue(srcId, out var trueConnect) && dstId != trueConnect))
            {
                continue;
            }

            Vector3 srcPos = GetRowPosition(row, "srcPos");
            Vector3 dstPos = GetRowPosition(row, "dstPos");
            dungeonOffsets[srcId] = new DungeonOffset
            {
                TileX = dstParts[1], TileZ = dstParts[2], TileOffset = dstPos - srcPos
            };
        }

        foreach (Param.Row row in convParam.Rows)
        {
            // Dungeon -> Dungeon
            // Calculating destination (legacy) in terms of source (already legacy)
            // Only one iteration of this appears to be needed.
            var dstParts = GetRowMapParts(row, dstPartFields).ToArray();
            if (dstParts[0] == 60)
            {
                continue;
            }

            var srcId = FormatMap(GetRowMapParts(row, srcPartFields));
            var dstId = FormatMap(dstParts);
            if (!dungeonOffsets.ContainsKey(dstId) && dungeonOffsets.TryGetValue(srcId, out DungeonOffset val))
            {
                Vector3 srcPos = GetRowPosition(row, "srcPos");
                Vector3 dstPos = GetRowPosition(row, "dstPos");
                dungeonOffsets[dstId] = new DungeonOffset
                {
                    TileX = val.TileX, TileZ = val.TileZ, TileOffset = val.TileOffset + srcPos - dstPos
                };
            }
        }
        // Custom cases

        // Custom case, as this map is only ever loaded from m11_05/m19_00, so assumes the same origin.
        if (dungeonOffsets.ContainsKey("m11_05_00_00"))
        {
            dungeonOffsets["m11_71_00_00"] = dungeonOffsets["m11_05_00_00"];
        }

        // m60_00_00_99's origin matches m60_10_08_02
        dungeonOffsets["m60_00_00_00"] = new DungeonOffset
        {
            TileX = 40, TileZ = 32, TileOffset = new Vector3(384, 0, 384)
        };

        // Colosseums are not connected to any maps, but their in-game map position in emevd matches the overworld colosseums
        if (dungeonOffsets.ContainsKey("m11_00_00_00"))
        {
            DungeonOffset leyndell = dungeonOffsets["m11_00_00_00"];
            dungeonOffsets["m45_00_00_00"] = new DungeonOffset
            {
                TileX = leyndell.TileX,
                TileZ = leyndell.TileZ,
                TileOffset = leyndell.TileOffset + new Vector3(-359.44f, 32.74f, -492.72f)
            };
        }

        dungeonOffsets["m45_01_00_00"] = new DungeonOffset
        {
            TileX = 47, TileZ = 42, TileOffset = new Vector3(-2.34f, 150.4f, -43.36f)
        };
        dungeonOffsets["m45_02_00_00"] = new DungeonOffset
        {
            TileX = 42, TileZ = 40, TileOffset = new Vector3(-24.47f, 208.82f, -66.69f)
        };

        eldenRingOffsets = dungeonOffsets;
        return true;
    }

    private static bool ToEldenGlobalCoords(IList<byte> mapId, Vector3 local, int originTileX, int originTileZ,
        out Vector3 global)
    {
        int tileX, tileZ;
        if (mapId[3] == 99)
        {
            // Treat skybox maps same as their originals
            mapId = mapId.ToArray();
            mapId[3] = 0;
        }

        if (mapId[0] == 60 && mapId[1] > 0 && mapId[2] > 0)
        {
            var scale = mapId[3] % 10;
            var scaleFactor = 1;
            if (scale == 1)
            {
                scaleFactor = 2;
                local += new Vector3(128, 0, 128);
            }
            else if (scale == 2)
            {
                scaleFactor = 4;
                local += new Vector3(384, 0, 384);
            }

            tileX = mapId[1] * scaleFactor;
            tileZ = mapId[2] * scaleFactor;
        }
        else
        {
            var mapIdStr = FormatMap(mapId);
            if (!eldenRingOffsets.TryGetValue(mapIdStr, out DungeonOffset offset))
            {
                global = default;
                return false;
            }

            local += offset.TileOffset;
            tileX = offset.TileX;
            tileZ = offset.TileZ;
        }

        global = local + new Vector3((tileX - originTileX) * 256, 0, (tileZ - originTileZ) * 256);
        return true;
    }

    private static (int, int) GetClosestTile(Vector3 global, int originTileX, int originTileZ)
    {
        return ((int)Math.Round(global.X / 256) + originTileX, (int)Math.Round(global.Z / 256) + originTileZ);
    }

    private static bool TryParseMap(string map, out byte[] parts)
    {
        try
        {
            parts = map.TrimStart('m').Split('_').Select(p => byte.Parse(p)).ToArray();
            if (parts.Length == 4)
            {
                return true;
            }
        }
        catch (Exception ex) when (ex is FormatException || ex is OverflowException)
        {
        }

        parts = null;
        return false;
    }

    private static string FormatMap(IEnumerable<byte> parts)
    {
        return "m" + string.Join("_", parts.Select(p => p == 0xFF ? "XX" : $"{p:d2}"));
    }

    private static List<byte> GetRowMapParts(Param.Row row, List<string> fields)
    {
        List<byte> bytes = fields.Select(f => (byte)row.GetCellHandleOrThrow(f).Value).ToList();
        while (bytes.Count < 4)
        {
            bytes.Add(0);
        }

        return bytes;
    }

    private static Vector3 GetRowPosition(Param.Row row, string type)
    {
        return new Vector3((float)row.GetCellHandleOrThrow($"{type}X").Value,
            (float)row.GetCellHandleOrThrow($"{type}Y").Value,
            (float)row.GetCellHandleOrThrow($"{type}Z").Value);
    }

    private class DungeonOffset
    {
        public int TileX { get; set; }
        public int TileZ { get; set; }
        public Vector3 TileOffset { get; set; }
    }
}
