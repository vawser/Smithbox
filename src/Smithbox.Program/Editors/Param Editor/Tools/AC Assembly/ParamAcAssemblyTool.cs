using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// AC6: edits the design file of the selected CharaInitParam row (the file named by its assemDataID in
/// asmparam.designbnd). The design overrides the row's equip fields in game, so this is where an AC's parts, names
/// and paint are actually set.
/// </summary>
public class ParamAcAssemblyTool
{
    private const string CharaInitParam = "CharaInitParam";
    private const string AssemDataField = "assemDataID";

    private static readonly string[] MainColorKeys =
    {
        "PARAM_AcAssembly_Color_Main",
        "PARAM_AcAssembly_Color_Sub",
        "PARAM_AcAssembly_Color_Support",
        "PARAM_AcAssembly_Color_Optional",
        "PARAM_AcAssembly_Color_Joint",
        "PARAM_AcAssembly_Color_Device",
    };

    private readonly ParamEditorView View;
    private readonly ProjectEntry Project;

    private int TemplateId = 1200;

    // The ID typed for a new design, for the row it was suggested for
    private Param.Row NewDesignRow;
    private int NewDesignId;
    private string TemplateFilter = "";
    private string PickerFilter = "";

    private int ColoringRecord = 0;

    // The name fields' text while they are edited
    private string AcNameBuffer = "";
    private string DataNameBuffer = "";
    private (int Id, int Version) BufferKey = (-1, -1);

    /// <summary>
    /// A colour drag in progress: the design's bytes from before it, recorded as one undo step once it ends
    /// </summary>
    private (int Id, byte[] Before)? PendingLiveEdit;

    public ParamAcAssemblyTool(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private AcDesignBank Bank => View.Editor.AcDesigns;

    public void Display()
    {
        if (Project.Descriptor.ProjectType != ProjectType.AC6)
            return;

        if (!ToolSections.Header($"{LOC.Get("PARAM_AcAssembly_Header")}##acAssemblyHeader"))
            return;

        ImGui.BeginChild("AcAssemblyToolSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("PARAM_AcAssembly_Hint"));

        FlushLiveEditIfIdle();

        if (!Bank.EnsureLoaded())
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, Bank.LoadError ?? "");
            ImGui.EndChild();
            return;
        }

        var row = View.Selection.GetActiveParam() == CharaInitParam ? View.Selection.GetActiveRow() : null;

        if (row == null)
        {
            GUI.Spacer();
            GUI.WrappedTextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_AcAssembly_Select_Row"));
            ImGui.EndChild();
            return;
        }

        var assemDataId = ReadInt(row, AssemDataField);

        GUI.Spacer();
        ImGui.Text(LOC.Get("PARAM_AcAssembly_Row", row.ID, row.Name ?? ""));
        ImGui.Text(LOC.Get("PARAM_AcAssembly_AssemDataID", assemDataId?.ToString() ?? "-"));

        if (assemDataId == null)
        {
            ImGui.EndChild();
            return;
        }

        if (assemDataId <= 0 || !Bank.HasDesign(assemDataId.Value))
        {
            DisplayCreate(row, assemDataId.Value);
            ImGui.EndChild();
            return;
        }

        var id = assemDataId.Value;

        var design = Bank.GetDesign(id, out var error);

        if (design == null)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PARAM_AcAssembly_Error_Read_Design", Bank.GetEntryName(id), error));
            ImGui.EndChild();
            return;
        }

        ImGui.TextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_AcAssembly_File", Bank.GetEntryName(id),
            design.Compressed ? "ASMC" : LOC.Get("PARAM_AcAssembly_Uncompressed"), design.AssembleVersion));

        DisplayNames(id, design);
        DisplayParts(row, id, design);
        DisplayColoring(id, design);
        DisplaySave();

        ImGui.EndChild();
    }

    #region Create

    /// <summary>
    /// For a row without a design file: either its assemDataID is set but names no file (the file is created at that
    /// ID), or it isn't set (an ID is chosen, and the row is pointed at it in the same undo step)
    /// </summary>
    private void DisplayCreate(Param.Row row, int assemDataId)
    {
        var linked = assemDataId > 0;

        GUI.Spacer();
        GUI.WrappedTextColored(UI.Current.ImGui_AliasName_Text, linked
            ? LOC.Get("PARAM_AcAssembly_No_Design", assemDataId)
            : LOC.Get("PARAM_AcAssembly_No_AssemDataID"));

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_AcAssembly_Header_Create"), LOC.Get("PARAM_AcAssembly_Header_Create_TT"));

        var id = assemDataId;

        if (!linked)
        {
            if (NewDesignRow != row)
            {
                NewDesignRow = row;
                NewDesignId = SuggestDesignId(row.ID);
            }

            ImGui.SetNextItemWidth(160f * DPI.UIScale());
            ImGui.InputInt($"{LOC.Get("PARAM_AcAssembly_New_ID")}##acAssemblyNewId", ref NewDesignId, 0, 0);
            GUI.Tooltip(LOC.Get("PARAM_AcAssembly_New_ID_TT"));

            id = NewDesignId;

            if (id <= 0)
            {
                GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("PARAM_AcAssembly_New_ID_Invalid"));
                return;
            }

            // The ID already has a design: the row can use it as it is
            if (Bank.HasDesign(id))
            {
                GUI.WrappedTextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_AcAssembly_New_ID_Exists", DescribeDesign(id)));

                if (ImGui.Button($"{LOC.Get("PARAM_AcAssembly_Link")}##acAssemblyLink"))
                {
                    var link = SetAssemDataAction(row, id);

                    if (link != null)
                    {
                        link.DisplayName = LOC.Get("PARAM_AcAssembly_Action_Link", row.ID, id);
                        Execute(link);
                    }
                }

                GUI.Tooltip(LOC.Get("PARAM_AcAssembly_Link_TT"));
                return;
            }
        }

        if (!Bank.HasDesign(TemplateId))
            TemplateId = Bank.DesignIds.FirstOrDefault(-1);

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##acAssemblyTemplate", DescribeDesign(TemplateId), ImGuiComboFlags.HeightLarge))
        {
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##acAssemblyTemplateFilter", LOC.Get("PARAM_AcAssembly_Filter_Hint"), ref TemplateFilter, 128);

            foreach (var candidate in Bank.DesignIds)
            {
                var label = DescribeDesign(candidate);

                if (TemplateFilter != "" && !label.Contains(TemplateFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.Selectable($"{label}##template{candidate}", candidate == TemplateId))
                    TemplateId = candidate;
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(LOC.Get("PARAM_AcAssembly_Template_TT"));

        var template = Bank.GetDesign(TemplateId, out _);

        ImGui.BeginDisabled(template == null);

        if (ImGui.Button($"{LOC.Get("PARAM_AcAssembly_Create")}##acAssemblyCreate"))
        {
            var copy = AcDesign.Read(Bank.GetBytes(TemplateId));

            // Its parts come from the row, so the new design starts as the AC the row describes
            FillFromRow(row, copy);

            var displayName = LOC.Get("PARAM_AcAssembly_Action_Create", id);
            var create = new AcDesignEditAction(Bank, id, null, copy.Write(), displayName);

            if (linked)
            {
                Execute(create);
            }
            else if (SetAssemDataAction(row, id) is { } link)
            {
                // The file and the row's assemDataID as one undo step
                Execute(new CompoundAction(new List<EditorAction> { create, link }) { DisplayName = displayName });
            }
        }

        GUI.Tooltip(LOC.Get(linked ? "PARAM_AcAssembly_Create_TT" : "PARAM_AcAssembly_Create_Link_TT"));

        ImGui.EndDisabled();
    }

    /// <summary>
    /// The row's own ID when it's free (as most vanilla designs are numbered), else the next free ID after it
    /// </summary>
    private int SuggestDesignId(int rowId)
    {
        var id = Math.Max(rowId, 1);

        while (Bank.HasDesign(id) && id < int.MaxValue)
            id++;

        return id;
    }

    private static PropertiesChangedAction SetAssemDataAction(Param.Row row, int id)
    {
        var cell = row[AssemDataField];

        if (cell == null)
            return null;

        var property = typeof(Param.Cell).GetProperty("Value");
        return new PropertiesChangedAction(property, cell.Value, id);
    }

    private string DescribeDesign(int id)
    {
        if (id < 0)
            return "";

        var design = Bank.GetDesign(id, out _);
        var acName = design?.GetString(AcDesign.SectionAcName);
        var dataName = design?.GetString(AcDesign.SectionDataName);

        var names = string.Join(" / ", new[] { acName, dataName }.Where(n => !string.IsNullOrEmpty(n)));

        return names == "" ? $"{id}" : $"{id}: {names}";
    }

    #endregion

    #region Names

    private void DisplayNames(int id, AcDesign design)
    {
        if (BufferKey != (id, Bank.Version))
        {
            AcNameBuffer = design.GetString(AcDesign.SectionAcName) ?? "";
            DataNameBuffer = design.GetString(AcDesign.SectionDataName) ?? "";
            BufferKey = (id, Bank.Version);
        }

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_AcAssembly_Header_Names"), LOC.Get("PARAM_AcAssembly_Header_Names_TT"));

        if (ImGui.BeginTable("##acAssemblyNames", 2, ImGuiTableFlags.SizingFixedFit))
        {
            ImGui.TableSetupColumn("label", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("value", ImGuiTableColumnFlags.WidthStretch);

            NameRow(id, design, AcDesign.SectionAcName, "PARAM_AcAssembly_AcName", ref AcNameBuffer);
            NameRow(id, design, AcDesign.SectionDataName, "PARAM_AcAssembly_DataName", ref DataNameBuffer);

            ImGui.EndTable();
        }
    }

    private void NameRow(int id, AcDesign design, string section, string locKey, ref string buffer)
    {
        // Older designs have no DataName section
        if (design.GetSection(section) == null)
            return;

        ImGui.TableNextRow();
        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get(locKey));
        GUI.Tooltip(LOC.Get($"{locKey}_TT"));

        ImGui.TableNextColumn();
        ImGui.SetNextItemWidth(-1);
        ImGui.InputText($"##acAssembly{section}", ref buffer, 64);

        if (GUI.InputCommitted())
        {
            var value = buffer;
            Edit(id, LOC.Get("PARAM_AcAssembly_Action_Name", id), d => d.SetString(section, value));
        }
    }

    #endregion

    #region Parts

    private void DisplayParts(Param.Row row, int id, AcDesign design)
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_AcAssembly_Header_Parts"), LOC.Get("PARAM_AcAssembly_Header_Parts_TT"));

        var bank = View.GetPrimaryBank();
        var count = design.AssembleSlotCount;

        if (ImGui.BeginTable("##acAssemblyParts", 3, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg))
        {
            ImGui.TableSetupColumn("slot", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("id", ImGuiTableColumnFlags.WidthFixed, 110f * DPI.UIScale());
            ImGui.TableSetupColumn("part", ImGuiTableColumnFlags.WidthStretch);

            foreach (var slot in AcAssemblySlot.Known)
            {
                if (slot.Index >= count)
                    continue;

                PartRow(id, design, slot, bank);
            }

            ImGui.EndTable();
        }

        if (ImGui.Button($"{LOC.Get("PARAM_AcAssembly_Fill_From_Row")}##acAssemblyFill"))
        {
            Edit(id, LOC.Get("PARAM_AcAssembly_Action_Fill", id), d => FillFromRow(row, d));
        }

        GUI.Tooltip(LOC.Get("PARAM_AcAssembly_Fill_From_Row_TT"));

        // The slots whose use isn't known, as stored
        var others = Enumerable.Range(0, count).Where(i => !AcAssemblySlot.KnownIndices.Contains(i)).ToList();

        if (others.Count > 0 && ImGui.TreeNode($"{LOC.Get("PARAM_AcAssembly_Other_Slots")}##acAssemblyOtherSlots"))
        {
            GUI.Tooltip(LOC.Get("PARAM_AcAssembly_Other_Slots_TT"));

            foreach (var index in others)
            {
                var raw = design.GetRawSlot(index);

                ImGui.SetNextItemWidth(160f * DPI.UIScale());
                ImGui.InputInt($"{LOC.Get("PARAM_AcAssembly_Slot_Index", index)}##acAssemblyRaw{index}", ref raw, 0, 0);

                if (GUI.InputCommitted())
                {
                    var value = raw;
                    Edit(id, LOC.Get("PARAM_AcAssembly_Action_Part", id), d => d.SetRawSlot(index, value));
                }
            }

            ImGui.TreePop();
        }
    }

    private void PartRow(int id, AcDesign design, AcAssemblySlot slot, ParamBank bank)
    {
        var partId = design.GetSlotId(slot);
        Param param = null;
        bank?.Params?.TryGetValue(slot.ParamName, out param);

        ImGui.TableNextRow();

        ImGui.TableNextColumn();
        ImGui.AlignTextToFramePadding();
        ImGui.Text(LOC.Get(slot.LocKey));

        ImGui.TableNextColumn();
        var input = partId;
        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt($"##acAssemblyPart{slot.Index}", ref input, 0, 0);

        if (GUI.InputCommitted())
        {
            var value = input;
            Edit(id, LOC.Get("PARAM_AcAssembly_Action_Part", id), d => d.SetSlotId(slot, value));
        }

        GUI.Tooltip(LOC.Get("PARAM_AcAssembly_Part_ID_TT", slot.ParamName));

        ImGui.TableNextColumn();

        var partRow = partId >= 0 ? param?[partId] : null;
        var name = partId < 0 ? LOC.Get("PARAM_AcAssembly_Empty")
            : partRow == null ? LOC.Get("PARAM_AcAssembly_Missing_Row")
            : string.IsNullOrEmpty(partRow.Name) ? $"{partId}" : partRow.Name;

        if (partId >= 0 && partRow == null)
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Invalid_Text_Color);

        ImGui.SetNextItemWidth(-1);
        var open = ImGui.BeginCombo($"##acAssemblyPick{slot.Index}", name, ImGuiComboFlags.HeightLarge);

        if (partId >= 0 && partRow == null)
            ImGui.PopStyleColor();

        if (partRow != null && ImGui.IsItemClicked(ImGuiMouseButton.Middle))
            EditorCommandQueue.AddCommand($@"param/select/new/{slot.ParamName}/{partId}");

        if (!open)
        {
            GUI.Tooltip(LOC.Get("PARAM_AcAssembly_Pick_TT"));
            return;
        }

        if (ImGui.IsWindowAppearing())
            PickerFilter = "";

        ImGui.SetNextItemWidth(-1);

        if (ImGui.IsWindowAppearing())
            ImGui.SetKeyboardFocusHere();

        ImGui.InputTextWithHint($"##acAssemblyPickFilter{slot.Index}", LOC.Get("PARAM_AcAssembly_Filter_Hint"), ref PickerFilter, 128);

        if (ImGui.Selectable($"{LOC.Get("PARAM_AcAssembly_Empty")}##pickEmpty", partId < 0))
            Edit(id, LOC.Get("PARAM_AcAssembly_Action_Part", id), d => d.SetSlotId(slot, -1));

        if (param != null)
        {
            foreach (var candidate in param.Rows)
            {
                var label = string.IsNullOrEmpty(candidate.Name) ? $"{candidate.ID}" : $"{candidate.ID}: {candidate.Name}";

                if (PickerFilter != "" && !label.Contains(PickerFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.Selectable($"{label}##pick{candidate.ID}", candidate.ID == partId))
                {
                    var value = candidate.ID;
                    Edit(id, LOC.Get("PARAM_AcAssembly_Action_Part", id), d => d.SetSlotId(slot, value));
                }
            }
        }

        ImGui.EndCombo();
    }

    /// <summary>
    /// Sets the design's known parts from the row's equip fields (the values the design overrides)
    /// </summary>
    private static void FillFromRow(Param.Row row, AcDesign design)
    {
        foreach (var slot in AcAssemblySlot.Known)
        {
            if (slot.Index >= design.AssembleSlotCount)
                continue;

            var value = ReadInt(row, slot.RowField);

            if (value != null)
                design.SetSlotId(slot, value.Value <= 0 ? -1 : value.Value);
        }
    }

    #endregion

    #region Coloring

    private void DisplayColoring(int id, AcDesign design)
    {
        if (!design.HasColoring)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("PARAM_AcAssembly_Header_Coloring"), LOC.Get("PARAM_AcAssembly_Header_Coloring_TT"));

        ColoringRecord = Math.Clamp(ColoringRecord, 0, AcDesign.ColoringRecordCount - 1);

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##acAssemblyColoringRecord", LOC.Get("PARAM_AcAssembly_Coloring_Record", ColoringRecord)))
        {
            for (var r = 0; r < AcDesign.ColoringRecordCount; r++)
            {
                DrawSwatch(design, r);
                ImGui.SameLine();

                if (ImGui.Selectable($"{LOC.Get("PARAM_AcAssembly_Coloring_Record", r)}##coloringRecord{r}", r == ColoringRecord))
                    ColoringRecord = r;
            }

            ImGui.EndCombo();
        }

        GUI.Tooltip(LOC.Get("PARAM_AcAssembly_Coloring_Record_TT"));

        var record = ColoringRecord;
        var flags = ImGuiColorEditFlags.AlphaBar | ImGuiColorEditFlags.Uint8;

        for (var c = 0; c < AcDesign.ColoringMainColorCount; c++)
        {
            var color = design.GetColor(record, c);
            var colorIndex = c;

            if (ImGui.ColorEdit4($"{LOC.Get(MainColorKeys[c])}##acAssemblyColor{c}", ref color, flags))
                LiveEdit(id, d => d.SetColor(record, colorIndex, color));
        }

        if (ImGui.TreeNode($"{LOC.Get("PARAM_AcAssembly_Extra_Colors")}##acAssemblyExtraColors"))
        {
            GUI.Tooltip(LOC.Get("PARAM_AcAssembly_Extra_Colors_TT"));

            for (var c = 0; c < AcDesign.ColoringExtraColorCount; c++)
            {
                var color = design.GetColor(record, c, extra: true);
                var colorIndex = c;

                if (ImGui.ColorEdit4($"{LOC.Get("PARAM_AcAssembly_Extra_Color", c)}##acAssemblyExtraColor{c}", ref color, flags))
                    LiveEdit(id, d => d.SetColor(record, colorIndex, color, extra: true));
            }

            ImGui.TreePop();
        }

        if (ImGui.Button($"{LOC.Get("PARAM_AcAssembly_Copy_Record_To_All")}##acAssemblyCopyRecord"))
        {
            Edit(id, LOC.Get("PARAM_AcAssembly_Action_Coloring", id), d =>
            {
                for (var r = 0; r < AcDesign.ColoringRecordCount; r++)
                {
                    if (r != record)
                        d.CopyColoringRecord(record, r);
                }
            });
        }

        GUI.Tooltip(LOC.Get("PARAM_AcAssembly_Copy_Record_To_All_TT"));
    }

    private static void DrawSwatch(AcDesign design, int record)
    {
        var size = ImGui.GetTextLineHeight();

        for (var c = 0; c < AcDesign.ColoringMainColorCount; c++)
        {
            if (c > 0)
                ImGui.SameLine(0, 1);

            var color = design.GetColor(record, c);
            color.W = 1f;

            ImGui.ColorButton($"##swatch{record}_{c}", color, ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoBorder, new Vector2(size, size));
        }
    }

    #endregion

    private void DisplaySave()
    {
        GUI.Spacer();

        ImGui.BeginDisabled(!Bank.IsDirty);

        if (ImGui.Button($"{LOC.Get("PARAM_AcAssembly_Save")}##acAssemblySave"))
            Bank.Save();

        ImGui.EndDisabled();

        GUI.Tooltip(LOC.Get("PARAM_AcAssembly_Save_TT"));

        if (Bank.IsDirty)
        {
            ImGui.SameLine();
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_AcAssembly_Unsaved"));
        }
    }

    #region Edits

    /// <summary>
    /// Applies a change to a copy of the design and records it as one undo step, if it changed anything
    /// </summary>
    private void Edit(int id, string displayName, Action<AcDesign> change)
    {
        FlushLiveEdit();

        var before = Bank.GetBytes(id);
        var baseline = AcDesign.Read(before).Write();

        var copy = AcDesign.Read(before);
        change(copy);
        var after = copy.Write();

        // Compared with the unchanged design written the same way, as re-compressing alone changes the bytes
        if (!baseline.AsSpan().SequenceEqual(after))
            Execute(new AcDesignEditAction(Bank, id, before, after, displayName));
    }

    /// <summary>
    /// Applies a change straight away (a colour being dragged), recording the bytes from before the drag so the
    /// whole drag becomes one undo step when it ends
    /// </summary>
    private void LiveEdit(int id, Action<AcDesign> change)
    {
        if (PendingLiveEdit is { } pending && pending.Id != id)
            FlushLiveEdit();

        var before = Bank.GetBytes(id);
        PendingLiveEdit ??= (id, before);

        var copy = AcDesign.Read(before);
        change(copy);
        Bank.SetBytes(id, copy.Write());
    }

    private void FlushLiveEditIfIdle()
    {
        if (PendingLiveEdit != null && !ImGui.IsAnyItemActive() && !ImGui.IsMouseDown(ImGuiMouseButton.Left))
            FlushLiveEdit();
    }

    private void FlushLiveEdit()
    {
        if (PendingLiveEdit is not { } pending)
            return;

        PendingLiveEdit = null;

        var after = Bank.GetBytes(pending.Id);

        // Already applied: executing the action only sets the same bytes again
        Execute(new AcDesignEditAction(Bank, pending.Id, pending.Before, after, LOC.Get("PARAM_AcAssembly_Action_Coloring", pending.Id)));
    }

    private void Execute(EditorAction action)
    {
        View.Editor.ActionManager.ExecuteAction(action);
    }

    #endregion

    private static int? ReadInt(Param.Row row, string field)
    {
        var cell = row[field];

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
