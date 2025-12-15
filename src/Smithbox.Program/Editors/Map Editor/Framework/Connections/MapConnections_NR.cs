using Andre.Formats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Business logic for cross-map connections in NR.
/// </summary>
internal class MapConnections_NR
{
    private static Dictionary<string, TileDefinition> NightreignOffsets;

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

    private static bool TryInitializeOffsets(MapEditorScreen editor)
    {
        if (NightreignOffsets != null)
        {
            return NightreignOffsets.Count > 0;
        }

        if (editor.Project.ParamEditor == null)
            return false;

        Dictionary<string, TileDefinition> dungeonOffsets = new();


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

        };

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

        NightreignOffsets = dungeonOffsets;
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
            if (!NightreignOffsets.TryGetValue(mapIdStr, out TileDefinition offset))
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
