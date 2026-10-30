using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;


public class EntMoveAction : ViewportAction
{
    private MapEditorView View;

    private readonly List<MsbEntity> Moveables = new();
    private readonly bool SetSelection;
    private readonly Entity TargetBTL;
    private readonly MapContainer TargetMap;

    // Original placement and target map for each index of Moveables that was moved on the first execute
    private readonly Dictionary<int, (EntityPlacement Source, MapContainer Target)> Moves = new();

    private LightAtlasEdit AtlasEdit;

    public EntMoveAction(MapEditorView view, List<MsbEntity> objects, bool setSelection,
        MapContainer targetMap = null, Entity targetBTL = null)
    {
        View = view;
        Moveables.AddRange(objects);
        SetSelection = setSelection;
        TargetMap = targetMap;
        TargetBTL = targetBTL;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = View.Universe;

        var movesCached = Moves.Count > 0;

        for (var i = 0; i < Moveables.Count(); i++)
        {
            var ent = Moveables[i];

            if (ent.MapID == null)
            {
                Smithbox.LogError(this,
                    LOC.Get("MAP_Actions_Duplicate_Failed", ent.Name));
                continue;
            }

            (EntityPlacement Source, MapContainer Target) move;

            if (movesCached)
            {
                // Only redo the moves that were made on the first execute
                if (!Moves.TryGetValue(i, out move))
                    continue;
            }
            else
            {
                MapContainer sourceMap = View.Selection.GetMapContainerFromMapID(ent.MapID);

                // Moving within the same map if no target is given
                MapContainer targetMap = TargetMap != null
                    ? View.Selection.GetMapContainerFromMapID(TargetMap.Name)
                    : sourceMap;

                if (sourceMap == null || targetMap == null)
                    continue;

                move = (EntityPlacement.Capture(sourceMap, ent), targetMap);
                Moves[i] = move;
            }

            var source = move.Source;

            // Remove from source map and parent, then add to target map
            EntityPlacement.Detach(source.Map, ent);
            move.Target.Objects.Add(ent);

            // Update MapID if moving to a different map
            if (TargetMap != null && source.Map != move.Target)
            {
                ent.ContainingMap = move.Target;
            }

            // Set new parent
            if (TargetBTL != null && ent.WrappedObject is BTL.Light)
            {
                TargetBTL.AddChild(ent);
            }
            else if (TargetMap != null)
            {
                // Moving to a targeted map, update parent.
                if (TargetMap.MapOffsetNode != null)
                {
                    TargetMap.MapOffsetNode.AddChild(ent);
                }
                else
                {
                    TargetMap.RootObject.AddChild(ent);
                }
            }
            else
            {
                // Moving within the same map, restore to original parent or root
                if (source.Parent != null)
                {
                    source.Parent.AddChild(ent);
                }
                else
                {
                    source.Map.RootObject.AddChild(ent);
                }
            }

            // Update render model
            ent.AssignDrawable();
            ent.UpdateRenderModel();

            if (ent.RenderSceneMesh != null)
            {
                ent.RenderSceneMesh.RenderSelectionOutline = true;
                ent.RenderSceneMesh.SetSelectable(ent);
            }

            // Mark maps as having unsaved changes
            MapContainer.MarkEntityDirty(source.Map, ent);
            MapContainer.MarkEntityDirty(move.Target, ent);
        }

        if (AtlasEdit == null)
        {
            AtlasEdit = LightAtlasEdit.ForMovedParts(Moves
                .Where(e => e.Value.Source.Map != e.Value.Target)
                .Select(e => (Moveables[e.Key], e.Value.Source.Map, e.Value.Target)));
        }
        else
        {
            AtlasEdit.Redo();
        }

        if (SetSelection)
        {
            universe.View.ViewportSelection.ClearSelection();
            foreach (MsbEntity m in Moveables)
            {
                universe.View.ViewportSelection.AddSelection(m);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = View.Universe;

        AtlasEdit?.Undo();

        var restores = new List<(Entity Entity, EntityPlacement Placement)>();

        foreach (var (i, move) in Moves)
        {
            var ent = Moveables[i];

            MapContainer.MarkEntityDirty(move.Source.Map, ent);
            MapContainer.MarkEntityDirty(move.Target, ent);

            // Remove from current location
            EntityPlacement.Detach(move.Target, ent);

            // Restore MapID if it was changed
            if (move.Source.Map != move.Target)
            {
                ent.ContainingMap = move.Source.Map;
            }

            restores.Add((ent, move.Source));
        }

        // Restore original map index and parent index
        EntityPlacement.RestoreAll(restores);

        // Entities that had no parent were at the map root
        foreach (var (ent, placement) in restores)
        {
            if (placement.Parent == null)
            {
                placement.Map.RootObject.AddChild(ent);
            }
        }

        if (SetSelection)
        {
            universe.View.ViewportSelection.ClearSelection();
            foreach (MsbEntity m in Moveables)
            {
                universe.View.ViewportSelection.AddSelection(m);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
