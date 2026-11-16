using Andre.Formats;
using StudioCore.Formats;
using System.Runtime.CompilerServices;
using System.Threading;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Tracks which params have been modified since they were last loaded or saved.
/// The state is attached to the Param (or StructParam) itself, so an edit only needs the row or cell it changed,
/// and the state is discarded along with the param when a bank is reloaded.
/// <para>
/// Each dirty param also carries an edit version that goes up on every change. A save takes a snapshot of the
/// versions before it writes, and afterwards only clears params whose version hasn't moved, so an edit made while
/// the save is running keeps its dirty mark.
/// </para>
/// </summary>
public static class ParamDirtyTracker
{
    private sealed class DirtyState
    {
        public long Version;
    }

    private static readonly ConditionalWeakTable<object, DirtyState> DirtyParams = new();

    // Params changed since their diff caches (modified-row colours, "modified" search) were last refreshed.
    // Separate from the dirty state, which a save clears while the diffs still need refreshing. Weak keys, so a
    // closed project's params aren't kept alive by marks nobody will take.
    private static readonly ConditionalWeakTable<Param, object> DiffStaleParams = new();
    private static readonly object DiffStaleLock = new();

    public static void MarkDirty(Param param)
    {
        MarkDirtyInternal(param);

        if (param != null)
        {
            lock (DiffStaleLock)
            {
                DiffStaleParams.AddOrUpdate(param, null);
            }
        }
    }

    /// <summary>
    /// Returns the params changed since the last call, and clears the list. Used by the diff refresh, which then
    /// re-diffs only these params.
    /// </summary>
    public static List<Param> TakeDiffStaleParams()
    {
        lock (DiffStaleLock)
        {
            var result = new List<Param>();

            foreach (var entry in DiffStaleParams)
            {
                result.Add(entry.Key);
            }

            DiffStaleParams.Clear();

            return result;
        }
    }

    public static void MarkDirty(StructParam stayParam)
    {
        MarkDirtyInternal(stayParam);
    }

    public static void MarkDirty(Param.Row row)
    {
        if (row == null)
            return;

        MarkDirty(row.GetParentParam());
    }

    public static void MarkDirty(Param.Cell cell)
    {
        MarkDirty(cell.GetRow());
    }

    /// <summary>
    /// Mark the param that owns the changed object, if it is a param, row or cell
    /// </summary>
    public static void MarkDirty(object changed)
    {
        switch (changed)
        {
            case Param param:
                MarkDirty(param);
                break;

            case Param.Row row:
                MarkDirty(row);
                break;

            case Param.Cell cell:
                MarkDirty(cell);
                break;

            case StructParam stayParam:
                MarkDirty(stayParam);
                break;

            case StructCell stayCell:
                MarkDirty(stayCell.Parent);
                break;
        }
    }

    // Counts every change to a param, and unlike the dirty version is never reset by a save, so it can key caches
    // of anything computed from the param's contents
    private static readonly ConditionalWeakTable<object, DirtyState> EditCounts = new();

    private static void MarkDirtyInternal(object param)
    {
        if (param == null)
            return;

        var state = DirtyParams.GetValue(param, _ => new DirtyState());
        Interlocked.Increment(ref state.Version);

        var count = EditCounts.GetValue(param, _ => new DirtyState());
        Interlocked.Increment(ref count.Version);
    }

    /// <summary>
    /// How many changes the param (or stay param) has had since it was loaded. Goes up on every edit, undo and
    /// redo, and isn't reset by a save.
    /// </summary>
    public static long GetEditCount(object param)
    {
        if (param == null)
            return 0;

        return EditCounts.TryGetValue(param, out var state) ? Interlocked.Read(ref state.Version) : 0;
    }

    public static bool IsDirty(object param)
    {
        if (param == null)
            return false;

        return DirtyParams.TryGetValue(param, out _);
    }

    /// <summary>
    /// The current edit version of the param, or 0 if it is not dirty.
    /// </summary>
    public static long GetVersion(object param)
    {
        if (param == null)
            return 0;

        return DirtyParams.TryGetValue(param, out var state) ? Interlocked.Read(ref state.Version) : 0;
    }

    public static void ClearDirty(object param)
    {
        if (param == null)
            return;

        DirtyParams.Remove(param);
    }

    /// <summary>
    /// Clears the dirty mark only if the param hasn't been edited since <paramref name="version"/> was read.
    /// Returns false if the param was edited in the meantime and so stays dirty.
    /// </summary>
    public static bool ClearDirtyIfUnchanged(object param, long version)
    {
        if (param == null)
            return true;

        if (!DirtyParams.TryGetValue(param, out var state))
            return true;

        if (Interlocked.Read(ref state.Version) != version)
            return false;

        DirtyParams.Remove(param);

        // An edit could land between the version check and the removal. Re-mark it if so.
        if (Interlocked.Read(ref state.Version) != version)
        {
            DirtyParams.AddOrUpdate(param, state);
            return false;
        }

        return true;
    }
}
