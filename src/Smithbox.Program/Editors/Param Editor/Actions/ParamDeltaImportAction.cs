using Andre.Formats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Imports a delta patch into the primary bank as one undoable step (used for merged changes).
/// <para>
/// The importer edits rows in place, adds and removes rows. The first Execute records each param's row order and
/// the data (raw field bytes, name, ID) of every row, runs the import, then keeps only what changed: the new row
/// order and the before / after data of rows whose data changed. Undo puts back the old order and data, Redo the
/// new. Row objects are kept, so selections and earlier undo steps that refer to them stay valid.
/// </para>
/// </summary>
public class ParamDeltaImportAction : EditorAction
{
    private readonly ParamEditorScreen Editor;
    private readonly ParamDeltaImporter Importer;
    private readonly ParamDeltaPatch Patch;
    private readonly ParamDeltaImportOptions Options;

    private readonly record struct RowState(byte[] Data, string Name, int ID);

    private sealed class ParamState
    {
        public Param Param;
        public List<Param.Row> Before;
        public List<Param.Row> After;
        public Dictionary<Param.Row, RowState> BeforeData;
        public Dictionary<Param.Row, RowState> AfterData = new();
    }

    private readonly List<ParamState> States = new();
    private bool HasExecuted = false;

    /// <summary>
    /// False once executed if the import changed nothing
    /// </summary>
    public bool HasChanges => States.Count > 0;

    public int ChangedParamCount => States.Count;

    public ParamDeltaImportAction(ParamEditorScreen editor, ParamDeltaImporter importer, ParamDeltaPatch patch,
        ParamDeltaImportOptions options)
    {
        Editor = editor;
        Importer = importer;
        Patch = patch;
        Options = options;
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
            foreach (var state in States)
            {
                Restore(state, state.After, state.AfterData, state.Before);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        foreach (var state in States)
        {
            Restore(state, state.Before, state.BeforeData, state.After);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    private void FirstExecute()
    {
        var primaryBank = Editor.Project.Handler.ParamData.PrimaryBank;

        foreach (var paramDelta in Patch.Params)
        {
            if (!primaryBank.Params.TryGetValue(paramDelta.Name, out var param))
                continue;

            if (States.Any(s => s.Param == param))
                continue;

            States.Add(new ParamState
            {
                Param = param,
                Before = param.Rows.ToList(),
                BeforeData = param.Rows.ToDictionary(r => r, Capture)
            });
        }

        Importer.HandleParamImport(Patch, Options);

        foreach (var state in States.ToList())
        {
            state.After = state.Param.Rows.ToList();
            var afterSet = state.After.ToHashSet();

            // Keep only rows whose data changed: rows that are new or removed need just the order
            var changed = new Dictionary<Param.Row, RowState>();

            foreach (var (row, before) in state.BeforeData)
            {
                if (!afterSet.Contains(row))
                    continue;

                var now = Capture(row);
                if (now.ID != before.ID || now.Name != before.Name || !now.Data.AsSpan().SequenceEqual(before.Data))
                {
                    changed[row] = before;
                    state.AfterData[row] = now;
                }
            }

            state.BeforeData = changed;

            if (changed.Count == 0 && state.Before.SequenceEqual(state.After))
            {
                States.Remove(state);
                continue;
            }

            Refresh(state.Param);
        }
    }

    private void Restore(ParamState state, List<Param.Row> order, Dictionary<Param.Row, RowState> data,
        List<Param.Row> otherOrder)
    {
        var param = state.Param;

        param.ClearRows();
        foreach (var row in order)
            param.AddRow(row);

        foreach (var (row, rowState) in data)
        {
            row.SetRawData(rowState.Data);
            row.Name = rowState.Name;
            row.ID = rowState.ID;
        }

        // Rows that are out of the param now shouldn't stay selected
        var present = order.ToHashSet();
        var gone = otherOrder.Where(r => !present.Contains(r)).ToList();

        if (gone.Count > 0)
        {
            foreach (var view in Editor.ViewHandler.ParamEditorViews)
            {
                if (view == null)
                    continue;

                foreach (var row in gone)
                    view.Selection.RemoveRowFromAllSelections(row);
            }
        }

        Refresh(param);
    }

    private void Refresh(Param param)
    {
        ParamDirtyTracker.MarkDirty(param);
        Editor.ViewHandler.RefreshTableGroupsFor(param, refreshGroupRows: true);
    }

    private static RowState Capture(Param.Row row)
    {
        return new RowState(row.GetRawData(), row.Name, row.ID);
    }
}
