using StudioCore.Editors.Common;
using System;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A gate edit of the Navigation tool (rebuild, move, cost, add, remove): the NVA's gate nodes from before and after,
/// of which one or the other is put back. The NVA is marked, so the map's save writes it.
/// </summary>
public class NavigationGateEditAction : ViewportAction, IMapHistoryItem
{
    public string Describe() => LOC.Get("MAP_History_Navigation_Gates");

    private readonly MapContainer Map;
    private readonly NavGateState Before;
    private readonly NavGateState After;
    private readonly Action Changed;

    /// <param name="before">The gates before the edit.</param>
    /// <param name="after">The gates after it, which has already been made.</param>
    /// <param name="changed">Called after each apply, e.g. to redraw the overlay.</param>
    public NavigationGateEditAction(MapContainer map, NavGateState before, NavGateState after, Action changed)
    {
        Map = map;
        Before = before;
        After = after;
        Changed = changed;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        After.Restore();
        Finish();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Before.Restore();
        Finish();
        return ActionEvent.NoEvent;
    }

    private void Finish()
    {
        Map?.MarkDirty(MapFileType.NVA);
        Changed?.Invoke();
    }
}
