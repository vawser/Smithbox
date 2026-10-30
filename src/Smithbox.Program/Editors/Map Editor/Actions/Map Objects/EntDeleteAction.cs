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

        foreach (var (ent, placement) in Records)
        {
            MapContainer.MarkEntityDirty(placement.Map, ent);

            EntityPlacement.Detach(placement.Map, ent);

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

            foreach (var (ent, _) in Records)
            {
                sel.AddSelection(ent);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}
