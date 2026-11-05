using HKX2;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using SoulsFormats.KF4;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using static StudioCore.Editors.MapEditor.DuplicateAction;

namespace StudioCore.Editors.MapEditor;

public class EntDuplicateAction : ViewportAction
{
    private MapEditorView View;
    private Action<bool> PostExecutionAction;

    private static readonly Regex TrailIDRegex = new(@"_(?<id>\d+)$");

    private readonly List<MsbEntity> Clonables = new();
    private readonly List<ObjectContainer> CloneMaps = new();
    private readonly List<MsbEntity> Clones = new();

    // Clone for each index of Clonables; sources that were skipped have no entry,
    // so Clones and Clonables indices do not line up
    private readonly Dictionary<int, MsbEntity> CloneBySource = new();

    private readonly Entity TargetBTL;
    private readonly MapContainer TargetMap;

    private string CreatedMapGroupGUID = "";

    private LightAtlasEdit AtlasEdit;

    private readonly bool DuplicateToMap;
    private readonly bool CreateMapGroupEntry;
    private readonly bool IncrementEntityID;
    private readonly bool IncrementInstanceID;
    private readonly bool IncrementPartNames;
    private readonly bool ClearEntityID;
    private readonly bool ClearEntityGroupID;
    private readonly bool RandomiseRotation;
    private readonly RandomiseRotationDirection RandomRotationDir;
    private readonly float RandomRotationRange;
    private readonly bool PlaceAtListEnd;

    /// <summary>
    /// The clones made by the first execute
    /// </summary>
    public IReadOnlyList<MsbEntity> CreatedClones => Clones;

    public EntDuplicateAction(MapEditorView view, 
        List<MsbEntity> objects, 
        MapContainer targetMap = null, 
        Entity targetBTL = null, 
        bool duplicateToMap = false, 
        bool createMapGroupEntry = false,
        bool incrementEntityID = false,
        bool incrementInstanceID = false,
        bool incrementPartNames = false,
        bool clearEntityID = false,
        bool clearEntityGroupID = false,
        bool randomiseRotation = false,
        RandomiseRotationDirection randomRotationDir = RandomiseRotationDirection.X,
        float randomRotationRange = 360,
        bool placeAtListEnd = false)
    {
        PlaceAtListEnd = placeAtListEnd;
        View = view;

        Clonables.AddRange(objects);

        TargetMap = targetMap;
        TargetBTL = targetBTL;

        DuplicateToMap = duplicateToMap;
        CreateMapGroupEntry = createMapGroupEntry;

        IncrementEntityID = incrementEntityID;
        IncrementInstanceID = incrementInstanceID;
        IncrementPartNames = incrementPartNames;
        ClearEntityID = clearEntityID;
        ClearEntityGroupID = clearEntityGroupID;
        RandomiseRotation = randomiseRotation;
        RandomRotationDir = randomRotationDir;
        RandomRotationRange = randomRotationRange;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var clonesCached = Clones.Count() > 0;

        var objectnames = new Dictionary<string, HashSet<string>>();
        var mapPartEntities = new Dictionary<MapContainer, HashSet<MsbEntity>>();

        for (var i = 0; i < Clonables.Count(); i++)
        {
            if (Clonables[i].MapID == null)
            {
                Smithbox.LogError(this,
                    LOC.Get("MAP_Actions_Duplicate_Failed", Clonables[i].Name));
                continue;
            }

            MapContainer map;
            if (TargetMap != null)
            {
                map = View.Selection.GetMapContainerFromMapID(TargetMap.Name);
            }
            else
            {
                map = View.Selection.GetMapContainerFromMapID(Clonables[i].MapID);
            }

            if (map != null)
            {
                // Get list of names that exist so our duplicate names don't trample over them.
                // Keyed by the map the clones go into, as that is where the names must be unique.
                if (!objectnames.ContainsKey(map.Name))
                {
                    var nameset = new HashSet<string>();
                    foreach (Entity n in map.Objects)
                    {
                        nameset.Add(n.Name);
                    }

                    objectnames.Add(map.Name, nameset);
                }

                MsbEntity newobj;
                if (clonesCached)
                {
                    // Only redo the clones that were made on the first execute
                    if (!CloneBySource.TryGetValue(i, out newobj))
                        continue;
                }
                else
                {
                    newobj = (MsbEntity)Clonables[i].Clone();
                }

                GenerateUniqueName(Clonables[i], newobj, objectnames[map.Name]);

                map.Objects.Insert(GetInsertIndex(map, Clonables[i], PlaceAtListEnd), newobj);

                if (TargetBTL != null && newobj.WrappedObject is BTL.Light)
                {
                    TargetBTL.AddChild(newobj);
                }
                else if (DuplicateToMap && TargetMap != null)
                {
                    // Duping to a targeted map, update parent. The container resolved by ID is used, as
                    // the one passed in may be from an earlier load of the map.
                    if (map.MapOffsetNode != null)
                    {
                        map.MapOffsetNode.AddChild(newobj);
                    }
                    else
                    {
                        map.RootObject.AddChild(newobj);
                    }
                }
                else if (Clonables[i].Parent != null)
                {
                    var idx = Clonables[i].Parent.ChildIndex(Clonables[i]);
                    Clonables[i].Parent.AddChild(newobj, idx + 1);
                }

                if (newobj is MsbEntity msbEnt)
                {
                    msbEnt.AssignDrawable();
                }
                newobj.UpdateRenderModel();

                if (newobj.RenderSceneMesh != null)
                {
                    newobj.RenderSceneMesh.RenderSelectionOutline = true;
                    newobj.RenderSceneMesh.SetSelectable(newobj);
                }

                // Marked on redo as well, as the clones are re-added to the map
                MapContainer.MarkEntityDirty(map, newobj);

                if (!clonesCached)
                {
                    Clones.Add(newobj);
                    CloneMaps.Add(map);
                    CloneBySource[i] = newobj;
                }
                else
                {
                    if (newobj.RenderSceneMesh != null)
                    {
                        newobj.RenderSceneMesh.AutoRegister = true;
                        newobj.RenderSceneMesh.Register();
                    }
                }

                if (IncrementEntityID)
                {
                    MapEditorActionHelper.SetUniqueEntityID(View, newobj, map);
                }

                if (IncrementInstanceID)
                {
                    MapEditorActionHelper.SetUniqueInstanceID(View, newobj, map);
                }

                if (IncrementPartNames)
                {
                    MapEditorActionHelper.SetSelfPartNames(View, newobj, map);
                }

                if (ClearEntityID)
                {
                    MapEditorActionHelper.ClearEntityID(View, newobj, map);
                }

                if (ClearEntityGroupID)
                {
                    MapEditorActionHelper.ClearEntityGroupID(View, newobj, map);
                }

                if(RandomiseRotation)
                {
                    MapEditorActionHelper.RandomiseRotation(View, newobj, map, RandomRotationDir, RandomRotationRange);
                }
            }
        }

        if (AtlasEdit == null)
        {
            AtlasEdit = LightAtlasEdit.ForNewParts(Clones, CloneMaps);
        }
        else
        {
            AtlasEdit.Redo();
        }

        View.ViewportSelection.ClearSelection();
        foreach (MsbEntity c in Clones)
        {
            View.ViewportSelection.AddSelection(c);
        }

        if(CreateMapGroupEntry)
        {
            var curSelection = View.ViewportSelection.GetEntitySelection();
            CreatedMapGroupGUID = View.MapGroupsView.CreateMapGroupEntry(curSelection);
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(false);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Undo()
    {
        AtlasEdit?.Undo();

        for (var i = 0; i < Clones.Count(); i++)
        {
            var map = CloneMaps[i] as MapContainer;

            MapContainer.MarkEntityDirty(map, Clones[i]);
            EntityPlacement.Detach(map, Clones[i]);

            if (Clones[i].RenderSceneMesh != null)
            {
                Clones[i].RenderSceneMesh.AutoRegister = false;
                Clones[i].RenderSceneMesh.UnregisterWithScene();
            }

            Clones[i].CollisionSceneMesh = null;
            Clones[i].CollisionLayerMeshes = null;
            Clones[i].PrefabMemberMeshes = null;
            Clones[i].SelectionFillMesh = null;
            Clones[i].SelectionWireMesh = null;
            Clones[i].ClearGrass();
            Clones[i].ClearEdgePoints();
            Clones[i].ClearWalls();
        }

        // The clones no longer exist, so hand the selection (and its outline) back to the originals
        View.ViewportSelection.ClearSelection();
        foreach (MsbEntity c in Clonables)
        {
            View.ViewportSelection.AddSelection(c);
        }

        if (CreateMapGroupEntry)
        {
            View.MapGroupsView.DeleteMapGroupEntry(CreatedMapGroupGUID);
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(false);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    /// <summary>
    /// Where a clone goes in the target map's object list: straight after its source if the source is in that map
    /// (unless placing at the list end), otherwise after the last object of the same type (so a pasted or
    /// duplicated-to-map object joins its type, rather than going to the front of the list, ahead of every other
    /// object), or at the end if there is none.
    /// </summary>
    private static int GetInsertIndex(MapContainer map, MsbEntity source, bool placeAtListEnd)
    {
        var sourceIndex = map.Objects.IndexOf(source);

        if (sourceIndex >= 0 && !placeAtListEnd)
            return sourceIndex + 1;

        var type = source.WrappedObject?.GetType();

        if (type != null)
        {
            for (var i = map.Objects.Count - 1; i >= 0; i--)
            {
                if (map.Objects[i].WrappedObject?.GetType() == type)
                    return i + 1;
            }
        }

        return map.Objects.Count;
    }

    private void GenerateUniqueName(MsbEntity source, MsbEntity clone, HashSet<string> names)
    {
        if (source.Name == null)
        {
            clone.Name = "";
            names.Add(clone.Name);
        }
        else
        {
            Match idmatch = TrailIDRegex.Match(source.Name);
            if (idmatch.Success)
            {
                var idstring = idmatch.Result("${id}");
                var id = int.Parse(idstring);
                var baseName = source.Name.Substring(0, source.Name.Length - idstring.Length);
                var newid = idstring;

                while (names.Contains(baseName + newid))
                {
                    id++;
                    newid = id.ToString("D" + idstring.Length);
                }

                clone.Name = baseName + newid;
                names.Add(clone.Name);
            }
            else
            {
                var idstring = "0001";
                var id = int.Parse(idstring);
                var newid = idstring;

                while (names.Contains(source.Name + "_" + newid))
                {
                    id++;
                    newid = id.ToString("D" + idstring.Length);
                }

                clone.Name = source.Name + "_" + newid;
                names.Add(clone.Name);
            }
        }
    }
}
