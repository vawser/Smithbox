using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// The slot panel drawn above the fields of a row with 100 one-bit slots (RandomAppearParam, by field presence):
/// a 10×10 grid, the share of sessions it places in, the row that holds the other slots, Make Complement Row, and
/// the map parts that use the row (from <see cref="NrPlacementIndex"/>). One slot is drawn per session, and a part
/// on the row is placed only if its slot is set (Claude/Documentation/MSB Research.md §5.4a).
/// </summary>
public class RandomAppearPanel
{
    private const int SlotCount = NrPlacementFilter.SlotCount;
    private const int GridColumns = 10;

    private readonly ParamEditorView View;

    // The slot cells per paramdef field order: index i holds the def index of "slot{i}", or null if not a slot param
    private readonly Dictionary<PARAMDEF, int[]> SlotLayouts = new();

    // The row masks of the param, rebuilt when the param or the edit version changes
    private Param MaskParam;
    private int MaskVersion = -1;
    private int MaskRowCount = -1;
    private List<(Param.Row Row, UInt128 Mask)> Masks = new();

    private int ComplementId = -1;
    private Param.Row ComplementIdFor;

    public RandomAppearPanel(ParamEditorView view)
    {
        View = view;
    }

    public void Display(string activeParam, Param.Row row)
    {
        if (row == null || activeParam == null)
            return;

        var layout = GetSlotLayout(row);

        if (layout == null)
            return;

        // Tools window style section title, as the Properties windows use (UI.md §2.4)
        if (!ParamFieldSections.Header(ParamFieldSections.RandomAppearKey, LOC.Get("PARAM_RandomAppear_Header"),
                LOC.Get("PARAM_RandomAppear_Header_TT")))
            return;

        var cells = row.Cells.ToArray();
        var mask = GetMask(cells, layout);
        var count = PopCount(mask);

        ImGui.TextUnformatted(LOC.Get("PARAM_RandomAppear_Count", count, SlotCount, count));
        GUI.Tooltip(LOC.Get("PARAM_RandomAppear_Count_TT"));

        DisplayGrid(cells, layout, mask);
        DisplayBulkButtons(cells, layout, mask);

        var param = View.GetPrimaryBank().Params?.GetValueOrDefault(activeParam);

        if (param != null)
        {
            DisplayDuplicates(param, row);
            DisplayComplement(param, activeParam, row, layout, mask);
        }

        DisplayUsedBy(row);

        ImGui.Spacing();
    }

    /// <summary>
    /// The cell indices of slot0–slot99, when the row has exactly those fields as one-bit values. Taken from the
    /// cells, not the def's field list, as the columns leave out fields of other regulation versions.
    /// </summary>
    private int[] GetSlotLayout(Param.Row row)
    {
        var def = row.Def;

        if (def == null)
            return null;

        if (SlotLayouts.TryGetValue(def, out var cached))
            return cached;

        var layout = new int[SlotCount];
        Array.Fill(layout, -1);

        var found = 0;
        var cells = row.Cells;

        for (var i = 0; i < cells.Count; i++)
        {
            var field = cells[i].Def;
            var name = field.InternalName;

            if (field.BitSize != 1 || !name.StartsWith("slot", StringComparison.OrdinalIgnoreCase) ||
                !int.TryParse(name.AsSpan(4), out var index) || index < 0 || index >= SlotCount || layout[index] >= 0)
                continue;

            layout[index] = i;
            found++;
        }

        var result = found == SlotCount ? layout : null;
        SlotLayouts[def] = result;
        return result;
    }

    private static UInt128 GetMask(Param.Cell[] cells, int[] layout)
    {
        UInt128 mask = 0;

        for (var i = 0; i < SlotCount; i++)
        {
            if (cells[layout[i]].Value is IConvertible v && v.ToInt64(null) != 0)
                mask |= (UInt128)1 << i;
        }

        return mask;
    }

    private static readonly UInt128 AllSlots = ((UInt128)1 << SlotCount) - 1;

    private static int PopCount(UInt128 mask) => (int)UInt128.PopCount(mask);

    private static bool IsSet(UInt128 mask, int slot) => (mask & ((UInt128)1 << slot)) != 0;

    private void DisplayGrid(Param.Cell[] cells, int[] layout, UInt128 mask)
    {
        var flags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.NoHostExtendX;

        if (!ImGui.BeginTable("randomAppearGrid", GridColumns + 1, flags))
            return;

        ImGui.TableSetupColumn("");

        for (var c = 0; c < GridColumns; c++)
        {
            ImGui.TableSetupColumn($"+{c}");
        }

        ImGui.TableHeadersRow();

        for (var r = 0; r < SlotCount / GridColumns; r++)
        {
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.TextDisabled($"{r * GridColumns}");

            for (var c = 0; c < GridColumns; c++)
            {
                var slot = r * GridColumns + c;
                ImGui.TableSetColumnIndex(c + 1);

                var set = IsSet(mask, slot);

                if (ImGui.Checkbox($"##randomAppearSlot{slot}", ref set))
                {
                    var target = set ? mask | ((UInt128)1 << slot) : mask & ~((UInt128)1 << slot);
                    ApplyMask(cells, layout, mask, target, LOC.Get("PARAM_RandomAppear_Edit_Slot", slot));
                }
                GUI.Tooltip(LOC.Get("PARAM_RandomAppear_Slot_TT", slot));
            }
        }

        ImGui.EndTable();
    }

    private void DisplayBulkButtons(Param.Cell[] cells, int[] layout, UInt128 mask)
    {
        if (ImGui.Button($"{LOC.Get("PARAM_RandomAppear_All")}##randomAppearAll"))
            ApplyMask(cells, layout, mask, AllSlots, LOC.Get("PARAM_RandomAppear_All"));
        GUI.Tooltip(LOC.Get("PARAM_RandomAppear_All_TT"));

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("PARAM_RandomAppear_None")}##randomAppearNone"))
            ApplyMask(cells, layout, mask, 0, LOC.Get("PARAM_RandomAppear_None"));
        GUI.Tooltip(LOC.Get("PARAM_RandomAppear_None_TT"));

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("PARAM_RandomAppear_Invert")}##randomAppearInvert"))
            ApplyMask(cells, layout, mask, ~mask & AllSlots, LOC.Get("PARAM_RandomAppear_Invert"));
        GUI.Tooltip(LOC.Get("PARAM_RandomAppear_Invert_TT"));
    }

    /// <summary>
    /// Sets the slots that differ between the two masks, as one undo step
    /// </summary>
    private void ApplyMask(Param.Cell[] cells, int[] layout, UInt128 current, UInt128 target, string displayName)
    {
        var info = typeof(Param.Cell).GetProperty("Value");
        var actions = new List<EditorAction>();

        for (var i = 0; i < SlotCount; i++)
        {
            var want = IsSet(target, i);

            if (want == IsSet(current, i))
                continue;

            var cell = cells[layout[i]];
            var newValue = Convert.ChangeType(want ? 1 : 0, cell.Value.GetType());
            actions.Add(new PropertiesChangedAction(info, cell, newValue));
        }

        if (actions.Count == 0)
            return;

        var compound = new CompoundAction(actions) { DisplayName = displayName };
        View.Editor.ActionManager.ExecuteAction(compound);
    }

    private void DisplayDuplicates(Param param, Param.Row row)
    {
        var sameId = param.Rows.Count(r => r.ID == row.ID);

        if (sameId > 1)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PARAM_RandomAppear_Duplicate_ID", sameId, row.ID));
    }

    /// <summary>
    /// The row whose slots are exactly the others: two parts on such a pair never appear together, and one of
    /// them always does
    /// </summary>
    private void DisplayComplement(Param param, string activeParam, Param.Row row, int[] layout, UInt128 mask)
    {
        var masks = GetParamMasks(param, layout);
        var inverse = ~mask & AllSlots;

        var complement = masks.FirstOrDefault(m => m.Row != row && m.Mask == inverse).Row;

        if (complement != null)
        {
            var label = string.IsNullOrEmpty(complement.Name) ? $"{complement.ID}" : $"{complement.ID} {complement.Name}";

            ImGui.TextUnformatted(LOC.Get("PARAM_RandomAppear_Complement"));
            ImGui.SameLine();

            if (ImGui.SmallButton($"{label}##randomAppearComplement"))
                EditorCommandQueue.AddCommand($"param/select/-1/{activeParam}/{complement.ID}");
            GUI.Tooltip(LOC.Get("PARAM_RandomAppear_Complement_TT"));
            return;
        }

        ImGui.TextDisabled(LOC.Get("PARAM_RandomAppear_No_Complement"));

        if (ComplementIdFor != row)
        {
            ComplementIdFor = row;
            ComplementId = NextFreeId(param, row.ID + 1);
        }

        GUI.SetInputWidth();
        ImGui.InputInt($"{LOC.Get("PARAM_RandomAppear_Complement_ID")}##randomAppearComplementId", ref ComplementId);
        GUI.Tooltip(LOC.Get("PARAM_RandomAppear_Complement_ID_TT"));

        var taken = param.Rows.Any(r => r.ID == ComplementId);

        ImGui.BeginDisabled(taken || ComplementId < 0);

        if (ImGui.Button($"{LOC.Get("PARAM_RandomAppear_Make_Complement")}##randomAppearMakeComplement"))
            MakeComplement(param, activeParam, row, layout, inverse);

        ImGui.EndDisabled();

        GUI.Tooltip(taken
            ? LOC.Get("PARAM_RandomAppear_Make_Complement_Taken_TT", ComplementId)
            : LOC.Get("PARAM_RandomAppear_Make_Complement_TT"));
    }

    private static int NextFreeId(Param param, int from)
    {
        var used = param.Rows.Select(r => r.ID).ToHashSet();
        var id = Math.Max(0, from);

        while (used.Contains(id))
            id++;

        return id;
    }

    private void MakeComplement(Param param, string activeParam, Param.Row row, int[] layout, UInt128 inverse)
    {
        var template = new Param.Row(row)
        {
            ID = ComplementId,
            Name = LOC.Get("PARAM_RandomAppear_Complement_Name", row.ID)
        };

        var cells = template.Cells.ToArray();

        for (var i = 0; i < SlotCount; i++)
        {
            var cell = cells[layout[i]];
            cell.Value = Convert.ChangeType(IsSet(inverse, i) ? 1 : 0, cell.Value.GetType());
        }

        var action = new AddParamsAction(View.Editor, param, activeParam, new List<Param.Row> { template }, false, false);
        View.Editor.ActionManager.ExecuteAction(action);

        EditorCommandQueue.AddCommand($"param/select/-1/{activeParam}/{ComplementId}");
        ComplementIdFor = null;
    }

    private List<(Param.Row Row, UInt128 Mask)> GetParamMasks(Param param, int[] layout)
    {
        var version = View.Editor.ActionManager.Version;

        if (MaskParam == param && MaskVersion == version && MaskRowCount == param.Rows.Count)
            return Masks;

        MaskParam = param;
        MaskVersion = version;
        MaskRowCount = param.Rows.Count;
        Masks = param.Rows.Select(r => (r, GetMask(r.Cells.ToArray(), layout))).ToList();

        return Masks;
    }

    private void DisplayUsedBy(Param.Row row)
    {
        ImGui.Spacing();
        ImGui.TextUnformatted(LOC.Get("PARAM_RandomAppear_Used_By"));
        GUI.Tooltip(LOC.Get("PARAM_RandomAppear_Used_By_TT"));

        var index = NrPlacementIndex.For(View.Project);

        if (index.IsScanning)
        {
            ImGui.SameLine();
            ImGui.TextDisabled(LOC.Get("PARAM_RandomAppear_Scanning", index.Progress, index.Total));
            return;
        }

        ImGui.SameLine();

        var scanLabel = index.IsScanned ? LOC.Get("PARAM_RandomAppear_Rescan") : LOC.Get("PARAM_RandomAppear_Scan");

        if (ImGui.SmallButton($"{scanLabel}##randomAppearScan"))
            index.Scan();
        GUI.Tooltip(LOC.Get("PARAM_RandomAppear_Scan_TT"));

        if (!index.IsScanned)
            return;

        ImGui.SameLine();
        ImGui.TextDisabled(LOC.Get("PARAM_RandomAppear_Scanned_At", index.ScannedAt.ToString("HH:mm")));

        var users = index.UsingRandomAppear(row.ID);

        if (users.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("PARAM_RandomAppear_Unused"));
            return;
        }

        for (var i = 0; i < users.Count; i++)
        {
            var user = users[i];
            var label = user.EntityID > 0
                ? $"{user.MapID}: {user.Name} ({user.EntityID})"
                : $"{user.MapID}: {user.Name}";

            if (ImGui.Selectable($"{label}##randomAppearUser{i}"))
            {
                EditorCommandQueue.AddCommand($"map/load/{user.MapID}");
                EditorCommandQueue.AddCommand($"map/select/{user.MapID}/{user.Name}/{nameof(MsbEntityType.Part)}");
            }
            GUI.Tooltip(LOC.Get("PARAM_RandomAppear_User_TT", user.PartType));
        }
    }
}
