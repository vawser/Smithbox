using StudioCore.Editors.Common;
using System;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// An edit of the Navigation tool that adds or removes a link (a pair of user edges) in a navmesh file. Edges after the
/// change are renumbered, so the action keeps the whole mesh state from before and after the edit and puts back one or
/// the other. Only user edges move, which no connection names (NavUserEdges.ConnectionsNameEdgesFrom is checked first),
/// so the NVA and NVC files need no change. Saved through the map's navmesh files (MapFileType.Navmesh).
/// </summary>
public class NavigationLinkEditAction : ViewportAction, IMapHistoryItem
{
    public string Describe() => LOC.Get("MAP_History_Navigation_Links");

    private readonly MapContainer Map;
    private readonly NavmeshFile File;
    private readonly NavMeshState Before;
    private readonly NavMeshState After;
    private readonly Action Changed;

    /// <param name="before">The mesh before the edit.</param>
    /// <param name="after">The mesh after the edit, which has already been made.</param>
    /// <param name="changed">Called after each apply, e.g. to redraw the overlay.</param>
    public NavigationLinkEditAction(MapContainer map, NavmeshFile file, NavMeshState before, NavMeshState after, Action changed)
    {
        Map = map;
        File = file;
        Before = before;
        After = after;
        Changed = changed;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        After.Restore(File.Mesh);
        Finish();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Before.Restore(File.Mesh);
        Finish();
        return ActionEvent.NoEvent;
    }

    private void Finish()
    {
        Map?.MarkDirty(MapFileType.Navmesh);
        Changed?.Invoke();
    }
}
