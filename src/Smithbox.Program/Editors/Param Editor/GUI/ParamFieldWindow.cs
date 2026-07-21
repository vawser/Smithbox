using Andre.Formats;
using Hexa.NET.ImGui;
using HKLib.hk2018.hkHashMapDetail;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ParamEditor;


public class ParamFieldWindow
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamEditorView ParentView;

    private Dictionary<string, PropertyInfo[]> _propCache = new();


    public ParamFieldWindow(ParamEditorScreen editor, ProjectEntry project, ParamEditorView curView)
    {
        Editor = editor;
        Project = project;
        ParentView = curView;
    }

    public void Display(bool isActiveView, string activeParam, Param.Row activeRow)
    {
        DisplayTitle();

        if (activeRow == null)
        {
            ImGui.BeginChild("columnsNONE");

            FocusManager.SetFocus(EditorFocusContext.ParamEditor_FieldList);

            ImGui.AlignTextToFramePadding();
            ImGui.Text(LOC.Get("PARAM_FieldWindow_SelectRow_Hint"));
            ImGui.EndChild();
        }
        else
        {
            ImGui.BeginChild("columns" + activeParam);

            FocusManager.SetFocus(EditorFocusContext.ParamEditor_FieldList);

            Param vanillaParam = ParentView.GetVanillaBank().Params?.GetValueOrDefault(activeParam);

            var bank = ParentView.GetPrimaryBank();
            var curRow = activeRow;

            // Find the position of activeRow among all rows with the same ID in the primary param.
            // This is needed so that for params with non-unique row IDs (e.g. RandomAppearParam),
            // the nth duplicate in the primary is compared against the nth duplicate in vanilla/aux,
            // not always the first one as the this[int id] indexer would return.
            var primaryParam = bank.Params?.GetValueOrDefault(activeParam);
            int duplicateIndex = GetDuplicateIndex(primaryParam, activeRow);

            var vanillaRow = GetRowAtDuplicateIndex(vanillaParam, activeRow.ID, duplicateIndex);

            var auxRows = Editor.Project.Handler.ParamData.AuxBanks
                .Select((auxBank, i) => (auxBank.Key, GetRowAtDuplicateIndex(
                    auxBank.Value.Params?.GetValueOrDefault(activeParam),
                    activeRow.ID,
                    duplicateIndex)))
                .ToList();

            var compareRow = ParentView.Selection.GetCompareRow();

            DisplayFieldTable(activeRow, vanillaRow, auxRows, compareRow,
                ref ParentView.Selection.GetCurrentPropSearchString(),
                activeParam, isActiveView);

            ImGui.EndChild();
        }
    }
    public void DisplayTitle()
    {
        GUI.SimpleHeader(
            LOC.Get("PARAM_FieldWindow_Title"),
            LOC.Get("PARAM_FieldWindow_Title_TT"));
    }

    public void DisplayFieldTable(Param.Row curRow, Param.Row vanillaRow, List<(string, Param.Row)> auxRows,
        Param.Row compareRow, ref string propSearchString, string activeParam, bool isActiveView)
    {
        var meta = ParentView.GetParamData().GetParamMeta(curRow.Def);
        var annotations = Editor.Project.Handler.ParamData.GetParamAnnotations(curRow.Def.ParamType);

        var imguiId = 0;
        var showParamCompare = auxRows.Count > 0;
        var showRowCompare = compareRow != null;

        DisplayHeader(isActiveView, ref propSearchString);
        DisplayGraph(isActiveView, curRow, meta);

        bool useGroups = false;

        if (meta != null)
        {
            useGroups = CFG.Current.ParamEditor_Field_List_Enable_Field_Layouts
                             && Project.Handler.ParamData.FieldLayouts.Entries.Any(e => e.Name == meta.FieldLayout);
        }

        if (useGroups)
        {
            DisplayLayoutTable(meta, annotations, curRow, vanillaRow, auxRows, compareRow, ref propSearchString, activeParam, isActiveView, ref imguiId);
        }
        else
        {
            DisplayFlatTable(meta, annotations, curRow, vanillaRow, auxRows, compareRow, ref propSearchString, activeParam, isActiveView, ref imguiId);
        }
    }

    // Default field display
    public void DisplayFlatTable(ParamMeta meta, ParamAnnotationEntry annotations, Param.Row curRow, Param.Row vanillaRow, List<(string, Param.Row)> auxRows,
        Param.Row compareRow, ref string propSearchString, string activeParam, bool isActiveView, ref int imguiId)
    {
        var showParamCompare = auxRows.Count > 0;
        var showRowCompare = compareRow != null;

        // Determine column count
        var columnCount = GetColumnCount(showParamCompare, showRowCompare, auxRows);

        // Field Table
        if (EditorTableUtils.ImGuiTableStdColumns("ParamFieldsT", columnCount, false))
        {
            List<string> pinnedFields =
                Editor.Project.Descriptor.PinnedFields.GetValueOrDefault(activeParam, null);

            if (CFG.Current.ParamEditor_Field_List_Pinned_Stay_Visible)
            {
                ImGui.TableSetupScrollFreeze(columnCount, (showParamCompare ? 3 : 2) + (1 + pinnedFields?.Count ?? 0));
            }

            if (showParamCompare)
            {
                ImGui.TableNextColumn();

                // Main
                if (ImGui.TableNextColumn())
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(LOC.Get("PARAM_FieldWindow_Col_Current"));
                }

                // Vanilla
                if (CFG.Current.Param_ShowVanillaColumn && ImGui.TableNextColumn())
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(LOC.Get("PARAM_FieldWindow_Col_Vanilla"));
                }

                // Aux
                if (CFG.Current.Param_ShowAuxColumn)
                {
                    foreach ((var name, Param.Row r) in auxRows)
                    {
                        if (ImGui.TableNextColumn())
                        {
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text(name);
                        }
                    }
                }
            }

            int infoImGuiID = 1000;

            // ID and Name
            DisplayRowFields(curRow, meta, annotations, vanillaRow, auxRows, compareRow, ref infoImGuiID, activeParam, columnCount);

            var search = propSearchString;
            List<(ParamEditorPseudoColumn, Param.Column)> cols = CacheBank.GetCached(Editor, curRow, "fieldFilter",
                () => ParentView.MassEdit.CSE.Search((activeParam, curRow), search, true, true));

            List<(ParamEditorPseudoColumn, Param.Column)> vcols = CacheBank.GetCached(Editor, vanillaRow, "vFieldFilter",
                () => cols.Select((x, i) => x.GetAs(ParentView.GetVanillaBank().GetParamFromName(activeParam))).ToList());

            List<List<(ParamEditorPseudoColumn, Param.Column)>> auxCols = CacheBank.GetCached(Editor, auxRows,
                "auxFieldFilter", () => auxRows.Select((r, i) => cols.Select((c, j) => c.GetAs(Editor.Project.Handler.ParamData.AuxBanks[r.Item1].GetParamFromName(activeParam))).ToList()).ToList());

            if (CFG.Current.ParamEditor_Field_List_Pinned_Stay_Visible)
            {
                if (pinnedFields?.Count > 0)
                {
                    int pinnedImGuiID = 2000;

                    DisplayPinnedFields(pinnedFields, meta, annotations, curRow, vanillaRow, auxRows, compareRow, cols, vcols,
                        auxCols, ref imguiId, activeParam, ref pinnedImGuiID, columnCount);

                    EditorTableUtils.ImguiTableSeparator();
                }
            }

            // Main Fields
            DisplayFields(meta, annotations, curRow, vanillaRow, auxRows, compareRow, cols, vcols, auxCols, ref imguiId,
                activeParam, pinnedFields, columnCount);

            ImGui.EndTable();
        }
    }

    // Field Display affected by Field Layout
    public void DisplayLayoutTable(ParamMeta meta, ParamAnnotationEntry annotations, Param.Row curRow, Param.Row vanillaRow, List<(string, Param.Row)> auxRows,
        Param.Row compareRow, ref string propSearchString, string activeParam, bool isActiveView, ref int imguiId)
    {
        var showParamCompare = auxRows.Count > 0;
        var showRowCompare = compareRow != null;

        // Determine column count
        var columnCount = GetColumnCount(showParamCompare, showRowCompare, auxRows);

        var search = propSearchString;
        var cols = CacheBank.GetCached(Editor, curRow, "fieldFilter",
            () => ParentView.MassEdit.CSE.Search((activeParam, curRow), search, true, true));

        var vcols = CacheBank.GetCached(Editor, vanillaRow, "vFieldFilter",
             () => cols.Select((x, i) => x.GetAs(ParentView.GetVanillaBank().GetParamFromName(activeParam))).ToList());

        var auxCols = CacheBank.GetCached(Editor, auxRows,
            "auxFieldFilter", () => auxRows.Select((r, i) => cols.Select((c, j) => c.GetAs(Editor.Project.Handler.ParamData.AuxBanks[r.Item1].GetParamFromName(activeParam))).ToList()).ToList());

        var pinnedFields = Editor.Project.Descriptor.PinnedFields.GetValueOrDefault(activeParam, null);

        // Field Table
        if (EditorTableUtils.ImGuiTableGroupedColumns("ParamFieldsT", columnCount))
        {
            if (CFG.Current.ParamEditor_Field_List_Pinned_Stay_Visible)
            {
                ImGui.TableSetupScrollFreeze(columnCount, (showParamCompare ? 3 : 2) + (1 + pinnedFields?.Count ?? 0));
            }

            if (showParamCompare)
            {
                ImGui.TableNextColumn();

                // Main
                if (ImGui.TableNextColumn())
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(LOC.Get("PARAM_FieldWindow_Col_Current"));
                }

                // Vanilla
                if (CFG.Current.Param_ShowVanillaColumn && ImGui.TableNextColumn())
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.Text(LOC.Get("PARAM_FieldWindow_Col_Vanilla"));
                }

                // Aux
                if (CFG.Current.Param_ShowAuxColumn)
                {
                    foreach ((var name, Param.Row r) in auxRows)
                    {
                        if (ImGui.TableNextColumn())
                        {
                            ImGui.AlignTextToFramePadding();
                            ImGui.Text(name);
                        }
                    }
                }
            }

            int infoImGuiID = 1000;

            // ID and Name
            DisplayRowFields(curRow, meta, annotations, vanillaRow, auxRows, compareRow, ref infoImGuiID, activeParam, columnCount);

            // Pinned Fields
            if (CFG.Current.ParamEditor_Field_List_Pinned_Stay_Visible)
            {
                if (pinnedFields?.Count > 0)
                {
                    int pinnedImGuiID = 2000;

                    DisplayPinnedFields(pinnedFields, meta, annotations, curRow, vanillaRow, auxRows, compareRow, cols, vcols,
                        auxCols, ref imguiId, activeParam, ref pinnedImGuiID, columnCount);

                    EditorTableUtils.ImguiTableSeparator();
                }
            }

            ImGui.EndTable();
        }

        bool useLayout = CFG.Current.ParamEditor_Field_List_Enable_Field_Layouts
                         && Project.Handler.ParamData.FieldLayouts.Entries.Any(e => e.Name == meta.FieldLayout);

        var groupsDef = useLayout
            ? Project.Handler.ParamData.FieldLayouts.Entries.FirstOrDefault(e => e.Name == meta.FieldLayout)
            : null;

        if (useLayout && groupsDef.TotalChanceLot != null)
        {
            DisplayTotalChance(curRow, groupsDef);
        }

        // Main Fields (by group)
        ImGui.BeginChild("GroupedFields");

        DisplayFields(meta, annotations, curRow, vanillaRow, auxRows, compareRow, cols, vcols, auxCols, ref imguiId,
            activeParam, pinnedFields, columnCount);

        ImGui.EndChild();
    }

    private int GetColumnCount(bool showParamCompare, bool showRowCompare, List<(string, Param.Row)> auxRows)
    {
        // Determine column count
        var columnCount = 2;

        if (CFG.Current.Param_ShowVanillaColumn)
        {
            columnCount++;
        }

        if (showRowCompare)
        {
            columnCount++;
        }

        if (CFG.Current.Param_ShowAuxColumn && showParamCompare)
        {
            columnCount += auxRows.Count;
        }

        return columnCount;
    }

    private void DisplayHeader(bool isActiveView, ref string propSearchString)
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("ParamFieldListHeaderSection", searchHeight, ImGuiChildFlags.Borders);

        if (propSearchString != null)
        {
            if (isActiveView && InputManager.IsPressed(KeybindID.ParamEditor_Focus_Searchbar))
            {
                ImGui.SetKeyboardFocusHere();
            }

            // Autofill
            if (ParentView.MassEdit.AutoFill != null)
            {
                ImGui.AlignTextToFramePadding();
                var resAutoCol = ParentView.MassEdit.AutoFill.ColumnSearchBarAutoFill();
                if (resAutoCol != null)
                {
                    propSearchString = resAutoCol;
                    CacheBank.ClearCaches();
                }
            }

            ImGui.SameLine();

            // Field search
            ImGui.AlignTextToFramePadding();
            ImGui.InputTextWithHint("##fieldSearch", LOC.Get("PARAM_FieldWindow_Search_Hint"), ref propSearchString,
                255);
            GUI.Tooltip(
                LOC.Get("PARAM_FieldWindow_Search_TT", InputManager.GetHint(KeybindID.ParamEditor_Focus_Searchbar)));

            if (ImGui.IsItemEdited())
            {
                CacheBank.ClearCaches();
            }

            // Toggle Community Field Names
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Book}"))
            {
                if (CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Source)
                {
                    CFG.Current.ParamEditor_FieldNameMode = ParamFieldNameMode.Community;
                }
                else if (CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Community)
                {
                    CFG.Current.ParamEditor_FieldNameMode = ParamFieldNameMode.Source_Community;
                }
                else if (CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Source_Community)
                {
                    CFG.Current.ParamEditor_FieldNameMode = ParamFieldNameMode.Community_Source;
                }
                else if (CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Community_Source)
                {
                    CFG.Current.ParamEditor_FieldNameMode = ParamFieldNameMode.Source;
                }
            }

            GUI.Tooltip(
                LOC.Get("PARAM_FieldWindow_FieldNameMode_Hint", LOC.Get(CFG.Current.ParamEditor_FieldNameMode.GetDisplayName())));

            // Toggle Vanilla Columns
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.AddressBook}"))
            {
                CFG.Current.Param_ShowVanillaColumn = !CFG.Current.Param_ShowVanillaColumn;
            }

            var vanillaColumnMode = LOC.Get("PARAM_FieldWindow_VanillaCol_Hidden");
            if (CFG.Current.Param_ShowVanillaColumn)
                vanillaColumnMode = LOC.Get("PARAM_FieldWindow_VanillaCol_Visible");

            GUI.Tooltip(LOC.Get("PARAM_FieldWindow_VanillaCol_Hint", vanillaColumnMode));

            // Toggle Auxiliary Columns
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.AddressBookO}"))
            {
                CFG.Current.Param_ShowAuxColumn = !CFG.Current.Param_ShowAuxColumn;
            }

            var auxColumnMode = LOC.Get("PARAM_FieldWindow_AuxCol_Hidden");
            if (CFG.Current.Param_ShowAuxColumn)
                auxColumnMode = LOC.Get("PARAM_FieldWindow_AuxCol_Visible");

            GUI.Tooltip(LOC.Get("PARAM_FieldWindow_AuxCol_Hint", auxColumnMode));

            // Toggle Field Layouts
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Cubes}"))
            {
                CFG.Current.ParamEditor_Field_List_Enable_Field_Layouts = !CFG.Current.ParamEditor_Field_List_Enable_Field_Layouts;
            }

            var fieldLayoutsState = LOC.Get("PARAM_FieldWindow_FieldLayout_Hidden");
            if (CFG.Current.ParamEditor_Field_List_Enable_Field_Layouts)
                fieldLayoutsState = LOC.Get("PARAM_FieldWindow_FieldLayout_Visible");

            GUI.Tooltip(LOC.Get("PARAM_FieldWindow_FieldLayout_Hint", fieldLayoutsState));

            // Toggle Field Offset Column
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.MapSigns}"))
            {
                CFG.Current.ParamEditor_Field_List_Display_Offsets = !CFG.Current.ParamEditor_Field_List_Display_Offsets;
            }

            var fieldOffsetColumnMode = LOC.Get("PARAM_FieldWindow_FieldOffset_Hidden");
            if (CFG.Current.ParamEditor_Field_List_Display_Offsets)
                fieldOffsetColumnMode = LOC.Get("PARAM_FieldWindow_FieldOffset_Visible");

            GUI.Tooltip(LOC.Get("PARAM_FieldWindow_FieldOffset_Hint", fieldOffsetColumnMode));

            // Toggle Field Padding
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Hubzilla}"))
            {
                CFG.Current.ParamEditor_Field_List_Display_Padding = !CFG.Current.ParamEditor_Field_List_Display_Padding;
            }

            var fieldPaddingMode = LOC.Get("PARAM_FieldWindow_FieldPadding_Visible");
            if (!CFG.Current.ParamEditor_Field_List_Display_Padding)
                fieldPaddingMode = LOC.Get("PARAM_FieldWindow_FieldPadding_Hidden");

            GUI.Tooltip(LOC.Get("PARAM_FieldWindow_FieldPadding_Hint", fieldPaddingMode));

            // Toggle Modified Background
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Bars}"))
            {
                CFG.Current.ParamEditor_Field_List_Display_Modified_Field_Bg = !CFG.Current.ParamEditor_Field_List_Display_Modified_Field_Bg;
            }

            var rowModifiedBgMode = LOC.Get("PARAM_FieldWindow_ModifiedBg_Hidden");
            if (CFG.Current.ParamEditor_Field_List_Display_Modified_Field_Bg)
                rowModifiedBgMode = LOC.Get("PARAM_FieldWindow_ModifiedBg_Visible");

            GUI.Tooltip(LOC.Get("PARAM_FieldWindow_ModifiedBg_Hint", rowModifiedBgMode));
        }

        ImGui.EndChild();
    }

    private void DisplayGraph(bool isActiveView, Param.Row row, ParamMeta meta)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Graph)
            return;

        if (meta == null)
            return;

        var columnWidth = ImGui.GetColumnWidth();

        var graphSectionSize = new Vector2(columnWidth, 400);
        var graphSize = new Vector2(columnWidth * 0.9f, 400 * 0.8f);

        if (meta.CalcCorrectDef != null || meta.SoulCostDef != null)
        {
            ImGui.BeginChild("graphView", graphSectionSize);

            CalcCorrectGraphHelper.Display(ParentView, meta, row, graphSize);

            ImGui.EndChild();
        }
    }

    private void DisplayRowFields(Param.Row row, ParamMeta meta, ParamAnnotationEntry annotations, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, ref int imguiId, string activeParam, int columnCount)
    {
        PropertyInfo nameProp = row.GetType().GetProperty("Name");
        PropertyInfo idProp = row.GetType().GetProperty("ID");

        PropEditorPropInfoRow(row, meta, annotations, vrow, auxRows, crow, nameProp, 
            LOC.Get("PARAM_FieldWindow_Name_Prop"), ref imguiId,
            activeParam, 1_000_000);

        PropEditorPropInfoRow(row, meta, annotations, vrow, auxRows, crow, idProp, 
            LOC.Get("PARAM_FieldWindow_ID_Prop"), ref imguiId,
            activeParam, 2_000_000);

        ImGui.Spacing();
    }

    private void DisplayPinnedFields(List<string> pinList,
        ParamMeta meta, ParamAnnotationEntry annotations,
        Param.Row row, Param.Row vrow, List<(string, Param.Row)> auxRows, Param.Row crow,
        List<(ParamEditorPseudoColumn, Param.Column)> cols, List<(ParamEditorPseudoColumn, Param.Column)> vcols,
        List<List<(ParamEditorPseudoColumn, Param.Column)>> auxCols, ref int imguiId, string activeParam, ref int index, int columnCount)
    {
        var pinnedFields = new List<string>(pinList);
        foreach (var field in pinnedFields)
        {
            var primaryMatches =
                cols.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();

            var vanillaMatches =
                vcols.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();

            var auxMatches = auxCols.Select((aux, i) =>
                aux.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList()).ToList();

            for (var i = 0; i < primaryMatches.Count; i++)
            {
                PropEditorPropCellRow(
                    meta,
                    annotations,
                    row,
                    crow,
                    primaryMatches[i],
                    vrow,
                    vanillaMatches.Count > i ? vanillaMatches[i] : (ParamEditorPseudoColumn.None, null),
                    auxRows,
                    auxMatches.Select((x, j) => x.Count > i ? x[i] : (ParamEditorPseudoColumn.None, null)).ToList(),
                    OffsetTextOfColumn(primaryMatches[i].Item2),
                    ref imguiId, activeParam, true, 4_000_000);
                index++;
            }
        }
    }

    private bool BeginGroupTable(string tableId, int columnCount)
    {
        return EditorTableUtils.ImGuiTableStdColumnsNoScroll(tableId, columnCount);
    }

    private void DisplayFields(ParamMeta meta, ParamAnnotationEntry annotations,
        Param.Row row, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow,
        List<(ParamEditorPseudoColumn, Param.Column)> cols,
        List<(ParamEditorPseudoColumn, Param.Column)> vcols,
        List<List<(ParamEditorPseudoColumn, Param.Column)>> auxCols,
        ref int imguiId, string activeParam, List<string> pinnedFields, int columnCount)
    {
        if (meta == null)
            return;

        List<string> fieldOrder = meta is { AlternateOrder: not null } && CFG.Current.ParamEditor_Field_List_Allow_Rearrangement
            ? [.. meta.AlternateOrder]
            : [];

        foreach (PARAMDEF.Field field in row.Def.Fields)
        {
            if (!fieldOrder.Contains(field.InternalName))
            {
                fieldOrder.Add(field.InternalName);
            }
        }

        if (meta != null
            && CFG.Current.ParamEditor_Field_List_Allow_Rearrangement
            && (meta is { AlternateOrder: null } || meta.AlternateOrder.Count != fieldOrder.Count))
        {
            meta.AlternateOrder = [.. fieldOrder];
        }

        bool useLayout = CFG.Current.ParamEditor_Field_List_Enable_Field_Layouts
                         && Project.Handler.ParamData.FieldLayouts.Entries.Any(e => e.Name == meta.FieldLayout);

        var groupsDef = useLayout
            ? Project.Handler.ParamData.FieldLayouts.Entries.FirstOrDefault(e => e.Name == meta.FieldLayout)
            : null;

        // Flat Mode
        if (!useLayout || groupsDef == null)
        {
            int index = 0;

            foreach (var field in fieldOrder)
            {
                RenderField(meta, annotations, row, vrow, auxRows, crow, cols, vcols, auxCols, field, activeParam, ref index);
            }
        }
        else
        {
            // Pinned Fields
            if (CFG.Current.ParamEditor_Field_List_Pinned_Stay_Visible && pinnedFields?.Count > 0)
            {
                int pi = 0;
                DisplayPinnedFields(pinnedFields, meta, annotations, row, vrow, auxRows, crow,
                    cols, vcols, auxCols, ref imguiId, activeParam, ref pi, columnCount);
            }

            // Grouped Mode
            var groupedFieldNames = new HashSet<string>(
                groupsDef.Groups.SelectMany(g => g.Fields.Select(f => f)));

            if (CFG.Current.ParamEditor_Field_List_Unsorted_Field_Placement is FieldLayoutUnsortedPlacement.Top)
            {
                DisplayUnsortedFields(fieldOrder, groupedFieldNames, meta, annotations, row, vrow, auxRows, crow, cols, vcols, auxCols, activeParam, columnCount);
            }

            foreach (var layout in groupsDef.Groups)
            {
                var layoutFields = fieldOrder
                    .Where(f => layout.Fields.Any(gf => gf == f))
                    .ToList();

                var hasChanceLot = layout.ChanceLot != null;

                if (layoutFields.Count == 0)
                    continue;

                if (CFG.Current.ParamEditor_Field_List_Field_Layout_Display_Type is FieldLayoutMode.Collapsible)
                {
                    var name = layout.GetName();
                    if (!CFG.Current.ParamEditor_Field_List_Enable_Field_Layout_Category_Names)
                        name = "";

                    bool open = ImGui.CollapsingHeader(
                        $"{name}##grp_{activeParam}_{layout.Key}",
                        ImGuiTreeNodeFlags.DefaultOpen);

                    if (open && hasChanceLot)
                    {
                        DisplayChance(row, layout);
                    }

                    if (open && BeginGroupTable($"ParamFieldsG_{activeParam}_{layout.Key}", columnCount))
                    {
                        int idx = 0;
                        foreach (var field in layoutFields)
                        {
                            RenderField(meta, annotations, row, vrow, auxRows, crow, cols, vcols, auxCols, field, activeParam, ref idx);
                        }

                        ImGui.EndTable();
                    }
                }
                else if (CFG.Current.ParamEditor_Field_List_Field_Layout_Display_Type is FieldLayoutMode.Header)
                {
                    if (CFG.Current.ParamEditor_Field_List_Enable_Field_Layout_Category_Names)
                    {
                        GUI.SimpleHeader($"{layout.GetName()}", "");
                    }

                    if (hasChanceLot)
                    {
                        DisplayChance(row, layout);
                    }

                    if (BeginGroupTable($"ParamFieldsG_{activeParam}_{layout.Key}", columnCount))
                    {
                        int idx = 0;
                        foreach (var field in layoutFields)
                        {
                            RenderField(meta, annotations, row, vrow, auxRows, crow, cols, vcols, auxCols, field, activeParam, ref idx);
                        }

                        ImGui.EndTable();
                    }

                    if (!CFG.Current.ParamEditor_Field_List_Enable_Field_Layout_Category_Names)
                    {
                        ImGui.Separator();
                    }
                }
                else if (CFG.Current.ParamEditor_Field_List_Field_Layout_Display_Type is FieldLayoutMode.Separator)
                {
                    if (hasChanceLot)
                    {
                        DisplayChance(row, layout);
                    }

                    if (BeginGroupTable($"ParamFieldsG_{activeParam}_{layout.Key}", columnCount))
                    {
                        int idx = 0;
                        foreach (var field in layoutFields)
                        {
                            RenderField(meta, annotations, row, vrow, auxRows, crow, cols, vcols, auxCols, field, activeParam, ref idx);
                        }

                        ImGui.EndTable();
                    }

                    ImGui.Separator();
                }
            }

            if (CFG.Current.ParamEditor_Field_List_Unsorted_Field_Placement is FieldLayoutUnsortedPlacement.Bottom)
            {
                DisplayUnsortedFields(fieldOrder, groupedFieldNames, meta, annotations, row, vrow, auxRows, crow, cols, vcols, auxCols, activeParam, columnCount);
            }
        }
    }

    private void DisplayChance(Param.Row row, FieldLayoutEntry layout)
    {
        if (!CFG.Current.ParamEditor_Field_List_Enable_Field_Layout_Chance_Hints)
            return;

        var chanceLot = layout.ChanceLot;

        float curChance = 0;
        float totalChance = 0;

        foreach (var field in row.Columns)
        {
            var fieldName = field.Def.InternalName;

            if (fieldName == chanceLot.TargetField)
            {
                var val = field.GetValue(row);

                float intVal = 0;
                var success = float.TryParse($"{val}", out intVal);
                if (success)
                {
                    curChance = intVal;
                }
            }

            if (chanceLot.ChanceSet.Contains(fieldName))
            {
                var val = field.GetValue(row);

                float intVal = 0;
                var success = float.TryParse($"{val}", out intVal);
                if (success)
                {
                    totalChance = totalChance + intVal;
                }
            }
        }

        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            if (row.Def.ParamType == "ITEM_LOT_PARAM2")
            {
                var dropType = row.Columns.FirstOrDefault(e => e.Def.InternalName == "lotDropType");
                var value = $"{dropType.GetValue(row)}";

                // 3 is Confirmed, meaning the lot always occurs.
                if (value == "3")
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_FieldWindow_LotChance_Always"));
                    return;
                }
            }
        }

        if (curChance == 0)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_FieldWindow_LotChance_Never"));
            return;
        }

        var chance = Math.Round((curChance / totalChance) * 100, 2);

        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_FieldWindow_LotChance_Percent", chance));
    }

    private void DisplayTotalChance(Param.Row row, FieldLayout layout)
    {
        if (!CFG.Current.ParamEditor_Field_List_Enable_Field_Layout_Chance_Hints)
            return;

        var chanceLot = layout.TotalChanceLot;

        float totalChance = 0;

        foreach (var field in row.Columns)
        {
            var fieldName = field.Def.InternalName;

            if (chanceLot.ChanceSet.Contains(fieldName))
            {
                var val = field.GetValue(row);

                float intVal = 0;
                var success = float.TryParse($"{val}", out intVal);
                if (success)
                {
                    totalChance = totalChance + intVal;
                }
            }
        }

        if (Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            if (row.Def.ParamType == "ITEM_LOT_PARAM2")
            {
                var dropType = row.Columns.FirstOrDefault(e => e.Def.InternalName == "lotDropType");
                var value = $"{dropType.GetValue(row)}";

                // 3 is Confirmed, meaning the drop always occurs.
                if (value == "3")
                {
                    ImGui.AlignTextToFramePadding();
                    ImGui.TextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_FieldWindow_LotChance_Always"));
                    return;
                }
            }
        }

        if (totalChance == 0)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_FieldWindow_LotChance_Never"));
            return;
        }

        if (totalChance > 100)
            totalChance = 100;

        ImGui.AlignTextToFramePadding();
        ImGui.TextColored(UI.Current.ImGui_AliasName_Text, LOC.Get("PARAM_FieldWindow_LotChance_Percent", totalChance));
    }

    private void RenderField(ParamMeta meta, ParamAnnotationEntry annotations,
        Param.Row row, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow,
        List<(ParamEditorPseudoColumn, Param.Column)> cols,
        List<(ParamEditorPseudoColumn, Param.Column)> vcols,
        List<List<(ParamEditorPseudoColumn, Param.Column)>> auxCols,
        string field, string activeParam, ref int idx)
    {
        if (field.Equals("-"))
        {
            EditorTableUtils.ImguiTableSeparator();
            idx++;
            return;
        }

        var matches = cols?.Where((x, _) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();
        var vmatches = vcols?.Where((x, _) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();
        var auxMatches = auxCols?.Select((aux, _) =>
            aux.Where((x, _) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList()).ToList();

        if (matches == null)
            return;

        for (var i = 0; i < matches.Count; i++)
        {
            PropEditorPropCellRow(meta, annotations, row, crow,
                matches[i],
                vrow,
                vmatches.Count > i ? vmatches[i] : (ParamEditorPseudoColumn.None, null),
                auxRows,
                auxMatches.Select((x, _) => x.Count > i ? x[i] : (ParamEditorPseudoColumn.None, null)).ToList(),
                OffsetTextOfColumn(matches[i].Item2),
                ref idx, activeParam, false, 3_000_000);

            idx++;
        }
    }

    private void DisplayUnsortedFields(
        List<string> fieldOrder,
        HashSet<string> groupedFieldNames,
        ParamMeta meta, ParamAnnotationEntry annotations,
        Param.Row row, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow,
        List<(ParamEditorPseudoColumn, Param.Column)> cols,
        List<(ParamEditorPseudoColumn, Param.Column)> vcols,
        List<List<(ParamEditorPseudoColumn, Param.Column)>> auxCols,
        string activeParam, int columnCount)
    {
        var miscFieldOrder = new List<string>();
        bool prevWasMisc = false;

        foreach (var f in fieldOrder)
        {
            if (f.Equals("-"))
            {
                if (prevWasMisc)
                {
                    miscFieldOrder.Add(f);
                }

                prevWasMisc = false;
            }
            else if (!groupedFieldNames.Contains(f))
            {
                miscFieldOrder.Add(f);
                prevWasMisc = true;
            }
            else
            {
                prevWasMisc = false;
            }
        }

        // Trim any trailing separators
        while (miscFieldOrder.Count > 0 && miscFieldOrder[^1].Equals("-"))
        {
            miscFieldOrder.RemoveAt(miscFieldOrder.Count - 1);
        }

        if (miscFieldOrder.Count > 0)
        {
            if (CFG.Current.ParamEditor_Field_List_Field_Layout_Display_Type is FieldLayoutMode.Collapsible)
            {
                bool open = ImGui.CollapsingHeader(
                    $"Unsorted##grp_{activeParam}_misc",
                    ImGuiTreeNodeFlags.DefaultOpen);

                if (open && BeginGroupTable($"ParamFieldsG_{activeParam}_misc", columnCount))
                {
                    int idx = 0;
                    foreach (var field in miscFieldOrder)
                    {
                        RenderField(meta, annotations, row, vrow, auxRows, crow, cols, vcols, auxCols, field, activeParam, ref idx);
                    }

                    ImGui.EndTable();
                }
            }
            else if (CFG.Current.ParamEditor_Field_List_Field_Layout_Display_Type is FieldLayoutMode.Header)
            {
                GUI.SimpleHeader($"{LOC.Get("PARAM_FieldWindow_Header_Unsorted")}", "");

                if (BeginGroupTable($"ParamFieldsG_{activeParam}_misc", columnCount))
                {
                    int idx = 0;
                    foreach (var field in miscFieldOrder)
                    {
                        RenderField(meta, annotations, row, vrow, auxRows, crow, cols, vcols, auxCols, field, activeParam, ref idx);
                    }

                    ImGui.EndTable();
                }
            }
            else if (CFG.Current.ParamEditor_Field_List_Field_Layout_Display_Type is FieldLayoutMode.Separator)
            {
                if (BeginGroupTable($"ParamFieldsG_{activeParam}_misc", columnCount))
                {
                    int idx = 0;
                    foreach (var field in miscFieldOrder)
                    {
                        RenderField(meta, annotations, row, vrow, auxRows, crow, cols, vcols, auxCols, field, activeParam, ref idx);
                    }

                    ImGui.EndTable();
                }

                ImGui.Separator();
            }
        }
    }

    // Many parameter options, which may be simplified.
    private void PropEditorPropInfoRow(Param.Row row,
        ParamMeta meta, ParamAnnotationEntry annotations, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, PropertyInfo prop, string visualName, ref int imguiId, string activeParam, int displayIndex)
    {
        FieldRow(
            prop.GetValue(row),
            crow != null ? prop.GetValue(crow) : null,
            vrow != null ? prop.GetValue(vrow) : null,
            auxRows.Select((r, i) => r.Item2 != null ? prop.GetValue(r.Item2) : null).ToList(),
            ref imguiId,
            "header",
            visualName,
            null,
            null,
            prop.PropertyType,
            prop,
            null,
            row,
            meta,
            activeParam,
            false,
            null,
            displayIndex);
    }

    private void PropEditorPropCellRow(ParamMeta meta, ParamAnnotationEntry annotations,
        Param.Row row, Param.Row crow,
        (ParamEditorPseudoColumn, Param.Column) col, Param.Row vrow, (ParamEditorPseudoColumn, Param.Column) vcol,
        List<(string, Param.Row)> auxRows, List<(ParamEditorPseudoColumn, Param.Column)> auxCols, string fieldOffset,
        ref int imguiId, string activeParam, bool isPinned, int displayIndex)
    {
        FieldRow(
            row.Get(col),
            crow?.Get(col),
            vcol.IsColumnValid() ? vrow?.Get(vcol) : null,
            auxRows.Select((r, i) => auxCols[i].IsColumnValid() ? r.Item2?.Get(auxCols[i]) : null).ToList(),
            ref imguiId,
            fieldOffset != null ? "0x" + fieldOffset : null, col.Item2.Def.InternalName,
            Editor.Project.Handler.ParamData.GetParamFieldMeta(meta, col.Item2.Def),
            Editor.Project.Handler.ParamData.GetFieldAnnotation(annotations, col.Item2.Def.InternalName),
            col.GetColumnType(),
            typeof(Param.Cell).GetProperty("Value"),
            row[col.Item2],
            row,
            meta,
            activeParam,
            isPinned,
            col.Item2,
            displayIndex);
    }


    private void FieldRow(object oldval, object compareval, object vanillaval,
        List<object> auxVals, ref int imguiId, string fieldOffset, string internalName, ParamFieldMeta cellMeta, ParamAnnotationFieldEntry fieldAnnotation,
        Type propType, PropertyInfo proprow, Param.Cell? nullableCell, Param.Row row, ParamMeta meta, string activeParam,
        bool isPinned, Param.Column col, int displayIndex)
    {
        var metaContext = new FieldMetaContext(ParentView, meta, cellMeta, fieldAnnotation, activeParam, internalName);

        object newval = null;

        if (!CFG.Current.ParamEditor_Field_List_Display_Padding && metaContext.IsPadding)
        {
            return;
        }

        //------------------------------
        // Name Column
        //------------------------------
        ImGui.PushID(imguiId + displayIndex);

        if (ImGui.TableNextColumn())
        {
            if (metaContext.InjectSeparator)
            {
                ImGui.Separator();
            }

            if(col != null)
            {
                FieldTooltipHelper.IconTooltip(ParentView, metaContext, col.Def);
            }

            // Field selection
            ImGui.Selectable("", false, ImGuiSelectableFlags.AllowOverlap);

            if (col != null)
            {
                FieldTooltipHelper.HoverTooltip(ParentView, metaContext, col.Def);
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("ParamRowNameMenu");
            }

            ImGui.SameLine();

            // Name column
            PropertyRowName(fieldOffset, ref internalName, cellMeta, fieldAnnotation);

            // Cache
            ParentView.FieldDecorators.HandleCache(metaContext, row, oldval);

            // Labels
            ParentView.FieldDecorators.HandleLabels(metaContext, row, oldval);
        }

        //------------------------------
        // Value Column
        //------------------------------
        var diffVanilla = ParamUtils.IsValueDiff(ref oldval, ref vanillaval, propType);
        var diffCompare = ParamUtils.IsValueDiff(ref oldval, ref compareval, propType);

        var diffAuxVanilla =
            auxVals.Select((o, i) => ParamUtils.IsValueDiff(ref o, ref vanillaval, propType)).ToList();

        var diffAuxPrimaryAndVanilla = auxVals.Select((o, i) =>
            ParamUtils.IsValueDiff(ref o, ref oldval, propType) &&
            ParamUtils.IsValueDiff(ref o, ref vanillaval, propType)).ToList();

        var count = diffAuxPrimaryAndVanilla.Where(x => x).Count();

        var conflict = (diffVanilla ? 1 : 0) + diffAuxPrimaryAndVanilla.Where(x => x).Count() > 1;

        var matchDefault = nullableCell?.Def.Default != null && nullableCell.Value.Def.Default.Equals(oldval);

        // Ignore for the Name field as this is almost always true if the user has named rows
        if (internalName == "Name")
            diffVanilla = false;

        if (ImGui.TableNextColumn())
        {
            bool pushedStyle = false;

            if (conflict)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AuxConflict_Text);
                pushedStyle = true;
            }
            else if (diffVanilla)
            {
                if (CFG.Current.ParamEditor_Field_List_Display_Modified_Field_Bg)
                {
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ParamDiffBackgroundColor);
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_PrimaryChanged_Text);
                }

                pushedStyle = true;
            }
            else if (metaContext.HasAnyReferenceElements())
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_IsRef_Text);
                pushedStyle = true;
            }
            else if (matchDefault)
            {

            }

            if (metaContext.InjectSeparator)
            {
                ImGui.Separator();
            }

            // Property Editor UI
            ParentView.FieldInputHandler.DisplayFieldInput(metaContext, propType, oldval, ref newval);

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("ParamRowValueMenu");
            }

            // Hints
            ParentView.FieldDecorators.HandleHints(metaContext, row, activeParam, internalName, oldval);

            // Clicks
            ParentView.FieldDecorators.HandleClick(metaContext, row, oldval);

            // Param Reference Buttons
            if (CFG.Current.ParamEditor_Field_List_Display_Map_Link)
            {
                // These are placed at the top, below the ID row
                if (imguiId == 1)
                {
                    ParamMapReferences.ReturnPointParam(Editor, activeParam, row, internalName);
                    ParamMapReferences.BonfireWarpParam(Editor, activeParam, row, internalName);
                    ParamMapReferences.GameAreaParam(Editor, activeParam, row, internalName);
                    ParamMapReferences.ItemLotParam(Editor, activeParam, row, internalName);
                }
            }

            if (CFG.Current.ParamEditor_Field_List_Display_Model_Link)
            {
                // These are placed at the top, below the ID row
                if (imguiId == 1)
                {
                    ParamMapReferences.AssetGeometryParam(Editor, activeParam, row, internalName);
                    ParamMapReferences.BuddyStoneParam(Editor, activeParam, row, internalName);
                }

                // These are placed in-line with the current field
                ParamMapReferences.GrassTypeParam(Editor, activeParam, row, internalName);
                ParamMapReferences.BulletParam(Editor, activeParam, row, internalName);
            }

            // Color Picker
            FieldColorPicker.ColorPicker(ParentView, activeParam, row, internalName);

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("ParamRowValueMenu");
            }

            if (pushedStyle)
            {
                ImGui.PopStyleColor();
            }
        }

        //------------------------------
        // Vanilla/Aux Columns
        //------------------------------
        if (conflict)
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_Input_Conflict_Background);
        }

        if (CFG.Current.Param_ShowVanillaColumn && ImGui.TableNextColumn())
        {
            Comparison_ValueColumn(metaContext, activeParam, vanillaval, propType, row, "vanilla", cellMeta, internalName);
        }

        if (CFG.Current.Param_ShowAuxColumn)
        {
            for (var i = 0; i < auxVals.Count; i++)
            {
                if (ImGui.TableNextColumn())
                {
                    if (!conflict && diffAuxVanilla[i])
                    {
                        if (CFG.Current.ParamEditor_Field_List_Display_Modified_Field_Bg)
                        {
                            ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ParamDiffBackgroundColor);
                        }
                        else
                        {
                            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_PrimaryChanged_Text);
                        }
                    }

                    Comparison_ValueColumn(metaContext, activeParam, auxVals[i], propType, row, i.ToString(), cellMeta, internalName);

                    if (!conflict && diffAuxVanilla[i])
                    {
                        ImGui.PopStyleColor();
                    }
                }
            }
        }

        if (conflict)
        {
            ImGui.PopStyleColor();
        }

        if (compareval != null && ImGui.TableNextColumn())
        {
            if (diffCompare)
            {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ParamCompareDiffBackgroundColor);
            }

            Comparison_ValueColumn(metaContext, activeParam, compareval, propType, row, "compRow", cellMeta, internalName);

            if (diffCompare)
            {
                ImGui.PopStyleColor();
            }
        }

        //if (ImGui.BeginPopup("ParamRowCommonMenu"))
        //{
        //    AdditionalElementsForContextMenu(metaContext, row, internalName, oldval, ref newval);

        //    DefaultContextMenu(metaContext, internalName, activeParam,
        //        activeParam != null, isPinned, col, propType, oldval, true);

        //    ImGui.EndPopup();
        //}

        if (ImGui.BeginPopup("ParamRowNameMenu"))
        {
            DefaultContextMenu(metaContext, internalName, activeParam,
                activeParam != null, isPinned, col, propType, oldval, true);

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("ParamRowValueMenu"))
        {
            var anyDisplayed = AdditionalElementsForContextMenu(metaContext, row, internalName, oldval, ref newval);

            // Only show default if no additional elements are displayed
            if (!anyDisplayed)
            {
                DefaultContextMenu(metaContext, internalName, activeParam,
                    activeParam != null, isPinned, col, propType, oldval, false);
            }

            ImGui.EndPopup();
        }

        // Context Menu Shortcuts
        if (ParamReferenceHelper.Shortcut(ParentView, metaContext, row, oldval, ref newval))
        {
            ParentView.FieldInputHandler.SetLastPropertyManual(newval);
        }

        var committed = ParentView.FieldInputHandler.UpdateProperty(nullableCell != null ? nullableCell : row, proprow, oldval);

        if (committed)
        {
            Editor.Project.Handler.ParamData.PrimaryBank.RefreshParamRowDiffs(Editor, row, activeParam);
        }

        ImGui.PopID();
        imguiId++;
    }

    private void Comparison_ValueColumn(FieldMetaContext metaContext, string activeParam, object colVal, Type propType, Param.Row row, string imguiSuffix, ParamFieldMeta cellMeta, string internalName)
    {
        if (colVal == null)
        {
            ImGui.AlignTextToFramePadding();
            ImGui.TextUnformatted("");
        }
        else
        {
            string value;

            if (propType == typeof(byte[]))
            {
                value = ParamUtils.Dummy8Write((byte[])colVal);
            }
            else
            {
                value = colVal.ToString();
            }

            ImGui.AlignTextToFramePadding();
            ImGui.InputText("##colval" + imguiSuffix, ref value, 256, ImGuiInputTextFlags.ReadOnly);

            // Hints
            ParentView.FieldDecorators.HandleHints(metaContext, row, activeParam, internalName, colVal);
        }
    }

    private string OffsetTextOfColumn(Param.Column col)
    {
        if (col == null)
        {
            return null;
        }

        if (col.Def.BitSize == -1)
        {
            return col.GetByteOffset().ToString("x");
        }

        var offS = col.GetBitOffset();

        if (col.Def.BitSize == 1)
        {
            return $"{col.GetByteOffset().ToString("x")} [{offS}]";
        }

        return $"{col.GetByteOffset().ToString("x")} [{offS}-{offS + col.Def.BitSize - 1}]";
    }

    private void PropertyRowName(string fieldOffset, ref string internalName, ParamFieldMeta cellMeta, ParamAnnotationFieldEntry fieldAnnotation)
    {
        var altName = fieldAnnotation?.Name;

        var printedName = internalName;

        if (!string.IsNullOrWhiteSpace(altName))
        {
            switch (CFG.Current.ParamEditor_FieldNameMode)
            {
                case ParamFieldNameMode.Source:
                    printedName = internalName;
                    break;

                case ParamFieldNameMode.Community:
                    printedName = altName;
                    break;

                case ParamFieldNameMode.Source_Community:
                    printedName = $"{internalName} ({altName})";
                    break;

                case ParamFieldNameMode.Community_Source:
                    printedName = $"{altName} ({internalName})";
                    break;
            }
        }

        if (fieldOffset != null && CFG.Current.ParamEditor_Field_List_Display_Offsets)
        {
            printedName = $"{fieldOffset} {printedName}";
        }

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(printedName);
    }

    private void DefaultContextMenu(FieldMetaContext metaContext, string internalName, string activeParam, bool showPinOptions, bool isPinned, Param.Column col, Type propType, dynamic oldval, bool isNameMenu)
    {
        var altName = metaContext.FieldAnnotation?.Name;
    
        // Information
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_FieldWindow_Context_Info_Header")}##infoMenuHeader"))
        {
            var displayAttributes = CFG.Current.ParamEditor_Field_Context_Display_Field_Attributes;
            var displayDescription = CFG.Current.ParamEditor_Field_Context_Display_Field_Description;

            // Field Information
            if (col != null)
            {
                if (displayAttributes)
                {
                    ParamFieldUtils.ImGui_DisplayPropertyInfo(propType, internalName, isNameMenu, !isNameMenu, altName,
                        col.Def.ArrayLength,
                        col.Def.BitSize);
                }

                if (isNameMenu && displayDescription)
                {
                    if (metaContext.Description != null)
                    {
                        ImGui.AlignTextToFramePadding();
                        ImGui.TextColored(new Vector4(.4f, .7f, 1f, 1f), $"{metaContext.Description}");
                    }
                    else
                    {
                        ImGui.AlignTextToFramePadding();
                        ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 0.7f),
                            LOC.Get("PARAM_FieldWindow_Context_No_Desc"));
                    }
                }
            }
            else
            {
                ImGui.AlignTextToFramePadding();
                ImGui.TextColored(new Vector4(1.0f, 0.7f, 0.4f, 1.0f), Utils.ImGuiEscape(internalName, "", true));
            }

            // Copy Field Name
            if (ImGui.MenuItem($"{LOC.Get("PARAM_FieldWindow_Context_Action_Copy_Field_Name")}##copyFieldNameAction"))
            {
                PlatformUtils.Instance.SetClipboardText(internalName);
            }

            ImGui.EndMenu();
        }

        // Search
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_FieldWindow_Context_Search_Header")}##searchMenuHeader"))
        {
            // Add to Search
            if (ImGui.MenuItem($"{LOC.Get("PARAM_FieldWindow_Context_Action_Add_to_Searchbar")}##addToSearchbarAction"))
            {
                if (col != null)
                {
                    EditorCommandQueue.AddCommand($@"param/search/prop {internalName.Replace(" ", "\\s")} ");
                }
                else
                {
                    EditorCommandQueue.AddCommand($@"param/search/{internalName.Replace(" ", "\\s")} ");
                }
            }

            // Search for Non-Default Values
            if (col != null)
            {
                // Search for Non-Default Values
                if (ImGui.MenuItem($"{LOC.Get("PARAM_FieldWindow_Context_Action_Search_NonDefault_Vals")}##searchNonDefaultValuesAction"))
                {
                    EditorCommandQueue.AddCommand($@"param/search/proprange {internalName.Replace(" ", "\\s")} 0.01 {int.MaxValue}");
                }
            }

            ImGui.EndMenu();
        }

        // Value
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_FieldWindow_Context_Value_Header")}##valueMenuHeader"))
        {
            // Reset to vanilla
            if (ImGui.Selectable($"{LOC.Get("PARAM_FieldWindow_Context_Action_Reset_to_Vanilla")}##resetToVanillaAction"))
            {
                ParentView.MassEdit.ApplyMassEdit($"selection && !added: {Regex.Escape(internalName)}: = vanilla;");
            }

            // Value Distribution
            if (ImGui.Selectable($"{LOC.Get("PARAM_FieldWindow_Context_Action_View_Value_Distribution")}##viewValueDistAction"))
            {
                EditorCommandQueue.AddCommand($@"param/menu/distributionPopup/{internalName}");
            }

            ImGui.EndMenu();
        }

        // Mass Edit
        if (ImGui.BeginMenu("Mass Edit"))
        {
            // Add to Palette
            if (ImGui.MenuItem($"{LOC.Get("PARAM_FieldWindow_Context_Action_Add_to_Palette")}##addToPaletteAction"))
            {
                ParentView.MassEdit.ConstructCommandFromField(internalName);
            }
            GUI.Tooltip(LOC.Get("PARAM_FieldWindow_Context_Action_Add_to_Palette_TT"));

            // Command Palette
            if (ImGui.Selectable($"{LOC.Get("PARAM_FieldWindow_Context_Action_View_Command_Palette")}##commandPaletteAction"))
            {
                EditorCommandQueue.AddCommand(
                    $@"param/menu/massEditRegex/selection: {Regex.Escape(internalName)}: ");
            }
            GUI.Tooltip(LOC.Get("PARAM_FieldWindow_Context_Action_View_Command_Palette_TT"));

            // Autofill
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_FieldWindow_Context_AutoFill_Header")}##autoFillMenuHeader"))
            {
                if (ParentView.MassEdit.AutoFill != null)
                {
                    var res = ParentView.MassEdit.AutoFill.MassEditOpAutoFill();
                    if (res != null)
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/menu/massEditRegex/selection: {Regex.Escape(internalName)}: " + res);
                    }
                }

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("PARAM_FieldWindow_Context_AutoFill_TT"));

            ImGui.EndMenu();
        }

        // Pin Options
        if (showPinOptions)
        {
            // Pinning
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_FieldWindow_Context_Pin_Header")}##pinningMenuHeader"))
            {
                // Pin / Unpin
                if (ImGui.MenuItem(isPinned ? LOC.Get("PARAM_FieldWindow_Context_Action_Unpin") : LOC.Get("PARAM_FieldWindow_Context_Action_Pin")))
                {
                    if (!Editor.Project.Descriptor.PinnedFields.ContainsKey(activeParam))
                    {
                        Editor.Project.Descriptor.PinnedFields.Add(activeParam, new List<string>());
                    }

                    List<string> pinned = Editor.Project.Descriptor.PinnedFields[activeParam];

                    if (isPinned)
                    {
                        pinned.Remove(internalName);
                    }
                    else if (!pinned.Contains(internalName))
                    {
                        pinned.Add(internalName);
                    }
                }

                if (isPinned)
                {
                    ParamFieldUtils.PinListReorderOptions(Editor.Project.Descriptor.PinnedFields[activeParam],
                        internalName);
                }

                // Unpin All
                if (ImGui.Selectable($"{LOC.Get("PARAM_FieldWindow_Context_Action_Unpin_All")}##unpinAllAction"))
                {
                    Editor.Project.Descriptor.PinnedFields.Clear();
                }

                ImGui.EndMenu();
            }
        }

        // Comparison
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_FieldWindow_Context_Comparison_Header")}##comparisonMenuHeader"))
        {
            if (col != null)
            {
                // Set Compare Field
                if (ImGui.Selectable($"{LOC.Get("PARAM_FieldWindow_Context_Action_Set_Compare_Field")}##setCompareFieldAction"))
                {
                    ParentView.Selection.SetCompareCol(col);
                }
            }

            // Clear Compare Field
            if (ImGui.Selectable($"{LOC.Get("PARAM_FieldWindow_Context_Action_Clear_Compare_Field")}##clearCompareFieldAction"))
            {
                ParentView.Selection.ClearCompareCol();
            }

            ImGui.EndMenu();
        }

        // Name Manipulation
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_FieldWindow_Context_Comparison_Name_Manipulation")}##nameManipMenuHeader"))
        {
            // Set Target Field
            if (ImGui.Selectable($"{LOC.Get("PARAM_FieldWindow_Context_Action_Set_Target_Field")}##setTargetFieldAction"))
            {
                ParentView.ParamRowWindow.SetNameManpulationTargetField(col.Def.InternalName);
            }
            GUI.Tooltip(LOC.Get("PARAM_FieldWindow_Context_Action_Set_Target_Field_TT"));

            ImGui.EndMenu();
        }
    }

    private bool AdditionalElementsForContextMenu(FieldMetaContext metaContext, Param.Row row, string internalName,
         dynamic oldval, ref object newval)
    {
        var isDisplayingAdditionalElements = false;

        if (metaContext.HasAnyDisplayedElements())
        {
            if (ParentView.FieldDecorators.HandleContextMenu(metaContext, row, oldval, ref newval))
            {
                ParentView.FieldInputHandler.SetLastPropertyManual(newval);
            }

            isDisplayingAdditionalElements = true;
        }

        if (CFG.Current.ParamEditor_Field_Context_Display_Reference_Search)
        {
            if (metaContext.DisplayVirtualReference || metaContext.DisplayExternalReference)
            {
                isDisplayingAdditionalElements = true;

                VirtualParamReferenceHelper.ContextMenu(ParentView, metaContext.VirtualReference, oldval, row, internalName);

                ExternalReferenceHelper.ContextMenu(ParentView, metaContext.VirtualReference, oldval, row, internalName, metaContext.ExternalReferences);
            }
        }

        return isDisplayingAdditionalElements;
    }
    /// <summary>
    /// Returns the position of <paramref name="row"/> among all rows in <paramref name="param"/>
    /// that share the same ID. Returns 0 if the param is null or the row is not found.
    /// </summary>
    private static int GetDuplicateIndex(Param param, Param.Row row)
    {
        if (param == null || row == null)
            return 0;

        int index = 0;
        foreach (var r in param.Rows)
        {
            if (r == row)
                return index;
            if (r.ID == row.ID)
                index++;
        }

        return 0;
    }

    /// <summary>
    /// Returns the row at position <paramref name="duplicateIndex"/> among all rows in
    /// <paramref name="param"/> that have the given <paramref name="id"/>.
    /// Falls back to the first match if the index is out of range, and returns null if
    /// no match exists at all.
    /// </summary>
    private static Param.Row GetRowAtDuplicateIndex(Param param, int id, int duplicateIndex)
    {
        if (param == null)
            return null;

        int count = 0;
        foreach (var row in param.Rows)
        {
            if (row.ID == id)
            {
                if (count == duplicateIndex)
                    return row;
                count++;
            }
        }

        return null;
    }
}
