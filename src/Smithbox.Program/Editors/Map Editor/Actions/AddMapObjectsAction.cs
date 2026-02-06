using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;


public class AddMapObjectsAction : ViewportAction
{
    private MapEditorView View;

    private static Regex TrailIDRegex = new(@"_(?<id>\d+)$");
    private readonly List<MsbEntity> Added = new();
    private readonly List<ObjectContainer> AddedMaps = new();
    private readonly MapContainer Map;
    private readonly Entity Parent;
    private readonly bool SetSelection;
    private MapContainer TargetMap;

    public AddMapObjectsAction(MapEditorView view, MapContainer map, List<MsbEntity> objects,
        bool setSelection, Entity parent, MapContainer targetMap = null)
    {
        View = view;
        Map = map;
        Added.AddRange(objects);
        SetSelection = setSelection;
        Parent = parent;
        TargetMap = targetMap;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = View.Universe;

        for (var i = 0; i < Added.Count(); i++)
        {
            if (Map != null)
            {
                Map.Objects.Add(Added[i]);
                Parent.AddChild(Added[i]);
                Added[i].UpdateRenderModel();
                if (Added[i].RenderSceneMesh != null)
                {
                    Added[i].RenderSceneMesh.SetSelectable(Added[i]);
                }

                if (Added[i].RenderSceneMesh != null)
                {
                    Added[i].RenderSceneMesh.AutoRegister = true;
                    Added[i].RenderSceneMesh.Register();
                }

                MsbEntity ent = Added[i];
                MapContainer m;

                AddedMaps.Add(Map);

                if (TargetMap != null)
                {
                    m = View.Selection.GetMapContainerFromMapID(TargetMap.Name);

                    // Prefab-specific
                    if (CFG.Current.Prefab_ApplyUniqueInstanceID)
                    {
                        MapEditorActionHelper.SetUniqueInstanceID(View, ent, m);
                    }
                    if (CFG.Current.Prefab_ApplyUniqueEntityID)
                    {
                        MapEditorActionHelper.SetUniqueEntityID(View, ent, m);
                    }
                    if (CFG.Current.Prefab_ApplySpecificEntityGroupID)
                    {
                        MapEditorActionHelper.SetSpecificEntityGroupID(View, ent, m);
                    }
                }
            }
            else
            {
                AddedMaps.Add(null);
            }
        }

        if (SetSelection)
        {
            universe.Selection.ClearSelection();
            foreach (MsbEntity c in Added)
            {
                universe.Selection.AddSelection(c);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = View.Universe;

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

        //Clones.Clear();
        if (SetSelection)
        {
            universe.Selection.ClearSelection();
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
