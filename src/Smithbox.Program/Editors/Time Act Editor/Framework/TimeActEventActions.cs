using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Changes the time span of an event
/// </summary>
public class TimeActMoveEventAction : TimeActAction
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

    public override TimeActEntry TargetEntry => Entry;
    public override TAE.Event TargetEvent => Event;

    public override string Describe() => LOC.Get("TAE_History_Move_Event", NameEvent(Event));

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
public class TimeActAddEventAction : TimeActAction
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
        bool isDelete = false, IReadOnlyDictionary<int, TimeActGroupMapEntry> groupMap = null)
    {
        Selection = selection;
        Entry = entry;
        Animation = animation;
        Event = ev;
        IsDelete = isDelete;

        Index = isDelete ? animation.Events.IndexOf(ev) : animation.Events.Count;

        // A group of its own, as the game's files mostly have (see TimeActEventGroups)
        if (!isDelete)
        {
            NewGroup = TimeActEventGroups.Create(animation, ev.Type, groupMap);
        }
    }

    public override TimeActEntry TargetEntry => Entry;
    public override TAE.Animation TargetAnimation => Animation;
    public override TAE.Event TargetEvent => Event;

    public override string Describe() =>
        LOC.Get(IsDelete ? "TAE_History_Delete_Event" : "TAE_History_Add_Event", NameEvent(Event));

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

        // An undo or redo from another animation leaves the selection alone, as the event isn't on show
        if (Selection.SelectedAnimation == Animation)
        {
            Selection.SelectedEvent = Event;
        }

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
public class TimeActAddEventsAction : TimeActAction
{
    private readonly TimeActSelection Selection;
    private readonly TimeActEntry Entry;
    private readonly TAE.Animation Animation;
    private readonly List<TAE.Event> Events;

    // Groups made for types the animation has none for. Events of the same type share one.
    private readonly List<TAE.EventGroup> NewGroups = new();

    private readonly int Index;

    public TimeActAddEventsAction(TimeActSelection selection, TimeActEntry entry, TAE.Animation animation,
        List<TAE.Event> events, IReadOnlyDictionary<int, TimeActGroupMapEntry> groupMap = null)
    {
        Selection = selection;
        Entry = entry;
        Animation = animation;
        Events = events;

        Index = animation.Events.Count;

        // Each event gets a group of its own, as the game's files mostly have (see TimeActEventGroups)
        foreach (var ev in events)
        {
            if (TimeActEventGroups.Create(animation, ev.Type, groupMap) is not { } group)
                break;

            NewGroups.Add(group);
            ev.Group = group;
        }
    }

    public override TimeActEntry TargetEntry => Entry;
    public override TAE.Animation TargetAnimation => Animation;
    public override TAE.Event TargetEvent => Events.FirstOrDefault();

    public override string Describe() => Events.Count == 1
        ? LOC.Get("TAE_History_Add_Event", NameEvent(Events[0]))
        : LOC.Get("TAE_History_Add_Events", Events.Count);

    public override ActionEvent Execute(bool isRedo = false)
    {
        Animation.EventGroups.AddRange(NewGroups);
        Animation.Events.InsertRange(Index, Events);

        // A redo from another animation leaves the selection alone, as the events aren't on show
        if (Selection.SelectedAnimation == Animation)
        {
            Selection.SelectedEvents.Clear();
            Selection.SelectedEvents.AddRange(Events);
        }

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
public class TimeActDeleteEventsAction : TimeActAction
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

    public override TimeActEntry TargetEntry => Entry;
    public override TAE.Animation TargetAnimation => Animation;

    public override string Describe() => Removed.Count == 1
        ? LOC.Get("TAE_History_Delete_Event", NameEvent(Removed[0].Event))
        : LOC.Get("TAE_History_Delete_Events", Removed.Count);

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

        // An undo from another animation leaves the selection alone, as the events aren't on show
        if (Selection.SelectedAnimation == Animation)
        {
            Selection.SelectedEvents.Clear();
            Selection.SelectedEvents.AddRange(SelectionOrder);
        }

        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Changes the value of an event parameter
/// </summary>
public class TimeActSetParameterAction : TimeActAction
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

    public override TimeActEntry TargetEntry => Entry;
    public override TAE.Event TargetEvent => Event;

    public override string Describe() => LOC.Get("TAE_History_Set_Parameter", Key, NameEvent(Event), FormatValue(NewValue));

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

/// <summary>
/// Puts a template on an event it doesn't fit exactly, at the user's say. The template reads past values it would
/// assert, and the event is written back through it from then on, so the bytes it doesn't keep are given up.
/// Undo takes the template off again and puts back the bytes the event had.
/// </summary>
public class TimeActForceTemplateAction : TimeActAction
{
    private readonly TimeActEntry Entry;
    private readonly TAE.Event Event;
    private readonly TAE.Template.EventTemplate Template;
    private readonly bool BigEndian;
    private readonly byte[] OldBytes;

    public TimeActForceTemplateAction(TimeActEntry entry, TAE.Event ev, TAE.Template.EventTemplate template)
    {
        Entry = entry;
        Event = ev;
        Template = template;
        BigEndian = entry.TimeAct.BigEndian;
        OldBytes = ev.GetParameterBytes(BigEndian).ToArray();
    }

    public override TimeActEntry TargetEntry => Entry;
    public override TAE.Event TargetEvent => Event;

    public override string Describe() => LOC.Get("TAE_History_Force_Template", Template.Name, NameEvent(Event));

    public override ActionEvent Execute(bool isRedo = false)
    {
        // Always from the bytes the event had when this was made, so a redo gives the same values as the first time
        Event.RemoveTemplate(OldBytes.ToArray());
        Event.ApplyTemplate(BigEndian, Template, true);
        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Event.RemoveTemplate(OldBytes.ToArray());
        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Replaces the raw parameter bytes of an event that has no template on it, for events no template fits.
/// The bytes are copied both ways, as the event hands out its own array.
/// </summary>
public class TimeActSetParameterBytesAction : TimeActAction
{
    private readonly TimeActEntry Entry;
    private readonly TAE.Event Event;
    private readonly bool BigEndian;
    private readonly byte[] OldBytes;
    private readonly byte[] NewBytes;

    public TimeActSetParameterBytesAction(TimeActEntry entry, TAE.Event ev, byte[] newBytes)
    {
        Entry = entry;
        Event = ev;
        BigEndian = entry.TimeAct.BigEndian;
        OldBytes = ev.GetParameterBytes(BigEndian).ToArray();
        NewBytes = newBytes.ToArray();
    }

    public override TimeActEntry TargetEntry => Entry;
    public override TAE.Event TargetEvent => Event;

    public override string Describe() => LOC.Get("TAE_History_Set_Bytes", NameEvent(Event));

    public override ActionEvent Execute(bool isRedo = false)
    {
        Event.SetParameterBytes(BigEndian, NewBytes.ToArray());
        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Event.SetParameterBytes(BigEndian, OldBytes.ToArray());
        Entry.IsDirty = true;

        return ActionEvent.NoEvent;
    }
}
