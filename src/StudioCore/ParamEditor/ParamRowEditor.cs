using Andre.Formats;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.ParamEditor;

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

    private void PropEditorParamRow_RowFields(ParamBank bank, Param.Row row, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, ref int imguiId, ParamEditorSelectionState selection)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.8f, 0.8f, 1.0f, 1.0f));
        PropertyInfo nameProp = row.GetType().GetProperty("Name");
        PropertyInfo idProp = row.GetType().GetProperty("ID");
        PropEditorPropInfoRow(bank, row, vrow, auxRows, crow, nameProp, "Name", ref imguiId, selection);
        PropEditorPropInfoRow(bank, row, vrow, auxRows, crow, idProp, "ID", ref imguiId, selection);
        ImGui.PopStyleColor();
        ImGui.Spacing();
    }

    private void PropEditorParamRow_PinnedFields(List<string> pinList, ParamBank bank, Param.Row row,
        Param.Row vrow, List<(string, Param.Row)> auxRows, Param.Row crow, List<(PseudoColumn, Param.Column)> cols,
        List<(PseudoColumn, Param.Column)> vcols, List<List<(PseudoColumn, Param.Column)>> auxCols, ref int imguiId,
        string activeParam, ParamEditorSelectionState selection)
    {
        List<string> pinnedFields = new List<string>(pinList);
        foreach (var field in pinnedFields)
        {
            List<(PseudoColumn, Param.Column)> matches =
                cols.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();
            List<(PseudoColumn, Param.Column)> vmatches =
                vcols.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();
            List<List<(PseudoColumn, Param.Column)>> auxMatches = auxCols.Select((aux, i) =>
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
        string activeParam, ParamEditorSelectionState selection)
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

        var lastRowExists = false;
        foreach (var field in fieldOrder)
        {
            if (field.Equals("-") && lastRowExists)
            {
                EditorDecorations.ImguiTableSeparator();
                lastRowExists = false;
                continue;
            }

            List<(PseudoColumn, Param.Column)> matches =
                cols?.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();
            List<(PseudoColumn, Param.Column)> vmatches =
                vcols?.Where((x, i) => x.Item2 != null && x.Item2.Def.InternalName == field).ToList();
            List<List<(PseudoColumn, Param.Column)>> auxMatches = auxCols?.Select((aux, i) =>
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
        ParamMetaData meta = ParamMetaData.Get(row.Def);
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
                _paramEditor._projectSettings.PinnedFields.GetValueOrDefault(activeParam, null);

            ImGui.TableSetupScrollFreeze(columnCount, (showParamCompare ? 3 : 2) + (1 + pinnedFields?.Count ?? 0));
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

            PropEditorParamRow_RowFields(bank, row, vrow, auxRows, crow, ref imguiId, selection);
            EditorDecorations.ImguiTableSeparator();

            var search = propSearchString;
            List<(PseudoColumn, Param.Column)> cols = UICache.GetCached(_paramEditor, row, "fieldFilter",
                () => CellSearchEngine.cse.Search((activeParam, row), search, true, true));
            List<(PseudoColumn, Param.Column)> vcols = UICache.GetCached(_paramEditor, vrow, "vFieldFilter",
                () => cols.Select((x, i) => x.GetAs(ParamBank.VanillaBank.GetParamFromName(activeParam))).ToList());
            List<List<(PseudoColumn, Param.Column)>> auxCols = UICache.GetCached(_paramEditor, auxRows,
                "auxFieldFilter",
                () => auxRows.Select((r, i) =>
                    cols.Select((c, j) => c.GetAs(ParamBank.AuxBanks[r.Item1].GetParamFromName(activeParam)))
                        .ToList()).ToList());

            if (pinnedFields?.Count > 0)
            {
                PropEditorParamRow_PinnedFields(pinnedFields, bank, row, vrow, auxRows, crow, cols, vcols, auxCols,
                    ref imguiId, activeParam, selection);
                EditorDecorations.ImguiTableSeparator();
            }

            PropEditorParamRow_MainFields(meta, bank, row, vrow, auxRows, crow, cols, vcols, auxCols, ref imguiId,
                activeParam, selection);
            ImGui.EndTable();
        }

        if (meta.CalcCorrectDef != null || meta.SoulCostDef != null)
        {
            EditorDecorations.DrawCalcCorrectGraph(_paramEditor, meta, row);
        }
    }

    // Many parameter options, which may be simplified.
    private void PropEditorPropInfoRow(ParamBank bank, Param.Row row, Param.Row vrow,
        List<(string, Param.Row)> auxRows, Param.Row crow, PropertyInfo prop, string visualName, ref int imguiId,
        ParamEditorSelectionState selection)
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
            null,
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
        bool isPinned, Param.Column? col, ParamEditorSelectionState selection)
    {
        var Wiki = cellMeta?.Wiki;

        List<ParamRef> RefTypes = cellMeta?.RefTypes;
        List<FMGRef> FmgRef = cellMeta?.FmgRef;
        List<ExtRef> ExtRefs = cellMeta?.ExtRefs;
        var VirtualRef = cellMeta?.VirtualRef;

        ParamEnum Enum = cellMeta?.EnumType;
        var IsBool = cellMeta?.IsBool ?? false;

        var displayRefTypes = !CFG.Current.Param_HideReferenceRows && RefTypes != null;
        var displayFmgRef = !CFG.Current.Param_HideReferenceRows && FmgRef != null;
        var displayEnum = !CFG.Current.Param_HideEnums && Enum != null;

        object newval = null;

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
                ImGui.OpenPopup("ParamRowCommonMenu");
            }

            ImGui.SameLine();

            PropertyRowName(fieldOffset, ref internalName, cellMeta);

            if (displayRefTypes || displayFmgRef || displayEnum)
            {
                ImGui.BeginGroup();
                if (displayRefTypes)
                {
                    EditorDecorations.ParamRefText(RefTypes, row);
                }

                if (displayFmgRef)
                {
                    EditorDecorations.FmgRefText(FmgRef, row);
                }

                if (displayEnum)
                {
                    EditorDecorations.EnumNameText(Enum);
                }

                ImGui.EndGroup();
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup("ParamRowCommonMenu");
                }
            }
        }

        var diffVanilla = ParamUtils.IsValueDiff(ref oldval, ref vanillaval, propType);
        var diffCompare = ParamUtils.IsValueDiff(ref oldval, ref compareval, propType);
        List<bool> diffAuxVanilla =
            auxVals.Select((o, i) => ParamUtils.IsValueDiff(ref o, ref vanillaval, propType)).ToList();
        List<bool> diffAuxPrimaryAndVanilla = auxVals.Select((o, i) =>
            ParamUtils.IsValueDiff(ref o, ref oldval, propType) &&
            ParamUtils.IsValueDiff(ref o, ref vanillaval, propType)).ToList();
        var count = diffAuxPrimaryAndVanilla.Where(x => x).Count();
        var conflict = (diffVanilla ? 1 : 0) + diffAuxPrimaryAndVanilla.Where(x => x).Count() > 1;

        var matchDefault = nullableCell?.Def.Default != null && nullableCell.Value.Def.Default.Equals(oldval);
        var isRef = (CFG.Current.Param_HideReferenceRows == false && (RefTypes != null || FmgRef != null)) ||
                    (CFG.Current.Param_HideEnums == false && Enum != null) || VirtualRef != null ||
                    ExtRefs != null;

        if (ImGui.TableNextColumn())
        {
            if (conflict)
            {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.25f, 0.2f, 0.2f, 1.0f));
            }
            else if (diffVanilla)
            {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.22f, 0.2f, 1.0f));
            }

            if (isRef)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.5f, 1.0f, 1.0f));
            }
            else if (matchDefault)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.75f, 0.75f, 0.75f, 1.0f));
            }

            // Property Editor UI
            ParamEditorCommon.PropertyField(propType, oldval, ref newval, IsBool);

            if (isRef || matchDefault) //if diffVanilla, remove styling later
            {
                ImGui.PopStyleColor();
            }

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                ImGui.OpenPopup("ParamRowCommonMenu");
            }

            if (displayRefTypes || displayFmgRef || displayEnum)
            {
                ImGui.BeginGroup();
                if (displayRefTypes)
                {
                    EditorDecorations.ParamRefsSelectables(bank, RefTypes, row, oldval);
                }

                if (displayFmgRef)
                {
                    EditorDecorations.FmgRefSelectable(_paramEditor, FmgRef, row, oldval);
                }

                if (displayEnum)
                {
                    EditorDecorations.EnumValueText(Enum.values, oldval.ToString());
                }

                ImGui.EndGroup();
                EditorDecorations.ParamRefEnumQuickLink(bank, oldval, RefTypes, row, FmgRef, Enum);
                if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                {
                    ImGui.OpenPopup("ParamRowCommonMenu");
                }
            }

            if (conflict || diffVanilla)
            {
                ImGui.PopStyleColor();
            }
        }

        ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.180f, 0.180f, 0.196f, 1.0f));
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.9f, 0.9f, 0.9f, 1.0f));
        if (conflict)
        {
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.25f, 0.2f, 0.2f, 1.0f));
        }

        if (CFG.Current.Param_ShowVanillaParams && ImGui.TableNextColumn())
        {
            AdditionalColumnValue(vanillaval, propType, bank, RefTypes, FmgRef, row, Enum, "vanilla");
        }

        for (var i = 0; i < auxVals.Count; i++)
        {
            if (ImGui.TableNextColumn())
            {
                if (!conflict && diffAuxVanilla[i])
                {
                    ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.35f, 1.0f));
                }

                AdditionalColumnValue(auxVals[i], propType, bank, RefTypes, FmgRef, row, Enum, i.ToString());
                if (!conflict && diffAuxVanilla[i])
                {
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
                ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0.2f, 0.2f, 0.35f, 1.0f));
            }

            AdditionalColumnValue(compareval, propType, bank, RefTypes, FmgRef, row, Enum, "compRow");
            if (diffCompare)
            {
                ImGui.PopStyleColor();
            }
        }

        ImGui.PopStyleColor(2);
        if (ImGui.BeginPopup("ParamRowCommonMenu"))
        {
            PropertyRowNameContextMenuItems(bank, internalName, cellMeta, activeParam, activeParam != null,
                isPinned, col, selection, propType, Wiki, oldval);
            PropertyRowValueContextMenuItems(bank, row, internalName, VirtualRef, ExtRefs, oldval, ref newval,
                RefTypes, FmgRef, Enum);
            ImGui.EndPopup();
        }

        var committed = ParamEditorCommon.UpdateProperty(ContextActionManager,
            nullableCell != null ? nullableCell : row, proprow, oldval);
        if (committed && !ParamBank.VanillaBank.IsLoadingParams)
        {
            ParamBank.PrimaryBank.RefreshParamRowDiffs(row, activeParam);
        }

        ImGui.PopID();
        imguiId++;
    }

    private void AdditionalColumnValue(object colVal, Type propType, ParamBank bank, List<ParamRef> RefTypes,
        List<FMGRef> FmgRef, Param.Row context, ParamEnum Enum, string imguiSuffix)
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
            if (CFG.Current.Param_HideReferenceRows == false && RefTypes != null)
            {
                EditorDecorations.ParamRefsSelectables(bank, RefTypes, context, colVal);
            }

            if (CFG.Current.Param_HideReferenceRows == false && FmgRef != null)
            {
                EditorDecorations.FmgRefSelectable(_paramEditor, FmgRef, context, colVal);
            }

            if (CFG.Current.Param_HideEnums == false && Enum != null)
            {
                EditorDecorations.EnumValueText(Enum.values, colVal.ToString());
            }
        }
    }

    private static string OffsetTextOfColumn(Param.Column? col)
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
                    {
                        printedName = $"{printedName} ({internalName})";
                    }
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
        ParamEditorSelectionState selection, Type propType, string Wiki, dynamic oldval)
    {
        var scale = MapStudioNew.GetUIScale();
        var altName = cellMeta?.AltName;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 10f) * scale);

        if (col != null)
        {
            EditorDecorations.ImGui_DisplayPropertyInfo(propType, internalName, altName, col.Def.ArrayLength, col.Def.BitSize);
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
        else
        {
            // Headers
            ImGui.TextColored(new Vector4(1.0f, 0.7f, 0.4f, 1.0f), Utils.ImGuiEscape(internalName, "", true));
        }

        ImGui.Separator();

        if (showPinOptions)
        {
            if (ImGui.MenuItem(isPinned ? "Unpin " : "Pin " + internalName))
            {
                if (!_paramEditor._projectSettings.PinnedFields.ContainsKey(activeParam))
                {
                    _paramEditor._projectSettings.PinnedFields.Add(activeParam, new List<string>());
                }

                List<string> pinned = _paramEditor._projectSettings.PinnedFields[activeParam];
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
                EditorDecorations.PinListReorderOptions(_paramEditor._projectSettings.PinnedFields[activeParam], internalName);
            }
            ImGui.Separator();
        }

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

        if (col != null && ImGui.MenuItem("Compare field"))
        {
            selection.SetCompareCol(col);
        }

        if (ImGui.Selectable("View value distribution in selected rows..."))
        {
            EditorCommandQueue.AddCommand($@"param/menu/distributionPopup/{internalName}");
        }

        if (ParamEditorScreen.EditorMode && ImGui.BeginMenu("Find rows with this value..."))
        {
            foreach (KeyValuePair<string, Param> p in bank.Params)
            {
                var v = (int)oldval;
                Param.Row r = p.Value[v];
                if (r != null && ImGui.Selectable($@"{p.Key}: {Utils.ImGuiEscape(r.Name, "null")}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{p.Key}/{v}");
                }
            }

            ImGui.EndMenu();
        }

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

    private void PropertyRowValueContextMenuItems(ParamBank bank, Param.Row row, string internalName,
        string VirtualRef, List<ExtRef> ExtRefs, dynamic oldval, ref object newval, List<ParamRef> RefTypes,
        List<FMGRef> FmgRef, ParamEnum Enum)
    {
        if (VirtualRef != null || ExtRefs != null)
        {
            ImGui.Separator();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.75f, 1.0f, 1.0f));
            EditorDecorations.VirtualParamRefSelectables(bank, VirtualRef, oldval, row, internalName, ExtRefs,
                _paramEditor);
            ImGui.PopStyleColor();
        }

        if (RefTypes != null || FmgRef != null || Enum != null)
        {
            ImGui.Separator();
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1.0f, 0.75f, 0.75f, 1.0f));
            if (EditorDecorations.ParamRefEnumContextMenuItems(bank, oldval, ref newval, RefTypes, row, FmgRef,
                    Enum, ContextActionManager))
            {
                ParamEditorCommon.SetLastPropertyManual(newval);
            }

            ImGui.PopStyleColor();
        }

        ImGui.Separator();
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
