#nullable enable
using SoulsFormats;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// An undoable change to a cutscene's MQB, as a pair of closures. Marks the file changed, and puts its changed state
/// back on undo.
/// </summary>
public class CutsceneEditAction : EditorAction
{
    private readonly CutsceneFile File;
    private readonly Action Do;
    private readonly Action UndoChange;
    private readonly bool WasDirty;

    public CutsceneEditAction(CutsceneFile file, Action execute, Action undo)
    {
        File = file;
        Do = execute;
        UndoChange = undo;
        WasDirty = file.IsDirty;
    }

    public override ActionEvent Execute()
    {
        Do();
        File.IsDirty = true;
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        UndoChange();
        File.IsDirty = WasDirty;
        return ActionEvent.NoEvent;
    }

    /// <summary>An action that sets a value, from what it is now to what it should be</summary>
    public static CutsceneEditAction Set<T>(CutsceneFile file, Func<T> get, Action<T> set, T value)
    {
        var before = get();
        return new CutsceneEditAction(file, () => set(value), () => set(before));
    }
}

/// <summary>
/// What editing an MQB needs that the format doesn't give: deep copies of its objects, new event IDs, and the rules
/// the vanilla files keep (Claude/Tools/MqbFieldSurvey, ER):
/// <list type="bullet">
/// <item>Event IDs are unique within a file, and below 100000: a new event takes the highest + 1.</item>
/// <item>Unk18 always equals Unk1C. For models, cameras, offsets and HKTs both equal Duration; for other types they are
/// mostly 101. A new duration carries them along when they equalled the old one.</item>
/// <item>New events go in a timeline of their own, appended to the cut, so no timeline index (ParentIndex) moves.
/// Deleting an event leaves its timeline, empty, which vanilla files also have.</item>
/// </list>
/// </summary>
public static class CutsceneMqbOps
{
    public static int NextEventId(MQB mqb)
    {
        var max = 0;
        foreach (var cut in mqb.Cuts)
            foreach (var timeline in cut.Timelines)
                foreach (var e in timeline.Events)
                    max = Math.Max(max, e.ID);

        return max + 1;
    }

    /// <summary>The new Unk18/Unk1C for a new duration: carried along if they equalled the old one, else kept.</summary>
    public static int Unk18For(MQB.Event e, int newDuration) => e.Unk18 == e.Duration ? newDuration : e.Unk18;

    public static void SetDuration(MQB.Event e, int duration)
    {
        var unk = Unk18For(e, duration);
        e.Duration = duration;
        e.Unk18 = unk;
        e.Unk1C = unk;
    }

    /// <summary>The timeline holding an event in a cut, -1 if none</summary>
    public static int TimelineOf(MQB.Cut cut, MQB.Event e)
    {
        for (var t = 0; t < cut.Timelines.Count; t++)
            if (cut.Timelines[t].Events.Contains(e))
                return t;
        return -1;
    }

    // ---- copies

    public static MQB.Event Clone(MQB.Event e) => new()
    {
        ID = e.ID,
        ResourceIndex = e.ResourceIndex,
        StartFrame = e.StartFrame,
        Duration = e.Duration,
        Unk14 = e.Unk14,
        Unk18 = e.Unk18,
        Unk1C = e.Unk1C,
        Unk20 = e.Unk20,
        Parameters = e.Parameters.Select(Clone).ToList(),
        Transforms = e.Transforms.Select(Clone).ToList()
    };

    public static MQB.Resource Clone(MQB.Resource r) => new()
    {
        Name = r.Name,
        ParentIndex = r.ParentIndex,
        Type = r.Type,
        Parameters = r.Parameters.Select(Clone).ToList(),
        Path = r.Path
    };

    public static MQB.Parameter Clone(MQB.Parameter p) => new()
    {
        Name = p.Name,
        Type = p.Type,
        MemberCount = p.MemberCount,
        Value = p.Value is byte[] bytes ? bytes.ToArray() : p.Value,
        Sequences = p.Sequences.Select(s => new MQB.Parameter.Sequence
        {
            ValueType = s.ValueType,
            PointType = s.PointType,
            ValueIndex = s.ValueIndex,
            Points = s.Points.Select(pt => new MQB.Parameter.Sequence.Point
            {
                Value = pt.Value,
                Unk08 = pt.Unk08,
                Unk10 = pt.Unk10,
                Unk14 = pt.Unk14
            }).ToList()
        }).ToList()
    };

    public static MQB.Transform Clone(MQB.Transform t) => new()
    {
        Frame = t.Frame,
        Translation = t.Translation,
        Unk10 = t.Unk10,
        Unk1C = t.Unk1C,
        Rotation = t.Rotation,
        Unk34 = t.Unk34,
        Unk40 = t.Unk40,
        Scale = t.Scale,
        Unk58 = t.Unk58,
        Unk64 = t.Unk64
    };

    // ---- structure

    /// <summary>
    /// An action adding an event in a new timeline at the end of the cut, under <paramref name="parentTimeline" />,
    /// and, if given, a new resource at the end of the resource list for it to use.
    /// </summary>
    public static CutsceneEditAction AddEvent(CutsceneFile file, MQB.Cut cut, MQB.Event e, int parentTimeline,
        List<MQB.Parameter> timelineParameters, MQB.Resource? newResource)
    {
        var mqb = file.Mqb!;
        var timeline = new MQB.Timeline
        {
            ParentIndex = parentTimeline,
            Parameters = timelineParameters,
            Events = new List<MQB.Event> { e }
        };

        return new CutsceneEditAction(file,
            () =>
            {
                if (newResource != null)
                {
                    mqb.Resources.Add(newResource);
                    e.ResourceIndex = mqb.Resources.Count - 1;
                }

                cut.Timelines.Add(timeline);
            },
            () =>
            {
                // Added last, so they are last: taken off the end
                cut.Timelines.Remove(timeline);
                if (newResource != null)
                    mqb.Resources.Remove(newResource);
            });
    }

    /// <summary>
    /// An action copying an event into a timeline of its own beside it (the same parent), with a new ID. With
    /// <paramref name="newResource" />, the copy gets a copy of the resource too, so it can be told apart (a second
    /// subtitle, light, SFX).
    /// </summary>
    public static CutsceneEditAction Duplicate(CutsceneFile file, MQB.Cut cut, MQB.Event e, bool newResource)
    {
        var mqb = file.Mqb!;
        var t = TimelineOf(cut, e);
        var source = t >= 0 ? cut.Timelines[t] : null;

        var copy = Clone(e);
        copy.ID = NextEventId(mqb);

        MQB.Resource? resource = null;
        if (newResource && e.ResourceIndex >= 0 && e.ResourceIndex < mqb.Resources.Count)
        {
            resource = Clone(mqb.Resources[e.ResourceIndex]);
            resource.Name = UniqueName(mqb, resource.Name);
        }

        return AddEvent(file, cut, copy, source?.ParentIndex ?? -1,
            source?.Parameters.Select(Clone).ToList() ?? new List<MQB.Parameter>(), resource);
    }

    /// <summary>An action taking an event out of its timeline, which stays (empty if it was the only one)</summary>
    public static CutsceneEditAction? Delete(CutsceneFile file, MQB.Cut cut, MQB.Event e)
    {
        var t = TimelineOf(cut, e);
        if (t < 0)
            return null;

        var timeline = cut.Timelines[t];
        var index = timeline.Events.IndexOf(e);

        return new CutsceneEditAction(file,
            () => timeline.Events.Remove(e),
            () => timeline.Events.Insert(Math.Min(index, timeline.Events.Count), e));
    }

    /// <summary>A resource name the file doesn't use yet: the name with _2, _3... after its stem</summary>
    public static string UniqueName(MQB mqb, string name)
    {
        var names = mqb.Resources.Select(r => r.Name).ToHashSet(StringComparer.Ordinal);
        if (!names.Contains(name))
            return name;

        var stem = name;
        var underscore = name.LastIndexOf('_');
        if (underscore > 0 && int.TryParse(name[(underscore + 1)..], out _))
            stem = name[..underscore];

        for (var i = 2; ; i++)
        {
            var candidate = $"{stem}_{i}";
            if (!names.Contains(candidate))
                return candidate;
        }
    }

    /// <summary>The types an event can be added of from a template: not the ones that need the cut's Havok data</summary>
    public static bool CanTemplate(int type) => type is not (CutsceneResourceTypes.Model or CutsceneResourceTypes.Camera
        or CutsceneResourceTypes.Offset or CutsceneResourceTypes.Animation);
}
