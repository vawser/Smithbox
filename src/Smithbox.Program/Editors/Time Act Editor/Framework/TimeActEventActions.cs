using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Changes the time span of an event
/// </summary>
public class TimeActMoveEventAction : ViewportAction
{
    private readonly TimeActEntry Entry;
    private readonly TAE.Event Event;

    private readonly float OldStart;
    private readonly float OldEnd;
    private readonly float NewStart;
    private readonly float NewEnd;

    public TimeActMoveEventAction(TimeActEntry entry, TAE.Event ev, float oldStart, float oldEnd, float newStart, float newEnd)
    {
        Entry = entry;
        Event = ev;
        OldStart = oldStart;
        OldEnd = oldEnd;
        NewStart = newStart;
        NewEnd = newEnd;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Event.StartTime = NewStart;
        Event.EndTime = NewEnd;
        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Event.StartTime = OldStart;
        Event.EndTime = OldEnd;
        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Adds an event to an animation, or removes one when it is told to delete instead
/// </summary>
public class TimeActAddEventAction : ViewportAction
{
    private readonly TimeActSelection Selection;
    private readonly TimeActEntry Entry;
    private readonly TAE.Animation Animation;
    private readonly TAE.Event Event;

    private readonly bool IsDelete;

    // The group is created along with the event, if the animation has none for it
    private readonly TAE.EventGroup NewGroup;

    private int Index;

    public TimeActAddEventAction(TimeActSelection selection, TimeActEntry entry, TAE.Animation animation, TAE.Event ev,
        bool isDelete = false)
    {
        Selection = selection;
        Entry = entry;
        Animation = animation;
        Event = ev;
        IsDelete = isDelete;

        Index = isDelete ? animation.Events.IndexOf(ev) : animation.Events.Count;

        // Only animations that use groups need one, as older files leave them out entirely
        if (!isDelete && animation.EventGroups.Count > 0)
        {
            ev.Group = animation.Events.FirstOrDefault(e => e.Type == ev.Type && e.Group != null)?.Group
                ?? animation.EventGroups.FirstOrDefault(e => e.GroupType == ev.Type);

            if (ev.Group == null)
            {
                NewGroup = new TAE.EventGroup(ev.Type);
                NewGroup.GroupData.DataType = (TAE.EventGroup.EventGroupDataType)ev.Type;
            }
        }
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (IsDelete)
        {
            Remove();
        }
        else
        {
            Insert();
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (IsDelete)
        {
            Insert();
        }
        else
        {
            Remove();
        }

        return ActionEvent.NoEvent;
    }

    private void Insert()
    {
        if (NewGroup != null)
        {
            Animation.EventGroups.Add(NewGroup);
            Event.Group = NewGroup;
        }

        Animation.Events.Insert(Index, Event);

        Selection.SelectedEvent = Event;
        Entry.IsDirty = true;
    }

    private void Remove()
    {
        Animation.Events.RemoveAt(Index);

        if (NewGroup != null)
        {
            Animation.EventGroups.Remove(NewGroup);
        }

        Selection.SelectedEvents.Remove(Event);

        Entry.IsDirty = true;
    }
}

/// <summary>
/// Adds several events to the end of an animation at once, and selects them in the order given
/// </summary>
public class TimeActAddEventsAction : ViewportAction
{
    private readonly TimeActSelection Selection;
    private readonly TimeActEntry Entry;
    private readonly TAE.Animation Animation;
    private readonly List<TAE.Event> Events;

    // Groups made for types the animation has none for. Events of the same type share one.
    private readonly List<TAE.EventGroup> NewGroups = new();

    private readonly int Index;

    public TimeActAddEventsAction(TimeActSelection selection, TimeActEntry entry, TAE.Animation animation,
        List<TAE.Event> events)
    {
        Selection = selection;
        Entry = entry;
        Animation = animation;
        Events = events;

        Index = animation.Events.Count;

        // Only animations that use groups need one, as older files leave them out entirely
        if (animation.EventGroups.Count == 0)
            return;

        foreach (var ev in events)
        {
            ev.Group = animation.Events.FirstOrDefault(e => e.Type == ev.Type && e.Group != null)?.Group
                ?? animation.EventGroups.FirstOrDefault(e => e.GroupType == ev.Type)
                ?? NewGroups.FirstOrDefault(e => e.GroupType == ev.Type);

            if (ev.Group == null)
            {
                var group = new TAE.EventGroup(ev.Type);
                group.GroupData.DataType = (TAE.EventGroup.EventGroupDataType)ev.Type;

                NewGroups.Add(group);
                ev.Group = group;
            }
        }
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Animation.EventGroups.AddRange(NewGroups);
        Animation.Events.InsertRange(Index, Events);

        Selection.SelectedEvents.Clear();
        Selection.SelectedEvents.AddRange(Events);

        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Animation.Events.RemoveRange(Index, Events.Count);

        foreach (var group in NewGroups)
        {
            Animation.EventGroups.Remove(group);
        }

        Selection.SelectedEvents.RemoveAll(Events.Contains);

        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Removes several events from an animation at once. Undoing it puts each back where it was,
/// and selects them again.
/// </summary>
public class TimeActDeleteEventsAction : ViewportAction
{
    private readonly TimeActSelection Selection;
    private readonly TimeActEntry Entry;
    private readonly TAE.Animation Animation;

    // In the order they sit in the animation, so they can be put back from the front
    private readonly List<(int Index, TAE.Event Event)> Removed;

    // The order they were selected in, so the same event is primary again after an undo
    private readonly List<TAE.Event> SelectionOrder;

    public TimeActDeleteEventsAction(TimeActSelection selection, TimeActEntry entry, TAE.Animation animation,
        IEnumerable<TAE.Event> events)
    {
        Selection = selection;
        Entry = entry;
        Animation = animation;

        SelectionOrder = events.Where(animation.Events.Contains).Distinct().ToList();

        Removed = SelectionOrder
            .Select(e => (animation.Events.IndexOf(e), e))
            .OrderBy(e => e.Item1)
            .ToList();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        // From the back, so the indices of the rest still hold
        for (int i = Removed.Count - 1; i >= 0; i--)
        {
            Animation.Events.RemoveAt(Removed[i].Index);

            Selection.SelectedEvents.Remove(Removed[i].Event);
        }

        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var (index, ev) in Removed)
        {
            Animation.Events.Insert(index, ev);
        }

        Selection.SelectedEvents.Clear();
        Selection.SelectedEvents.AddRange(SelectionOrder);

        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Changes the value of an event parameter
/// </summary>
public class TimeActSetParameterAction : ViewportAction
{
    private readonly TimeActEntry Entry;
    private readonly TAE.Event Event;
    private readonly string Key;
    private readonly object OldValue;
    private readonly object NewValue;

    public TimeActSetParameterAction(TimeActEntry entry, TAE.Event ev, string key, object newValue)
    {
        Entry = entry;
        Event = ev;
        Key = key;
        OldValue = ev.Parameters[key];
        NewValue = newValue;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Event.Parameters[Key] = NewValue;
        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Event.Parameters[Key] = OldValue;
        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }
}
