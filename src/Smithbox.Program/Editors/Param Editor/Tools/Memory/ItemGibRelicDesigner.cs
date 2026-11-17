using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Item Gib designer: lets the user pick the value of each slot of a prepared descriptor
/// (NR relics: which effect goes in each effect / curse slot). The slots, their pools and the value param
/// come from the offsets JSON (itemGivePrepare), so the UI has no per-game knowledge.
/// </summary>
public class ItemGibRelicDesigner
{
    // Slot choices: not present = keep the game's roll.
    public const int ChoiceNone = -1;

    private readonly Dictionary<string, int> choices = new();
    private bool allowAnyValue;

    private string filter = "";
    private bool filterExact;

    private int lastRowId = -1;
    private string lastParam;

    // Cached options per slot: rebuilt when the row, the filter or "allow any" changes.
    private readonly Dictionary<string, List<(int Value, string Label)>> optionCache = new();
    private string optionCacheKey;

    public bool HasChoices => choices.Count > 0;

    /// <summary>
    /// The slots shown for a row: the prepare's slots whose pool field is set (> 0) on the row.
    /// </summary>
    public static IEnumerable<(ItemGiveSlot Slot, int PoolId)> GetActiveSlots(Param.Row row, ItemGivePrepare prepare)
    {
        foreach (var slot in prepare.Slots)
        {
            var poolId = GetRowInt(row, slot.Field);
            if (poolId is > 0)
                yield return (slot, poolId.Value);
        }
    }

    /// <summary>
    /// The values to write after the game's prepare call: (descriptor offset, value) for each slot the user set.
    /// </summary>
    public (int Offset, int Value)[] GetOverrides(Param.Row row, ItemGivePrepare prepare)
    {
        return GetActiveSlots(row, prepare)
            .Where(s => choices.ContainsKey(s.Slot.Field))
            .Select(s => (s.Slot.Offset, choices[s.Slot.Field]))
            .ToArray();
    }

    public void Reset()
    {
        choices.Clear();
        optionCache.Clear();
        optionCacheKey = null;
    }

    public void Display(ParamEditorView view, Param.Row row, string paramName, ItemGivePrepare prepare)
    {
        if (row.ID != lastRowId || paramName != lastParam)
        {
            lastRowId = row.ID;
            lastParam = paramName;
            Reset();
        }

        var bank = view.GetPrimaryBank();
        var poolParam = bank.GetParamFromName(prepare.SlotPoolParam);
        var valueParam = bank.GetParamFromName(prepare.SlotValueParam);

        // Designer
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_Tools_ItemGib_Designer_Header"),
            LOC.Get("PARAM_Tools_ItemGib_Designer_Header_TT"));

        if (poolParam == null || valueParam == null)
        {
            GUI.WrappedText(LOC.Get("PARAM_Tools_ItemGib_Designer_Missing_Params", prepare.SlotPoolParam, prepare.SlotValueParam));
            return;
        }

        var slots = GetActiveSlots(row, prepare).ToList();
        if (!slots.Any())
        {
            GUI.WrappedText(LOC.Get("PARAM_Tools_ItemGib_Designer_No_Slots"));
            return;
        }

        if (ImGui.Checkbox($"{LOC.Get("PARAM_Tools_ItemGib_Designer_Allow_Any")}##itemGibDesignerAllowAny", ref allowAnyValue))
        {
            optionCacheKey = null;
        }
        GUI.Tooltip(LOC.Get("PARAM_Tools_ItemGib_Designer_Allow_Any_TT"));

        var cacheKey = $"{row.ID}|{allowAnyValue}|{filter}|{filterExact}";
        if (cacheKey != optionCacheKey)
        {
            optionCache.Clear();
            optionCacheKey = cacheKey;
        }

        foreach (var (slot, poolId) in slots)
        {
            // Slot
            GUI.Spacer();
            GUI.SimpleHeader(
                GetSlotTitle(slot),
                LOC.Get("PARAM_Tools_ItemGib_Designer_Slot_TT", poolId));

            if (!optionCache.TryGetValue(slot.Field, out var options))
            {
                options = BuildOptions(prepare, poolParam, valueParam, poolId);
                optionCache[slot.Field] = options;
            }

            DisplaySlotCombo(slot, options, valueParam);
        }
    }

    private void DisplaySlotCombo(ItemGiveSlot slot, List<(int Value, string Label)> options, Param valueParam)
    {
        var preview = !choices.TryGetValue(slot.Field, out var chosen)
            ? LOC.Get("PARAM_Tools_ItemGib_Designer_Random")
            : chosen == ChoiceNone
                ? LOC.Get("PARAM_Tools_ItemGib_Designer_None")
                : GetValueLabel(valueParam, chosen);

        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo($"##itemGibSlot_{slot.Field}", preview, ImGuiComboFlags.HeightLarge))
        {
            EditorFilters.DisplayFramedListFilter($"itemGibSlotFilter_{slot.Field}", ref filter, ref filterExact);

            if (ImGui.Selectable($"{LOC.Get("PARAM_Tools_ItemGib_Designer_Random")}##itemGibSlotRandom_{slot.Field}", !choices.ContainsKey(slot.Field)))
            {
                choices.Remove(slot.Field);
            }
            GUI.Tooltip(LOC.Get("PARAM_Tools_ItemGib_Designer_Random_TT"));

            if (ImGui.Selectable($"{LOC.Get("PARAM_Tools_ItemGib_Designer_None")}##itemGibSlotNone_{slot.Field}", chosen == ChoiceNone && choices.ContainsKey(slot.Field)))
            {
                choices[slot.Field] = ChoiceNone;
            }
            GUI.Tooltip(LOC.Get("PARAM_Tools_ItemGib_Designer_None_TT"));

            ImGui.Separator();

            var clipper = new ImGuiListClipper();
            clipper.Begin(options.Count);
            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var (value, label) = options[i];
                    var selected = choices.TryGetValue(slot.Field, out var c) && c == value;
                    if (ImGui.Selectable($"{label}##itemGibSlotOption_{slot.Field}_{i}", selected))
                    {
                        choices[slot.Field] = value;
                    }
                }
            }
            clipper.End();

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("PARAM_Tools_ItemGib_Designer_Slot_Combo_TT"));
    }

    private List<(int Value, string Label)> BuildOptions(ItemGivePrepare prepare, Param poolParam, Param valueParam, int poolId)
    {
        IEnumerable<(int Value, int? Weight)> values;
        if (allowAnyValue)
        {
            values = valueParam.Rows.Select(r => (r.ID, (int?)null));
        }
        else
        {
            // The pool: every row of the pool param with this ID (a pool is a group of rows sharing an ID).
            values = poolParam.Rows
                .Where(r => r.ID == poolId)
                .Select(r => (GetRowInt(r, prepare.SlotPoolField) ?? -1, prepare.SlotPoolWeightField != null ? GetRowInt(r, prepare.SlotPoolWeightField) : null))
                .Where(v => v.Item1 >= 0);
        }

        return values
            .GroupBy(v => v.Value)
            .Select(g => g.First())
            .Select(v => (v.Value, Label: v.Weight is { } w
                ? LOC.Get("PARAM_Tools_ItemGib_Designer_Option_Weighted", GetValueLabel(valueParam, v.Value), w)
                : GetValueLabel(valueParam, v.Value)))
            .Where(o => string.IsNullOrEmpty(filter) || EditorFilters.IsMatch(filter, o.Label, filterExact))
            .ToList();
    }

    // Row names of the value param by ID (Param's own lookups are linear), rebuilt when the param changes.
    private Dictionary<int, string> valueNames = new();
    private Param valueNamesSource;
    private int valueNamesCount = -1;

    private string GetValueLabel(Param valueParam, int value)
    {
        if (valueParam != valueNamesSource || valueParam.Rows.Count != valueNamesCount)
        {
            valueNamesSource = valueParam;
            valueNamesCount = valueParam.Rows.Count;
            valueNames = new();
            foreach (var r in valueParam.Rows)
                valueNames.TryAdd(r.ID, r.Name);
        }

        var name = valueNames.GetValueOrDefault(value);
        return string.IsNullOrWhiteSpace(name) ? $"{value}" : $"{value} {name}";
    }

    private static string GetSlotTitle(ItemGiveSlot slot)
    {
        return slot.Kind != null
            ? LOC.Get($"PARAM_Tools_ItemGib_Slot_{slot.Kind}", slot.Number ?? "")
            : slot.Field;
    }

    private static int? GetRowInt(Param.Row row, string field)
    {
        var cell = row?[field];
        if (cell == null)
            return null;

        try
        {
            return Convert.ToInt32(cell.Value.Value);
        }
        catch
        {
            return null;
        }
    }
}
