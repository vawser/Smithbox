using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Applies the chosen merge items of one time act (Claude/Documentation/Time Act.md §5.2, D5 of Gparam's):
/// Execute records the time act (<see cref="TimeActMergeSnapshot"/>) and runs <see cref="TimeActMerge.Apply"/>;
/// Undo puts the record back into the same objects, so the selection and history still point at them. The chosen
/// items are fixed when it is made, so a redo applies the same ones; stale items (edited since the analysis) are
/// skipped on every Execute.
/// </summary>
public class TimeActMergeAction : TimeActAction
{
    private readonly TimeActEditorView View;
    private readonly TimeActContainerWrapper Source;
    private readonly TimeActEntry Entry;
    private readonly List<TimeActMergeItem> Items;
    private readonly HashSet<long> AnimationIDs;

    private TimeActMergeSnapshot Snapshot;

    /// <summary>What the last Execute applied and skipped</summary>
    public TimeActMergeApplyResult LastResult { get; private set; }

    public TimeActMergeAction(TimeActEditorView view, TimeActContainerWrapper source, TimeActEntry entry,
        IEnumerable<TimeActMergeItem> items)
    {
        View = view;
        Source = source;
        Entry = entry;
        Items = items.ToList();
        AnimationIDs = Items.Where(e => e.AnimationID >= 0).Select(e => e.AnimationID).ToHashSet();
    }

    public override TimeActContainerWrapper TargetSource => Source;
    public override TimeActEntry TargetEntry => Entry;

    public override string Describe() => LOC.Get("TAE_History_Merge_TimeAct", Entry.Name, Items.Count);

    public override ActionEvent Execute(bool isRedo = false)
    {
        Entry.Load();

        if (Entry.TimeAct == null)
        {
            LastResult = new TimeActMergeApplyResult { Stale = Items.Count };
            return ActionEvent.NoEvent;
        }

        var selected = SelectedAnimation();

        Snapshot = TimeActMergeSnapshot.Take(Entry.TimeAct, AnimationIDs);
        LastResult = TimeActMerge.Apply(Entry.TimeAct, Items);

        Changed(selected);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (Snapshot == null)
            return ActionEvent.NoEvent;

        var selected = SelectedAnimation();

        Snapshot.Restore();

        Changed(selected);

        return ActionEvent.NoEvent;
    }

    private TAE.Animation SelectedAnimation() =>
        View.Selection.SelectedTimeAct == Entry ? View.Selection.SelectedAnimation : null;

    private void Changed(TAE.Animation selected)
    {
        // Their events come without templates: applied again once the animation is selected
        foreach (var animation in Entry.TimeAct.Animations.Where(e => AnimationIDs.Contains(e.ID)))
            Entry.ForgetTemplate(animation);

        Entry.IsDirty = true;
        Entry.MarkAnimationsChanged();

        if (selected == null)
            return;

        // The animation on show: shown again (its events may be others), or let go if it is gone
        if (Entry.TimeAct.Animations.Contains(selected))
            View.AnimationSelectionWindow.SelectAnimation(selected);
        else
            View.AnimationSelectionWindow.ClearSelection();
    }
}
