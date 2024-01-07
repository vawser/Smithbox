using Andre.Formats;
using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Veldrid;

namespace StudioCore.ParamEditor;

public class ParamEditorView
{
    private static readonly Vector4 AUXCONFLICTCOLOUR = new(1, 0.7f, 0.7f, 1);
    private static readonly Vector4 AUXADDEDCOLOUR = new(0.7f, 0.7f, 1, 1);
    private static readonly Vector4 PRIMARYCHANGEDCOLOUR = new(0.7f, 1, 0.7f, 1);
    private static readonly Vector4 ALLVANILLACOLOUR = new(0.9f, 0.9f, 0.9f, 1);

    private readonly ParamRowEditor _propEditor;
    private readonly Dictionary<string, string> lastRowSearch = new();
    private bool _arrowKeyPressed;
    private bool _focusRows;
    private int _gotoParamRow = -1;
    private bool _mapParamView;

    internal ParamEditorScreen _paramEditor;

    internal ParamEditorSelectionState _selection;
    internal int _viewIndex;

    private string lastParamSearch = "";

    public ParamEditorView(ParamEditorScreen parent, int index)
    {
        _paramEditor = parent;
        _viewIndex = index;
        _propEditor = new ParamRowEditor(parent.EditorActionManager, _paramEditor);
        _selection = new ParamEditorSelectionState(_paramEditor);
    }

    private void ParamView_ParamList_Header(bool isActiveView)
    {
        if (ParamBank.PrimaryBank.ParamVersion != 0)
        {
            ImGui.Text($"Param version {Utils.ParseParamVersion(ParamBank.PrimaryBank.ParamVersion)}");

            if (_paramEditor.ParamUpgradeVersionSoftWhitelist != 0)
            {
                if (ParamBank.PrimaryBank.ParamVersion < ParamBank.VanillaBank.ParamVersion
                    || _paramEditor.ParamUpgradeVersionSoftWhitelist > ParamBank.PrimaryBank.ParamVersion)
                {
                    ImGui.SameLine();
                    ImGui.Text("(out of date)");
                }
                else if (_paramEditor.ParamUpgradeVersionSoftWhitelist < ParamBank.PrimaryBank.ParamVersion)
                {
                    ImGui.SameLine();
                    ImGui.Text("(unsupported version)");
                }
            }
        }

        if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.Param_SearchParam))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.InputText($"Search <{KeyBindings.Current.Param_SearchParam.HintText}>",
            ref _selection.currentParamSearchString, 256);
        var resAutoParam = AutoFill.ParamSearchBarAutoFill();
        if (resAutoParam != null)
        {
            _selection.SetCurrentRowSearchString(resAutoParam);
        }

        if (!_selection.currentParamSearchString.Equals(lastParamSearch))
        {
            UICache.ClearCaches();
            lastParamSearch = _selection.currentParamSearchString;
        }

        if (ParamBank.PrimaryBank.AssetLocator.Type is GameType.DemonsSouls
            or GameType.DarkSoulsPTDE
            or GameType.DarkSoulsRemastered)
        {
            // This game has DrawParams, add UI element to toggle viewing DrawParam and GameParams.
            if (ImGui.Checkbox("Edit Drawparams", ref _mapParamView))
            {
                UICache.ClearCaches();
            }

            ImGui.Separator();
        }
        else if (ParamBank.PrimaryBank.AssetLocator.Type is GameType.DarkSoulsIISOTFS)
        {
            // DS2 has map params, add UI element to toggle viewing map params and GameParams.
            if (ImGui.Checkbox("Edit Map Params", ref _mapParamView))
            {
                UICache.ClearCaches();
            }

            ImGui.Separator();
        }
    }

    private void ParamView_ParamList_Pinned(float scale)
    {
        List<string> pinnedParamKeyList = new(_paramEditor._projectSettings.PinnedParams);

        if (pinnedParamKeyList.Count > 0)
        {
            //ImGui.Text("        Pinned Params");
            foreach (var paramKey in pinnedParamKeyList)
            {
                HashSet<int> primary = ParamBank.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);
                Param p = ParamBank.PrimaryBank.Params[paramKey];
                if (p != null)
                {
                    ParamMetaData meta = ParamMetaData.Get(p.AppliedParamdef);
                    var Wiki = meta?.Wiki;
                    if (Wiki != null)
                    {
                        if (EditorDecorations.HelpIcon(paramKey + "wiki", ref Wiki, true))
                        {
                            meta.Wiki = Wiki;
                        }
                    }
                }

                ImGui.Indent(15.0f * scale);
                if (ImGui.Selectable(paramKey, paramKey == _selection.GetActiveParam()))
                {
                    EditorCommandQueue.AddCommand($@"param/view/{_viewIndex}/{paramKey}");
                }

                if (ImGui.BeginPopupContextItem())
                {
                    if (ImGui.Selectable("Unpin " + paramKey))
                    {
                        _paramEditor._projectSettings.PinnedParams.Remove(paramKey);
                    }
                    EditorDecorations.PinListReorderOptions(_paramEditor._projectSettings.PinnedParams, paramKey);

                    ImGui.EndPopup();
                }

                ImGui.Unindent(15.0f * scale);
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }
    }

    private void ParamView_ParamList_Main(bool doFocus, float scale, float scrollTo)
    {
        List<string> paramKeyList = UICache.GetCached(_paramEditor, _viewIndex, () =>
        {
            List<(ParamBank, Param)> list =
                ParamSearchEngine.pse.Search(true, _selection.currentParamSearchString, true, true);
            List<string> keyList = list.Where(param => param.Item1 == ParamBank.PrimaryBank)
                .Select(param => ParamBank.PrimaryBank.GetKeyForParam(param.Item2)).ToList();

            if (ParamBank.PrimaryBank.AssetLocator.Type is GameType.DemonsSouls
                or GameType.DarkSoulsPTDE
                or GameType.DarkSoulsRemastered)
            {
                if (_mapParamView)
                {
                    keyList = keyList.FindAll(p => p.EndsWith("Bank"));
                }
                else
                {
                    keyList = keyList.FindAll(p => !p.EndsWith("Bank"));
                }
            }
            else if (ParamBank.PrimaryBank.AssetLocator.Type is GameType.DarkSoulsIISOTFS)
            {
                if (_mapParamView)
                {
                    keyList = keyList.FindAll(p => ParamBank.DS2MapParamlist.Contains(p.Split('_')[0]));
                }
                else
                {
                    keyList = keyList.FindAll(p => !ParamBank.DS2MapParamlist.Contains(p.Split('_')[0]));
                }
            }

            if (CFG.Current.Param_AlphabeticalParams)
            {
                keyList.Sort();
            }

            return keyList;
        });

        foreach (var paramKey in paramKeyList)
        {
            HashSet<int> primary = ParamBank.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);
            Param p = ParamBank.PrimaryBank.Params[paramKey];
            if (p != null)
            {
                ParamMetaData meta = ParamMetaData.Get(p.AppliedParamdef);
                var Wiki = meta?.Wiki;
                if (Wiki != null)
                {
                    if (EditorDecorations.HelpIcon(paramKey + "wiki", ref Wiki, true))
                    {
                        meta.Wiki = Wiki;
                    }
                }
            }

            ImGui.Indent(15.0f * scale);
            if (primary != null ? primary.Any() : false)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, PRIMARYCHANGEDCOLOUR);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ALLVANILLACOLOUR);
            }

            if (ImGui.Selectable($"{paramKey}", paramKey == _selection.GetActiveParam()))
            {
                //_selection.setActiveParam(param.Key);
                EditorCommandQueue.AddCommand($@"param/view/{_viewIndex}/{paramKey}");
            }

            ImGui.PopStyleColor();

            if (doFocus && paramKey == _selection.GetActiveParam())
            {
                scrollTo = ImGui.GetCursorPosY();
            }

            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.Selectable("Pin " + paramKey) &&
                    !_paramEditor._projectSettings.PinnedParams.Contains(paramKey))
                {
                    _paramEditor._projectSettings.PinnedParams.Add(paramKey);
                }

                if (ParamEditorScreen.EditorMode && p != null)
                {
                    ParamMetaData meta = ParamMetaData.Get(p.AppliedParamdef);
                    if (meta != null && meta.Wiki == null && ImGui.MenuItem("Add wiki..."))
                    {
                        meta.Wiki = "Empty wiki...";
                    }

                    if (meta?.Wiki != null && ImGui.MenuItem("Remove wiki"))
                    {
                        meta.Wiki = null;
                    }
                }

                ImGui.EndPopup();
            }

            ImGui.Unindent(15.0f * scale);
        }

        if (doFocus)
        {
            ImGui.SetScrollFromPosY(scrollTo - ImGui.GetScrollY());
        }
    }

    private void ParamView_ParamList(bool doFocus, bool isActiveView, float scale, float scrollTo)
    {
        ParamView_ParamList_Header(isActiveView);

        ParamView_ParamList_Pinned(scale);

        ImGui.BeginChild("paramTypes");
        ParamView_ParamList_Main(doFocus, scale, scrollTo);
        ImGui.EndChild();
    }

    private void ParamView_RowList_Header(ref bool doFocus, bool isActiveView, ref float scrollTo,
        string activeParam)
    {
        scrollTo = 0;

        //Goto ID
        if (ImGui.Button($"Goto ID <{KeyBindings.Current.Param_GotoRowID.HintText}>") ||
            (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.Param_GotoRowID)))
        {
            ImGui.OpenPopup("gotoParamRow");
        }

        if (ImGui.BeginPopup("gotoParamRow"))
        {
            var gotorow = 0;
            ImGui.SetKeyboardFocusHere();
            ImGui.InputInt("Goto Row ID", ref gotorow);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                _gotoParamRow = gotorow;
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        //Row ID/name search
        if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.Param_SearchRow))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.InputText($"Search <{KeyBindings.Current.Param_SearchRow.HintText}>",
            ref _selection.GetCurrentRowSearchString(), 256);
        var resAutoRow = AutoFill.RowSearchBarAutoFill();
        if (resAutoRow != null)
        {
            _selection.SetCurrentRowSearchString(resAutoRow);
        }

        if (!lastRowSearch.ContainsKey(_selection.GetActiveParam()) || !lastRowSearch[_selection.GetActiveParam()]
                .Equals(_selection.GetCurrentRowSearchString()))
        {
            UICache.ClearCaches();
            lastRowSearch[_selection.GetActiveParam()] = _selection.GetCurrentRowSearchString();
            doFocus = true;
        }

        if (ImGui.IsItemActive())
        {
            _paramEditor._isSearchBarActive = true;
        }
        else
        {
            _paramEditor._isSearchBarActive = false;
        }

        UIHints.AddImGuiHintButton("MassEditHint", ref UIHints.SearchBarHint);
    }

    private void ParamView_RowList(bool doFocus, bool isActiveView, float scrollTo, string activeParam)
    {
        if (!_selection.ActiveParamExists())
        {
            ImGui.Text("Select a param to see rows");
        }
        else
        {
            IParamDecorator decorator = null;
            if (_paramEditor._decorators.ContainsKey(activeParam))
            {
                decorator = _paramEditor._decorators[activeParam];
            }

            ParamView_RowList_Header(ref doFocus, isActiveView, ref scrollTo, activeParam);

            Param para = ParamBank.PrimaryBank.Params[activeParam];
            HashSet<int> vanillaDiffCache = ParamBank.PrimaryBank.GetVanillaDiffRows(activeParam);
            List<(HashSet<int>, HashSet<int>)> auxDiffCaches = ParamBank.AuxBanks.Select((bank, i) =>
                (bank.Value.GetVanillaDiffRows(activeParam), bank.Value.GetPrimaryDiffRows(activeParam))).ToList();

            Param.Column compareCol = _selection.GetCompareCol();
            PropertyInfo compareColProp = typeof(Param.Cell).GetProperty("Value");

            //ImGui.BeginChild("rows" + activeParam);
            if (EditorDecorations.ImGuiTableStdColumns("rowList", compareCol == null ? 1 : 2, false))
            {
                List<Param.Row> pinnedRowList = _paramEditor._projectSettings.PinnedRows
                    .GetValueOrDefault(activeParam, new List<int>()).Select(id => para[id]).ToList();

                ImGui.TableSetupColumn("rowCol", ImGuiTableColumnFlags.None, 1f);
                if (compareCol != null)
                {
                    ImGui.TableSetupColumn("rowCol2", ImGuiTableColumnFlags.None, 0.4f);
                    ImGui.TableSetupScrollFreeze(2, 1 + pinnedRowList.Count);
                    if (ImGui.TableNextColumn())
                    {
                        ImGui.Text("ID\t\tName");
                    }

                    if (ImGui.TableNextColumn())
                    {
                        ImGui.Text(compareCol.Def.InternalName);
                    }
                }
                else
                {
                    ImGui.TableSetupScrollFreeze(1, pinnedRowList.Count);
                }

                ImGui.PushID("pinned");

                var selectionCachePins = _selection.GetSelectionCache(pinnedRowList, "pinned");
                if (pinnedRowList.Count != 0)
                {
                    var lastCol = false;
                    for (var i = 0; i < pinnedRowList.Count(); i++)
                    {
                        Param.Row row = pinnedRowList[i];
                        if (row == null)
                        {
                            continue;
                        }

                        lastCol = ParamView_RowList_Entry(selectionCachePins, i, activeParam, null, row,
                            vanillaDiffCache, auxDiffCaches, decorator, ref scrollTo, false, true, compareCol,
                            compareColProp);
                    }

                    if (lastCol)
                    {
                        ImGui.Spacing();
                    }

                    if (EditorDecorations.ImguiTableSeparator())
                    {
                        ImGui.Spacing();
                    }
                }

                ImGui.PopID();

                // Up/Down arrow key input
                if ((InputTracker.GetKey(Key.Up) || InputTracker.GetKey(Key.Down)) && !ImGui.IsAnyItemActive())
                {
                    _arrowKeyPressed = true;
                }

                if (_focusRows)
                {
                    ImGui.SetNextWindowFocus();
                    _arrowKeyPressed = false;
                    _focusRows = false;
                }

                List<Param.Row> rows = UICache.GetCached(_paramEditor, (_viewIndex, activeParam),
                    () => RowSearchEngine.rse.Search((ParamBank.PrimaryBank, para),
                        _selection.GetCurrentRowSearchString(), true, true));

                var enableGrouping = !CFG.Current.Param_DisableRowGrouping && ParamMetaData
                    .Get(ParamBank.PrimaryBank.Params[activeParam].AppliedParamdef).ConsecutiveIDs;

                // Rows
                var selectionCache = _selection.GetSelectionCache(rows, "regular");
                for (var i = 0; i < rows.Count; i++)
                {
                    Param.Row currentRow = rows[i];
                    if (enableGrouping)
                    {
                        Param.Row prev = i - 1 > 0 ? rows[i - 1] : null;
                        Param.Row next = i + 1 < rows.Count ? rows[i + 1] : null;
                        if (prev != null && next != null && prev.ID + 1 != currentRow.ID &&
                            currentRow.ID + 1 == next.ID)
                        {
                            EditorDecorations.ImguiTableSeparator();
                        }

                        ParamView_RowList_Entry(selectionCache, i, activeParam, rows, currentRow, vanillaDiffCache,
                            auxDiffCaches, decorator, ref scrollTo, doFocus, false, compareCol, compareColProp);
                        if (prev != null && next != null && prev.ID + 1 == currentRow.ID &&
                            currentRow.ID + 1 != next.ID)
                        {
                            EditorDecorations.ImguiTableSeparator();
                        }
                    }
                    else
                    {
                        ParamView_RowList_Entry(selectionCache, i, activeParam, rows, currentRow, vanillaDiffCache,
                            auxDiffCaches, decorator, ref scrollTo, doFocus, false, compareCol, compareColProp);
                    }
                }

                if (doFocus)
                {
                    ImGui.SetScrollFromPosY(scrollTo - ImGui.GetScrollY());
                }

                ImGui.EndTable();
            }
            //ImGui.EndChild();
        }
    }

    private void ParamView_FieldList(bool isActiveView, string activeParam, Param.Row activeRow)
    {
        if (activeRow == null)
        {
            ImGui.BeginChild("columnsNONE");
            ImGui.Text("Select a row to see properties");
            ImGui.EndChild();
        }
        else
        {
            ImGui.BeginChild("columns" + activeParam);
            Param vanillaParam = ParamBank.VanillaBank.Params?.GetValueOrDefault(activeParam);
            _propEditor.PropEditorParamRow(
                ParamBank.PrimaryBank,
                activeRow,
                vanillaParam?[activeRow.ID],
                ParamBank.AuxBanks.Select((bank, i) =>
                    (bank.Key, bank.Value.Params?.GetValueOrDefault(activeParam)?[activeRow.ID])).ToList(),
                _selection.GetCompareRow(),
                ref _selection.GetCurrentPropSearchString(),
                activeParam,
                isActiveView,
                _selection);
            ImGui.EndChild();
        }
    }

    public void ParamView(bool doFocus, bool isActiveView)
    {
        var scale = MapStudioNew.GetUIScale();

        if (EditorDecorations.ImGuiTableStdColumns("paramsT", 3, true))
        {
            ImGui.TableSetupColumn("paramsCol", ImGuiTableColumnFlags.None, 0.5f);
            ImGui.TableSetupColumn("paramsCol2", ImGuiTableColumnFlags.None, 0.5f);
            var scrollTo = 0f;
            if (ImGui.TableNextColumn())
            {
                ParamView_ParamList(doFocus, isActiveView, scale, scrollTo);
            }

            var activeParam = _selection.GetActiveParam();
            if (ImGui.TableNextColumn())
            {
                ParamView_RowList(doFocus, isActiveView, scrollTo, activeParam);
            }

            Param.Row activeRow = _selection.GetActiveRow();
            if (ImGui.TableNextColumn())
            {
                ParamView_FieldList(isActiveView, activeParam, activeRow);
            }

            ImGui.EndTable();
        }
    }

    private void ParamView_RowList_Entry_Row(bool[] selectionCache, int selectionCacheIndex, string activeParam,
        List<Param.Row> p, Param.Row r, HashSet<int> vanillaDiffCache,
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, IParamDecorator decorator, ref float scrollTo,
        bool doFocus, bool isPinned)
    {
        var diffVanilla = vanillaDiffCache.Contains(r.ID);
        var auxDiffVanilla = auxDiffCaches.Where(cache => cache.Item1.Contains(r.ID)).Count() > 0;
        if (diffVanilla)
        {
            // If the auxes are changed bu
            var auxDiffPrimaryAndVanilla = (auxDiffVanilla ? 1 : 0) + auxDiffCaches
                .Where(cache => cache.Item1.Contains(r.ID) && cache.Item2.Contains(r.ID)).Count() > 1;
            if (auxDiffVanilla && auxDiffPrimaryAndVanilla)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, AUXCONFLICTCOLOUR);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, PRIMARYCHANGEDCOLOUR);
            }
        }
        else
        {
            if (auxDiffVanilla)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, AUXADDEDCOLOUR);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, ALLVANILLACOLOUR);
            }
        }

        var selected = selectionCache != null && selectionCacheIndex < selectionCache.Length
            ? selectionCache[selectionCacheIndex]
            : false;
        if (_gotoParamRow != -1 && !isPinned)
        {
            // Goto row was activated. As soon as a corresponding ID is found, change selection to it.
            if (r.ID == _gotoParamRow)
            {
                selected = true;
                _selection.SetActiveRow(r, true);
                _gotoParamRow = -1;
                ImGui.SetScrollHereY();
            }
        }

        if (_paramEditor.GotoSelectedRow && !isPinned)
        {
            var activeRow = _selection.GetActiveRow();
            if (activeRow == null)
            {
                _paramEditor.GotoSelectedRow = false;
            }
            else if (activeRow.ID == r.ID)
            {
                ImGui.SetScrollHereY();
                _paramEditor.GotoSelectedRow = false;
            }
        }

        var label = $@"{r.ID} {Utils.ImGuiEscape(r.Name)}";
        label = Utils.ImGui_WordWrapString(label, ImGui.GetColumnWidth(),
            CFG.Current.Param_DisableLineWrapping ? 1 : 3);
        if (ImGui.Selectable($@"{label}##{selectionCacheIndex}", selected))
        {
            _focusRows = true;
            if (InputTracker.GetKey(Key.LControl) || InputTracker.GetKey(Key.RControl))
            {
                _selection.ToggleRowInSelection(r);
            }
            else if (p != null && (InputTracker.GetKey(Key.LShift) || InputTracker.GetKey(Key.RShift)) &&
                     _selection.GetActiveRow() != null)
            {
                _selection.CleanSelectedRows();
                var start = p.IndexOf(_selection.GetActiveRow());
                var end = p.IndexOf(r);
                if (start != end && start != -1 && end != -1)
                {
                    foreach (Param.Row r2 in p.GetRange(start < end ? start : end, Math.Abs(end - start)))
                    {
                        _selection.AddRowToSelection(r2);
                    }
                }

                _selection.AddRowToSelection(r);
            }
            else
            {
                _selection.SetActiveRow(r, true);
            }
        }

        if (_arrowKeyPressed && ImGui.IsItemFocused()
                             && r != _selection.GetActiveRow())
        {
            if (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight))
            {
                // Add to selection
                _selection.AddRowToSelection(r);
            }
            else
            {
                // Exclusive selection
                _selection.SetActiveRow(r, true);
            }

            _arrowKeyPressed = false;
        }

        ImGui.PopStyleColor();

        if (ImGui.BeginPopupContextItem(r.ID.ToString()))
        {
            if (CFG.Current.Param_ShowHotkeysInContextMenu)
            {
                if (ImGui.Selectable(@$"Copy selection ({KeyBindings.Current.Param_Copy.HintText})", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.CopySelectionToClipboard(_selection);
                }

                if (ImGui.Selectable(@$"Paste clipboard ({KeyBindings.Current.Param_Paste.HintText})", false,
                        ParamBank.ClipboardRows.Any() ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
                }

                if (ImGui.Selectable(@$"Delete selection ({KeyBindings.Current.Core_Delete.HintText})", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.DeleteSelection(_selection);
                }

                if (ImGui.Selectable(@$"Duplicate selection ({KeyBindings.Current.Core_Duplicate.HintText})", false,
                        _selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    _paramEditor.DuplicateSelection(_selection);
                }

                ImGui.Separator();
            }

            if (ImGui.Selectable((isPinned ? "Unpin " : "Pin ") + r.ID))
            {
                if (!_paramEditor._projectSettings.PinnedRows.ContainsKey(activeParam))
                {
                    _paramEditor._projectSettings.PinnedRows.Add(activeParam, new List<int>());
                }

                List<int> pinned = _paramEditor._projectSettings.PinnedRows[activeParam];
                if (isPinned)
                {
                    pinned.Remove(r.ID);
                }
                else if (!pinned.Contains(r.ID))
                {
                    pinned.Add(r.ID);
                }
            }
            if (isPinned)
            {
                EditorDecorations.PinListReorderOptions(_paramEditor._projectSettings.PinnedRows[activeParam], r.ID);
            }
            ImGui.Separator();

            if (decorator != null)
            {
                decorator.DecorateContextMenuItems(r);
            }

            if (ImGui.Selectable("Compare..."))
            {
                _selection.SetCompareRow(r);
            }

            EditorDecorations.ParamRefReverseLookupSelectables(_paramEditor, ParamBank.PrimaryBank, activeParam,
                r.ID);
            if (ImGui.Selectable("Copy ID to clipboard"))
            {
                PlatformUtils.Instance.SetClipboardText($"{r.ID}");
            }

            ImGui.EndPopup();
        }

        if (decorator != null)
        {
            decorator.DecorateParam(r);
        }

        if (doFocus && _selection.GetActiveRow() == r)
        {
            scrollTo = ImGui.GetCursorPosY();
        }
    }

    private bool ParamView_RowList_Entry(bool[] selectionCache, int selectionCacheIndex, string activeParam,
        List<Param.Row> p, Param.Row r, HashSet<int> vanillaDiffCache,
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, IParamDecorator decorator, ref float scrollTo,
        bool doFocus, bool isPinned, Param.Column compareCol, PropertyInfo compareColProp)
    {
        var scale = MapStudioNew.GetUIScale();

        if (CFG.Current.UI_CompactParams)
        {
            // ItemSpacing only affects clickable area for selectables in tables. Add additional height to prevent gaps between selectables.
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 2.0f) * scale);
        }

        var lastCol = false;
        if (ImGui.TableNextColumn())
        {
            ParamView_RowList_Entry_Row(selectionCache, selectionCacheIndex, activeParam, p, r, vanillaDiffCache,
                auxDiffCaches, decorator, ref scrollTo, doFocus, isPinned);
            lastCol = true;
        }

        if (compareCol != null)
        {
            if (ImGui.TableNextColumn())
            {
                Param.Cell c = r[compareCol];
                object newval = null;
                ImGui.PushID("compareCol_" + selectionCacheIndex);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));
                ParamEditorCommon.PropertyField(compareCol.ValueType, c.Value, ref newval, false);
                if (ParamEditorCommon.UpdateProperty(_propEditor.ContextActionManager, c, compareColProp,
                        c.Value) && !ParamBank.VanillaBank.IsLoadingParams)
                {
                    ParamBank.PrimaryBank.RefreshParamRowDiffs(r, activeParam);
                }

                ImGui.PopStyleVar();
                ImGui.PopID();
                lastCol = true;
            }
            else
            {
                lastCol = false;
            }
        }

        if (CFG.Current.UI_CompactParams)
        {
            ImGui.PopStyleVar();
        }

        return lastCol;
    }
}
