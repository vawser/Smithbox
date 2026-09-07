using HKX2;
using Microsoft.AspNetCore.Mvc.ViewEngines;
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

    private readonly Entity TargetBTL;
    private readonly MapContainer TargetMap;

    private string CreatedMapGroupGUID = "";

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
        float randomRotationRange = 360)
    {
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
                // Get list of names that exist so our duplicate names don't trample over them
                if (!objectnames.ContainsKey(Clonables[i].MapID))
                {
                    var nameset = new HashSet<string>();
                    foreach (Entity n in map.Objects)
                    {
                        nameset.Add(n.Name);
                    }

                    objectnames.Add(Clonables[i].MapID, nameset);
                }

                MsbEntity newobj = clonesCached ? Clones[i] : (MsbEntity)Clonables[i].Clone();

                GenerateUniqueName(Clonables[i], newobj, objectnames);

                if (TargetMap == null)
                {
                    map.Objects.Insert(map.Objects.IndexOf(Clonables[i]) + 1, newobj);
                }
                else
                {
                    map.Objects.Insert(map.Objects.IndexOf(Clonables[i]) + 1, newobj);
                    //map.Objects.Add(newobj);
                }

                if (TargetBTL != null && newobj.WrappedObject is BTL.Light)
                {
                    TargetBTL.AddChild(newobj);
                }
                else if (DuplicateToMap && TargetMap != null)
                {
                    // Duping to a targeted map, update parent.
                    if (TargetMap.MapOffsetNode != null)
                    {
                        TargetMap.MapOffsetNode.AddChild(newobj);
                    }
                    else
                    {
                        TargetMap.RootObject.AddChild(newobj);
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

                if (!clonesCached)
                {
                    Clones.Add(newobj);
                    CloneMaps.Add(map);
                    map.HasUnsavedChanges = true;
                }
                else
                {
                    if (Clones[i].RenderSceneMesh != null)
                    {
                        Clones[i].RenderSceneMesh.AutoRegister = true;
                        Clones[i].RenderSceneMesh.Register();
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
        for (var i = 0; i < Clones.Count(); i++)
        {
            CloneMaps[i].Objects.Remove(Clones[i]);
            if (Clones[i] != null)
            {
                if (Clones[i].Parent != null)
                {
                    Clones[i].Parent.RemoveChild(Clones[i]);
                }
            }

            if (Clones[i].RenderSceneMesh != null)
            {
                Clones[i].RenderSceneMesh.AutoRegister = false;
                Clones[i].RenderSceneMesh.UnregisterWithScene();
            }
        }

        View.ViewportSelection.ClearSelection();
        foreach (MsbEntity c in Clones)
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

    private void GenerateUniqueName(MsbEntity source, MsbEntity clone, Dictionary<string, HashSet<string>> objectnames)
    {
        if (source.Name == null)
        {
            clone.Name = "";
            objectnames[source.MapID].Add(clone.Name);
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

                while (objectnames[source.MapID].Contains(baseName + newid))
                {
                    id++;
                    newid = id.ToString("D" + idstring.Length);
                }

                clone.Name = baseName + newid;
                objectnames[source.MapID].Add(clone.Name);
            }
            else
            {
                var idstring = "0001";
                var id = int.Parse(idstring);
                var newid = idstring;

                while (objectnames[source.MapID].Contains(source.Name + "_" + newid))
                {
                    id++;
                    newid = id.ToString("D" + idstring.Length);
                }

                clone.Name = source.Name + "_" + newid;
                objectnames[source.MapID].Add(clone.Name);
            }
        }
    }
}
