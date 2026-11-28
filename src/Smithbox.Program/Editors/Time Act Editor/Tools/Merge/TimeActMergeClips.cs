using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// The HKX clip half of Time Act Merge (Claude/Documentation/Time Act.md §5.2, D2): their clips against
/// mine and the game's, whole files by full animation ID, three-way like the rest. Clips are never removed: an
/// anibnd that leaves one out is usually split differently, not deleting it. Run on the worker.
/// </summary>
public static class TimeActMergeClips
{
    /// <summary>One side's copy of a clip: stored in a binder, or imported and not saved yet (mine only)</summary>
    private sealed class Side
    {
        public BinderFileHeader Header;
        public Func<Memory<byte>> ReadStored;
        public Func<byte[]> Read;
        public byte[] Pending;

        private byte[] _bytes;
        public byte[] Bytes => _bytes ??= Pending ?? Read();
    }

    /// <summary>
    /// Clip items for one source. <paramref name="minePending"/> is the source's imports as they were when the analysis
    /// started (taken on the UI thread); each item's Mine is mine's import of that clip then (or null), for the stale
    /// check at Apply.
    /// </summary>
    public static (List<TimeActMergeItem> Items, TimeActMergeProblem Problem) Plan(TimeActContainerWrapper mine,
        IReadOnlyDictionary<long, TimeActPendingAnimationFile> minePending, TimeActContainerWrapper theirs,
        TimeActContainerWrapper vanilla, string displayName)
    {
        var items = new List<TimeActMergeItem>();

        // Action IDs have no clips
        if (theirs == null || mine.IsActionSource)
            return (items, null);

        TimeActAnimationFiles theirFiles = null, mineFiles = null, vanillaFiles = null;

        try
        {
            theirFiles = TimeActAnimationFiles.Open(theirs, false);

            if (!theirFiles.HasAnimationFiles)
                return (items, null);

            mineFiles = TimeActAnimationFiles.Open(mine, false);
            vanillaFiles = vanilla != null ? TimeActAnimationFiles.Open(vanilla, false) : null;

            foreach (var id in theirFiles.StoredIDs.OrderBy(e => e))
            {
                var o = Stored(theirFiles, id);
                minePending.TryGetValue(id, out var pending);
                var m = pending != null ? new Side { Pending = pending.Bytes } : Stored(mineFiles, id);
                var v = vanillaFiles != null ? Stored(vanillaFiles, id) : null;

                if (m != null && Same(o, m))
                    continue;

                if (v != null && Same(o, v))
                    continue;

                TimeActMergeKind kind;
                string reason = null;

                if (m == null)
                {
                    // Mine has no such clip: they added it, or they changed one I removed
                    kind = v == null ? TimeActMergeKind.Added : TimeActMergeKind.Conflict;
                    reason = v == null ? null : "Deleted_Changed";
                }
                else if (v != null && Same(m, v))
                {
                    kind = TimeActMergeKind.Changed;
                }
                else
                {
                    kind = TimeActMergeKind.Conflict;
                    reason = v == null ? "Both_Added" : "Both_Changed";
                }

                theirFiles.TryGetStored(id, out _, out var theirBinder, out _, out _);

                items.Add(new TimeActMergeItem
                {
                    Level = TimeActMergeLevel.Clip,
                    Kind = kind,
                    Reason = reason,
                    BinderID = -1,
                    TimeActName = displayName,
                    AnimationID = id,
                    Mine = pending,
                    Theirs = new TimeActMergeClip(id, o.Bytes, BinderPaths(mineFiles, pending, id, theirBinder), theirBinder)
                });
            }

            return (items, null);
        }
        catch (Exception e)
        {
            return (null, new TimeActMergeProblem("Clips_Read_Failed", displayName, e.Message));
        }
        finally
        {
            theirFiles?.Dispose();
            mineFiles?.Dispose();
            vanillaFiles?.Dispose();
        }
    }

    private static Side Stored(TimeActAnimationFiles files, long id) =>
        files.TryGetStored(id, out var header, out _, out var readStored, out var read)
            ? new Side { Header = header, ReadStored = readStored, Read = read }
            : null;

    /// <summary>
    /// Whether two copies are the same file: by their stored bytes when both are stored the same way (no decompressing),
    /// else by their contents
    /// </summary>
    private static bool Same(Side a, Side b)
    {
        if (a.Header != null && b.Header != null)
        {
            if (a.Header.UncompressedSize > 0 && b.Header.UncompressedSize > 0 &&
                a.Header.UncompressedSize != b.Header.UncompressedSize)
                return false;

            if (a.Header.Flags == b.Header.Flags && a.Header.CompressedSize == b.Header.CompressedSize &&
                a.ReadStored().Span.SequenceEqual(b.ReadStored().Span))
                return true;
        }

        return a.Bytes.AsSpan().SequenceEqual(b.Bytes);
    }

    /// <summary>
    /// Where their clip goes in mine: where mine has it (or an import of mine would), else mine's binder of the same
    /// file name as theirs (a split anibnd), else with its category as an import goes
    /// </summary>
    private static IReadOnlyList<string> BinderPaths(TimeActAnimationFiles mine, TimeActPendingAnimationFile pending,
        long id, string theirBinder)
    {
        if (pending != null)
            return pending.BinderPaths.ToList();

        if (mine.IsStored(id))
            return mine.GetImportBinderPaths(id);

        var same = theirBinder != null ? mine.FindBinderPath(System.IO.Path.GetFileName(theirBinder)) : null;

        return same != null ? [same] : mine.GetImportBinderPaths(id);
    }
}
