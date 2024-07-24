﻿using Andre.Formats;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Toolbar;
using StudioCore.Interface;
using StudioCore.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ParamEditor;

public class ParamRowEditor
{
    private readonly ParamEditorScreen _paramEditor;

    private Dictionary<string, PropertyInfo[]> _propCache = new();
    public ActionManager ContextActionManager;

    public ParamRowEditor(ActionManager manager, ParamEditorScreen paramEditorScreen)
    {
        ContextActionManager = manager;
        _paramEditor = paramEditorScreen;
    }

    private static void PropEditorParamRow_Header(bool isActiveView, ref string propSearchString)
    {
        if (propSearchString != null)
        {
            if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.Param_SearchField))
            {
                ImGui.SetKeyboardFocusHere();
            }

            ImGui.InputText($"Search <{KeyBindings.Current.Param_SearchField.HintText}>", ref propSearchString,
                255);
            if (ImGui.IsItemEdited())
            {
                UICache.ClearCaches();
            }

            var resAutoCol = AutoFill.ColumnSearchBarAutoFill();
            if (resAutoCol != null)
            {
                propSearchString = resAutoCol;
                UICache.ClearCaches();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }
    }

    private static void PropEditorParamRow_ColorEditors(bool isActiveView, ParamMetaData meta, Param.Row row, List<(PseudoColumn, Param.Column)> cols, int index)
    {
        if(meta.colorEditors.Count > 0)
        {
            foreach (var colorEditor in meta.colorEditors)
            {
                var name = colorEditor.Value.name;

                // RED
                var rInfo = colorEditor.Value.values.ElementAt(0);
                float rVal = 0.0f;

                if (rInfo.Key != null)
                {
                    var rField = rInfo.Key;
                    var rType = rInfo.Value;

                    float.TryParse(GetColorValue(row, cols, rField), out rVal);
                    if (rType != "f32") // If not a float, then the value will need to be divided to work with ColorEdit4
                    {
                        rVal = (rVal / 255);
                    }
                }

                // GREEN
                var gInfo = colorEditor.Value.values.ElementAt(1);
                float gVal = 0.0f;

                if (gInfo.Key != null)
                {
                    var gField = gInfo.Key;
                    var gType = gInfo.Value;

                    float.TryParse(GetColorValue(row, cols, gField), out gVal);
                    if (gType != "f32")
                    {
                        gVal = (gVal / 255);
                    }
                }

                // BLUE
                var bInfo = colorEditor.Value.values.ElementAt(2);
                float bVal = 0.0f;

                if(bInfo.Key != null)
                {
                    var bField = bInfo.Key;
                    var bType = bInfo.Value;

                    float.TryParse(GetColorValue(row, cols, bField), out bVal);
                    if (bType != "f32")
                    {
                        bVal = (bVal / 255);
                    }
                }

                // ALPHA
                var aInfo = colorEditor.Value.values.ElementAt(3);
                float aVal = 0.0f;

                if (aInfo.Key != null)
                {
                    var aField = aInfo.Key;
                    var aType = aInfo.Value;

                    if (aField != "null")
                    {
                        float.TryParse(GetColorValue(row, cols, aField), out aVal);
                    }
                    if (aType != "f32")
                    {
                        aVal = (aVal / 255);
                    }
                }

                var color = new Vector4(rVal, gVal, bVal, aVal);

                var flags = ImGuiColorEditFlags.NoPicker | ImGuiColorEditFlags.NoInputs;

                ImGui.ColorEdit4($"{name}##ColorEdit_{name}{index}", ref color, flags);
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

    private void PropEditorParamRow_RowFields(ParamBank bank, Param.Row row, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, ref int imguiId, ParamEditorSelectionState selection, string activeParam)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        PropertyInfo nameProp = row.GetType().GetProperty("Name");
        PropertyInfo idProp = row.GetType().GetProperty("ID");
        PropEditorPropInfoRow(bank, row, vrow, auxRows, crow, nameProp, "Name", ref imguiId, selection, activeParam);
        PropEditorPropInfoRow(bank, row, vrow, auxRows, crow, idProp, "ID", ref imguiId, selection, activeParam);
        ImGui.PopStyleColor();
        ImGui.Spacing();
    }

    private void PropEditorParamRow_PinnedFields(List<string> pinList, ParamBank bank, Param.Row row,
        Param.Row vrow, List<(string, Param.Row)> auxRows, Param.Row crow, List<(PseudoColumn, Param.Column)> cols,
        List<(PseudoColumn, Param.Column)> vcols, List<List<(PseudoColumn, Param.Column)>> auxCols, ref int imguiId,
        string activeParam, ParamEditorSelectionState selection)
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
                    row,
                    crow,
                    matches[i],
                    vrow,
                    vmatches.Count > i ? vmatches[i] : (PseudoColumn.None, null),
                    auxRows,
                    auxMatches.Select((x, j) => x.Count > i ? x[i] : (PseudoColumn.None, null)).ToList(),
                    OffsetTextOfColumn(matches[i].Item2),
                    ref imguiId, activeParam, true, selection);
            }
        }
    }

    private void PropEditorParamRow_MainFields(ParamMetaData meta, ParamBank bank, Param.Row row, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, List<(PseudoColumn, Param.Column)> cols,
        List<(PseudoColumn, Param.Column)> vcols, List<List<(PseudoColumn, Param.Column)>> auxCols, ref int imguiId,
        string activeParam, ParamEditorSelectionState selection, List<string> pinnedFields)
    {
        List<string> fieldOrder = meta != null && meta.AlternateOrder != null && CFG.Current.Param_AllowFieldReorder
            ? meta.AlternateOrder
            : new List<string>();

        foreach (PARAMDEF.Field field in row.Def.Fields)
        {
            if (!fieldOrder.Contains(field.InternalName))
            {
                fieldOrder.Add(field.InternalName);
            }
        }

        var firstRow = true;
        var lastRowExists = false;
        foreach (var field in fieldOrder)
        {
            if(firstRow)
            {
                firstRow = false;

                if (!CFG.Current.Param_PinnedRowsStayVisible)
                {
                    if (pinnedFields?.Count > 0)
                    {
                        PropEditorParamRow_PinnedFields(pinnedFields, bank, row, vrow, auxRows, crow, cols, vcols, auxCols,
                            ref imguiId, activeParam, selection);
                        EditorDecorations.ImguiTableSeparator();
                    }
                }
            }

            if (field.Equals("-") && lastRowExists)
            {
                EditorDecorations.ImguiTableSeparator();
                lastRowExists = false;
                continue;
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
                    row,
                    crow,
                    matches[i],
                    vrow,
                    vmatches.Count > i ? vmatches[i] : (PseudoColumn.None, null),
                    auxRows,
                    auxMatches.Select((x, j) => x.Count > i ? x[i] : (PseudoColumn.None, null)).ToList(),
                    OffsetTextOfColumn(matches[i].Item2),
                    ref imguiId, activeParam, false, selection);
                lastRowExists = true;
            }
        }
    }

    public void PropEditorParamRow(ParamBank bank, Param.Row row, Param.Row vrow, List<(string, Param.Row)> auxRows,
        Param.Row crow, ref string propSearchString, string activeParam, bool isActiveView,
        ParamEditorSelectionState selection)
    {
        var meta = ParamMetaData.Get(row.Def);
        var imguiId = 0;
        var showParamCompare = auxRows.Count > 0;
        var showRowCompare = crow != null;

        PropEditorParamRow_Header(isActiveView, ref propSearchString);

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
                Smithbox.ProjectHandler.CurrentProject.Config.PinnedFields.GetValueOrDefault(activeParam, null);

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

            PropEditorParamRow_RowFields(bank, row, vrow, auxRows, crow, ref imguiId, selection, activeParam);

            var search = propSearchString;
            List<(PseudoColumn, Param.Column)> cols = UICache.GetCached(_paramEditor, row, "fieldFilter",
                () => CellSearchEngine.cse.Search((activeParam, row), search, true, true));

            List<(PseudoColumn, Param.Column)> vcols = UICache.GetCached(_paramEditor, vrow, "vFieldFilter",
                () => cols.Select((x, i) => x.GetAs(ParamBank.VanillaBank.GetParamFromName(activeParam))).ToList());

            List<List<(PseudoColumn, Param.Column)>> auxCols = UICache.GetCached(_paramEditor, auxRows,
                "auxFieldFilter", () => auxRows.Select((r, i) => cols.Select((c, j) => c.GetAs(ParamBank.AuxBanks[r.Item1].GetParamFromName(activeParam))).ToList()).ToList());

            if (CFG.Current.Param_PinnedRowsStayVisible)
            {
                if (pinnedFields?.Count > 0)
                {
                    PropEditorParamRow_PinnedFields(pinnedFields, bank, row, vrow, auxRows, crow, cols, vcols, auxCols,
                        ref imguiId, activeParam, selection);
                    EditorDecorations.ImguiTableSeparator();
                }
            }

            if (CFG.Current.Param_ShowColorPreview)
            {
                PropEditorParamRow_ColorEditors(isActiveView, meta, row, cols, imguiId);
            }

            PropEditorParamRow_MainFields(meta, bank, row, vrow, auxRows, crow, cols, vcols, auxCols, ref imguiId,
                activeParam, selection, pinnedFields);

            if (CFG.Current.Param_ShowGraphVisualisation)
            {
                if (meta.CalcCorrectDef != null || meta.SoulCostDef != null)
                {
                    EditorDecorations.DrawCalcCorrectGraph(_paramEditor, meta, row);
                }
            }

            ImGui.EndTable();
        }

    }

    // Many parameter options, which may be simplified.
    private void PropEditorPropInfoRow(ParamBank bank, Param.Row row, Param.Row vrow,
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
            activeParam,
            false,
            null,
            selection);
    }

    private void PropEditorPropCellRow(ParamBank bank, Param.Row row, Param.Row crow,
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
            FieldMetaData.Get(col.Item2.Def),
            col.GetColumnType(),
            typeof(Param.Cell).GetProperty("Value"),
            row[col.Item2],
            row,
            activeParam,
            isPinned,
            col.Item2,
            selection);
    }

    private void PropEditorPropRow(ParamBank bank, object oldval, object compareval, object vanillaval,
        List<object> auxVals, ref int imguiId, string fieldOffset, string internalName, FieldMetaData cellMeta,
        Type propType, PropertyInfo proprow, Param.Cell? nullableCell, Param.Row row, string activeParam,
        bool isPinned, Param.Column col, ParamEditorSelectionState selection)
    {
        var Wiki = cellMeta?.Wiki;

        List<ParamRef> RefTypes = cellMeta?.RefTypes;
        List<FMGRef> FmgRef = cellMeta?.FmgRef;
        List<ExtRef> ExtRefs = cellMeta?.ExtRefs;
        List<TexRef> TextureRef = cellMeta?.TextureRef;

        var VirtualRef = cellMeta?.VirtualRef;

        ParamEnum Enum = cellMeta?.EnumType;
        var IsBool = cellMeta?.IsBool ?? false;
        var IsInvertedPercentage = cellMeta?.IsInvertedPercentage ?? false;
        var IsHiddenField = cellMeta?.IsHidden ?? false;

        var displayRefTypes = !CFG.Current.Param_HideReferenceRows && RefTypes != null;
        var displayFmgRef = !CFG.Current.Param_HideReferenceRows && FmgRef != null;
        var displayTextureRef = !CFG.Current.Param_HideReferenceRows && TextureRef != null;
        var displayEnum = !CFG.Current.Param_HideEnums && Enum != null;

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

        if (cellMeta != null)
        {
            showParticleEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowParticleEnumList;
            showSoundEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowSoundEnumList;
            showFlagEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowFlagEnumList;
            showCutsceneEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowCutsceneEnumList;
            showMovieEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowMovieEnumList;
            showProjectEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowProjectEnumList;
        }

        object newval = null;

        if(CFG.Current.Param_HidePaddingFields && IsHiddenField)
        {
            return;
        }

        ImGui.PushID(imguiId);

        if (ImGui.TableNextColumn())
        {
            ImGui.AlignTextToFramePadding();
            if (Wiki != null)
            {
                if (EditorDecorations.HelpIcon(internalName, ref Wiki, true))
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

            ImGui.Selectable("", false, ImGuiSelectableFlags.AllowItemOverlap);

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

            PropertyRowName(fieldOffset, ref internalName, cellMeta);

            if (displayRefTypes || displayFmgRef || displayTextureRef || displayEnum || showParticleEnum || showSoundEnum || showFlagEnum || showCutsceneEnum || showMovieEnum || showProjectEnum)
            {
                ImGui.BeginGroup();

                // Param Ref
                if (displayRefTypes)
                {
                    EditorDecorations.ParamRefText(RefTypes, row);
                }

                // Text Ref
                if (displayFmgRef)
                {
                    EditorDecorations.FmgRefText(FmgRef, row);
                }

                // Texture Ref
                if (displayTextureRef)
                {
                    EditorDecorations.TextureRefText(TextureRef, row);
                }

                if (displayEnum)
                {
                    EditorDecorations.EnumNameText(Enum);
                }

                // Particle list
                if (showParticleEnum)
                {
                    EditorDecorations.AliasEnumNameText("PARTICLES");
                }

                // Sound list
                if (showSoundEnum)
                {
                    EditorDecorations.AliasEnumNameText("SOUNDS");
                }

                // Flag list
                if (showFlagEnum)
                {
                    EditorDecorations.ConditionalAliasEnumNameText("FLAGS", row, FlagAliasEnum_ConditionalField, FlagAliasEnum_ConditionalValue);
                }

                // Cutscene list
                if (showCutsceneEnum)
                {
                    EditorDecorations.AliasEnumNameText("CUTSCENES");
                }

                // Movie list
                if (showMovieEnum)
                {
                    EditorDecorations.ConditionalAliasEnumNameText("MOVIES", row, MovieAliasEnum_ConditionalField, MovieAliasEnum_ConditionalValue);
                }

                // Project Enum
                if (showProjectEnum)
                {
                    EditorDecorations.ProjectEnumNameText(cellMeta.ProjectEnumType);
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
                ImGui.PushStyleColor(ImGuiCol.FrameBg, CFG.Current.ImGui_Input_Conflict_Background);
            }
            else if (diffVanilla)
            {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, CFG.Current.ImGui_Input_Vanilla_Background);
            }

            if (isRef)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_IsRef_Text);
            }
            else if (matchDefault)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
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

            if (displayRefTypes || displayFmgRef || displayTextureRef || displayEnum || showParticleEnum || showSoundEnum || showFlagEnum || showCutsceneEnum || showMovieEnum || showProjectEnum)
            {
                ImGui.BeginGroup();

                // ParamRef
                if (displayRefTypes)
                {
                    EditorDecorations.ParamRefsSelectables(bank, RefTypes, row, oldval);
                }

                // FmgRef
                if (displayFmgRef)
                {
                    EditorDecorations.FmgRefSelectable(_paramEditor, FmgRef, row, oldval);
                }

                // TextureRef
                if (displayTextureRef)
                {
                    EditorDecorations.TextureRefSelectable(_paramEditor, TextureRef, row, oldval);
                }

                // Enum
                if (displayEnum)
                {
                    EditorDecorations.EnumValueText(Enum.values, oldval.ToString());
                }

                // ParticleAlias
                if (showParticleEnum)
                {
                    EditorDecorations.AliasEnumValueText(Smithbox.BankHandler.ParticleAliases.GetEnumDictionary(), oldval.ToString());
                }

                // SoundAlias
                if (showSoundEnum)
                {
                    EditorDecorations.AliasEnumValueText(Smithbox.BankHandler.SoundAliases.GetEnumDictionary(), oldval.ToString());
                }

                // FlagAlias
                if (showFlagEnum)
                {
                    EditorDecorations.ConditionalAliasEnumValueText(Smithbox.BankHandler.EventFlagAliases.GetEnumDictionary(), oldval.ToString(), row, FlagAliasEnum_ConditionalField, FlagAliasEnum_ConditionalValue);
                }

                // CutsceneAlias
                if (showCutsceneEnum)
                {
                    EditorDecorations.AliasEnumValueText(Smithbox.BankHandler.CutsceneAliases.GetEnumDictionary(), oldval.ToString());
                }

                // MovieAlias
                if (showMovieEnum)
                {
                    EditorDecorations.ConditionalAliasEnumValueText(Smithbox.BankHandler.MovieAliases.GetEnumDictionary(), oldval.ToString(), row, MovieAliasEnum_ConditionalField, MovieAliasEnum_ConditionalValue);
                }

                // ProjectEnum
                if (showProjectEnum)
                {
                    EditorDecorations.ProjectEnumValueText(cellMeta.ProjectEnumType, oldval.ToString());
                }

                ImGui.EndGroup();

            }

            EditorDecorations.ParamRefEnumQuickLink(bank, oldval, RefTypes, row, FmgRef, Enum, TextureRef);

            // Param Reference Buttons
            if (CFG.Current.Param_ViewInMapOption)
        {
                // These are placed at the top, below the ID row
                if (imguiId == 1)
                {
                    ParamReferenceUtils.BonfireWarpParam(activeParam, row, internalName);
                    ParamReferenceUtils.GameAreaParam(activeParam, row, internalName);
                    ParamReferenceUtils.ItemLotParam(activeParam, row, internalName);
                }
            }

            if (CFG.Current.Param_ViewModelOption)
            {
                // These are placed at the top, below the ID row
                if (imguiId == 1)
                {
                    ParamReferenceUtils.AssetGeometryParam(activeParam, row, internalName);
                    ParamReferenceUtils.BuddyStoneParam(activeParam, row, internalName);
                }

                // These are placed in-line with the current field
                ParamReferenceUtils.GrassTypeParam(activeParam, row, internalName);
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

            if (conflict || diffVanilla)
            {
                ImGui.PopStyleColor();
            }
        }

        ImGui.PushStyleColor(ImGuiCol.FrameBg, CFG.Current.ImGui_Input_Default_Background);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);

        if (conflict)
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, CFG.Current.ImGui_Input_Conflict_Background);
        }

        if (CFG.Current.Param_ShowVanillaParams && ImGui.TableNextColumn())
        {
            AdditionalColumnValue(vanillaval, propType, bank, RefTypes, FmgRef, row, Enum, TextureRef, "vanilla", cellMeta);
        }

        for (var i = 0; i < auxVals.Count; i++)
        {
            if (ImGui.TableNextColumn())
            {
                if (!conflict && diffAuxVanilla[i])
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, CFG.Current.ImGui_Input_AuxVanilla_Background);

                AdditionalColumnValue(auxVals[i], propType, bank, RefTypes, FmgRef, row, Enum, TextureRef, i.ToString(), cellMeta);
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
                ImGui.PushStyleColor(ImGuiCol.FrameBg, CFG.Current.ImGui_Input_DiffCompare_Background);
            }

            AdditionalColumnValue(compareval, propType, bank, RefTypes, FmgRef, row, Enum, TextureRef, "compRow", cellMeta);

            if (diffCompare)
            {
                ImGui.PopStyleColor();
            }
        }

        ImGui.PopStyleColor(2);

        if (ImGui.BeginPopup("ParamRowCommonMenu"))
        {
            PropertyRowNameContextMenuItems(bank, internalName, cellMeta, activeParam, activeParam != null,
                isPinned, col, selection, propType, Wiki, oldval, true);
            PropertyRowValueContextMenuItems(bank, row, cellMeta, internalName, VirtualRef, ExtRefs, oldval, ref newval,
                RefTypes, FmgRef, TextureRef, Enum);

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("ParamRowNameMenu"))
        {
            PropertyRowNameContextMenuItems(bank, internalName, cellMeta, activeParam, activeParam != null,
                isPinned, col, selection, propType, Wiki, oldval, true);

            ImGui.EndPopup();
        }

        if (ImGui.BeginPopup("ParamRowValueMenu"))
        {
            PropertyRowNameContextMenuItems(bank, internalName, cellMeta, activeParam, activeParam != null,
                isPinned, col, selection, propType, Wiki, oldval, false);
            PropertyRowValueContextMenuItems(bank, row, cellMeta, internalName, VirtualRef, ExtRefs, oldval, ref newval,
                RefTypes, FmgRef, TextureRef, Enum);

            ImGui.EndPopup();
        }

        var committed = ParamEditorCommon.UpdateProperty(ContextActionManager,
            nullableCell != null ? nullableCell : row, proprow, oldval);
        if (committed && !ParamBank.VanillaBank.IsLoadingParams)
            ParamBank.PrimaryBank.RefreshParamRowDiffs(row, activeParam);

        ImGui.PopID();
        imguiId++;
    }

    private void AdditionalColumnValue(object colVal, Type propType, ParamBank bank, List<ParamRef> RefTypes,
        List<FMGRef> FmgRef, Param.Row context, ParamEnum Enum, List<TexRef> TextureRef, string imguiSuffix, FieldMetaData cellMeta)
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

            if (cellMeta != null)
            {
                showParticleEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowParticleEnumList;
                showSoundEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowSoundEnumList;
                showFlagEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowFlagEnumList;
                showCutsceneEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowCutsceneEnumList;
                showMovieEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowMovieEnumList;
                showProjectEnum = !CFG.Current.Param_HideEnums && cellMeta.ShowProjectEnumList;
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
                EditorDecorations.ParamRefsSelectables(bank, RefTypes, context, colVal);
            }

            if (CFG.Current.Param_HideReferenceRows == false && FmgRef != null)
            {
                EditorDecorations.FmgRefSelectable(_paramEditor, FmgRef, context, colVal);
            }

            if (CFG.Current.Param_HideReferenceRows == false && TextureRef != null)
            {
                EditorDecorations.TextureRefSelectable(_paramEditor, TextureRef, context, colVal);
            }

            if (CFG.Current.Param_HideEnums == false && Enum != null)
            {
                EditorDecorations.EnumValueText(Enum.values, colVal.ToString());
            }

            // ParticleAlias
            if (showParticleEnum)
            {
                EditorDecorations.AliasEnumValueText(Smithbox.BankHandler.ParticleAliases.GetEnumDictionary(), colVal.ToString());
            }

            // SoundAlias
            if (showSoundEnum)
            {
                EditorDecorations.AliasEnumValueText(Smithbox.BankHandler.SoundAliases.GetEnumDictionary(), colVal.ToString());
            }

            // FlagAlias
            if (showFlagEnum)
            {
                EditorDecorations.ConditionalAliasEnumValueText(Smithbox.BankHandler.EventFlagAliases.GetEnumDictionary(), colVal.ToString(), context, FlagAliasEnum_ConditionalField, FlagAliasEnum_ConditionalValue);
            }

            // CutsceneAlias
            if (showCutsceneEnum)
            {
                EditorDecorations.AliasEnumValueText(Smithbox.BankHandler.CutsceneAliases.GetEnumDictionary(), colVal.ToString());
            }

            // MovieAlias
            if (showMovieEnum)
            {
                EditorDecorations.ConditionalAliasEnumValueText(Smithbox.BankHandler.MovieAliases.GetEnumDictionary(), colVal.ToString(), context, MovieAliasEnum_ConditionalField, MovieAliasEnum_ConditionalValue);
            }

            // ProjectEnum
            if (showProjectEnum)
            {
                EditorDecorations.ProjectEnumValueText(cellMeta.ProjectEnumType, colVal.ToString());
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

    private static void PropertyRowName(string fieldOffset, ref string internalName, FieldMetaData cellMeta)
    {
        var altName = cellMeta?.AltName;

        if (cellMeta != null && ParamEditorScreen.EditorMode)
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

    private void PropertyRowNameContextMenuItems(ParamBank bank, string internalName, FieldMetaData cellMeta,
        string activeParam, bool showPinOptions, bool isPinned, Param.Column col,
        ParamEditorSelectionState selection, Type propType, string Wiki, dynamic oldval, bool isNameMenu)
    {
        var scale = Smithbox.GetUIScale();
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
            string currInput = ParamAction_MassEdit._currentMEditRegexInput;

            if(currInput == "")
            {
                // Add selection section if input is empty
                ParamAction_MassEdit._currentMEditRegexInput = $"selection: {propertyName}: ";
            }
            else
            {
                // Otherwise just add the property name
                currInput = $"{currInput}{propertyName}";
                ParamAction_MassEdit._currentMEditRegexInput = currInput;
            }
        }

        // Pin Options
        if (showPinOptions && CFG.Current.Param_FieldContextMenu_PinOptions)
        {
            if (ImGui.MenuItem(isPinned ? "解 Unpin " : "钉 Pin " + internalName))
            {
                if (!Smithbox.ProjectHandler.CurrentProject.Config.PinnedFields.ContainsKey(activeParam))
                {
                    Smithbox.ProjectHandler.CurrentProject.Config.PinnedFields.Add(activeParam, new List<string>());
                }

                List<string> pinned = Smithbox.ProjectHandler.CurrentProject.Config.PinnedFields[activeParam];

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
                EditorDecorations.PinListReorderOptions(Smithbox.ProjectHandler.CurrentProject.Config.PinnedFields[activeParam],
                    internalName);
            }

            if (ImGui.Selectable("全解 Unpin all"))
            {
                Smithbox.ProjectHandler.CurrentProject.Config.PinnedFields.Clear();
            }

            ImGui.Separator();
        }

        // Compare
        if (CFG.Current.Param_FieldContextMenu_CompareOptions)
        {
            if (col != null && ImGui.MenuItem("区块比较 Compare field"))
            {
                selection.SetCompareCol(col);
            }
        }

        // Value Distribution
        if (CFG.Current.Param_FieldContextMenu_ValueDistribution)
        {
            if (ImGui.Selectable("浏览选中行介绍 View value distribution in selected rows..."))
            {
                EditorCommandQueue.AddCommand($@"param/menu/distributionPopup/{internalName}");
            }
        }

        // Editor Mode
        if (ParamEditorScreen.EditorMode && cellMeta != null)
        {
            if (ImGui.BeginMenu("添加引用 Add Reference"))
            {
                foreach (var p in bank.Params.Keys)
                {
                    if (ImGui.MenuItem(p + "##add" + p))
                    {
                        if (cellMeta.RefTypes == null)
                        {
                            cellMeta.RefTypes = new List<ParamRef>();
                        }

                        cellMeta.RefTypes.Add(new ParamRef(p));
                    }
                }

                ImGui.EndMenu();
            }

            if (cellMeta.RefTypes != null && ImGui.BeginMenu("移除引用 Remove Reference"))
            {
                foreach (ParamRef p in cellMeta.RefTypes)
                {
                    if (ImGui.MenuItem(p.param + "##remove" + p.param))
                    {
                        cellMeta.RefTypes.Remove(p);

                        if (cellMeta.RefTypes.Count == 0)
                        {
                            cellMeta.RefTypes = null;
                        }

                        break;
                    }
                }

                ImGui.EndMenu();
            }

            if (ImGui.MenuItem(cellMeta.IsBool ? "移除 Remove bool toggle" : "添加 Add bool toggle"))
            {
                cellMeta.IsBool = !cellMeta.IsBool;
            }

            if (cellMeta.Wiki == null && ImGui.MenuItem("添加 Add wiki..."))
            {
                cellMeta.Wiki = "Empty wiki...";
            }

            if (cellMeta.Wiki != null && ImGui.MenuItem("移除 Remove wiki"))
            {
                cellMeta.Wiki = null;
            }
        }

        ImGui.PopStyleVar();
    }

    private void PropertyRowValueContextMenuItems(ParamBank bank, Param.Row row, FieldMetaData cellMeta, string internalName,
        string VirtualRef, List<ExtRef> ExtRefs, dynamic oldval, ref object newval, List<ParamRef> RefTypes,
        List<FMGRef> FmgRef, List<TexRef> TextureRef, ParamEnum Enum)
    {
        if (CFG.Current.Param_FieldContextMenu_References)
        {
            if (RefTypes != null || FmgRef != null || TextureRef != null || Enum != null || cellMeta != null)
            {
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Ref_Text);

                if (EditorDecorations.ParamRefEnumContextMenuItems(bank, cellMeta, oldval, ref newval, RefTypes, row, FmgRef, TextureRef, Enum, ContextActionManager))
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
                ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_VirtualRef_Text);
                EditorDecorations.VirtualParamRefSelectables(bank, VirtualRef, oldval, row, internalName, ExtRefs,
                    _paramEditor);
                ImGui.PopStyleColor();
            }
        }

        if (CFG.Current.Param_FieldContextMenu_MassEdit)
        {
            ImGui.Separator();

            if (!CFG.Current.Param_FieldContextMenu_FullMassEdit)
            {
                if (ImGui.Selectable("批量编辑 Mass edit"))
                {
                    EditorCommandQueue.AddCommand(
                        $@"param/menu/massEditRegex/selection: {Regex.Escape(internalName)}: ");
                }

                if (ImGui.Selectable("重置默认 Reset to vanilla"))
                {
                    MassParamEditRegex.PerformMassEdit(ParamBank.PrimaryBank,
                        $"selection && !added: {Regex.Escape(internalName)}: = vanilla;",
                         Smithbox.EditorHandler.ParamEditor._activeView._selection);
                }
            }
            else
            {
                if (ImGui.CollapsingHeader("批量编辑 Mass edit", ImGuiTreeNodeFlags.SpanFullWidth))
                {
                    ImGui.Separator();

                    if (ImGui.Selectable("手动 Manually..."))
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/menu/massEditRegex/selection: {Regex.Escape(internalName)}: ");
                    }

                    if (ImGui.Selectable("重置默认 Reset to vanilla..."))
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
