using SoulsFormats;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActSelection
{
    public TimeActContainerWrapper SelectedTimeActContainerWrapper;
    public TimeActModelWrapper SelectedTimeActModelWrapper;
    public TimeActEntry SelectedTimeAct;
    public TAE.Animation SelectedAnimation;

    /// <summary>
    /// The selected events, in the order they were selected. The first is the primary one,
    /// whose values the properties show.
    /// </summary>
    public List<TAE.Event> SelectedEvents = new();

    /// <summary>
    /// The primary selected event. Setting it replaces the whole selection with just that event.
    /// </summary>
    public TAE.Event SelectedEvent
    {
        get => SelectedEvents.Count > 0 ? SelectedEvents[0] : null;
        set
        {
            SelectedEvents.Clear();

            if (value != null)
            {
                SelectedEvents.Add(value);
            }
        }
    }

    /// <summary>
    /// Drops selected events that aren't in the selected animation, such as ones an undo in another animation put
    /// back. The properties and edits go by the selected time act, so an event from elsewhere would be edited
    /// without its own time act being marked as modified.
    /// </summary>
    public void PruneEvents()
    {
        if (SelectedEvents.Count == 0)
            return;

        var events = SelectedAnimation?.Events;

        if (events == null)
        {
            SelectedEvents.Clear();
            return;
        }

        // A select-all can hold every event of the animation, so the lookup is only built when needed
        if (SelectedEvents.Count <= 8)
        {
            SelectedEvents.RemoveAll(e => !events.Contains(e));
            return;
        }

        var inAnimation = new HashSet<TAE.Event>(events);

        SelectedEvents.RemoveAll(e => !inAnimation.Contains(e));
    }

    public bool IsEventSelected(TAE.Event ev)
    {
        return SelectedEvents.Contains(ev);
    }

    /// <summary>
    /// Whether more than one event is selected, and they are all of the same type, so they can be edited together
    /// </summary>
    public bool AreSelectedEventsSameType()
    {
        if (SelectedEvents.Count < 2)
            return false;

        var type = SelectedEvents[0].Type;

        return SelectedEvents.All(e => e.Type == type);
    }

    public bool IsAnyModelLoaded()
    {
        if (SelectedTimeActModelWrapper != null)
        {
            return true;
        }

        return false;
    }
}
