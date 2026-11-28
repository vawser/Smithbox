using SoulsFormats;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// What a merge can change in one time act, recorded before it is applied so undo can put it back into the same
/// objects (Claude/Documentation/Time Act.md §5.2). A merge changes the header, which animations are listed,
/// an animation's properties and which event and group lists it holds, and (from P2) the events of the animations it
/// touches in place; it never changes an object it takes over from theirs, so recording references is enough for
/// everything but those events.
///
/// The selection and the undo history keep pointing at the same time act, animation and event objects after a
/// restore, as nothing is swapped for a copy.
/// </summary>
public class TimeActMergeSnapshot
{
    private sealed record EventState(TAE.Event Event, float Start, float End, int Unk04, TAE.EventGroup Group,
        byte[] Bytes, TAE.Template.EventTemplate Template);

    private sealed record AnimationState(TAE.Animation Animation, long ID, TAE.Animation.AnimMiniHeader MiniHeader,
        string AnimFileName, List<TAE.Event> Events, TAE.Event[] EventItems, List<TAE.EventGroup> Groups,
        TAE.EventGroup[] GroupItems, List<EventState> EventStates);

    private TAE TimeAct;
    private bool BigEndian;

    private int ID;
    private long EventBank;
    private string SkeletonName;
    private string SibName;
    private byte[] Flags;

    private TAE.Animation[] Animations;
    private List<AnimationState> AnimationStates;

    /// <summary>
    /// Records the time act. The events of the animations with the given IDs are recorded value by value as well
    /// (those a merge changes in place); every other animation only by its lists.
    /// </summary>
    public static TimeActMergeSnapshot Take(TAE timeAct, IEnumerable<long> eventAnimationIDs = null)
    {
        var withEvents = eventAnimationIDs?.ToHashSet() ?? new HashSet<long>();

        var snapshot = new TimeActMergeSnapshot
        {
            TimeAct = timeAct,
            BigEndian = timeAct.BigEndian,
            ID = timeAct.ID,
            EventBank = timeAct.EventBank,
            SkeletonName = timeAct.SkeletonName,
            SibName = timeAct.SibName,
            Flags = timeAct.Flags?.ToArray(),
            Animations = timeAct.Animations.ToArray(),
            AnimationStates = new List<AnimationState>(timeAct.Animations.Count)
        };

        foreach (var animation in timeAct.Animations)
        {
            List<EventState> events = null;

            if (withEvents.Contains(animation.ID))
            {
                events = animation.Events
                    .Select(e => new EventState(e, e.StartTime, e.EndTime, e.Unk04, e.Group,
                        e.GetParameterBytes(timeAct.BigEndian).ToArray(), e.Template))
                    .ToList();
            }

            snapshot.AnimationStates.Add(new AnimationState(animation, animation.ID, animation.MiniHeader,
                animation.AnimFileName, animation.Events, animation.Events.ToArray(), animation.EventGroups,
                animation.EventGroups.ToArray(), events));
        }

        return snapshot;
    }

    /// <summary>Puts everything recorded back, into the same objects</summary>
    public void Restore()
    {
        TimeAct.ID = ID;
        TimeAct.EventBank = EventBank;
        TimeAct.SkeletonName = SkeletonName;
        TimeAct.SibName = SibName;
        TimeAct.Flags = Flags?.ToArray();

        TimeAct.Animations.Clear();
        TimeAct.Animations.AddRange(Animations);

        foreach (var state in AnimationStates)
        {
            var animation = state.Animation;

            animation.ID = state.ID;
            animation.MiniHeader = state.MiniHeader;
            animation.AnimFileName = state.AnimFileName;

            animation.Events = state.Events;
            animation.Events.Clear();
            animation.Events.AddRange(state.EventItems);

            animation.EventGroups = state.Groups;
            animation.EventGroups.Clear();
            animation.EventGroups.AddRange(state.GroupItems);

            if (state.EventStates == null)
                continue;

            foreach (var ev in state.EventStates)
            {
                ev.Event.StartTime = ev.Start;
                ev.Event.EndTime = ev.End;
                ev.Event.Unk04 = ev.Unk04;
                ev.Event.Group = ev.Group;

                // Back to the recorded bytes, then the template it had, as the revert actions do
                ev.Event.RemoveTemplate(ev.Bytes.ToArray());

                if (ev.Template != null)
                    ev.Event.ApplyTemplate(BigEndian, ev.Template, true);
            }
        }
    }
}
