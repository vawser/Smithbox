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

    public static void MarkDirty(Param param)
    {
        MarkDirtyInternal(param);
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

    private static void MarkDirtyInternal(object param)
    {
        if (param == null)
            return;

        var state = DirtyParams.GetValue(param, _ => new DirtyState());
        Interlocked.Increment(ref state.Version);
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
