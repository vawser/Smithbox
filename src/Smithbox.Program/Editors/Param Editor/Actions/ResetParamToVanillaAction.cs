using Andre.Formats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Makes a param the same as its vanilla version, as one undoable step:
/// <list type="bullet">
/// <item>modified rows get their vanilla values (they keep their current name)</item>
/// <item>rows the mod added are removed</item>
/// <item>vanilla rows the mod deleted come back (with their vanilla name)</item>
/// <item>the rows are put back in vanilla order</item>
/// </list>
/// Rows are matched to vanilla by ID, by occurrence for repeated IDs (the second row with ID 100 matches the second
/// vanilla row with ID 100). Unmodified rows keep their row object, so selections and earlier undo steps that refer
/// to them stay valid; a reset row is a new object, and every view's selection follows it (and back on undo).
/// </summary>
public class ResetParamToVanillaAction : EditorAction
{
    private readonly ParamEditorScreen Editor;
    private readonly Param Param;

    private readonly List<Param.Row> NewOrder;
    private readonly List<(Param.Row Old, Param.Row New)> Replaced;
    private readonly List<Param.Row> Removed;
    private List<Param.Row> PreviousOrder = new();

    public int ResetRowCount => Replaced.Count;
    public int RemovedRowCount => Removed.Count;
    public int RestoredRowCount { get; }

    /// <summary>
    /// False when the param already matches vanilla (values, rows and order), so there is nothing to do
    /// </summary>
    public bool HasChanges { get; }

    private ResetParamToVanillaAction(ParamEditorScreen editor, Param param, List<Param.Row> newOrder,
        List<(Param.Row, Param.Row)> replaced, List<Param.Row> removed, int restored)
    {
        Editor = editor;
        Param = param;
        NewOrder = newOrder;
        Replaced = replaced;
        Removed = removed;
        RestoredRowCount = restored;
        HasChanges = replaced.Count > 0 || removed.Count > 0 || restored > 0 || !newOrder.SequenceEqual(param.Rows);
    }

    /// <summary>
    /// Whether the two params can be reset one from the other: same param type and row layout. A param from a
    /// different regulation version than vanilla can't be, as its rows don't line up with vanilla's.
    /// </summary>
    public static bool CanReset(Param param, Param vanillaParam)
    {
        return param != null && vanillaParam != null
            && param.AppliedParamdef != null && vanillaParam.AppliedParamdef != null
            && param.ParamType == vanillaParam.ParamType
            && param.RowSize == vanillaParam.RowSize;
    }

    /// <summary>
    /// Works out what the reset will change. Nothing is changed until the action is executed.
    /// </summary>
    public static ResetParamToVanillaAction Create(ParamEditorScreen editor, Param param, Param vanillaParam)
    {
        // The param's rows by ID, in order
        var rowsById = new Dictionary<int, List<Param.Row>>();

        foreach (var row in param.Rows)
        {
            if (!rowsById.TryGetValue(row.ID, out var list))
            {
                list = new List<Param.Row>();
                rowsById[row.ID] = list;
            }

            list.Add(row);
        }

        var occurrences = new Dictionary<int, int>();
        var matched = new HashSet<Param.Row>();
        var newOrder = new List<Param.Row>(vanillaParam.Rows.Count);
        var replaced = new List<(Param.Row, Param.Row)>();
        var restored = 0;

        foreach (var vanillaRow in vanillaParam.Rows)
        {
            var occurrence = occurrences.GetValueOrDefault(vanillaRow.ID);
            occurrences[vanillaRow.ID] = occurrence + 1;

            Param.Row match = rowsById.TryGetValue(vanillaRow.ID, out var idRows) && occurrence < idRows.Count
                ? idRows[occurrence]
                : null;

            if (match == null)
            {
                // Deleted by the mod: bring the vanilla row back
                newOrder.Add(new Param.Row(vanillaRow, param));
                restored++;
                continue;
            }

            matched.Add(match);

            if (match.RowMatches(vanillaRow))
            {
                // Unmodified: keep the row as it is
                newOrder.Add(match);
                continue;
            }

            var reset = new Param.Row(vanillaRow, param);
            reset.Name = match.Name;

            newOrder.Add(reset);
            replaced.Add((match, reset));
        }

        // Rows with no vanilla counterpart were added by the mod
        var removed = param.Rows.Where(r => !matched.Contains(r)).ToList();

        return new ResetParamToVanillaAction(editor, param, newOrder, replaced, removed, restored);
    }

    public override ActionEvent Execute()
    {
        PreviousOrder = Param.Rows.ToList();

        SetRows(NewOrder);

        foreach (var view in Editor.ViewHandler.ParamEditorViews)
        {
            if (view == null)
                continue;

            foreach (var (oldRow, newRow) in Replaced)
            {
                view.Selection.ReplaceRowInAllSelections(oldRow, newRow);
            }

            foreach (var row in Removed)
            {
                view.Selection.RemoveRowFromAllSelections(row);
            }
        }

        ParamDirtyTracker.MarkDirty(Param);
        Editor.ViewHandler.RefreshTableGroupsFor(Param, refreshGroupRows: true);

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        SetRows(PreviousOrder);

        foreach (var view in Editor.ViewHandler.ParamEditorViews)
        {
            if (view == null)
                continue;

            foreach (var (oldRow, newRow) in Replaced)
            {
                view.Selection.ReplaceRowInAllSelections(newRow, oldRow);
            }
        }

        ParamDirtyTracker.MarkDirty(Param);
        Editor.ViewHandler.RefreshTableGroupsFor(Param, refreshGroupRows: true);

        return ActionEvent.ObjectAddedRemoved;
    }

    private void SetRows(List<Param.Row> rows)
    {
        Param.ClearRows();

        foreach (var row in rows)
        {
            Param.AddRow(row);
        }
    }
}
