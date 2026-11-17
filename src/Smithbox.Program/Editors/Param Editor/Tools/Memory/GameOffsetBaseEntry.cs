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
    // Item Gib. Null layout = this base has no give-item function.
    public ItemGiveLayout? ItemGiveLayout = null;
    public int ItemGiveMaxEntries = 1;
    public int ItemGiveSource = 0;
    public ItemGiveArg ItemGiveR9 = new(false, 0);
    public ItemGiveArg[] ItemGiveStackArgs = Array.Empty<ItemGiveArg>();
    public ItemGiveAddress ItemGiveFunc = new();
    public ItemGiveAddress ItemGiveManager = new();
    public List<ItemGivePrepare> ItemGivePrepares = new();

    // SpEffect Gib. Null = not supported by this base.
    public SpEffectGive SpEffectGive = null;

    public bool SupportsItemGive => ItemGiveLayout != null && ItemGiveFunc.IsSet && ItemGiveManager.IsSet;

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

        ParamBaseAobRelativeOffsets = ParseRelativeOffsets(data.paramBaseAobRelativeOffset);

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

        if (Enum.TryParse<ItemGiveLayout>(data.itemGiveLayout, true, out var layout))
        {
            ItemGiveLayout = layout;
        }

        if (!string.IsNullOrEmpty(data.itemGiveMaxEntries))
        {
            ItemGiveMaxEntries = Math.Max(1, int.Parse(data.itemGiveMaxEntries));
        }

        if (!string.IsNullOrEmpty(data.itemGiveSource))
        {
            ItemGiveSource = Utils.ParseHexFromString(data.itemGiveSource);
        }

        if (!string.IsNullOrEmpty(data.itemGiveR9))
        {
            ItemGiveR9 = ItemGiveArg.Parse(data.itemGiveR9);
        }

        if (!string.IsNullOrEmpty(data.itemGiveStackArgs))
        {
            ItemGiveStackArgs = data.itemGiveStackArgs.Split(',').Select(ItemGiveArg.Parse).ToArray();
        }

        ItemGiveFunc = ItemGiveAddress.Parse(data.itemGiveFuncAob, data.itemGiveFuncAobRelativeOffset, data.itemGiveFuncOffset);
        ItemGiveManager = ItemGiveAddress.Parse(data.itemGiveManagerAob, data.itemGiveManagerAobRelativeOffset, data.itemGiveManagerOffset);

        if (data.spEffectGive is { } sp)
        {
            var give = new SpEffectGive
            {
                Param = sp.param,
                Func = ItemGiveAddress.Parse(sp.funcAob, sp.funcAobRelativeOffset, sp.funcOffset),
                ChrManager = ItemGiveAddress.Parse(sp.chrManagerAob, sp.chrManagerAobRelativeOffset, sp.chrManagerOffset),
                PlayerPath = string.IsNullOrEmpty(sp.playerPath) ? Array.Empty<int>() : ParsePath(sp.playerPath),
                Flag = string.IsNullOrEmpty(sp.flag) ? 0 : Utils.ParseHexFromString(sp.flag),
                Xmm2 = float.TryParse(sp.xmm2, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var f) ? f : null,
            };
            if (give.IsSupported)
                SpEffectGive = give;
        }

        foreach (var p in data.itemGivePrepare ?? new())
        {
            ItemGivePrepares.Add(new ItemGivePrepare
            {
                Param = p.param,
                Lookup = ItemGiveAddress.Parse(p.lookupFuncAob, p.lookupFuncAobRelativeOffset, p.lookupFuncOffset),
                Prepare = ItemGiveAddress.Parse(p.prepareFuncAob, p.prepareFuncAobRelativeOffset, p.prepareFuncOffset),
                Slots = (p.slots ?? new()).Select(s => s.Split(':')).Select(s => new ItemGiveSlot(
                    s[0], Utils.ParseHexFromString(s[1]), s.Length > 2 ? s[2] : null, s.Length > 3 ? s[3] : null)).ToList(),
                SlotPoolParam = p.slotPoolParam,
                SlotPoolField = p.slotPoolField,
                SlotPoolWeightField = p.slotPoolWeightField,
                SlotValueParam = p.slotValueParam,
            });
        }
    }

    /// <summary>
    /// "0x03/0x07,..." → [(start, end)]: each pair reads a rel32 at match+start and resolves it against match+end.
    /// </summary>
    public static List<(int, int)> ParseRelativeOffsets(string str)
    {
        var result = new List<(int, int)>();
        if (string.IsNullOrEmpty(str))
            return result;

        foreach (var relativeOffset in str.Split(','))
        {
            var split = relativeOffset.Split('/');
            result.Add((Utils.ParseHexFromString(split[0]), Utils.ParseHexFromString(split[1])));
        }
        return result;
    }
}

public enum ItemGiveLayout
{
    // int count; { int id; int quantity; int durability; }[count]. Status: 16 zero bytes. (DS3)
    Durability,
    // int count; { int id; int quantity; int -1; int gemId; }[maxEntries]. Status: { -1, 0, 0 }. (ER)
    Gem,
    // int count; { int id; int quantity; int -1; int itemGiveSource; }[maxEntries]. Status: 16 zero bytes. (SDT)
    Source,
    // byte count; pad[7]; ItemDescriptor[maxEntries] (0x50 bytes: id at +0, u16 quantity at +0x48,
    // the other fields at their constructor defaults). Status: { -1, 0, 0 }. (NR)
    Descriptor,
    // int count; { int id; int quantity; int -1; }[maxEntries]. Status: 16 zero bytes (unread). (AC6)
    Basic,
}

/// <summary>
/// A register / stack argument of the give-item call: an immediate, or a pointer to a scratch block.
/// </summary>
public readonly record struct ItemGiveArg(bool Scratch, long Value)
{
    public static ItemGiveArg Parse(string str)
    {
        str = str.Trim();
        if (str.Equals("scratch", StringComparison.OrdinalIgnoreCase))
            return new ItemGiveArg(true, 0);

        return new ItemGiveArg(false, Utils.ParseHexFromString(str));
    }
}

/// <summary>
/// SpEffect Gib: Func(player ChrIns*, spEffectId, Flag), the player being [ChrManager] followed through PlayerPath.
/// </summary>
public class SpEffectGive
{
    public string Param;
    public ItemGiveAddress Func = new();
    public ItemGiveAddress ChrManager = new();
    public int[] PlayerPath = Array.Empty<int>();
    public int Flag;
    public float? Xmm2;

    public bool IsSupported => !string.IsNullOrEmpty(Param) && Func.IsSet && ChrManager.IsSet;
}

/// <summary>
/// Calls made for each item of Param before the give: Lookup(&rowRef, rowId), then Prepare(&rowRef, &descriptor).
/// </summary>
public class ItemGivePrepare
{
    public string Param;
    public ItemGiveAddress Lookup = new();
    public ItemGiveAddress Prepare = new();

    // Designer slots: the row field holding the pool ID, and the descriptor offset the chosen value is written to.
    public List<ItemGiveSlot> Slots = new();
    public string SlotPoolParam;
    public string SlotPoolField;
    public string SlotPoolWeightField;
    public string SlotValueParam;

    public bool HasSlots => Slots.Count > 0 && SlotPoolParam != null && SlotPoolField != null && SlotValueParam != null;
}

/// <summary>
/// A designer slot, from "field:offset:kind:number". Kind and number label it in the UI
/// (PARAM_Tools_ItemGib_Slot_{Kind} with {0} = number); without them the field name is shown.
/// </summary>
public record ItemGiveSlot(string Field, int Offset, string Kind, string Number);

/// <summary>
/// A module-relative address found by AOB (preferred) or given as a hard offset.
/// </summary>
public class ItemGiveAddress
{
    public string Aob;
    public List<(int, int)> AobRelativeOffsets = new();
    public int? Offset;

    public bool IsSet => !string.IsNullOrEmpty(Aob) || Offset != null;

    public static ItemGiveAddress Parse(string aob, string relativeOffsets, string offset)
    {
        return new ItemGiveAddress
        {
            Aob = string.IsNullOrEmpty(aob) ? null : aob,
            AobRelativeOffsets = GameOffsetBaseEntry.ParseRelativeOffsets(relativeOffsets),
            Offset = string.IsNullOrEmpty(offset) ? null : Utils.ParseHexFromString(offset),
        };
    }
}
