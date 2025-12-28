using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// The field column within a Param Editor view
/// </summary>
public class ParamFieldView
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamEditorView View;

    private Dictionary<string, PropertyInfo[]> _propCache = new();
    public ActionManager ContextActionManager;

    public ParamFieldView(ParamEditorScreen editor, ProjectEntry project, ParamEditorView view)
    {
        Editor = editor;
        Project = project;
        View = view;

        ContextActionManager = editor.EditorActionManager;
    }

    /// <summary>
    /// Entry point
    /// </summary>
    /// <param name="isActiveView"></param>
    /// <param name="activeParam"></param>
    /// <param name="activeRow"></param>
    public void Display(bool isActiveView, string activeParam, Param.Row activeRow)
    {
        ImGui.Text("Fields");
        ImGui.Separator();

        if (activeRow == null)
        {
            ImGui.BeginChild("columnsNONE");

            Editor.ContextManager.SetWindowContext(ParamEditorContext.FieldList);

            ImGui.Text("Select a row to see properties");
            ImGui.EndChild();
        }
        else
        {
            ImGui.BeginChild("columns" + activeParam);

            Editor.ContextManager.SetWindowContext(ParamEditorContext.FieldList);

            Param vanillaParam = Editor.Project.ParamData.VanillaBank.Params?.GetValueOrDefault(activeParam);

            var bank = Editor.Project.ParamData.PrimaryBank;
            var curRow = activeRow;
            var vanillaRow = vanillaParam?[activeRow.ID];
            var auxRows = Editor.Project.ParamData.AuxBanks
                .Select((bank, i) => (bank.Key, bank.Value.Params?
                .GetValueOrDefault(activeParam)?[activeRow.ID]))
                .ToList();
            var compareRow = View.Selection.GetCompareRow();

            DisplayColumn(bank, activeRow, vanillaRow, auxRows, compareRow, ref View.Selection.GetCurrentPropSearchString(),
                activeParam, isActiveView, View.Selection);

            ImGui.EndChild();
        }
    }

    /// <summary>
    /// Handling of the field column.
    /// </summary>
    /// <param name="bank"></param>
    /// <param name="curRow"></param>
    /// <param name="vanillaRow"></param>
    /// <param name="auxRows"></param>
    /// <param name="compareRow"></param>
    /// <param name="propSearchString"></param>
    /// <param name="activeParam"></param>
    /// <param name="isActiveView"></param>
    /// <param name="selection"></param>
    public void DisplayColumn(ParamBank bank, Param.Row curRow, Param.Row vanillaRow, List<(string, Param.Row)> auxRows,
        Param.Row compareRow, ref string propSearchString, string activeParam, bool isActiveView,
        ParamSelection selection)
    {
        var meta = Editor.Project.ParamData.GetParamMeta(curRow.Def);

        var imguiId = 0;
        var showParamCompare = auxRows.Count > 0;
        var showRowCompare = compareRow != null;

        DisplayHeader(Editor, isActiveView, ref propSearchString);
        DisplayGraph(Editor, isActiveView, curRow, meta);

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
        if (ParamEditorDecorations.ImGuiTableStdColumns("ParamFieldsT", columnCount, false))
        {
            List<string> pinnedFields =
                Editor.Project.PinnedFields.GetValueOrDefault(activeParam, null);

            if (CFG.Current.Param_PinnedFieldsStayVisible)
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

            ParamEditorDecorations.ImguiTableSeparator();

            // ID and Name
            DisplayRowFields(bank, curRow, meta, vanillaRow, auxRows, compareRow, ref imguiId, selection, activeParam);

            var search = propSearchString;
            List<(ParamEditorPseudoColumn, Param.Column)> cols = UICache.GetCached(Editor, curRow, "fieldFilter",
                () => Editor.MassEditHandler.cse.Search((activeParam, curRow), search, true, true));

            List<(ParamEditorPseudoColumn, Param.Column)> vcols = UICache.GetCached(Editor, vanillaRow, "vFieldFilter",
                () => cols.Select((x, i) => x.GetAs(Editor.Project.ParamData.VanillaBank.GetParamFromName(activeParam))).ToList());

            List<List<(ParamEditorPseudoColumn, Param.Column)>> auxCols = UICache.GetCached(Editor, auxRows,
                "auxFieldFilter", () => auxRows.Select((r, i) => cols.Select((c, j) => c.GetAs(Editor.Project.ParamData.AuxBanks[r.Item1].GetParamFromName(activeParam))).ToList()).ToList());

            // Pinned Fields
            if (CFG.Current.Param_PinnedFieldsStayVisible)
            {
                if (pinnedFields?.Count > 0)
                {
                    int i = 0;

                    DisplayPinnedFields(pinnedFields, bank, meta, curRow, vanillaRow, auxRows, compareRow, cols, vcols,
                        auxCols, ref imguiId, activeParam, selection, ref i);

                    ParamEditorDecorations.ImguiTableSeparator();
                }
            }

            // Main Fields
            if (!CFG.Current.Param_PinGroups_ShowOnlyPinnedFields)
            {
                DisplayFields(meta, bank, curRow, vanillaRow, auxRows, compareRow, cols, vcols, auxCols, ref imguiId,
                    activeParam, selection, pinnedFields);
            }

            ImGui.EndTable();
        }

    }


    /// <summary>
    /// Display the header for the field view
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="isActiveView"></param>
    /// <param name="propSearchString"></param>
    private static void DisplayHeader(ParamEditorScreen editor, bool isActiveView, ref string propSearchString)
    {
        if (propSearchString != null)
        {
            if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SearchField))
            {
                ImGui.SetKeyboardFocusHere();
            }

            // Autofill
            if (editor.MassEditHandler.AutoFill != null)
            {
                ImGui.AlignTextToFramePadding();
                var resAutoCol = editor.MassEditHandler.AutoFill.ColumnSearchBarAutoFill();
                if (resAutoCol != null)
                {
                    propSearchString = resAutoCol;
                    UICache.ClearCaches();
                }
            }

            ImGui.SameLine();

            // Field search
            ImGui.AlignTextToFramePadding();
            ImGui.InputText("##fieldSearch", ref propSearchString,
                255);
            UIHelper.Tooltip($"Search <{KeyBindings.Current.PARAM_SearchField.HintText}>");

            if (ImGui.IsItemEdited())
            {
                UICache.ClearCaches();
            }

            // Toggle Community Field Names
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Book}", DPI.IconButtonSize))
            {
                CFG.Current.Param_MakeMetaNamesPrimary = !CFG.Current.Param_MakeMetaNamesPrimary;
            }

            var communityFieldNameMode = "Internal";
            if (CFG.Current.Param_MakeMetaNamesPrimary)
                communityFieldNameMode = "Community";

            UIHelper.Tooltip($"Toggle field name display type between Internal and Community.\nCurrent Mode: {communityFieldNameMode}");

            // Toggle Vanilla Columns
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.AddressBook}", DPI.IconButtonSize))
            {
                CFG.Current.Param_ShowVanillaColumn = !CFG.Current.Param_ShowVanillaColumn;
            }

            var vanillaColumnMode = "Hidden";
            if (CFG.Current.Param_ShowVanillaColumn)
                vanillaColumnMode = "Visible";

            UIHelper.Tooltip($"Toggle the display of the vanilla columns.\nCurrent Mode: {vanillaColumnMode}");

            // Toggle Auxiliary Columns
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.AddressBookO}", DPI.IconButtonSize))
            {
                CFG.Current.Param_ShowAuxColumn = !CFG.Current.Param_ShowAuxColumn;
            }

            var auxColumnMode = "Hidden";
            if (CFG.Current.Param_ShowAuxColumn)
                auxColumnMode = "Visible";

            UIHelper.Tooltip($"Toggle the display of the auxiliary columns.\nCurrent Mode: {auxColumnMode}");

            // Toggle Field Offset Column
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.MapSigns}", DPI.IconButtonSize))
            {
                CFG.Current.Param_ShowFieldOffsets = !CFG.Current.Param_ShowFieldOffsets;
            }

            var fieldOffsetColumnMode = "Hidden";
            if (CFG.Current.Param_ShowFieldOffsets)
                fieldOffsetColumnMode = "Visible";

            UIHelper.Tooltip($"Toggle the display of the field offset column.\nCurrent Mode: {fieldOffsetColumnMode}");

            // Toggle Field Padding
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Hubzilla}", DPI.IconButtonSize))
            {
                CFG.Current.Param_HidePaddingFields = !CFG.Current.Param_HidePaddingFields;
            }

            var fieldPaddingMode = "Hidden";
            if (!CFG.Current.Param_HidePaddingFields)
                fieldPaddingMode = "Visible";

            UIHelper.Tooltip($"Toggle the display of padding field.\nCurrent Mode: {fieldPaddingMode}");

            // Toggle Icon Preview
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Eye}", DPI.IconButtonSize))
            {
                CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn = !CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn;
            }

            var iconPreviewMode = "Hidden";
            if (CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn)
                iconPreviewMode = "Visible";

            UIHelper.Tooltip($"Toggle the display of icons.\nCurrent Mode: {iconPreviewMode}");
        }
    }

    /// <summary>
    /// Display a graph for param rows that support it
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="isActiveView"></param>
    /// <param name="row"></param>
    private static void DisplayGraph(ParamEditorScreen editor, bool isActiveView, Param.Row row, ParamMeta meta)
    {
        if (meta == null)
            return;

        if (CFG.Current.Param_ShowGraphVisualisation)
        {
            var columnWidth = ImGui.GetColumnWidth();

            var graphSectionSize = new Vector2(columnWidth, 400);
            var graphSize = new Vector2(columnWidth * 0.9f, 400 * 0.8f);

            if (meta.CalcCorrectDef != null || meta.SoulCostDef != null)
            {
                ImGui.BeginChild("graphView", graphSectionSize);

                FieldDecorators.DrawCalcCorrectGraph(editor, meta, row, graphSize);

                ImGui.EndChild();
            }
        }
    }

    /// <summary>
    /// The ID / Name fields from the Param.Row
    /// </summary>
    /// <param name="bank"></param>
    /// <param name="row"></param>
    /// <param name="meta"></param>
    /// <param name="vrow"></param>
    /// <param name="auxRows"></param>
    /// <param name="crow"></param>
    /// <param name="imguiId"></param>
    /// <param name="selection"></param>
    /// <param name="activeParam"></param>
    private void DisplayRowFields(ParamBank bank, Param.Row row, ParamMeta meta, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, ref int imguiId, ParamSelection selection, 
        string activeParam)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);

        PropertyInfo nameProp = row.GetType().GetProperty("Name");
        PropertyInfo idProp = row.GetType().GetProperty("ID");

        PropEditorPropInfoRow(bank, row, meta, vrow, auxRows, crow, nameProp, "Name", ref imguiId, selection,
            activeParam);
        PropEditorPropInfoRow(bank, row, meta, vrow, auxRows, crow, idProp, "ID", ref imguiId, selection, 
            activeParam);

        ImGui.PopStyleColor();
        ImGui.Spacing();
    }

    /// <summary>
    /// The pinned fields
    /// </summary>
    /// <param name="pinList"></param>
    /// <param name="bank"></param>
    /// <param name="meta"></param>
    /// <param name="row"></param>
    /// <param name="vrow"></param>
    /// <param name="auxRows"></param>
    /// <param name="crow"></param>
    /// <param name="cols"></param>
    /// <param name="vcols"></param>
    /// <param name="auxCols"></param>
    /// <param name="imguiId"></param>
    /// <param name="activeParam"></param>
    /// <param name="selection"></param>
    /// <param name="index"></param>
    private void DisplayPinnedFields(List<string> pinList, ParamBank bank, ParamMeta meta,
        Param.Row row, Param.Row vrow, List<(string, Param.Row)> auxRows, Param.Row crow, 
        List<(ParamEditorPseudoColumn, Param.Column)> cols, List<(ParamEditorPseudoColumn, Param.Column)> vcols, 
        List<List<(ParamEditorPseudoColumn, Param.Column)>> auxCols, ref int imguiId, string activeParam, 
        ParamSelection selection, ref int index)
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
                PropEditorPropCellRow(bank,
                    meta,
                    row,
                    crow,
                    primaryMatches[i],
                    vrow,
                    vanillaMatches.Count > i ? vanillaMatches[i] : (ParamEditorPseudoColumn.None, null),
                    auxRows,
                    auxMatches.Select((x, j) => x.Count > i ? x[i] : (ParamEditorPseudoColumn.None, null)).ToList(),
                    OffsetTextOfColumn(primaryMatches[i].Item2),
                    ref imguiId, activeParam, true, selection);
                index++;
            }
        }
    }

    /// <summary>
    /// The main fields
    /// </summary>
    /// <param name="meta"></param>
    /// <param name="bank"></param>
    /// <param name="row"></param>
    /// <param name="vrow"></param>
    /// <param name="auxRows"></param>
    /// <param name="crow"></param>
    /// <param name="cols"></param>
    /// <param name="vcols"></param>
    /// <param name="auxCols"></param>
    /// <param name="imguiId"></param>
    /// <param name="activeParam"></param>
    /// <param name="selection"></param>
    /// <param name="pinnedFields"></param>
    private void DisplayFields(ParamMeta meta, ParamBank bank, Param.Row row, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, List<(ParamEditorPseudoColumn, Param.Column)> cols,
        List<(ParamEditorPseudoColumn, Param.Column)> vcols, List<List<(ParamEditorPseudoColumn, Param.Column)>> auxCols, ref int imguiId,
        string activeParam, ParamSelection selection, List<string> pinnedFields)
    {
        List<string> fieldOrder = meta is { AlternateOrder: not null } && CFG.Current.Param_AllowFieldReorder
            ? [..meta.AlternateOrder]
            : [];

        foreach (PARAMDEF.Field field in row.Def.Fields)
        {
            if (!fieldOrder.Contains(field.InternalName))
            {
                fieldOrder.Add(field.InternalName);
            }
        }

        if (meta != null &&
            CFG.Current.Param_AllowFieldReorder && 
            (meta is { AlternateOrder: null } || meta.AlternateOrder.Count != fieldOrder.Count))
        {
            meta.AlternateOrder = [..fieldOrder];
        }
        var firstRow = true;
        var lastRowExists = false;
        int index = 0;
        foreach (var field in fieldOrder)
        {
            if(firstRow)
            {
                firstRow = false;

                if (!CFG.Current.Param_PinnedFieldsStayVisible)
                {
                    if (pinnedFields?.Count > 0)
                    {
                        DisplayPinnedFields(pinnedFields, bank, meta, row, vrow, auxRows, crow, cols, vcols, 
                            auxCols, ref imguiId, activeParam, selection, ref index);
                        ParamEditorDecorations.ImguiTableSeparator();
                    }
                }
            }

            if (field.Equals("-"))
            {
                if (Editor.EditorMode)
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
                    ParamEditorDecorations.ImguiTableSeparator();
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
                PropEditorPropCellRow(bank,
                    meta,
                    row,
                    crow,
                    matches[i],
                    vrow,
                    vmatches.Count > i ? vmatches[i] : (ParamEditorPseudoColumn.None, null),
                    auxRows,
                    auxMatches.Select((x, j) => x.Count > i ? x[i] : (ParamEditorPseudoColumn.None, null)).ToList(),
                    OffsetTextOfColumn(matches[i].Item2),
                    ref imguiId, activeParam, false, selection);
                index++;
                lastRowExists = true;
            }
        }
    }

    // Many parameter options, which may be simplified.
    private void PropEditorPropInfoRow(ParamBank bank, Param.Row row, ParamMeta meta, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, PropertyInfo prop, string visualName, ref int imguiId,
        ParamSelection selection, string activeParam)
    {
        PropEditorPropRow(
            bank,
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
            null,
            selection);
    }

    private void PropEditorPropCellRow(ParamBank bank, ParamMeta meta, Param.Row row, Param.Row crow,
        (ParamEditorPseudoColumn, Param.Column) col, Param.Row vrow, (ParamEditorPseudoColumn, Param.Column) vcol,
        List<(string, Param.Row)> auxRows, List<(ParamEditorPseudoColumn, Param.Column)> auxCols, string fieldOffset,
        ref int imguiId, string activeParam, bool isPinned, ParamSelection selection)
    {
        PropEditorPropRow(
            bank,
            row.Get(col),
            crow?.Get(col),
            vcol.IsColumnValid() ? vrow?.Get(vcol) : null,
            auxRows.Select((r, i) => auxCols[i].IsColumnValid() ? r.Item2?.Get(auxCols[i]) : null).ToList(),
            ref imguiId,
            fieldOffset != null ? "0x" + fieldOffset : null, col.Item2.Def.InternalName,
            Editor.Project.ParamData.GetParamFieldMeta(meta, col.Item2.Def),
            col.GetColumnType(),
            typeof(Param.Cell).GetProperty("Value"),
            row[col.Item2],
            row,
            meta,
            activeParam,
            isPinned,
            col.Item2,
            selection);
    }


    private void PropEditorPropRow(ParamBank bank, object oldval, object compareval, object vanillaval,
        List<object> auxVals, ref int imguiId, string fieldOffset, string internalName, ParamFieldMeta cellMeta,
        Type propType, PropertyInfo proprow, Param.Cell? nullableCell, Param.Row row, ParamMeta meta, string activeParam,
        bool isPinned, Param.Column col, ParamSelection selection)
    {
        var Wiki = cellMeta?.Wiki;

        List<ParamRef> RefTypes = cellMeta?.RefTypes;
        List<FMGRef> FmgRef = cellMeta?.FmgRef;
        List<ExtRef> ExtRefs = cellMeta?.ExtRefs;
        List<FMGRef> MapFmgRef = cellMeta?.MapFmgRef;

        IconConfig IconConfig = cellMeta?.IconConfig;

        var VirtualRef = cellMeta?.VirtualRef;

        ParamEnum Enum = cellMeta?.EnumType;
        var IsBool = cellMeta?.IsBool ?? false;
        var IsInvertedPercentage = cellMeta?.IsInvertedPercentage ?? false;
        var IsPaddingField = cellMeta?.IsPaddingField ?? false;
        var IsObsoleteField = cellMeta?.IsObsoleteField ?? false;
        var AddSeparator = cellMeta?.AddSeparatorNextLine ?? false;

        var displayRefTypes = !CFG.Current.Param_HideReferenceRows && RefTypes != null;
        var displayFmgRef = !CFG.Current.Param_HideReferenceRows && FmgRef != null;
        var displayIcon = !CFG.Current.Param_HideReferenceRows && IconConfig != null;
        var displayEnum = !CFG.Current.Param_HideEnums && Enum != null;
        var displayMapFmgRef = !CFG.Current.Param_HideReferenceRows && MapFmgRef != null;

        bool showParticleEnum = false;
        bool showSoundEnum = false;
        bool showFlagEnum = false;
        bool showCutsceneEnum = false;
        bool showMovieEnum = false;
        bool showCharacterEnum = false;
        bool showProjectEnum = false;
        bool showTileRef = false;

        var FlagAliasEnum_ConditionalField = cellMeta?.FlagAliasEnum_ConditionalField;
        var FlagAliasEnum_ConditionalValue = cellMeta?.FlagAliasEnum_ConditionalValue;

        var MovieAliasEnum_ConditionalField = cellMeta?.MovieAliasEnum_ConditionalField;
        var MovieAliasEnum_ConditionalValue = cellMeta?.MovieAliasEnum_ConditionalValue;

        bool showParamFieldOffset = false;
        var paramFieldIndex = "";

        if (cellMeta != null)
        {
            showParticleEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowParticleEnumList;
            showSoundEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowSoundEnumList;
            showFlagEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowFlagEnumList;
            showCutsceneEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowCutsceneEnumList;
            showMovieEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowMovieEnumList;
            showCharacterEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowCharacterEnumList;
            showProjectEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowProjectEnumList;

            showParamFieldOffset = cellMeta.ShowParamFieldOffset;
            paramFieldIndex = cellMeta.ParamFieldOffsetIndex;

            if(cellMeta.TileRef != null)
            {
                showTileRef = true;
            }
        }

        object newval = null;

        if(CFG.Current.Param_HidePaddingFields && IsPaddingField)
        {
            return;
        }

        if (CFG.Current.Param_HideObsoleteFields && IsObsoleteField)
        {
            return;
        }

        ImGui.PushID(imguiId);

        if (ImGui.TableNextColumn())
        {
            if(AddSeparator)
            {
                ImGui.Separator();
            }

            // Help icon text
            if (CFG.Current.Param_ShowFieldDescription_onIcon || CFG.Current.Param_ShowFieldLimits_onIcon)
            {
                ImGui.AlignTextToFramePadding();

                if (Wiki != null)
                {
                    var helpIconText = "";

                    if(CFG.Current.Param_ShowFieldDescription_onIcon)
                    {
                        helpIconText = Wiki;
                    }

                    if (CFG.Current.Param_ShowFieldLimits_onIcon)
                    {
                        if (CFG.Current.Param_ShowFieldDescription_onIcon)
                        {
                            helpIconText = helpIconText +
                                "\n" +
                                "-----\n";
                        }

                        helpIconText = helpIconText +
                        $"Minimum: {col.Def.Minimum}\n" +
                        $"Maximum: {col.Def.Maximum}\n" +
                        $"Increment: {col.Def.Increment}";
                    }

                    if (ParamEditorDecorations.HelpIcon(internalName, ref helpIconText, true))
                    {
                        cellMeta.Wiki = Wiki;
                    }

                    ImGui.SameLine();
                }
                else
                {
                    ImGui.Text(" ");
                    ImGui.SameLine();
                }
            }

            // Field selection
            ImGui.Selectable("", false, ImGuiSelectableFlags.AllowOverlap);

            // Help hover text
            if (CFG.Current.Param_ShowFieldDescription_onName || CFG.Current.Param_ShowFieldLimits_onName)
            {
                if (Wiki != null)
                {
                    var helpIconText = "";

                    if (CFG.Current.Param_ShowFieldDescription_onName)
                    {
                        helpIconText = Wiki;
                    }
                    if (CFG.Current.Param_ShowFieldLimits_onName)
                    {
                        if (CFG.Current.Param_ShowFieldDescription_onName)
                        {
                            helpIconText = helpIconText + 
                                "\n" + 
                                "-----\n";
                        }

                        helpIconText = helpIconText +
                        $"Minimum: {col.Def.Minimum}\n" +
                        $"Maximum: {col.Def.Maximum}\n" +
                        $"Increment: {col.Def.Increment}";
                    }

                    UIHelper.Tooltip(helpIconText);
                }
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                if (!CFG.Current.Param_FieldContextMenu_Split)
                {
                    ImGui.OpenPopup("ParamRowCommonMenu");
                }
                else
                {
                    ImGui.OpenPopup("ParamRowNameMenu");
                }
            }

            ImGui.SameLine();

            // Name column
            PropertyRowName(Editor, fieldOffset, ref internalName, cellMeta);

            if (displayRefTypes || displayFmgRef || displayIcon || displayEnum || showParticleEnum || showSoundEnum || showFlagEnum || showCutsceneEnum || showMovieEnum || showCharacterEnum || showProjectEnum || showParamFieldOffset || displayMapFmgRef || showTileRef)
            {
                ImGui.BeginGroup();

                // Param Ref
                if (displayRefTypes)
                {
                    FieldDecorators.ParamReference_Title(RefTypes, row);
                }

                // Text Ref
                if (displayFmgRef)
                {
                    FieldDecorators.TextReference_Title(FmgRef, row);
                }

                // Map Text Ref
                if(displayMapFmgRef)
                {
                    FieldDecorators.TextReference_Title(MapFmgRef, row, "MAP FMGS");
                }

                // Field Icon
                if (displayIcon)
                {
                    FieldDecorators.FieldIcon_Title(IconConfig, row);
                }

                if (displayEnum)
                {
                    FieldDecorators.Enum_Title(Enum);
                }

                // Particle list
                if (showParticleEnum)
                {
                    FieldDecorators.AliasEnum_Title("PARTICLES");
                }

                // Sound list
                if (showSoundEnum)
                {
                    FieldDecorators.AliasEnum_Title("SOUNDS");
                }

                // Flag list
                if (showFlagEnum)
                {
                    FieldDecorators.ConditionalAliasEnum_Title("FLAGS", row, FlagAliasEnum_ConditionalField, FlagAliasEnum_ConditionalValue);
                }

                // Cutscene list
                if (showCutsceneEnum)
                {
                    FieldDecorators.AliasEnum_Title("CUTSCENES");
                }

                // Movie list
                if (showMovieEnum)
                {
                    FieldDecorators.ConditionalAliasEnum_Title("MOVIES", row, MovieAliasEnum_ConditionalField, MovieAliasEnum_ConditionalValue);
                }

                // Character list
                if (showCharacterEnum)
                {
                    FieldDecorators.AliasEnum_Title("CHARACTERS");
                }

                // Project Enum
                if (showProjectEnum)
                {
                    FieldDecorators.ProjectEnum_Title(Editor, cellMeta.ProjectEnumType);
                }

                // TileRef
                if (showTileRef)
                {
                    FieldDecorators.TileRef_Title(Editor, cellMeta.TileRef);
                }

                // Param Field Offset
                if (showParamFieldOffset)
                {
                    FieldDecorators.ParamFieldOffset_Title(activeParam, row, paramFieldIndex);
                }

                ImGui.EndGroup();

                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    if (!CFG.Current.Param_FieldContextMenu_Split)
                    {
                        ImGui.OpenPopup("ParamRowCommonMenu");
                    }
                    else
                    {
                        ImGui.OpenPopup("ParamRowNameMenu");
                    }
                }
            }
        }

        // Compare column
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
        var isRef = CFG.Current.Param_HideReferenceRows == false && (RefTypes != null || FmgRef != null || IconConfig != null) || CFG.Current.Param_HideEnums == false && Enum != null || VirtualRef != null || ExtRefs != null || CFG.Current.Param_HideEnums == false && showParticleEnum;

        if (ImGui.TableNextColumn())
        {
            if (conflict)
            {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_Input_Conflict_Background);
            }
            else if (diffVanilla)
            {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_Input_Vanilla_Background);
            }

            if (isRef)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_IsRef_Text);
            }
            else if (matchDefault)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            }

            if (AddSeparator)
            {
                ImGui.Separator();
            }

            // Property Editor UI
            ParamFieldInput.DisplayFieldInput(Editor, propType, oldval, ref newval, IsBool, IsInvertedPercentage);

            if (isRef || matchDefault) //if diffVanilla, remove styling later
            {
                ImGui.PopStyleColor();
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                if (!CFG.Current.Param_FieldContextMenu_Split)
                {
                    ImGui.OpenPopup("ParamRowCommonMenu");
                }
                else
                {
                    ImGui.OpenPopup("ParamRowValueMenu");
                }
            }

            if (displayRefTypes || displayFmgRef || displayIcon || displayEnum || showParticleEnum || showSoundEnum || showFlagEnum || showCutsceneEnum || showMovieEnum || showCharacterEnum || showProjectEnum || showParamFieldOffset || displayMapFmgRef || showTileRef)
            {
                ImGui.BeginGroup();

                // ParamRef
                if (displayRefTypes)
                {
                    FieldDecorators.ParamReference_Value(Editor, bank, RefTypes, row, oldval);
                }

                // FmgRef
                if (displayFmgRef)
                {
                    FieldDecorators.TextReference_Value(Editor, FmgRef, row, oldval);
                }

                // MapFmgRef
                if (displayMapFmgRef)
                {
                    FieldDecorators.TextReference_Value(Editor, MapFmgRef, row, oldval);
                }

                // Field Icon
                if (displayIcon)
                {
                    FieldDecorators.FieldIcon_Display(Editor, Editor.Project.TextureViewer, IconConfig, row, oldval, internalName, 0);
                }

                // Enum
                if (displayEnum)
                {
                    FieldDecorators.Enum_Value(Enum.Values, oldval.ToString());
                }

                // ParticleAlias
                if (showParticleEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Particles, out List<AliasEntry> particles))
                {
                    FieldDecorators.AliasEnum_Value(particles, oldval.ToString());
                }

                // SoundAlias
                if (showSoundEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Sounds, out List<AliasEntry> sounds))
                {
                    FieldDecorators.AliasEnum_Value(sounds, oldval.ToString());
                }

                // FlagAlias
                if (showFlagEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.EventFlags, out List<AliasEntry> eventFlags))
                {
                    FieldDecorators.ConditionalAliasEnum_Value(eventFlags, oldval.ToString(), row, FlagAliasEnum_ConditionalField, FlagAliasEnum_ConditionalValue);
                }

                // CutsceneAlias
                if (showCutsceneEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Cutscenes, out List<AliasEntry> cutscenes))
                {
                    FieldDecorators.AliasEnum_Value(cutscenes, oldval.ToString());
                }

                // MovieAlias
                if (showMovieEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Movies, out List<AliasEntry> movies))
                {
                    FieldDecorators.ConditionalAliasEnum_Value(movies, oldval.ToString(), row, MovieAliasEnum_ConditionalField, MovieAliasEnum_ConditionalValue);
                }

                // CharacterAlias
                if (showCharacterEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Characters, out List<AliasEntry> characters))
                {
                    FieldDecorators.AliasEnum_Value(characters, oldval.ToString(), true);
                }

                // ProjectEnum
                if (showProjectEnum)
                {
                    FieldDecorators.ProjectEnum_Value(Editor, cellMeta.ProjectEnumType, oldval.ToString());
                }

                // TileRef
                if(showTileRef)
                {
                    FieldDecorators.TileRef_Value(Editor, cellMeta.TileRef, oldval.ToString());
                }

                // Param Field Offset
                if (showParamFieldOffset)
                {
                    FieldDecorators.ParamFieldOffset_Value(Editor, activeParam, row, paramFieldIndex);
                }

                ImGui.EndGroup();

            }

            FieldDecorators.ParamReference_ContextMenu(Editor, bank, oldval, row, RefTypes);
            FieldDecorators.TextReference_ContextMenu(Editor, bank, oldval, row, FmgRef);

            // Param Reference Buttons
            if (CFG.Current.Param_ViewInMapOption)
            {
                // These are placed at the top, below the ID row
                if (imguiId == 1)
                {
                    ParamMetaReferences.ReturnPointParam(Editor, activeParam, row, internalName);
                    ParamMetaReferences.BonfireWarpParam(Editor, activeParam, row, internalName);
                    ParamMetaReferences.GameAreaParam(Editor, activeParam, row, internalName);
                    ParamMetaReferences.ItemLotParam(Editor, activeParam, row, internalName);
                }
            }

            if (CFG.Current.Param_ViewModelOption)
            {
                // These are placed at the top, below the ID row
                if (imguiId == 1)
                {
                    ParamMetaReferences.AssetGeometryParam(Editor, activeParam, row, internalName);
                    ParamMetaReferences.BuddyStoneParam(Editor, activeParam, row, internalName);
                }

                // These are placed in-line with the current field
                ParamMetaReferences.GrassTypeParam(Editor, activeParam, row, internalName);
                ParamMetaReferences.BulletParam(Editor, activeParam, row, internalName);
            }

            // Color Picker
            ParamMetaReferences.ColorPicker(Editor, activeParam, row, internalName);

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                if (!CFG.Current.Param_FieldContextMenu_Split)
                {
                    ImGui.OpenPopup("ParamRowCommonMenu");
                }
                else
                {
                    ImGui.OpenPopup("ParamRowValueMenu");
                }
            }

            if (conflict || diffVanilla)
            {
                ImGui.PopStyleColor();
            }
        }

        ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_Input_Default_Background);
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);

        if (conflict)
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_Input_Conflict_Background);
        }

        if (CFG.Current.Param_ShowVanillaColumn && ImGui.TableNextColumn())
        {
            AdditionalColumnValue(activeParam, vanillaval, propType, bank, RefTypes, FmgRef, MapFmgRef, row, Enum, IconConfig, "vanilla", cellMeta, internalName);
        }

        if (CFG.Current.Param_ShowAuxColumn)
        {
            for (var i = 0; i < auxVals.Count; i++)
            {
                if (ImGui.TableNextColumn())
                {
                    if (!conflict && diffAuxVanilla[i])
                        ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_Input_AuxVanilla_Background);

                    AdditionalColumnValue(activeParam, auxVals[i], propType, bank, RefTypes, FmgRef, MapFmgRef, row, Enum, IconConfig, i.ToString(), cellMeta, internalName);

                    if (!conflict && diffAuxVanilla[i])
                        ImGui.PopStyleColor();
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
                ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_Input_DiffCompare_Background);
            }

            AdditionalColumnValue(activeParam, compareval, propType, bank, RefTypes, FmgRef, MapFmgRef, row, Enum, IconConfig, "compRow", cellMeta, internalName);

            if (diffCompare)
            {
                ImGui.PopStyleColor();
            }
        }

        ImGui.PopStyleColor(2);

        if (ImGui.BeginPopup("ParamRowCommonMenu"))
        {
            PropertyRowNameContextMenuItems(bank, internalName, cellMeta, meta, activeParam, 
                activeParam != null, isPinned, col, selection, propType, Wiki, oldval, true);
            PropertyRowValueContextMenuItems(Editor, bank, row, cellMeta, internalName, VirtualRef, ExtRefs, oldval, ref newval,
                RefTypes, FmgRef, MapFmgRef, IconConfig, Enum, internalName);

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("ParamRowNameMenu"))
        {
            PropertyRowNameContextMenuItems(bank, internalName, cellMeta, meta, activeParam, 
                activeParam != null, isPinned, col, selection, propType, Wiki, oldval, true);

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("ParamRowValueMenu"))
        {
            PropertyRowNameContextMenuItems(bank, internalName, cellMeta, meta, activeParam, activeParam != null,
                isPinned, col, selection, propType, Wiki, oldval, false);
            PropertyRowValueContextMenuItems(Editor, bank, row, cellMeta, internalName, VirtualRef, ExtRefs, oldval, ref newval,
                RefTypes, FmgRef, MapFmgRef, IconConfig, Enum, internalName);

            ImGui.EndPopup();
        }

        // Context Menu Shortcuts
        if (FieldDecorators.ParamReference_ShortcutItems(Editor, bank, cellMeta, oldval, ref newval, RefTypes, row, FmgRef, MapFmgRef, IconConfig, Enum, ContextActionManager))
        {
            ParamFieldInput.SetLastPropertyManual(newval);
        }

        var committed = ParamFieldInput.UpdateProperty(Editor, ContextActionManager,
            nullableCell != null ? nullableCell : row, proprow, oldval);

        if (committed)
        {
            if (Project.TextureViewer != null)
            {
                Project.TextureViewer.ImagePreview.ClearIcons();
            }

            Editor.Project.ParamData.PrimaryBank.RefreshParamRowDiffs(Editor, row, activeParam);
        }

        ImGui.PopID();
        imguiId++;
    }

    private void AdditionalColumnValue(string activeParam, object colVal, Type propType, ParamBank bank, List<ParamRef> RefTypes,
        List<FMGRef> FmgRef, List<FMGRef> MapFmgRef, Param.Row context, ParamEnum Enum, IconConfig iconConfig, string imguiSuffix, ParamFieldMeta cellMeta, string fieldName)
    {
        if (colVal == null)
        {
            ImGui.TextUnformatted("");
        }
        else
        {
            string value;

            bool showParticleEnum = false;
            bool showSoundEnum = false;
            bool showFlagEnum = false;
            bool showCutsceneEnum = false;
            bool showMovieEnum = false;
            bool showCharacterEnum = false;
            bool showProjectEnum = false;
            bool showTileRef = false;

            var FlagAliasEnum_ConditionalField = cellMeta?.FlagAliasEnum_ConditionalField;
            var FlagAliasEnum_ConditionalValue = cellMeta?.FlagAliasEnum_ConditionalValue;

            var MovieAliasEnum_ConditionalField = cellMeta?.MovieAliasEnum_ConditionalField;
            var MovieAliasEnum_ConditionalValue = cellMeta?.MovieAliasEnum_ConditionalValue;

            var showParamFieldOffset = false;
            var paramFieldIndex = "";

            if (cellMeta != null)
            {
                showParticleEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowParticleEnumList;
                showSoundEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowSoundEnumList;
                showFlagEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowFlagEnumList;
                showCutsceneEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowCutsceneEnumList;
                showMovieEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowMovieEnumList;
                showCharacterEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowCharacterEnumList;
                showProjectEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowProjectEnumList;

                showParamFieldOffset = cellMeta.ShowParamFieldOffset;
                paramFieldIndex = cellMeta.ParamFieldOffsetIndex;

                if(cellMeta.TileRef != null)
                {
                    showTileRef = true;
                }
            }

            if (propType == typeof(byte[]))
            {
                value = ParamUtils.Dummy8Write((byte[])colVal);
            }
            else
            {
                value = colVal.ToString();
            }

            ImGui.InputText("##colval" + imguiSuffix, ref value, 256, ImGuiInputTextFlags.ReadOnly);

            if (CFG.Current.Param_HideReferenceRows == false && RefTypes != null)
            {
                FieldDecorators.ParamReference_Value(Editor, bank, RefTypes, context, colVal);
            }

            if (CFG.Current.Param_HideReferenceRows == false && FmgRef != null)
            {
                FieldDecorators.TextReference_Value(Editor, FmgRef, context, colVal);
            }

            if (CFG.Current.Param_HideReferenceRows == false && MapFmgRef != null)
            {
                FieldDecorators.TextReference_Value(Editor, MapFmgRef, context, colVal);
            }

            if (CFG.Current.Param_HideReferenceRows == false && iconConfig != null)
            {
                FieldDecorators.FieldIcon_Display(Editor, Editor.Project.TextureViewer, iconConfig, context, colVal, fieldName, 1);
            }

            if (CFG.Current.Param_HideEnums == false && Enum != null)
            {
                FieldDecorators.Enum_Value(Enum.Values, colVal.ToString());
            }

            // ParticleAlias
            if (showParticleEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Particles, out List<AliasEntry> particles))
            {
                FieldDecorators.AliasEnum_Value(particles, colVal.ToString());
            }

            // SoundAlias
            if (showSoundEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Sounds, out List<AliasEntry> sounds))
            {
                FieldDecorators.AliasEnum_Value(sounds, colVal.ToString());
            }

            // FlagAlias
            if (showFlagEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.EventFlags, out List<AliasEntry> eventFlags))
            {
                FieldDecorators.ConditionalAliasEnum_Value(eventFlags, colVal.ToString(), context, FlagAliasEnum_ConditionalField, FlagAliasEnum_ConditionalValue);
            }

            // CutsceneAlias
            if (showCutsceneEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Cutscenes, out List<AliasEntry> cutscenes))
            {
                FieldDecorators.AliasEnum_Value(cutscenes, colVal.ToString());
            }

            // MovieAlias
            if (showMovieEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Movies, out List<AliasEntry> movies))
            {
                FieldDecorators.ConditionalAliasEnum_Value(movies, colVal.ToString(), context, MovieAliasEnum_ConditionalField, MovieAliasEnum_ConditionalValue);
            }

            // CharacterAlias
            if (showCharacterEnum && Editor.Project.CommonData.Aliases.TryGetValue(ProjectAliasType.Characters, out List<AliasEntry> characters))
            {
                FieldDecorators.AliasEnum_Value(characters, colVal.ToString(), true);
            }

            // ProjectEnum
            if (showProjectEnum)
            {
                FieldDecorators.ProjectEnum_Value(Editor, cellMeta.ProjectEnumType, colVal.ToString());
            }

            // TileRef
            if (showTileRef)
            {
                FieldDecorators.TileRef_Value(Editor, cellMeta.TileRef, colVal.ToString());
            }

            // Param Field Offset
            if (showParamFieldOffset)
            {
                FieldDecorators.ParamFieldOffset_Value(Editor, activeParam, context, paramFieldIndex);
            }
        }
    }

    private static string OffsetTextOfColumn(Param.Column col)
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

    private static void PropertyRowName(ParamEditorScreen editor, string fieldOffset, ref string internalName, ParamFieldMeta cellMeta)
    {
        var altName = cellMeta?.AltName;

        if (cellMeta != null && editor.EditorMode)
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
                if (CFG.Current.Param_MakeMetaNamesPrimary)
                {
                    printedName = altName;
                    if (CFG.Current.Param_ShowSecondaryNames)
                        printedName = $"{printedName} ({internalName})";
                }
                else if (CFG.Current.Param_ShowSecondaryNames)
                {
                    printedName = $"{printedName} ({altName})";
                }
            }

            if (fieldOffset != null && CFG.Current.Param_ShowFieldOffsets)
            {
                printedName = $"{fieldOffset} {printedName}";
            }

            ImGui.TextUnformatted(printedName);
        }
    }

    private void PropertyRowNameContextMenuItems(ParamBank bank, string internalName, ParamFieldMeta cellMeta,
        ParamMeta meta, string activeParam, bool showPinOptions, bool isPinned, Param.Column col,
        ParamSelection selection, Type propType, string Wiki, dynamic oldval, bool isNameMenu)
    {
        var scale = DPI.UIScale();
        var altName = cellMeta?.AltName;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 10f) * scale);

        // Copy Name
        if (ImGui.MenuItem("Copy Internal Name"))
        {
            PlatformUtils.Instance.SetClipboardText(internalName);
        }

        // Field Information
        if (col != null)
        {
            if (CFG.Current.Param_FieldContextMenu_PropertyInfo)
            {
                ParamEditorDecorations.ImGui_DisplayPropertyInfo(propType, internalName, isNameMenu, !isNameMenu, altName,
                    col.Def.ArrayLength,
                    col.Def.BitSize);
            }

            if (isNameMenu && CFG.Current.Param_FieldContextMenu_Description)
            {
                if (Wiki != null)
                {
                    ImGui.TextColored(new Vector4(.4f, .7f, 1f, 1f), $"{Wiki}");
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
            // Headers
            if (CFG.Current.Param_FieldContextMenu_Name)
            {
                ImGui.TextColored(new Vector4(1.0f, 0.7f, 0.4f, 1.0f), Utils.ImGuiEscape(internalName, "", true));
            }
        }

        if (isNameMenu && (CFG.Current.Param_FieldContextMenu_Name || CFG.Current.Param_FieldContextMenu_Description || CFG.Current.Param_FieldContextMenu_PropertyInfo))
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
            Editor.MassEditHandler.ConstructCommandFromField(internalName);
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
        if (showPinOptions && CFG.Current.Param_FieldContextMenu_PinOptions)
        {
            if (ImGui.MenuItem(isPinned ? "Unpin " : "Pin " + internalName))
            {
                if (!Editor.Project.PinnedFields.ContainsKey(activeParam))
                {
                    Editor.Project.PinnedFields.Add(activeParam, new List<string>());
                }

                List<string> pinned = Editor.Project.PinnedFields[activeParam];

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
                ParamEditorDecorations.PinListReorderOptions(Editor.Project.PinnedFields[activeParam],
                    internalName);
            }

            if (ImGui.Selectable("Unpin all"))
            {
                Editor.Project.PinnedFields.Clear();
            }

            ImGui.Separator();
        }

        // Compare
        if (CFG.Current.Param_FieldContextMenu_CompareOptions)
        {
            if (col != null && ImGui.MenuItem("Compare field"))
            {
                selection.SetCompareCol(col);
            }
        }

        // Value Distribution
        if (CFG.Current.Param_FieldContextMenu_ValueDistribution)
        {
            if (ImGui.Selectable("View value distribution in selected rows..."))
            {
                EditorCommandQueue.AddCommand($@"param/menu/distributionPopup/{internalName}");
            }
        }

        ImGui.PopStyleVar();
    }

    private void PropertyRowValueContextMenuItems(ParamEditorScreen editor, ParamBank bank, Param.Row row, ParamFieldMeta cellMeta, string internalName,
        string VirtualRef, List<ExtRef> ExtRefs, dynamic oldval, ref object newval, List<ParamRef> RefTypes,
        List<FMGRef> FmgRef, List<FMGRef> MapFmgRef, IconConfig iconConfig, ParamEnum Enum, string fieldName)
    {
        if (CFG.Current.Param_FieldContextMenu_References)
        {
            if (RefTypes != null || FmgRef != null || MapFmgRef != null || iconConfig != null || Enum != null || cellMeta != null)
            {
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Ref_Text);

                if (FieldDecorators.Decorator_ContextMenuItems(Editor, bank, cellMeta, oldval, ref newval, RefTypes, row, FmgRef, MapFmgRef, iconConfig, Enum, ContextActionManager))
                {
                    ParamFieldInput.SetLastPropertyManual(newval);
                }

                ImGui.PopStyleColor();
            }
        }

        if (CFG.Current.Param_FieldContextMenu_ReferenceSearch)
        {
            if (VirtualRef != null || ExtRefs != null)
            {
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_VirtualRef_Text);
                FieldDecorators.VirtualParamReference_ContextMenu(Editor, bank, VirtualRef, oldval, row, internalName);
                FieldDecorators.ExternalReference_ContextMenu(Editor, bank, VirtualRef, oldval, row, internalName, ExtRefs);
                ImGui.PopStyleColor();
            }
        }

        if (CFG.Current.Param_FieldContextMenu_MassEdit)
        {
            ImGui.Separator();

            if (!CFG.Current.Param_FieldContextMenu_FullMassEdit)
            {
                if (ImGui.Selectable("Mass edit"))
                {
                    EditorCommandQueue.AddCommand(
                        $@"param/menu/massEditRegex/selection: {Regex.Escape(internalName)}: ");
                }

                if (ImGui.Selectable("Reset to vanilla"))
                {
                    Editor.MassEditHandler.ApplyMassEdit($"selection && !added: {Regex.Escape(internalName)}: = vanilla;");
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

                    if (editor.MassEditHandler.AutoFill != null)
                    {
                        var res = editor.MassEditHandler.AutoFill.MassEditOpAutoFill();
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
