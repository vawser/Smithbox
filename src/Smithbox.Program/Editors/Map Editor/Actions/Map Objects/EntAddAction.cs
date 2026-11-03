using HKX2;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    private int NextIndex;
    private UniqueIdCache IdCache;

    public int AddedCount => NextIndex;
    public int TotalCount => Added.Count;

    public EntAddAction(MapEditorView view, MapContainer map, List<MsbEntity> objects, Entity parent)
    {
        View = view;
        Map = map;
        Parent = parent;

        Added.AddRange(objects);
    }

    /// <summary>
    /// Adds objects until the time budget is used, so a large action can be spread across several frames.
    /// Execute must still be called afterwards, and will only add the objects that are left.
    /// </summary>
    /// <returns>True if every object has been added.</returns>
    public bool ExecuteStep(TimeSpan budget)
    {
        var timer = Stopwatch.StartNew();

        while (NextIndex < Added.Count)
        {
            AddNext();

            if (timer.Elapsed >= budget)
                break;
        }

        return NextIndex >= Added.Count;
    }

    private void AddNext()
    {
        if (NextIndex == 0)
        {
            // Rebuilt on every execute so it stays index-aligned with Added on redo
            AddedMaps.Clear();
            IdCache = new();
        }

        var i = NextIndex;
        NextIndex++;

        if (Map != null)
        {
            var msbEnt = Added[i];

            // Adding a child builds its mesh, unless that has already been done by an earlier execute
            var hasMesh = msbEnt.SetupRenderMesh;

            Map.Objects.Add(msbEnt);
            Parent.AddChild(msbEnt);

            if (hasMesh)
            {
                msbEnt.AssignDrawable();
                msbEnt.UpdateRenderModel();
            }

            if (msbEnt.RenderSceneMesh != null)
            {
                msbEnt.RenderSceneMesh.RenderSelectionOutline = true;
                msbEnt.RenderSceneMesh.SetSelectable(msbEnt);
                msbEnt.RenderSceneMesh.AutoRegister = true;
                msbEnt.RenderSceneMesh.Register();
            }

            AddedMaps.Add(Map);

            MapContainer.MarkEntityDirty(Map, msbEnt);

            // Prefab-specific
            if (CFG.Current.Prefab_ApplyUniqueInstanceID)
            {
                MapEditorActionHelper.SetUniqueInstanceID(View, msbEnt, Map, IdCache);
            }
            if (CFG.Current.Prefab_ApplyUniqueEntityID)
            {
                MapEditorActionHelper.SetUniqueEntityID(View, msbEnt, Map, IdCache);
            }
            if (CFG.Current.Prefab_ApplySpecificEntityGroupID)
            {
                MapEditorActionHelper.SetSpecificEntityGroupID(View, msbEnt, Map);
            }
        }
        else
        {
            AddedMaps.Add(null);
        }
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        while (NextIndex < Added.Count)
        {
            AddNext();
        }

        NextIndex = 0;
        IdCache = null;

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
            Added[i].CollisionLayerMeshes = null;
            Added[i].PrefabMemberMeshes = null;
            Added[i].SelectionFillMesh = null;
            Added[i].SelectionWireMesh = null;
            (Added[i] as MsbEntity)?.ClearGrass();
            (Added[i] as MsbEntity)?.ClearEdgePoints();
            (Added[i] as MsbEntity)?.ClearWalls();
        }

        View.ViewportSelection.ClearSelection();
        return ActionEvent.ObjectAddedRemoved;
    }
}
