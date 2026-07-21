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

    public string TargetString = "";
    public string ReplaceString = "";

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

        GUI.SimpleHeader($"{rowListTitle}", "");
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
        ImGui.InputTextWithHint($"##rowSearch", LOC.Get("PARAM_RowWindow_Search_Hint"), 
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

        // Go to selected
        ImGui.AlignTextToFramePadding();
        if (ImGui.Button($"{Icons.LocationArrow}") ||
            Context.IsActiveView && InputManager.IsPressed(KeybindID.Jump))
        {
            ParentView.JumpToSelectedRow = true;
        }
        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Action_Go_To_TT", InputManager.GetHint(KeybindID.Jump)));

        ImGui.SameLine();

        // Mass Edit Hint
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

        // Toggle Modified Background
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}"))
        {
            CFG.Current.ParamEditor_Row_List_Display_Modified_Row_Bg = !CFG.Current.ParamEditor_Row_List_Display_Modified_Row_Bg;
        }

        var rowModifiedBgMode = LOC.Get("PARAM_RowWindow_Toggle_Modified_Background_Hidden");
        if (CFG.Current.ParamEditor_Row_List_Display_Modified_Row_Bg)
            rowModifiedBgMode = LOC.Get("PARAM_RowWindow_Toggle_Modified_Background_Visible");

        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Toggle_Modified_Background_Hint", rowModifiedBgMode));

        // Display Decorators
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.FileText}"))
        {
            CFG.Current.ParamEditor_Row_List_Display_Decorators = !CFG.Current.ParamEditor_Row_List_Display_Decorators;
        }

        var displayDecoratorMode = LOC.Get("PARAM_RowWindow_Toggle_Text_Decorator_Hidden");
        if (CFG.Current.ParamEditor_Row_List_Display_Decorators)
            displayDecoratorMode = LOC.Get("PARAM_RowWindow_Toggle_Text_Decorator_Visible");

        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Toggle_Text_Decorator_Hint", displayDecoratorMode));        

        // Quick Export
        if (CFG.Current.Developer_Enable_Tools)
        {
            ParamDebugTools.DisplayQuickRowNameExport(Editor, Project);
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
                    name = $"{key} ({fieldAnnotation.Name}";
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
            var height = 20 + (20 * pinnedRowList.Count);

            // Limit height and enable scrollbar beyond this height
            if (height > 250)
                height = 250;

            ImGui.BeginChild("PinnedRowSection", new Vector2(0, height), ImGuiChildFlags.None, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

            var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY ;

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
            if (popColor)
            {
                ImGui.PopStyleColor(1);
            }

            var tempName = r.Name;

            var width = ImGui.GetWindowWidth();
            ImGui.PushItemWidth(width);

            var input = new DelayedInputTextHandler(tempName);

            if (input.Draw($"##rowNameInput_{r.ID}", out string newValue))
            {
                var editCommand = $"selection: Name := {newValue}";
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

                // Unpin All
                if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Unpin_All")}##unpinAllAction"))
                {
                    Editor.Project.Descriptor.PinnedRows.Clear();
                }
                GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Unpin_All_TT"));

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

            if (imguiKey == "name")
            {
                // Name Manipulation
                if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Name_Manipulation_Header")}##nameManipMenuHeader"))
                {
                    // Adjust
                    if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Adjust_Header")}##adjustMenuHeader"))
                    {
                        // Clear Text from Name
                        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Clear_Text_From_Name")}##clearTextAction", false,
                            ParentView.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                        {
                            ParamRowOperations.AdjustRowName(ParentView, NameAdjustment, ParamRowNameAdjustType.Clear);
                        }
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Clear_Text_From_Name_TT"));

                        // Prepend Text from Name
                        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Prepend_Text_To_Name")}##prependTextAction", false,
                            ParentView.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                        {
                            ParamRowOperations.AdjustRowName(ParentView, NameAdjustment, ParamRowNameAdjustType.Prepend);
                        }
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Prepend_Text_To_Name_TT"));

                        // Postpend Text from Name
                        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Postpend_Text_To_Name")}##postpendTextAction", false,
                                ParentView.Selection.RowSelectionExists()
                                    ? ImGuiSelectableFlags.None
                                    : ImGuiSelectableFlags.Disabled))
                        {
                            ParamRowOperations.AdjustRowName(ParentView, NameAdjustment, ParamRowNameAdjustType.Postpend);
                        }
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Postpend_Text_To_Name_TT"));

                        // Remove Text from Name
                        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Remove_Text_To_Name")}##removeTextAction", false,
                                ParentView.Selection.RowSelectionExists()
                                    ? ImGuiSelectableFlags.None
                                    : ImGuiSelectableFlags.Disabled))
                        {
                            ParamRowOperations.AdjustRowName(ParentView, NameAdjustment, ParamRowNameAdjustType.Remove);
                        }
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Remove_Text_To_Name_TT"));

                        // Text to Apply
                        ImGui.InputText($"{LOC.Get("PARAM_RowWindow_Context_Text_To_Apply_Input")}##nameAdjustment", ref NameAdjustment, 255);
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Text_To_Apply_Input_TT"));

                        ImGui.EndMenu();

                    }

                    // Inherit
                    if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Inherit_Header")}##inheritMenuHeader"))
                    {
                        // Proliferate name into Param Reference
                        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Proliferate_Name")}##proliferateNameAction", false,
                            ParentView.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                        {
                            ParamRowOperations.ProliferateRowName(ParentView, TargetField);
                        }
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Proliferate_Name_TT"));

                        // Inherit Name from Param Reference
                        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_Ref")}##inheritNameParamRefAction", false,
                                ParentView.Selection.RowSelectionExists()
                                    ? ImGuiSelectableFlags.None
                                    : ImGuiSelectableFlags.Disabled))
                        {
                            ParamRowOperations.InheritRowName(ParentView, TargetField);
                        }
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_Ref_TT"));

                        // Inherit Name from FMG Refernece
                        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_FMG")}##inheritNameFmgRefAction", false,
                                ParentView.Selection.RowSelectionExists()
                                    ? ImGuiSelectableFlags.None
                                    : ImGuiSelectableFlags.Disabled))
                        {
                            ParamRowOperations.InheritRowNameFromFMG(ParentView, TargetField);
                        }
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_FMG_TT"));

                        // Inherit Name from Alias
                        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_Alias")}##inheritNameAliasRefAction", false,
                                ParentView.Selection.RowSelectionExists()
                                    ? ImGuiSelectableFlags.None
                                    : ImGuiSelectableFlags.Disabled))
                        {
                            ParamRowOperations.InheritRowNameFromAlias(ParentView, TargetField);
                        }
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_Alias_TT"));

                        // Target Field
                        ImGui.InputText($"{LOC.Get("PARAM_RowWindow_Context_Target_Field")}##targetField", ref TargetField, 255);
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Target_Field_TT"));

                        ImGui.EndMenu();
                    }

                    // Replace
                    if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Replace_Header")}##replaceMenuHeader"))
                    {
                        // Target String
                        ImGui.InputText($"{LOC.Get("PARAM_RowWindow_Context_Target_String")}##targetString", ref TargetString, 255);
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Target_String_TT"));

                        // Replacement String
                        ImGui.InputText($"{LOC.Get("PARAM_RowWindow_Context_Replace_String")}##replaceString", ref ReplaceString, 255);
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Replace_String_TT"));

                        // Replace
                        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Replace")}##replaceStringAction", false,
                                ParentView.Selection.RowSelectionExists()
                                    ? ImGuiSelectableFlags.None
                                    : ImGuiSelectableFlags.Disabled))
                        {
                            ParamRowOperations.ReplaceStringInRowName(ParentView, TargetString, ReplaceString);
                        }
                        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Replace_TT"));

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenu();
                }
            }

            // Information
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Info_Header")}##infoMenuHeader"))
            {
                // Copy ID
                if(ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID")}##copyIdAction"))
                {
                    var selection = ParentView.Selection.GetSelectedRows();
                    StringBuilder _builder = new();
                    foreach(var entry in selection)
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
                        _builder.AppendLine(entry.Name.ToString());
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
                        _builder.AppendLine($"{entry.ID.ToString()};{entry.Name.ToString()}");
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

    public void SetNameManpulationTargetField(string internalName)
    {
        TargetField = internalName;
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
