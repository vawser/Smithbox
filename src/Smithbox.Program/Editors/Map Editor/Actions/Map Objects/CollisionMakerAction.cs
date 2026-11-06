using Havok.Shared;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The shape swaps of one Collision Maker commit: each edited body gets its rebuilt compressed mesh. Undoing gives
/// the bodies their old shapes back. Both mark the maps' collision as modified, and call back so the displayed
/// meshes follow.
/// </summary>
public class CollisionMakerAction : ViewportAction
{
    public record ShapeSwap(MapContainer Map, string FileName, IHavokBody Body, object OldShape, object NewShape);

    private readonly List<ShapeSwap> Swaps;
    private readonly Action<IReadOnlyList<ShapeSwap>> OnChanged;

    public CollisionMakerAction(List<ShapeSwap> swaps, Action<IReadOnlyList<ShapeSwap>> onChanged)
    {
        Swaps = swaps;
        OnChanged = onChanged;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var swap in Swaps)
        {
            CollisionMakerShapes.SetShape(swap.Body, swap.NewShape);
            swap.Map?.MarkDirty(MapFileType.Collision);
        }

        OnChanged?.Invoke(Swaps);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var swap in Swaps)
        {
            CollisionMakerShapes.SetShape(swap.Body, swap.OldShape);
            swap.Map?.MarkDirty(MapFileType.Collision);
        }

        OnChanged?.Invoke(Swaps);

        return ActionEvent.NoEvent;
    }
}
