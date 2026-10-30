using HKX2;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;


public class EntAddAction : ViewportAction
{
    private MapEditorView View;

    private readonly List<MsbEntity> Added = new();
    private readonly List<ObjectContainer> AddedMaps = new();

    private readonly MapContainer Map;
    private readonly Entity Parent;

    private LightAtlasEdit AtlasEdit;

    public EntAddAction(MapEditorView view, MapContainer map, List<MsbEntity> objects, Entity parent)
    {
        View = view;
        Map = map;
        Parent = parent;

        Added.AddRange(objects);
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        // Rebuilt on every execute so it stays index-aligned with Added on redo
        AddedMaps.Clear();

        for (var i = 0; i < Added.Count(); i++)
        {
            if (Map != null)
            {
                Map.Objects.Add(Added[i]);
                Parent.AddChild(Added[i]);

                if (Added[i] is MsbEntity msbEnt)
                {
                    msbEnt.AssignDrawable();
                }
                Added[i].UpdateRenderModel();

                if (Added[i].RenderSceneMesh != null)
                {
                    Added[i].RenderSceneMesh.RenderSelectionOutline = true;
                    Added[i].RenderSceneMesh.SetSelectable(Added[i]);
                    Added[i].RenderSceneMesh.AutoRegister = true;
                    Added[i].RenderSceneMesh.Register();
                }

                AddedMaps.Add(Map);

                MapContainer.MarkEntityDirty(Map, Added[i]);

                // Prefab-specific
                if (CFG.Current.Prefab_ApplyUniqueInstanceID)
                {
                    MapEditorActionHelper.SetUniqueInstanceID(View, Added[i], Map);
                }
                if (CFG.Current.Prefab_ApplyUniqueEntityID)
                {
                    MapEditorActionHelper.SetUniqueEntityID(View, Added[i], Map);
                }
                if (CFG.Current.Prefab_ApplySpecificEntityGroupID)
                {
                    MapEditorActionHelper.SetSpecificEntityGroupID(View, Added[i], Map);
                }
            }
            else
            {
                AddedMaps.Add(null);
            }
        }

        if (AtlasEdit == null)
        {
            AtlasEdit = LightAtlasEdit.ForNewParts(Added, AddedMaps);
        }
        else
        {
            AtlasEdit.Redo();
        }

        View.ViewportSelection.ClearSelection();
        foreach (MsbEntity c in Added)
        {
            View.ViewportSelection.AddSelection(c);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        AtlasEdit?.Undo();

        for (var i = 0; i < Added.Count(); i++)
        {
            // Nothing was added for this entity if there was no target map
            if (AddedMaps[i] == null)
                continue;

            var map = AddedMaps[i] as MapContainer;

            MapContainer.MarkEntityDirty(map, Added[i]);
            EntityPlacement.Detach(map, Added[i]);

            if (Added[i].RenderSceneMesh != null)
            {
                Added[i].RenderSceneMesh.AutoRegister = false;
                Added[i].RenderSceneMesh.UnregisterWithScene();
            }

            Added[i].CollisionSceneMesh = null;
        }

        View.ViewportSelection.ClearSelection();
        return ActionEvent.ObjectAddedRemoved;
    }
}
