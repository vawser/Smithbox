using Andre.Formats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Replaces whole row table groups (every row with one ID) with new rows, as one undoable step.
/// An empty row list deletes the group. Used where groups are merged as a unit, since replacing rows one at a
/// time can't tell apart rows that share an ID.
/// </summary>
public class ReplaceRowGroupsAction : EditorAction
{
    private readonly ParamEditorScreen Editor;
    private readonly Param Param;
    private readonly List<(int ID, List<Param.Row> NewRows)> Groups;

    private class Step
    {
        public int ID;
        public List<Param.Row> NewRows;
        public List<(int Index, Param.Row Row)> OldRows = new();
    }

    private readonly List<Step> Steps = new();

    /// <param name="groups">The new rows of each ID, already created with <paramref name="param"/> as parent</param>
    public ReplaceRowGroupsAction(ParamEditorScreen editor, Param param, List<(int ID, List<Param.Row> NewRows)> groups)
    {
        Editor = editor;
        Param = param;
        Groups = groups;
    }

    public override ActionEvent Execute()
    {
        // Redo starts from the state the first Execute saw (Undo restores it), so recording again is the same
        Steps.Clear();

        foreach (var (id, newRows) in Groups)
        {
            var step = new Step { ID = id, NewRows = newRows };

            for (var i = 0; i < Param.Rows.Count; i++)
            {
                if (Param.Rows[i].ID == id)
                    step.OldRows.Add((i, Param.Rows[i]));
            }

            ParamRowGroups.ReplaceGroup(Param, id, newRows);
            Steps.Add(step);
        }

        // Replaced rows are out of the param, so they can't stay selected
        foreach (var view in Editor.ViewHandler.ParamEditorViews)
        {
            if (view == null)
                continue;

            foreach (var step in Steps)
            {
                foreach (var (_, row) in step.OldRows)
                {
                    view.Selection.RemoveRowFromAllSelections(row);
                }
            }
        }

        Refresh();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        // Last step first, so each recorded index matches the param as that step found it
        for (var s = Steps.Count - 1; s >= 0; s--)
        {
            var step = Steps[s];

            foreach (var row in step.NewRows)
            {
                Param.RemoveRow(row);
            }

            foreach (var (index, row) in step.OldRows.OrderBy(e => e.Index))
            {
                Param.InsertRow(index, row);
            }
        }

        foreach (var view in Editor.ViewHandler.ParamEditorViews)
        {
            if (view == null)
                continue;

            foreach (var step in Steps)
            {
                foreach (var row in step.NewRows)
                {
                    view.Selection.RemoveRowFromAllSelections(row);
                }
            }
        }

        Refresh();

        return ActionEvent.NoEvent;
    }

    private void Refresh()
    {
        ParamDirtyTracker.MarkDirty(Param);

        Editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask();

        Editor.ViewHandler.RefreshTableGroupsFor(Param, refreshGroupRows: true);
    }
}
