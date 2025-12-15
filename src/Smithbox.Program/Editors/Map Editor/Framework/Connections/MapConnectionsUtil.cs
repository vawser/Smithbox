using Andre.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Common util functions used by the MapConnections classes
/// </summary>
public static class MapConnectionsUtil
{
    public static string FormatMap(IEnumerable<byte> parts)
    {
        return "m" + string.Join("_", parts.Select(p => p == 0xFF ? "XX" : $"{p:d2}"));
    }

    public static List<byte> GetRowMapParts(Param.Row row, List<string> fields)
    {
        List<byte> bytes = fields.Select(f => (byte)row.GetCellHandleOrThrow(f).Value).ToList();
        while (bytes.Count < 4)
        {
            bytes.Add(0);
        }

        return bytes;
    }

    public static Vector3 GetRowPosition(Param.Row row, string type)
    {
        return new Vector3((float)row.GetCellHandleOrThrow($"{type}X").Value,
            (float)row.GetCellHandleOrThrow($"{type}Y").Value,
            (float)row.GetCellHandleOrThrow($"{type}Z").Value);
    }

    public static bool TryParseMap(string map, out byte[] parts)
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
    public static (int, int) GetClosestTile(Vector3 global, int originTileX, int originTileZ, int xDiv = 256, int zDiv = 256)
    {
        return ((int)Math.Round(global.X / xDiv) + originTileX, (int)Math.Round(global.Z / zDiv) + originTileZ);
    }
}
