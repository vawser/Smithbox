using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Decorators;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// The row column within a Param Editor view
/// </summary>
public class ParamRowView
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamEditorView View;

    public readonly Dictionary<string, string> lastRowSearch = new();
    public bool _arrowKeyPressed;
    public bool _focusRows;
    public int _gotoParamRow = -1;

    public ParamRowView(ParamEditorScreen editor, ProjectEntry project, ParamEditorView view)
    {
        Editor = editor;
        Project = project;
        View = view;
    }

    /// <summary>
    /// Entry point for the rows column.
    /// </summary>
    /// <param name="doFocus"></param>
    /// <param name="isActiveView"></param>
    /// <param name="scrollTo"></param>
    /// <param name="activeParam"></param>
    public void Display(bool doFocus, bool isActiveView, float scrollTo, string activeParam)
    {
        if (!View.Selection.ActiveParamExists())
        {
            ImGui.Text("Select a param to see rows");
        }
        else
        {
            var fmgDecorator = Editor.DecoratorHandler.GetFmgRowDecorator(activeParam);

            DisplayHeader(ref doFocus, isActiveView, ref scrollTo, activeParam);

            Param para = Editor.Project.ParamData.PrimaryBank.Params[activeParam];

            HashSet<int> vanillaDiffCache = Editor.Project.ParamData.PrimaryBank.GetVanillaDiffRows(activeParam);

            var auxDiffCaches = Editor.Project.ParamData.AuxBanks.Select((bank, i) =>
                (bank.Value.GetVanillaDiffRows(activeParam), bank.Value.GetPrimaryDiffRows(activeParam))).ToList();

            Param.Column compareCol = View.Selection.GetCompareCol();
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

                var selectionCachePins = View.Selection.GetSelectionCache(pinnedRowList, "pinned");
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

                        lastCol = HandleRowPresentation(selectionCachePins, i, activeParam, null, row,
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

                List<Param.Row> rows = UICache.GetCached(Editor, (View.ViewIndex, activeParam),
                    () => Editor.MassEditHandler.rse.Search((Editor.Project.ParamData.PrimaryBank, para),

                View.Selection.GetCurrentRowSearchString(), true, true));

                var enableGrouping = !CFG.Current.Param_DisableRowGrouping && meta.ConsecutiveIDs;

                // Rows
                var selectionCache = View.Selection.GetSelectionCache(rows, "regular");

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

                            HandleRowPresentation(selectionCache, i, activeParam, rows, currentRow, vanillaDiffCache,
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
                            HandleRowPresentation(selectionCache, i, activeParam, rows, currentRow, vanillaDiffCache,
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

    /// <summary>
    /// The header for the Rows column.
    /// </summary>
    /// <param name="doFocus"></param>
    /// <param name="isActiveView"></param>
    /// <param name="scrollTo"></param>
    /// <param name="activeParam"></param>
    private void DisplayHeader(ref bool doFocus, bool isActiveView, ref float scrollTo,
        string activeParam)
    {
        ImGui.Text("Rows");
        ImGui.Separator();

        scrollTo = 0;

        // Auto fill
        if (Editor.MassEditHandler.AutoFill != null)
        {
            ImGui.AlignTextToFramePadding();
            var resAutoRow = Editor.MassEditHandler.AutoFill.RowSearchBarAutoFill();

            if (resAutoRow != null)
            {
                View.Selection.SetCurrentRowSearchString(resAutoRow);
            }
        }

        ImGui.SameLine();

        // Row Search
        if (isActiveView && InputTracker.GetKeyDown(KeyBindings.Current.PARAM_SearchRow))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputText($"##rowSearch", ref View.Selection.GetCurrentRowSearchString(), 256);
        UIHelper.Tooltip($"Search <{KeyBindings.Current.PARAM_SearchRow.HintText}>");

        if (!lastRowSearch.ContainsKey(View.Selection.GetActiveParam()) ||
            !lastRowSearch[View.Selection.GetActiveParam()].Equals(View.Selection.GetCurrentRowSearchString()))
        {
            UICache.ClearCaches();
            lastRowSearch[View.Selection.GetActiveParam()] = View.Selection.GetCurrentRowSearchString();
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

    /// <summary>
    /// The handling of a row.
    /// </summary>
    /// <param name="selectionCache"></param>
    /// <param name="selectionCacheIndex"></param>
    /// <param name="activeParam"></param>
    /// <param name="p"></param>
    /// <param name="r"></param>
    /// <param name="vanillaDiffCache"></param>
    /// <param name="auxDiffCaches"></param>
    /// <param name="fmgDecorator"></param>
    /// <param name="scrollTo"></param>
    /// <param name="doFocus"></param>
    /// <param name="isPinned"></param>
    /// <param name="meta"></param>
    private void DisplayRow(bool[] selectionCache, int selectionCacheIndex, string activeParam,
        List<Param.Row> p, Param.Row r, HashSet<int> vanillaDiffCache,
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, FmgRowDecorator fmgDecorator, ref float scrollTo,
        bool doFocus, bool isPinned, ParamMeta meta)
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
                View.Selection.SetActiveRow(r, true);
                _gotoParamRow = -1;
                ImGui.SetScrollHereY();
            }
        }

        if (Editor.GotoSelectedRow && !isPinned)
        {
            var activeRow = View.Selection.GetActiveRow();

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
                View.Selection.ToggleRowInSelection(r);
            }
            else if (p != null && (InputTracker.GetKey(Key.LShift) || InputTracker.GetKey(Key.RShift)) && View.Selection.GetActiveRow() != null)
            {
                View.Selection.CleanSelectedRows();
                var start = p.IndexOf(View.Selection.GetActiveRow());
                var end = p.IndexOf(r);

                if (start != end && start != -1 && end != -1)
                {
                    foreach (Param.Row r2 in p.GetRange(start < end ? start : end, Math.Abs(end - start)))
                    {
                        View.Selection.AddRowToSelection(r2);
                    }
                }

                View.Selection.AddRowToSelection(r);
            }
            else
            {
                View.Selection.SetActiveRow(r, true);
            }
        }

        if (_arrowKeyPressed && ImGui.IsItemFocused() && r != View.Selection.GetActiveRow())
        {
            if (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight))
            {
                // Add to selection
                View.Selection.AddRowToSelection(r);
            }
            else
            {
                // Exclusive selection
                View.Selection.SetActiveRow(r, true);
            }

            _arrowKeyPressed = false;
        }

        ImGui.PopStyleColor();

        DisplayContextMenu(r, selectionCacheIndex, isPinned, activeParam, fmgDecorator);

        if (fmgDecorator != null)
        {
            fmgDecorator.DecorateParam(r);
        }

        if (doFocus && View.Selection.GetActiveRow() == r)
        {
            scrollTo = ImGui.GetCursorPosY();
        }
    }

    /// <summary>
    /// Handles the compare column aspect -> display rows
    /// </summary>
    /// <param name="selectionCache"></param>
    /// <param name="selectionCacheIndex"></param>
    /// <param name="activeParam"></param>
    /// <param name="p"></param>
    /// <param name="r"></param>
    /// <param name="vanillaDiffCache"></param>
    /// <param name="auxDiffCaches"></param>
    /// <param name="fmgDecorator"></param>
    /// <param name="scrollTo"></param>
    /// <param name="doFocus"></param>
    /// <param name="isPinned"></param>
    /// <param name="compareCol"></param>
    /// <param name="compareColProp"></param>
    /// <param name="meta"></param>
    /// <returns></returns>
    private bool HandleRowPresentation(bool[] selectionCache, int selectionCacheIndex, string activeParam,
        List<Param.Row> p, Param.Row r, HashSet<int> vanillaDiffCache,
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, FmgRowDecorator fmgDecorator, ref float scrollTo,
        bool doFocus, bool isPinned, Param.Column compareCol, PropertyInfo compareColProp, ParamMeta meta)
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
            DisplayRow(selectionCache, selectionCacheIndex, activeParam, p, r, vanillaDiffCache,
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

                ParamFieldInput.DisplayFieldInput(compareCol.ValueType, c.Value, ref newval, false, false);

                if (ParamFieldInput.UpdateProperty(Editor.EditorActionManager, c, compareColProp,
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

    /// <summary>
    /// The context menu for a Row entry.
    /// </summary>
    /// <param name="r"></param>
    /// <param name="selectionCacheIndex"></param>
    /// <param name="isPinned"></param>
    /// <param name="activeParam"></param>
    /// <param name="fmgDecorator"></param>
    public void DisplayContextMenu(Param.Row r, int selectionCacheIndex, bool isPinned, string activeParam, FmgRowDecorator fmgDecorator)
    {
        if (ImGui.BeginPopupContextItem($"{r.ID}_{selectionCacheIndex}"))
        {
            var width = CFG.Current.Param_RowContextMenu_Width;

            ImGui.SetNextItemWidth(width);

            // Name Input
            if (CFG.Current.Param_RowContextMenu_NameInput)
            {
                if (View.Selection.RowSelectionExists())
                {
                    var name = View.Selection.GetActiveRow().Name;
                    if (name != null)
                    {
                        ImGui.InputText("##nameMassEdit", ref name, 255);

                        if (ImGui.IsItemDeactivatedAfterEdit())
                        {
                            var editCommand = $"selection: Name := {name}";
                            Editor._activeView.Selection.SortSelection();

                            Editor.MassEditHandler.ApplyMassEdit(editCommand);
                        }
                    }
                }
            }

            // General Options
            if (CFG.Current.Param_RowContextMenu_ShortcutTools)
            {
                // Copy
                if (ImGui.Selectable(@$"Copy", false,
                        View.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    Editor.CopySelectionToClipboard(View.Selection);
                }
                UIHelper.Tooltip($"Shortcut: {KeyBindings.Current.PARAM_CopyToClipboard.HintText}\n\n" +
                    "Copy the current row selection to the clipboard.");

                // Paste
                if (ImGui.Selectable(@$"Paste", false,
                        Editor.Project.ParamData.PrimaryBank.ClipboardRows.Any() ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                {
                    EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
                }
                UIHelper.Tooltip($"Shortcut: {KeyBindings.Current.PARAM_PasteClipboard.HintText}\n\n" +
                    "Paste the current row clipboard into the current param.");

                // Delete
                if (ImGui.Selectable(@$"Delete", false,
                        View.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    Editor.DeleteSelection(View.Selection);
                }
                UIHelper.Tooltip($"Shortcut: {KeyBindings.Current.CORE_DeleteSelectedEntry.HintText}\n\n" +
                    "Delete the current row selection from the param.");

                // Duplicate
                if (ImGui.Selectable(@$"Duplicate", false,
                        View.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    Editor.ParamTools.DuplicateRow();
                }
                UIHelper.Tooltip($"Shortcut: {KeyBindings.Current.CORE_DuplicateSelectedEntry.HintText}\n\n" +
                    "Duplicate the current row selection, automatically incrementing the row ID.");

                // Duplicate To
                if (ImGui.BeginMenu("Duplicate To", Editor.ParamTools.IsCommutativeParam()))
                {
                    Editor.ParamTools.DisplayCommutativeDropDownMenu();

                    ImGui.EndMenu();
                }
                UIHelper.Tooltip($"Duplicate the current row selection into the chosen target param.");

                // Copy ID
                if (ImGui.Selectable(@$"Copy ID", false,
                        View.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    Editor.ParamTools.CopyRowDetails(false);
                }
                UIHelper.Tooltip($"Shortcut: {KeyBindings.Current.PARAM_CopyId.HintText}\n\n" +
                    "Copy the current row selection ID to the clipboard (multiple rows will produce a list of IDs).");

                // Copy ID and Name
                if (ImGui.Selectable(@$"Copy ID and Name", false,
                        View.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    Editor.ParamTools.CopyRowDetails(true);
                }
                UIHelper.Tooltip($"Shortcut: {KeyBindings.Current.PARAM_CopyIdAndName.HintText}\n\n" +
                    "Copy the current row selection ID and Name to the clipboard (multiple rows will produce a list of IDs and Names).");

                // Revert to Default
                if (ImGui.Selectable(@$"Revert to Default", false,
                        View.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                {
                    Editor.ParamTools.SetRowToDefault();
                }
                UIHelper.Tooltip($"Revert the current row selection field values to the vanilla field values.");

                ImGui.Separator();
            }

            // Pinning Options
            if (CFG.Current.Param_RowContextMenu_PinOptions)
            {
                // Pin
                if (!isPinned)
                {
                    if (ImGui.Selectable($"Pin"))
                    {
                        if (!Editor.Project.PinnedRows.ContainsKey(activeParam))
                        {
                            Editor.Project.PinnedRows.Add(activeParam, new List<int>());
                        }

                        List<int> pinned = Editor.Project.PinnedRows[activeParam];

                        foreach (var entry in View.Selection.GetSelectedRows())
                        {
                            if (!pinned.Contains(entry.ID))
                            {
                                pinned.Add(entry.ID);
                            }
                        }
                    }
                    UIHelper.Tooltip($"Pin the current row selection to the top of the row list.");
                }
                // Unpin
                else if (isPinned)
                {
                    if (ImGui.Selectable($"Unpin"))
                    {
                        if (!Editor.Project.PinnedRows.ContainsKey(activeParam))
                        {
                            Editor.Project.PinnedRows.Add(activeParam, new List<int>());
                        }

                        List<int> pinned = Editor.Project.PinnedRows[activeParam];

                        foreach (var entry in View.Selection.GetSelectedRows())
                        {
                            if (pinned.Contains(entry.ID))
                            {
                                pinned.Remove(entry.ID);
                            }
                        }
                    }
                    UIHelper.Tooltip($"Unpin the current row selection from top of the row list.");
                }

                ImGui.Separator();
            }

            // Decorator Options (e.g. Go to Text)
            if (fmgDecorator != null)
            {
                fmgDecorator.DecorateContextMenuItems(r);
            }


            // Comparison Options
            if (CFG.Current.Param_RowContextMenu_CompareOptions)
            {
                if (ImGui.Selectable("Compare"))
                {
                    View.Selection.SetCompareRow(r);
                }
                UIHelper.Tooltip($"Set this row as the row comparison target within the field window.");
            }

            // Reverse Lookup Options
            if (CFG.Current.Param_RowContextMenu_ReverseLoopup)
            {
                FieldDecorators.ParamReverseLookup_Value(Editor, Editor.Project.ParamData.PrimaryBank, activeParam, r.ID);
            }

            FieldDecorators.ParamQuickSearch(Editor, Editor.Project.ParamData.PrimaryBank, activeParam, r.ID);

            ImGui.EndPopup();
        }
    }
}