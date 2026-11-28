using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>A conflict the Full Mod merge has to resolve (one per conflicting merge item)</summary>
public sealed class TimeActFullModConflict
{
    /// <summary>"[prefix -> ]a000 -> 3000 -> event 1 @ 0.4 (timing)"</summary>
    public string InternalPath { get; init; }

    public string ExistingValue { get; init; }
    public string IncomingValue { get; init; }

    /// <summary>Both_Changed, Both_Added, Deleted_Changed or Groups_Changed</summary>
    public string Reason { get; init; }
}

/// <summary>
/// Time Act Merge for the Param Editor's Full Mod merge (Claude/Documentation/Time Act.md §5.4, D3): merges a
/// later mod's time act (a .tae binder entry) into the earlier result, three-way against the game's copy (two-way when
/// there is none). Their changes and additions are taken, each conflict is decided by the caller (strategy or saved
/// resolution), and what they removed is never removed: a later mod leaving an animation or event out isn't asking for
/// it to go. No editor dependencies.
/// </summary>
public static class TimeActFullModMerge
{
    /// <summary>A time act binder entry, by its name</summary>
    public static bool IsTimeActFile(string name) =>
        !string.IsNullOrWhiteSpace(name) && name.EndsWith(".tae", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Merge <paramref name="theirsBytes"/> into <paramref name="mineBytes"/>. <paramref name="takeTheirs"/> is asked
    /// for each conflict (true: theirs). <paramref name="merged"/> is mine's bytes when nothing was taken. False (with
    /// <paramref name="error"/>) when a side doesn't read as a time act, or the two differ in format or byte order, so
    /// the caller falls back to a whole-entry conflict.
    /// </summary>
    public static bool TryMerge(byte[] mineBytes, byte[] theirsBytes, byte[] vanillaBytes, string name, string internalPrefix,
        Func<TimeActFullModConflict, bool> takeTheirs, out byte[] merged, out int applied, out string error)
    {
        merged = null;
        applied = 0;
        error = null;

        try
        {
            var mine = new TimeActMergeTimeAct { BinderID = 0, Name = name, Bytes = mineBytes };
            var theirs = new TimeActMergeTimeAct { BinderID = 0, Name = name, Bytes = theirsBytes };
            var vanilla = vanillaBytes is { Length: > 0 }
                ? new TimeActMergeTimeAct { BinderID = 0, Name = name, Bytes = vanillaBytes }
                : null;

            if (mine.ReadFailed || theirs.ReadFailed)
            {
                error = $"{name}: not a time act";
                return false;
            }

            if (mine.TimeAct.Format != theirs.TimeAct.Format || mine.TimeAct.BigEndian != theirs.TimeAct.BigEndian)
            {
                error = $"{name}: another time act format or byte order";
                return false;
            }

            // A game copy that doesn't read or is of another format says nothing: two-way
            if (vanilla != null && (vanilla.ReadFailed || vanilla.TimeAct.Format != mine.TimeAct.Format))
                vanilla = null;

            var plan = TimeActMerge.BuildPlan(name, vanilla != null ? [vanilla] : null, [mine], [theirs],
                new TimeActMergeOptions { SourceIsWhole = true, ConflictMode = TimeActMergeConflictMode.KeepMine });

            if (plan.Problems.Count > 0)
            {
                error = string.Join("; ", plan.Problems.Select(p => $"{p.Code} {p.Subject}"));
                return false;
            }

            var chosen = new List<TimeActMergeItem>();

            foreach (var item in plan.Items)
            {
                switch (item.Kind)
                {
                    case TimeActMergeKind.Added:
                    case TimeActMergeKind.Changed:
                        chosen.Add(item);
                        break;

                    case TimeActMergeKind.Conflict:
                        var conflict = new TimeActFullModConflict
                        {
                            InternalPath = string.IsNullOrEmpty(internalPrefix) ? item.Path : $"{internalPrefix} -> {item.Path}",
                            ExistingValue = DescribeSide(item.Mine, item.Level, mine.TimeAct.BigEndian),
                            IncomingValue = DescribeSide(item.Theirs, item.Level, mine.TimeAct.BigEndian),
                            Reason = item.Reason
                        };

                        if (takeTheirs(conflict))
                        {
                            item.Choice = TimeActMergeChoice.Theirs;
                            chosen.Add(item);
                        }
                        break;

                    // RemovedByThem: never applied
                }
            }

            if (chosen.Count == 0)
            {
                merged = mineBytes;
                return true;
            }

            foreach (var item in chosen)
                item.Included = true;

            var result = TimeActMerge.Apply(mine.TimeAct, chosen);
            applied = result.Applied;
            merged = mine.TimeAct.Write();
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            return false;
        }
    }

    /// <summary>One side of an item as plain text for a conflict table: "-" when absent</summary>
    public static string DescribeSide(object side, TimeActMergeLevel level, bool bigEndian)
    {
        static string Time(float t) => t.ToString("0.###", CultureInfo.InvariantCulture);
        static string End(TAE.Event e) => e.IsInfiniteEnd ? "end" : Time(e.MemeEndTime);

        switch (side)
        {
            case null:
                return "-";

            case TAE t:
                return string.Format(CultureInfo.InvariantCulture, "ID {0}, bank {1}", t.ID, t.EventBank);

            case TAE.Animation a when level == TimeActMergeLevel.AnimationProperties:
                return a.MiniHeader switch
                {
                    TAE.Animation.AnimMiniHeader.ImportOtherAnim i => $"imports {i.ImportFromAnimID}",
                    TAE.Animation.AnimMiniHeader.Standard s => $"loop {(s.IsLoopByDefault ? 1 : 0)}, {a.AnimFileName}",
                    _ => a.AnimFileName ?? ""
                };

            case TAE.Animation a:
                return string.Format(CultureInfo.InvariantCulture, "{0} events", a.Events.Count);

            case TAE.Event e when level == TimeActMergeLevel.EventTiming:
                return $"{Time(e.StartTime)} - {End(e)}";

            case TAE.Event e when level == TimeActMergeLevel.EventContent:
                return $"u{e.Unk04} {Convert.ToHexString(e.GetParameterBytes(bigEndian)).TrimEnd('0')}";

            case TAE.Event e:
                return $"[{e.Type}] {Time(e.StartTime)} - {End(e)}";
        }

        return side.ToString();
    }
}
