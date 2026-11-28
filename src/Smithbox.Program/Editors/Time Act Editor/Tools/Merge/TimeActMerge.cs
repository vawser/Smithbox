using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>What a merge item does (Claude/Documentation/Time Act.md §5.1)</summary>
public enum TimeActMergeKind
{
    /// <summary>Only they have it (and the game doesn't): add theirs</summary>
    Added,

    /// <summary>Only they changed it: take theirs</summary>
    Changed,

    /// <summary>Both changed it differently, both added it differently, or I deleted it and they changed it</summary>
    Conflict,

    /// <summary>They deleted a vanilla entry I still have (whole sources only)</summary>
    RemovedByThem
}

/// <summary>The level a merge item works at</summary>
public enum TimeActMergeLevel
{
    /// <summary>A whole time act (added, re-added over my deletion, or removed)</summary>
    TimeAct,

    /// <summary>A time act's header (ID, EventBank, SkeletonName, SibName, Flags)</summary>
    Header,

    /// <summary>A whole animation (added, re-added over my deletion, removed, or added by both differently)</summary>
    Animation,

    /// <summary>An animation's mini header and file name</summary>
    AnimationProperties,

    /// <summary>
    /// An animation's events and groups as a whole: taken when only they changed the events, or when the groups
    /// can't be lined up (a side removed or reordered the game's groups)
    /// </summary>
    AnimationBody,

    /// <summary>A whole event (added, re-added over my deletion, or removed)</summary>
    Event,

    /// <summary>An event's start and end</summary>
    EventTiming,

    /// <summary>An event's Unk04, parameters and group</summary>
    EventContent,

    /// <summary>
    /// An animation's HKX file, whole, by full animation ID (Claude/Documentation/Time Act.md §5.2). Built by
    /// the editor, which reads the clip binders; applied as a pending import, not by <see cref="TimeActMerge.Apply"/>.
    /// </summary>
    Clip
}

/// <summary>
/// Their HKX file for a clip item: the bytes (decompressed, as an import holds them), the binders of mine it goes
/// into, and the binder of theirs it came from
/// </summary>
public record TimeActMergeClip(long ID, byte[] Bytes, IReadOnlyList<string> BinderPaths, string TheirsBinder);

/// <summary>
/// Where an event they added goes: after mine's counterpart of one of their earlier events, or after another event
/// of theirs the merge puts in
/// </summary>
public readonly record struct TimeActMergeAnchor(int MineIndex, int TheirsIndex)
{
    public static TimeActMergeAnchor Mine(int index) => new(index, -1);
    public static TimeActMergeAnchor Theirs(int index) => new(-1, index);
}

/// <summary>
/// What the event items of one animation share: the animations as analysed and how their events and groups line up
/// </summary>
public class TimeActMergeEventContext
{
    public TAE.Animation MineAnimation { get; init; }
    public TAE.Animation TheirsAnimation { get; init; }

    /// <summary>
    /// The game's group count. Edits add groups after the game's and leave them in place when events go, so the first
    /// ones of each side line up with the game's by position; the rest are that side's own.
    /// </summary>
    public int VanillaGroupCount { get; init; }

    /// <summary>Which of mine's events (by index) were matched to the game's; the others are my additions</summary>
    public bool[] MineMatched { get; init; }
}

/// <summary>
/// The starting choice for conflicts. Saved as an int in <c>TimeActEditor_Merge_Conflict_Mode</c> (P5).
/// </summary>
public enum TimeActMergeConflictMode
{
    KeepMine = 0,
    TakeTheirs = 1,
    AskEach = 2
}

/// <summary>A conflict's resolution</summary>
public enum TimeActMergeChoice
{
    Unresolved,
    Mine,
    Theirs
}

public class TimeActMergeOptions
{
    /// <summary>
    /// The source holds these time acts whole, so anything vanilla it lacks was deleted. False for partial sources,
    /// which never report removals.
    /// </summary>
    public bool SourceIsWhole { get; init; } = true;

    public TimeActMergeConflictMode ConflictMode { get; init; } = TimeActMergeConflictMode.KeepMine;

    /// <summary>
    /// Tick Removed-by-Them items by default. Never ticks one where I changed the entry
    /// (<see cref="TimeActMergeItem.MineChanged"/>).
    /// </summary>
    public bool IncludeRemovals { get; init; } = false;

    /// <summary>Compare and merge the sources' HKX clips too (D2). Clips are never removed.</summary>
    public bool IncludeClips { get; init; } = true;
}

/// <summary>
/// A time act as the merge sees it: its binder file and either its stored bytes, its parsed time act, or both.
/// A time act with unsaved edits is given parsed (a clone, for the stale check) and without bytes, as its stored
/// bytes no longer say what it holds.
/// </summary>
public class TimeActMergeTimeAct
{
    public int BinderID { get; init; }

    /// <summary>The name without extension, such as a000</summary>
    public string Name { get; init; }

    /// <summary>The binder file's full name, flags and compression, for adding it to another binder</summary>
    public string BinderFileName { get; init; }
    public Binder.FileFlags BinderFlags { get; init; }
    public DCX.Type CompressionType { get; init; }

    /// <summary>The stored bytes, or empty when the time act has changes they don't hold</summary>
    public ReadOnlyMemory<byte> Bytes { get; init; }

    /// <summary>The caller's own reference (a TimeActEntry in the editor)</summary>
    public object Tag { get; init; }

    private TAE _timeAct;
    private bool _readFailed;

    public TimeActMergeTimeAct(TAE timeAct = null)
    {
        _timeAct = timeAct;
    }

    /// <summary>The parsed time act, read from the bytes the first time it is asked for. Null if it can't be read.</summary>
    public TAE TimeAct
    {
        get
        {
            if (_timeAct != null || _readFailed || Bytes.IsEmpty)
                return _timeAct;

            try
            {
                _timeAct = TAE.Read(Bytes.ToArray());
            }
            catch (Exception)
            {
                _readFailed = true;
            }

            return _timeAct;
        }
    }

    public bool ReadFailed => TimeAct == null;

    /// <summary>Whether the stored bytes speak for the time act (it was never parsed into an edited state)</summary>
    public bool HasBytes => !Bytes.IsEmpty;
}

/// <summary>Something that kept an entry out of the merge (localization key suffix in P5: TAE_Merge_Problem_{Code})</summary>
public record TimeActMergeProblem(string Code, string Subject, string Detail = null);

/// <summary>
/// One change the merge would make. The objects are references into what the plan was built from (the editor
/// passes clones of its own time acts, so <see cref="Mine"/> also records what was analysed for the stale check).
/// </summary>
public class TimeActMergeItem
{
    public TimeActMergeLevel Level { get; init; }
    public TimeActMergeKind Kind { get; init; }

    public int BinderID { get; init; }

    /// <summary>The time act's name (a000), for display</summary>
    public string TimeActName { get; init; }

    /// <summary>The animation ID (Animation, AnimationProperties and AnimationBody levels), else -1</summary>
    public long AnimationID { get; init; } = -1;

    /// <summary>
    /// Vanilla / mine / theirs at this level: TimeActMergeTimeAct (TimeAct), TAE (Header) or TAE.Animation (the
    /// animation levels). Null where that side lacks it.
    /// </summary>
    public object Vanilla { get; init; }
    public object Mine { get; init; }
    public object Theirs { get; init; }

    /// <summary>Removed by Them: I changed the entry they deleted, so it is never ticked by default</summary>
    public bool MineChanged { get; init; }

    /// <summary>Event levels: the event's index in the game's, mine's (as analysed) and their events; -1 where absent</summary>
    public int VanillaIndex { get; init; } = -1;
    public int MineIndex { get; init; } = -1;
    public int TheirsIndex { get; init; } = -1;

    /// <summary>Event levels: their event's group, by index in their animation's groups; -1 for none</summary>
    public int TheirsGroupIndex { get; init; } = -1;

    /// <summary>An event they add: where it goes, nearest first (at the start when none is found)</summary>
    public IReadOnlyList<TimeActMergeAnchor> Preceding { get; init; } = Array.Empty<TimeActMergeAnchor>();

    /// <summary>Event levels: shared by the items of the animation</summary>
    public TimeActMergeEventContext Events { get; init; }

    /// <summary>Why it is a conflict (localization key suffix, e.g. "Both_Changed")</summary>
    public string Reason { get; init; }

    public bool Included { get; set; }

    /// <summary>Conflicts only</summary>
    public TimeActMergeChoice Choice { get; set; } = TimeActMergeChoice.Unresolved;

    /// <summary>Whether applying this item writes their side (else it is skipped or keeps mine)</summary>
    public bool TakesTheirs => Included && (Kind != TimeActMergeKind.Conflict || Choice == TimeActMergeChoice.Theirs);

    public string Path => Level switch
    {
        TimeActMergeLevel.TimeAct => TimeActName,
        TimeActMergeLevel.Header => $"{TimeActName} -> (header)",
        TimeActMergeLevel.Animation => $"{TimeActName} -> {AnimationID}",
        TimeActMergeLevel.AnimationProperties => $"{TimeActName} -> {AnimationID} -> (properties)",
        TimeActMergeLevel.AnimationBody => $"{TimeActName} -> {AnimationID} -> (events)",
        TimeActMergeLevel.Clip => $"(clip) {AnimationID}",
        _ => $"{TimeActName} -> {AnimationID} -> {DescribeEvent()}{Level switch
        {
            TimeActMergeLevel.EventTiming => " (timing)",
            TimeActMergeLevel.EventContent => " (content)",
            _ => ""
        }}"
    };

    private string DescribeEvent()
    {
        var ev = (Theirs ?? Mine ?? Vanilla) as TAE.Event;
        return ev == null ? "event" : $"event {ev.Type} @ {ev.StartTime:0.###}";
    }

    public override string ToString() => $"{Kind} {Level} {Path}{(Reason != null ? $" ({Reason})" : "")}";
}

/// <summary>The merge plan for one source (an anibnd, or an asset's container)</summary>
public class TimeActMergeSourcePlan
{
    public string SourceKey { get; init; }

    public List<TimeActMergeItem> Items { get; } = new();

    /// <summary>Time acts or animations left out of the merge, and why</summary>
    public List<TimeActMergeProblem> Problems { get; } = new();

    public int Count(TimeActMergeKind kind) => Items.Count(e => e.Kind == kind);

    public bool HasUnresolved => Items.Any(e =>
        e.Included && e.Kind == TimeActMergeKind.Conflict && e.Choice == TimeActMergeChoice.Unresolved);

    /// <summary>The items of one time act below the TimeAct level, for <see cref="TimeActMerge.Apply"/></summary>
    public IEnumerable<TimeActMergeItem> ItemsOf(int binderID) =>
        Items.Where(e => e.BinderID == binderID && e.Level is not (TimeActMergeLevel.TimeAct or TimeActMergeLevel.Clip));
}

/// <summary>Result of applying items to a time act</summary>
public class TimeActMergeApplyResult
{
    public int Applied;

    /// <summary>Items whose target changed since the analysis (or whose parent is gone), left alone</summary>
    public int Stale;

    public List<string> StaleItems { get; } = new();

    public void Add(TimeActMergeApplyResult other)
    {
        Applied += other.Applied;
        Stale += other.Stale;
        StaleItems.AddRange(other.StaleItems);
    }
}

/// <summary>
/// Three-way time act merge: vanilla (V) / mine (M) / theirs (O) (Claude/Documentation/Time Act.md §5.1).
/// Time acts match by binder ID, animations by ID. Per entry: O = M or O = V: nothing (my edits kept). M = V: Changed
/// (theirs). Only O, not in V: Added. Both changed differently, both added differently, or I deleted and they changed:
/// Conflict. In V and M but not O (whole sources): Removed by Them. Changes to different animations of one time act
/// combine; an animation's properties and its events are separate items, so those combine too.
///
/// P1 merges an animation's events as a whole (AnimationBody); P2 matches them one by one.
///
/// Applying takes their objects over rather than copying them: a copy made through the public constructors loses
/// the layout an animation was read with (an empty file name block, a missing name tail), so it wouldn't write back
/// as theirs did. The plan owns what it read for theirs, so nothing else holds those objects.
///
/// No editor dependencies, so Claude/Tools/TimeActMergeSurvey can run it.
/// </summary>
public static class TimeActMerge
{
    #region Plan

    /// <summary>
    /// The plan for one source. <paramref name="vanilla"/> is null when the game has no such source; mine and theirs
    /// must both be given (a source only they have isn't merged, it is added whole).
    /// </summary>
    public static TimeActMergeSourcePlan BuildPlan(string sourceKey, IReadOnlyList<TimeActMergeTimeAct> vanilla,
        IReadOnlyList<TimeActMergeTimeAct> mine, IReadOnlyList<TimeActMergeTimeAct> theirs,
        TimeActMergeOptions options = null)
    {
        options ??= new TimeActMergeOptions();

        if (mine == null)
            throw new ArgumentNullException(nameof(mine));
        if (theirs == null)
            throw new ArgumentNullException(nameof(theirs));

        var plan = new TimeActMergeSourcePlan { SourceKey = sourceKey };

        var v = IndexTimeActs(plan, vanilla, "vanilla");
        var m = IndexTimeActs(plan, mine, "mine");
        var o = IndexTimeActs(plan, theirs, "theirs");

        foreach (var id in m.Keys.Union(o.Keys).OrderBy(e => e))
        {
            v.TryGetValue(id, out var vt);
            m.TryGetValue(id, out var mt);
            o.TryGetValue(id, out var ot);

            var name = (mt ?? ot).Name;

            if (mt != null && ot != null)
            {
                CompareTimeAct(plan, id, name, vt, mt, ot, options);
                continue;
            }

            if (ot != null)
            {
                // Only they have it
                if (vt == null)
                {
                    Add(plan, new TimeActMergeItem
                    {
                        Level = TimeActMergeLevel.TimeAct, Kind = TimeActMergeKind.Added, BinderID = id,
                        TimeActName = name, Theirs = ot
                    });
                }
                else if (!SameTimeAct(ot, vt))
                {
                    // I deleted it, they changed it
                    Add(plan, new TimeActMergeItem
                    {
                        Level = TimeActMergeLevel.TimeAct, Kind = TimeActMergeKind.Conflict, BinderID = id,
                        TimeActName = name, Vanilla = vt, Theirs = ot, Reason = "Deleted_Changed"
                    });
                }
                // else: I deleted it, they left it alone: keep the deletion

                continue;
            }

            // Only I have it: removed by them when the game has it and the source is whole
            if (vt != null && options.SourceIsWhole)
            {
                Add(plan, new TimeActMergeItem
                {
                    Level = TimeActMergeLevel.TimeAct, Kind = TimeActMergeKind.RemovedByThem, BinderID = id,
                    TimeActName = name, Vanilla = vt, Mine = mt, MineChanged = !SameTimeAct(mt, vt)
                });
            }
        }

        SetDefaults(plan, options);
        return plan;
    }

    private static Dictionary<int, TimeActMergeTimeAct> IndexTimeActs(TimeActMergeSourcePlan plan,
        IReadOnlyList<TimeActMergeTimeAct> list, string side)
    {
        var index = new Dictionary<int, TimeActMergeTimeAct>();

        if (list == null)
            return index;

        foreach (var timeAct in list)
        {
            if (!index.TryAdd(timeAct.BinderID, timeAct))
                plan.Problems.Add(new TimeActMergeProblem("Duplicate_TimeAct", timeAct.Name, side));
        }

        return index;
    }

    private static void CompareTimeAct(TimeActMergeSourcePlan plan, int id, string name, TimeActMergeTimeAct vt,
        TimeActMergeTimeAct mt, TimeActMergeTimeAct ot, TimeActMergeOptions options)
    {
        // The usual case: a time act neither side changed, or one only I changed, is never parsed
        if (BytesEqual(ot, mt) || (vt != null && BytesEqual(ot, vt)))
            return;

        var mine = mt.TimeAct;
        var theirs = ot.TimeAct;

        if (mine == null || theirs == null)
        {
            plan.Problems.Add(new TimeActMergeProblem("Read_Failed", name, mine == null ? "mine" : "theirs"));
            return;
        }

        if (mine.BigEndian != theirs.BigEndian || mine.Format != theirs.Format)
        {
            plan.Problems.Add(new TimeActMergeProblem("Format_Mismatch", name,
                $"{mine.Format}{(mine.BigEndian ? " BE" : "")} / {theirs.Format}{(theirs.BigEndian ? " BE" : "")}"));
            return;
        }

        var vanilla = vt?.TimeAct;

        // A broken or different game copy says nothing about these: merged two-way
        if (vanilla != null && (vanilla.BigEndian != mine.BigEndian || vanilla.Format != mine.Format))
            vanilla = null;

        if (vt != null && vanilla == null)
            plan.Problems.Add(new TimeActMergeProblem("Vanilla_Unreadable", name));

        CompareHeader(plan, id, name, vanilla, mine, theirs);
        CompareAnimations(plan, id, name, vanilla, mine, theirs, options);
    }

    private static void CompareHeader(TimeActMergeSourcePlan plan, int id, string name, TAE v, TAE m, TAE o)
    {
        if (TimeActCompare.HeadersEqual(o, m) || (v != null && TimeActCompare.HeadersEqual(o, v)))
            return;

        var kind = v != null && TimeActCompare.HeadersEqual(m, v) ? TimeActMergeKind.Changed : TimeActMergeKind.Conflict;

        Add(plan, new TimeActMergeItem
        {
            Level = TimeActMergeLevel.Header, Kind = kind, BinderID = id, TimeActName = name,
            Vanilla = v, Mine = m, Theirs = o,
            Reason = kind == TimeActMergeKind.Conflict ? (v == null ? "Both_Added" : "Both_Changed") : null
        });
    }

    private static void CompareAnimations(TimeActMergeSourcePlan plan, int id, string name, TAE vanilla, TAE mine,
        TAE theirs, TimeActMergeOptions options)
    {
        var bigEndian = mine.BigEndian;

        var v = IndexAnimations(vanilla);
        var m = IndexAnimations(mine);
        var o = IndexAnimations(theirs);

        // The Save check flags these and the game has none: which of two is which can't be told, so neither merges
        var duplicates = v.Duplicates.Concat(m.Duplicates).Concat(o.Duplicates).ToHashSet();

        foreach (var duplicate in duplicates.OrderBy(e => e))
            plan.Problems.Add(new TimeActMergeProblem("Duplicate_Animation", $"{name} -> {duplicate}"));

        foreach (var animationID in m.Map.Keys.Union(o.Map.Keys).OrderBy(e => e))
        {
            if (duplicates.Contains(animationID))
                continue;

            v.Map.TryGetValue(animationID, out var va);
            m.Map.TryGetValue(animationID, out var ma);
            o.Map.TryGetValue(animationID, out var oa);

            TimeActMergeItem Item(TimeActMergeLevel level, TimeActMergeKind kind, string reason = null,
                bool mineChanged = false) => new()
            {
                Level = level, Kind = kind, BinderID = id, TimeActName = name, AnimationID = animationID,
                Vanilla = va, Mine = ma, Theirs = oa, Reason = reason, MineChanged = mineChanged
            };

            if (ma != null && oa != null)
            {
                if (TimeActCompare.AnimationsEqual(oa, ma, bigEndian) ||
                    (va != null && TimeActCompare.AnimationsEqual(oa, va, bigEndian)))
                    continue;

                if (va == null)
                {
                    // Both added it, differently: with nothing to merge against, one side's whole animation
                    Add(plan, Item(TimeActMergeLevel.Animation, TimeActMergeKind.Conflict, "Both_Added"));
                    continue;
                }

                CompareAnimationParts(plan, Item, va, ma, oa, bigEndian, options);
                continue;
            }

            if (oa != null)
            {
                if (va == null)
                    Add(plan, Item(TimeActMergeLevel.Animation, TimeActMergeKind.Added));
                else if (!TimeActCompare.AnimationsEqual(oa, va, bigEndian))
                    Add(plan, Item(TimeActMergeLevel.Animation, TimeActMergeKind.Conflict, "Deleted_Changed"));

                continue;
            }

            if (va != null && options.SourceIsWhole)
            {
                Add(plan, Item(TimeActMergeLevel.Animation, TimeActMergeKind.RemovedByThem,
                    mineChanged: !TimeActCompare.AnimationsEqual(ma, va, bigEndian)));
            }
        }
    }

    /// <summary>The properties and the events of an animation all three sides have, as separate items</summary>
    private static void CompareAnimationParts(TimeActMergeSourcePlan plan,
        Func<TimeActMergeLevel, TimeActMergeKind, string, bool, TimeActMergeItem> item,
        TAE.Animation va, TAE.Animation ma, TAE.Animation oa, bool bigEndian, TimeActMergeOptions options)
    {
        if (!SameProperties(oa, ma) && !SameProperties(oa, va))
        {
            var kind = SameProperties(ma, va) ? TimeActMergeKind.Changed : TimeActMergeKind.Conflict;

            Add(plan, item(TimeActMergeLevel.AnimationProperties, kind,
                kind == TimeActMergeKind.Conflict ? "Both_Changed" : null, false));
        }

        if (SameBody(oa, ma, bigEndian) || SameBody(oa, va, bigEndian))
            return;

        var groupsLineUp = GroupPrefixIntact(va, ma) && GroupPrefixIntact(va, oa);

        // Only they changed the events: theirs as a whole, exact, unless they removed some. Their removals go through
        // the Removed by Them items event by event (off by default, never in a Full Mod merge), so those are merged
        // event by event; mine is the game's there, so only their side is matched. Groups that don't line up leave
        // the whole body, removals and all.
        if (SameBody(ma, va, bigEndian))
        {
            if (!groupsLineUp || TimeActCompare.MatchEvents(oa.Events, va.Events, bigEndian).Removed.Count == 0)
            {
                Add(plan, item(TimeActMergeLevel.AnimationBody, TimeActMergeKind.Changed, null, false));
                return;
            }
        }
        else if (!groupsLineUp)
        {
            // Both did, and the groups don't line up: one side's events as a whole
            Add(plan, item(TimeActMergeLevel.AnimationBody, TimeActMergeKind.Conflict, "Groups_Changed", false));
            return;
        }

        CompareEvents(plan, item(TimeActMergeLevel.Event, TimeActMergeKind.Added, null, false), va, ma, oa, bigEndian,
            options);
    }

    #endregion

    #region Events

    /// <summary>
    /// The events of an animation both sides changed (Claude/Documentation/Time Act.md §5.1). Each side is
    /// matched to the game's events; per game event, their change to its timing and to its content (Unk04, parameters,
    /// group) are separate items, so one side retiming an event and the other changing its values combine.
    /// </summary>
    private static void CompareEvents(TimeActMergeSourcePlan plan, TimeActMergeItem like, TAE.Animation va,
        TAE.Animation ma, TAE.Animation oa, bool bigEndian, TimeActMergeOptions options)
    {
        var mm = TimeActCompare.MatchEvents(ma.Events, va.Events, bigEndian);
        var om = TimeActCompare.MatchEvents(oa.Events, va.Events, bigEndian);

        var vIndex = IndexOf(va.Events);
        var mIndex = IndexOf(ma.Events);
        var oIndex = IndexOf(oa.Events);

        var mineOf = new TAE.Event[va.Events.Count];
        var theirsOf = new TAE.Event[va.Events.Count];

        foreach (var (m, v) in mm.Matches)
            mineOf[vIndex[v]] = m;

        foreach (var (o, v) in om.Matches)
            theirsOf[vIndex[v]] = o;

        var vanillaGroups = IndexOf(va.EventGroups);
        var mineGroups = IndexOf(ma.EventGroups);
        var theirsGroups = IndexOf(oa.EventGroups);
        var n = va.EventGroups.Count;

        var mineMatched = new bool[ma.Events.Count];
        foreach (var m in mm.Matches.Keys)
            mineMatched[mIndex[m]] = true;

        var context = new TimeActMergeEventContext
        {
            MineAnimation = ma,
            TheirsAnimation = oa,
            VanillaGroupCount = n,
            MineMatched = mineMatched
        };

        GroupRef Ref(TAE.Event ev, Dictionary<TAE.EventGroup, int> groups, int side) =>
            ev.Group == null || !groups.TryGetValue(ev.Group, out var i) ? new GroupRef(-1, 0)
            : i < n ? new GroupRef(i, 0) : new GroupRef(i, side);

        bool ContentEqual(TAE.Event a, GroupRef ga, TAE.Event b, GroupRef gb) =>
            a.Unk04 == b.Unk04 && ga == gb &&
            TimeActCompare.ParametersEqual(a.GetParameterBytes(bigEndian), b.GetParameterBytes(bigEndian));

        TimeActMergeItem Item(TimeActMergeLevel level, TimeActMergeKind kind, int vi, TAE.Event m, TAE.Event o,
            string reason = null, bool mineChanged = false, IReadOnlyList<TimeActMergeAnchor> preceding = null) => new()
        {
            Level = level, Kind = kind, BinderID = like.BinderID, TimeActName = like.TimeActName,
            AnimationID = like.AnimationID, Events = context, Reason = reason, MineChanged = mineChanged,
            Vanilla = vi >= 0 ? va.Events[vi] : null, Mine = m, Theirs = o,
            VanillaIndex = vi, MineIndex = m != null ? mIndex[m] : -1, TheirsIndex = o != null ? oIndex[o] : -1,
            TheirsGroupIndex = o?.Group != null && theirsGroups.TryGetValue(o.Group, out var g) ? g : -1,
            Preceding = preceding ?? Array.Empty<TimeActMergeAnchor>()
        };

        // Where an event of theirs goes: after mine's counterpart of the nearest earlier event of theirs that mine
        // still has, or after an earlier event of theirs that goes in too
        List<TimeActMergeAnchor> Anchors(int theirsIndex)
        {
            var anchors = new List<TimeActMergeAnchor>();

            for (int j = theirsIndex - 1; j >= 0; j--)
            {
                var earlier = oa.Events[j];

                if (om.Matches.TryGetValue(earlier, out var ve))
                {
                    var counterpart = mineOf[vIndex[ve]];

                    if (counterpart != null)
                    {
                        anchors.Add(TimeActMergeAnchor.Mine(mIndex[counterpart]));
                        break;
                    }
                }
                else
                {
                    anchors.Add(TimeActMergeAnchor.Theirs(j));
                }
            }

            return anchors;
        }

        for (int vi = 0; vi < va.Events.Count; vi++)
        {
            var v = va.Events[vi];
            var m = mineOf[vi];
            var o = theirsOf[vi];
            var gv = Ref(v, vanillaGroups, 0);

            if (o == null)
            {
                // They removed it: only from a whole source, and never ticked when I changed it
                if (m != null && options.SourceIsWhole)
                {
                    var mineChanged = !TimingEqual(m, v) || !ContentEqual(m, Ref(m, mineGroups, 1), v, gv);
                    Add(plan, Item(TimeActMergeLevel.Event, TimeActMergeKind.RemovedByThem, vi, m, null,
                        mineChanged: mineChanged));
                }

                continue;
            }

            var go = Ref(o, theirsGroups, 2);
            var timingChanged = !TimingEqual(o, v);
            var contentChanged = !ContentEqual(o, go, v, gv);

            if (!timingChanged && !contentChanged)
                continue;

            if (m == null)
            {
                // I removed it, they changed it
                Add(plan, Item(TimeActMergeLevel.Event, TimeActMergeKind.Conflict, vi, null, o, "Deleted_Changed",
                    preceding: Anchors(oIndex[o])));
                continue;
            }

            var gm = Ref(m, mineGroups, 1);

            if (timingChanged && !TimingEqual(o, m))
            {
                var kind = TimingEqual(m, v) ? TimeActMergeKind.Changed : TimeActMergeKind.Conflict;
                Add(plan, Item(TimeActMergeLevel.EventTiming, kind, vi, m, o,
                    kind == TimeActMergeKind.Conflict ? "Both_Changed" : null));
            }

            if (contentChanged && !ContentEqual(o, go, m, gm))
            {
                var kind = ContentEqual(m, gm, v, gv) ? TimeActMergeKind.Changed : TimeActMergeKind.Conflict;
                Add(plan, Item(TimeActMergeLevel.EventContent, kind, vi, m, o,
                    kind == TimeActMergeKind.Conflict ? "Both_Changed" : null));
            }
        }

        // Their additions, unless I added the very same event
        var mineAdded = mm.Added.GroupBy(e => EventKey(e, bigEndian)).ToDictionary(e => e.Key, e => e.Count());

        foreach (var o in om.Added.OrderBy(e => oIndex[e]))
        {
            var key = EventKey(o, bigEndian);

            if (mineAdded.TryGetValue(key, out var count) && count > 0)
            {
                mineAdded[key] = count - 1;
                continue;
            }

            Add(plan, Item(TimeActMergeLevel.Event, TimeActMergeKind.Added, -1, null, o,
                preceding: Anchors(oIndex[o])));
        }
    }

    /// <summary>
    /// A group as an event sees it: one of the game's (by position, side 0), or one a side added (its index there and
    /// which side, so two sides' own groups never count as the same), or none (index -1)
    /// </summary>
    private readonly record struct GroupRef(int Index, int Side);

    /// <summary>
    /// The game's groups are still the first ones, in order: edits add groups after them and leave them in place when
    /// events go, so groups line up by position
    /// </summary>
    private static bool GroupPrefixIntact(TAE.Animation vanilla, TAE.Animation other)
    {
        if (other.EventGroups.Count < vanilla.EventGroups.Count)
            return false;

        for (int i = 0; i < vanilla.EventGroups.Count; i++)
        {
            if (!SameGroup(vanilla.EventGroups[i], other.EventGroups[i]))
                return false;
        }

        return true;
    }

    private static bool SameGroup(TAE.EventGroup a, TAE.EventGroup b) =>
        a.GroupType == b.GroupType && a.GroupData.Equals(b.GroupData) &&
        (a.RawGroupData ?? []).AsSpan().SequenceEqual(b.RawGroupData ?? []);

    public static bool TimingEqual(TAE.Event a, TAE.Event b) =>
        a.StartTime == b.StartTime && TimeActCompare.EndsEqual(a, b);

    private static Dictionary<T, int> IndexOf<T>(List<T> list) where T : class
    {
        var index = new Dictionary<T, int>(list.Count, ReferenceEqualityComparer.Instance);

        for (int i = 0; i < list.Count; i++)
            index.TryAdd(list[i], i);

        return index;
    }

    /// <summary>Type, times, Unk04 and parameters without trailing zeros (padding), as the matcher keys events</summary>
    private static string EventKey(TAE.Event ev, bool bigEndian)
    {
        var bytes = ev.GetParameterBytes(bigEndian);
        var length = bytes.Length;

        while (length > 0 && bytes[length - 1] == 0)
            length--;

        return $"{ev.Type}|{BitConverter.SingleToInt32Bits(ev.StartTime)}|{BitConverter.SingleToInt32Bits(ev.MemeEndTime)}" +
            $"|{ev.Unk04}|{Convert.ToHexString(bytes, 0, length)}";
    }

    private sealed class AnimationIndex
    {
        public Dictionary<long, TAE.Animation> Map { get; } = new();
        public HashSet<long> Duplicates { get; } = new();
    }

    private static AnimationIndex IndexAnimations(TAE timeAct)
    {
        var index = new AnimationIndex();

        if (timeAct == null)
            return index;

        foreach (var animation in timeAct.Animations)
        {
            if (!index.Map.TryAdd(animation.ID, animation))
                index.Duplicates.Add(animation.ID);
        }

        return index;
    }

    private static void Add(TimeActMergeSourcePlan plan, TimeActMergeItem item) => plan.Items.Add(item);

    private static void SetDefaults(TimeActMergeSourcePlan plan, TimeActMergeOptions options) =>
        ApplyDefaults(plan.Items, options);

    /// <summary>
    /// Ticks items as the options start them: changes and additions on, conflicts by the conflict mode, removals by
    /// Include Removals (never one where I changed the entry). For items built outside <see cref="BuildPlan"/> (clips).
    /// </summary>
    public static void ApplyDefaults(IEnumerable<TimeActMergeItem> items, TimeActMergeOptions options)
    {
        foreach (var item in items)
        {
            switch (item.Kind)
            {
                case TimeActMergeKind.Conflict:
                    item.Included = true;
                    item.Choice = options.ConflictMode switch
                    {
                        TimeActMergeConflictMode.TakeTheirs => TimeActMergeChoice.Theirs,
                        TimeActMergeConflictMode.AskEach => TimeActMergeChoice.Unresolved,
                        _ => TimeActMergeChoice.Mine
                    };
                    break;

                case TimeActMergeKind.RemovedByThem:
                    item.Included = options.IncludeRemovals && !item.MineChanged;
                    break;

                default:
                    item.Included = true;
                    break;
            }
        }
    }

    #endregion

    #region Comparison

    /// <summary>Whether both have stored bytes and they are the same</summary>
    public static bool BytesEqual(TimeActMergeTimeAct a, TimeActMergeTimeAct b)
    {
        return a.HasBytes && b.HasBytes && a.Bytes.Span.SequenceEqual(b.Bytes.Span);
    }

    /// <summary>Whether two time acts hold the same: by bytes when both have them, else parsed</summary>
    public static bool SameTimeAct(TimeActMergeTimeAct a, TimeActMergeTimeAct b)
    {
        if (BytesEqual(a, b))
            return true;

        var x = a.TimeAct;
        var y = b.TimeAct;

        return x != null && y != null && SameTimeAct(x, y);
    }

    public static bool SameTimeAct(TAE a, TAE b)
    {
        if (a.BigEndian != b.BigEndian || a.Format != b.Format || !TimeActCompare.HeadersEqual(a, b) ||
            a.Animations.Count != b.Animations.Count)
            return false;

        for (int i = 0; i < a.Animations.Count; i++)
        {
            if (a.Animations[i].ID != b.Animations[i].ID ||
                !TimeActCompare.AnimationsEqual(a.Animations[i], b.Animations[i], a.BigEndian))
                return false;
        }

        return true;
    }

    public static bool SameProperties(TAE.Animation a, TAE.Animation b)
    {
        return TimeActCompare.MiniHeadersEqual(a.MiniHeader, b.MiniHeader) &&
            (a.AnimFileName ?? "") == (b.AnimFileName ?? "");
    }

    public static bool SameBody(TAE.Animation a, TAE.Animation b, bool bigEndian)
    {
        return TimeActCompare.EventListsEqual(a, b, bigEndian) && TimeActCompare.GroupsEqual(a, b);
    }

    #endregion

    #region Apply

    /// <summary>The ticked items, taking theirs (resolved conflicts included)</summary>
    public static List<TimeActMergeItem> ChosenItems(TimeActMergeSourcePlan plan) =>
        plan.Items.Where(e => e.TakesTheirs).ToList();

    /// <summary>
    /// Whether the target still holds what was analysed for the item: the time act header, or the animation of its
    /// ID, still equal to <see cref="TimeActMergeItem.Mine"/> (or still absent when mine lacked it). Run before any
    /// item of the time act is applied, as applying one changes what the next one would see.
    /// </summary>
    public static bool MatchesAnalysed(TAE target, TimeActMergeItem item)
    {
        switch (item.Level)
        {
            case TimeActMergeLevel.Header:
                return item.Mine is TAE mine && TimeActCompare.HeadersEqual(target, mine);

            case TimeActMergeLevel.Animation:
            case TimeActMergeLevel.AnimationProperties:
            case TimeActMergeLevel.AnimationBody:
                var found = FindAnimations(target, item.AnimationID);

                if (item.Mine is not TAE.Animation analysed)
                    return found.Count == 0;

                return found.Count == 1 && TimeActCompare.AnimationsEqual(found[0], analysed, target.BigEndian);

            case TimeActMergeLevel.Event:
            case TimeActMergeLevel.EventTiming:
            case TimeActMergeLevel.EventContent:
                // By the whole animation: the items point at mine's events by their index as analysed
                var withEvents = FindAnimations(target, item.AnimationID);

                return withEvents.Count == 1 &&
                    TimeActCompare.AnimationsEqual(withEvents[0], item.Events.MineAnimation, target.BigEndian);

            default:
                return false;
        }
    }

    private static bool IsEventLevel(TimeActMergeItem item) =>
        item.Level is TimeActMergeLevel.Event or TimeActMergeLevel.EventTiming or TimeActMergeLevel.EventContent;

    /// <summary>
    /// Applies the chosen items of one time act in place (all below the TimeAct level). Stale items are skipped:
    /// whatever was edited since the analysis is left alone. Their objects are taken over (see the class remarks).
    /// The caller marks the time act modified.
    /// </summary>
    public static TimeActMergeApplyResult Apply(TAE target, IEnumerable<TimeActMergeItem> chosen)
    {
        var result = new TimeActMergeApplyResult();

        var items = chosen.Where(e => e.TakesTheirs && e.Level is not (TimeActMergeLevel.TimeAct or TimeActMergeLevel.Clip)).ToList();

        // All checked first: applying the properties of an animation changes what its events item would compare
        var stale = items.Where(e => !MatchesAnalysed(target, e)).ToHashSet();

        foreach (var item in stale)
        {
            result.Stale++;
            result.StaleItems.Add(item.Path);
        }

        var fresh = items.Where(e => !stale.Contains(e)).ToList();

        foreach (var item in fresh.Where(e => !IsEventLevel(e)).OrderBy(ApplyOrder))
        {
            ApplyItem(target, item);
            result.Applied++;
        }

        // Events last, an animation at a time: its items all point into the events as they were analysed
        foreach (var animation in fresh.Where(IsEventLevel).GroupBy(e => e.AnimationID))
        {
            var list = animation.ToList();
            ApplyEvents(target, list);
            result.Applied += list.Count;
        }

        return result;
    }

    /// <summary>
    /// Applies the event items of one animation in place: their removals, then changes to mine's events, then their
    /// additions in their order. Groups they added that the chosen events use go after mine's, in their order. Their
    /// events and groups are taken over.
    /// </summary>
    private static void ApplyEvents(TAE target, List<TimeActMergeItem> items)
    {
        var context = items[0].Events;
        var live = FindAnimations(target, items[0].AnimationID)[0];
        var bigEndian = target.BigEndian;

        // As analysed (the stale check made sure), so the indices hold
        var mineEvents = live.Events.ToList();
        var mineGroups = live.EventGroups.ToList();
        var theirs = context.TheirsAnimation;
        var n = context.VanillaGroupCount;

        var mineOnly = new HashSet<TAE.Event>(ReferenceEqualityComparer.Instance);
        for (int i = 0; i < mineEvents.Count; i++)
        {
            if (!context.MineMatched[i])
                mineOnly.Add(mineEvents[i]);
        }

        // Their own groups that the chosen events go in, in their order
        var takesGroup = items.Where(e => e.Level != TimeActMergeLevel.EventTiming && e.Theirs != null);
        var newGroups = new Dictionary<int, TAE.EventGroup>();

        foreach (var index in takesGroup.Select(e => e.TheirsGroupIndex).Where(e => e >= n).Distinct().OrderBy(e => e))
        {
            var group = theirs.EventGroups[index];
            newGroups[index] = group;
            live.EventGroups.Add(group);
        }

        TAE.EventGroup Group(int theirsIndex) =>
            theirsIndex < 0 ? null : theirsIndex < n ? mineGroups[theirsIndex] : newGroups[theirsIndex];

        foreach (var item in items.Where(e => e.Kind == TimeActMergeKind.RemovedByThem))
            live.Events.Remove(mineEvents[item.MineIndex]);

        foreach (var item in items.Where(e => e.Level == TimeActMergeLevel.EventTiming))
            SetTiming(mineEvents[item.MineIndex], (TAE.Event)item.Theirs);

        foreach (var item in items.Where(e => e.Level == TimeActMergeLevel.EventContent))
        {
            var mine = mineEvents[item.MineIndex];
            var source = (TAE.Event)item.Theirs;

            mine.Unk04 = source.Unk04;
            mine.RemoveTemplate(source.GetParameterBytes(bigEndian).ToArray());
            mine.Group = Group(item.TheirsGroupIndex);
        }

        // Added, or put back over my deletion
        var inserted = new Dictionary<int, TAE.Event>();

        foreach (var item in items.Where(e => e.Level == TimeActMergeLevel.Event && e.Theirs != null)
            .OrderBy(e => e.TheirsIndex))
        {
            var ev = (TAE.Event)item.Theirs;
            ev.Group = Group(item.TheirsGroupIndex);

            var position = 0;

            foreach (var anchor in item.Preceding)
            {
                var after = anchor.MineIndex >= 0 ? mineEvents[anchor.MineIndex]
                    : inserted.GetValueOrDefault(anchor.TheirsIndex);
                var at = after != null ? live.Events.IndexOf(after) : -1;

                if (at >= 0)
                {
                    position = at + 1;
                    break;
                }
            }

            // My own additions there stay first
            while (position < live.Events.Count && mineOnly.Contains(live.Events[position]))
                position++;

            live.Events.Insert(position, ev);
            inserted[item.TheirsIndex] = ev;
        }
    }

    /// <summary>
    /// Their start and end on my event. The end as stored: an infinite end read as the placeholder may need
    /// float.MaxValue to stay infinite on an event that wasn't read with one.
    /// </summary>
    private static void SetTiming(TAE.Event mine, TAE.Event source)
    {
        mine.StartTime = source.StartTime;
        mine.EndTime = source.EndTime;

        if (!TimeActCompare.EndsEqual(mine, source))
            mine.EndTime = source.MemeEndTime;
    }

    // Removals first, then the header, whole animations, and the parts of animations both sides have
    private static int ApplyOrder(TimeActMergeItem item) => item.Kind == TimeActMergeKind.RemovedByThem ? 0 : item.Level switch
    {
        TimeActMergeLevel.Header => 1,
        TimeActMergeLevel.Animation => 2,
        TimeActMergeLevel.AnimationProperties => 3,
        _ => 4
    };

    private static void ApplyItem(TAE target, TimeActMergeItem item)
    {
        switch (item.Level)
        {
            case TimeActMergeLevel.Header:
                var theirsHeader = (TAE)item.Theirs;

                target.ID = theirsHeader.ID;
                target.EventBank = theirsHeader.EventBank;
                target.SkeletonName = theirsHeader.SkeletonName;
                target.SibName = theirsHeader.SibName;
                target.Flags = theirsHeader.Flags?.ToArray();
                break;

            case TimeActMergeLevel.Animation:
                ApplyAnimation(target, item);
                break;

            case TimeActMergeLevel.AnimationProperties:
                var properties = (TAE.Animation)item.Theirs;
                var withProperties = FindAnimations(target, item.AnimationID)[0];

                withProperties.MiniHeader = properties.MiniHeader?.GetClone();
                withProperties.AnimFileName = properties.AnimFileName;
                break;

            case TimeActMergeLevel.AnimationBody:
                var body = (TAE.Animation)item.Theirs;
                var withBody = FindAnimations(target, item.AnimationID)[0];

                withBody.Events = body.Events;
                withBody.EventGroups = body.EventGroups;
                break;
        }
    }

    private static void ApplyAnimation(TAE target, TimeActMergeItem item)
    {
        var existing = FindAnimations(target, item.AnimationID).FirstOrDefault();
        var theirs = (TAE.Animation)item.Theirs;

        if (theirs == null)
        {
            // Removed by them
            if (existing != null)
                target.Animations.Remove(existing);

            return;
        }

        if (existing != null)
        {
            // Added by both: theirs goes into my animation, which stays where it is for the selection and history
            existing.MiniHeader = theirs.MiniHeader?.GetClone();
            existing.AnimFileName = theirs.AnimFileName;
            existing.Events = theirs.Events;
            existing.EventGroups = theirs.EventGroups;
            return;
        }

        InsertAnimation(target, theirs);
    }

    /// <summary>Puts an animation in ID order, as the editor's Create and Duplicate do</summary>
    public static void InsertAnimation(TAE target, TAE.Animation animation)
    {
        var index = target.Animations.FindIndex(e => e.ID > animation.ID);

        target.Animations.Insert(index < 0 ? target.Animations.Count : index, animation);
    }

    private static List<TAE.Animation> FindAnimations(TAE target, long id) =>
        target.Animations.Where(e => e.ID == id).ToList();

    /// <summary>
    /// Applies a whole plan to a list of time acts (the survey; the editor builds actions instead): the TimeAct-level
    /// items add, re-add or remove whole time acts in binder ID order, then each time act both sides kept gets its own
    /// items. The time acts merged into are found by binder ID.
    /// </summary>
    public static TimeActMergeApplyResult ApplyToList(List<TimeActMergeTimeAct> target, IEnumerable<TimeActMergeItem> chosen)
    {
        var result = new TimeActMergeApplyResult();
        var items = chosen.Where(e => e.TakesTheirs).ToList();

        foreach (var item in items.Where(e => e.Level == TimeActMergeLevel.TimeAct))
        {
            var existing = target.FirstOrDefault(e => e.BinderID == item.BinderID);

            if (item.Theirs == null)
            {
                if (existing == null || (item.Mine != null && existing != item.Mine && !SameTimeAct(existing, (TimeActMergeTimeAct)item.Mine)))
                {
                    result.Stale++;
                    result.StaleItems.Add(item.Path);
                    continue;
                }

                target.Remove(existing);
            }
            else
            {
                if (existing != null)
                {
                    result.Stale++;
                    result.StaleItems.Add(item.Path);
                    continue;
                }

                var index = target.FindIndex(e => e.BinderID > item.BinderID);
                target.Insert(index < 0 ? target.Count : index, (TimeActMergeTimeAct)item.Theirs);
            }

            result.Applied++;
        }

        foreach (var group in items.Where(e => e.Level is not (TimeActMergeLevel.TimeAct or TimeActMergeLevel.Clip)).GroupBy(e => e.BinderID))
        {
            var timeAct = target.FirstOrDefault(e => e.BinderID == group.Key)?.TimeAct;

            if (timeAct == null)
            {
                foreach (var item in group)
                {
                    result.Stale++;
                    result.StaleItems.Add(item.Path);
                }

                continue;
            }

            result.Add(Apply(timeAct, group));
        }

        return result;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// A copy of an animation under another ID, its events with their bytes only (no templates) and their groups.
    /// Used where the original must stay as it is (the survey's edits); a merge takes their objects over instead.
    /// </summary>
    public static TAE.Animation CloneAnimation(TAE.Animation animation, long id, bool bigEndian)
    {
        var clone = new TAE.Animation(id, animation.MiniHeader?.GetClone(), animation.AnimFileName);

        var groups = new Dictionary<TAE.EventGroup, TAE.EventGroup>();

        foreach (var group in animation.EventGroups)
        {
            var groupClone = group.GetClone();

            groups[group] = groupClone;
            clone.EventGroups.Add(groupClone);
        }

        foreach (var ev in animation.Events)
        {
            var eventClone = ev.GetClone(bigEndian);

            if (ev.Group != null && groups.TryGetValue(ev.Group, out var groupClone))
                eventClone.Group = groupClone;

            clone.Events.Add(eventClone);
        }

        return clone;
    }

    #endregion
}
