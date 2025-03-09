using Andre.Formats;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
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
            if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SearchField))
            {
                ImGui.SetKeyboardFocusHere();
            }

            // Autofill
            ImGui.AlignTextToFramePadding();
            var resAutoCol = AutoFill.ColumnSearchBarAutoFill();
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
            UIHelper.ShowHoverTooltip($"Search <{KeyBindings.Current.PARAM_SearchField.HintText}>");

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

    private void PropEditorParamRow_RowFields(ParamBank bank, Param.Row row, ParamMetaData? meta, Param.Row vrow,
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

    private void PropEditorParamRow_PinnedFields(List<string> pinList, ParamBank bank, ParamMetaData? meta,
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

    private void PropEditorParamRow_MainFields(ParamMetaData? meta, ParamBank bank, Param.Row row, Param.Row vrow,
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
                if (ParamEditorScreen.EditorMode)
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

            PropEditorParamRow_RowFields(bank, row, meta, vrow, auxRows, crow, ref imguiId, selection, activeParam);

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
                    EditorDecorations.DrawCalcCorrectGraph(_paramEditor, meta, row);
                }
            }

            ImGui.EndTable();
        }

    }

    // Many parameter options, which may be simplified.
    private void PropEditorPropInfoRow(ParamBank bank, Param.Row row, ParamMetaData? meta, Param.Row vrow,
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

    private void PropEditorPropCellRow(ParamBank bank, ParamMetaData? meta, Param.Row row, Param.Row crow,
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
            meta,
            activeParam,
            isPinned,
            col.Item2,
            selection);
    }


    private void PropEditorPropRow(ParamBank bank, object oldval, object compareval, object vanillaval,
        List<object> auxVals, ref int imguiId, string fieldOffset, string internalName, FieldMetaData cellMeta,
        Type propType, PropertyInfo proprow, Param.Cell? nullableCell, Param.Row row, ParamMetaData? meta, string activeParam,
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
            ImGui.Selectable("", false, ImGuiSelectableFlags.AllowItemOverlap);

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

                    UIHelper.ShowHoverTooltip(helpIconText);
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
            PropertyRowName(fieldOffset, ref internalName, cellMeta);

            if (displayRefTypes || displayFmgRef || displayTextureRef || displayEnum || showParticleEnum || showSoundEnum || showFlagEnum || showCutsceneEnum || showMovieEnum || showProjectEnum || showParamFieldOffset || displayMapFmgRef)
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

                // Map Text Ref
                if(displayMapFmgRef)
                {
                    EditorDecorations.FmgRefText(MapFmgRef, row, "MAP FMGS");
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

                // Param Field Offset
                if(showParamFieldOffset)
                {
                    EditorDecorations.ParamFieldOffsetText(activeParam, row, paramFieldIndex);
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
                    EditorDecorations.ParamRefsSelectables(bank, RefTypes, row, oldval);
                }

                // FmgRef
                if (displayFmgRef)
                {
                    EditorDecorations.FmgRefSelectable(_paramEditor, FmgRef, row, oldval);
                }

                // MapFmgRef
                if (displayMapFmgRef)
                {
                    EditorDecorations.FmgRefSelectable(_paramEditor, MapFmgRef, row, oldval);
                }

                // TextureRef
                if (displayTextureRef)
                {
                    EditorDecorations.TextureRefSelectable(_paramEditor, TextureRef, row, oldval);
                }

                // Enum
                if (displayEnum)
                {
                    EditorDecorations.EnumValueText(Enum.Values, oldval.ToString());
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

                // Param Field Offset
                if (showParamFieldOffset)
                {
                    EditorDecorations.ParamFieldOffsetValueText(activeParam, row, paramFieldIndex);
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
                    ParamReferenceUtils.ReturnPointParam(activeParam, row, internalName);
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
                ParamReferenceUtils.BulletParam(activeParam, row, internalName);
            }

            // Color Picker
            ParamReferenceUtils.ColorPicker(activeParam, row, internalName);

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
        if (EditorDecorations.ParamRefEnumShortcutItems(bank, cellMeta, oldval, ref newval, RefTypes, row, FmgRef, MapFmgRef, TextureRef, Enum, ContextActionManager))
        {
            ParamEditorCommon.SetLastPropertyManual(newval);
        }

        var committed = ParamEditorCommon.UpdateProperty(ContextActionManager,
            nullableCell != null ? nullableCell : row, proprow, oldval);
        if (committed && !ParamBank.VanillaBank.IsLoadingParams)
            ParamBank.PrimaryBank.RefreshParamRowDiffs(row, activeParam);

        ImGui.PopID();
        imguiId++;
    }

    private void AdditionalColumnValue(string activeParam, object colVal, Type propType, ParamBank bank, List<ParamRef> RefTypes,
        List<FMGRef> FmgRef, List<FMGRef> MapFmgRef, Param.Row context, ParamEnum Enum, List<TexRef> TextureRef, string imguiSuffix, FieldMetaData cellMeta)
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
                EditorDecorations.ParamRefsSelectables(bank, RefTypes, context, colVal);
            }

            if (CFG.Current.Param_HideReferenceRows == false && FmgRef != null)
            {
                EditorDecorations.FmgRefSelectable(_paramEditor, FmgRef, context, colVal);
            }

            if (CFG.Current.Param_HideReferenceRows == false && MapFmgRef != null)
            {
                EditorDecorations.FmgRefSelectable(_paramEditor, MapFmgRef, context, colVal);
            }

            if (CFG.Current.Param_HideReferenceRows == false && TextureRef != null)
            {
                EditorDecorations.TextureRefSelectable(_paramEditor, TextureRef, context, colVal);
            }

            if (CFG.Current.Param_HideEnums == false && Enum != null)
            {
                EditorDecorations.EnumValueText(Enum.Values, colVal.ToString());
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

            // Param Field Offset
            if (showParamFieldOffset)
            {
                EditorDecorations.ParamFieldOffsetValueText(activeParam, context, paramFieldIndex);
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
        ParamMetaData? meta, string activeParam, bool showPinOptions, bool isPinned, Param.Column col,
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
            string currInput = _paramEditor.ToolWindow.MassEditHandler._currentMEditRegexInput;

            if(currInput == "")
            {
                // Add selection section if input is empty
                _paramEditor.ToolWindow.MassEditHandler._currentMEditRegexInput = $"selection: {propertyName}: ";
            }
            else
            {
                // Otherwise just add the property name
                currInput = $"{currInput}{propertyName}";
                _paramEditor.ToolWindow.MassEditHandler._currentMEditRegexInput = currInput;
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

            if (ImGui.Selectable("Unpin all"))
            {
                Smithbox.ProjectHandler.CurrentProject.Config.PinnedFields.Clear();
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

        // Editor Mode
        if (ParamEditorScreen.EditorMode && cellMeta != null)
        {
            if (ImGui.BeginMenu("Add Reference"))
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

            if (cellMeta.RefTypes != null && ImGui.BeginMenu("Remove Reference"))
            {
                foreach (ParamRef p in cellMeta.RefTypes)
                {
                    if (ImGui.MenuItem(p.ParamName + "##remove" + p.ParamName))
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

            if (ImGui.MenuItem(cellMeta.IsBool ? "Remove bool toggle" : "Add bool toggle"))
            {
                cellMeta.IsBool = !cellMeta.IsBool;
            }

            if (cellMeta.Wiki == null && ImGui.MenuItem("Add wiki..."))
            {
                cellMeta.Wiki = "Empty wiki...";
            }

            if (cellMeta.Wiki != null && ImGui.MenuItem("Remove wiki"))
            {
                cellMeta.Wiki = null;
            }
        }

        ImGui.PopStyleVar();
    }

    private void PropertyRowValueContextMenuItems(ParamBank bank, Param.Row row, FieldMetaData cellMeta, string internalName,
        string VirtualRef, List<ExtRef> ExtRefs, dynamic oldval, ref object newval, List<ParamRef> RefTypes,
        List<FMGRef> FmgRef, List<FMGRef> MapFmgRef, List<TexRef> TextureRef, ParamEnum Enum)
    {
        if (CFG.Current.Param_FieldContextMenu_References)
        {
            if (RefTypes != null || FmgRef != null || MapFmgRef != null || TextureRef != null || Enum != null || cellMeta != null)
            {
                ImGui.Separator();
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Ref_Text);

                if (EditorDecorations.ParamRefEnumContextMenuItems(bank, cellMeta, oldval, ref newval, RefTypes, row, FmgRef, MapFmgRef, TextureRef, Enum, ContextActionManager))
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
                if (ImGui.Selectable("Mass edit"))
                {
                    EditorCommandQueue.AddCommand(
                        $@"param/menu/massEditRegex/selection: {Regex.Escape(internalName)}: ");
                }

                if (ImGui.Selectable("Reset to vanilla"))
                {
                    MassParamEditRegex.PerformMassEdit(ParamBank.PrimaryBank,
                        $"selection && !added: {Regex.Escape(internalName)}: = vanilla;",
                         Smithbox.EditorHandler.ParamEditor._activeView._selection);
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
