using Microsoft.Extensions.Logging;
using SoulsFormats;
using SoulsFormats.Util;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.MsbEditor;

/// <summary>
///     An action that can be performed by the user in the editor that represents
///     a single atomic editor action that affects the state of the map. Each action
///     should have enough information to apply the action AND undo the action, as
///     these actions get pushed to a stack for undo/redo
/// </summary>
public abstract class Action
{
    public abstract ActionEvent Execute();
    public abstract ActionEvent Undo();
}

public class PropertiesChangedAction : Action
{
    private readonly object ChangedObject;
    private readonly List<PropertyChange> Changes = new();
    private Action<bool> PostExecutionAction;

    public PropertiesChangedAction(object changed)
    {
        ChangedObject = changed;
    }

    public PropertiesChangedAction(PropertyInfo prop, object changed, object newval)
    {
        ChangedObject = changed;
        var change = new PropertyChange();
        change.Property = prop;
        change.OldValue = prop.GetValue(ChangedObject);
        change.NewValue = newval;
        change.ArrayIndex = -1;
        Changes.Add(change);
    }

    public PropertiesChangedAction(PropertyInfo prop, int index, object changed, object newval)
    {
        ChangedObject = changed;
        var change = new PropertyChange();
        change.Property = prop;
        if (index != -1 && prop.PropertyType.IsArray)
        {
            var a = (Array)change.Property.GetValue(ChangedObject);
            change.OldValue = a.GetValue(index);
        }
        else
        {
            change.OldValue = prop.GetValue(ChangedObject);
        }

        change.NewValue = newval;
        change.ArrayIndex = index;
        Changes.Add(change);
    }

    public void AddPropertyChange(PropertyInfo prop, object newval, int index = -1)
    {
        var change = new PropertyChange();
        change.Property = prop;
        if (index != -1 && prop.PropertyType.IsArray)
        {
            var a = (Array)change.Property.GetValue(ChangedObject);
            change.OldValue = a.GetValue(index);
        }
        else
        {
            change.OldValue = prop.GetValue(ChangedObject);
        }

        change.NewValue = newval;
        change.ArrayIndex = index;
        Changes.Add(change);
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute()
    {
        foreach (PropertyChange change in Changes)
        {
            if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
            {
                var a = (Array)change.Property.GetValue(ChangedObject);
                a.SetValue(change.NewValue, change.ArrayIndex);
            }
            else
            {
                change.Property.SetValue(ChangedObject, change.NewValue);
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(false);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (PropertyChange change in Changes)
        {
            if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
            {
                var a = (Array)change.Property.GetValue(ChangedObject);
                a.SetValue(change.OldValue, change.ArrayIndex);
            }
            else
            {
                change.Property.SetValue(ChangedObject, change.OldValue);
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return ActionEvent.NoEvent;
    }

    private class PropertyChange
    {
        public int ArrayIndex;
        public object NewValue;
        public object OldValue;
        public PropertyInfo Property;
    }
}

/// <summary>
///     Copies values from one array to another without affecting references.
/// </summary>
public class ArrayPropertyCopyAction : Action
{
    private readonly List<PropertyChange> Changes = new();
    private Action<bool> PostExecutionAction;

    public ArrayPropertyCopyAction(Array source, Array target)
    {
        for (var i = 0; i < target.Length; i++)
        {
            PropertyChange change = new()
            {
                ChangedObj = target, OldVal = target.GetValue(i), NewVal = source.GetValue(i), ArrayIndex = i
            };
            Changes.Add(change);
        }
    }

    public ArrayPropertyCopyAction(Array source, IEnumerable<Array> targetList)
    {
        foreach (Array target in targetList)
        {
            for (var i = 0; i < target.Length; i++)
            {
                PropertyChange change = new()
                {
                    ChangedObj = target, OldVal = target.GetValue(i), NewVal = source.GetValue(i), ArrayIndex = i
                };
                Changes.Add(change);
            }
        }
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute()
    {
        foreach (PropertyChange change in Changes)
        {
            change.ChangedObj.SetValue(change.NewVal, change.ArrayIndex);
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(false);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (PropertyChange change in Changes)
        {
            change.ChangedObj.SetValue(change.OldVal, change.ArrayIndex);
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return ActionEvent.NoEvent;
    }

    private class PropertyChange
    {
        public int ArrayIndex;
        public Array ChangedObj;
        public object NewVal;
        public object OldVal;
    }
}

public class MultipleEntityPropertyChangeAction : Action
{
    private readonly HashSet<Entity> ChangedEnts = new();
    private readonly List<PropertyChange> Changes = new();

    public bool UpdateRenderModel = false;

    public MultipleEntityPropertyChangeAction(PropertyInfo prop, HashSet<Entity> changedEnts, object newval,
        int index = -1, int classIndex = -1)
    {
        ChangedEnts = changedEnts;
        foreach (Entity o in changedEnts)
        {
            var propObj = PropFinderUtil.FindPropertyObject(prop, o.WrappedObject, classIndex, false);
            var change = new PropertyChange
            {
                ChangedObj = propObj, Property = prop, NewValue = newval, ArrayIndex = index
            };
            if (index != -1 && prop.PropertyType.IsArray)
            {
                var a = (Array)change.Property.GetValue(propObj);
                change.OldValue = a.GetValue(index);
            }
            else
            {
                change.OldValue = prop.GetValue(propObj);
            }

            Changes.Add(change);
        }
    }

    public override ActionEvent Execute()
    {
        foreach (PropertyChange change in Changes)
        {
            if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
            {
                var a = (Array)change.Property.GetValue(change.ChangedObj);
                a.SetValue(change.NewValue, change.ArrayIndex);
            }
            else
            {
                change.Property.SetValue(change.ChangedObj, change.NewValue);
            }
        }

        foreach (Entity e in ChangedEnts)
        {
            if (UpdateRenderModel)
            {
                e.UpdateRenderModel();
            }

            // Clear name cache, forcing it to update.
            e.Name = null;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (PropertyChange change in Changes)
        {
            if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
            {
                var a = (Array)change.Property.GetValue(change.ChangedObj);
                a.SetValue(change.OldValue, change.ArrayIndex);
            }
            else
            {
                change.Property.SetValue(change.ChangedObj, change.OldValue);
            }
        }

        foreach (Entity e in ChangedEnts)
        {
            if (UpdateRenderModel)
            {
                e.UpdateRenderModel();
            }

            // Clear name cache, forcing it to update.
            e.Name = null;
        }

        return ActionEvent.NoEvent;
    }

    private class PropertyChange
    {
        public int ArrayIndex;
        public object ChangedObj;
        public object NewValue;
        public object OldValue;
        public PropertyInfo Property;
    }
}

public class CloneMapObjectsAction : Action
{
    private static readonly Regex TrailIDRegex = new(@"_(?<id>\d+)$");
    private readonly List<MapEntity> Clonables = new();
    private readonly List<ObjectContainer> CloneMaps = new();
    private readonly List<MapEntity> Clones = new();
    private readonly bool SetSelection;
    private readonly Entity TargetBTL;
    private readonly Map TargetMap;
    private readonly Universe Universe;
    private RenderScene Scene;

    public CloneMapObjectsAction(Universe univ, RenderScene scene, List<MapEntity> objects, bool setSelection,
        Map targetMap = null, Entity targetBTL = null)
    {
        Universe = univ;
        Scene = scene;
        Clonables.AddRange(objects);
        SetSelection = setSelection;
        TargetMap = targetMap;
        TargetBTL = targetBTL;
    }

    public override ActionEvent Execute()
    {
        var clonesCached = Clones.Count() > 0;

        var objectnames = new Dictionary<string, HashSet<string>>();
        Dictionary<Map, HashSet<MapEntity>> mapPartEntities = new();

        for (var i = 0; i < Clonables.Count(); i++)
        {
            if (Clonables[i].MapID == null)
            {
                TaskLogs.AddLog($"Failed to dupe {Clonables[i].Name}, as it had no defined MapID",
                    LogLevel.Warning);
                continue;
            }

            Map? m;
            if (TargetMap != null)
            {
                m = Universe.GetLoadedMap(TargetMap.Name);
            }
            else
            {
                m = Universe.GetLoadedMap(Clonables[i].MapID);
            }

            if (m != null)
            {
                // Get list of names that exist so our duplicate names don't trample over them
                if (!objectnames.ContainsKey(Clonables[i].MapID))
                {
                    var nameset = new HashSet<string>();
                    foreach (Entity n in m.Objects)
                    {
                        nameset.Add(n.Name);
                    }

                    objectnames.Add(Clonables[i].MapID, nameset);
                }

                // If this was executed in the past we reused the cloned objects so because redo
                // actions that follow this may reference the previously cloned object
                MapEntity newobj = clonesCached ? Clones[i] : (MapEntity)Clonables[i].Clone();

                // Use pattern matching to attempt renames based on appended ID
                Match idmatch = TrailIDRegex.Match(Clonables[i].Name);
                if (idmatch.Success)
                {
                    var idstring = idmatch.Result("${id}");
                    var id = int.Parse(idstring);
                    var newid = idstring;
                    while (objectnames[Clonables[i].MapID]
                           .Contains(Clonables[i].Name.Substring(0, Clonables[i].Name.Length - idstring.Length) +
                                     newid))
                    {
                        id++;
                        newid = id.ToString("D" + idstring.Length);
                    }

                    newobj.Name = Clonables[i].Name.Substring(0, Clonables[i].Name.Length - idstring.Length) +
                                  newid;
                    objectnames[Clonables[i].MapID].Add(newobj.Name);
                }
                else
                {
                    var idstring = "0001";
                    var id = int.Parse(idstring);
                    var newid = idstring;
                    while (objectnames[Clonables[i].MapID].Contains(Clonables[i].Name + "_" + newid))
                    {
                        id++;
                        newid = id.ToString("D" + idstring.Length);
                    }

                    newobj.Name = Clonables[i].Name + "_" + newid;
                    objectnames[Clonables[i].MapID].Add(newobj.Name);
                }

                // Get a unique Instance ID for MSBE parts
                if (newobj.WrappedObject is MSBE.Part msbePart)
                {
                    if (mapPartEntities.TryAdd(m, new HashSet<MapEntity>()))
                    {
                        foreach (Entity ent in m.Objects)
                        {
                            if (ent.WrappedObject != null && ent.WrappedObject is MSBE.Part)
                            {
                                mapPartEntities[m].Add((MapEntity)ent);
                            }
                        }
                    }

                    var newInstanceID = msbePart.InstanceID;
                    while (mapPartEntities[m].FirstOrDefault(e =>
                               ((MSBE.Part)e.WrappedObject).ModelName == msbePart.ModelName
                               && ((MSBE.Part)e.WrappedObject).InstanceID == newInstanceID) != null)
                    {
                        newInstanceID++;
                    }

                    msbePart.InstanceID = newInstanceID;
                    mapPartEntities[m].Add(newobj);
                }

                m.Objects.Insert(m.Objects.IndexOf(Clonables[i]) + 1, newobj);
                if (TargetBTL != null && newobj.WrappedObject is BTL.Light)
                {
                    TargetBTL.AddChild(newobj);
                }
                else if (TargetMap != null)
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

                newobj.UpdateRenderModel();
                if (newobj.RenderSceneMesh != null)
                {
                    newobj.RenderSceneMesh.SetSelectable(newobj);
                }

                if (!clonesCached)
                {
                    Clones.Add(newobj);
                    CloneMaps.Add(m);
                    m.HasUnsavedChanges = true;
                }
                else
                {
                    if (Clones[i].RenderSceneMesh != null)
                    {
                        Clones[i].RenderSceneMesh.AutoRegister = true;
                        Clones[i].RenderSceneMesh.Register();
                    }
                }
            }
        }

        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
            foreach (MapEntity c in Clones)
            {
                Universe.Selection.AddSelection(c);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Clones.Count(); i++)
        {
            CloneMaps[i].Objects.Remove(Clones[i]);
            if (Clones[i] != null)
            {
                Clones[i].Parent.RemoveChild(Clones[i]);
            }

            if (Clones[i].RenderSceneMesh != null)
            {
                Clones[i].RenderSceneMesh.AutoRegister = false;
                Clones[i].RenderSceneMesh.UnregisterWithScene();
            }
        }

        // Clones.Clear();
        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
            foreach (MapEntity c in Clonables)
            {
                Universe.Selection.AddSelection(c);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}

public class AddMapObjectsAction : Action
{
    private static Regex TrailIDRegex = new(@"_(?<id>\d+)$");
    private readonly List<MapEntity> Added = new();
    private readonly List<ObjectContainer> AddedMaps = new();
    private readonly Map Map;
    private readonly Entity Parent;
    private readonly bool SetSelection;
    private readonly Universe Universe;
    private RenderScene Scene;

    public AddMapObjectsAction(Universe univ, Map map, RenderScene scene, List<MapEntity> objects,
        bool setSelection, Entity parent)
    {
        Universe = univ;
        Map = map;
        Scene = scene;
        Added.AddRange(objects);
        SetSelection = setSelection;
        Parent = parent;
    }

    public override ActionEvent Execute()
    {
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

                AddedMaps.Add(Map);
            }
            else
            {
                AddedMaps.Add(null);
            }
        }

        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
            foreach (MapEntity c in Added)
            {
                Universe.Selection.AddSelection(c);
            }
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

        //Clones.Clear();
        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}

/// <summary>
///     Deprecated
/// </summary>
[Obsolete]
public class AddParamsAction : Action
{
    private readonly List<PARAM.Row> Clonables = new();
    private readonly List<PARAM.Row> Clones = new();
    private readonly PARAM Param;
    private readonly bool SetSelection;
    private string ParamString;

    public AddParamsAction(PARAM param, string pstring, List<PARAM.Row> rows, bool setsel)
    {
        Param = param;
        Clonables.AddRange(rows);
        ParamString = pstring;
        SetSelection = setsel;
    }

    public override ActionEvent Execute()
    {
        foreach (PARAM.Row row in Clonables)
        {
            var newrow = new PARAM.Row(row);
            if (Param[row.ID] == null)
            {
                newrow.Name = row.Name != null ? row.Name : "";
                var index = 0;
                foreach (PARAM.Row r in Param.Rows)
                {
                    if (r.ID > newrow.ID)
                    {
                        break;
                    }

                    index++;
                }

                Param.Rows.Insert(index, newrow);
            }
            else
            {
                newrow.Name = row.Name != null ? row.Name + "_1" : "";
                Param.Rows.Insert(Param.Rows.IndexOf(Param[row.ID]) + 1, newrow);
            }

            Clones.Add(newrow);
        }

        if (SetSelection)
        {
            // EditorCommandQueue.AddCommand($@"param/select/{ParamString}/{Clones[0].ID}");
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Clones.Count(); i++)
        {
            Param.Rows.Remove(Clones[i]);
        }

        Clones.Clear();
        if (SetSelection)
        {
        }

        return ActionEvent.NoEvent;
    }
}

public class DeleteMapObjectsAction : Action
{
    private readonly List<MapEntity> Deletables = new();
    private readonly List<int> RemoveIndices = new();
    private readonly List<ObjectContainer> RemoveMaps = new();
    private readonly List<MapEntity> RemoveParent = new();
    private readonly List<int> RemoveParentIndex = new();
    private readonly bool SetSelection;
    private readonly Universe Universe;
    private RenderScene Scene;

    public DeleteMapObjectsAction(Universe univ, RenderScene scene, List<MapEntity> objects, bool setSelection)
    {
        Universe = univ;
        Scene = scene;
        Deletables.AddRange(objects);
        SetSelection = setSelection;
    }

    public override ActionEvent Execute()
    {
        foreach (MapEntity obj in Deletables)
        {
            Map m = Universe.GetLoadedMap(obj.MapID);
            if (m != null)
            {
                RemoveMaps.Add(m);
                m.HasUnsavedChanges = true;
                RemoveIndices.Add(m.Objects.IndexOf(obj));
                m.Objects.RemoveAt(RemoveIndices.Last());
                if (obj.RenderSceneMesh != null)
                {
                    obj.RenderSceneMesh.AutoRegister = false;
                    obj.RenderSceneMesh.UnregisterWithScene();
                }

                RemoveParent.Add((MapEntity)obj.Parent);
                if (obj.Parent != null)
                {
                    RemoveParentIndex.Add(obj.Parent.RemoveChild(obj));
                }
                else
                {
                    RemoveParentIndex.Add(-1);
                }
            }
            else
            {
                RemoveMaps.Add(null);
                RemoveIndices.Add(-1);
                RemoveParent.Add(null);
                RemoveParentIndex.Add(-1);
            }
        }

        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Deletables.Count(); i++)
        {
            if (RemoveMaps[i] == null || RemoveIndices[i] == -1)
            {
                continue;
            }

            RemoveMaps[i].Objects.Insert(RemoveIndices[i], Deletables[i]);
            if (Deletables[i].RenderSceneMesh != null)
            {
                Deletables[i].RenderSceneMesh.AutoRegister = true;
                Deletables[i].RenderSceneMesh.Register();
            }

            if (RemoveParent[i] != null)
            {
                RemoveParent[i].AddChild(Deletables[i], RemoveParentIndex[i]);
            }
        }

        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
            foreach (MapEntity d in Deletables)
            {
                Universe.Selection.AddSelection(d);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }
}

/// <summary>
///     Deprecated
/// </summary>
[Obsolete]
public class DeleteParamsAction : Action
{
    private readonly List<PARAM.Row> Deletables = new();
    private readonly PARAM Param;
    private readonly List<int> RemoveIndices = new();
    private readonly bool SetSelection = false;

    public DeleteParamsAction(PARAM param, List<PARAM.Row> rows)
    {
        Param = param;
        Deletables.AddRange(rows);
    }

    public override ActionEvent Execute()
    {
        foreach (PARAM.Row row in Deletables)
        {
            RemoveIndices.Add(Param.Rows.IndexOf(row));
            Param.Rows.RemoveAt(RemoveIndices.Last());
        }

        if (SetSelection)
        {
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Deletables.Count(); i++)
        {
            Param.Rows.Insert(RemoveIndices[i], Deletables[i]);
        }

        if (SetSelection)
        {
        }

        return ActionEvent.NoEvent;
    }
}

public class ReorderContainerObjectsAction : Action
{
    private readonly List<ObjectContainer> Containers = new();
    private readonly bool SetSelection;
    private readonly List<Entity> SourceObjects = new();
    private readonly List<int> TargetIndices = new();
    private readonly Universe Universe;
    private int[] UndoIndices;

    public ReorderContainerObjectsAction(Universe univ, List<Entity> src, List<int> targets, bool setSelection)
    {
        Universe = univ;
        SourceObjects.AddRange(src);
        TargetIndices.AddRange(targets);
        SetSelection = setSelection;
    }

    public override ActionEvent Execute()
    {
        var sourceindices = new int[SourceObjects.Count];
        for (var i = 0; i < SourceObjects.Count; i++)
        {
            ObjectContainer m = SourceObjects[i].Container;
            Containers.Add(m);
            m.HasUnsavedChanges = true;
            sourceindices[i] = m.Objects.IndexOf(SourceObjects[i]);
        }

        for (var i = 0; i < sourceindices.Length; i++)
        {
            // Remove object and update indices
            var src = sourceindices[i];
            Containers[i].Objects.RemoveAt(src);
            for (var j = 0; j < sourceindices.Length; j++)
            {
                if (sourceindices[j] > src)
                {
                    sourceindices[j]--;
                }
            }

            for (var j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > src)
                {
                    TargetIndices[j]--;
                }
            }

            // Add new object
            var dest = TargetIndices[i];
            Containers[i].Objects.Insert(dest, SourceObjects[i]);
            for (var j = 0; j < sourceindices.Length; j++)
            {
                if (sourceindices[j] > dest)
                {
                    sourceindices[j]++;
                }
            }

            for (var j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > dest)
                {
                    TargetIndices[j]++;
                }
            }
        }

        UndoIndices = sourceindices;
        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
            foreach (Entity c in SourceObjects)
            {
                Universe.Selection.AddSelection(c);
            }
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < TargetIndices.Count; i++)
        {
            // Remove object and update indices
            var src = TargetIndices[i];
            Containers[i].Objects.RemoveAt(src);
            for (var j = 0; j < UndoIndices.Length; j++)
            {
                if (UndoIndices[j] > src)
                {
                    UndoIndices[j]--;
                }
            }

            for (var j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > src)
                {
                    TargetIndices[j]--;
                }
            }

            // Add new object
            var dest = UndoIndices[i];
            Containers[i].Objects.Insert(dest, SourceObjects[i]);
            for (var j = 0; j < UndoIndices.Length; j++)
            {
                if (UndoIndices[j] > dest)
                {
                    UndoIndices[j]++;
                }
            }

            for (var j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > dest)
                {
                    TargetIndices[j]++;
                }
            }
        }

        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
            foreach (Entity c in SourceObjects)
            {
                Universe.Selection.AddSelection(c);
            }
        }

        return ActionEvent.NoEvent;
    }
}

public class ChangeEntityHierarchyAction : Action
{
    private readonly bool SetSelection;
    private readonly List<Entity> SourceObjects = new();
    private readonly List<int> TargetIndices = new();
    private readonly List<Entity> TargetObjects = new();
    private readonly Universe Universe;
    private int[] UndoIndices;
    private Entity[] UndoObjects;

    public ChangeEntityHierarchyAction(Universe univ, List<Entity> src, List<Entity> targetEnts, List<int> targets,
        bool setSelection)
    {
        Universe = univ;
        SourceObjects.AddRange(src);
        TargetObjects.AddRange(targetEnts);
        TargetIndices.AddRange(targets);
        SetSelection = setSelection;
    }

    public override ActionEvent Execute()
    {
        var sourceindices = new int[SourceObjects.Count];
        for (var i = 0; i < SourceObjects.Count; i++)
        {
            sourceindices[i] = -1;
            if (SourceObjects[i].Parent != null)
            {
                sourceindices[i] = SourceObjects[i].Parent.ChildIndex(SourceObjects[i]);
            }
        }

        for (var i = 0; i < sourceindices.Length; i++)
        {
            // Remove object and update indices
            var src = sourceindices[i];
            if (src != -1)
            {
                SourceObjects[i].Parent.RemoveChild(SourceObjects[i]);
                for (var j = 0; j < sourceindices.Length; j++)
                {
                    if (SourceObjects[i].Parent == SourceObjects[j].Parent && sourceindices[j] > src)
                    {
                        sourceindices[j]--;
                    }
                }

                for (var j = 0; j < TargetIndices.Count; j++)
                {
                    if (SourceObjects[i].Parent == TargetObjects[j] && TargetIndices[j] > src)
                    {
                        TargetIndices[j]--;
                    }
                }
            }

            // Add new object
            var dest = TargetIndices[i];
            TargetObjects[i].AddChild(SourceObjects[i], dest);
            for (var j = 0; j < sourceindices.Length; j++)
            {
                if (TargetObjects[i] == SourceObjects[j].Parent && sourceindices[j] > dest)
                {
                    sourceindices[j]++;
                }
            }

            for (var j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetObjects[i] == TargetObjects[j] && TargetIndices[j] > dest)
                {
                    TargetIndices[j]++;
                }
            }
        }

        UndoIndices = sourceindices;
        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
            foreach (Entity c in SourceObjects)
            {
                Universe.Selection.AddSelection(c);
            }
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        /*for (int i = 0; i < TargetIndices.Count; i++)
        {
            // Remove object and update indices
            int src = TargetIndices[i];
            Containers[i].Objects.RemoveAt(src);
            for (int j = 0; j < UndoIndices.Length; j++)
            {
                if (UndoIndices[j] > src)
                {
                    UndoIndices[j]--;
                }
            }
            for (int j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > src)
                {
                    TargetIndices[j]--;
                }
            }

            // Add new object
            int dest = UndoIndices[i];
            Containers[i].Objects.Insert(dest, SourceObjects[i]);
            for (int j = 0; j < UndoIndices.Length; j++)
            {
                if (UndoIndices[j] > dest)
                {
                    UndoIndices[j]++;
                }
            }
            for (int j = 0; j < TargetIndices.Count; j++)
            {
                if (TargetIndices[j] > dest)
                {
                    TargetIndices[j]++;
                }
            }
        }
        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
            foreach (var c in SourceObjects)
            {
                Universe.Selection.AddSelection(c);
            }
        }*/
        return ActionEvent.NoEvent;
    }
}

public class ChangeMapObjectType : Action
{
    private readonly List<MapEntity> Entities = new();
    private readonly List<MapObjectChange> MapObjectChanges = new();
    private readonly string MsbParamstr;
    private readonly Type MsbType;
    private readonly string[] NewTypes;
    private readonly string[] OldTypes;
    private readonly bool SetSelection;

    private readonly Universe Universe;

    /// <summary>
    ///     Change selected map objects from one type to another. Only works for map objects of the same overarching type, such
    ///     as Parts or Regions.
    ///     Data for properties absent in targeted type will be lost, but will be restored for undo/redo.
    /// </summary>
    public ChangeMapObjectType(Universe universe, Type msbType, List<MapEntity> selectedEnts, string[] oldTypes,
        string[] newTypes, string msbParamStr, bool setSelection)
    {
        Universe = universe;
        MsbType = msbType;
        Entities.AddRange(selectedEnts);
        OldTypes = oldTypes;
        NewTypes = newTypes;
        SetSelection = setSelection;
        MsbParamstr = msbParamStr;

        // Go through applicable map entities and create WrappedObject with the new type for each.
        // Store entity, old obj, and new obj to be used when changing Entity's WrappedObject (including restoring the exact same objs in cases of undo/redo).
        for (var iType = 0; iType < OldTypes.Length; iType++)
        {
            // Get desired types for the current game's MSB
            Type sourceType = MsbType.GetNestedType(MsbParamstr).GetNestedType(OldTypes[iType]);
            Type targetType = MsbType.GetNestedType(MsbParamstr).GetNestedType(NewTypes[iType]);
            Type partType = MsbType.GetNestedType(MsbParamstr);

            foreach (MapEntity ent in Entities)
            {
                Type currentType = ent.WrappedObject.GetType();
                if (currentType == sourceType)
                {
                    Map map = Universe.GetLoadedMap(ent.MapID);
                    map.HasUnsavedChanges = true;

                    var sourceObj = ent.WrappedObject;
                    var targetObj = targetType.GetConstructor(Type.EmptyTypes).Invoke(Array.Empty<object>());

                    // Go through properties of source type and set them to target type (if they exist under the same name)
                    // Public set properties
                    foreach (PropertyInfo property in sourceType.GetProperties().Where(p => p.CanWrite))
                    {
                        // Make sure target type has this property (DummyAssets are missing some properties)
                        PropertyInfo targetProp = targetObj.GetType().GetProperty(property.Name);
                        if (targetProp != null)
                        {
                            targetProp.SetValue(targetObj, property.GetValue(sourceObj, null), null);
                        }
                    }

                    // Private set properties
                    foreach (PropertyInfo property in sourceType.GetProperties().Where(p => !p.CanWrite))
                    {
                        // Make sure target type has this property (DummyAssets are missing some properties)
                        PropertyInfo targetProp = targetObj.GetType().GetProperty(property.Name);
                        if (targetProp != null)
                        {
                            PropertyInfo targetPropInner = targetProp.DeclaringType.GetProperty(property.Name);
                            targetPropInner.SetValue(targetObj, property.GetValue(sourceObj, null),
                                BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);
                        }
                    }

                    MapObjectChanges.Add(new MapObjectChange(sourceObj, targetObj, ent));
                }
            }
        }
    }

    public override ActionEvent Execute()
    {
        foreach (MapObjectChange mapChangeObj in MapObjectChanges)
        {
            // Assign new dummied/undummied wrappedObj to entity
            mapChangeObj.Entity.WrappedObject = mapChangeObj.NewObject;
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        foreach (MapObjectChange mapChangeObj in MapObjectChanges)
        {
            // Restore old, stored WrappedObject to entity.
            mapChangeObj.Entity.WrappedObject = mapChangeObj.OldObject;
        }

        if (SetSelection)
        {
            Universe.Selection.ClearSelection();
            foreach (MapEntity ent in Entities)
            {
                Universe.Selection.AddSelection(ent);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    private record MapObjectChange(object OldObject, object NewObject, MapEntity Entity);
}

public class CompoundAction : Action
{
    private readonly List<Action> Actions;

    private Action<bool> PostExecutionAction;

    public CompoundAction(List<Action> actions)
    {
        Actions = actions;
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute()
    {
        var evt = ActionEvent.NoEvent;
        foreach (Action act in Actions)
        {
            if (act != null)
            {
                evt |= act.Execute();
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(false);
        }

        return evt;
    }

    public override ActionEvent Undo()
    {
        var evt = ActionEvent.NoEvent;
        foreach (Action act in Actions)
        {
            if (act != null)
            {
                evt |= act.Undo();
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return evt;
    }
}
