﻿using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Editors.ParamEditor.Decorators;
using StudioCore.Editors.ParamEditor.MassEdit;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ParamEditor;

public class ParamRowEditor
{
    public ParamEditorScreen Editor;

    private Dictionary<string, PropertyInfo[]> _propCache = new();
    public ActionManager ContextActionManager;

    public ParamRowEditor(ActionManager manager, ParamEditorScreen paramEditorScreen)
    {
        ContextActionManager = manager;
        Editor = paramEditorScreen;
    }

    private static void PropEditorParamRow_Header(ParamEditorScreen editor, bool isActiveView, ref string propSearchString)
    {
        if (propSearchString != null)
        {
            if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SearchField))
            {
                ImGui.SetKeyboardFocusHere();
            }

            // Autofill
            ImGui.AlignTextToFramePadding();
            var resAutoCol = AutoFill.ColumnSearchBarAutoFill(editor);
            if (resAutoCol != null)
            {
                propSearchString = resAutoCol;
                UICache.ClearCaches();
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
        }
    }

    private static string GetColorValue(Param.Row row, List<(PseudoColumn, Param.Column)> cols, string fieldName)
    {
        var matchVal = "";

        var matches = cols?.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == fieldName).ToList();
        var match = matches.FirstOrDefault();
        matchVal = "";
        if (match.Item2 != null)
        {
            matchVal = row.Get(match).ToString();
        }

        return (string)matchVal;
    }

    private void PropEditorParamRow_RowFields(ParamBank bank, Param.Row row, ParamMeta? meta, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, ref int imguiId, ParamEditorSelectionState selection, 
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

    private void PropEditorParamRow_PinnedFields(List<string> pinList, ParamBank bank, ParamMeta? meta,
        Param.Row row, Param.Row vrow, List<(string, Param.Row)> auxRows, Param.Row crow, 
        List<(PseudoColumn, Param.Column)> cols, List<(PseudoColumn, Param.Column)> vcols, 
        List<List<(PseudoColumn, Param.Column)>> auxCols, ref int imguiId, string activeParam, 
        ParamEditorSelectionState selection, ref int index)
    {
        var pinnedFields = new List<string>(pinList);
        foreach (var field in pinnedFields)
        {
            var matches =
                cols.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();
            var vmatches =
                vcols.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();
            var auxMatches = auxCols.Select((aux, i) =>
                aux.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList()).ToList();
            for (var i = 0; i < matches.Count; i++)
            {
                PropEditorPropCellRow(bank,
                    meta,
                    row,
                    crow,
                    matches[i],
                    vrow,
                    vmatches.Count > i ? vmatches[i] : (PseudoColumn.None, null),
                    auxRows,
                    auxMatches.Select((x, j) => x.Count > i ? x[i] : (PseudoColumn.None, null)).ToList(),
                    OffsetTextOfColumn(matches[i].Item2),
                    ref imguiId, activeParam, true, selection);
                index++;
            }
        }
    }

    private void PropEditorParamRow_MainFields(ParamMeta? meta, ParamBank bank, Param.Row row, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, List<(PseudoColumn, Param.Column)> cols,
        List<(PseudoColumn, Param.Column)> vcols, List<List<(PseudoColumn, Param.Column)>> auxCols, ref int imguiId,
        string activeParam, ParamEditorSelectionState selection, List<string> pinnedFields)
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

                if (!CFG.Current.Param_PinnedRowsStayVisible)
                {
                    if (pinnedFields?.Count > 0)
                    {
                        PropEditorParamRow_PinnedFields(pinnedFields, bank, meta, row, vrow, auxRows, crow, cols, vcols, 
                            auxCols, ref imguiId, activeParam, selection, ref index);
                        EditorDecorations.ImguiTableSeparator();
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
                    EditorDecorations.ImguiTableSeparator();
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
                    vmatches.Count > i ? vmatches[i] : (PseudoColumn.None, null),
                    auxRows,
                    auxMatches.Select((x, j) => x.Count > i ? x[i] : (PseudoColumn.None, null)).ToList(),
                    OffsetTextOfColumn(matches[i].Item2),
                    ref imguiId, activeParam, false, selection);
                index++;
                lastRowExists = true;
            }
        }
    }

    public void PropEditorParamRow(ParamBank bank, Param.Row row, Param.Row vrow, List<(string, Param.Row)> auxRows,
        Param.Row crow, ref string propSearchString, string activeParam, bool isActiveView,
        ParamEditorSelectionState selection)
    {
        var meta = Editor.Project.ParamData.GetParamMeta(row.Def);
        var imguiId = 0;
        var showParamCompare = auxRows.Count > 0;
        var showRowCompare = crow != null;

        PropEditorParamRow_Header(Editor, isActiveView, ref propSearchString);

        //ImGui.BeginChild("Param Fields");
        var columnCount = 2;

        if (CFG.Current.Param_ShowVanillaParams)
        {
            columnCount++;
        }

        if (showRowCompare)
        {
            columnCount++;
        }

        if (showParamCompare)
        {
            columnCount += auxRows.Count;
        }

        if (EditorDecorations.ImGuiTableStdColumns("ParamFieldsT", columnCount, false))
        {
            List<string> pinnedFields =
                Editor.Project.PinnedFields.GetValueOrDefault(activeParam, null);

            if (CFG.Current.Param_PinnedRowsStayVisible)
            {
                ImGui.TableSetupScrollFreeze(columnCount, (showParamCompare ? 3 : 2) + (1 + pinnedFields?.Count ?? 0));
            }

            if (showParamCompare)
            {
                ImGui.TableNextColumn();
                if (ImGui.TableNextColumn())
                {
                    ImGui.Text("Current");
                }

                if (CFG.Current.Param_ShowVanillaParams && ImGui.TableNextColumn())
                {
                    ImGui.Text("Vanilla");
                }

                foreach ((var name, Param.Row r) in auxRows)
                {
                    if (ImGui.TableNextColumn())
                    {
                        ImGui.Text(name);
                    }
                }
            }

            EditorDecorations.ImguiTableSeparator();

            PropEditorParamRow_RowFields(bank, row, meta, vrow, auxRows, crow, ref imguiId, selection, activeParam);

            var search = propSearchString;
            List<(PseudoColumn, Param.Column)> cols = UICache.GetCached(Editor, row, "fieldFilter",
                () => CellSearchEngine.Create(Editor).Search((activeParam, row), search, true, true));

            List<(PseudoColumn, Param.Column)> vcols = UICache.GetCached(Editor, vrow, "vFieldFilter",
                () => cols.Select((x, i) => x.GetAs(Editor.Project.ParamData.VanillaBank.GetParamFromName(activeParam))).ToList());

            List<List<(PseudoColumn, Param.Column)>> auxCols = UICache.GetCached(Editor, auxRows,
                "auxFieldFilter", () => auxRows.Select((r, i) => cols.Select((c, j) => c.GetAs(Editor.Project.ParamData.AuxBanks[r.Item1].GetParamFromName(activeParam))).ToList()).ToList());

            if (CFG.Current.Param_PinnedRowsStayVisible)
            {
                if (pinnedFields?.Count > 0)
                {
                    int i = 0;
                    PropEditorParamRow_PinnedFields(pinnedFields, bank, meta, row, vrow, auxRows, crow, cols, vcols, 
                        auxCols, ref imguiId, activeParam, selection, ref i);
                    EditorDecorations.ImguiTableSeparator();
                }
            }

            if (!CFG.Current.Param_PinGroups_ShowOnlyPinnedFields)
            {
                PropEditorParamRow_MainFields(meta, bank, row, vrow, auxRows, crow, cols, vcols, auxCols, ref imguiId,
                    activeParam, selection, pinnedFields);
            }

            if (CFG.Current.Param_ShowGraphVisualisation)
            {
                if (meta.CalcCorrectDef != null || meta.SoulCostDef != null)
                {
                    FieldDecorators.DrawCalcCorrectGraph(Editor, meta, row);
                }
            }

            ImGui.EndTable();
        }

    }

    // Many parameter options, which may be simplified.
    private void PropEditorPropInfoRow(ParamBank bank, Param.Row row, ParamMeta? meta, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, PropertyInfo prop, string visualName, ref int imguiId,
        ParamEditorSelectionState selection, string activeParam)
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

    private void PropEditorPropCellRow(ParamBank bank, ParamMeta? meta, Param.Row row, Param.Row crow,
        (PseudoColumn, Param.Column) col, Param.Row vrow, (PseudoColumn, Param.Column) vcol,
        List<(string, Param.Row)> auxRows, List<(PseudoColumn, Param.Column)> auxCols, string fieldOffset,
        ref int imguiId, string activeParam, bool isPinned, ParamEditorSelectionState selection)
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
        Type propType, PropertyInfo proprow, Param.Cell? nullableCell, Param.Row row, ParamMeta? meta, string activeParam,
        bool isPinned, Param.Column col, ParamEditorSelectionState selection)
    {
        var Wiki = cellMeta?.Wiki;

        List<ParamRef> RefTypes = cellMeta?.RefTypes;
        List<FMGRef> FmgRef = cellMeta?.FmgRef;
        List<ExtRef> ExtRefs = cellMeta?.ExtRefs;
        List<TexRef> TextureRef = cellMeta?.TextureRef;
        List<FMGRef> MapFmgRef = cellMeta?.MapFmgRef;

        var VirtualRef = cellMeta?.VirtualRef;

        ParamEnum Enum = cellMeta?.EnumType;
        var IsBool = cellMeta?.IsBool ?? false;
        var IsInvertedPercentage = cellMeta?.IsInvertedPercentage ?? false;
        var IsPaddingField = cellMeta?.IsPaddingField ?? false;
        var IsObsoleteField = cellMeta?.IsObsoleteField ?? false;
        var AddSeparator = cellMeta?.AddSeparatorNextLine ?? false;

        var displayRefTypes = !CFG.Current.Param_HideReferenceRows && RefTypes != null;
        var displayFmgRef = !CFG.Current.Param_HideReferenceRows && FmgRef != null;
        var displayTextureRef = !CFG.Current.Param_HideReferenceRows && TextureRef != null;
        var displayEnum = !CFG.Current.Param_HideEnums && Enum != null;
        var displayMapFmgRef = !CFG.Current.Param_HideReferenceRows && MapFmgRef != null;

        bool showParticleEnum = false;
        bool showSoundEnum = false;
        bool showFlagEnum = false;
        bool showCutsceneEnum = false;
        bool showMovieEnum = false;
        bool showProjectEnum = false;

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
            showProjectEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowProjectEnumList;

            showParamFieldOffset = cellMeta.ShowParamFieldOffset;
            paramFieldIndex = cellMeta.ParamFieldOffsetIndex;
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

                    if (EditorDecorations.HelpIcon(internalName, ref helpIconText, true))
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

            if (displayRefTypes || displayFmgRef || displayTextureRef || displayEnum || showParticleEnum || showSoundEnum || showFlagEnum || showCutsceneEnum || showMovieEnum || showProjectEnum || showParamFieldOffset || displayMapFmgRef)
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

                // Texture Ref
                if (displayTextureRef)
                {
                    FieldDecorators.TextureReference_Title(TextureRef, row);
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

                // Project Enum
                if (showProjectEnum)
                {
                    FieldDecorators.ProjectEnum_Title(Editor, cellMeta.ProjectEnumType);
                }

                // Param Field Offset
                if(showParamFieldOffset)
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
        var isRef = CFG.Current.Param_HideReferenceRows == false && (RefTypes != null || FmgRef != null || TextureRef != null) || CFG.Current.Param_HideEnums == false && Enum != null || VirtualRef != null || ExtRefs != null || CFG.Current.Param_HideEnums == false && showParticleEnum;

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
            ParamEditorCommon.PropertyField(propType, oldval, ref newval, IsBool, IsInvertedPercentage);

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

            if (displayRefTypes || displayFmgRef || displayTextureRef || displayEnum || showParticleEnum || showSoundEnum || showFlagEnum || showCutsceneEnum || showMovieEnum || showProjectEnum || showParamFieldOffset || displayMapFmgRef)
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

                // TextureRef
                if (displayTextureRef)
                {
                    FieldDecorators.TextureReference_Value(Editor, Editor.Project.TextureViewer, TextureRef, row, oldval);
                }

                // Enum
                if (displayEnum)
                {
                    FieldDecorators.Enum_Value(Enum.Values, oldval.ToString());
                }

                // ParticleAlias
                if (showParticleEnum)
                {
                    FieldDecorators.AliasEnum_Value(Editor.Project.Aliases.Particles, oldval.ToString());
                }

                // SoundAlias
                if (showSoundEnum)
                {
                    FieldDecorators.AliasEnum_Value(Editor.Project.Aliases.Sounds, oldval.ToString());
                }

                // FlagAlias
                if (showFlagEnum)
                {
                    FieldDecorators.ConditionalAliasEnum_Value(Editor.Project.Aliases.EventFlags, oldval.ToString(), row, FlagAliasEnum_ConditionalField, FlagAliasEnum_ConditionalValue);
                }

                // CutsceneAlias
                if (showCutsceneEnum)
                {
                    FieldDecorators.AliasEnum_Value(Editor.Project.Aliases.Cutscenes, oldval.ToString());
                }

                // MovieAlias
                if (showMovieEnum)
                {
                    FieldDecorators.ConditionalAliasEnum_Value(Editor.Project.Aliases.Movies, oldval.ToString(), row, MovieAliasEnum_ConditionalField, MovieAliasEnum_ConditionalValue);
                }

                // ProjectEnum
                if (showProjectEnum)
                {
                    FieldDecorators.ProjectEnum_Value(Editor, cellMeta.ProjectEnumType, oldval.ToString());
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
            FieldDecorators.TextureReference_ContextMenu(Editor, bank, oldval, row, TextureRef);

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

        if (CFG.Current.Param_ShowVanillaParams && ImGui.TableNextColumn())
        {
            AdditionalColumnValue(activeParam, vanillaval, propType, bank, RefTypes, FmgRef, MapFmgRef, row, Enum, TextureRef, "vanilla", cellMeta);
        }

        for (var i = 0; i < auxVals.Count; i++)
        {
            if (ImGui.TableNextColumn())
            {
                if (!conflict && diffAuxVanilla[i])
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, UI.Current.ImGui_Input_AuxVanilla_Background);

                AdditionalColumnValue(activeParam, auxVals[i], propType, bank, RefTypes, FmgRef, MapFmgRef, row, Enum, TextureRef, i.ToString(), cellMeta);
                if (!conflict && diffAuxVanilla[i])
                    ImGui.PopStyleColor();
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

            AdditionalColumnValue(activeParam, compareval, propType, bank, RefTypes, FmgRef, MapFmgRef, row, Enum, TextureRef, "compRow", cellMeta);

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
            PropertyRowValueContextMenuItems(bank, row, cellMeta, internalName, VirtualRef, ExtRefs, oldval, ref newval,
                RefTypes, FmgRef, MapFmgRef, TextureRef, Enum);

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
            PropertyRowValueContextMenuItems(bank, row, cellMeta, internalName, VirtualRef, ExtRefs, oldval, ref newval,
                RefTypes, FmgRef, MapFmgRef, TextureRef, Enum);

            ImGui.EndPopup();
        }

        // Context Menu Shortcuts
        if (FieldDecorators.ParamReference_ShortcutItems(Editor, bank, cellMeta, oldval, ref newval, RefTypes, row, FmgRef, MapFmgRef, TextureRef, Enum, ContextActionManager))
        {
            ParamEditorCommon.SetLastPropertyManual(newval);
        }

        var committed = ParamEditorCommon.UpdateProperty(ContextActionManager,
            nullableCell != null ? nullableCell : row, proprow, oldval);

        if (committed)
            Editor.Project.ParamData.PrimaryBank.RefreshParamRowDiffs(Editor, row, activeParam);

        ImGui.PopID();
        imguiId++;
    }

    private void AdditionalColumnValue(string activeParam, object colVal, Type propType, ParamBank bank, List<ParamRef> RefTypes,
        List<FMGRef> FmgRef, List<FMGRef> MapFmgRef, Param.Row context, ParamEnum Enum, List<TexRef> TextureRef, string imguiSuffix, ParamFieldMeta cellMeta)
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
            bool showProjectEnum = false;

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
                showProjectEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowProjectEnumList;

                showParamFieldOffset = cellMeta.ShowParamFieldOffset;
                paramFieldIndex = cellMeta.ParamFieldOffsetIndex;
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

            if (CFG.Current.Param_HideReferenceRows == false && TextureRef != null)
            {
                FieldDecorators.TextureReference_Value(Editor, Editor.Project.TextureViewer, TextureRef, context, colVal);
            }

            if (CFG.Current.Param_HideEnums == false && Enum != null)
            {
                FieldDecorators.Enum_Value(Enum.Values, colVal.ToString());
            }

            // ParticleAlias
            if (showParticleEnum)
            {
                FieldDecorators.AliasEnum_Value(Editor.Project.Aliases.Particles, colVal.ToString());
            }

            // SoundAlias
            if (showSoundEnum)
            {
                FieldDecorators.AliasEnum_Value(Editor.Project.Aliases.Sounds, colVal.ToString());
            }

            // FlagAlias
            if (showFlagEnum)
            {
                FieldDecorators.ConditionalAliasEnum_Value(Editor.Project.Aliases.EventFlags, colVal.ToString(), context, FlagAliasEnum_ConditionalField, FlagAliasEnum_ConditionalValue);
            }

            // CutsceneAlias
            if (showCutsceneEnum)
            {
                FieldDecorators.AliasEnum_Value(Editor.Project.Aliases.Cutscenes, colVal.ToString());
            }

            // MovieAlias
            if (showMovieEnum)
            {
                FieldDecorators.ConditionalAliasEnum_Value(Editor.Project.Aliases.Movies, colVal.ToString(), context, MovieAliasEnum_ConditionalField, MovieAliasEnum_ConditionalValue);
            }

            // ProjectEnum
            if (showProjectEnum)
            {
                FieldDecorators.ProjectEnum_Value(Editor, cellMeta.ProjectEnumType, colVal.ToString());
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
        ParamMeta? meta, string activeParam, bool showPinOptions, bool isPinned, Param.Column col,
        ParamEditorSelectionState selection, Type propType, string Wiki, dynamic oldval, bool isNameMenu)
    {
        var scale = DPI.GetUIScale();
        var altName = cellMeta?.AltName;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 10f) * scale);

        // Field Information
        if (col != null)
        {
            if (CFG.Current.Param_FieldContextMenu_PropertyInfo)
            {
                EditorDecorations.ImGui_DisplayPropertyInfo(propType, internalName, isNameMenu, !isNameMenu, altName,
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
            var propertyName = internalName.Replace(" ", "\\s");
            string currInput = Editor.MassEditHandler._currentMEditRegexInput;

            if(currInput == "")
            {
                // Add selection section if input is empty
                Editor.MassEditHandler._currentMEditRegexInput = $"selection: {propertyName}: ";
            }
            else
            {
                // Otherwise just add the property name
                currInput = $"{currInput}{propertyName}";
                Editor.MassEditHandler._currentMEditRegexInput = currInput;
            }
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
                EditorDecorations.PinListReorderOptions(Editor.Project.PinnedFields[activeParam],
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

    private void PropertyRowValueContextMenuItems(ParamBank bank, Param.Row row, ParamFieldMeta cellMeta, string internalName,
        string VirtualRef, List<ExtRef> ExtRefs, dynamic oldval, ref object newval, List<ParamRef> RefTypes,
        List<FMGRef> FmgRef, List<FMGRef> MapFmgRef, List<TexRef> TextureRef, ParamEnum Enum)
    {
        if (CFG.Current.Param_FieldContextMenu_References)
        {
            if (RefTypes != null || FmgRef != null || MapFmgRef != null || TextureRef != null || Enum != null || cellMeta != null)
            {
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Ref_Text);

                if (FieldDecorators.Decorator_ContextMenuItems(Editor, bank, cellMeta, oldval, ref newval, RefTypes, row, FmgRef, MapFmgRef, TextureRef, Enum, ContextActionManager))
                {
                    ParamEditorCommon.SetLastPropertyManual(newval);
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
                    MassParamEditRegex.PerformMassEdit(Editor.Project.ParamData.PrimaryBank,
                        $"selection && !added: {Regex.Escape(internalName)}: = vanilla;",
                         Editor._activeView._selection);
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
                    var res = AutoFill.MassEditOpAutoFill();
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
