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
using Veldrid;
using static StudioCore.Editors.ParamEditor.ParamUtils;

namespace StudioCore.Editors.ParamEditor;

public class ParamRowWindow
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamEditorView ParentView;

    public readonly Dictionary<string, string> lastRowSearch = new();
    public bool _arrowKeyPressed;
    public bool _focusRows;
    public int _gotoParamRow = -1;

    public string TargetField = "";
    public string NameAdjustment = "";

    public ParamRowWindow(ParamEditorScreen editor, ProjectEntry project, ParamEditorView curView)
    {
        Editor = editor;
        Project = project;
        ParentView = curView;
    }

    public void Display(bool doFocus, bool isActiveView, float scrollTo, string activeParam)
    {
        if (!ParentView.Selection.ActiveParamExists())
        {
            ImGui.Text("Select a param to see rows");
        }
        else
        {
            var fmgDecorator = ParentView.RowDecorators.GetFmgRowDecorator(activeParam);

            DisplayHeader(ref doFocus, isActiveView, ref scrollTo, activeParam);

            Param para = Editor.Project.Handler.ParamData.PrimaryBank.Params[activeParam];

            HashSet<int> vanillaDiffCache = Editor.Project.Handler.ParamData.PrimaryBank.GetVanillaDiffRows(activeParam);

            var auxDiffCaches = Editor.Project.Handler.ParamData.AuxBanks.Select((bank, i) =>
                (bank.Value.GetVanillaDiffRows(activeParam), bank.Value.GetPrimaryDiffRows(activeParam))).ToList();

            Param.Column compareCol = ParentView.Selection.GetCompareCol();
            PropertyInfo compareColProp = typeof(Param.Cell).GetProperty("Value");

            //ImGui.BeginChild("rows" + activeParam);
            if (EditorTableUtils.ImGuiTableStdColumns("rowList", compareCol == null ? 1 : 2, false))
            {
                var curParam = Editor.Project.Handler.ParamData.PrimaryBank.Params[activeParam];
                var meta = Editor.Project.Handler.ParamData.GetParamMeta(curParam.AppliedParamdef);

                var pinnedRowList = Editor.Project.Descriptor.PinnedRows
                    .GetValueOrDefault(activeParam, new List<int>()).Select(id => para[id]).ToList();

                ImGui.TableSetupColumn("rowCol", ImGuiTableColumnFlags.None, 1f);
                if (compareCol != null)
                {
                    ImGui.TableSetupColumn("rowCol2", ImGuiTableColumnFlags.None, 0.4f);

                    if (CFG.Current.ParamEditor_Row_List_Pinned_Stay_Visible)
                    {
                        ImGui.TableSetupScrollFreeze(2, 1 + pinnedRowList.Count);
                    }
                    if (ImGui.TableNextColumn())
                    {
                        FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);
                        ImGui.Text("ID\t\tName");
                    }

                    if (ImGui.TableNextColumn())
                    {
                        FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);
                        ImGui.Text(compareCol.Def.InternalName);
                    }
                }
                else
                {
                    if (CFG.Current.ParamEditor_Row_List_Pinned_Stay_Visible)
                    {
                        ImGui.TableSetupScrollFreeze(1, pinnedRowList.Count);
                    }
                }

                ImGui.PushID("pinned");

                var selectionCachePins = ParentView.Selection.GetSelectionCache(pinnedRowList, "pinned");
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

                    if (EditorTableUtils.ImguiTableSeparator())
                    {
                        ImGui.Spacing();
                    }
                }

                ImGui.PopID();

                if (InputManager.HasArrowSelection())
                {
                    _arrowKeyPressed = true;
                }

                if (_focusRows)
                {
                    ImGui.SetNextWindowFocus();
                    _arrowKeyPressed = false;
                    _focusRows = false;
                }

                List<Param.Row> rows = CacheBank.GetCached(Editor, (ParentView.ViewIndex, activeParam),
                    () => ParentView.MassEdit.RSE.Search((Editor.Project.Handler.ParamData.PrimaryBank, para),

                ParentView.Selection.GetCurrentRowSearchString(), true, true));

                var enableGrouping = false;

                if (meta != null)
                {
                    enableGrouping = CFG.Current.ParamEditor_Row_List_Enable_Row_Grouping && meta.ConsecutiveIDs;
                }

                // Rows
                var selectionCache = ParentView.Selection.GetSelectionCache(rows, "regular");

                for (var i = 0; i < rows.Count; i++)
                {
                    Param.Row currentRow = rows[i];

                    var displayRow = false;

                    if (ParentView.ParamTableWindow.IsInTableGroupMode(activeParam))
                    {
                        if (currentRow.ID == ParentView.ParamTableWindow.CurrentTableGroup)
                        {
                            displayRow = true;
                        }
                    }
                    else
                    {
                        displayRow = true;
                    }

                    if (displayRow)
                    {
                        // Display groupings if ConsecutiveIDs is set in the meta for the current param.
                        if (enableGrouping)
                        {
                            Param.Row prev = i - 1 > 0 ? rows[i - 1] : null;
                            Param.Row next = i + 1 < rows.Count ? rows[i + 1] : null;
                            if (prev != null && next != null && prev.ID + 1 != currentRow.ID &&
                                currentRow.ID + 1 == next.ID)
                            {
                                EditorTableUtils.ImguiTableSeparator();
                            }

                            HandleRowPresentation(selectionCache, i, activeParam, rows, currentRow, vanillaDiffCache,
                                auxDiffCaches, fmgDecorator, ref scrollTo, doFocus, false, compareCol, compareColProp,
                                meta);

                            if (prev != null && next != null && prev.ID + 1 == currentRow.ID &&
                                currentRow.ID + 1 != next.ID)
                            {
                                EditorTableUtils.ImguiTableSeparator();
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
        if (ParentView.MassEdit.AutoFill != null)
        {
            ImGui.AlignTextToFramePadding();
            var resAutoRow = ParentView.MassEdit.AutoFill.RowSearchBarAutoFill();

            if (resAutoRow != null)
            {
                ParentView.Selection.SetCurrentRowSearchString(resAutoRow);
            }
        }

        ImGui.SameLine();

        // Row Search
        if (isActiveView && InputManager.IsPressed(KeybindID.ParamEditor_Focus_Searchbar))
        {
            ImGui.SetKeyboardFocusHere();
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputText($"##rowSearch", ref ParentView.Selection.GetCurrentRowSearchString(), 256);
        UIHelper.Tooltip($"Search <{InputManager.GetHint(KeybindID.ParamEditor_Focus_Searchbar)}>");

        if (!lastRowSearch.ContainsKey(ParentView.Selection.GetActiveParam())
            || !lastRowSearch[ParentView.Selection.GetActiveParam()].Equals(ParentView.Selection.GetCurrentRowSearchString()))
        {
            CacheBank.ClearCaches();
            lastRowSearch[ParentView.Selection.GetActiveParam()] = ParentView.Selection.GetCurrentRowSearchString();

            doFocus = true;
        }

        if (ImGui.IsItemActive())
        {
            ParentView._isSearchBarActive = true;
        }
        else
        {
            ParentView._isSearchBarActive = false;
        }

        ImGui.SameLine();

        // Go to selected
        ImGui.AlignTextToFramePadding();
        if (ImGui.Button($"{Icons.LocationArrow}") ||
            isActiveView && InputManager.IsPressed(KeybindID.Jump))
        {
            ParentView.JumpToSelectedRow = true;
        }
        UIHelper.Tooltip($"Go to selected <{InputManager.GetHint(KeybindID.Jump)}>");

        ImGui.SameLine();

        // Mass Edit Hint
        ImGui.AlignTextToFramePadding();

        if (ImGui.Button($"{Icons.QuestionCircle}"))
        {
            ImGui.OpenPopup("massEditHint");
        }
        UIHelper.Tooltip(ParamEditorHints.SearchBarHint);

        if (ImGui.BeginPopup("massEditHint"))
        {
            ImGui.Text(ParamEditorHints.SearchBarHint);

            ImGui.EndPopup();
        }

        ImGui.SameLine();

        // Toggle Modified Background
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}"))
        {
            CFG.Current.ParamEditor_Row_List_Display_Modified_Row_Bg = !CFG.Current.ParamEditor_Row_List_Display_Modified_Row_Bg;
        }

        var rowModifiedBgMode = "Hide Background";
        if (CFG.Current.ParamEditor_Row_List_Display_Modified_Row_Bg)
            rowModifiedBgMode = "Display Background";

        UIHelper.Tooltip($"Toggle the display of the modified background on modified rows.\nCurrent Mode: {rowModifiedBgMode}");

        // Quick Export
        if (CFG.Current.Developer_Enable_Tools)
        {
            ParamDebugTools.DisplayQuickRowNameExport(Editor, Project);
        }

        ImGui.Separator();
    }

    private void DisplayRow(bool[] selectionCache, int selectionCacheIndex, string activeParam,
        List<Param.Row> p, Param.Row r, HashSet<int> vanillaDiffCache,
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, FmgRowDecorator fmgDecorator, ref float scrollTo,
        bool doFocus, bool isPinned, ParamMeta meta)
    {
        var diffVanilla = vanillaDiffCache.Contains(r.ID);
        var auxDiffVanilla = auxDiffCaches.Where(cache => cache.Item1.Contains(r.ID)).Count() > 0;

        var popColor = true;

        if (diffVanilla)
        {
            // If the auxes are changed 
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
                popColor = false;
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
                ParentView.Selection.SetActiveRow(r, true);
                _gotoParamRow = -1;
                ImGui.SetScrollHereY();
            }
        }

        if (ParentView.JumpToSelectedRow && !isPinned)
        {
            var activeRow = ParentView.Selection.GetActiveRow();

            if (activeRow == null)
            {
                ParentView.JumpToSelectedRow = false;
            }
            else if (activeRow.ID == r.ID)
            {
                ImGui.SetScrollHereY();
                ParentView.JumpToSelectedRow = false;
            }
        }

        var label = $@"{r.ID} {Utils.ImGuiEscape(r.Name)}";

        label = Utils.ImGui_WordWrapString(label, ImGui.GetColumnWidth(),
            !CFG.Current.ParamEditor_Row_List_Enable_Line_Wrapping ? 1 : 3);

        if (ParentView.ParamTableWindow.IsInTableGroupMode(activeParam))
        {
            if (CFG.Current.ParamEditor_Table_List_Row_Display_Type is ParamTableRowDisplayType.None)
            {
                label = $@"{Utils.ImGuiEscape(r.Name)}";
            }
        }

        if (CFG.Current.ParamEditor_Row_List_Display_Modified_Row_Bg)
        {
            if (diffVanilla)
            {
                if (selected)
                {
                    ImGui.PushStyleColor(ImGuiCol.Header, UI.Current.ParamRowDiffBackgroundColor);
                }
                else
                {
                    UIHelper.ApplyDiffHeaderBackground();
                }
            }
        }

        if (ImGui.Selectable($@"{label}##{selectionCacheIndex}", selected))
        {
            if (Editor.ViewHandler.ActiveView.ViewIndex != ParentView.ViewIndex)
            {
                EditorCommandQueue.AddCommand($@"param/view/{ParentView.ViewIndex}/{ParentView.Selection.GetActiveParam()}");
            }

            _focusRows = true;

            if (InputManager.HasCtrlDown())
            {
                ParentView.Selection.ToggleRowInSelection(r);
            }
            else if (p != null && (InputManager.HasShiftDown())
                && ParentView.Selection.GetActiveRow() != null)
            {
                ParentView.Selection.CleanSelectedRows();

                var start = p.IndexOf(ParentView.Selection.GetActiveRow());
                var end = p.IndexOf(r);

                if (start != end && start != -1 && end != -1)
                {
                    foreach (Param.Row r2 in p.GetRange(start < end ? start : end, Math.Abs(end - start)))
                    {
                        if (ParentView.ParamTableWindow.IsInTableGroupMode(activeParam))
                        {
                            if (r2.ID == ParentView.ParamTableWindow.CurrentTableGroup)
                            {
                                ParentView.Selection.AddRowToSelection(r2);
                            }
                        }
                        else
                        {
                            ParentView.Selection.AddRowToSelection(r2);
                        }
                    }
                }

                ParentView.Selection.AddRowToSelection(r);
            }
            else
            {
                ParentView.Selection.SetActiveRow(r, true);
            }
        }

        if (_arrowKeyPressed && ImGui.IsItemFocused() && r != ParentView.Selection.GetActiveRow())
        {
            if (InputManager.HasCtrlDown())
            {
                // Add to selection
                ParentView.Selection.AddRowToSelection(r);
            }
            else
            {
                // Exclusive selection
                ParentView.Selection.SetActiveRow(r, true);
            }

            _arrowKeyPressed = false;
        }

        if (CFG.Current.ParamEditor_Row_List_Display_Modified_Row_Bg)
        {
            if (diffVanilla)
            {
                if (selected)
                {
                    ImGui.PopStyleColor(1);
                }
            }
        }

        if (popColor)
        {
            ImGui.PopStyleColor(1);
        }

        DisplayContextMenu(r, selectionCacheIndex, isPinned, activeParam, fmgDecorator);

        if (fmgDecorator != null)
        {
            fmgDecorator.DecorateParam(r);
        }

        // Roll Chance for Table Group View
        if (ParentView.ParamTableWindow.IsInTableGroupMode(activeParam))
        {
            ParentView.ParamTableWindow.DisplayTableEntryChance(r);
        }

        if (doFocus && ParentView.Selection.GetActiveRow() == r)
        {
            scrollTo = ImGui.GetCursorPosY();
        }
    }

    private bool HandleRowPresentation(bool[] selectionCache, int selectionCacheIndex, string activeParam,
        List<Param.Row> p, Param.Row r, HashSet<int> vanillaDiffCache,
        List<(HashSet<int>, HashSet<int>)> auxDiffCaches, FmgRowDecorator fmgDecorator, ref float scrollTo,
        bool doFocus, bool isPinned, Param.Column compareCol, PropertyInfo compareColProp, ParamMeta meta)
    {
        if (CFG.Current.ParamEditor_Enable_Compact_Mode)
        {
            // ItemSpacing only affects clickable area for selectables in tables. Add additional height to prevent gaps between selectables.
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 2.0f));
        }

        var lastCol = false;

        if (ImGui.TableNextColumn())
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);

            DisplayRow(selectionCache, selectionCacheIndex, activeParam, p, r, vanillaDiffCache,
                auxDiffCaches, fmgDecorator, ref scrollTo, doFocus, isPinned, meta);
            lastCol = true;
        }

        if (compareCol != null)
        {
            if (ImGui.TableNextColumn())
            {
                FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);

                Param.Cell c = r[compareCol];
                object newval = null;
                ImGui.PushID("compareCol_" + selectionCacheIndex);
                ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));

                var metaContext = new FieldMetaContext(
                    ParentView, meta, meta.GetField(c.Def), ParentView.Selection.GetActiveParam(), c.Def.InternalName);

                ParentView.FieldInputHandler.DisplayFieldInput(metaContext, compareCol.ValueType, c.Value, ref newval);

                if (ParentView.FieldInputHandler.UpdateProperty(c, compareColProp,
                        c.Value))
                {
                    Editor.Project.Handler.ParamData.PrimaryBank.RefreshParamRowDiffs(Editor, r, activeParam);
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

        if (CFG.Current.ParamEditor_Enable_Compact_Mode)
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
            // Name Input
            if (CFG.Current.ParamEditor_Row_Context_Display_Row_Name_Input)
            {
                if (ParentView.Selection.RowSelectionExists())
                {
                    var name = ParentView.Selection.GetActiveRow().Name;
                    if (name != null)
                    {
                        ImGui.InputText("##nameMassEdit", ref name, 255);

                        if (ImGui.IsItemDeactivatedAfterEdit())
                        {
                            var editCommand = $"selection: Name := {name}";
                            ParentView.Selection.SortSelection();

                            ParentView.MassEdit.ApplyMassEdit(editCommand);
                        }
                    }
                }
            }

            // Copy
            if (ImGui.Selectable(@$"Copy", false,
                    ParentView.Selection.RowSelectionExists()
                        ? ImGuiSelectableFlags.None
                        : ImGuiSelectableFlags.Disabled))
            {
                Editor.Clipboard.CopySelectionToClipboard(ParentView);
            }
            UIHelper.Tooltip($"Shortcut: {InputManager.GetHint(KeybindID.Copy)}\n\n" +
                "Copy the current row selection to the clipboard.");

            // Paste
            if (ImGui.Selectable(@$"Paste", false,
                    Editor.Project.Handler.ParamData.PrimaryBank.ClipboardRows.Any() ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
            {
                EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
            }
            UIHelper.Tooltip($"Shortcut: {InputManager.GetHint(KeybindID.Paste)}\n\n" +
                "Paste the current row clipboard into the current param.");

            // Delete
            if (ImGui.Selectable(@$"Delete", false,
                    ParentView.Selection.RowSelectionExists()
                        ? ImGuiSelectableFlags.None
                        : ImGuiSelectableFlags.Disabled))
            {
                ParamRowDelete.ApplyDelete(ParentView);
            }
            UIHelper.Tooltip($"Shortcut: {InputManager.GetHint(KeybindID.Delete)}\n\n" +
                "Delete the current row selection from the param.");

            // Duplicate
            if (ImGui.BeginMenu("Duplicate"))
            {
                ImGui.InputInt("Offset##duplicateOffset", ref CFG.Current.Param_Toolbar_Duplicate_Offset);

                UIHelper.Tooltip("The ID offset to apply when duplicating.\nSet to 0 for row indexed params to duplicate as expected.");

                ImGui.InputInt("Amount##duplicateAmount", ref CFG.Current.Param_Toolbar_Duplicate_Amount);

                UIHelper.Tooltip("The number of times the current selection will be duplicated.");

                if (ImGui.Selectable(@$"Apply", false,
                    ParentView.Selection.RowSelectionExists()
                        ? ImGuiSelectableFlags.None
                        : ImGuiSelectableFlags.Disabled))
                {
                    ParamRowDuplicate.ApplyDuplicate(ParentView);
                }
                UIHelper.Tooltip($"Shortcut: {InputManager.GetHint(KeybindID.Duplicate)}\n\n" +
                    "Duplicate the current row selection, automatically incrementing the row ID.");

                ImGui.EndMenu();
            }

            // Duplicate To
            if (ImGui.BeginMenu("Duplicate To", ParamRowDuplicate.IsCommutativeParam(ParentView)))
            {
                ParamRowDuplicate.ApplyCommutativeDuplicate(ParentView);

                ImGui.EndMenu();
            }
            UIHelper.Tooltip($"Duplicate the current row selection into the chosen target param.");

            // Revert to Default
            if (ImGui.Selectable(@$"Revert to Default", false,
                    ParentView.Selection.RowSelectionExists()
                        ? ImGuiSelectableFlags.None
                        : ImGuiSelectableFlags.Disabled))
            {
                ParamRowOperations.SetRowToDefault(ParentView);
            }
            UIHelper.Tooltip($"Revert the current row selection field values to the vanilla field values.");

            ImGui.Separator();

            // Pin
            if (!isPinned)
            {
                if (ImGui.Selectable($"Pin"))
                {
                    if (!Editor.Project.Descriptor.PinnedRows.ContainsKey(activeParam))
                    {
                        Editor.Project.Descriptor.PinnedRows.Add(activeParam, new List<int>());
                    }

                    List<int> pinned = Editor.Project.Descriptor.PinnedRows[activeParam];

                    foreach (var entry in ParentView.Selection.GetSelectedRows())
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
                    if (!Editor.Project.Descriptor.PinnedRows.ContainsKey(activeParam))
                    {
                        Editor.Project.Descriptor.PinnedRows.Add(activeParam, new List<int>());
                    }

                    List<int> pinned = Editor.Project.Descriptor.PinnedRows[activeParam];

                    foreach (var entry in ParentView.Selection.GetSelectedRows())
                    {
                        if (pinned.Contains(entry.ID))
                        {
                            pinned.Remove(entry.ID);
                        }
                    }
                }
                UIHelper.Tooltip($"Unpin the current row selection from top of the row list.");
            }

            // Decorator Options (e.g. Go to Text)
            if (fmgDecorator != null)
            {
                fmgDecorator.DecorateContextMenuItems(r);
            }

            // Advanced Contextual Actions
            if (CFG.Current.ParamEditor_Row_Context_Display_Advanced_Options)
            {
                // Quick Search
                if (CFG.Current.ParamEditor_Row_Context_Display_Finder_Quick_Option)
                {
                    ParamRowTools.ParamQuickSearch(ParentView, activeParam, r.ID);
                }

                // Comparison Options
                if (ImGui.Selectable("Compare"))
                {
                    ParentView.Selection.SetCompareRow(r);
                }
                UIHelper.Tooltip($"Set this row as the row comparison target within the field window.");

                // Reverse Lookup Options
                ParamRowTools.ParamReverseLookup_Value(ParentView, activeParam, r.ID);

                ImGui.Separator();

                if (ImGui.BeginMenu("Row Name Inherit Actions"))
                {
                    // Proliferate name to references
                    if (ImGui.Selectable(@$"Proliferate name to references", false,
                        ParentView.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.ProliferateRowName(ParentView, TargetField);
                    }
                    UIHelper.Tooltip($"Proliferate the name of this row to the references pointed to by the named field within this row.");

                    // Inherit Name from reference
                    if (ImGui.Selectable(@$"Inherit name from reference", false,
                            ParentView.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.InheritRowName(ParentView, TargetField);
                    }
                    UIHelper.Tooltip($"Inherit the name of the referenced row connected to via the target field.");

                    // Inherit Name from text
                    if (ImGui.Selectable(@$"Inherit name from text", false,
                            ParentView.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.InheritRowNameFromFMG(ParentView, TargetField);
                    }
                    UIHelper.Tooltip($"Inherit the name of the referenced FMG connected to via the target field.");

                    // Inherit Name from alias
                    if (ImGui.Selectable(@$"Inherit name from alias", false,
                            ParentView.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.InheritRowNameFromAlias(ParentView, TargetField);
                    }
                    UIHelper.Tooltip($"Inherit the name of the referenced Alias connected to via the target field.");

                    ImGui.InputText("Inherit Name Field##targetField", ref TargetField, 255);
                    UIHelper.Tooltip("The internal name of the field to target for the inherit name actions.");

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Row Name Adjustment Actions"))
                {
                    if (ImGui.Selectable(@$"Clear text from name", false,
                        ParentView.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.AdjustRowName(ParentView, NameAdjustment, ParamRowNameAdjustType.Clear);
                    }
                    UIHelper.Tooltip($"Prepend text to the names of all currently selected rows.");

                    if (ImGui.Selectable(@$"Prepend text to name", false,
                        ParentView.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.AdjustRowName(ParentView, NameAdjustment, ParamRowNameAdjustType.Prepend);
                    }
                    UIHelper.Tooltip($"Prepend text to the names of all currently selected rows.");

                    if (ImGui.Selectable(@$"Postpend text to name", false,
                            ParentView.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.AdjustRowName(ParentView, NameAdjustment, ParamRowNameAdjustType.Postpend);
                    }
                    UIHelper.Tooltip($"Postpend text to the names of all currently selected rows.");

                    if (ImGui.Selectable(@$"Remove text from name", false,
                            ParentView.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.AdjustRowName(ParentView, NameAdjustment, ParamRowNameAdjustType.Remove);
                    }
                    UIHelper.Tooltip($"Remove text from the names of all currently selected rows.");

                    ImGui.InputText("Text to Apply##nameAdjustment", ref NameAdjustment, 255);
                    UIHelper.Tooltip("The string to pre or post pend to the existing name.");

                    ImGui.EndMenu();

                }

                ImGui.Separator();

                var selectedRowCount = ParentView.Selection.GetSelectedRows().Count;
                ImGui.Text($"{selectedRowCount} rows selected currently.");
            }

            ImGui.EndPopup();
        }
    }

    /// <summary>
    /// Select all for the Row window
    /// </summary>
    public void SelectAll()
    {
        ParentView.GetPrimaryBank().ClipboardParam = ParentView.Selection.GetActiveParam();

        var activeParam = ParentView.Selection.GetActiveParam();

        foreach (Param.Row row in CacheBank.GetCached(Editor, (ParentView.ViewIndex, ParentView.Selection.GetActiveParam()),
            () => ParentView.MassEdit.RSE.Search((ParentView.GetPrimaryBank(), ParentView.GetPrimaryBank().Params[ParentView.Selection.GetActiveParam()]),
            ParentView.Selection.GetCurrentRowSearchString(), true, true)))
        {
            if (ParentView.ParamTableWindow.IsInTableGroupMode(activeParam))
            {
                if (row.ID == ParentView.ParamTableWindow.CurrentTableGroup)
                {
                    ParentView.Selection.AddRowToSelection(row);
                }
            }
            else
            {
                ParentView.Selection.AddRowToSelection(row);
            }
        }
    }
}
