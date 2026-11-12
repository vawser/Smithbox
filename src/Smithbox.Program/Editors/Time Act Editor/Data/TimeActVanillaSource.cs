using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// A time act compared with the game's version, found by its binder ID
/// </summary>
public class TimeActEntryDiff
{
    public TimeActVanillaStatus Status;

    public TimeActEntry Vanilla;

    /// <summary>
    /// Whether the animations were compared, rather than only the stored bytes. Only then are the counts known.
    /// </summary>
    public bool Detailed;

    public bool HeaderChanged;
    public int ModifiedAnimations;
    public int AddedAnimations;

    /// <summary>
    /// The game's animations that this time act no longer has
    /// </summary>
    public List<TAE.Animation> RemovedAnimations = new();
}

/// <summary>
/// An animation compared with the game's version, found by its ID in the same time act
/// </summary>
public class TimeActAnimationDiff
{
    public TimeActVanillaStatus Status;

    public TAE.Animation Vanilla;

    public bool MiniHeaderChanged;
    public bool FileNameChanged;
    public bool GroupsChanged;
    public bool EventsChanged;

    /// <summary>
    /// The events matched with the game's, worked out when first asked for
    /// </summary>
    public TimeActEventMatch Events;
}

/// <summary>
/// Compares a source with the game's own copy of it, read in the background the first time it is needed, as an
/// anibnd can hold a lot of animation files. The results are kept until the next edit to any time act.
/// Only used on the main thread.
/// </summary>
public class TimeActVanillaSource
{
    private readonly TimeActContainerWrapper Source;

    private Task<Dictionary<int, TimeActEntry>> ReadTask;

    // The game's animations of each of its time acts, by ID
    private readonly Dictionary<TimeActEntry, Dictionary<long, TAE.Animation>> VanillaAnimations = new();

    private int CacheVersion = -1;
    private readonly Dictionary<TimeActEntry, TimeActEntryDiff> EntryDiffs = new();
    private readonly Dictionary<TimeActEntry, TimeActEntryDiff> DetailedEntryDiffs = new();
    private readonly Dictionary<TAE.Animation, TimeActAnimationDiff> AnimationDiffs = new();

    public TimeActVanillaSource(TimeActContainerWrapper source)
    {
        Source = source;
    }

    /// <summary>
    /// Whether the game's version is still being read
    /// </summary>
    public bool IsReading => ReadTask is { IsCompleted: false };

    /// <summary>
    /// Whether the game has this source at all. False until it has been read.
    /// </summary>
    public bool GameHasSource { get; private set; }

    /// <summary>
    /// The game's time acts by binder ID, or null while they are being read or if they couldn't be
    /// </summary>
    private Dictionary<int, TimeActEntry> GetVanillaTimeActs()
    {
        ReadTask ??= Task.Run(ReadVanilla);

        return ReadTask.IsCompletedSuccessfully ? ReadTask.Result : null;
    }

    private Dictionary<int, TimeActEntry> ReadVanilla()
    {
        var timeActs = new Dictionary<int, TimeActEntry>();

        try
        {
            var vanilla = Source.CreateVanillaCopy();

            if (!vanilla.TargetFS.FileExists(Source.SourcePath))
                return timeActs;

            foreach (var entry in vanilla.ReadTimeActs())
            {
                timeActs.TryAdd(entry.BinderID, entry);
            }

            GameHasSource = true;
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_Vanilla_Read_Failed", Source.Name), e);
            return null;
        }

        return timeActs;
    }

    private void CheckVersion()
    {
        if (CacheVersion == TimeActEntry.EditVersion)
            return;

        CacheVersion = TimeActEntry.EditVersion;

        EntryDiffs.Clear();
        DetailedEntryDiffs.Clear();
        AnimationDiffs.Clear();
    }

    #region Time acts
    /// <summary>
    /// How the time act compares with the game's. A time act that hasn't been read is only compared by its stored
    /// bytes. With detail, the animations are compared and counted, which reads the time act.
    /// </summary>
    public TimeActEntryDiff GetEntryDiff(TimeActEntry entry, bool detailed = false)
    {
        CheckVersion();

        var cache = detailed ? DetailedEntryDiffs : EntryDiffs;

        if (cache.TryGetValue(entry, out var cached))
            return cached;

        var vanillaTimeActs = GetVanillaTimeActs();

        // Not kept, so it is worked out once the game's version has been read
        if (vanillaTimeActs == null)
            return new TimeActEntryDiff { Status = TimeActVanillaStatus.Unknown };

        var diff = CompareEntry(entry, vanillaTimeActs, detailed);

        cache[entry] = diff;

        return diff;
    }

    private TimeActEntryDiff CompareEntry(TimeActEntry entry, Dictionary<int, TimeActEntry> vanillaTimeActs, bool detailed)
    {
        var diff = new TimeActEntryDiff();

        if (!vanillaTimeActs.TryGetValue(entry.BinderID, out var vanilla))
        {
            diff.Status = TimeActVanillaStatus.Added;
            return diff;
        }

        diff.Vanilla = vanilla;

        // The usual case for a time act that hasn't been changed: the project doesn't have its own copy
        if (!entry.IsDirty && entry.StoredBytesEqual(vanilla))
        {
            diff.Status = TimeActVanillaStatus.Same;
            diff.Detailed = true;
            return diff;
        }

        if (!detailed && entry.TimeAct == null)
        {
            diff.Status = TimeActVanillaStatus.Modified;
            return diff;
        }

        entry.Load();
        vanilla.Load();

        if (entry.TimeAct == null || vanilla.TimeAct == null)
        {
            diff.Status = TimeActVanillaStatus.Modified;
            return diff;
        }

        diff.Detailed = true;
        diff.HeaderChanged = !TimeActCompare.HeadersEqual(entry.TimeAct, vanilla.TimeAct);

        var vanillaAnimations = GetVanillaAnimations(vanilla);
        var bigEndian = entry.TimeAct.BigEndian;
        var ids = new HashSet<long>();

        foreach (var animation in entry.TimeAct.Animations)
        {
            ids.Add(animation.ID);

            if (!vanillaAnimations.TryGetValue(animation.ID, out var vanillaAnimation))
            {
                diff.AddedAnimations++;
            }
            else if (!TimeActCompare.AnimationsEqual(animation, vanillaAnimation, bigEndian))
            {
                diff.ModifiedAnimations++;
            }

            // The list only needs to know whether anything differs
            if (!detailed && (diff.AddedAnimations > 0 || diff.ModifiedAnimations > 0))
            {
                diff.Detailed = false;
                diff.Status = TimeActVanillaStatus.Modified;
                return diff;
            }
        }

        diff.RemovedAnimations = vanilla.TimeAct.Animations.Where(e => !ids.Contains(e.ID)).ToList();

        var changed = diff.HeaderChanged || diff.AddedAnimations > 0 || diff.ModifiedAnimations > 0 ||
            diff.RemovedAnimations.Count > 0;

        diff.Status = changed ? TimeActVanillaStatus.Modified : TimeActVanillaStatus.Same;

        return diff;
    }

    private Dictionary<long, TAE.Animation> GetVanillaAnimations(TimeActEntry vanilla)
    {
        if (VanillaAnimations.TryGetValue(vanilla, out var animations))
            return animations;

        animations = new Dictionary<long, TAE.Animation>();

        foreach (var animation in vanilla.TimeAct.Animations)
        {
            animations.TryAdd(animation.ID, animation);
        }

        VanillaAnimations[vanilla] = animations;

        return animations;
    }
    #endregion

    #region Animations
    /// <summary>
    /// How an animation of the time act compares with the game's animation of the same ID
    /// </summary>
    public TimeActAnimationDiff GetAnimationDiff(TimeActEntry entry, TAE.Animation animation)
    {
        CheckVersion();

        if (AnimationDiffs.TryGetValue(animation, out var cached))
            return cached;

        var entryDiff = GetEntryDiff(entry);

        var diff = new TimeActAnimationDiff();

        switch (entryDiff.Status)
        {
            case TimeActVanillaStatus.Unknown:
                diff.Status = TimeActVanillaStatus.Unknown;
                return diff;

            case TimeActVanillaStatus.Added:
                diff.Status = TimeActVanillaStatus.Added;
                break;

            default:
                entryDiff.Vanilla.Load();

                if (entryDiff.Vanilla.TimeAct == null || entry.TimeAct == null)
                {
                    diff.Status = TimeActVanillaStatus.Unknown;
                    return diff;
                }

                if (!GetVanillaAnimations(entryDiff.Vanilla).TryGetValue(animation.ID, out diff.Vanilla))
                {
                    diff.Status = TimeActVanillaStatus.Added;
                    break;
                }

                // A time act the same as the game's has nothing to compare
                if (entryDiff.Status == TimeActVanillaStatus.Same)
                {
                    diff.Status = TimeActVanillaStatus.Same;
                    break;
                }

                CompareAnimation(animation, diff, entry.TimeAct.BigEndian);
                break;
        }

        AnimationDiffs[animation] = diff;

        return diff;
    }

    private static void CompareAnimation(TAE.Animation animation, TimeActAnimationDiff diff, bool bigEndian)
    {
        var vanilla = diff.Vanilla;

        diff.MiniHeaderChanged = !TimeActCompare.MiniHeadersEqual(animation.MiniHeader, vanilla.MiniHeader);
        diff.FileNameChanged = (animation.AnimFileName ?? "") != (vanilla.AnimFileName ?? "");
        diff.EventsChanged = !TimeActCompare.EventListsEqual(animation, vanilla, bigEndian);
        diff.GroupsChanged = !diff.EventsChanged && !TimeActCompare.GroupsEqual(animation, vanilla);

        diff.Status = diff.MiniHeaderChanged || diff.FileNameChanged || diff.EventsChanged || diff.GroupsChanged
            ? TimeActVanillaStatus.Modified
            : TimeActVanillaStatus.Same;
    }
    #endregion

    #region Events
    /// <summary>
    /// The events of the animation matched with the game's
    /// </summary>
    public TimeActEventMatch GetEventMatch(TimeActEntry entry, TAE.Animation animation)
    {
        var diff = GetAnimationDiff(entry, animation);

        if (diff.Status == TimeActVanillaStatus.Unknown)
            return null;

        if (diff.Events != null)
            return diff.Events;

        diff.Events = TimeActCompare.MatchEvents(animation.Events, diff.Vanilla?.Events ?? [], entry.TimeAct.BigEndian);

        return diff.Events;
    }
    #endregion
}
