using Andre.Formats;
using DotNext.Collections.Generic;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Business logic for cross-map connections in ER.
/// </summary>
internal class MapConnections_ER
{
    private static Dictionary<string, TileDefinition> eldenRingOffsets;

    public static Transform? GetMapTransform(
        MapEditorScreen editor,
        string mapid)
    {
        if (!TryInitializeOffsets(editor))
        {
            return null;
        }

        if (!MapConnectionsUtil.TryParseMap(mapid, out var target) ||
            !ToGlobalCoords(target, Vector3.Zero, 0, 0, out Vector3 targetGlobal))
        {
            return null;
        }

        (var originX, var originZ) = MapConnectionsUtil.GetClosestTile(targetGlobal, 0, 0);
        // Recenter target in terms of closest tile center, for maximum precision
        if (!ToGlobalCoords(target, Vector3.Zero, originX, originZ, out targetGlobal))
        {
            return null;
        }

        var closestDistSq = float.PositiveInfinity;
        Vector3 closestOriginGlobal = Vector3.Zero;
        ObjectContainer closestMap = null;

        foreach (var entry in editor.Project.MapData.PrimaryBank.Maps)
        {
            var mapID = entry.Key.Filename;
            var container = entry.Value.MapContainer;

            // Added since the old load flow meant the MapContainer for the target map was always added after this,
            // whereas now the MapContainers are always present, so we need to skip self matches.
            if (mapid == mapID)
                continue;

            if (container == null)
                continue;

            if (!container.RootObject.HasTransform
                || !MapConnectionsUtil.TryParseMap(mapID, out var origin)
                || !ToGlobalCoords(origin, Vector3.Zero, originX, originZ, out Vector3 originGlobal))
            {
                continue;
            }

            var distSq = Vector3.DistanceSquared(targetGlobal, originGlobal);
            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closestOriginGlobal = originGlobal;
                closestMap = container;
            }
        }

        if (closestMap == null)
        {
            return null;
        }

        Vector3 targetOffset = targetGlobal - closestOriginGlobal;
        return closestMap.RootObject.GetLocalTransform() + targetOffset;
    }

    public static IReadOnlyDictionary<string, MapConnectionRelationType> GetRelatedMaps(
        MapEditorScreen editor,
        string mapid,
        List<byte[]> connectColMaps = null)
    {
        var allMapIds = editor.Project.MapData.MapFiles.Entries.Select(e => e.Filename).ToList();

        connectColMaps ??= new List<byte[]>();
        SortedDictionary<string, MapConnectionRelationType> relations = new();
        if (!MapConnectionsUtil.TryParseMap(mapid, out var parts))
        {
            return relations;
        }

        if (editor.Project.ProjectType == ProjectType.ER && (parts[0] == 60 || parts[0] == 61) && parts[1] > 0 && parts[2] > 0)
        {
            var topIndex = parts[0];

            var scale = parts[3] % 10;
            if (scale < 2)
            {
                var tileX = parts[1];
                var tileZ = parts[2];
                tileX /= 2;
                tileZ /= 2;

                var parent = "";

                parent = MapConnectionsUtil.FormatMap(new byte[] { topIndex, tileX, tileZ, (byte)(parts[3] + 1) });

                if (allMapIds.Contains(parent))
                {
                    relations[parent] = MapConnectionRelationType.Parent;
                    if (scale == 0)
                    {
                        tileX /= 2;
                        tileZ /= 2;
                        var ancestor = MapConnectionsUtil.FormatMap(new byte[] { topIndex, tileX, tileZ, (byte)(parts[3] + 2) });
                        if (allMapIds.Contains(ancestor))
                        {
                            relations[ancestor] = MapConnectionRelationType.Ancestor;
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
                        var childX = (byte)(tileX * 2 + x);
                        var childZ = (byte)(tileZ * 2 + z);
                        var child = MapConnectionsUtil.FormatMap(new byte[] { topIndex, childX, childZ, (byte)(parts[3] - 1) });
                        if (allMapIds.Contains(child))
                        {
                            relations[child] = MapConnectionRelationType.Child;
                            if (scale != 2)
                            {
                                continue;
                            }

                            for (var cx = 0; cx <= 1; cx++)
                            {
                                for (var cz = 0; cz <= 1; cz++)
                                {
                                    var descX = (byte)(childX * 2 + cx);
                                    var descZ = (byte)(childZ * 2 + cz);
                                    var desc = MapConnectionsUtil.FormatMap(new byte[] { topIndex, descX, descZ, (byte)(parts[3] - 2) });
                                    if (allMapIds.Contains(desc))
                                    {
                                        relations[desc] = MapConnectionRelationType.Descendant;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        Dictionary<string, string> colPatterns = new();
        foreach (var connectParts in connectColMaps)
        {
            var connectMapId = MapConnectionsUtil.FormatMap(connectParts);
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
                    relations[connectMapId] = MapConnectionRelationType.Connection;
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
            if (editor.Project.ProjectType == ProjectType.ER && (connectParts[0] == 60 || connectParts[0] == 61) && firstWildcard < 3)
            {
                continue;
            }

            if (editor.Project.ProjectType == ProjectType.BB && connectParts[0] == 29)
            {
                continue;
            }

            if (editor.Project.ProjectType == ProjectType.AC6)
            {
                //TODO AC6
            }

            if (editor.Project.ProjectType == ProjectType.NR)
            {
                //TODO NR
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
                    relations[matchingMap] = MapConnectionRelationType.Connection;
                }
            }
        }

        return relations;
    }

    private static bool TryInitializeOffsets(MapEditorScreen editor)
    {
        if (eldenRingOffsets != null)
        {
            return eldenRingOffsets.Count > 0;
        }

        if (editor.Project.ParamEditor == null)
            return false;

        IReadOnlyDictionary<string, Param> loadedParams = editor.Project.ParamData.PrimaryBank.Params;
        // Do not explicitly check ParamBank's game type here, but fail gracefully if the param does not exist
        if (loadedParams == null || !loadedParams.TryGetValue("WorldMapLegacyConvParam", out Param convParam))
        {
            return false;
        }

        // Now, attempt to populate the offset dictionary. This relies on param field names matching official PARAM names.
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
        Dictionary<string, TileDefinition> dungeonOffsets = new();
        foreach (Param.Row row in convParam.Rows)
        {
            // Dungeon -> World conversions
            // Calculating source (legacy) in terms of destination (overworld)
            if ((byte)row.GetCellHandleOrThrow("isBasePoint").Value == 0)
            {
                continue;
            }

            var dstParts = MapConnectionsUtil.GetRowMapParts(row, dstPartFields).ToArray();
            if (dstParts[0] != 60 && dstParts[0] != 61)
            {
                continue;
            }

            var srcId = MapConnectionsUtil.FormatMap(MapConnectionsUtil.GetRowMapParts(row, srcPartFields));
            var dstId = MapConnectionsUtil.FormatMap(dstParts);
            if (dungeonOffsets.ContainsKey(srcId)
                || correctConnects.TryGetValue(srcId, out var trueConnect) && dstId != trueConnect)
            {
                continue;
            }

            Vector3 srcPos = MapConnectionsUtil.GetRowPosition(row, "srcPos");
            Vector3 dstPos = MapConnectionsUtil.GetRowPosition(row, "dstPos");
            dungeonOffsets[srcId] = new TileDefinition
            {
                TileX = dstParts[1],
                TileZ = dstParts[2],
                TileOffset = dstPos - srcPos
            };
        }

        foreach (Param.Row row in convParam.Rows)
        {
            // Dungeon -> Dungeon
            // Calculating destination (legacy) in terms of source (already legacy)
            // Only one iteration of this appears to be needed.
            var dstParts = MapConnectionsUtil.GetRowMapParts(row, dstPartFields).ToArray();
            if (dstParts[0] == 60 || dstParts[0] == 61)
            {
                continue;
            }

            var srcId = MapConnectionsUtil.FormatMap(MapConnectionsUtil.GetRowMapParts(row, srcPartFields));
            var dstId = MapConnectionsUtil.FormatMap(dstParts);
            if (!dungeonOffsets.ContainsKey(dstId) && dungeonOffsets.TryGetValue(srcId, out TileDefinition val))
            {
                Vector3 srcPos = MapConnectionsUtil.GetRowPosition(row, "srcPos");
                Vector3 dstPos = MapConnectionsUtil.GetRowPosition(row, "dstPos");
                dungeonOffsets[dstId] = new TileDefinition
                {
                    TileX = val.TileX,
                    TileZ = val.TileZ,
                    TileOffset = val.TileOffset + srcPos - dstPos
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
        dungeonOffsets["m60_00_00_00"] = new TileDefinition
        {
            TileX = 40,
            TileZ = 32,
            TileOffset = new Vector3(384, 0, 384)
        };

        // Colosseums are not connected to any maps, but their in-game map position in emevd matches the overworld colosseums
        if (dungeonOffsets.ContainsKey("m11_00_00_00"))
        {
            TileDefinition leyndell = dungeonOffsets["m11_00_00_00"];
            dungeonOffsets["m45_00_00_00"] = new TileDefinition
            {
                TileX = leyndell.TileX,
                TileZ = leyndell.TileZ,
                TileOffset = leyndell.TileOffset + new Vector3(-359.44f, 32.74f, -492.72f)
            };
        }

        dungeonOffsets["m45_01_00_00"] = new TileDefinition
        {
            TileX = 47,
            TileZ = 42,
            TileOffset = new Vector3(-2.34f, 150.4f, -43.36f)
        };
        dungeonOffsets["m45_02_00_00"] = new TileDefinition
        {
            TileX = 42,
            TileZ = 40,
            TileOffset = new Vector3(-24.47f, 208.82f, -66.69f)
        };

        eldenRingOffsets = dungeonOffsets;
        return true;
    }

    private static bool ToGlobalCoords(IList<byte> mapId, Vector3 local, int originTileX, int originTileZ,
        out Vector3 global)
    {
        int tileX, tileZ;
        if (mapId[3] == 99)
        {
            // Treat skybox maps same as their originals
            mapId = mapId.ToArray();
            mapId[3] = 0;
        }

        if ((mapId[0] == 60 || mapId[0] == 61) && mapId[1] > 0 && mapId[2] > 0)
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
            var mapIdStr = MapConnectionsUtil.FormatMap(mapId);
            if (!eldenRingOffsets.TryGetValue(mapIdStr, out TileDefinition offset))
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
}
