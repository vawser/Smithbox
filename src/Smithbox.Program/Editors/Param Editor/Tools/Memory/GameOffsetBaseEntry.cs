using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public class GameOffsetBaseEntry
{
    // AOB for param base offset. If null, ParamBaseOffset will be used instead.
    public string ParamBaseAobPattern;
    public List<(int, int)> ParamBaseAobRelativeOffsets = new();

    // Hard offset for param base. Unused if ParamBase AOB is set.
    public int ParamBaseOffset = 0;

    // Empty when the repository slot points straight at the PARAM header (DS2S).
    public int[] paramInnerPath = Array.Empty<int>();
    public int paramCountOffset;
    public int paramDataOffset;

    public int rowPointerOffset;
    public int rowHeaderSize;
    public int? ERItemGiveFuncOffset = null;
    public int? ERMapItemManOffset = null;

    // Hops after [paramBase] and before the per-param offset. Empty for most games.
    public int[] paramOuterPath = Array.Empty<int>();

    // Per-map params (<prefix>_mAA_BB_CC_DD): offset by prefix, and the map ID's offset in the outer object.
    public Dictionary<string, int> mapParamOffsets = new();
    public int? MapIdOffset = null;

    public Dictionary<string, int> paramOffsets = new();
    public Dictionary<string, int> itemIDCategories = new();

    private static int[] ParsePath(string path)
    {
        return path.Split("/").Select(Utils.ParseHexFromString).ToArray();
    }

    private static readonly Regex MapParamName = new(@"^(?<prefix>.+)_m(?<a>\d{2})_(?<b>\d{2})_(?<c>\d{2})_(?<d>\d{2})$");

    /// <summary>
    /// Offset of a param in this base: by exact name, or by prefix for a per-map param.
    /// mapId is the map the param belongs to (0xAABBCCDD), when it is a per-map param.
    /// </summary>
    public bool TryGetParamOffset(string name, out int offset, out uint? mapId)
    {
        mapId = null;
        if (name == null)
        {
            offset = 0;
            return false;
        }

        if (paramOffsets.TryGetValue(name, out offset))
            return true;

        if (mapParamOffsets.Count > 0)
        {
            var m = MapParamName.Match(name);
            if (m.Success && mapParamOffsets.TryGetValue(m.Groups["prefix"].Value, out offset))
            {
                mapId = (uint.Parse(m.Groups["a"].Value) << 24) | (uint.Parse(m.Groups["b"].Value) << 16)
                      | (uint.Parse(m.Groups["c"].Value) << 8) | uint.Parse(m.Groups["d"].Value);
                return true;
            }
        }

        return false;
    }

    public static string FormatMapId(uint id)
    {
        return $"m{id >> 24:D2}_{(id >> 16) & 0xFF:D2}_{(id >> 8) & 0xFF:D2}_{id & 0xFF:D2}";
    }

    public GameOffsetBaseEntry() { }

    public void Fill(GameOffsetBase data)
    {
        paramOffsets = new();
        itemIDCategories = new();

        if (!string.IsNullOrEmpty(data.paramBase))
        {
            ParamBaseOffset = Utils.ParseHexFromString(data.paramBase);
        }

        if (!string.IsNullOrEmpty(data.paramBaseAob))
        {
            ParamBaseAobPattern = data.paramBaseAob;
        }

        if (!string.IsNullOrEmpty(data.paramBaseAobRelativeOffset))
        {
            foreach (var relativeOffset in data.paramBaseAobRelativeOffset.Split(','))
            {
                var split = relativeOffset.Split('/');
                ParamBaseAobRelativeOffsets.Add(new(Utils.ParseHexFromString(split[0]), Utils.ParseHexFromString(split[1])));
            }
        }

        if (!string.IsNullOrEmpty(data.paramInnerPath))
        {
            paramInnerPath = ParsePath(data.paramInnerPath);
        }

        if (!string.IsNullOrEmpty(data.paramOuterPath))
        {
            paramOuterPath = ParsePath(data.paramOuterPath);
        }

        if (!string.IsNullOrEmpty(data.mapIdOffset))
        {
            MapIdOffset = Utils.ParseHexFromString(data.mapIdOffset);
        }

        foreach (var entry in data.mapParamOffsets ?? new())
        {
            var split = entry.Split(':');
            mapParamOffsets.Add(split[0], Utils.ParseHexFromString(split[1]));
        }

        if (!string.IsNullOrEmpty(data.paramCountOffset))
        {
            paramCountOffset = Utils.ParseHexFromString(data.paramCountOffset);
        }

        if (!string.IsNullOrEmpty(data.paramDataOffset))
        {
            paramDataOffset = Utils.ParseHexFromString(data.paramDataOffset);
        }

        if (!string.IsNullOrEmpty(data.rowPointerOffset))
        {
            rowPointerOffset = Utils.ParseHexFromString(data.rowPointerOffset);
        }

        if (!string.IsNullOrEmpty(data.rowHeaderSize))
        {
            rowHeaderSize = Utils.ParseHexFromString(data.rowHeaderSize);
        }

        foreach (var entry in data.paramOffsets)
        {
            var name = entry.Split(':')[0];
            var address = entry.Split(':')[1];

            paramOffsets.Add(name, Utils.ParseHexFromString(address));
        }

        foreach (var entry in data.itemIDCategories)
        {
            var name = entry.Split(':')[0];
            var address = entry.Split(':')[1];

            itemIDCategories.Add(name, Utils.ParseHexFromString(address));
        }

        if (!string.IsNullOrEmpty(data.ERItemGiveFuncOffset))
        {
            ERItemGiveFuncOffset = Utils.ParseHexFromString(data.ERItemGiveFuncOffset);
        }
        if (!string.IsNullOrEmpty(data.ERMapItemManOffset))
        {
            ERMapItemManOffset = Utils.ParseHexFromString(data.ERMapItemManOffset);
        }
    }
}
