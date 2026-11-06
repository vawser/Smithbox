using Havok.Shared;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The shape swaps of one Collision Editor commit: each edited body gets its rebuilt compressed mesh. Undoing gives
/// the bodies their old shapes back. Both mark the maps' collision as modified, and call back so the displayed
/// meshes follow. In Sekiro, the WALL and NOJUMPWALL marks of rebuilt low collisions move to the new shape keys with
/// them.
/// </summary>
public class CollisionEditorAction : ViewportAction
{
    public record ShapeSwap(MapContainer Map, string FileName, IHavokBody Body, object OldShape, object NewShape);

    /// <summary>The wall marks of a collision before and after its triangles were given new keys.</summary>
    public record WallRemap(MapContainer Map, string MapId, string ModelName, WallBank.WallMarks Before, WallBank.WallMarks After);

    private readonly List<ShapeSwap> Swaps;
    private readonly List<WallRemap> Remaps;
    private readonly WallBank WallBank;
    private readonly Action<IReadOnlyList<ShapeSwap>> OnChanged;

    public CollisionEditorAction(List<ShapeSwap> swaps, Action<IReadOnlyList<ShapeSwap>> onChanged,
        WallBank wallBank = null, List<WallRemap> remaps = null)
    {
        Swaps = swaps;
        OnChanged = onChanged;
        WallBank = wallBank;
        Remaps = remaps ?? new List<WallRemap>();
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var swap in Swaps)
        {
            CollisionEditorShapes.SetShape(swap.Body, swap.NewShape);
            swap.Map?.MarkDirty(MapFileType.Collision);
        }

        foreach (var remap in Remaps)
        {
            WallBank?.SetMarks(remap.MapId, remap.ModelName, remap.After);
            remap.Map?.MarkDirty(MapFileType.Wall);
        }

        OnChanged?.Invoke(Swaps);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var swap in Swaps)
        {
            CollisionEditorShapes.SetShape(swap.Body, swap.OldShape);
            swap.Map?.MarkDirty(MapFileType.Collision);
        }

        foreach (var remap in Remaps)
        {
            WallBank?.SetMarks(remap.MapId, remap.ModelName, remap.Before);
            remap.Map?.MarkDirty(MapFileType.Wall);
        }

        OnChanged?.Invoke(Swaps);

        return ActionEvent.NoEvent;
    }
}
