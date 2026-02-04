using Andre.Formats;
using Hexa.NET.ImGui;
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
        ImGui.Text("Fields");
        ImGui.Separator();

        if (activeRow == null)
        {
            ImGui.BeginChild("columnsNONE");

            FocusManager.SetFocus(EditorFocusContext.ParamEditor_FieldList);

            ImGui.Text("Select a row to see properties");
            ImGui.EndChild();
        }
        else
        {
            ImGui.BeginChild("columns" + activeParam);

            FocusManager.SetFocus(EditorFocusContext.ParamEditor_FieldList);

            Param vanillaParam = ParentView.GetVanillaBank().Params?.GetValueOrDefault(activeParam);

            var bank = ParentView.GetPrimaryBank();
            var curRow = activeRow;
            var vanillaRow = vanillaParam?[activeRow.ID];

            var auxRows = Editor.Project.Handler.ParamData.AuxBanks
                .Select((bank, i) => (bank.Key, bank.Value.Params?
                .GetValueOrDefault(activeParam)?[activeRow.ID]))
                .ToList();

            var compareRow = ParentView.Selection.GetCompareRow();

            DisplayColumn(activeRow, vanillaRow, auxRows, compareRow,
                ref ParentView.Selection.GetCurrentPropSearchString(),
                activeParam, isActiveView);

            ImGui.EndChild();
        }
    }

    public void DisplayColumn(Param.Row curRow, Param.Row vanillaRow, List<(string, Param.Row)> auxRows,
        Param.Row compareRow, ref string propSearchString, string activeParam, bool isActiveView)
    {
        var meta = ParentView.GetParamData().GetParamMeta(curRow.Def);

        var imguiId = 0;
        var showParamCompare = auxRows.Count > 0;
        var showRowCompare = compareRow != null;

        DisplayHeader(isActiveView, ref propSearchString);
        DisplayGraph(isActiveView, curRow, meta);

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
                    ImGui.Text("Current");
                }

                // Vanilla
                if (CFG.Current.Param_ShowVanillaColumn && ImGui.TableNextColumn())
                {
                    ImGui.Text("Vanilla");
                }

                // Aux
                if (CFG.Current.Param_ShowAuxColumn)
                {
                    foreach ((var name, Param.Row r) in auxRows)
                    {
                        if (ImGui.TableNextColumn())
                        {
                            ImGui.Text(name);
                        }
                    }
                }
            }

            EditorTableUtils.ImguiTableSeparator();

            // ID and Name
            DisplayRowFields(curRow, meta, vanillaRow, auxRows, compareRow, ref imguiId, activeParam);

            var search = propSearchString;
            List<(ParamEditorPseudoColumn, Param.Column)> cols = CacheBank.GetCached(Editor, curRow, "fieldFilter",
                () => ParentView.MassEdit.CSE.Search((activeParam, curRow), search, true, true));

            List<(ParamEditorPseudoColumn, Param.Column)> vcols = CacheBank.GetCached(Editor, vanillaRow, "vFieldFilter",
                () => cols.Select((x, i) => x.GetAs(ParentView.GetVanillaBank().GetParamFromName(activeParam))).ToList());

            List<List<(ParamEditorPseudoColumn, Param.Column)>> auxCols = CacheBank.GetCached(Editor, auxRows,
                "auxFieldFilter", () => auxRows.Select((r, i) => cols.Select((c, j) => c.GetAs(Editor.Project.Handler.ParamData.AuxBanks[r.Item1].GetParamFromName(activeParam))).ToList()).ToList());

            // Pinned Fields
            if (CFG.Current.ParamEditor_Field_List_Pinned_Stay_Visible)
            {
                if (pinnedFields?.Count > 0)
                {
                    int i = 0;

                    DisplayPinnedFields(pinnedFields, meta, curRow, vanillaRow, auxRows, compareRow, cols, vcols,
                        auxCols, ref imguiId, activeParam, ref i);

                    EditorTableUtils.ImguiTableSeparator();
                }
            }

            // Main Fields
            DisplayFields(meta, curRow, vanillaRow, auxRows, compareRow, cols, vcols, auxCols, ref imguiId,
                activeParam, pinnedFields);

            ImGui.EndTable();
        }
    }

    private void DisplayHeader(bool isActiveView, ref string propSearchString)
    {
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
            ImGui.InputText("##fieldSearch", ref propSearchString,
                255);
            UIHelper.Tooltip($"Search <{InputManager.GetHint(KeybindID.ParamEditor_Focus_Searchbar)}>");

            if (ImGui.IsItemEdited())
            {
                CacheBank.ClearCaches();
            }

            // Toggle Community Field Names
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Book}"))
            {
                if(CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Source)
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

            UIHelper.Tooltip($"Toggle field name display type between Internal and Community.\nCurrent Mode: {CFG.Current.ParamEditor_FieldNameMode.GetDisplayName()}");

            // Toggle Vanilla Columns
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.AddressBook}"))
            {
                CFG.Current.Param_ShowVanillaColumn = !CFG.Current.Param_ShowVanillaColumn;
            }

            var vanillaColumnMode = "Hidden";
            if (CFG.Current.Param_ShowVanillaColumn)
                vanillaColumnMode = "Visible";

            UIHelper.Tooltip($"Toggle the display of the vanilla columns.\nCurrent Mode: {vanillaColumnMode}");

            // Toggle Auxiliary Columns
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.AddressBookO}"))
            {
                CFG.Current.Param_ShowAuxColumn = !CFG.Current.Param_ShowAuxColumn;
            }

            var auxColumnMode = "Hidden";
            if (CFG.Current.Param_ShowAuxColumn)
                auxColumnMode = "Visible";

            UIHelper.Tooltip($"Toggle the display of the auxiliary columns.\nCurrent Mode: {auxColumnMode}");

            // Toggle Field Offset Column
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.MapSigns}"))
            {
                CFG.Current.ParamEditor_Field_List_Display_Offsets = !CFG.Current.ParamEditor_Field_List_Display_Offsets;
            }

            var fieldOffsetColumnMode = "Hidden";
            if (CFG.Current.ParamEditor_Field_List_Display_Offsets)
                fieldOffsetColumnMode = "Visible";

            UIHelper.Tooltip($"Toggle the display of the field offset column.\nCurrent Mode: {fieldOffsetColumnMode}");

            // Toggle Field Padding
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Hubzilla}"))
            {
                CFG.Current.ParamEditor_Field_List_Display_Padding = !CFG.Current.ParamEditor_Field_List_Display_Padding;
            }

            var fieldPaddingMode = "Hidden";
            if (!CFG.Current.ParamEditor_Field_List_Display_Padding)
                fieldPaddingMode = "Visible";

            UIHelper.Tooltip($"Toggle the display of padding field.\nCurrent Mode: {fieldPaddingMode}");

            // Toggle Modified Background
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Bars}"))
            {
                CFG.Current.ParamEditor_Field_List_Display_Modified_Field_Background = !CFG.Current.ParamEditor_Field_List_Display_Modified_Field_Background;
            }

            var rowModifiedBgMode = "Hide Background";
            if (CFG.Current.ParamEditor_Field_List_Display_Modified_Field_Background)
                rowModifiedBgMode = "Display Background";

            UIHelper.Tooltip($"Toggle the display of the modified background on modified fields.\nCurrent Mode: {rowModifiedBgMode}");
        }
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

    private void DisplayRowFields(Param.Row row, ParamMeta meta, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, ref int imguiId, string activeParam)
    {
        PropertyInfo nameProp = row.GetType().GetProperty("Name");
        PropertyInfo idProp = row.GetType().GetProperty("ID");

        PropEditorPropInfoRow(row, meta, vrow, auxRows, crow, nameProp, "Name", ref imguiId,
            activeParam);
        PropEditorPropInfoRow(row, meta, vrow, auxRows, crow, idProp, "ID", ref imguiId,
            activeParam);

        ImGui.Spacing();
    }

    private void DisplayPinnedFields(List<string> pinList, ParamMeta meta,
        Param.Row row, Param.Row vrow, List<(string, Param.Row)> auxRows, Param.Row crow,
        List<(ParamEditorPseudoColumn, Param.Column)> cols, List<(ParamEditorPseudoColumn, Param.Column)> vcols,
        List<List<(ParamEditorPseudoColumn, Param.Column)>> auxCols, ref int imguiId, string activeParam, ref int index)
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
                PropEditorPropCellRow(meta,
                    row,
                    crow,
                    primaryMatches[i],
                    vrow,
                    vanillaMatches.Count > i ? vanillaMatches[i] : (ParamEditorPseudoColumn.None, null),
                    auxRows,
                    auxMatches.Select((x, j) => x.Count > i ? x[i] : (ParamEditorPseudoColumn.None, null)).ToList(),
                    OffsetTextOfColumn(primaryMatches[i].Item2),
                    ref imguiId, activeParam, true);
                index++;
            }
        }
    }

    private void DisplayFields(ParamMeta meta, Param.Row row, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, List<(ParamEditorPseudoColumn, Param.Column)> cols,
        List<(ParamEditorPseudoColumn, Param.Column)> vcols, List<List<(ParamEditorPseudoColumn, Param.Column)>> auxCols, ref int imguiId, string activeParam, List<string> pinnedFields)
    {
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

        if (meta != null &&
            CFG.Current.ParamEditor_Field_List_Allow_Rearrangement &&
            (meta is { AlternateOrder: null } || meta.AlternateOrder.Count != fieldOrder.Count))
        {
            meta.AlternateOrder = [.. fieldOrder];
        }

        var firstRow = true;
        var lastRowExists = false;
        int index = 0;
        foreach (var field in fieldOrder)
        {
            if (firstRow)
            {
                firstRow = false;

                if (pinnedFields?.Count > 0)
                {
                    DisplayPinnedFields(pinnedFields, meta, row, vrow, auxRows, crow, cols, vcols,
                        auxCols, ref imguiId, activeParam, ref index);

                    EditorTableUtils.ImguiTableSeparator();
                }
            }

            if (field.Equals("-"))
            {
                if (ParentView.MetaEditor.IsInEditorMode)
                {
                    var ncols = ImGui.TableGetColumnCount();
                    ImGui.TableNextRow();
                    for (var i = 0; i < ncols; i++)
                    {
                        if (ImGui.TableNextColumn())
                        {
                            ImGui.Selectable($"---##{index}{i}", false);
                            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                            {
                                ImGui.OpenPopup($"SeparatorContextMenu##{index}");
                            }
                        }
                    }
                }
                else if (lastRowExists)
                {
                    EditorTableUtils.ImguiTableSeparator();
                    lastRowExists = false;
                    continue;
                }
                index++;
            }

            var matches =
                cols?.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();
            var vmatches =
                vcols?.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();
            var auxMatches = auxCols?.Select((aux, i) =>
                aux.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList()).ToList();

            for (var i = 0; i < matches.Count; i++)
            {
                PropEditorPropCellRow(meta,
                    row,
                    crow,
                    matches[i],
                    vrow,
                    vmatches.Count > i ? vmatches[i] : (ParamEditorPseudoColumn.None, null),
                    auxRows,
                    auxMatches.Select((x, j) => x.Count > i ? x[i] : (ParamEditorPseudoColumn.None, null)).ToList(),
                    OffsetTextOfColumn(matches[i].Item2),
                    ref imguiId, activeParam, false);
                index++;
                lastRowExists = true;
            }
        }
    }

    // Many parameter options, which may be simplified.
    private void PropEditorPropInfoRow(Param.Row row, ParamMeta meta, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, PropertyInfo prop, string visualName, ref int imguiId, string activeParam)
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
            prop.PropertyType,
            prop,
            null,
            row,
            meta,
            activeParam,
            false,
            null);
    }

    private void PropEditorPropCellRow(ParamMeta meta, Param.Row row, Param.Row crow,
        (ParamEditorPseudoColumn, Param.Column) col, Param.Row vrow, (ParamEditorPseudoColumn, Param.Column) vcol,
        List<(string, Param.Row)> auxRows, List<(ParamEditorPseudoColumn, Param.Column)> auxCols, string fieldOffset,
        ref int imguiId, string activeParam, bool isPinned)
    {
        FieldRow(
            row.Get(col),
            crow?.Get(col),
            vcol.IsColumnValid() ? vrow?.Get(vcol) : null,
            auxRows.Select((r, i) => auxCols[i].IsColumnValid() ? r.Item2?.Get(auxCols[i]) : null).ToList(),
            ref imguiId,
            fieldOffset != null ? "0x" + fieldOffset : null, col.Item2.Def.InternalName,
            Editor.Project.Handler.ParamData.GetParamFieldMeta(meta, col.Item2.Def),
            col.GetColumnType(),
            typeof(Param.Cell).GetProperty("Value"),
            row[col.Item2],
            row,
            meta,
            activeParam,
            isPinned,
            col.Item2);
    }


    private void FieldRow(object oldval, object compareval, object vanillaval,
        List<object> auxVals, ref int imguiId, string fieldOffset, string internalName, ParamFieldMeta cellMeta,
        Type propType, PropertyInfo proprow, Param.Cell? nullableCell, Param.Row row, ParamMeta meta, string activeParam,
        bool isPinned, Param.Column col)
    {
        var metaContext = new FieldMetaContext(ParentView, meta, cellMeta, activeParam, internalName);

        object newval = null;

        if (!CFG.Current.ParamEditor_Field_List_Display_Padding && metaContext.IsPadding)
        {
            return;
        }

        //------------------------------
        // Name Column
        //------------------------------
        ImGui.PushID(imguiId);

        if (ImGui.TableNextColumn())
        {
            if (metaContext.InjectSeparator)
            {
                ImGui.Separator();
            }

            FieldTooltipHelper.IconTooltip(ParentView, metaContext, col);

            // Field selection
            ImGui.Selectable("", false, ImGuiSelectableFlags.AllowOverlap);

            FieldTooltipHelper.HoverTooltip(ParentView, metaContext, col);

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                if (CFG.Current.ParamEditor_Field_Context_Split)
                {
                    ImGui.OpenPopup("ParamRowNameMenu");
                }
                else
                {
                    ImGui.OpenPopup("ParamRowCommonMenu");
                }
            }

            ImGui.SameLine();

            // Name column
            PropertyRowName(fieldOffset, ref internalName, cellMeta);

            // Labels
            ParentView.FieldDecorators.HandleLabels(metaContext, row);
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
                if (CFG.Current.ParamEditor_Field_List_Display_Modified_Field_Background)
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
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);
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
                if (CFG.Current.ParamEditor_Field_Context_Split)
                {
                    ImGui.OpenPopup("ParamRowValueMenu");
                }
                else
                {
                    ImGui.OpenPopup("ParamRowCommonMenu");
                }
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
                if (CFG.Current.ParamEditor_Field_Context_Split)
                {
                    ImGui.OpenPopup("ParamRowValueMenu");
                }
                else
                {
                    ImGui.OpenPopup("ParamRowCommonMenu");
                }
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
                        if (CFG.Current.ParamEditor_Field_List_Display_Modified_Field_Background)
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

        if (ImGui.BeginPopup("ParamRowCommonMenu"))
        {
            FieldName_ContextMenu(metaContext, internalName, activeParam,
                activeParam != null, isPinned, col, propType, oldval, true);

            FieldValue_ContextMenu(metaContext, row, internalName, oldval, ref newval);

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("ParamRowNameMenu"))
        {
            FieldName_ContextMenu(metaContext, internalName, activeParam,
                activeParam != null, isPinned, col, propType, oldval, true);

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("ParamRowValueMenu"))
        {
            FieldName_ContextMenu(metaContext, internalName, activeParam,
                activeParam != null, isPinned, col, propType, oldval, false);

            FieldValue_ContextMenu(metaContext, row, internalName, oldval, ref newval);

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

    private void PropertyRowName(string fieldOffset, ref string internalName, ParamFieldMeta cellMeta)
    {
        var altName = cellMeta?.AltName;

        if (cellMeta != null && ParentView.MetaEditor.IsInEditorMode)
        {
            var editName = !string.IsNullOrWhiteSpace(altName) ? altName : internalName;
            ImGui.InputText("##editName", ref editName, 128);

            if (editName.Equals(internalName) || editName.Equals(""))
            {
                cellMeta.AltName = null;
            }
            else
            {
                cellMeta.AltName = editName;
            }
        }
        else
        {
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

            ImGui.TextUnformatted(printedName);
        }
    }

    private void FieldName_ContextMenu(FieldMetaContext metaContext, string internalName, string activeParam, bool showPinOptions, bool isPinned, Param.Column col, Type propType, dynamic oldval, bool isNameMenu)
    {
        var altName = metaContext.FieldMeta?.AltName;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 10f));

        // Copy Name
        if (ImGui.MenuItem("Copy Internal Name"))
        {
            PlatformUtils.Instance.SetClipboardText(internalName);
        }

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
                    ImGui.TextColored(new Vector4(.4f, .7f, 1f, 1f), $"{metaContext.Description}");
                }
                else
                {
                    ImGui.TextColored(new Vector4(1.0f, 1.0f, 1.0f, 0.7f),
                        "Info regarding this field has not been written.");
                }
            }
        }
        else
        {
            ImGui.TextColored(new Vector4(1.0f, 0.7f, 0.4f, 1.0f), Utils.ImGuiEscape(internalName, "", true));
        }

        if (isNameMenu && (displayAttributes || displayDescription))
        {
            ImGui.Separator();
        }

        if (!isNameMenu)
        {
            ImGui.PopStyleVar();
            return;
        }

        // Add to Search
        if (ImGui.MenuItem("Add to Searchbar"))
        {
            if (col != null)
            {
                EditorCommandQueue.AddCommand($@"param/search/prop {internalName.Replace(" ", "\\s")} ");
            }
            else
            {
                // Headers
                EditorCommandQueue.AddCommand($@"param/search/{internalName.Replace(" ", "\\s")} ");
            }
        }

        // Add to Mass Edit
        if (ImGui.MenuItem("Add to Mass Edit"))
        {
            ParentView.MassEdit.ConstructCommandFromField(internalName);
        }

        // Search for Non-Default Values
        if (col != null)
        {
            if (ImGui.MenuItem("Search for Non-Default Values"))
            {
                EditorCommandQueue.AddCommand($@"param/search/proprange {internalName.Replace(" ", "\\s")} 0.01 {int.MaxValue}");
            }
        }

        // Pin Options
        if (showPinOptions)
        {
            if (ImGui.MenuItem(isPinned ? "Unpin " : "Pin " + internalName))
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

            if (ImGui.Selectable("Unpin all"))
            {
                Editor.Project.Descriptor.PinnedFields.Clear();
            }

            ImGui.Separator();
        }

        // Compare
        if (col != null && ImGui.MenuItem("Compare field"))
        {
            ParentView.Selection.SetCompareCol(col);
        }

        // Value Distribution
        if (ImGui.Selectable("View value distribution in selected rows..."))
        {
            EditorCommandQueue.AddCommand($@"param/menu/distributionPopup/{internalName}");
        }

        ImGui.PopStyleVar();
    }

    private void FieldValue_ContextMenu(FieldMetaContext metaContext, Param.Row row, string internalName,
         dynamic oldval, ref object newval)
    {
        if (metaContext.HasAnyDisplayedElements())
        {
            ImGui.Separator();
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);

            if (ParentView.FieldDecorators.HandleContextMenu(metaContext, row, oldval, ref newval))
            {
                ParentView.FieldInputHandler.SetLastPropertyManual(newval);
            }

            ImGui.PopStyleColor();
        }

        if (CFG.Current.ParamEditor_Field_Context_Display_Reference_Search)
        {
            if (metaContext.DisplayVirtualReference || metaContext.DisplayExternalReference)
            {
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Benefit_Text_Color);

                VirtualParamReferenceHelper.ContextMenu(ParentView, metaContext.VirtualReference, oldval, row, internalName);

                ExternalReferenceHelper.ContextMenu(ParentView, metaContext.VirtualReference, oldval, row, internalName, metaContext.ExternalReferences);

                ImGui.PopStyleColor();
            }
        }

        var massEditDisplayMode = CFG.Current.ParamEditor_Field_List_Context_Mass_Edit_Display_Mode;

        if (massEditDisplayMode is not ParamFieldMassEditMode.None)
        {
            ImGui.Separator();

            if (massEditDisplayMode is ParamFieldMassEditMode.CommandPalette)
            {
                if (ImGui.Selectable("Mass edit"))
                {
                    EditorCommandQueue.AddCommand(
                        $@"param/menu/massEditRegex/selection: {Regex.Escape(internalName)}: ");
                }

                if (ImGui.Selectable("Reset to vanilla"))
                {
                    ParentView.MassEdit.ApplyMassEdit($"selection && !added: {Regex.Escape(internalName)}: = vanilla;");
                }
            }
            else
            {
                if (ImGui.CollapsingHeader("Mass edit", ImGuiTreeNodeFlags.SpanFullWidth))
                {
                    ImGui.Separator();

                    if (ImGui.Selectable("Manually..."))
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/menu/massEditRegex/selection: {Regex.Escape(internalName)}: ");
                    }

                    if (ImGui.Selectable("Reset to vanilla..."))
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/menu/massEditRegex/selection && !added: {Regex.Escape(internalName)}: = vanilla;");
                    }

                    ImGui.Separator();

                    if (ParentView.MassEdit.AutoFill != null)
                    {
                        var res = ParentView.MassEdit.AutoFill.MassEditOpAutoFill();
                        if (res != null)
                        {
                            EditorCommandQueue.AddCommand(
                                $@"param/menu/massEditRegex/selection: {Regex.Escape(internalName)}: " + res);
                        }
                    }
                }
            }
        }
    }
}
