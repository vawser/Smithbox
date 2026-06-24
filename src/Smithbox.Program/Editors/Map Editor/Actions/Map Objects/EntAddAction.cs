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

    private static Regex TrailIDRegex = new(@"_(?<id>\d+)$");
    private readonly List<MsbEntity> Added = new();
    private readonly List<ObjectContainer> AddedMaps = new();
    private readonly MapContainer Map;
    private readonly Entity Parent;
    private readonly bool SetSelection;
    private MapContainer TargetMap;

    public EntAddAction(MapEditorView view, MapContainer map, List<MsbEntity> objects,
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

                if (Added[i] is MsbEntity msbEnt)
                {
                    msbEnt.AssignDrawable();
                }
                Added[i].UpdateRenderModel();
                Added[i].RenderSceneMesh.RenderSelectionOutline = true;

                if (Added[i].RenderSceneMesh != null)
                {
                    Added[i].RenderSceneMesh.SetSelectable(Added[i]);
                }

                MsbEntity ent = Added[i];

                AddedMaps.Add(Map);

                // Prefab-specific
                if (CFG.Current.Prefab_ApplyUniqueInstanceID)
                {
                    MapEditorActionHelper.SetUniqueInstanceID(View, ent, Map);
                }
                if (CFG.Current.Prefab_ApplyUniqueEntityID)
                {
                    MapEditorActionHelper.SetUniqueEntityID(View, ent, Map);
                }
                if (CFG.Current.Prefab_ApplySpecificEntityGroupID)
                {
                    MapEditorActionHelper.SetSpecificEntityGroupID(View, ent, Map);
                }
            }
            else
            {
                AddedMaps.Add(null);
            }
        }

        if (SetSelection)
        {
            universe.View.ViewportSelection.ClearSelection();
            foreach (MsbEntity c in Added)
            {
                universe.View.ViewportSelection.AddSelection(c);
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
        }

        //Clones.Clear();
        if (SetSelection)
        {
            universe.View.ViewportSelection.ClearSelection();
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}
