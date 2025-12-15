using Andre.Formats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;


public class DeleteParamsAction : EditorAction
{
    private ParamEditorScreen Editor;
    private readonly List<Param.Row> Deletables = new();
    private readonly Param Param;
    private readonly List<int> RemoveIndices = new();
    private readonly bool SetSelection = false;

    public DeleteParamsAction(ParamEditorScreen editor, Param param, List<Param.Row> rows)
    {
        Editor = editor;
        Param = param;
        Deletables.AddRange(rows);
    }

    public override ActionEvent Execute()
    {
        foreach (Param.Row row in Deletables)
        {
            RemoveIndices.Add(Param.IndexOfRow(row));
            Param.RemoveRowAt(RemoveIndices.Last());
        }

        if (SetSelection)
        {
        }

        var curParam = Editor._activeView.Selection.GetActiveParam();

        if (Editor._activeView.TableGroupView.IsInTableGroupMode(curParam))
        {
            var curGroup = Editor._activeView.TableGroupView.CurrentTableGroup;
            Editor._activeView.TableGroupView.UpdateTableGroupSelection(curGroup);

            Editor._activeView.TableGroupView.UpdateTableSelection();
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (var i = Deletables.Count() - 1; i >= 0; i--)
        {
            Param.InsertRow(RemoveIndices[i], Deletables[i]);
        }

        if (SetSelection)
        {
        }

        Editor.Project.ParamData.RefreshParamDifferenceCacheTask();

        var curParam = Editor._activeView.Selection.GetActiveParam();

        if (Editor._activeView.TableGroupView.IsInTableGroupMode(curParam))
        {
            var curGroup = Editor._activeView.TableGroupView.CurrentTableGroup;
            Editor._activeView.TableGroupView.UpdateTableGroupSelection(curGroup);

            Editor._activeView.TableGroupView.UpdateTableSelection();
        }
        return ActionEvent.NoEvent;
    }
}
