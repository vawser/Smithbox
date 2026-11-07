using StudioCore.Editors.Common;
using System;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A re-stitch of the Navigation tool: connections removed from and added to a map's NVA and its NVC files. The action
/// keeps the connectors from before and after the re-stitch and puts back one or the other. The changed files are
/// marked, so the map's save writes them (MapFileType.NVA and NVC).
/// </summary>
public class NavigationStitchAction : ViewportAction
{
    private readonly MapContainer Map;
    private readonly NavigationBank Bank;
    private readonly NavConnectionState Before;
    private readonly NavConnectionState After;
    private readonly NavRemapResult Changes;
    private readonly Action Changed;
    private readonly NavGateState GatesBefore;
    private readonly NavGateState GatesAfter;

    /// <param name="before">The connectors before the re-stitch.</param>
    /// <param name="after">The connectors after it, which has already been made.</param>
    /// <param name="changes">What the re-stitch changed, so undo and redo mark the same files.</param>
    /// <param name="changed">Called after each apply, e.g. to redraw the overlay.</param>
    /// <param name="gatesBefore">The gate nodes before, when the re-stitch rebuilt gates; else null.</param>
    /// <param name="gatesAfter">The gate nodes after, when the re-stitch rebuilt gates; else null.</param>
    public NavigationStitchAction(MapContainer map, NavigationBank bank, NavConnectionState before, NavConnectionState after, NavRemapResult changes, Action changed,
        NavGateState gatesBefore = null, NavGateState gatesAfter = null)
    {
        Map = map;
        Bank = bank;
        Before = before;
        After = after;
        Changes = changes;
        Changed = changed;
        GatesBefore = gatesBefore;
        GatesAfter = gatesAfter;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        After.Restore();
        GatesAfter?.Restore();
        Finish();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Before.Restore();
        GatesBefore?.Restore();
        Finish();
        return ActionEvent.NoEvent;
    }

    private void Finish()
    {
        if (Map != null)
            Bank?.MarkEdited(Map, Changes);
        Changed?.Invoke();
    }
}
