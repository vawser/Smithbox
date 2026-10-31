using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class EntDeleteAction : ViewportAction
{
    private MapEditorView View;

    private readonly List<MsbEntity> Deletables = new();
    private readonly bool SetSelection;

    private readonly List<(Entity Entity, EntityPlacement Placement)> Records = new();

    public EntDeleteAction(MapEditorView view, List<MsbEntity> objects, bool setSelection)
    {
        View = view;
        Deletables.AddRange(objects);
        SetSelection = setSelection;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        Records.Clear();

        // Capture every placement before removing anything, so the indices are all relative to the original lists
        foreach (var obj in Deletables)
        {
            var map = View.Selection.GetMapContainerFromMapID(obj.MapID);

            Records.Add((obj, EntityPlacement.Capture(map, obj)));
        }

        // The light atlas entries of a deleted part go with it, unless they are being deleted themselves
        var captured = new HashSet<Entity>(Deletables);

        foreach (var obj in Deletables)
        {
            var map = View.Selection.GetMapContainerFromMapID(obj.MapID);

            if (map == null)
                continue;

            foreach (var entry in map.LightAtlasResolver.AutomaticDeletion(obj))
            {
                if (captured.Add(entry))
                {
                    Records.Add((entry, EntityPlacement.Capture(map, entry)));
                }
            }
        }

        foreach (var (ent, placement) in Records)
        {
            MapContainer.MarkEntityDirty(placement.Map, ent);

            EntityPlacement.Detach(placement.Map, ent);

            (ent as MsbEntity)?.ClearGrass();
            (ent as MsbEntity)?.ClearEdgePoints();

            ent.RenderSceneMesh?.Dispose();
            ent.RenderSceneMesh = null;
        }

        if (SetSelection)
            View.Universe.View.ViewportSelection.ClearSelection();

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        EntityPlacement.RestoreAll(Records);

        foreach (var (ent, placement) in Records)
        {
            MapContainer.MarkEntityDirty(placement.Map, ent);

            if (ent is MsbEntity msbEnt)
            {
                msbEnt.AssignDrawable();
            }
        }

        if (SetSelection)
        {
            var sel = View.Universe.View.ViewportSelection;
            sel.ClearSelection();

            foreach (var ent in Deletables)
            {
                sel.AddSelection(ent);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
