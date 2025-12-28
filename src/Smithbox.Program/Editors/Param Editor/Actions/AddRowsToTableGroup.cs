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
    private readonly List<Param.Row> Removed = new();
    private readonly List<int> RemovedIndex = new();
    private bool AppendOnly = false;
    private Action<bool> PostExecutionAction;
    private bool IgnoreNameAppend = false;

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
        foreach (Param.Row row in Clonables)
        {
            var newrow = new Param.Row(row);

            var paramMeta = Editor.Project.ParamData.GetParamMeta(row.Def);

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

            Clones.Add(newrow);
        }

        Editor.Project.ParamData.RefreshParamDifferenceCacheTask();

        var activeParam = Editor._activeView.Selection.GetActiveParam();
        Editor._activeView.TableGroupView.UpdateTableSelection(activeParam);

        return ActionEvent.NoEvent;
    }

    public List<Param.Row> GetResultantRows()
    {
        return Clones;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Clones.Count(); i++)
        {
            Param.RemoveRow(Clones[i]);
        }

        for (var i = Removed.Count() - 1; i >= 0; i--)
        {
            Param.InsertRow(RemovedIndex[i], Removed[i]);
        }

        Clones.Clear();
        RemovedIndex.Clear();
        Removed.Clear();

        var activeParam = Editor._activeView.Selection.GetActiveParam();
        Editor._activeView.TableGroupView.UpdateTableSelection(activeParam);

        return ActionEvent.NoEvent;
    }
    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }
}