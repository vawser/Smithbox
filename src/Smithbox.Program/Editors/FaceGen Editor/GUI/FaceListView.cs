#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The face rows: a face param's, picked at the top when the game has two, filtered by ID or name and by whether any
/// character uses them. The arrow keys step through the faces.
/// <para>
/// As in the Param Editor's row list, several rows can be selected (Ctrl+click, Shift+click, Select All) and copied,
/// pasted, deleted, duplicated, renamed, given a new ID or reverted to vanilla, through the Param Editor's own row
/// operations and undo history. The other windows show the active row: the one last clicked.
/// </para>
/// </summary>
public class FaceListView
{
    private enum UsageFilter
    {
        All,
        Used,
        Unused
    }

    private readonly FaceGenEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;
    private UsageFilter Usage = UsageFilter.All;

    // The rows that pass the filters, made again when the param, the filters or the param's rows change (any edit,
    // undo or redo can rename, renumber, add or remove a row)
    private List<Param.Row> Rows = new();
    private (Param? Param, int Count, string Filter, bool Exact, UsageFilter Usage, EditorAction? LastAction) RowsFor;

    // The selected rows of the shown param. The active row is the range anchor for Shift+click.
    private readonly HashSet<Param.Row> Selected = new();

    // Context menu inputs: the name and ID to give every selected row
    private string ContextNameBuffer = "";
    private int ContextIdBuffer;

    private bool OpenPasteMenu;
    private bool PasteMenuOpen;

    public bool ScrollToSelection;

    private const string PastePopupId = "faceGenPastePopup";

    // "Assign to Characters": gives the face to character rows
    private readonly FaceAssignDialog AssignDialog;

    public FaceListView(FaceGenEditorView view)
    {
        View = view;
        AssignDialog = new FaceAssignDialog(view);
    }

    private ParamEditorScreen? ParamEditor => View.Editor.ParamEditor;

    private string? ParamName => View.ParamName;

    public void ClearSelection() => Selected.Clear();

    /// <summary>Adds rows to the selection (e.g. the variations just made), keeping the active row</summary>
    public void SelectMany(IEnumerable<Param.Row> rows) => Selected.UnionWith(rows);

    /// <summary>Shows only the faces no character wears (the Face Check tool's link)</summary>
    public void ShowUnused()
    {
        Usage = UsageFilter.Unused;
        ScrollToSelection = true;
    }

    /// <summary>The selected rows of the shown param, in param order (or the active row, if none are selected)</summary>
    public List<Param.Row> GetSelectedRows() =>
        View.ActiveParam is { } param ? GetSelectedRows(param) : new List<Param.Row>();

    /// <summary>Makes <paramref name="row"/> the only selected row (it has just been shown)</summary>
    public void SelectOnly(Param.Row row)
    {
        Selected.Clear();
        Selected.Add(row);
    }

    public void Display()
    {
        var project = View.Project;
        var paramNames = FaceGenParams.GetFaceParams(project);

        if (paramNames.Count == 0)
        {
            GUI.WrappedText(LOC.Get("FACEGEN_List_No_Params"));
            return;
        }

        if (ParamName == null || !paramNames.Contains(ParamName))
            View.ShowParam(paramNames[0]);

        // Only shown when there is a choice: the games from BB to SDT have both kinds of face param
        if (paramNames.Count > 1)
        {
            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo("##faceGenParam", ParamName))
            {
                foreach (var name in paramNames)
                {
                    if (ImGui.Selectable($"{name}##{name}", name == ParamName))
                        View.ShowParam(name);
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("FACEGEN_List_Param_TT"));
        }

        EditorFilters.DisplayFramedListFilter("faceGenFilter", ref Filter, ref ExactFilter);

        var isReferenced = ParamName == FaceGenParams.GetReferencedFaceParam(project);

        // Which faces the characters use is only known for the param they point at
        if (isReferenced)
        {
            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo("##faceGenUsage", UsageLabel(Usage)))
            {
                foreach (var usage in Enum.GetValues<UsageFilter>())
                {
                    if (ImGui.Selectable($"{UsageLabel(usage)}##faceGenUsage{usage}", usage == Usage))
                        Usage = usage;
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("FACEGEN_List_Usage_TT"));
        }

        var param = View.ActiveParam;

        if (param == null)
            return;

        var users = isReferenced ? FaceGenParams.GetUsers(project) : null;

        UpdateRows(param, users);

        if (ImGui.Button($"{LOC.Get("FACEGEN_List_New_Face")}##faceGenNewFace", new System.Numerics.Vector2(-1, 0)))
            NewFace(param);
        GUI.Tooltip(LOC.Get("FACEGEN_List_New_Face_TT"));

        ImGui.TextDisabled(LOC.Get("FACEGEN_List_Count", Rows.Count, param.Rows.Count));

        if (ImGui.BeginChild("faceGenRows", default, ImGuiChildFlags.Borders))
        {
            DisplayRows(param, users);
        }

        ImGui.EndChild();

        DisplayPasteMenu();
        AssignDialog.Display();
    }

    private static string UsageLabel(UsageFilter usage) => usage switch
    {
        UsageFilter.Used => LOC.Get("FACEGEN_List_Usage_Used"),
        UsageFilter.Unused => LOC.Get("FACEGEN_List_Usage_Unused"),
        _ => LOC.Get("FACEGEN_List_Usage_All")
    };

    private void UpdateRows(Param param, FaceGenParams.FaceUsers? users)
    {
        var usage = users != null ? Usage : UsageFilter.All;
        var key = (param, param.Rows.Count, Filter, ExactFilter, usage, ParamEditor?.ActionManager.PeekUndoAction());

        if (key == RowsFor)
            return;

        RowsFor = key;

        var filter = Filter.Trim();

        Rows = param.Rows
            .Where(row => filter.Length == 0 || Matches(row, filter, ExactFilter))
            .Where(row => usage == UsageFilter.All || users!.Get(row.ID).Count > 0 == (usage == UsageFilter.Used))
            .ToList();

        // Rows that were deleted (or belong to another param) can't stay selected
        if (Selected.Count > 0)
        {
            var inParam = new HashSet<Param.Row>(param.Rows);
            Selected.RemoveWhere(row => !inParam.Contains(row));

            if (Selected.Count == 0 && View.ActiveRow is { } active && inParam.Contains(active))
                Selected.Add(active);
        }
    }

    private static bool Matches(Param.Row row, string filter, bool exact)
    {
        return EditorFilters.IsMatch(filter, row.ID.ToString(), exact, row.Name ?? "");
    }

    private void DisplayRows(Param param, FaceGenParams.FaceUsers? users)
    {
        var active = View.ActiveRow;
        var startY = ImGui.GetCursorPosY();

        if (ScrollToSelection && active != null && Rows.IndexOf(active) is >= 0 and var activeIndex)
        {
            GUI.ScrollClippedRowIntoView(startY, activeIndex);
            ScrollToSelection = false;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(Rows.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var row = Rows[i];
                var isSelected = Selected.Contains(row);

                var clicked = ImGui.Selectable($"{FaceGenParams.RowLabel(row.ID, row.Name)}##faceGenRow{i}", isSelected);

                HandleRowInput(clicked, row);

                DisplayContextMenu(param, row, i);

                // How many characters wear the face, at the end of the line
                if (users != null && users.Get(row.ID).Count is > 0 and var count)
                {
                    var text = LOC.Get("FACEGEN_List_User_Count", count);
                    ImGui.SameLine();
                    var room = ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(text).X;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0f, room));
                    ImGui.TextDisabled(text);
                }
            }
        }

        clipper.End();
    }

    /// <summary>
    /// A click shows the row; Ctrl+click adds it to or takes it from the selection; Shift+click selects the rows
    /// from the active row to it. A right-click outside the selection selects the row first, so the context menu
    /// acts on the row under the mouse; inside a multi-selection it keeps the selection.
    /// </summary>
    private void HandleRowInput(bool clicked, Param.Row row)
    {
        if (clicked)
        {
            if (InputManager.HasCtrlDown() && Selected.Count > 0)
            {
                if (!Selected.Remove(row))
                    Selected.Add(row);
            }
            else if (InputManager.HasShiftDown() && SelectRangeTo(row))
            {
            }
            else
            {
                View.Select(ParamName!, row);
                ScrollToSelection = false;
            }
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && !Selected.Contains(row))
        {
            View.Select(ParamName!, row);
            ScrollToSelection = false;
        }
    }

    /// <summary>Selects the shown rows from the active row to <paramref name="target"/>. False if there is no active row shown.</summary>
    private bool SelectRangeTo(Param.Row target)
    {
        var start = View.ActiveRow != null ? Rows.IndexOf(View.ActiveRow) : -1;
        var end = Rows.IndexOf(target);

        if (start < 0 || end < 0)
            return false;

        Selected.Clear();

        for (var i = Math.Min(start, end); i <= Math.Max(start, end); i++)
            Selected.Add(Rows[i]);

        return true;
    }

    /// <summary>
    /// The selected rows that are in <paramref name="param"/>, in param order (or the active row, if none are
    /// selected)
    /// </summary>
    private List<Param.Row> GetSelectedRows(Param param)
    {
        var rows = Selected.ToList();

        if (rows.Count == 0 && View.ActiveRow is { } active)
            rows.Add(active);

        return rows.Where(row => param.IndexOfRow(row) >= 0).OrderBy(param.IndexOfRow).ToList();
    }

    // ---- new face

    /// <summary>
    /// A new face: a copy of the active face (or of the first face, if none is shown), placed as the Param Editor's
    /// duplicate options say, and shown
    /// </summary>
    private void NewFace(Param param)
    {
        if (ParamEditor is not { } editor)
            return;

        var source = View.ActiveRow ?? param.Rows.FirstOrDefault();

        if (source == null)
            return;

        var created = ParamRowDuplicate.DuplicateRows(editor, param, new[] { source });

        if (created.Count > 0)
            View.Select(ParamName!, created[0]);
    }

    // ---- context menu

    private void DisplayContextMenu(Param param, Param.Row row, int index)
    {
        if (!ImGui.BeginPopupContextItem($"faceGenRowContext{index}"))
            return;

        var editor = ParamEditor;
        var selectedRows = GetSelectedRows(param);
        var hasSelection = selectedRows.Count > 0;
        var sourceRow = (View.ActiveRow is { } active && selectedRows.Contains(active) ? active : null)
            ?? selectedRows.FirstOrDefault();

        if (ImGui.IsWindowAppearing() && sourceRow != null)
        {
            ContextNameBuffer = sourceRow.Name ?? "";
            ContextIdBuffer = sourceRow.ID;
        }

        if (editor != null && hasSelection)
        {
            // Name / ID inputs: these apply to every selected row (e.g. to rename a batch)
            ImGui.InputText("##faceGenRowName", ref ContextNameBuffer, 1024);
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Edit_Applies_To_Selection", selectedRows.Count));

            if (GUI.InputCommitted())
                ParamRowOperations.SetRowName(editor, selectedRows, ContextNameBuffer);

            ImGui.InputInt("##faceGenRowId", ref ContextIdBuffer);
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Edit_Applies_To_Selection", selectedRows.Count));

            if (GUI.InputCommitted())
                ParamRowOperations.SetRowID(editor, selectedRows, ContextIdBuffer);

            // Every selected row gets this one ID, which is easy to do by accident
            if (selectedRows.Count > 1)
            {
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color,
                    LOC.Get("PARAM_RowWindow_Context_Same_ID_Warning", selectedRows.Count));
            }
        }

        var rowFlags = editor != null && hasSelection ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled;

        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy")}##faceGenCopy", false, rowFlags))
            CopySelection(param);
        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Copy_TT", InputManager.GetHint(KeybindID.Copy)));

        var canPaste = editor != null && View.Project.Handler.ParamData.PrimaryBank.ClipboardRows.Any();

        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Paste")}##faceGenPaste", false,
                canPaste ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            OpenPasteMenu = true;
        }
        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Paste_TT", InputManager.GetHint(KeybindID.Paste)));

        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Delete")}##faceGenDelete", false, rowFlags))
            DeleteSelection(param);
        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Delete_TT", InputManager.GetHint(KeybindID.Delete)));

        if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Duplicate_Header")}##faceGenDuplicateMenu"))
        {
            ImGui.InputInt($"{LOC.Get("PARAM_RowWindow_Context_DuplicateOffset")}##faceGenDuplicateOffset",
                ref CFG.Current.Param_Toolbar_Duplicate_Offset);
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_DuplicateOffset_TT"));

            ImGui.InputInt($"{LOC.Get("PARAM_RowWindow_Context_DuplicateAmount")}##faceGenDuplicateAmount",
                ref CFG.Current.Param_Toolbar_Duplicate_Amount);
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_DuplicateAmount_TT"));

            ImGui.Separator();

            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_DuplicateApply")}##faceGenDuplicateApply", false, rowFlags))
                DuplicateSelection(param);
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_DuplicateApply_TT", InputManager.GetHint(KeybindID.Duplicate)));

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Value_Header")}##faceGenValueMenu"))
        {
            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Revert_to_Default")}##faceGenRevert", false, rowFlags))
                ParamRowOperations.SetRowToDefault(editor!, ParamName!, selectedRows);
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Revert_to_Default_TT"));

            ImGui.EndMenu();
        }

        if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Info_Header")}##faceGenInfoMenu"))
        {
            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID")}##faceGenCopyId"))
                CopyText(selectedRows, r => r.ID.ToString());
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID_TT"));

            // Unnamed rows copy as an empty line, so the line count still matches the selection
            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy_Name")}##faceGenCopyName"))
                CopyText(selectedRows, r => r.Name ?? "");
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Copy_Name_TT"));

            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID_And_Name")}##faceGenCopyIdAndName"))
                CopyText(selectedRows, r => $"{r.ID};{r.Name ?? ""}");
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID_And_Name_TT"));

            ImGui.EndMenu();
        }

        ImGui.Separator();

        // Only the face param characters point at can be worn
        var canAssign = editor != null && ParamName == FaceGenParams.GetReferencedFaceParam(View.Project);

        if (ImGui.Selectable($"{LOC.Get("FACEGEN_Assign_Menu")}##faceGenAssign", false,
                canAssign ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            AssignDialog.Open(row.ID);
        }
        GUI.Tooltip(LOC.Get(canAssign ? "FACEGEN_Assign_Menu_TT" : "FACEGEN_Assign_Menu_Other_Param_TT"));

        if (ImGui.Selectable($"{LOC.Get("FACEGEN_Open_In_Param_Editor")}##openInParamEditor"))
            EditorCommandQueue.AddCommand($"param/select/-1/{ParamName}/{row.ID}");

        ImGui.Separator();

        ImGui.Text(LOC.Get("PARAM_RowWindow_Context_Row_Selection_Count_TT", selectedRows.Count));

        ImGui.EndPopup();
    }

    private static void CopyText(List<Param.Row> rows, Func<Param.Row, string> line)
    {
        var builder = new StringBuilder();

        foreach (var row in rows)
            builder.AppendLine(line(row));

        PlatformUtils.Instance.SetClipboardText(builder.ToString());
    }

    // ---- row operations, through the Param Editor's (so they share its undo history)

    private void CopySelection(Param param)
    {
        var rows = GetSelectedRows(param);

        if (ParamEditor is { } editor && rows.Count > 0)
            editor.Clipboard.CopyRowsToClipboard(ParamName!, rows);
    }

    private void DuplicateSelection(Param param)
    {
        if (ParamEditor is not { } editor)
            return;

        var created = ParamRowDuplicate.DuplicateRows(editor, param, GetSelectedRows(param));

        if (created.Count > 0)
        {
            View.Select(ParamName!, created[0]);
            Selected.UnionWith(created);
        }
    }

    /// <summary>Deletes the selected rows, and shows the row that takes their place in the list</summary>
    private void DeleteSelection(Param param)
    {
        if (ParamEditor is not { } editor)
            return;

        var rows = GetSelectedRows(param);

        if (rows.Count == 0)
            return;

        var next = FindRowAfterDeletion(rows);

        if (!ParamRowDelete.ApplyDelete(editor, param, rows))
            return;

        Selected.Clear();

        if (next != null)
            View.Select(ParamName!, next);
    }

    /// <summary>
    /// The first shown row after the last deleted row, or failing that the last shown row before the first one
    /// </summary>
    private Param.Row? FindRowAfterDeletion(List<Param.Row> deleted)
    {
        var deletedSet = new HashSet<Param.Row>(deleted);
        var indices = deleted.Select(row => Rows.IndexOf(row)).Where(i => i >= 0).ToList();

        if (indices.Count == 0)
            return null;

        for (var i = indices.Max() + 1; i < Rows.Count; i++)
        {
            if (!deletedSet.Contains(Rows[i]))
                return Rows[i];
        }

        for (var i = indices.Min() - 1; i >= 0; i--)
        {
            if (!deletedSet.Contains(Rows[i]))
                return Rows[i];
        }

        return null;
    }

    private void SelectAll()
    {
        if (Rows.Count == 0)
            return;

        if (View.ActiveRow == null || !Rows.Contains(View.ActiveRow))
            View.Select(ParamName!, Rows[0]);

        Selected.UnionWith(Rows);
    }

    /// <summary>Shows the next or previous face of the list, so the faces can be looked through one by one</summary>
    private void Step(int direction)
    {
        if (Rows.Count == 0)
            return;

        var index = View.ActiveRow != null ? Rows.IndexOf(View.ActiveRow) : -1;
        var next = index < 0 ? 0 : Math.Clamp(index + direction, 0, Rows.Count - 1);

        if (next != index)
            View.Select(ParamName!, Rows[next]);
    }

    // ---- paste: the Param Editor's paste menu

    private void DisplayPasteMenu()
    {
        if (OpenPasteMenu)
        {
            ImGui.OpenPopup(PastePopupId);
            OpenPasteMenu = false;
        }

        PasteMenuOpen = false;

        if (ParamEditor is not { } editor || !ImGui.BeginPopup(PastePopupId))
            return;

        PasteMenuOpen = true;

        var param = View.ActiveParam;
        var selectedRows = param != null ? GetSelectedRows(param) : new List<Param.Row>();

        // Only another face param can be gone to here; rows of any other param are pasted in the Param Editor
        var clipboardParam = View.Project.Handler.ParamData.PrimaryBank.ClipboardParam;
        Action<string>? goToParam = FaceGenParams.IsFaceParam(View.Project, clipboardParam)
            ? View.ShowParam
            : null;

        var pasted = editor.PasteMenu.DisplayPasteControls(ParamName!, selectedRows, goToParam);

        if (pasted != null)
        {
            if (CFG.Current.Param_PasteThenSelect && pasted.Count > 0)
            {
                View.Select(ParamName!, pasted[0]);
                Selected.UnionWith(pasted);
            }

            ImGui.CloseCurrentPopup();
        }

        ImGui.EndPopup();
    }

    // ---- shortcuts, while the list is focused

    public void Shortcuts()
    {
        // The paste menu's own keys are its fields'
        if (PasteMenuOpen || AssignDialog.IsOpen || View.ActiveParam is not { } param)
            return;

        if (GUI.ListStepKey() is not 0 and var step)
            Step(step);

        if (InputManager.IsPressed(KeybindID.SelectAll))
            SelectAll();

        if (InputManager.IsPressed(KeybindID.Copy))
            CopySelection(param);

        if (InputManager.IsPressed(KeybindID.Paste) && View.Project.Handler.ParamData.PrimaryBank.ClipboardRows.Any())
            OpenPasteMenu = true;

        if (InputManager.IsPressed(KeybindID.Duplicate))
            DuplicateSelection(param);

        if (InputManager.IsPressed(KeybindID.Delete))
            DeleteSelection(param);
    }
}
