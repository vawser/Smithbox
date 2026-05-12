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

namespace StudioCore.Editors.ParamEditor;

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

    public HashSet<int> VanillaDiffCache { get; set; }
    public List<(HashSet<int>, HashSet<int>)> AuxDiffCaches { get; set; }

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

    public string TargetField = "";
    public string NameAdjustment = "";

    private RowListContext Context;
    private Param.Row ID_EditRow { get; set; }
    private Param.Row Name_EditRow { get; set; }

    private Param.Row DragSourceRow;


    public ParamRowWindow(ParamEditorScreen editor, ProjectEntry project, ParamEditorView curView)
    {
        Editor = editor;
        Project = project;
        ParentView = curView;
    }

    public void Display(bool doFocus, bool isActiveView, float scrollTo, string activeParam)
    {
        Context = new RowListContext();
        Context.DoFocus = doFocus;
        Context.IsActiveView = isActiveView;
        Context.ScrollTo = scrollTo;
        Context.ActiveParam = activeParam;

        DisplayTitle();

        if (!ParentView.Selection.ActiveParamExists())
        {
            ImGui.Text("Select a param to see rows");
        }
        else
        {
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

            DisplayHeaderRowList();
            DisplayPinnedRowList();
            DisplayRowList();
        }
    }

    public void DisplayTitle()
    {
        var rowListTitle = "Row List";

        UIHelper.SimpleHeader($"{rowListTitle}", "");
    }

    private void DisplayHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("ParamRowListHeaderSection", searchHeight, ImGuiChildFlags.Borders);

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

        // Row Search
        if (Context.IsActiveView && InputManager.IsPressed(KeybindID.ParamEditor_Focus_Searchbar))
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

        // Go to selected
        ImGui.AlignTextToFramePadding();
        if (ImGui.Button($"{Icons.LocationArrow}") ||
            Context.IsActiveView && InputManager.IsPressed(KeybindID.Jump))
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

        ImGui.EndChild();
    }

    private void DisplayHeaderRowList()
    {
        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders;

        var columnCount = 2;

        if (ImGui.BeginTable($"fullRowListHeader", columnCount, tblFlags))
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);

            ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthStretch, 0.2f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            // Comparison Column
            if (Context.CompareColumn != null)
            {
                ImGui.TableSetupColumn("Comparison", ImGuiTableColumnFlags.WidthStretch);
            }

            // ID
            ImGui.TableNextRow();
            ImGui.TableSetColumnIndex(0);
            ImGui.Text("ID");

            // Name
            ImGui.TableSetColumnIndex(1);
            ImGui.Text("Name");

            // Comparison Column
            if (Context.CompareColumn != null)
            {
                ImGui.TableSetColumnIndex(2);

                var key = Context.CompareColumn.Def.InternalName;
                var name = key;

                if(CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Community)
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
                        name = $"{key} ({fieldAnnotation.Name}";
                    }
                }

                ImGui.Text(name);
            }

            ImGui.EndTable();
        }
    }

    private void DisplayPinnedRowList()
    {
        var pinnedRowList = Editor.Project.Descriptor.PinnedRows
            .GetValueOrDefault(Context.ActiveParam, new List<int>()).Select(id => Context.CurParam[id]).ToList();

        if (pinnedRowList.Count > 0)
        {
            var height = (20 * pinnedRowList.Count);

            ImGui.BeginChild("PinnedRowSection", new Vector2(0, height), ImGuiChildFlags.None);

            var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable;

            if (CFG.Current.ParamEditor_Enable_Table_Borders)
            {
                tblFlags = tblFlags | ImGuiTableFlags.Borders;
            }

            var columnCount = 2;

            if (Context.CompareColumn != null)
            {
                columnCount = 3;
            }

            if (ImGui.BeginTable($"pinnedRowList", columnCount, tblFlags))
            {
                FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);

                ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthStretch, 0.2f);
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
                // Comparison Column
                if (Context.CompareColumn != null)
                {
                    ImGui.TableSetupColumn("Comparison", ImGuiTableColumnFlags.WidthStretch);
                }

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
        ImGui.BeginChild("FullRowSection", ImGuiChildFlags.None);

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

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable;

        if (CFG.Current.ParamEditor_Enable_Table_Borders)
        {
            tblFlags = tblFlags | ImGuiTableFlags.Borders;
        }

        var columnCount = 2;

        if (Context.CompareColumn != null)
        {
            columnCount = 3;
        }

        if (ImGui.BeginTable($"fullRowList", columnCount, tblFlags))
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);

            ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthStretch, 0.2f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthStretch);
            // Comparison Column
            if (Context.CompareColumn != null)
            {
                ImGui.TableSetupColumn("Comparison", ImGuiTableColumnFlags.WidthStretch);
            }

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

            // Rows
            var selectionCache = ParentView.Selection.GetSelectionCache(rows, "regular");

            for (var i = 0; i < rows.Count; i++)
            {
                Param.Row currentRow = rows[i];

                var displayRow = false;

                if (ParentView.ParamTableWindow.IsInTableGroupMode(Context.ActiveParam))
                {
                    if (currentRow.ID == ParentView.ParamTableWindow.CurrentTableGroupID)
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
                    HandleRowPresentation(selectionCache, i, rows, currentRow, false);
                }
            }

            if (Context.DoFocus)
            {
                ImGui.SetScrollFromPosY(Context.ScrollTo - ImGui.GetScrollY());
            }

            ImGui.EndTable();
        }

        ImGui.EndChild();
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

    private void DisplayRowID(bool[] selectionCache, int selectionCacheIndex,
        List<Param.Row> p, Param.Row r, bool isPinned)
    {
        var selected = selectionCache != null && selectionCacheIndex < selectionCache.Length
            ? selectionCache[selectionCacheIndex]
            : false;

        var label = $@"{r.ID}";

        label = Utils.ImGui_WordWrapString(label, ImGui.GetColumnWidth(),
            !CFG.Current.ParamEditor_Row_List_Enable_Line_Wrapping ? 1 : 3);

        if (ID_EditRow == null | ID_EditRow != r)
        {
            if (ImGui.Selectable($@"{label}##{selectionCacheIndex}_id", selected))
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
                            if (ParentView.ParamTableWindow.IsInTableGroupMode(Context.ActiveParam))
                            {
                                if (r2.ID == ParentView.ParamTableWindow.CurrentTableGroupID)
                                {
                                    ClearEditRows();
                                    ParentView.Selection.AddRowToSelection(r2);
                                }
                            }
                            else
                            {
                                ClearEditRows();
                                ParentView.Selection.AddRowToSelection(r2);
                            }
                        }
                    }

                    ClearEditRows();
                    ParentView.Selection.AddRowToSelection(r);
                }
                else if (InputManager.HasAltDown())
                {
                    if (Name_EditRow == null)
                    {
                        ID_EditRow = r;
                    }
                }
                else
                {
                    ClearEditRows();
                    ParentView.Selection.SetActiveRow(r, true);
                }
            }

            if (_arrowKeyPressed && ImGui.IsItemFocused() && r != ParentView.Selection.GetActiveRow())
            {
                if (InputManager.HasCtrlDown())
                {
                    // Add to selection
                    ClearEditRows();
                    ParentView.Selection.AddRowToSelection(r);
                }
                else
                {
                    // Exclusive selection
                    ClearEditRows();
                    ParentView.Selection.SetActiveRow(r, true);
                }

                _arrowKeyPressed = false;
            }

            // Drag source
            if (ImGui.BeginDragDropSource(ImGuiDragDropFlags.None))
            {
                DragSourceRow = r;
                unsafe
                {
                    byte dummy = 0;
                    ImGui.SetDragDropPayload("PARAM_ROW", &dummy, 1);
                }
                ImGui.Text($"Row {r.ID}");
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
        else if(r == ID_EditRow)
        {
            var tempID = r.ID;

            var width = ImGui.GetWindowWidth();
            ImGui.PushItemWidth(width);
            ImGui.InputInt($"##rowIdInput_{r.ID}", ref tempID, 255);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var editCommand = $"selection: ID := {tempID}";
                ParentView.Selection.SortSelection();
                ParentView.MassEdit.ApplyMassEdit(editCommand);
            }
        }
    }

    private void DisplayRow(bool[] selectionCache, int selectionCacheIndex,
        List<Param.Row> p, Param.Row r, bool isPinned)
    {
        var diffVanilla = Context.VanillaDiffCache.Contains(r.ID);
        var auxDiffVanilla = Context.AuxDiffCaches.Where(cache => cache.Item1.Contains(r.ID)).Count() > 0;

        var popColor = true;

        if (diffVanilla)
        {
            // If the auxes are changed 
            var auxDiffPrimaryAndVanilla = (auxDiffVanilla ? 1 : 0) + Context.AuxDiffCaches
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

        var label = $@"{Utils.ImGuiEscape(r.Name)}";

        label = Utils.ImGui_WordWrapString(label, ImGui.GetColumnWidth(),
            !CFG.Current.ParamEditor_Row_List_Enable_Line_Wrapping ? 1 : 3);

        if (ParentView.ParamTableWindow.IsInTableGroupMode(Context.ActiveParam))
        {
            if (CFG.Current.ParamEditor_Table_List_Row_Name_Display_Type is ParamTableRowDisplayType.None)
            {
                // Ignore the option if the Name is empty
                if (r.Name != "")
                {
                    label = $@"{Utils.ImGuiEscape(r.Name)}";
                }
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

        if (Name_EditRow == null | Name_EditRow != r)
        {
            if (ImGui.Selectable($@"{label}##{selectionCacheIndex}_name", selected))
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
                            if (ParentView.ParamTableWindow.IsInTableGroupMode(Context.ActiveParam))
                            {
                                if (r2.ID == ParentView.ParamTableWindow.CurrentTableGroupID)
                                {
                                    ClearEditRows();
                                    ParentView.Selection.AddRowToSelection(r2);
                                }
                            }
                            else
                            {
                                ClearEditRows();
                                ParentView.Selection.AddRowToSelection(r2);
                            }
                        }
                    }

                    ClearEditRows();
                    ParentView.Selection.AddRowToSelection(r);
                }
                else if (InputManager.HasAltDown())
                {
                    if (ID_EditRow == null)
                    {
                        Name_EditRow = r;
                    }
                }
                else
                {
                    ClearEditRows();
                    ParentView.Selection.SetActiveRow(r, true);
                }
            }

            if (_arrowKeyPressed && ImGui.IsItemFocused() && r != ParentView.Selection.GetActiveRow())
            {
                if (InputManager.HasCtrlDown())
                {
                    // Add to selection
                    ClearEditRows();
                    ParentView.Selection.AddRowToSelection(r);
                }
                else
                {
                    // Exclusive selection
                    ClearEditRows();
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
        else if(Name_EditRow == r)
        {
            var tempName = r.Name;

            var width = ImGui.GetWindowWidth();
            ImGui.PushItemWidth(width);
            ImGui.InputText($"##rowNameInput_{r.ID}", ref tempName, 255);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                var editCommand = $"selection: Name := {tempName}";
                ParentView.Selection.SortSelection();
                ParentView.MassEdit.ApplyMassEdit(editCommand);
            }
        }
    }

    public void DisplayContextMenu(string imguiKey, Param.Row r, int selectionCacheIndex, bool isPinned)
    {
        if (ImGui.BeginPopupContextItem($"{imguiKey}_{r.ID}_{selectionCacheIndex}"))
        {
            // Name Input
            if (imguiKey == "id")
            {
                if (CFG.Current.ParamEditor_Row_Context_Display_Row_Name_Input)
                {
                    if (ParentView.Selection.RowSelectionExists())
                    {
                        var id = ParentView.Selection.GetActiveRow().ID;
                        ImGui.InputInt("##newRowId", ref id);

                        if (ImGui.IsItemDeactivatedAfterEdit())
                        {
                            var editCommand = $"selection: ID := {id}";
                            ParentView.Selection.SortSelection();
                            ParentView.MassEdit.ApplyMassEdit(editCommand);
                        }
                    }
                }
            }

            if (imguiKey == "name")
            {
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
                UIHelper.Tooltip($"Pin the current row selection to the top of the row list.");
            }
            // Unpin
            else if (isPinned)
            {
                if (ImGui.Selectable($"Unpin"))
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
                UIHelper.Tooltip($"Unpin the current row selection from top of the row list.");
            }

            // Decorator Options (e.g. Go to Text)
            if (Context.FmgRowDecorator != null)
            {
                Context.FmgRowDecorator.DecorateContextMenuItems(r);
            }

            // Advanced Contextual Actions
            if (CFG.Current.ParamEditor_Row_Context_Display_Advanced_Options)
            {
                // Quick Search
                if (CFG.Current.ParamEditor_Row_Context_Display_Finder_Quick_Option)
                {
                    ParamRowTools.ParamQuickSearch(ParentView, Context.ActiveParam, r.ID);
                }

                // Comparison Options
                if (ImGui.Selectable("Compare Row"))
                {
                    ParentView.Selection.SetCompareRow(r);
                }
                UIHelper.Tooltip($"Set this row as the row comparison target within the field window.");

                // Reverse Lookup Options
                ParamRowTools.ParamReverseLookup_Value(ParentView, Context.ActiveParam, r.ID);

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
