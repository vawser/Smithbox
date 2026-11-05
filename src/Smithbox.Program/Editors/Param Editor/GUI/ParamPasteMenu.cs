using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class ParamPasteMenu
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public bool DisplayPasteMenu = false;
    public ParamPasteMenu(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Open()
    {
        ImGui.OpenPopup("ctrlVPopup");
    }

    public void Display()
    {
        var activeView = Editor.ViewHandler.ActiveView;
        var clipboard = Editor.Clipboard;

        if (ImGui.BeginPopup("ctrlVPopup"))
        {
            DisplayPasteMenu = true;

            try
            {
                if (activeView != null)
                {
                    DisplayContents(activeView, clipboard);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, e.Message, e);
                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
        else
        {
            DisplayPasteMenu = false;
        }
    }

    private void DisplayContents(ParamEditorView activeView, ParamClipboard clipboard)
    {
        var bank = activeView.GetPrimaryBank();
        var clipboardParamKey = bank.ClipboardParam;
        var activeParamKey = activeView.Selection.GetActiveParam();

        // Rows are pasted into the param they were copied from. Their row data belongs to that param, so
        // pasting them anywhere else isn't supported here. If the user is looking at a different param, say so
        // instead of silently writing into the other one.
        if (clipboardParamKey == null || bank.ClipboardRows.Count == 0
            || !bank.Params.TryGetValue(clipboardParamKey, out var targetParam))
        {
            ImGui.TextUnformatted(LOC.Get("PARAM_PasteMenu_Clipboard_Empty"));
            return;
        }

        ImGui.TextUnformatted(LOC.Get("PARAM_PasteMenu_Target", bank.ClipboardRows.Count, clipboardParamKey));
        ImGui.Separator();

        if (activeParamKey != clipboardParamKey)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color,
                LOC.Get("PARAM_PasteMenu_Different_Param", clipboardParamKey, activeParamKey ?? "-"));

            if (ImGui.Button($"{LOC.Get("PARAM_PasteMenu_Action_Go_To_Param", clipboardParamKey)}##goToClipboardParam"))
            {
                EditorCommandQueue.AddCommand($@"param/view/{activeView.ViewIndex}/{clipboardParamKey}");
            }

            return;
        }

        ImGui.Checkbox(
            $"{LOC.Get("PARAM_PasteMenu_Checkbox_Select_New_Rows")}##selectNewRowsAfterPasteToggle",
            ref CFG.Current.Param_PasteThenSelect);

        ImGui.Checkbox($"{LOC.Get("PARAM_PasteMenu_Checkbox_Paste_After_Selection")}##pasteAfterSelectionToggle",
            ref CFG.Current.Param_PasteAfterSelection);

        var pasteThenSelect = CFG.Current.Param_PasteThenSelect;
        var pasteAfterSelection = CFG.Current.Param_PasteAfterSelection;

        long offset = 0;
        string inputError = null;

        if (!pasteAfterSelection)
        {
            ImGui.InputText($"{LOC.Get("PARAM_PasteMenu_Row_Title")}##rowTitle",
                ref clipboard._currentCtrlVValue, 20);

            if (ImGui.IsItemEdited() && long.TryParse(clipboard._currentCtrlVValue, out var newBase))
            {
                clipboard._currentCtrlVOffset = (newBase - clipboard._clipboardBaseRow).ToString();
            }

            ImGui.InputText($"{LOC.Get("PARAM_PasteMenu_Offset_Title")}##rowOffset",
                ref clipboard._currentCtrlVOffset, 20);

            if (ImGui.IsItemEdited() && long.TryParse(clipboard._currentCtrlVOffset, out var newOffset))
            {
                clipboard._currentCtrlVValue = (clipboard._clipboardBaseRow + newOffset).ToString();
            }

            // Validate here rather than letting a half-typed value throw, so the user sees why Submit is missing
            if (!long.TryParse(clipboard._currentCtrlVValue, out _)
                || !long.TryParse(clipboard._currentCtrlVOffset, out offset))
            {
                inputError = LOC.Get("PARAM_PasteMenu_Invalid_Number");
            }
            else
            {
                var minId = bank.ClipboardRows.Min(r => (long)r.ID) + offset;
                var maxId = bank.ClipboardRows.Max(r => (long)r.ID) + offset;

                if (minId < int.MinValue || maxId > int.MaxValue)
                {
                    inputError = LOC.Get("PARAM_PasteMenu_ID_Out_Of_Range");
                }
                else
                {
                    ImGui.TextUnformatted(minId == maxId
                        ? LOC.Get("PARAM_PasteMenu_Resulting_ID", minId)
                        : LOC.Get("PARAM_PasteMenu_Resulting_IDs", minId, maxId));
                }
            }
        }

        // The bottom-most selected row: paste-after-selection inserts below it. The selection list is in click
        // order, so it can't just take the last entry.
        Param.Row anchorRow = null;

        if (pasteAfterSelection)
        {
            anchorRow = activeView.Selection.GetSelectedRows()?
                .Where(r => r != null && r.GetParentParam() == targetParam)
                .OrderBy(r => targetParam.IndexOfRow(r))
                .LastOrDefault();

            if (anchorRow == null)
            {
                inputError = LOC.Get("PARAM_PasteMenu_No_Selection_Exists");
            }
        }

        if (inputError != null)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, inputError);
            return;
        }

        if (!ImGui.Button($"{LOC.Get("PARAM_PasteMenu_Action_Submit")}##submitAction"))
        {
            return;
        }

        List<Param.Row> rowsToInsert = new();
        var insertIndex = -1;

        if (!pasteAfterSelection)
        {
            foreach (Param.Row r in bank.ClipboardRows)
            {
                Param.Row newrow = new(r); // more cloning
                newrow.ID = (int)(r.ID + offset);
                rowsToInsert.Add(newrow);
            }
        }
        else
        {
            insertIndex = targetParam.IndexOfRow(anchorRow) + 1;

            // IDs already taken, so each new row gets the next free ID after the anchor row
            var usedIds = new HashSet<int>(targetParam.Rows.Select(r => r.ID));
            var nextId = anchorRow.ID;

            foreach (Param.Row r in bank.ClipboardRows)
            {
                Param.Row newrow = new(r);

                do
                {
                    nextId++;
                }
                while (usedIds.Contains(nextId));

                newrow.ID = nextId;
                usedIds.Add(nextId);

                rowsToInsert.Add(newrow);
            }

            // Reverse so that inserting each row at the same fixed index leaves them in ascending ID order
            rowsToInsert.Reverse();
        }

        var paramAction = new AddParamsAction(Editor, targetParam, "legacystring", rowsToInsert, false,
            false, insertIndex);

        // Refused while a save is running
        if (!Editor.ActionManager.ExecuteAction(paramAction))
        {
            return;
        }

        // Selection management
        if (pasteThenSelect)
        {
            var res = paramAction.GetResultantRows();
            if (res.Count > 0)
            {
                activeView.Selection.SetActiveRow(res[0], true);

                foreach (Param.Row r in res)
                {
                    activeView.Selection.AddRowToSelection(r);
                }

                EditorCommandQueue.AddCommand($@"param/select/-1/{activeView.Selection.GetActiveParam()}/{res[0].ID}/addOnly");
            }
        }

        ImGui.CloseCurrentPopup();
    }
}
