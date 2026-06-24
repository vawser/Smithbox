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

    public EntAddAction(MapEditorView view, MapContainer map, List<MsbEntity> objects, Entity parent)
    {
        View = view;
        Map = map;
        Parent = parent;

        Added.AddRange(objects);
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
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

        View.ViewportSelection.ClearSelection();
        foreach (MsbEntity c in Added)
        {
            View.ViewportSelection.AddSelection(c);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Added.Count(); i++)
        {
            AddedMaps[i].Objects.Remove(Added[i]);
            if (Added[i] != null)
            {
                Added[i].Parent.RemoveChild(Added[i]);
            }

            if (Added[i].RenderSceneMesh != null)
            {
                Added[i].RenderSceneMesh.AutoRegister = false;
                Added[i].RenderSceneMesh.UnregisterWithScene();
            }
        }

        View.ViewportSelection.ClearSelection();
        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
