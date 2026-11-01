using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
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

        if (Selection.SelectedEvent == Event)
        {
            Selection.SelectedEvent = null;
        }

        Entry.IsDirty = true;
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
