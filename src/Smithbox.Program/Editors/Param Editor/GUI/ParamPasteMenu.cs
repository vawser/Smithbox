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

        var pasteThenSelect = CFG.Current.Param_PasteThenSelect;
        var pasteAfterSelection = CFG.Current.Param_PasteAfterSelection;

        if (ImGui.BeginPopup("ctrlVPopup"))
        {
            DisplayPasteMenu = true;

            try
            {
                long offset = 0;

                ImGui.Checkbox("Select new rows after paste", ref CFG.Current.Param_PasteThenSelect);

                ImGui.Checkbox("Paste after selection", ref CFG.Current.Param_PasteAfterSelection);

                var insertIndex = -1;

                if (pasteAfterSelection)
                {
                    //ImGui.Text("Note: Allows out-of-order rows, which may confuse later ID-based row additions.");
                }
                else
                {
                    ImGui.InputText("Row", ref clipboard._currentCtrlVValue, 20);
                    if (ImGui.IsItemEdited())
                    {
                        offset = long.Parse(clipboard._currentCtrlVValue) - clipboard._clipboardBaseRow;

                        clipboard._currentCtrlVOffset = offset.ToString();
                    }

                    ImGui.InputText("Offset", ref clipboard._currentCtrlVOffset, 20);
                    if (ImGui.IsItemEdited())
                    {
                        offset = long.Parse(clipboard._currentCtrlVOffset);

                        clipboard._currentCtrlVValue = (clipboard._clipboardBaseRow + offset).ToString();
                    }

                    // Recheck that this is valid
                    offset = long.Parse(clipboard._currentCtrlVValue);
                    offset = long.Parse(clipboard._currentCtrlVOffset);
                }

                var disableSubmit = pasteAfterSelection &&
                                    !activeView.Selection.RowSelectionExists();
                if (disableSubmit)
                {
                    ImGui.TextUnformatted("No selection exists");
                }
                else if (ImGui.Button("Submit"))
                {
                    List<Param.Row> rowsToInsert = new();

                    if (!pasteAfterSelection)
                    {
                        foreach (Param.Row r in activeView.GetPrimaryBank().ClipboardRows)
                        {
                            Param.Row newrow = new(r); // more cloning
                            newrow.ID = (int)(r.ID + offset);
                            rowsToInsert.Add(newrow);
                        }
                    }
                    else
                    {
                        List<Param.Row> rows = activeView.Selection.GetSelectedRows();

                        Param param = activeView.GetPrimaryBank().Params[activeView.Selection.GetActiveParam()];

                        insertIndex = param.IndexOfRow(rows.Last()) + 1;

                        foreach (Param.Row r in activeView.GetPrimaryBank().ClipboardRows)
                        {
                            // Determine new ID based on paste target. Increment ID until a free ID is found.
                            Param.Row newrow = new(r);
                            newrow.ID = activeView.Selection.GetSelectedRows().Last().ID;
                            do
                            {
                                newrow.ID++;
                            }
                            while (activeView.GetPrimaryBank().Params[activeView.Selection.GetActiveParam()][newrow.ID] != null || rowsToInsert.Exists(e => e.ID == newrow.ID));

                            rowsToInsert.Add(newrow);
                        }

                        // Do a clever thing by reversing order, making ID order incremental and resulting in row insertion being in the correct order because of the static index.
                        rowsToInsert.Reverse();
                    }

                    var paramAction = new AddParamsAction(Editor,
                        activeView.GetPrimaryBank().Params[activeView.GetPrimaryBank().ClipboardParam], "legacystring", rowsToInsert, false,
                        false, insertIndex);

                    Editor.ActionManager.ExecuteAction(paramAction);

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
            catch
            {
                ImGui.EndPopup();
                return;
            }

            ImGui.EndPopup();
        }
        else
        {
            DisplayPasteMenu = false;
        }
    }
}
