using SoulsFormats;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// An animation whose mini header points at another, so it plays that animation's motion, and with Import Other
/// Anim its events as well
/// </summary>
public record TimeActImportLink(TimeActEntry Entry, TAE.Animation Animation, bool ImportsEvents);

/// <summary>
/// An event of another animation that is the same as the one looked for
/// </summary>
public record TimeActEventLink(TimeActEntry Entry, TAE.Animation Animation, TAE.Event Event);

/// <summary>
/// What points at an animation or event from elsewhere in its source, so a change to something shared can be seen
/// to reach further. Imports are by full animation ID, and only reach within the source. Finding them reads every
/// time act of the source, so it is only done when asked for, and kept until the next edit.
/// </summary>
public class TimeActReverseLinks
{
    private readonly TimeActContainerWrapper Source;

    private int ImportVersion = -1;
    private Dictionary<long, List<TimeActImportLink>> Imports;

    private int EventVersion = -1;
    private (TAE.Event Event, bool SameTimes) EventKey;
    private List<TimeActEventLink> EventLinks;

    public TimeActReverseLinks(TimeActContainerWrapper source)
    {
        Source = source;
    }

    /// <summary>
    /// The animations of the source that import the given one
    /// </summary>
    public List<TimeActImportLink> GetImporters(TimeActEntry entry, TAE.Animation animation)
    {
        if (Imports == null || ImportVersion != TimeActEntry.EditVersion)
        {
            BuildImports();
        }

        var id = entry.GetFullAnimationID(animation, Source.AnimationCategorySize);

        return Imports.TryGetValue(id, out var links)
            ? links.Where(e => e.Animation != animation).ToList()
            : [];
    }

    private void BuildImports()
    {
        foreach (var entry in Source.TimeActs)
        {
            entry.Load();
        }

        ImportVersion = TimeActEntry.EditVersion;
        Imports = new();

        foreach (var entry in Source.TimeActs)
        {
            if (entry.TimeAct == null)
                continue;

            foreach (var animation in entry.TimeAct.Animations)
            {
                (long Target, bool Events)? import = animation.MiniHeader switch
                {
                    TAE.Animation.AnimMiniHeader.Standard { ImportsHKX: true } standard =>
                        (standard.ImportHKXSourceAnimID, false),
                    TAE.Animation.AnimMiniHeader.ImportOtherAnim other => (other.ImportFromAnimID, true),
                    _ => null
                };

                if (import is not { } link)
                    continue;

                if (!Imports.TryGetValue(link.Target, out var list))
                {
                    list = new();
                    Imports[link.Target] = list;
                }

                list.Add(new TimeActImportLink(entry, animation, link.Events));
            }
        }
    }

    /// <summary>
    /// The events of other animations of the source with the same type, Unk04 and parameters as the given one,
    /// and with the same start and end too if asked
    /// </summary>
    public List<TimeActEventLink> FindIdenticalEvents(TAE.Event ev, bool sameTimes)
    {
        if (EventLinks != null && EventVersion == TimeActEntry.EditVersion && EventKey == (ev, sameTimes))
            return EventLinks;

        foreach (var entry in Source.TimeActs)
        {
            entry.Load();
        }

        EventVersion = TimeActEntry.EditVersion;
        EventKey = (ev, sameTimes);
        EventLinks = new();

        var bigEndian = Source.TimeActs.FirstOrDefault(e => e.TimeAct != null)?.TimeAct.BigEndian ?? false;
        var bytes = ev.GetParameterBytes(bigEndian);

        foreach (var entry in Source.TimeActs)
        {
            if (entry.TimeAct == null)
                continue;

            foreach (var animation in entry.TimeAct.Animations)
            {
                foreach (var other in animation.Events)
                {
                    if (other == ev || other.Type != ev.Type || other.Unk04 != ev.Unk04)
                        continue;

                    if (sameTimes && (other.StartTime != ev.StartTime || !TimeActCompare.EndsEqual(other, ev)))
                        continue;

                    if (!TimeActCompare.ParametersEqual(other.GetParameterBytes(bigEndian), bytes))
                        continue;

                    EventLinks.Add(new TimeActEventLink(entry, animation, other));
                }
            }
        }

        return EventLinks;
    }
}
