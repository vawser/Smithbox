using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

public class CloneMapObjectsAction : ViewportAction
{
    private MapEditorView View;

    private static readonly Regex TrailIDRegex = new(@"_(?<id>\d+)$");
    private readonly List<MsbEntity> Clonables = new();
    private readonly List<CloneRecord> Records = new();
    private readonly bool SetSelection;
    private readonly Entity TargetBTL;
    private readonly MapContainer TargetMap;
    private readonly bool Silent = false;

    public CloneMapObjectsAction(MapEditorView view, List<MsbEntity> objects, bool setSelection, MapContainer targetMap = null, Entity targetBTL = null, bool silent = false)
    {
        View = view;
        Clonables.AddRange(objects);
        SetSelection = setSelection;
        TargetMap = targetMap;
        TargetBTL = targetBTL;
        Silent = silent;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = View.Universe;
        var isRecorded = Records.Count > 0;

        if (!isRecorded)
        {
            // First execution - create clones and records
            var objectnames = new Dictionary<string, HashSet<string>>();

            for (var i = 0; i < Clonables.Count; i++)
            {
                if (Clonables[i].MapID == null)
                {
                    if (!Silent)
                    {
                        Smithbox.Log(this, $"Failed to dupe {Clonables[i].Name}, as it had no defined MapID",
                            LogLevel.Warning);
                    }
                    continue;
                }

                MapContainer targetMapContainer = TargetMap != null
                    ? View.Selection.GetMapContainerFromMapID(TargetMap.Name)
                    : View.Selection.GetMapContainerFromMapID(Clonables[i].MapID);

                if (targetMapContainer == null)
                    continue;

                // Build name set for collision detection
                if (!objectnames.ContainsKey(Clonables[i].MapID))
                {
                    var nameset = new HashSet<string>();
                    foreach (Entity n in targetMapContainer.Objects)
                    {
                        nameset.Add(n.Name);
                    }
                    objectnames.Add(Clonables[i].MapID, nameset);
                }

                // Create clone
                MsbEntity newobj = (MsbEntity)Clonables[i].Clone();

                // Persist the supports name bool
                if (!Clonables[i].SupportsName)
                {
                    newobj.SupportsName = false;
                }

                // Generate unique name
                GenerateUniqueName(Clonables[i], newobj, objectnames);

                // Determine insertion point
                int mapInsertIndex = TargetMap == null
                    ? targetMapContainer.Objects.IndexOf(Clonables[i]) + 1
                    : targetMapContainer.Objects.Count;

                if(CFG.Current.Toolbar_Duplicate_Place_at_List_End)
                {
                    var curEnt = Clonables[i];
                    var type = curEnt.WrappedObject.GetType();

                    var categoryList = targetMapContainer.Objects.Where(e => e.WrappedObject.GetType() == type).ToList();

                    var insertEnt = categoryList.Last();

                    mapInsertIndex = TargetMap == null
                    ? targetMapContainer.Objects.IndexOf(insertEnt) + 1
                    : targetMapContainer.Objects.Count;
                }

                // Determine parent
                Entity targetParent = null;
                int parentInsertIndex = -1;

                if (TargetBTL != null && newobj.WrappedObject is BTL.Light)
                {
                    targetParent = TargetBTL;
                    parentInsertIndex = TargetBTL.Children.Count;
                }
                else if (TargetMap != null)
                {
                    targetParent = TargetMap.MapOffsetNode ?? TargetMap.RootObject;
                    parentInsertIndex = targetParent.Children.Count;
                }
                else if (Clonables[i].Parent != null)
                {
                    targetParent = Clonables[i].Parent;
                    parentInsertIndex = Clonables[i].Parent.ChildIndex(Clonables[i]) + 1;
                }

                // Create record
                Records.Add(new CloneRecord
                {
                    Map = targetMapContainer,
                    Clone = newobj,
                    MapInsertIndex = mapInsertIndex,
                    Parent = (MsbEntity)targetParent,
                    ParentInsertIndex = parentInsertIndex
                });

                if (TargetMap != null)
                {
                    Smithbox.Log(this, $"Duplicated {newobj.Name} to {TargetMap.Name}");
                }
            }
        }

        // Insert into maps in order
        foreach (var record in Records.OrderBy(r => r.Map).ThenBy(r => r.MapInsertIndex))
        {
            int idx = record.MapInsertIndex;
            if (idx > record.Map.Objects.Count)
                idx = record.Map.Objects.Count;

            record.Map.Objects.Insert(idx, record.Clone);
            record.Map.HasUnsavedChanges = true;
        }

        // Build hierarchy
        var parentGroups = Records
            .Where(r => r.Parent != null && r.ParentInsertIndex >= 0)
            .GroupBy(r => r.Parent);

        foreach (var group in parentGroups)
        {
            foreach (var record in group.OrderBy(x => x.ParentInsertIndex))
            {
                int idx = record.ParentInsertIndex;
                if (idx > record.Parent.Children.Count)
                    idx = record.Parent.Children.Count;

                record.Parent.Children.Insert(idx, record.Clone);
                record.Clone.Parent = record.Parent;
            }
        }

        // Apply configuration-based updates (only on first execution)
        if (!isRecorded)
        {
            foreach (var record in Records)
            {
                if (CFG.Current.Toolbar_Duplicate_Increment_Entity_ID)
                {
                    MapEditorActionHelper.SetUniqueEntityID(View, record.Clone, record.Map);
                }
                if (CFG.Current.Toolbar_Duplicate_Increment_InstanceID)
                {
                    MapEditorActionHelper.SetUniqueInstanceID(View, record.Clone, record.Map);
                }
                if (CFG.Current.Toolbar_Duplicate_Increment_PartNames)
                {
                    MapEditorActionHelper.SetSelfPartNames(View, record.Clone, record.Map);
                }
                if (CFG.Current.Toolbar_Duplicate_Clear_Entity_ID)
                {
                    MapEditorActionHelper.ClearEntityID(View, record.Clone, record.Map);
                }
                if (CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs)
                {
                    MapEditorActionHelper.ClearEntityGroupID(View, record.Clone, record.Map);
                }
            }
        }

        // Update render models and register meshes
        foreach (var record in Records)
        {
            record.Clone.UpdateRenderModel();
            if (record.Clone.RenderSceneMesh != null)
            {
                record.Clone.RenderSceneMesh.SetSelectable(record.Clone);
                record.Clone.RenderSceneMesh.AutoRegister = true;
                record.Clone.RenderSceneMesh.Register();
            }
        }

        // Update selection
        if (SetSelection)
        {
            universe.View.ViewportSelection.ClearSelection();
            foreach (var record in Records)
            {
                universe.View.ViewportSelection.AddSelection(record.Clone);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = View.Universe;

        // Remove from maps in reverse order
        var mapOrdered = Records
            .OrderByDescending(r => r.Map)
            .ThenByDescending(r => r.MapInsertIndex);

        foreach (var record in mapOrdered)
        {
            record.Map.Objects.Remove(record.Clone);

            if (record.Clone.RenderSceneMesh != null)
            {
                record.Clone.RenderSceneMesh.AutoRegister = false;
                record.Clone.RenderSceneMesh.UnregisterWithScene();
            }
        }

        // Remove from hierarchy
        foreach (var record in Records.Where(r => r.Parent != null))
        {
            record.Parent.Children.Remove(record.Clone);
            record.Clone.Parent = null;
        }

        // Restore selection to original objects
        if (SetSelection)
        {
            universe.View.ViewportSelection.ClearSelection();
            foreach (MsbEntity c in Clonables)
            {
                universe.View.ViewportSelection.AddSelection(c);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    private void GenerateUniqueName(MsbEntity source, MsbEntity clone, Dictionary<string, HashSet<string>> objectnames)
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

    public override string GetEditMessage()
    {
        return "";
    }

    private struct CloneRecord
    {
        public MapContainer Map;
        public MsbEntity Clone;
        public int MapInsertIndex;

        public MsbEntity Parent;
        public int ParentInsertIndex;
    }
}