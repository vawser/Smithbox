using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Moves the selected parts, regions and events within the list of objects of their type, which decides their
/// order in the saved MSB. Several can be moved at once, also across types and maps: within each type the
/// selected objects move together as a block, keeping their relative order, and the others keep theirs.
/// </summary>
public class EntReorderAction : ViewportAction
{
    private MapEditorView View;

    private List<MsbEntity> selection = new();

    // The object order of each map changed by the last execute, for undo
    private readonly Dictionary<MapContainer, List<Entity>> storedObjectOrders = new();

    private TreeObjectOrderMovementType MoveSelectionDir;

    public EntReorderAction(MapEditorView view, List<MsbEntity> objects, TreeObjectOrderMovementType moveDir)
    {
        View = view;
        selection.AddRange(objects);

        MoveSelectionDir = moveDir;
    }

    /// <summary>
    /// Whether the object's position in its map's list can be changed by this action
    /// </summary>
    public static bool IsReorderable(Entity ent)
    {
        return ent?.WrappedObject is IMsbPart or IMsbRegion or IMsbEvent;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        // Re-captured on every execute (including redo), so it always reflects the order before this action
        storedObjectOrders.Clear();

        var reorderable = selection.Where(IsReorderable).ToList();

        if (reorderable.Count == 0)
        {
            Smithbox.LogError(this, LOC.Get("MAP_Actions_Reorder_Nothing_Reorderable"));
            return ActionEvent.NoEvent;
        }

        foreach (var mapGroup in reorderable.GroupBy(e => View.Selection.GetMapContainerFromMapID(e.MapID)))
        {
            var map = mapGroup.Key;

            if (map == null)
                continue;

            var objects = map.Objects;
            var snapshot = objects.ToList();
            var changed = false;

            foreach (var typeGroup in mapGroup.GroupBy(e => e.WrappedObject.GetType()))
            {
                var selected = typeGroup.Cast<Entity>().ToHashSet();
                var type = typeGroup.Key;

                // The positions of every object of the type, which need not be contiguous in the list
                // (created, pasted and moved-in objects are appended)
                var positions = new List<int>();
                for (var i = 0; i < objects.Count; i++)
                {
                    if (objects[i].WrappedObject?.GetType() == type)
                    {
                        positions.Add(i);
                    }
                }

                var sequence = positions.Select(i => objects[i]).ToList();
                var reordered = Reorder(sequence, selected, MoveSelectionDir);

                // Written back into the same positions, so objects of other types don't move
                for (var k = 0; k < positions.Count; k++)
                {
                    if (objects[positions[k]] != reordered[k])
                    {
                        objects[positions[k]] = reordered[k];
                        changed = true;
                    }
                }
            }

            if (changed)
            {
                storedObjectOrders[map] = snapshot;
            }
        }

        if (storedObjectOrders.Count == 0)
        {
            // Already at the requested end
            return ActionEvent.NoEvent;
        }

        MapContainer.MarkEntitiesDirty(reorderable);

        return ActionEvent.ObjectAddedRemoved;
    }

    /// <summary>
    /// The sequence with the selected objects moved as a block, the others keeping their order
    /// </summary>
    private static List<Entity> Reorder(List<Entity> sequence, HashSet<Entity> selected, TreeObjectOrderMovementType direction)
    {
        var result = sequence.ToList();

        switch (direction)
        {
            // Each selected object passes the unselected one before it; one at the top stops the block
            case TreeObjectOrderMovementType.Up:
                for (var i = 1; i < result.Count; i++)
                {
                    if (selected.Contains(result[i]) && !selected.Contains(result[i - 1]))
                    {
                        (result[i - 1], result[i]) = (result[i], result[i - 1]);
                    }
                }
                break;

            case TreeObjectOrderMovementType.Down:
                for (var i = result.Count - 2; i >= 0; i--)
                {
                    if (selected.Contains(result[i]) && !selected.Contains(result[i + 1]))
                    {
                        (result[i + 1], result[i]) = (result[i], result[i + 1]);
                    }
                }
                break;

            case TreeObjectOrderMovementType.Top:
                result = sequence.Where(selected.Contains).Concat(sequence.Where(e => !selected.Contains(e))).ToList();
                break;

            case TreeObjectOrderMovementType.Bottom:
                result = sequence.Where(e => !selected.Contains(e)).Concat(sequence.Where(selected.Contains)).ToList();
                break;
        }

        return result;
    }

    public override ActionEvent Undo()
    {
        // Nothing was captured if execute changed nothing
        if (storedObjectOrders.Count == 0)
            return ActionEvent.NoEvent;

        foreach (var (map, order) in storedObjectOrders)
        {
            // Restore into the existing list rather than aliasing it to the stored snapshot
            map.Objects.Clear();
            map.Objects.AddRange(order);
        }

        MapContainer.MarkEntitiesDirty(selection.Where(IsReorderable));

        return ActionEvent.ObjectAddedRemoved;
    }
}
