using Andre.Formats;
using StudioCore.Formats;
using System.Runtime.CompilerServices;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Tracks which params have been modified since they were last loaded or saved.
/// The state is attached to the Param (or StructParam) itself, so an edit only needs the row or cell it changed,
/// and the state is discarded along with the param when a bank is reloaded.
/// </summary>
public static class ParamDirtyTracker
{
    private static readonly ConditionalWeakTable<object, object> DirtyParams = new();

    private static readonly object DirtyMarker = new();

    public static void MarkDirty(Param param)
    {
        if (param == null)
            return;

        DirtyParams.AddOrUpdate(param, DirtyMarker);
    }

    public static void MarkDirty(StructParam stayParam)
    {
        if (stayParam == null)
            return;

        DirtyParams.AddOrUpdate(stayParam, DirtyMarker);
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

    public static bool IsDirty(object param)
    {
        if (param == null)
            return false;

        return DirtyParams.TryGetValue(param, out _);
    }

    public static void ClearDirty(object param)
    {
        if (param == null)
            return;

        DirtyParams.Remove(param);
    }
}
