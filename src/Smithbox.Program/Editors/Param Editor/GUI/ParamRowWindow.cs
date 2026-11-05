using Andre.Formats;
using Hexa.NET.DirectXTex;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// The row list context (per frame)
/// </summary>
public class RowListContext
{
    public bool IsActiveView { get; set; }
    public bool DoFocus { get; set; }
    public float ScrollTo { get; set; }

    public string ActiveParam { get; set; }
    public Param CurParam { get; set; }
    public ParamMeta CurMeta { get; set; }
    public ParamAnnotationEntry CurAnnotation { get; set; }

    public Param.Column CompareColumn { get; set; }
    public PropertyInfo CompareColumnProperty { get; set; }

    public HashSet<Param.Row> VanillaDiffCache { get; set; }
    public List<(HashSet<Param.Row>, HashSet<Param.Row>)> AuxDiffCaches { get; set; }

    public FmgRowDecorator FmgRowDecorator { get; set; }

    public RowListContext() { }
}

public class ParamRowWindow
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamEditorView ParentView;

    public readonly Dictionary<string, string> lastRowSearch = new();
    public bool _arrowKeyPressed;
    public bool _focusRows;
    public int _gotoParamRow = -1;

    private RowListContext Context;
    // Alt-click inline editing: the row being edited and its edit buffer. The buffers persist across frames,
    // so the committed value doesn't depend on which frame ImGui reports the deactivation in.
    private Param.Row ID_EditRow { get; set; }
    private Param.Row Name_EditRow { get; set; }
    private int _idEditBuffer;
    private string _nameEditBuffer = "";
    private bool _focusEditInput;

    // Buffers for the row context menu's ID / Name inputs, filled when the menu opens
    private int _contextIdBuffer;
    private string _contextNameBuffer = "";

    private Param.Row DragSourceRow;

    // Keyboard navigation: the row the last key or click moved to (the far end of a Shift range), and a row
    // that Home/End/PageUp/PageDown moved to, still waiting to be scrolled to and focused
    private Param.Row _keyCursorRow;
    private Param.Row _keyFocusRow;

    // Rows shown after the search (and table-group filter) vs. rows in the param, from the last frame's list
    private int _shownRowCount = -1;
    private int _totalRowCount = -1;


    public ParamRowWindow(ParamEditorScreen editor, ProjectEntry project, ParamEditorView curView)
    {
        Editor = editor;
        Project = project;
        ParentView = curView;
    }

    public void Display(bool doFocus, bool isActiveView, float scrollTo, string activeParam)
    {
        Context = new RowListContext
        {
            DoFocus = doFocus,
            IsActiveView = isActiveView,
            ScrollTo = scrollTo,
            ActiveParam = activeParam
        };

        DisplayTitle();

        if (!ParentView.Selection.ActiveParamExists())
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);
            ImGui.Text(LOC.Get("PARAM_RowWindow_Hint"));
        }
        else
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);

            DisplayHeader();

            Context.FmgRowDecorator = ParentView.RowDecorators.GetFmgRowDecorator(activeParam);
            Context.CurParam = Editor.Project.Handler.ParamData.PrimaryBank.Params[activeParam];
            Context.CurMeta = Editor.Project.Handler.ParamData.GetParamMeta(Context.CurParam.AppliedParamdef);
            Context.CurAnnotation = Editor.Project.Handler.ParamData.GetParamAnnotations(Context.CurParam.AppliedParamdef.ParamType);

            Context.VanillaDiffCache = Editor.Project.Handler.ParamData.PrimaryBank
                .GetVanillaDiffRows(activeParam);
            Context.AuxDiffCaches = Editor.Project.Handler.ParamData.AuxBanks
                .Select((bank, i) => (bank.Value.GetVanillaDiffRows(activeParam),
                bank.Value.GetPrimaryDiffRows(activeParam))).ToList();

            Context.CompareColumn = ParentView.Selection.GetCompareCol();
            Context.CompareColumnProperty = typeof(Param.Cell).GetProperty("Value");

            DisplayPinnedRowList();
            DisplayRowList();
        }
    }

    public void DisplayTitle()
    {
        var rowListTitle = LOC.Get("PARAM_RowWindow_Title");

        GUI.TitleHeader($"{rowListTitle}", "");
    }

    private void DisplayHeader()
    {
        ImGui.BeginChild("ParamRowListHeaderSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        Context.ScrollTo = 0;

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

        if (Context.IsActiveView && InputManager.IsPressed(KeybindID.ParamEditor_Focus_Searchbar))
        {
            ImGui.SetKeyboardFocusHere();
        }

        // Search
        ImGui.AlignTextToFramePadding();
        ImGui.InputTextWithHint($"##rowSearch",
            LOC.Get("PARAM_RowWindow_Search_Hint"),
            ref ParentView.Selection.GetCurrentRowSearchString(), 256);

        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Search_TT", InputManager.GetHint(KeybindID.ParamEditor_Focus_Searchbar)));

        if (!lastRowSearch.ContainsKey(ParentView.Selection.GetActiveParam())
            || !lastRowSearch[ParentView.Selection.GetActiveParam()].Equals(ParentView.Selection.GetCurrentRowSearchString()))
        {
            CacheBank.ClearCaches();
            lastRowSearch[ParentView.Selection.GetActiveParam()] = ParentView.Selection.GetCurrentRowSearchString();

            Context.DoFocus = true;
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

        // Action: Go to Selected
        ImGui.AlignTextToFramePadding();
        if (ImGui.Button($"{Icons.LocationArrow}") ||
            Context.IsActiveView && InputManager.IsPressed(KeybindID.Jump))
        {
            ParentView.JumpToSelectedRow = true;
        }
        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Action_Go_To_TT",
            InputManager.GetHint(KeybindID.Jump),
            InputManager.GetHint(KeybindID.Find)));

        ImGui.SameLine();

        // Action: Mass Edit Hint
        ImGui.AlignTextToFramePadding();

        if (ImGui.Button($"{Icons.QuestionCircle}"))
        {
            ImGui.OpenPopup("massEditHint");
        }
        GUI.Tooltip(ParamEditorHints.SearchBarHint);

        if (ImGui.BeginPopup("massEditHint"))
        {
            ImGui.Text(ParamEditorHints.SearchBarHint);

            ImGui.EndPopup();
        }

        ImGui.SameLine();

        // Toggle: Display Modified Background
        GUI.DisplayToggleButton("modifiedBackgroundToggle", Icons.Bars,
            ref CFG.Current.ParamEditor_Row_List_Display_Modified_Row_Bg,
            "PARAM_RowWindow_Toggle_Modified_Background_Hidden",
            "PARAM_RowWindow_Toggle_Modified_Background_Visible",
            "PARAM_RowWindow_Toggle_Modified_Background_Hint");

        // Toggle: Display Decorators
        GUI.DisplayToggleButton("modifiedBackgroundToggle", Icons.PaintBrush,
            ref CFG.Current.ParamEditor_Row_List_Display_Decorators,
            "PARAM_RowWindow_Toggle_Text_Decorator_Hidden",
            "PARAM_RowWindow_Toggle_Text_Decorator_Visible",
            "PARAM_RowWindow_Toggle_Text_Decorator_Hint");

        // Quick Export
        if (CFG.Current.Developer_Enable_Tools)
        {
            ParamDebugTools.DisplayQuickRowNameExport(Editor, Project);
        }

        // Match count, e.g. "312 / 4,120 rows" (from the previous frame's list, which is drawn after this header)
        if (_shownRowCount >= 0 && _totalRowCount >= 0)
        {
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextDisabled(LOC.Get("PARAM_RowWindow_Search_Match_Count",
                _shownRowCount.ToString("N0"), _totalRowCount.ToString("N0")));
        }

        ImGui.EndChild();
    }

    private void ListHeader()
    {
        var colFlags = ImGuiTableColumnFlags.WidthStretch;

        ImGui.TableSetupColumn("ID", colFlags, 0.2f);
        ImGui.TableSetupColumn("Name", colFlags);
        // Comparison Column
        if (Context.CompareColumn != null)
        {
            ImGui.TableSetupColumn("Comparison", colFlags);
        }

        var columnCount = Context.CompareColumn != null ? 3 : 2;
        ImGui.TableSetupScrollFreeze(columnCount, 1);

        // ID
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        ImGui.Text(LOC.Get("PARAM_RowWindow_Col_ID"));

        // Name
        ImGui.TableSetColumnIndex(1);
        ImGui.Text(LOC.Get("PARAM_RowWindow_Col_Name"));

        // Comparison Column
        if (Context.CompareColumn != null)
        {
            ImGui.TableSetColumnIndex(2);

            var key = Context.CompareColumn.Def.InternalName;
            var name = key;

            if (CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Community)
            {
                var fieldAnnotation = Context.CurAnnotation.Fields.FirstOrDefault(e => e.Field == key);
                if (fieldAnnotation != null)
                {
                    name = fieldAnnotation.Name;
                }
            }
            else if (CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Community_Source)
            {
                var fieldAnnotation = Context.CurAnnotation.Fields.FirstOrDefault(e => e.Field == key);
                if (fieldAnnotation != null)
                {
                    name = $"{fieldAnnotation.Name} ({key})";
                }
            }
            else if (CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Source_Community)
            {
                var fieldAnnotation = Context.CurAnnotation.Fields.FirstOrDefault(e => e.Field == key);
                if (fieldAnnotation != null)
                {
                    name = $"{key} ({fieldAnnotation.Name})";
                }
            }

            ImGui.Text(name);
        }
    }

    private void DisplayPinnedRowList()
    {
        var pinnedRowList = Editor.Project.Descriptor.PinnedRows
            .GetValueOrDefault(Context.ActiveParam, new List<int>()).Select(id => Context.CurParam[id]).ToList();

        if (pinnedRowList.Count > 0)
        {
            // Header row plus one row per pin, from the font-based row height (DPI-scaled, and matching
            // compact mode or not). Past 10 pins the panel scrolls.
            var visibleRows = Math.Min(pinnedRowList.Count, 10) + 1;
            var height = ImGui.GetFrameHeightWithSpacing() * visibleRows;

            ImGui.BeginChild("PinnedRowSection", new Vector2(0, height), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY;

            if (CFG.Current.ParamEditor_Enable_Table_Borders)
            {
                tblFlags = tblFlags | ImGuiTableFlags.Borders;
            }
            else
            {
                tblFlags = tblFlags | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV;
            }

            var columnCount = 2;

            if (Context.CompareColumn != null)
            {
                columnCount = 3;
            }

            if (ImGui.BeginTable($"pinnedRowList", columnCount, tblFlags))
            {
                FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);

                ListHeader();

                // Pinned Rows
                ImGui.PushID("pinned");
                var selectionCachePins = ParentView.Selection.GetSelectionCache(pinnedRowList, "pinned");

                for (var i = 0; i < pinnedRowList.Count(); i++)
                {
                    Param.Row row = pinnedRowList[i];

                    if (row == null)
                    {
                        continue;
                    }

                    HandleRowPresentation(selectionCachePins, i, null, row, true);
                }
                ImGui.PopID();

                ImGui.EndTable();
            }

            ImGui.EndChild();
        }
    }

    private void DisplayRowList()
    {
        ImGui.BeginChild("FullRowSection", new Vector2(0, 0), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

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

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY;

        if (CFG.Current.ParamEditor_Enable_Table_Borders)
        {
            tblFlags = tblFlags | ImGuiTableFlags.Borders;
        }
        else
        {
            tblFlags = tblFlags | ImGuiTableFlags.BordersOuterH | ImGuiTableFlags.BordersOuterV;
        }

        var columnCount = 2;

        if (Context.CompareColumn != null)
        {
            columnCount = 3;
        }

        if (ImGui.BeginTable($"fullRowList", columnCount, tblFlags))
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);

            ListHeader();

            // All Rows
            var curSearchTerm = ParentView.Selection.GetCurrentRowSearchString();

            List<Param.Row> rows = CacheBank.GetCached(
                Editor, (ParentView.ViewIndex, Context.ActiveParam),
                () => ParentView.MassEdit.RSE.Search(
                    (Editor.Project.Handler.ParamData.PrimaryBank, Context.CurParam),
                   curSearchTerm, true, true)
                );

            var enableGrouping = false;

            if (Context.CurMeta != null)
            {
                enableGrouping = CFG.Current.ParamEditor_Row_List_Enable_Row_Grouping && Context.CurMeta.ConsecutiveIDs;
            }

            // In table-group mode only the current group's rows are shown. Filter first so the clipper
            // (and the selection cache and shift-click ranges, which index into this list) see only those rows.
            var inTableGroupMode = ParentView.ParamTableWindow.IsInTableGroupMode(Context.ActiveParam);
            var tableGroupID = ParentView.ParamTableWindow.CurrentTableGroupID;

            List<Param.Row> displayRows = inTableGroupMode
                ? rows.Where(r => r.ID == tableGroupID).ToList()
                : rows;

            // The cache version includes the group, since each group gives a different list
            // Count for the search bar (U6), shown in the header next frame
            _shownRowCount = displayRows.Count;
            _totalRowCount = Context.CurParam.Rows.Count;

            var selectionCache = ParentView.Selection.GetSelectionCache(displayRows,
                inTableGroupMode ? $"regular_{tableGroupID}" : "regular");

            HandlePageKeys(displayRows);

            // Page keys change the selection, so rebuild the cache this frame rather than drawing stale highlights
            if (_keyFocusRow != null)
            {
                selectionCache = ParentView.Selection.GetSelectionCache(displayRows,
                    inTableGroupMode ? $"regular_{tableGroupID}" : "regular");
            }

            DisplayRows(displayRows, selectionCache);

            if (Context.DoFocus)
            {
                ImGui.SetScrollFromPosY(Context.ScrollTo - ImGui.GetScrollY());
            }

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// Draws the rows, using a list clipper so only the rows in view are submitted each frame.
    /// <para>
    /// Go to row, Jump to selected and the search-change focus all scroll by calling ImGui from inside the target
    /// row's draw code (SetScrollHereY / GetCursorPosY). So those rows are always included, even when they're off
    /// screen.
    /// </para>
    /// <para>
    /// The clipper assumes every row is the same height. With line wrapping on, rows can be up to three lines
    /// tall, so in that mode every row is still drawn.
    /// </para>
    /// </summary>
    private void DisplayRows(List<Param.Row> displayRows, bool[] selectionCache)
    {
        if (CFG.Current.ParamEditor_Row_List_Enable_Line_Wrapping)
        {
            for (var i = 0; i < displayRows.Count; i++)
            {
                HandleRowPresentation(selectionCache, i, displayRows, displayRows[i], false);
            }

            return;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(displayRows.Count);

        foreach (var index in GetForcedRowIndices(displayRows))
        {
            clipper.IncludeItemByIndex(index);
        }

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                HandleRowPresentation(selectionCache, i, displayRows, displayRows[i], false);
            }
        }

        clipper.End();
    }

    /// <summary>
    /// Indices of rows that must be drawn this frame even if they're scrolled out of view, because a pending
    /// scroll targets them. These use the same matching rules as the checks in <see cref="DisplayRow"/>.
    /// </summary>
    private List<int> GetForcedRowIndices(List<Param.Row> displayRows)
    {
        var indices = new List<int>();
        var activeRow = ParentView.Selection.GetActiveRow();

        // Go to row: matched by ID
        if (_gotoParamRow != -1)
        {
            var index = displayRows.FindIndex(r => r.ID == _gotoParamRow);
            if (index >= 0)
                indices.Add(index);
        }

        // Jump to selected row: matched by ID
        if (ParentView.JumpToSelectedRow && activeRow != null)
        {
            var index = displayRows.FindIndex(r => r.ID == activeRow.ID);
            if (index >= 0)
                indices.Add(index);
        }

        // Focus after a search or selection command: matched by reference
        if (Context.DoFocus && activeRow != null)
        {
            var index = displayRows.IndexOf(activeRow);
            if (index >= 0)
                indices.Add(index);
        }

        // Home/End/PageUp/PageDown target, scrolled to and focused in DisplayRowID
        if (_keyFocusRow != null)
        {
            var index = displayRows.IndexOf(_keyFocusRow);
            if (index >= 0)
                indices.Add(index);
            else
                _keyFocusRow = null;
        }

        return indices;
    }

    private void HandleRowPresentation(bool[] selectionCache, int selectionCacheIndex,
        List<Param.Row> rowList, Param.Row row, bool isPinned)
    {
        if (CFG.Current.ParamEditor_Enable_Compact_Mode)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 2.0f));
        }

        // ID
        ImGui.TableNextRow();
        ImGui.TableSetColumnIndex(0);
        DisplayRowID(selectionCache, selectionCacheIndex, rowList, row, isPinned);

        // Name
        ImGui.TableSetColumnIndex(1);
        DisplayRow(selectionCache, selectionCacheIndex, rowList, row, isPinned);

        // Comparison Column
        if (Context.CompareColumn != null)
        {
            ImGui.TableSetColumnIndex(2);
            Param.Cell c = row[Context.CompareColumn];
            object newval = null;
            ImGui.PushID("compareCol_" + selectionCacheIndex);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 0));

            var fieldAnnotation = Editor.Project.Handler.ParamData.GetFieldAnnotation(Context.CurAnnotation, c.Def.InternalName);

            var metaContext = new FieldMetaContext(
                ParentView, Context.CurMeta, Context.CurMeta.GetField(c.Def), fieldAnnotation, ParentView.Selection.GetActiveParam(), c.Def.InternalName);

            ParentView.FieldInputHandler.DisplayFieldInput(metaContext, Context.CompareColumn.ValueType, c.Value, ref newval);

            if (ParentView.FieldInputHandler.UpdateProperty(c, Context.CompareColumnProperty,
                    c.Value))
            {
                Editor.Project.Handler.ParamData.PrimaryBank.RefreshParamRowDiffs(Editor, row, Context.ActiveParam);
            }

            ImGui.PopStyleVar();
            ImGui.PopID();
        }

        if (CFG.Current.ParamEditor_Enable_Compact_Mode)
        {
            ImGui.PopStyleVar();
        }
    }

    /// <summary>
    /// Mouse and keyboard handling shared by a row's ID and Name selectables. Call it straight after the
    /// Selectable, so the ImGui "last item" queries refer to it.
    /// </summary>
    /// <param name="clicked">The Selectable's return value</param>
    /// <param name="rowList">The displayed row list (null for the pinned rows, which don't support ranges)</param>
    /// <param name="isIdColumn">Alt-click edits the ID in the ID column and the name in the Name column</param>
    private void HandleRowSelectableInput(bool clicked, List<Param.Row> rowList, Param.Row r, bool isIdColumn)
    {
        var selection = ParentView.Selection;

        if (clicked)
        {
            FocusThisView();

            _focusRows = true;

            if (InputManager.HasCtrlDown())
            {
                selection.ToggleRowInSelection(r);
                _keyCursorRow = r;
            }
            else if (rowList != null && InputManager.HasShiftDown() && selection.GetActiveRow() != null)
            {
                ClearEditRows();
                SelectRangeFromActiveRow(rowList, r);
            }
            else if (InputManager.HasAltDown())
            {
                // Edits the clicked row only, whatever is selected
                if (isIdColumn && Name_EditRow == null)
                {
                    ID_EditRow = r;
                    _idEditBuffer = r.ID;
                    _focusEditInput = true;
                }
                else if (!isIdColumn && ID_EditRow == null)
                {
                    Name_EditRow = r;
                    _nameEditBuffer = r.Name ?? "";
                    _focusEditInput = true;
                }
            }
            else
            {
                ClearEditRows();
                selection.SetActiveRow(r, true);
                _keyCursorRow = r;
            }
        }

        // Right-click on a row outside the selection selects it first, so the context menu acts on the row
        // under the mouse. Right-clicking inside a multi-selection keeps it.
        if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
        {
            var selectedRows = selection.GetSelectedRows();

            if (selectedRows == null || !selectedRows.Contains(r))
            {
                FocusThisView();
                ClearEditRows();
                selection.SetActiveRow(r, true);
                _keyCursorRow = r;
            }
        }

        // Arrow-key navigation: ImGui moves nav focus between the selectables, and the selection follows it.
        // Shift extends a range from the active row, Ctrl adds to the selection.
        if (_arrowKeyPressed && ImGui.IsItemFocused())
        {
            if (rowList != null && InputManager.HasShiftDown() && selection.GetActiveRow() != null)
            {
                ClearEditRows();
                SelectRangeFromActiveRow(rowList, r);
                _arrowKeyPressed = false;
            }
            else if (r != selection.GetActiveRow())
            {
                ClearEditRows();

                if (InputManager.HasCtrlDown())
                {
                    selection.AddRowToSelection(r);
                }
                else
                {
                    selection.SetActiveRow(r, true);
                }

                _keyCursorRow = r;
                _arrowKeyPressed = false;
            }
        }
    }

    /// <summary>
    /// Makes this the active view (queued, as the click handlers always did), if it isn't already
    /// </summary>
    private void FocusThisView()
    {
        if (Editor.ViewHandler.ActiveView.ViewIndex != ParentView.ViewIndex)
        {
            EditorCommandQueue.AddCommand($@"param/view/{ParentView.ViewIndex}/{ParentView.Selection.GetActiveParam()}");
        }
    }

    /// <summary>
    /// Selects every displayed row between the active row (the anchor, which stays active) and <paramref name="target"/>
    /// </summary>
    private void SelectRangeFromActiveRow(List<Param.Row> rowList, Param.Row target)
    {
        var selection = ParentView.Selection;

        // Keeps only the active row
        selection.CleanSelectedRows();

        var start = rowList.IndexOf(selection.GetActiveRow());
        var end = rowList.IndexOf(target);

        if (start == -1 || end == -1)
        {
            selection.AddRowToSelection(target);
        }
        else
        {
            for (var i = Math.Min(start, end); i <= Math.Max(start, end); i++)
            {
                selection.AddRowToSelection(rowList[i]);
            }
        }

        _keyCursorRow = target;
    }

    /// <summary>
    /// Home / End / PageUp / PageDown in the row list: move the keyboard cursor to the first / last row or by a
    /// page, selecting it (or, with Shift, extending the range from the active row to it).
    /// Called inside the row table (its scrolling window), before the rows are drawn.
    /// </summary>
    private void HandlePageKeys(List<Param.Row> displayRows)
    {
        if (displayRows.Count == 0)
            return;

        if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.ChildWindows) || ImGui.IsAnyItemActive())
            return;

        if (ID_EditRow != null || Name_EditRow != null)
            return;

        var home = ImGui.IsKeyPressed(ImGuiKey.Home);
        var end = ImGui.IsKeyPressed(ImGuiKey.End);
        var pageUp = ImGui.IsKeyPressed(ImGuiKey.PageUp);
        var pageDown = ImGui.IsKeyPressed(ImGuiKey.PageDown);

        if (!home && !end && !pageUp && !pageDown)
            return;

        // The cursor is the row the last key or click moved to. With a Shift range that's the range's far end,
        // not the active row.
        var cursor = _keyCursorRow != null && displayRows.Contains(_keyCursorRow)
            ? _keyCursorRow
            : ParentView.Selection.GetActiveRow();

        var current = cursor != null ? displayRows.IndexOf(cursor) : -1;
        // Rows that fit in the (scrolling) table window, less one for the frozen header row
        var pageSize = Math.Max(1, (int)(ImGui.GetWindowHeight() / ImGui.GetFrameHeightWithSpacing()) - 1);

        int target;

        if (home)
            target = 0;
        else if (end)
            target = displayRows.Count - 1;
        else if (pageUp)
            target = current < 0 ? 0 : current - pageSize;
        else
            target = current < 0 ? 0 : current + pageSize;

        target = Math.Clamp(target, 0, displayRows.Count - 1);

        var targetRow = displayRows[target];

        if (InputManager.HasShiftDown() && ParentView.Selection.GetActiveRow() != null)
        {
            SelectRangeFromActiveRow(displayRows, targetRow);
        }
        else
        {
            ParentView.Selection.SetActiveRow(targetRow, true);
            _keyCursorRow = targetRow;
        }

        // Scrolled to and focused when the row is drawn (it's force-included in the clipper)
        _keyFocusRow = targetRow;
    }

    private void DisplayRowID(bool[] selectionCache, int selectionCacheIndex,
        List<Param.Row> p, Param.Row r, bool isPinned)
    {
        var selected = selectionCache != null && selectionCacheIndex < selectionCache.Length
            ? selectionCache[selectionCacheIndex]
            : false;

        var label = $@"{r.ID}";

        label = Utils.ImGui_WordWrapString(label, ImGui.GetColumnWidth(),
            !CFG.Current.ParamEditor_Row_List_Enable_Line_Wrapping ? 1 : 3);

        if (ID_EditRow == null || ID_EditRow != r)
        {
            // Home/End/PageUp/PageDown moved the keyboard cursor here: scroll to it and give it nav focus,
            // so the next arrow key continues from this row
            if (!isPinned && _keyFocusRow == r)
            {
                ImGui.SetScrollHereY();
                ImGui.SetKeyboardFocusHere();
                _keyFocusRow = null;
            }

            var clicked = ImGui.Selectable($@"{label}##{selectionCacheIndex}_id", selected);

            HandleRowSelectableInput(clicked, p, r, true);

            // Drag source
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
            {
                DragSourceRow = r;
                unsafe
                {
                    byte dummy = 0;
                    ImGui.SetDragDropPayload("PARAM_ROW", &dummy, 1);
                }
                ImGui.Text(LOC.Get("PARAM_RowWindow_Row_Drag_TT", r.ID));
                ImGui.EndDragDropSource();
            }

            // Drop target
            if (ImGui.BeginDragDropTarget())
            {
                unsafe
                {
                    var payload = ImGui.AcceptDragDropPayload("PARAM_ROW");
                    if (!payload.IsNull && DragSourceRow != null && DragSourceRow != r)
                    {
                        int dropTargetIndex = Context.CurParam.IndexOfRow(r);

                        if (dropTargetIndex >= 0)
                        {
                            List<Param.Row> rowsToMove;
                            var selectedRows = ParentView.Selection.GetSelectedRows();

                            if (selectedRows.Contains(DragSourceRow))
                            {
                                rowsToMove = selectedRows
                                    .OrderBy(row => Context.CurParam.IndexOfRow(row))
                                    .ToList();
                            }
                            else
                            {
                                rowsToMove = new List<Param.Row> { DragSourceRow };
                            }

                            var action = new ReorderRowAction(Context.CurParam, rowsToMove, dropTargetIndex);
                            ParentView.Editor.ActionManager.ExecuteAction(action);
                        }

                        DragSourceRow = null;
                    }
                }
                ImGui.EndDragDropTarget();
            }

            DisplayContextMenu("id", r, selectionCacheIndex, isPinned);
        }
        else if (r == ID_EditRow)
        {
            // SetNextItemWidth rather than PushItemWidth: the old push was never popped
            ImGui.SetNextItemWidth(-1);

            if (_focusEditInput)
            {
                ImGui.SetKeyboardFocusHere();
                _focusEditInput = false;
            }

            ImGui.InputInt("##rowIdInput", ref _idEditBuffer, 255);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                // Enter or click-away commits. Escape restores the original value, so it's a no-op here.
                ParamRowOperations.SetRowID(ParentView, new[] { r }, _idEditBuffer);
                ClearEditRows();
            }
            else if (ImGui.IsItemDeactivated())
            {
                // Left without editing
                ClearEditRows();
            }
        }
    }

    private void DisplayRow(bool[] selectionCache, int selectionCacheIndex,
        List<Param.Row> p, Param.Row r, bool isPinned)
    {
        // r belongs to the primary bank, and Context.VanillaDiffCache holds the primary
        // bank's own row instances, so a direct reference lookup is valid here and (unlike
        // a plain ID lookup) won't conflate two different rows that happen to share an ID.
        var diffVanilla = Context.VanillaDiffCache.Contains(r);

        // The aux caches hold row instances that belong to the aux ParamBank, not the
        // primary bank, so r will never be reference-equal to any entry in them - these
        // have to keep comparing by ID.
        var auxDiffVanilla = Context.AuxDiffCaches.Where(cache => cache.Item1.Any(row => row.ID == r.ID)).Count() > 0;

        var popColor = true;

        if (diffVanilla)
        {
            // If the auxes are changed 
            var auxDiffPrimaryAndVanilla = (auxDiffVanilla ? 1 : 0) + Context.AuxDiffCaches
                .Where(cache => cache.Item1.Any(row => row.ID == r.ID) && cache.Item2.Any(row => row.ID == r.ID)).Count() > 1;

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

        var label = $@"{Utils.ImGuiEscape(r.Name)}";

        label = Utils.ImGui_WordWrapString(label, ImGui.GetColumnWidth(),
            !CFG.Current.ParamEditor_Row_List_Enable_Line_Wrapping ? 1 : 3);

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
                    GUI.ApplyDiffHeaderBackground();
                }
            }
        }

        if (Name_EditRow == null || Name_EditRow != r)
        {
            var clicked = ImGui.Selectable($@"{label}##{selectionCacheIndex}_name", selected);

            HandleRowSelectableInput(clicked, p, r, false);

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

            DisplayContextMenu("name", r, selectionCacheIndex, isPinned);

            if (Context.FmgRowDecorator != null)
            {
                Context.FmgRowDecorator.DecorateParam(r);
            }

            // Roll Chance for Table Group View
            if (ParentView.ParamTableWindow.IsInTableGroupMode(Context.ActiveParam))
            {
                ParentView.ParamTableWindow.DisplayTableEntryChance(r);
            }

            if (Context.DoFocus && ParentView.Selection.GetActiveRow() == r)
            {
                Context.ScrollTo = ImGui.GetCursorPosY();
            }
        }
        else if (Name_EditRow == r)
        {
            // The modified-row header colour pushed above is only popped in the selectable branch, so pop it here too
            if (CFG.Current.ParamEditor_Row_List_Display_Modified_Row_Bg && diffVanilla && selected)
            {
                ImGui.PopStyleColor(1);
            }

            if (popColor)
            {
                ImGui.PopStyleColor(1);
            }

            ImGui.SetNextItemWidth(-1);

            if (_focusEditInput)
            {
                ImGui.SetKeyboardFocusHere();
                _focusEditInput = false;
            }

            ImGui.InputText("##rowNameInput", ref _nameEditBuffer, 1024);

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                // Enter or click-away commits. Escape restores the original name, so it's a no-op here.
                ParamRowOperations.SetRowName(ParentView, new[] { r }, _nameEditBuffer);
                ClearEditRows();
            }
            else if (ImGui.IsItemDeactivated())
            {
                // Left without editing
                ClearEditRows();
            }
        }
    }

    public void DisplayContextMenu(string imguiKey, Param.Row r, int selectionCacheIndex, bool isPinned)
    {
        if (ImGui.BeginPopupContextItem($"{imguiKey}_{r.ID}_{selectionCacheIndex}"))
        {
            // ID / Name inputs: these apply to every selected row (e.g. to rename a batch, or to move rows
            // into one table group), so the count is shown in the tooltip
            var selectedRows = ParentView.Selection.GetSelectedRows()?.Where(row => row != null).ToList()
                ?? new List<Param.Row>();
            var sourceRow = ParentView.Selection.GetActiveRow() ?? selectedRows.FirstOrDefault();

            if (ImGui.IsWindowAppearing() && sourceRow != null)
            {
                _contextIdBuffer = sourceRow.ID;
                _contextNameBuffer = sourceRow.Name ?? "";
            }

            if (imguiKey == "id")
            {
                if (CFG.Current.ParamEditor_Row_Context_Display_Row_Name_Input && sourceRow != null)
                {
                    ImGui.InputInt("##newRowId", ref _contextIdBuffer);
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Edit_Applies_To_Selection", selectedRows.Count));

                    // Every selected row gets this one ID (useful for table groups, but easy to do by accident)
                    if (selectedRows.Count > 1)
                    {
                        ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color,
                            LOC.Get("PARAM_RowWindow_Context_Same_ID_Warning", selectedRows.Count));
                    }

                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        ParamRowOperations.SetRowID(ParentView, selectedRows, _contextIdBuffer);
                    }
                }
            }

            if (imguiKey == "name")
            {
                if (CFG.Current.ParamEditor_Row_Context_Display_Row_Name_Input && sourceRow != null)
                {
                    ImGui.InputText("##nameMassEdit", ref _contextNameBuffer, 1024);
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Edit_Applies_To_Selection", selectedRows.Count));

                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        ParamRowOperations.SetRowName(ParentView, selectedRows, _contextNameBuffer);
                    }
                }
            }

            // Copy
            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy")}##copyAction", false,
                    ParentView.Selection.RowSelectionExists()
                        ? ImGuiSelectableFlags.None
                        : ImGuiSelectableFlags.Disabled))
            {
                Editor.Clipboard.CopySelectionToClipboard(ParentView);
            }
            GUI.Tooltip(
                LOC.Get("PARAM_RowWindow_Context_Action_Copy_TT", InputManager.GetHint(KeybindID.Copy)));

            // Paste
            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Paste")}##pasteAction", false,
                    Editor.Project.Handler.ParamData.PrimaryBank.ClipboardRows.Any() ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
            {
                EditorCommandQueue.AddCommand(@"param/menu/ctrlVPopup");
            }
            GUI.Tooltip(
                LOC.Get("PARAM_RowWindow_Context_Action_Paste_TT", InputManager.GetHint(KeybindID.Paste)));

            // Delete
            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Delete")}##deleteAction", false,
                    ParentView.Selection.RowSelectionExists()
                        ? ImGuiSelectableFlags.None
                        : ImGuiSelectableFlags.Disabled))
            {
                ParamRowDelete.ApplyDelete(ParentView);
            }
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Delete_TT", InputManager.GetHint(KeybindID.Delete)));

            // Duplicate
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Duplicate_Header")}##duplicateMenuHeader"))
            {
                // Offset
                ImGui.InputInt($"{LOC.Get("PARAM_RowWindow_Context_DuplicateOffset")}##duplicateOffset",
                    ref CFG.Current.Param_Toolbar_Duplicate_Offset);

                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_DuplicateOffset_TT"));

                // Amount
                ImGui.InputInt($"{LOC.Get("PARAM_RowWindow_Context_DuplicateAmount")}##duplicateAmount",
                    ref CFG.Current.Param_Toolbar_Duplicate_Amount);

                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_DuplicateAmount_TT"));

                // Reference fields to offset, and which fields that will change
                ParamRowDuplicate.DisplayReferenceOffsetOptions(ParentView);

                ImGui.Separator();

                // Apply
                if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_DuplicateApply")}##duplicateApplyAction",
                    false,
                    ParentView.Selection.RowSelectionExists()
                        ? ImGuiSelectableFlags.None
                        : ImGuiSelectableFlags.Disabled))
                {
                    ParamRowDuplicate.ApplyDuplicate(ParentView);
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_DuplicateApply_TT", InputManager.GetHint(KeybindID.Duplicate)));

                ImGui.EndMenu();
            }

            // Duplicate To
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_DuplicateTo_Header")}##duplicateToMenuHeader", ParamRowDuplicate.IsCommutativeParam(ParentView)))
            {
                ParamRowDuplicate.ApplyCommutativeDuplicate(ParentView);

                ImGui.EndMenu();
            }
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_DuplicateTo_Header_TT"));

            // Jump
            if (HasJumpOption())
            {
                if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Jump_Header")}##jumpMenuHeader"))
                {
                    // Decorator Options (e.g. Go to Text)
                    if (Context.FmgRowDecorator != null)
                    {
                        Context.FmgRowDecorator.DecorateContextMenuItems(r);
                    }

                    ImGui.EndMenu();
                }
            }

            // Value
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Value_Header")}##valueMenuHeader"))
            {
                // Revert to Default
                if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Revert_to_Default")}##revertToDefaultAction", false,
                    ParentView.Selection.RowSelectionExists()
                        ? ImGuiSelectableFlags.None
                        : ImGuiSelectableFlags.Disabled))
                {
                    ParamRowOperations.SetRowToDefault(ParentView);
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Revert_to_Default_TT"));

                ImGui.EndMenu();
            }

            // Mass Edit
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_MassEdit_Header")}##massEditMenuHeader"))
            {
                // Command Palette
                if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Command_Palette")}##commandPaletteAction"))
                {
                    EditorCommandQueue.AddCommand(
                        $@"param/menu/massEditRegex/selection: ");
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Command_Palette_TT"));

                // Autofill
                if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Autofill_Header")}##autoFillMenuHeader"))
                {
                    if (ParentView.MassEdit.AutoFill != null)
                    {
                        var res = ParentView.MassEdit.AutoFill.MassEditCompleteAutoFill();
                        if (res != null)
                        {
                            EditorCommandQueue.AddCommand(
                                $@"{res}");
                        }
                    }

                    ImGui.EndMenu();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Autofill_Header_TT"));

                ImGui.EndMenu();
            }

            // Pinning
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Pinning_Header")}##pinningMenuHeader"))
            {
                if (!isPinned)
                {
                    // Pin
                    if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Pin")}##pinAction"))
                    {
                        if (!Editor.Project.Descriptor.PinnedRows.ContainsKey(Context.ActiveParam))
                        {
                            Editor.Project.Descriptor.PinnedRows.Add(Context.ActiveParam, new List<int>());
                        }

                        List<int> pinned = Editor.Project.Descriptor.PinnedRows[Context.ActiveParam];

                        foreach (var entry in ParentView.Selection.GetSelectedRows())
                        {
                            if (!pinned.Contains(entry.ID))
                            {
                                pinned.Add(entry.ID);
                            }
                        }
                    }
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Pin_TT"));
                }
                else if (isPinned)
                {
                    // Unpin
                    if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Unpin")}##unpinAction"))
                    {
                        if (!Editor.Project.Descriptor.PinnedRows.ContainsKey(Context.ActiveParam))
                        {
                            Editor.Project.Descriptor.PinnedRows.Add(Context.ActiveParam, new List<int>());
                        }

                        List<int> pinned = Editor.Project.Descriptor.PinnedRows[Context.ActiveParam];

                        foreach (var entry in ParentView.Selection.GetSelectedRows())
                        {
                            if (pinned.Contains(entry.ID))
                            {
                                pinned.Remove(entry.ID);
                            }
                        }
                    }
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Unpin_TT"));
                }

                // Unpin All (this param only, as the tooltip says)
                if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Unpin_All")}##unpinAllAction"))
                {
                    if (Context.ActiveParam != null)
                    {
                        Editor.Project.Descriptor.PinnedRows.Remove(Context.ActiveParam);
                    }
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Unpin_All_TT"));

                // Unpin All in All Params (the old Unpin All behaviour)
                if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Unpin_All_Params")}##unpinAllParamsAction"))
                {
                    Editor.Project.Descriptor.PinnedRows.Clear();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Unpin_All_Params_TT"));

                ImGui.EndMenu();
            }

            // Search
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Search_Header")}##searchMenuHeader"))
            {
                ParamRowTools.ParamQuickSearch(ParentView, Context.ActiveParam, r.ID);
                ParamRowTools.ParamReverseLookup_Value(ParentView, Context.ActiveParam, r.ID);

                ImGui.EndMenu();
            }

            // Comparison
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Comparison_Header")}##comparisonMenuHeader"))
            {
                // Set Compare Row
                if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Set_Compare_Row")}##setCompareRowAction"))
                {
                    ParentView.Selection.SetCompareRow(r);
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Set_Compare_Row_TT"));

                // Clear Compare Row
                if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Clear_Compare_Row")}##clearCompareRow"))
                {
                    ParentView.Selection.ClearCompareRow();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Clear_Compare_Row_TT"));

                ImGui.EndMenu();
            }

            // Row Name Manipulation
            ParentView.ToolMenu.RowNameManipulationTool.DisplayRowContextMenu(imguiKey);

            // Information
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Info_Header")}##infoMenuHeader"))
            {
                // Copy ID
                if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID")}##copyIdAction"))
                {
                    var selection = ParentView.Selection.GetSelectedRows();
                    StringBuilder _builder = new();
                    foreach (var entry in selection)
                    {
                        _builder.AppendLine($"{entry.ID.ToString()}");
                    }

                    PlatformUtils.Instance.SetClipboardText(_builder.ToString());
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID_TT"));

                // Copy Name
                if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy_Name")}##copyNameAction"))
                {
                    var selection = ParentView.Selection.GetSelectedRows();
                    StringBuilder _builder = new();
                    foreach (var entry in selection)
                    {
                        // Unnamed rows copy as an empty line, so the line count still matches the selection
                        _builder.AppendLine(entry.Name ?? "");
                    }

                    PlatformUtils.Instance.SetClipboardText(_builder.ToString());
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Copy_Name_TT"));

                // Copy ID and Name
                if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID_And_Name")}##copyIdAndNameAction"))
                {
                    var selection = ParentView.Selection.GetSelectedRows();
                    StringBuilder _builder = new();
                    foreach (var entry in selection)
                    {
                        _builder.AppendLine($"{entry.ID};{entry.Name ?? ""}");
                    }

                    PlatformUtils.Instance.SetClipboardText(_builder.ToString());
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID_And_Name_TT"));

                ImGui.EndMenu();
            }

            ImGui.Separator();

            var selectedRowCount = ParentView.Selection.GetSelectedRows().Count;
            ImGui.Text(LOC.Get("PARAM_RowWindow_Context_Row_Selection_Count_TT", selectedRowCount));

            ImGui.EndPopup();
        }
    }

    private bool HasJumpOption()
    {
        if (Context.FmgRowDecorator != null)
            return true;

        return false;
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
                if (row.ID == ParentView.ParamTableWindow.CurrentTableGroupID)
                {
                    ClearEditRows();
                    ParentView.Selection.AddRowToSelection(row);
                }
            }
            else
            {
                ClearEditRows();
                ParentView.Selection.AddRowToSelection(row);
            }
        }
    }

    public void ClearEditRows()
    {
        ID_EditRow = null;
        Name_EditRow = null;
    }
}