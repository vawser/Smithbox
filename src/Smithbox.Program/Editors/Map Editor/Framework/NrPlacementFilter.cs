using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The per-session placement rule of maps whose parts have a Variation and a RandomAppearParamID (Nightreign), as
/// traced in nightreign.exe (Claude/Documentation/MSB Research.md §5.4a). The game decides when a map loads which
/// assets and enemies exist this session:
/// - Variation −1 is always placed; otherwise only when it equals the map's variation. An asset also matches
///   Variation + 10 when its VariationAlsoPlus10 is on, in the maps whose block allows it (m16_00, m19_00).
/// - RandomAppearParamID 0 is always placed; otherwise only when the session's random slot (0–99) is set in that
///   RandomAppearParam row. A missing row counts as set.
/// Other part types aren't filtered: the exe's placement tests were traced for assets and characters only.
/// </summary>
public static class NrPlacementFilter
{
    public const string VariationField = "EntityData.Variation";
    public const string AlsoPlus10Field = "EntityData.VariationAlsoPlus10";
    public const string RandomAppearField = "EntityData.RandomAppearParamID";

    public const string RandomAppearParam = "RandomAppearParam";
    public const int SlotCount = 100;

    /// <summary>
    /// The maps whose block lets an asset also match Variation + 10 (WorldBlockInfo +4Dh, set only for these two)
    /// </summary>
    private static readonly HashSet<string> AlsoPlus10Maps = new(StringComparer.OrdinalIgnoreCase)
    {
        "m16_00_00_00",
        "m19_00_00_00"
    };

    /// <summary>
    /// The placement fields of a part. False if the part has no Variation field (other games, or a part type the
    /// rule doesn't cover).
    /// </summary>
    public static bool TryGetFields(Entity entity, out PlacementFields fields)
    {
        fields = default;
        return entity?.WrappedObject != null && TryGetFields(entity.WrappedObject, out fields);
    }

    /// <summary>
    /// As <see cref="TryGetFields(Entity, out PlacementFields)"/>, for an MSB part read outside the editor
    /// </summary>
    public static bool TryGetFields(object part, out PlacementFields fields)
    {
        fields = default;

        if (part == null)
            return false;

        var isAsset = EntityHelper.IsPartAsset(part);

        if (!isAsset && !EntityHelper.IsPartEnemy(part))
            return false;

        if (!MsbEntryColumns.TryGetValue(part, VariationField, out var variation) || variation is not IConvertible v)
            return false;

        var alsoPlus10 = isAsset &&
            MsbEntryColumns.TryGetValue(part, AlsoPlus10Field, out var plus) && plus is IConvertible p && p.ToInt64(null) != 0;

        var randomAppear = MsbEntryColumns.TryGetValue(part, RandomAppearField, out var random) && random is IConvertible r
            ? r.ToInt32(null)
            : 0;

        fields = new PlacementFields(v.ToInt32(null), alsoPlus10, randomAppear);
        return true;
    }

    /// <summary>
    /// Whether the map's block lets assets match Variation + 10
    /// </summary>
    public static bool AllowsAlsoPlus10(string mapId) => mapId != null && AlsoPlus10Maps.Contains(mapId);

    /// <summary>
    /// Whether a part with these fields is placed. Null for the map variation or the slot means "any": that filter
    /// passes everything.
    /// </summary>
    public static bool IsPlaced(PlacementFields fields, string mapId, int? mapVariation, int? slot, RandomAppearTable table)
    {
        if (mapVariation is { } block && fields.Variation >= 0)
        {
            var matches = fields.Variation == block ||
                (fields.AlsoPlus10 && AllowsAlsoPlus10(mapId) && fields.Variation + 10 == block);

            if (!matches)
                return false;
        }

        if (slot is { } s && fields.RandomAppearParamID != 0 && table != null && !table.IsSet(fields.RandomAppearParamID, s))
            return false;

        return true;
    }

    /// <summary>
    /// The arena maps whose block variation is the expedition target (MSB Research.md §5.4a, setter 1406784B0)
    /// </summary>
    private static readonly HashSet<string> ArenaMaps = new(StringComparer.OrdinalIgnoreCase)
    {
        "m16_00_00_00",
        "m16_10_00_00",
        "m18_00_00_00",
        "m19_00_00_00"
    };

    /// <summary>
    /// Whether a map's block variation can be anything but 0: the arenas, and the small-base maps (m30–m49
    /// mAA_BB_00_00, which a lot result's Variation ID sets). In every other block a part with Variation 1 or more is
    /// never placed.
    /// </summary>
    public static bool HasSessionVariation(string mapId)
    {
        if (mapId == null)
            return false;

        if (ArenaMaps.Contains(mapId))
            return true;

        return TryGetSmallBaseId(mapId, out var id) && id / 100 is >= 30 and <= 49;
    }

    public const string ScenarioKey0Field = "UnkA8Data.ScenarioPlacementKey0";
    public const string ScenarioKey1Field = "UnkA8Data.ScenarioPlacementKey1";
    public const string ScenarioStepField = "UnkA8Data.ScenarioPlacementStep";

    /// <summary>
    /// A part's ScenarioPlacementParam key triple, when it has one
    /// </summary>
    public static bool TryGetScenarioKeys(object part, out int key0, out int key1, out int step)
    {
        key0 = key1 = step = -1;

        if (!MsbEntryColumns.TryGetValue(part, ScenarioKey0Field, out var k0) || k0 is not IConvertible c0 ||
            !MsbEntryColumns.TryGetValue(part, ScenarioKey1Field, out var k1) || k1 is not IConvertible c1 ||
            !MsbEntryColumns.TryGetValue(part, ScenarioStepField, out var s) || s is not IConvertible cs)
            return false;

        key0 = c0.ToInt32(null);
        key1 = c1.ToInt32(null);
        step = cs.ToInt32(null);
        return true;
    }

    /// <summary>
    /// The small-base ID LotResultSmallBaseAndSpot's smallBaseMapId uses for a map: mAA_BB_00_00 → AABB
    /// </summary>
    public static bool TryGetSmallBaseId(string mapId, out int id)
    {
        id = 0;

        if (mapId == null || mapId.Length != 12 || !mapId.EndsWith("_00_00", StringComparison.Ordinal) ||
            !int.TryParse(mapId.AsSpan(1, 2), out var aa) || !int.TryParse(mapId.AsSpan(4, 2), out var bb))
            return false;

        id = aa * 100 + bb;
        return true;
    }

    /// <summary>
    /// How many LotResultSmallBaseAndSpot rows pick each (small base, variation). Null when the project has no such
    /// param, or it lacks the smallBaseMapId / variationId fields.
    /// </summary>
    public static Dictionary<(int Map, int Variation), int> BuildLotCounts(ProjectEntry project)
    {
        if (project?.Handler?.ParamData?.PrimaryBank?.Params is not { } bank ||
            !bank.TryGetValue("LotResultSmallBaseAndSpot", out var param) || param.Rows.Count == 0)
            return null;

        var first = param.Rows.First();

        if (!CharacterComposition.HasField(first, "smallBaseMapId") || !CharacterComposition.HasField(first, "variationId"))
            return null;

        var counts = new Dictionary<(int, int), int>();

        foreach (var row in param.Rows)
        {
            var key = (CharacterComposition.GetValue(row, "smallBaseMapId"), CharacterComposition.GetValue(row, "variationId"));
            counts[key] = counts.GetValueOrDefault(key) + 1;
        }

        return counts;
    }

    /// <summary>
    /// The map variations that place a part with these fields, as shown in the variation picker: its Variation,
    /// and Variation + 10 for an asset that also matches it
    /// </summary>
    public static IEnumerable<int> MatchingVariations(PlacementFields fields, string mapId)
    {
        if (fields.Variation < 0)
            yield break;

        yield return fields.Variation;

        if (fields.AlsoPlus10 && AllowsAlsoPlus10(mapId))
            yield return fields.Variation + 10;
    }
}

public readonly record struct PlacementFields(int Variation, bool AlsoPlus10, int RandomAppearParamID);

/// <summary>
/// RandomAppearParam as one bit set per row: slot0–slot99. Built once from the param bank and rebuilt when asked.
/// The param has duplicate row IDs; the first row of an ID is used.
/// </summary>
public class RandomAppearTable
{
    private readonly Dictionary<int, BitArray> Rows = new();

    public static RandomAppearTable Build(ProjectEntry project)
    {
        if (project?.Handler?.ParamData?.PrimaryBank?.Params is not { } bank ||
            !bank.TryGetValue(NrPlacementFilter.RandomAppearParam, out var param))
            return null;

        var table = new RandomAppearTable();

        foreach (var row in param.Rows)
        {
            if (table.Rows.ContainsKey(row.ID))
                continue;

            var bits = new BitArray(NrPlacementFilter.SlotCount);

            foreach (var cell in row.Cells)
            {
                var name = cell.Def.InternalName;

                if (!name.StartsWith("slot", StringComparison.OrdinalIgnoreCase) ||
                    !int.TryParse(name.AsSpan(4), out var index) || index < 0 || index >= NrPlacementFilter.SlotCount)
                    continue;

                if (cell.Value is IConvertible value && value.ToInt64(null) != 0)
                    bits[index] = true;
            }

            table.Rows[row.ID] = bits;
        }

        return table;
    }

    /// <summary>
    /// Whether the row has the slot set. A missing row counts as set, as in the game.
    /// </summary>
    public bool IsSet(int rowId, int slot)
    {
        if (!Rows.TryGetValue(rowId, out var bits))
            return true;

        return slot >= 0 && slot < bits.Length && bits[slot];
    }

    /// <summary>
    /// The row whose slots are exactly the ones this row doesn't have (its other half), or null
    /// </summary>
    public int? FindComplement(int rowId)
    {
        if (!Rows.TryGetValue(rowId, out var bits))
            return null;

        foreach (var (id, other) in Rows)
        {
            if (id == rowId)
                continue;

            var complement = true;

            for (var i = 0; i < bits.Length && complement; i++)
            {
                if (bits[i] == other[i])
                    complement = false;
            }

            if (complement)
                return id;
        }

        return null;
    }

    /// <summary>
    /// How many of the 100 slots the row has, or -1 if there is no such row
    /// </summary>
    public int SlotCountOf(int rowId)
    {
        if (!Rows.TryGetValue(rowId, out var bits))
            return -1;

        var count = 0;

        for (var i = 0; i < bits.Length; i++)
        {
            if (bits[i])
                count++;
        }

        return count;
    }
}
