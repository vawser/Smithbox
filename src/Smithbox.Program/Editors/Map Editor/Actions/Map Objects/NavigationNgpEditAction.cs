using SoulsFormats;
using StudioCore.Editors.Common;
using System;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// An edit of the Navigation tool's NGP mode (DS2, DS2S): swaps the map's NGP for the copy the edit made (NgpEdit),
/// and back on undo. The map's save writes it (MapFileType.Navmesh).
/// </summary>
public class NavigationNgpEditAction : ViewportAction, IMapHistoryItem
{
    public string Describe() => LOC.Get("MAP_History_Navigation_Mesh");

    private readonly MapContainer Map;
    private readonly NgpBank Bank;
    private readonly NGP Before, After;
    private readonly Action Changed;

    /// <param name="changed">Called after each apply, e.g. to redraw the overlay.</param>
    public NavigationNgpEditAction(MapContainer map, NgpBank bank, NGP before, NGP after, Action changed)
    {
        Map = map;
        Bank = bank;
        Before = before;
        After = after;
        Changed = changed;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Bank.SetNgp(Map, After);
        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Bank.SetNgp(Map, Before);
        Changed?.Invoke();
        return ActionEvent.NoEvent;
    }
}
