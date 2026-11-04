using SoulsFormats;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Removes events from many animations at once, for the mass edit. Each animation's events are removed from the
/// back, so the positions of the rest still hold, and put back from the front on undo. The selection is put back
/// as it was before, rather than to the events of any one animation.
/// </summary>
public class TimeActMassDeleteEventsAction : ViewportAction
{
    private record Group(TimeActEntry Entry, TAE.Animation Animation, List<(int Index, TAE.Event Event)> Removed);

    private readonly TimeActSelection Selection;
    private readonly List<Group> Groups = new();
    private readonly HashSet<TAE.Event> RemovedEvents = new();

    private List<TAE.Event> SelectionBefore = new();

    public int EventCount => RemovedEvents.Count;

    public TimeActMassDeleteEventsAction(TimeActSelection selection,
        IEnumerable<(TimeActEntry Entry, TAE.Animation Animation, IEnumerable<TAE.Event> Events)> groups)
    {
        Selection = selection;

        foreach (var (entry, animation, events) in groups)
        {
            var removed = events
                .Where(animation.Events.Contains)
                .Distinct()
                .Select(e => (animation.Events.IndexOf(e), e))
                .OrderBy(e => e.Item1)
                .ToList();

            if (removed.Count == 0)
                continue;

            Groups.Add(new Group(entry, animation, removed));

            foreach (var (_, ev) in removed)
            {
                RemovedEvents.Add(ev);
            }
        }
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        SelectionBefore = Selection.SelectedEvents.ToList();

        foreach (var group in Groups)
        {
            for (int i = group.Removed.Count - 1; i >= 0; i--)
            {
                group.Animation.Events.RemoveAt(group.Removed[i].Index);
            }

            group.Entry.IsDirty = true;
        }

        Selection.SelectedEvents.RemoveAll(RemovedEvents.Contains);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var group in Groups)
        {
            foreach (var (index, ev) in group.Removed)
            {
                group.Animation.Events.Insert(index, ev);
            }

            group.Entry.IsDirty = true;
        }

        Selection.SelectedEvents.Clear();
        Selection.SelectedEvents.AddRange(SelectionBefore);

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Removes animations from many time acts at once, for the mass edit, in the same way as
/// <see cref="TimeActMassDeleteEventsAction"/>. If the selected animation goes, the one that took its place is
/// selected, as a single delete does. Undo puts the selected animation and events back as they were.
/// </summary>
public class TimeActMassDeleteAnimationsAction : ViewportAction
{
    private record Group(TimeActEntry Entry, List<(int Index, TAE.Animation Animation)> Removed);

    private readonly TimeActEditorView View;
    private readonly List<Group> Groups = new();
    private readonly HashSet<TAE.Animation> RemovedAnimations = new();

    private TAE.Animation AnimationBefore;
    private List<TAE.Event> EventsBefore = new();

    public int AnimationCount => RemovedAnimations.Count;

    public TimeActMassDeleteAnimationsAction(TimeActEditorView view,
        IEnumerable<(TimeActEntry Entry, IEnumerable<TAE.Animation> Animations)> groups)
    {
        View = view;

        foreach (var (entry, animations) in groups)
        {
            var list = entry.TimeAct.Animations;

            var removed = animations
                .Where(list.Contains)
                .Distinct()
                .Select(e => (list.IndexOf(e), e))
                .OrderBy(e => e.Item1)
                .ToList();

            if (removed.Count == 0)
                continue;

            Groups.Add(new Group(entry, removed));

            foreach (var (_, animation) in removed)
            {
                RemovedAnimations.Add(animation);
            }
        }
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var selection = View.Selection;

        AnimationBefore = selection.SelectedAnimation;
        EventsBefore = selection.SelectedEvents.ToList();

        foreach (var group in Groups)
        {
            var animations = group.Entry.TimeAct.Animations;

            for (int i = group.Removed.Count - 1; i >= 0; i--)
            {
                animations.RemoveAt(group.Removed[i].Index);
            }

            group.Entry.IsDirty = true;
            group.Entry.MarkAnimationsChanged();
        }

        if (selection.SelectedAnimation != null && RemovedAnimations.Contains(selection.SelectedAnimation))
        {
            SelectReplacement();
        }

        return ActionEvent.NoEvent;
    }

    /// <summary>
    /// The animation now where the selected one was, or the one before it at the end of the list
    /// </summary>
    private void SelectReplacement()
    {
        var selection = View.Selection;
        var entry = selection.SelectedTimeAct;

        var group = Groups.FirstOrDefault(e => e.Entry == entry);
        var animations = entry?.TimeAct?.Animations;

        if (group == null || animations == null || animations.Count == 0)
        {
            View.AnimationSelectionWindow.ClearSelection();
            return;
        }

        var index = group.Removed.First(e => e.Animation == AnimationBefore).Index;

        // The animations removed before it moved it up by one each
        index -= group.Removed.Count(e => e.Index < index);

        View.AnimationSelectionWindow.SelectAnimation(animations[System.Math.Clamp(index, 0, animations.Count - 1)]);
    }

    public override ActionEvent Undo()
    {
        foreach (var group in Groups)
        {
            var animations = group.Entry.TimeAct.Animations;

            foreach (var (index, animation) in group.Removed)
            {
                animations.Insert(System.Math.Clamp(index, 0, animations.Count), animation);
            }

            group.Entry.IsDirty = true;
            group.Entry.MarkAnimationsChanged();
        }

        var selection = View.Selection;

        if (AnimationBefore != null && selection.SelectedAnimation != AnimationBefore &&
            selection.SelectedTimeAct?.TimeAct?.Animations.Contains(AnimationBefore) == true)
        {
            View.AnimationSelectionWindow.SelectAnimation(AnimationBefore);
        }

        if (selection.SelectedAnimation == AnimationBefore)
        {
            selection.SelectedEvents.Clear();
            selection.SelectedEvents.AddRange(EventsBefore);
        }

        return ActionEvent.NoEvent;
    }
}
