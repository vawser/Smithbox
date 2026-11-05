using Andre.Formats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;


public class AddParamsAction : EditorAction
{
    private ParamEditorScreen Editor;
    private readonly bool appOnly;
    private readonly List<Param.Row> Clonables = new();
    private readonly List<Param.Row> Clones = new();
    private readonly int InsertIndex;
    private readonly Param Param;
    private readonly bool replParams;
    private string ParamString;
    private int IdOffset;
    private bool IsDuplicate;
    private Action<bool> PostExecutionAction;

    /// <summary>
    /// What the first Execute did for each source row, in order. Redo replays these steps with the same row
    /// instances, so later actions that hold a reference to a new row (e.g. a field edit on a pasted row) still
    /// point at the row that's in the param.
    /// </summary>
    private readonly List<RowInsertStep> Steps = new();
    private bool HasExecuted = false;

    private class RowInsertStep
    {
        public Param.Row Clone;

        // Index the clone was inserted at, or -1 if it wasn't inserted
        public int InsertIndex = -1;

        // Existing row removed to make way for the clone (replace mode), and the index it was removed from
        public Param.Row Replaced;
        public int ReplacedIndex = -1;
    }

    public AddParamsAction(ParamEditorScreen editor, Param param, string pstring, List<Param.Row> rows, bool appendOnly, bool replaceParams, int index = -1, int idOffset = 1, bool isDuplicate = false)
    {
        Editor = editor;
        Param = param;
        Clonables.AddRange(rows);
        ParamString = pstring;
        appOnly = appendOnly;
        replParams = replaceParams;
        InsertIndex = index;
        IdOffset = idOffset;
        IsDuplicate = isDuplicate;
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
            Replay();
        }

        ParamDirtyTracker.MarkDirty(Param);

        Editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask();

        var activeParam = Editor.ViewHandler.ActiveView.Selection.GetActiveParam();
        Editor.ViewHandler.ActiveView.ParamTableWindow.UpdateTableSelection(activeParam);

        return ActionEvent.NoEvent;
    }

    /// <summary>
    /// Creates the clones and inserts them, recording each step so it can be undone and replayed exactly
    /// </summary>
    private void FirstExecute()
    {
        foreach (Param.Row row in Clonables)
        {
            var newrow = new Param.Row(row);
            var step = new RowInsertStep { Clone = newrow };

            var paramMeta = Editor.Project.Handler.ParamData.GetParamMeta(row.Def);

            // Only apply for Duplicate action
            if (IsDuplicate)
            {
                ApplyDeepCopyOffset(newrow, paramMeta);
            }

            if (InsertIndex > -1)
            {
                newrow.Name = row.Name != null ? row.Name + "_1" : "";
                Param.InsertRow(InsertIndex, newrow);
            }
            else
            {
                if (Param[row.ID] != null)
                {
                    if (replParams)
                    {
                        Param.Row existing = Param[row.ID];
                        step.Replaced = existing;
                        step.ReplacedIndex = Param.IndexOfRow(existing);
                        Param.RemoveRow(existing);
                    }
                    else
                    {
                        var rowIdOffset = IdOffset;
                        if (rowIdOffset <= 0)
                            rowIdOffset = 1;

                        newrow.Name = row.Name != null ? row.Name + "_1" : "";
                        var newID = row.ID + rowIdOffset;
                        while (Param[newID] != null)
                        {
                            newID += rowIdOffset;
                        }

                        newrow.ID = newID;
                        Param.InsertRow(Param.IndexOfRow(Param[newID - rowIdOffset]) + 1, newrow);
                    }
                }

                if (Param[row.ID] == null)
                {
                    newrow.Name = row.Name != null ? row.Name : "";
                    if (appOnly)
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

            // Where the clone ended up (-1 if no branch inserted it, e.g. replace mode with a second row
            // still holding the ID)
            step.InsertIndex = Param.IndexOfRow(newrow);

            Steps.Add(step);
            Clones.Add(newrow);
        }
    }

    /// <summary>
    /// For a duplicate, offset the reference fields the user opted into by the ID offset, so the copy points at
    /// the matching copies of its attacks, bullets etc. Each field's meta <c>DeepCopyTarget</c> says which
    /// kind of reference it is (Attack, Bullet, Behavior, SpEffect or Source), and only the kinds whose
    /// option is ticked are changed.
    /// </summary>
    private void ApplyDeepCopyOffset(Param.Row newrow, ParamMeta paramMeta)
    {
        // Params without meta have no reference information, so there's nothing to offset
        if (paramMeta == null)
            return;

        // Same list the Duplicate menu shows its options and preview from
        var enabledTargets = ParamRowDuplicate.GetEnabledDeepCopyTargets();

        if (enabledTargets.Count == 0 || IdOffset == 0)
            return;

        foreach (var cell in newrow.Cells)
        {
            var meta = Editor.Project.Handler.ParamData.GetParamFieldMeta(paramMeta, cell.Def);

            if (meta?.DeepCopyTargetType == null)
                continue;

            if (!meta.DeepCopyTargetType.Any(t => enabledTargets.Contains(t.Trim())))
                continue;

            var value = cell.Value;

            if (value == null)
                continue;

            long id;

            try
            {
                id = Convert.ToInt64(value);
            }
            catch (Exception e) when (e is InvalidCastException or FormatException or OverflowException)
            {
                // Not a numeric field, so it can't hold a reference ID
                continue;
            }

            // -1 means "no reference"
            if (id == -1)
                continue;

            try
            {
                // Write back in the field's own type. Skip the field if the offset ID doesn't fit.
                cell.Value = Convert.ChangeType(id + IdOffset, value.GetType());
            }
            catch (OverflowException)
            {
            }
        }
    }

    /// <summary>
    /// Redo: Undo has restored the param to the state the first Execute saw, so the same steps at the same
    /// indices reproduce the same result with the same row instances
    /// </summary>
    private void Replay()
    {
        foreach (var step in Steps)
        {
            if (step.Replaced != null)
            {
                Param.RemoveRow(step.Replaced);
            }

            if (step.InsertIndex > -1)
            {
                Param.InsertRow(step.InsertIndex, step.Clone);
            }
        }
    }

    public List<Param.Row> GetResultantRows()
    {
        return Clones;
    }

    public override ActionEvent Undo()
    {
        // Reverse the steps exactly, last first, so each recorded index is valid when it's used
        for (var i = Steps.Count - 1; i >= 0; i--)
        {
            var step = Steps[i];

            if (step.InsertIndex > -1)
            {
                Param.RemoveRow(step.Clone);
            }

            if (step.Replaced != null)
            {
                Param.InsertRow(step.ReplacedIndex, step.Replaced);
            }
        }

        ParamDirtyTracker.MarkDirty(Param);

        // Same as Execute, so the modified-row colours update when undoing from anywhere (e.g. a mass edit rollback)
        Editor.Project.Handler.ParamData.RefreshParamDifferenceCacheTask();

        var activeParam = Editor.ViewHandler.ActiveView.Selection.GetActiveParam();
        Editor.ViewHandler.ActiveView.ParamTableWindow.UpdateTableSelection(activeParam);

        return ActionEvent.NoEvent;
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }
}
