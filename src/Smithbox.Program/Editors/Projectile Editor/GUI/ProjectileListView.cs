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

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The rows the editor can show: a bullet param's, or a spell param's, picked at the top and filtered by ID or name.
/// <para>
/// As in the Param Editor's row list, several rows can be selected (Ctrl+click, Shift+click, Select All) and copied,
/// pasted, deleted, duplicated, renamed, given a new ID or reverted to vanilla, through the Param Editor's own row
/// operations and undo history. The simulation shows the active row: the one last clicked.
/// </para>
/// </summary>
public class ProjectileListView
{
    private readonly ProjectileEditorView View;

    private string? ParamName;
    private string Filter = "";

    // The rows that pass the filter, made again when the param, the filter or the param's rows change (any edit,
    // undo or redo can rename, renumber, add or remove a row)
    private List<Param.Row> Rows = new();
    private (Param? Param, int Count, string Filter, EditorAction? LastAction) RowsFor;

    // The selected rows of the shown param. The active row (the simulated one) is the range anchor for Shift+click.
    private readonly HashSet<Param.Row> Selected = new();

    // Context menu inputs: the name and ID to give every selected row
    private string ContextNameBuffer = "";
    private int ContextIdBuffer;

    private bool OpenPasteMenu;
    private bool PasteMenuOpen;

    public bool ScrollToSelection;

    private const string PastePopupId = "projectilePastePopup";

    public ProjectileListView(ProjectileEditorView view)
    {
        View = view;
    }

    private ParamEditorScreen? ParamEditor => View.Editor.ParamEditor;

    public void ShowParam(string paramName)
    {
        if (ParamName != paramName)
            Selected.Clear();

        ParamName = paramName;
    }

    /// <summary>Makes <paramref name="row"/> the only selected row (it has just been shown)</summary>
    public void SelectOnly(Param.Row row)
    {
        Selected.Clear();
        Selected.Add(row);
    }

    public void Display()
    {
        var project = View.Project;
        var paramNames = ProjectileParams.GetProjectileParams(project);

        if (paramNames.Count == 0)
        {
            GUI.WrappedText(LOC.Get("PROJECTILE_List_No_Params"));
            return;
        }

        // The player's bullets first, where the game has a param of its own for them
        if (ParamName == null || !paramNames.Contains(ParamName))
            ShowParam(paramNames.FirstOrDefault(name => name is "Bullet" or "BulletParam") ?? paramNames[0]);

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##projectileParam", ParamName))
        {
            foreach (var name in paramNames)
            {
                var label = ProjectileParams.IsSpellParam(project, name) ? $"{name} ({LOC.Get("PROJECTILE_List_Spell")})" : name;

                if (ImGui.Selectable($"{label}##{name}", name == ParamName))
                    ShowParam(name);
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("PROJECTILE_List_Param_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##projectileFilter", LOC.Get("PROJECTILE_List_Filter_Hint"), ref Filter, 256);

        var param = ProjectileParams.GetParam(project, ParamName!);

        if (param == null)
            return;

        UpdateRows(param);

        ImGui.TextDisabled(LOC.Get("PROJECTILE_List_Count", Rows.Count, param.Rows.Count));

        if (ImGui.BeginChild("projectileRows", default, ImGuiChildFlags.Borders))
        {
            DisplayRows(param);
        }

        ImGui.EndChild();

        DisplayPasteMenu();
    }

    private void UpdateRows(Param param)
    {
        var key = (param, param.Rows.Count, Filter, ParamEditor?.ActionManager.PeekUndoAction());

        if (key == RowsFor)
            return;

        RowsFor = key;

        var filter = Filter.Trim();

        Rows = filter.Length == 0
            ? param.Rows.ToList()
            : param.Rows.Where(row => Matches(row, filter)).ToList();

        // Rows that were deleted (or belong to another param) can't stay selected
        if (Selected.Count > 0)
        {
            var inParam = new HashSet<Param.Row>(param.Rows);
            Selected.RemoveWhere(row => !inParam.Contains(row));

            // The params were reloaded: the simulation took the reloaded row with the same ID, so it stays selected
            if (Selected.Count == 0 && View.Simulator.ParamName == ParamName && View.Simulator.Row is { } active &&
                inParam.Contains(active))
            {
                Selected.Add(active);
            }
        }
    }

    private static bool Matches(Param.Row row, string filter)
    {
        return row.ID.ToString().Contains(filter, StringComparison.Ordinal) ||
               row.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true;
    }

    private void DisplayRows(Param param)
    {
        var active = View.Simulator.ParamName == ParamName ? View.Simulator.Row : null;
        var activeIndex = ScrollToSelection && active != null
            ? Rows.IndexOf(active)
            : -1;

        // The row to scroll to is brought into the clipper's range, then into view
        if (activeIndex >= 0)
        {
            ImGui.SetScrollY(activeIndex * ImGui.GetTextLineHeightWithSpacing() - ImGui.GetWindowHeight() * 0.4f);
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

                var clicked = ImGui.Selectable($"{ProjectileParams.RowLabel(row.ID, row.Name)}##projectileRow{i}", isSelected);

                HandleRowInput(clicked, row);

                DisplayContextMenu(param, row, i);
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
            }
        }

        if (ImGui.IsItemClicked(ImGuiMouseButton.Right) && !Selected.Contains(row))
            View.Select(ParamName!, row);
    }

    /// <summary>Selects the shown rows from the active row to <paramref name="target"/>. False if there is no active row shown.</summary>
    private bool SelectRangeTo(Param.Row target)
    {
        var active = View.Simulator.ParamName == ParamName ? View.Simulator.Row : null;
        var start = active != null ? Rows.IndexOf(active) : -1;
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

        if (rows.Count == 0 && View.Simulator.ParamName == ParamName && View.Simulator.Row is { } active)
            rows.Add(active);

        var order = new Dictionary<Param.Row, int>();
        var index = 0;

        foreach (var row in param.Rows)
            order[row] = index++;

        return rows.Where(order.ContainsKey).OrderBy(row => order[row]).ToList();
    }

    // ---- context menu

    private void DisplayContextMenu(Param param, Param.Row row, int index)
    {
        if (!ImGui.BeginPopupContextItem($"projectileRowContext{index}"))
            return;

        var editor = ParamEditor;
        var selectedRows = GetSelectedRows(param);
        var hasSelection = selectedRows.Count > 0;
        var sourceRow = (View.Simulator.Row is { } active && selectedRows.Contains(active) ? active : null)
            ?? selectedRows.FirstOrDefault();

        if (ImGui.IsWindowAppearing() && sourceRow != null)
        {
            ContextNameBuffer = sourceRow.Name ?? "";
            ContextIdBuffer = sourceRow.ID;
        }

        if (editor != null && hasSelection)
        {
            // Name / ID inputs: these apply to every selected row (e.g. to rename a batch)
            ImGui.InputText("##projectileRowName", ref ContextNameBuffer, 1024);
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Edit_Applies_To_Selection", selectedRows.Count));

            if (GUI.InputCommitted())
                ParamRowOperations.SetRowName(editor, selectedRows, ContextNameBuffer);

            ImGui.InputInt("##projectileRowId", ref ContextIdBuffer);
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

        // Copy
        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy")}##projectileCopy", false, rowFlags))
            CopySelection(param);
        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Copy_TT", InputManager.GetHint(KeybindID.Copy)));

        // Paste
        var canPaste = editor != null && View.Project.Handler.ParamData.PrimaryBank.ClipboardRows.Any();

        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Paste")}##projectilePaste", false,
                canPaste ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            OpenPasteMenu = true;
        }
        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Paste_TT", InputManager.GetHint(KeybindID.Paste)));

        // Delete
        if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Delete")}##projectileDelete", false, rowFlags))
            DeleteSelection(param);
        GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Delete_TT", InputManager.GetHint(KeybindID.Delete)));

        // Duplicate
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Duplicate_Header")}##projectileDuplicateMenu"))
        {
            ImGui.InputInt($"{LOC.Get("PARAM_RowWindow_Context_DuplicateOffset")}##projectileDuplicateOffset",
                ref CFG.Current.Param_Toolbar_Duplicate_Offset);
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_DuplicateOffset_TT"));

            ImGui.InputInt($"{LOC.Get("PARAM_RowWindow_Context_DuplicateAmount")}##projectileDuplicateAmount",
                ref CFG.Current.Param_Toolbar_Duplicate_Amount);
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_DuplicateAmount_TT"));

            // Reference fields to offset (e.g. the attack a bullet uses), and which fields will change
            ParamRowDuplicate.DisplayReferenceOffsetOptions(View.Project.Handler.ParamData, param);

            ImGui.Separator();

            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_DuplicateApply")}##projectileDuplicateApply", false, rowFlags))
                DuplicateSelection(param);
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_DuplicateApply_TT", InputManager.GetHint(KeybindID.Duplicate)));

            ImGui.EndMenu();
        }

        // Value
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Value_Header")}##projectileValueMenu"))
        {
            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Revert_to_Default")}##projectileRevert", false, rowFlags))
                ParamRowOperations.SetRowToDefault(editor!, ParamName!, selectedRows);
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Revert_to_Default_TT"));

            ImGui.EndMenu();
        }

        // Information
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Info_Header")}##projectileInfoMenu"))
        {
            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID")}##projectileCopyId"))
                CopyText(selectedRows, r => r.ID.ToString());
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID_TT"));

            // Unnamed rows copy as an empty line, so the line count still matches the selection
            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy_Name")}##projectileCopyName"))
                CopyText(selectedRows, r => r.Name ?? "");
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Copy_Name_TT"));

            if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID_And_Name")}##projectileCopyIdAndName"))
                CopyText(selectedRows, r => $"{r.ID};{r.Name ?? ""}");
            GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Copy_ID_And_Name_TT"));

            ImGui.EndMenu();
        }

        ImGui.Separator();

        if (ImGui.Selectable($"{LOC.Get("PROJECTILE_Open_In_Param_Editor")}##openInParamEditor"))
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
        if (ParamEditor is { } editor)
            ParamRowDuplicate.DuplicateRows(editor, param, GetSelectedRows(param));
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

        var active = View.Simulator.ParamName == ParamName ? View.Simulator.Row : null;

        if (active == null || !Rows.Contains(active))
            View.Select(ParamName!, Rows[0]);

        Selected.UnionWith(Rows);
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

        var param = ProjectileParams.GetParam(View.Project, ParamName!);
        var selectedRows = param != null ? GetSelectedRows(param) : new List<Param.Row>();

        // Only another bullet or spell param can be gone to here; rows of any other param are pasted in the Param Editor
        var clipboardParam = View.Project.Handler.ParamData.PrimaryBank.ClipboardParam;
        Action<string>? goToParam = clipboardParam != null && ProjectileParams.IsProjectileParam(View.Project, clipboardParam)
            ? ShowParam
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
        if (PasteMenuOpen || ParamName == null)
            return;

        var param = ProjectileParams.GetParam(View.Project, ParamName);

        if (param == null)
            return;

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
