using Andre.Formats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;


public class AddRowsToTableGroup : EditorAction
{
    private ParamEditorScreen Editor;
    private readonly List<Param.Row> Clonables = new();
    private readonly List<Param.Row> Clones = new();
    private readonly int InsertIndex;
    private readonly Param Param;
    private bool AppendOnly = false;
    private Action<bool> PostExecutionAction;
    private bool IgnoreNameAppend = false;

    /// <summary>
    /// Index each clone was inserted at by the first Execute (-1 if it wasn't inserted), parallel to Clones.
    /// Redo re-inserts the same row instances at these indices, so later actions that reference a new row still
    /// point at the row that's in the param.
    /// </summary>
    private readonly List<int> InsertedIndices = new();
    private bool HasExecuted = false;

    public AddRowsToTableGroup(ParamEditorScreen editor, Param param, List<Param.Row> rows, int insertIndex, bool appendOnly = false, bool ignoreNameAppend = false)
    {
        Editor = editor;
        Param = param;
        Clonables.AddRange(rows);
        AppendOnly = appendOnly;
        InsertIndex = insertIndex;
        IgnoreNameAppend = ignoreNameAppend;
    }

    public override ActionEvent Execute()
    {
        if (!HasExecuted)
        {
            FirstExecute();
            HasExecuted = true;
        }
        else
        {
            // Undo restored the state the first Execute saw, so the same indices give the same result
            for (var i = 0; i < Clones.Count; i++)
            {
                if (InsertedIndices[i] > -1)
                {
                    Param.InsertRow(InsertedIndices[i], Clones[i]);
                }
            }
        }

        ParamDirtyTracker.MarkDirty(Param);

        Editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask();

        var activeParam = Editor.ViewHandler.ActiveView.Selection.GetActiveParam();
        Editor.ViewHandler.ActiveView.ParamTableWindow.UpdateTableSelection(activeParam);

        return ActionEvent.NoEvent;
    }

    private void FirstExecute()
    {
        foreach (Param.Row row in Clonables)
        {
            var newrow = new Param.Row(row);

            if (InsertIndex > -1)
            {
                if (!IgnoreNameAppend)
                    newrow.Name = row.Name != null ? row.Name + "_1" : "";

                Param.InsertRow(InsertIndex, newrow);
            }
            else
            {
                if (Param[row.ID] != null)
                {
                    if (!IgnoreNameAppend)
                        newrow.Name = row.Name != null ? row.Name + "_1" : "";

                    newrow.ID = row.ID;

                    Param.InsertRow(Param.IndexOfRow(Param[row.ID]) + 1, newrow);
                }

                if (Param[row.ID] == null)
                {
                    newrow.Name = row.Name != null ? row.Name : "";
                    if (AppendOnly)
                    {
                        Param.AddRow(newrow);
                    }
                    else
                    {
                        var index = 0;
                        foreach (Param.Row r in Param.Rows)
                        {
                            if (r.ID > newrow.ID)
                            {
                                break;
                            }

                            index++;
                        }

                        Param.InsertRow(index, newrow);
                    }
                }
            }

            InsertedIndices.Add(Param.IndexOfRow(newrow));
            Clones.Add(newrow);
        }
    }

    public List<Param.Row> GetResultantRows()
    {
        return Clones;
    }

    public override ActionEvent Undo()
    {
        // Last inserted first, so each recorded index is valid when it's used on redo
        for (var i = Clones.Count - 1; i >= 0; i--)
        {
            if (InsertedIndices[i] > -1)
            {
                Param.RemoveRow(Clones[i]);
            }
        }

        ParamDirtyTracker.MarkDirty(Param);

        var activeParam = Editor.ViewHandler.ActiveView.Selection.GetActiveParam();
        Editor.ViewHandler.ActiveView.ParamTableWindow.UpdateTableSelection(activeParam);

        return ActionEvent.NoEvent;
    }
    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }
}
