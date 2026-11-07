using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A vertex edit of the Navigation tool's Edit tab: each edited navmesh file's document is swapped for a copy with the
/// moved vertices (see NavVertexEdit.Build). No face, edge or vertex is renumbered, so the NVA, NVC and ONAV files are
/// untouched; the map's save writes the navmesh binder (MapFileType.Navmesh).
/// </summary>
public class NavigationVertexEditAction : ViewportAction
{
    private readonly MapContainer Map;
    private readonly NavigationBank Bank;
    private readonly IReadOnlyList<NavFileChange> Files;
    private readonly Action Changed;

    /// <param name="files">The document each file had and gets.</param>
    /// <param name="changed">Called after each apply, e.g. to redraw the overlay.</param>
    public NavigationVertexEditAction(MapContainer map, NavigationBank bank, IReadOnlyList<NavFileChange> files, Action changed)
    {
        Map = map;
        Bank = bank;
        Files = files;
        Changed = changed;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var file in Files)
            Bank.SetNavmeshDocument(Map, file.Name, file.After);

        Finish();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var file in Files)
            Bank.SetNavmeshDocument(Map, file.Name, file.Before);

        Finish();
        return ActionEvent.NoEvent;
    }

    private void Finish()
    {
        Map.MarkDirty(MapFileType.Navmesh);
        Changed?.Invoke();
    }
}
