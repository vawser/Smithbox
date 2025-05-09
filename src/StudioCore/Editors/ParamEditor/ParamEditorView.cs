﻿using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using Octokit;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Editors.ParamEditor.Decorators;
using StudioCore.Editors.ParamEditor.MassEdit;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Tools.Generation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Veldrid;

namespace StudioCore.Editors.ParamEditor;

public class ParamEditorView
{
    internal ParamEditorScreen Editor;

    private readonly ParamRowEditor _propEditor;
    private readonly Dictionary<string, string> lastRowSearch = new();
    private bool _arrowKeyPressed;
    private bool _focusRows;
    private int _gotoParamRow = -1;

    internal ParamEditorSelectionState _selection;
    internal int _viewIndex;

    private string lastParamSearch = "";

    public ParamEditorView(ParamEditorScreen editor, int index)
    {
        Editor = editor;
        _viewIndex = index;
        _propEditor = new ParamRowEditor(editor.EditorActionManager, Editor);
        _selection = new ParamEditorSelectionState(Editor);
    }

    //------------------------------------
    // Param
    //------------------------------------
    private void ParamView_ParamList_Header(bool isActiveView)
    {
        ImGui.Text("Params");

        // Param Version
        if (Editor.Project.ParamData.PrimaryBank.ParamVersion != 0)
        {
            ImGui.SameLine();
            ImGui.Text($"- Version {Utils.ParseParamVersion(Editor.Project.ParamData.PrimaryBank.ParamVersion)}");

            if (Editor.Project.ParamData.PrimaryBank.ParamVersion < Editor.Project.ParamData.VanillaBank.ParamVersion)
            {
                ImGui.SameLine();
                UIHelper.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, "(out of date)");
            }
        }

        ImGui.Separator();

        // Autofill
        ImGui.AlignTextToFramePadding();
        var resAutoParam = AutoFill.ParamSearchBarAutoFill(Editor);

        if (resAutoParam != null)
        {
            _selection.SetCurrentParamSearchString(resAutoParam);
        }

        ImGui.SameLine();

        // Search param
        if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SearchParam))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputText($"##paramSearch",
            ref _selection.currentParamSearchString, 256);
        UIHelper.Tooltip($"Search <{KeyBindings.Current.PARAM_SearchParam.HintText}>");

        if (!_selection.currentParamSearchString.Equals(lastParamSearch))
        {
            UICache.ClearCaches();
            lastParamSearch = _selection.currentParamSearchString;
        }

        ImGui.Separator();
    }

    private void ParamView_ParamList_Pinned(float scale)
    {
        List<string> pinnedParamKeyList = new(Editor.Project.PinnedParams);

        if (pinnedParamKeyList.Count > 0)
        {
            //ImGui.Text("        Pinned Params");
            foreach (var paramKey in pinnedParamKeyList)
            {
                HashSet<int> primary = Editor.Project.ParamData.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);

                if (Editor.Project.ParamData.PrimaryBank.Params.ContainsKey(paramKey))
                {
                    Param p = Editor.Project.ParamData.PrimaryBank.Params[paramKey];
                    if (p != null)
                    {
                        var meta = Editor.Project.ParamData.GetParamMeta(p.AppliedParamdef);
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
                    if (ImGui.Selectable($"{paramKey}##pin{paramKey}", paramKey == _selection.GetActiveParam()))
                    {
                        EditorCommandQueue.AddCommand($@"param/view/{_viewIndex}/{paramKey}");
                    }

                    ParamEntryContextMenu.Display(Editor, p, paramKey, true);

                    ImGui.Unindent(15.0f * scale);
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
        }
    }

    private void ParamView_ParamList_Main(bool doFocus, float scale, float scrollTo)
    {
        List<string> paramKeyList = UICache.GetCached(Editor, _viewIndex, () =>
        {
            List<(ParamBank, Param)> list =
                ParamSearchEngine.Create(Editor).Search(true, _selection.currentParamSearchString, true, true);

            if (list != null)
            {
                var keyList = list.Where(param => param.Item1 == Editor.Project.ParamData.PrimaryBank)
                    .Select(param => Editor.Project.ParamData.PrimaryBank.GetKeyForParam(param.Item2)).ToList();

                if (CFG.Current.Param_AlphabeticalParams)
                {
                    keyList.Sort();
                }

                return keyList;
            }
            else
            {
                var keyList = Editor.Project.ParamData.PrimaryBank.Params.Select(e => e.Key).ToList();
            }

            return new List<string>();
        });


        var categoryObj = Editor.Project.ParamCategories;
        var categories = Editor.Project.ParamCategories.Categories;

        if (categories != null && CFG.Current.Param_DisplayParamCategories)
        {
            var generalParamList = new List<string>();

            // Build a general list from all unassigned params
            foreach (var paramKey in paramKeyList)
            {
                bool add = true;

                foreach (var category in categories)
                {
                    foreach (var entry in category.Params)
                    {
                        if (entry == paramKey)
                            add = false;
                    }
                }

                if (add)
                    generalParamList.Add(paramKey);
            }

            // TODO: perhaps add an actual ordering system to the ParamCategories (using SortID) instead of this crude boolean method
            if (categories.Count > 0)
            {
                // Categories - Forced Top
                foreach (var category in categories)
                {
                    if (category.ForceTop)
                    {
                        if (ImGui.CollapsingHeader($"{category.DisplayName}", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            DisplayParamList(paramKeyList, category.Params, doFocus, scale, scrollTo);
                        }
                    }
                }

                // General List
                if (ImGui.CollapsingHeader($"General", ImGuiTreeNodeFlags.DefaultOpen))
                {
                    DisplayParamList(paramKeyList, generalParamList, doFocus, scale, scrollTo);
                }

                // Categories - Default
                foreach (var category in categories)
                {
                    if (!category.ForceTop && !category.ForceBottom)
                    {
                        if (ImGui.CollapsingHeader($"{category.DisplayName}", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            DisplayParamList(paramKeyList, category.Params, doFocus, scale, scrollTo);
                        }
                    }
                }

                // Categories - Forced Bottom
                foreach (var category in categories)
                {
                    if (category.ForceBottom)
                    {
                        if (ImGui.CollapsingHeader($"{category.DisplayName}", ImGuiTreeNodeFlags.DefaultOpen))
                        {
                            DisplayParamList(paramKeyList, category.Params, doFocus, scale, scrollTo);
                        }
                    }
                }
            }
            else
            {
                // Fallback to full view
                DisplayParamList(paramKeyList, paramKeyList, doFocus, scale, scrollTo);
            }
        }
        else
        {
            // Fallback to full view
            DisplayParamList(paramKeyList, paramKeyList, doFocus, scale, scrollTo);
        }
    }

    public void DisplayParamList(List<string> paramKeyList, List<string> visibleParams, bool doFocus, float scale, float scrollTo)
    {
        foreach (var paramKey in paramKeyList)
        {
            HashSet<int> primary = Editor.Project.ParamData.PrimaryBank.VanillaDiffCache.GetValueOrDefault(paramKey, null);
            Param p = Editor.Project.ParamData.PrimaryBank.Params[paramKey];

            if (!visibleParams.Contains(paramKey))
                continue;

            if (p != null)
            {
                var meta = Editor.Project.ParamData.GetParamMeta(p.AppliedParamdef);

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
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_PrimaryChanged_Text);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            }

            var displayedName = paramKey;

            if (CFG.Current.Param_ShowParamCommunityName)
            {
                var meta = Editor.Project.ParamData.GetParamMeta(p.AppliedParamdef);
                var names = meta?.DisplayNames;

                if (names != null)
                {
                    var paramDisplayName = names.Where(e => e.Param == paramKey).FirstOrDefault();

                    if(paramDisplayName != null)
                    {
                        displayedName = paramDisplayName.Name;
                    }
                }
            }

            if (ImGui.Selectable($"{displayedName}##{paramKey}", paramKey == _selection.GetActiveParam()))
            {
                //_selection.setActiveParam(param.Key);
                EditorCommandQueue.AddCommand($@"param/view/{_viewIndex}/{paramKey}");
            }

            ImGui.PopStyleColor();

            if (doFocus && paramKey == _selection.GetActiveParam())
            {
                scrollTo = ImGui.GetCursorPosY();
            }

            // Context Menu
            ParamEntryContextMenu.Display(Editor, p, paramKey);

            ImGui.Unindent(15.0f * scale);
        }

        if (doFocus)
        {
            ImGui.SetScrollFromPosY(scrollTo - ImGui.GetScrollY());
        }
    }

    private void DisplayDS2MapNameAlias(string paramKey)
    {
        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            Regex pattern = new Regex(@"(m[0-9]{2}_[0-9]{2}_[0-9]{2}_[0-9]{2})");
            Match match = pattern.Match(paramKey);

            var alias = "";

            if (match.Captures.Count > 0)
            {
                var matchResult = Editor.Project.Aliases.MapNames.Where(e => e.ID == match.Captures[0].Value).FirstOrDefault();

                if (matchResult != null)
                {
                    alias = matchResult.Name;
                }
            }

            UIHelper.DisplayAlias(alias);
        }
    }

    private void ParamView_ParamList(bool doFocus, bool isActiveView, float scale, float scrollTo)
    {
        ParamView_ParamList_Header(isActiveView);

        if(CFG.Current.Param_PinnedRowsStayVisible)
        {
            ParamView_ParamList_Pinned(scale);
        }

        ImGui.BeginChild("paramTypes");

        if (!CFG.Current.Param_PinnedRowsStayVisible)
        {
            ParamView_ParamList_Pinned(scale);
        }

        if (!CFG.Current.Param_PinGroups_ShowOnlyPinnedParams)
        {
            ParamView_ParamList_Main(doFocus, scale, scrollTo);
        }

        ImGui.EndChild();
    }
    //------------------------------------
    // Row
    //------------------------------------
    private void ParamView_RowList_Header(ref bool doFocus, bool isActiveView, ref float scrollTo,
        string activeParam)
    {
        ImGui.Text("Rows");
        ImGui.Separator();

        scrollTo = 0;

        // Auto fill
        ImGui.AlignTextToFramePadding();
        var resAutoRow = AutoFill.RowSearchBarAutoFill(Editor);

        if (resAutoRow != null)
        {
            _selection.SetCurrentRowSearchString(resAutoRow);
        }

        ImGui.SameLine();

        // Row Search
        if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SearchRow))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputText($"##rowSearch",
            ref _selection.GetCurrentRowSearchString(), 256);
        UIHelper.Tooltip($"Search <{KeyBindings.Current.PARAM_SearchRow.HintText}>");

        if (!lastRowSearch.ContainsKey(_selection.GetActiveParam()) || !lastRowSearch[_selection.GetActiveParam()]
                .Equals(_selection.GetCurrentRowSearchString()))
        {
            UICache.ClearCaches();
            lastRowSearch[_selection.GetActiveParam()] = _selection.GetCurrentRowSearchString();
            doFocus = true;
        }

        if (ImGui.IsItemActive())
        {
            Editor._isSearchBarActive = true;
        }
        else
        {
            Editor._isSearchBarActive = false;
        }

        ImGui.SameLine();

        // Go to selected
        ImGui.AlignTextToFramePadding();
        if (ImGui.Button($"{Icons.LocationArrow}") ||
            isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_GoToRowID))
        {
            Editor.GotoSelectedRow = true;
        }
        UIHelper.Tooltip($"Go to selected <{KeyBindings.Current.PARAM_GoToSelectedRow.HintText}>");

        ImGui.SameLine();

        // Go to ID
        ImGui.AlignTextToFramePadding();
        if (ImGui.Button($"{Icons.InfoCircle}") ||
            isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_GoToRowID))
        {
            ImGui.OpenPopup("gotoParamRow");
        }
        UIHelper.Tooltip($"Go to ID <{KeyBindings.Current.PARAM_GoToRowID.HintText}>");

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

        ImGui.SameLine();

        // Mass Edit Hint
        ImGui.AlignTextToFramePadding();

        if (ImGui.Button($"{Icons.QuestionCircle}"))
        {
            ImGui.OpenPopup("massEditHint");
        }
        UIHelper.Tooltip(UIHints.SearchBarHint);

        if (ImGui.BeginPopup("massEditHint"))
        {
            ImGui.Text(UIHints.SearchBarHint);

            ImGui.EndPopup();
        }

        ImGui.Separator();
    }

    private void ParamView_RowList(bool doFocus, bool isActiveView, float scrollTo, string activeParam)
    {
        if (!_selection.ActiveParamExists())
        {
            ImGui.Text("Select a param to see rows");
        }
        else
        {
            var fmgDecorator = Editor.DecoratorHandler.GetFmgRowDecorator(activeParam);

            ParamView_RowList_Header(ref doFocus, isActiveView, ref scrollTo, activeParam);

            Param para = Editor.Project.ParamData.PrimaryBank.Params[activeParam];

            HashSet<int> vanillaDiffCache = Editor.Project.ParamData.PrimaryBank.GetVanillaDiffRows(activeParam);

            var auxDiffCaches = Editor.Project.ParamData.AuxBanks.Select((bank, i) =>
                (bank.Value.GetVanillaDiffRows(activeParam), bank.Value.GetPrimaryDiffRows(activeParam))).ToList();

            Param.Column compareCol = _selection.GetCompareCol();
            PropertyInfo compareColProp = typeof(Param.Cell).GetProperty("Value");

            //ImGui.BeginChild("rows" + activeParam);
            if (EditorDecorations.ImGuiTableStdColumns("rowList", compareCol == null ? 1 : 2, false))
            {
                var curParam = Editor.Project.ParamData.PrimaryBank.Params[activeParam];
                var meta = Editor.Project.ParamData.GetParamMeta(curParam.AppliedParamdef);

                var pinnedRowList = Editor.Project.PinnedRows
                    .GetValueOrDefault(activeParam, new List<int>()).Select(id => para[id]).ToList();

                ImGui.TableSetupColumn("rowCol", ImGuiTableColumnFlags.None, 1f);
                if (compareCol != null)
                {
                    ImGui.TableSetupColumn("rowCol2", ImGuiTableColumnFlags.None, 0.4f);
                    if (CFG.Current.Param_PinnedRowsStayVisible)
                    {
                        ImGui.TableSetupScrollFreeze(2, 1 + pinnedRowList.Count);
                    }
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
                    if (CFG.Current.Param_PinnedRowsStayVisible)
                    {
                        ImGui.TableSetupScrollFreeze(1, pinnedRowList.Count);
                    }
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
                            vanillaDiffCache, auxDiffCaches, fmgDecorator, ref scrollTo, false, true, compareCol,
                            compareColProp, meta);
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

                List<Param.Row> rows = UICache.GetCached(Editor, (_viewIndex, activeParam),
                    () => RowSearchEngine.Create(Editor).Search((Editor.Project.ParamData.PrimaryBank, para),

                        _selection.GetCurrentRowSearchString(), true, true));

                var enableGrouping = !CFG.Current.Param_DisableRowGrouping && meta.ConsecutiveIDs;

                // Rows
                var selectionCache = _selection.GetSelectionCache(rows, "regular");
                
                if (!CFG.Current.Param_PinGroups_ShowOnlyPinnedRows)
                {
                    for (var i = 0; i < rows.Count; i++)
                    {
                        Param.Row currentRow = rows[i];

                        // Display groupings if ConsecutiveIDs is set in the meta for the current param.
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
                                auxDiffCaches, fmgDecorator, ref scrollTo, doFocus, false, compareCol, compareColProp,
                                meta);

                            if (prev != null && next != null && prev.ID + 1 == currentRow.ID &&
                                currentRow.ID + 1 != next.ID)
                            {
                                EditorDecorations.ImguiTableSeparator();
                            }
                        }
                        else
                        {
                            ParamView_RowList_Entry(selectionCache, i, activeParam, rows, currentRow, vanillaDiffCache,
                                auxDiffCaches, fmgDecorator, ref scrollTo, doFocus, false, compareCol, compareColProp,
                                meta);
                        }
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

    //------------------------------------
    // Field
    //------------------------------------
    private void ParamView_FieldList_Header(bool isActiveView, string activeParam, Param.Row activeRow)
    {
        ImGui.Text("Fields");
        ImGui.Separator();
    }

    private void ParamView_FieldList(bool isActiveView, string activeParam, Param.Row activeRow)
    {
        ParamView_FieldList_Header(isActiveView, activeParam, activeRow);

        if (activeRow == null)
        {
            ImGui.BeginChild("columnsNONE");
            ImGui.Text("Select a row to see properties");
            ImGui.EndChild();
        }
        else
        {
            ImGui.BeginChild("columns" + activeParam);
            Param vanillaParam = Editor.Project.ParamData.VanillaBank.Params?.GetValueOrDefault(activeParam);

            _propEditor.PropEditorParamRow(
                Editor.Project.ParamData.PrimaryBank,
                activeRow,
                vanillaParam?[activeRow.ID],
                Editor.Project.ParamData.AuxBanks.Select((bank, i) =>
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
        var scale = DPI.GetUIScale();

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
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, FmgRowDecorator fmgDecorator, ref float scrollTo,
        bool doFocus, bool isPinned, ParamMeta? meta)
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
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AuxConflict_Text);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_PrimaryChanged_Text);
            }
        }
        else
        {
            if (auxDiffVanilla)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AuxAdded_Text);
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
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

        if (Editor.GotoSelectedRow && !isPinned)
        {
            var activeRow = _selection.GetActiveRow();

            if (activeRow == null)
            {
                Editor.GotoSelectedRow = false;
            }
            else if (activeRow.ID == r.ID)
            {
                ImGui.SetScrollHereY();
                Editor.GotoSelectedRow = false;
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
            else if (p != null && (InputTracker.GetKey(Key.LShift) || InputTracker.GetKey(Key.RShift)) && _selection.GetActiveRow() != null)
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

        if (_arrowKeyPressed && ImGui.IsItemFocused() && r != _selection.GetActiveRow())
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

        ParamRowContextMenu.Display(
            Editor,
            r, selectionCacheIndex, 
            isPinned, activeParam, 
            fmgDecorator, _selection, Editor);

        if (fmgDecorator != null)
        {
            fmgDecorator.DecorateParam(r);
        }

        if (doFocus && _selection.GetActiveRow() == r)
        {
            scrollTo = ImGui.GetCursorPosY();
        }
    }

    private bool ParamView_RowList_Entry(bool[] selectionCache, int selectionCacheIndex, string activeParam,
        List<Param.Row> p, Param.Row r, HashSet<int> vanillaDiffCache,
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, FmgRowDecorator fmgDecorator, ref float scrollTo,
        bool doFocus, bool isPinned, Param.Column compareCol, PropertyInfo compareColProp, ParamMeta? meta)
    {
        var scale = DPI.GetUIScale();

        if (CFG.Current.UI_CompactParams)
        {
            // ItemSpacing only affects clickable area for selectables in tables. Add additional height to prevent gaps between selectables.
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 2.0f) * scale);
        }

        var lastCol = false;

        if (ImGui.TableNextColumn())
        {
            ParamView_RowList_Entry_Row(selectionCache, selectionCacheIndex, activeParam, p, r, vanillaDiffCache,
                auxDiffCaches, fmgDecorator, ref scrollTo, doFocus, isPinned, meta);
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
                ParamEditorCommon.PropertyField(compareCol.ValueType, c.Value, ref newval, false, false);

                if (ParamEditorCommon.UpdateProperty(_propEditor.ContextActionManager, c, compareColProp,
                        c.Value))
                {
                    Editor.Project.ParamData.PrimaryBank.RefreshParamRowDiffs(Editor, r, activeParam);
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
