using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A model edit of the Navigation tool: a navmesh model removed (made a placeholder) or duplicated into a free
/// placeholder pair. The action keeps the NVA and NVC values from before and after, and the documents of the files it
/// replaced, and puts back one side or the other. The map's save writes the NVA, the navmesh binder (adding or
/// dropping files) and the changed NVC files.
/// </summary>
public class NavigationModelEditAction : ViewportAction
{
    private readonly MapContainer Map;
    private readonly NavigationBank Bank;
    private readonly NavModelState Before;
    private readonly NavModelState After;
    private readonly IReadOnlyList<NavFileChange> Files;
    private readonly NavRemapResult Changes;
    private readonly Action Changed;

    /// <param name="before">The NVA and NVC values before the edit.</param>
    /// <param name="after">The values after it, which has already been made to them (not yet to the files).</param>
    /// <param name="files">The documents each file had and gets; Execute puts the new ones in the bank.</param>
    /// <param name="changed">Called after each apply, e.g. to redraw the overlay.</param>
    public NavigationModelEditAction(MapContainer map, NavigationBank bank, NavModelState before, NavModelState after,
        IReadOnlyList<NavFileChange> files, NavRemapResult changes, Action changed)
    {
        Map = map;
        Bank = bank;
        Before = before;
        After = after;
        Files = files;
        Changes = changes;
        Changed = changed;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        After.Restore();
        foreach (var file in Files)
            Bank.SetNavmeshDocument(Map, file.Name, file.After);
        Finish();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Before.Restore();
        foreach (var file in Files)
            Bank.SetNavmeshDocument(Map, file.Name, file.Before);
        Finish();
        return ActionEvent.NoEvent;
    }

    private void Finish()
    {
        Map.MarkDirty(MapFileType.NVA);
        Map.MarkDirty(MapFileType.Navmesh);
        Bank.MarkEdited(Map, Changes);
        Changed?.Invoke();
    }
}
