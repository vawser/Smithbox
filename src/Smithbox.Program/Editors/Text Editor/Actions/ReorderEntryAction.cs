using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;
public class ReorderEntryAction : EditorAction
{
    private readonly FMG TargetFmg;
    private readonly List<FMG.Entry> EntriesToMove;

    private readonly int DropTargetIndex;

    private List<int> OriginalIndices = new();
    private int ResolvedInsertionIndex = -1;

    // Full Replace
    private readonly bool IsFullReplace;
    private readonly List<FMG.Entry> DesiredOrder;
    private List<FMG.Entry> PreviousOrder = new();

    // Drag to Order
    public ReorderEntryAction(FMG targetFmg, List<FMG.Entry> targetEntries, int dropIndex)
    {
        TargetFmg = targetFmg;
        EntriesToMove = targetEntries;
        DropTargetIndex = dropIndex;
    }

    // Full Replace
    public ReorderEntryAction(FMG targetFmg, List<FMG.Entry> targetEntries, bool fullReplace)
    {
        TargetFmg = targetFmg;
        IsFullReplace = true;
        DesiredOrder = new List<FMG.Entry>(targetEntries);
        EntriesToMove = new List<FMG.Entry>();
        DropTargetIndex = -1;
    }

    public override ActionEvent Execute()
    {
        return IsFullReplace ? ExecuteFullReplace() : ExecuteDrag();
    }

    private ActionEvent ExecuteDrag()
    {
        OriginalIndices = EntriesToMove
            .Select(r => TargetFmg.Entries.IndexOf(r))
            .ToList();

        if (ResolvedInsertionIndex == -1)
        {
            int shiftCount = OriginalIndices.Count(idx => idx < DropTargetIndex);
            ResolvedInsertionIndex = DropTargetIndex - shiftCount;
        }

        foreach (int idx in OriginalIndices.OrderByDescending(i => i))
            TargetFmg.Entries.RemoveAt(idx);

        int insertAt = ResolvedInsertionIndex;
        foreach (FMG.Entry entry in EntriesToMove)
        {
            TargetFmg.Entries.Insert(insertAt, entry);
            insertAt++;
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    private ActionEvent ExecuteFullReplace()
    {
        // Snapshot the current order for Undo.
        PreviousOrder = new List<FMG.Entry>(TargetFmg.Entries);

        ApplyOrder(DesiredOrder);

        return ActionEvent.ObjectAddedRemoved;
    }

    private void ApplyOrder(List<FMG.Entry> order)
    {
        // Remove from back to front to keep indices valid.
        for (int i = TargetFmg.Entries.Count - 1; i >= 0; i--)
            TargetFmg.Entries.RemoveAt(i);

        for (int i = 0; i < order.Count; i++)
            TargetFmg.Entries.Insert(i, order[i]);
    }

    public override ActionEvent Undo()
    {
        return IsFullReplace ? UndoFullReplace() : UndoDrag();
    }
    private ActionEvent UndoFullReplace()
    {
        ApplyOrder(PreviousOrder);
        return ActionEvent.ObjectAddedRemoved;
    }

    private ActionEvent UndoDrag()
    {
        foreach (FMG.Entry entry in EntriesToMove)
            TargetFmg.Entries.Remove(entry);

        var pairs = EntriesToMove
            .Zip(OriginalIndices, (row, idx) => (row, idx))
            .OrderBy(p => p.idx);

        foreach (var (entry, idx) in pairs)
            TargetFmg.Entries.Insert(idx, entry);

        return ActionEvent.ObjectAddedRemoved;
    }
}
