using Andre.Formats;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.MapEditor.Framework.MassEdit;
using StudioCore.Platform;
using StudioCore.Scene;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor.Actions.Viewport;

/// <summary>
///     An action that can be performed by the user in the editor that represents
///     a single atomic editor action that affects the state of the map. Each action
///     should have enough information to apply the action AND undo the action, as
///     these actions get pushed to a stack for undo/redo
/// </summary>
public abstract class ViewportAction
{
    public abstract ActionEvent Execute(bool isRedo = false);
    public abstract ActionEvent Undo();
    public abstract string GetEditMessage();
}

public class PropertiesChangedAction : ViewportAction
{
    private readonly object ChangedObject;
    private readonly List<PropertyChange> Changes = new();
    private Action<bool> PostExecutionAction;

    private string EditMessage = "";

    public PropertiesChangedAction(object changed)
    {
        ChangedObject = changed;
    }

    public PropertiesChangedAction(PropertyInfo prop, object changed, object newval, string entityName = "")
    {
        ChangedObject = changed;
        var change = new PropertyChange();
        change.Property = prop;
        change.OldValue = prop.GetValue(ChangedObject);
        change.NewValue = newval;
        change.ArrayIndex = -1;
        Changes.Add(change);

        EditMessage = $"{entityName} -> {prop.Name} was changed to {change.NewValue}";
    }

    public PropertiesChangedAction(PropertyInfo prop, int index, object changed, object newval, string entityName = "")
    {
        ChangedObject = changed;
        var change = new PropertyChange();
        change.Property = prop;

        var value = change.Property.GetValue(ChangedObject);
        if (index != -1 && prop.PropertyType.IsArray)
        {
            var a = (Array)value;
            change.OldValue = a.GetValue(index);
        }
        else if (value != null &&
            value.GetType().IsGenericType &&
            index != -1 && value is IList list)
        {
            change.OldValue = list[index];
        }
        else
        {
            change.OldValue = prop.GetValue(ChangedObject);
        }

        change.NewValue = newval;
        change.ArrayIndex = index;
        Changes.Add(change);

        EditMessage = $"{entityName} -> {prop.Name}[{change.ArrayIndex} was changed to {change.NewValue}";
    }

    public void AddPropertyChange(PropertyInfo prop, object newval, int index = -1)
    {
        var change = new PropertyChange();
        change.Property = prop;

        var value = change.Property.GetValue(ChangedObject);

        if (index != -1 && prop.PropertyType.IsArray)
        {
            var a = (Array)value;
            change.OldValue = a.GetValue(index);
        }
        else if (value != null &&
            value.GetType().IsGenericType &&
            index != -1 && value is IList list)
        {
            change.OldValue = list[index];
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

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (PropertyChange change in Changes)
        {
            var value = change.Property.GetValue(ChangedObject);

            Type valType = null;

            if (value != null)
                valType = value.GetType();

            if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
            {
                var a = (Array)value;
                a.SetValue(change.NewValue, change.ArrayIndex);
            }
            else if (value != null && valType != null && valType.IsGenericType && change.ArrayIndex != -1 && value is IList list)
            {
                list[change.ArrayIndex] = change.NewValue;
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
            var value = change.Property.GetValue(ChangedObject);

            Type valType = null;

            if (value != null)
                valType = value.GetType();

            if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
            {
                var a = (Array)value;
                a.SetValue(change.OldValue, change.ArrayIndex);
            }
            else if (value != null && valType != null && valType.IsGenericType && change.ArrayIndex != -1 && value is IList list)
            {
                list[change.ArrayIndex] = change.OldValue;
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

    public override string GetEditMessage()
    {
        return EditMessage;
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
public class ArrayPropertyCopyAction : ViewportAction
{
    private readonly List<PropertyChange> Changes = new();
    private Action<bool> PostExecutionAction;

    public ArrayPropertyCopyAction(Array source, Array target)
    {
        for (var i = 0; i < target.Length; i++)
        {
            PropertyChange change = new()
            {
                ChangedObj = target,
                OldVal = target.GetValue(i),
                NewVal = source.GetValue(i),
                ArrayIndex = i
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
                    ChangedObj = target,
                    OldVal = target.GetValue(i),
                    NewVal = source.GetValue(i),
                    ArrayIndex = i
                };
                Changes.Add(change);
            }
        }
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute(bool isRedo = false)
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

    public override string GetEditMessage()
    {
        return "";
    }

    private class PropertyChange
    {
        public int ArrayIndex;
        public Array ChangedObj;
        public object NewVal;
        public object OldVal;
    }
}

public class MultipleEntityPropertyChangeAction : ViewportAction
{
    private MapEditorScreen Editor;

    private readonly HashSet<Entity> ChangedEnts = new();
    private readonly List<PropertyChange> Changes = new();

    public bool UpdateRenderModel = false;
    public bool ClearName { get; set; }

    public MultipleEntityPropertyChangeAction(MapEditorScreen editor, PropertyInfo prop, HashSet<Entity> changedEnts, object newval,
        int index = -1, int classIndex = -1, bool clearName = true)
    {
        Editor = editor;

        ClearName = clearName;
        ChangedEnts = changedEnts;
        foreach (Entity o in changedEnts)
        {
            var propObj = PropFinderUtil.FindPropertyObject(prop, o.WrappedObject, index, classIndex, false);
            if (propObj != null)
            {
                var change = new PropertyChange
                {
                    ChangedObj = propObj,
                    Property = prop,
                    NewValue = newval,
                    ArrayIndex = index
                };

                var value = change.Property.GetValue(propObj);

                Type valType = null;

                if (value != null)
                {
                    valType = value.GetType();
                }

                if (index != -1 && prop.PropertyType.IsArray)
                {
                    var a = (Array)value;
                    change.OldValue = a.GetValue(index);
                }
                else if (value != null && valType != null && valType.IsGenericType && index != -1 && value is IList list)
                {
                    change.OldValue = list[index];
                }
                else
                {
                    change.OldValue = prop.GetValue(propObj);
                }

                Changes.Add(change);
            }
        }
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (PropertyChange change in Changes)
        {
            var value = change.Property.GetValue(change.ChangedObj);

            Type valType = null;

            if (value != null)
            {
                valType = value.GetType();
            }

            if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
            {
                var a = (Array)value;
                a.SetValue(change.NewValue, change.ArrayIndex);
            }
            else if (value != null && valType != null && valType.IsGenericType && change.ArrayIndex != -1 && value is IList list)
            {
                list[change.ArrayIndex] = change.NewValue;
            }
            else
            {
                change.Property.SetValue(change.ChangedObj, change.NewValue);
            }
        }

        if (ClearName)
        {
            foreach (Entity e in ChangedEnts)
            {
                if (UpdateRenderModel)
                {
                    e.UpdateRenderModel(Editor);
                }

                // Clear name cache, forcing it to update.
                e.Name = null;
            }
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (PropertyChange change in Changes)
        {
            var value = change.Property.GetValue(change.ChangedObj);

            Type valType = null;

            if (value != null)
            {
                valType = value.GetType();
            }

            if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
            {
                var a = (Array)value;
                a.SetValue(change.OldValue, change.ArrayIndex);
            }
            else if (value != null && valType != null && valType.IsGenericType && change.ArrayIndex != -1 && value is IList list)
            {
                list[change.ArrayIndex] = change.OldValue;
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
                e.UpdateRenderModel(Editor);
            }

            // Clear name cache, forcing it to update.
            if (ClearName)
                e.Name = null;
        }

        return ActionEvent.NoEvent;
    }

    public override string GetEditMessage()
    {
        return "";
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

public class CloneMapObjectsAction : ViewportAction
{
    private MapEditorScreen Editor;

    private static readonly Regex TrailIDRegex = new(@"_(?<id>\d+)$");
    private readonly List<MsbEntity> Clonables = new();
    private readonly List<ObjectContainer> CloneMaps = new();
    private readonly List<MsbEntity> Clones = new();
    private readonly bool SetSelection;
    private readonly Entity TargetBTL;
    private readonly MapContainer TargetMap;

    public CloneMapObjectsAction(MapEditorScreen editor, List<MsbEntity> objects, bool setSelection,
        MapContainer targetMap = null, Entity targetBTL = null)
    {
        Editor = editor;
        Clonables.AddRange(objects);
        SetSelection = setSelection;
        TargetMap = targetMap;
        TargetBTL = targetBTL;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

        var clonesCached = Clones.Count() > 0;

        var objectnames = new Dictionary<string, HashSet<string>>();
        Dictionary<MapContainer, HashSet<MsbEntity>> mapPartEntities = new();

        for (var i = 0; i < Clonables.Count(); i++)
        {
            if (Clonables[i].MapID == null)
            {
                TaskLogs.AddLog($"Failed to dupe {Clonables[i].Name}, as it had no defined MapID",
                    LogLevel.Warning);
                continue;
            }

            MapContainer m;
            if (TargetMap != null)
            {
                m = Editor.GetMapContainerFromMapID(TargetMap.Name);
            }
            else
            {
                m = Editor.GetMapContainerFromMapID(Clonables[i].MapID);
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
                MsbEntity newobj = clonesCached ? Clones[i] : (MsbEntity)Clonables[i].Clone();

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

                if (TargetMap == null)
                {
                    m.Objects.Insert(m.Objects.IndexOf(Clonables[i]) + 1, newobj);
                }
                else
                {
                    m.Objects.Add(newobj);
                }

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

                if (CFG.Current.Toolbar_Duplicate_Increment_Entity_ID)
                {
                    ViewportActionCommon.SetUniqueEntityID(Editor, newobj, m);
                }
                if (CFG.Current.Toolbar_Duplicate_Increment_InstanceID)
                {
                    ViewportActionCommon.SetUniqueInstanceID(Editor, newobj, m);
                }
                if (CFG.Current.Toolbar_Duplicate_Increment_PartNames)
                {
                    ViewportActionCommon.SetSelfPartNames(Editor, newobj, m);
                }
                if (CFG.Current.Toolbar_Duplicate_Clear_Entity_ID)
                {
                    ViewportActionCommon.ClearEntityID(Editor, newobj, m);
                }
                if (CFG.Current.Toolbar_Duplicate_Clear_Entity_Group_IDs)
                {
                    ViewportActionCommon.ClearEntityGroupID(Editor, newobj, m);
                }

                newobj.UpdateRenderModel(Editor);
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
            universe.Selection.ClearSelection(Editor);
            foreach (MsbEntity c in Clones)
            {
                universe.Selection.AddSelection(Editor, c);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = Editor.Universe;

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
            universe.Selection.ClearSelection(Editor);
            foreach (MsbEntity c in Clonables)
            {
                universe.Selection.AddSelection(Editor, c);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}

public class AddMapObjectsAction : ViewportAction
{
    private MapEditorScreen Editor;

    private static Regex TrailIDRegex = new(@"_(?<id>\d+)$");
    private readonly List<MsbEntity> Added = new();
    private readonly List<ObjectContainer> AddedMaps = new();
    private readonly MapContainer Map;
    private readonly Entity Parent;
    private readonly bool SetSelection;
    private MapContainer TargetMap;

    public AddMapObjectsAction(MapEditorScreen editor, MapContainer map, List<MsbEntity> objects,
        bool setSelection, Entity parent, MapContainer targetMap = null)
    {
        Editor = editor;
        Map = map;
        Added.AddRange(objects);
        SetSelection = setSelection;
        Parent = parent;
        TargetMap = targetMap;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

        for (var i = 0; i < Added.Count(); i++)
        {
            if (Map != null)
            {
                Map.Objects.Add(Added[i]);
                Parent.AddChild(Added[i]);
                Added[i].UpdateRenderModel(Editor);
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
                    m = Editor.GetMapContainerFromMapID(TargetMap.Name);

                    // Prefab-specific
                    if (CFG.Current.Prefab_ApplyUniqueInstanceID)
                    {
                        ViewportActionCommon.SetUniqueInstanceID(Editor, ent, m);
                    }
                    if (CFG.Current.Prefab_ApplyUniqueEntityID)
                    {
                        ViewportActionCommon.SetUniqueEntityID(Editor, ent, m);
                    }
                    if (CFG.Current.Prefab_ApplySpecificEntityGroupID)
                    {
                        ViewportActionCommon.SetSpecificEntityGroupID(Editor, ent, m);
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
            universe.Selection.ClearSelection(Editor);
            foreach (MsbEntity c in Added)
            {
                universe.Selection.AddSelection(Editor, c);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = Editor.Universe;

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
            universe.Selection.ClearSelection(Editor);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}

/// <summary>
///     Deprecated
/// </summary>
[Obsolete]
public class AddParamsAction : ViewportAction
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

    public override ActionEvent Execute(bool isRedo = false)
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

    public override string GetEditMessage()
    {
        return "";
    }
}

public class DeleteMapObjectsAction : ViewportAction
{
    private MapEditorScreen Editor;

    private readonly List<MsbEntity> Deletables = new();
    private readonly List<int> RemoveIndices = new();
    private readonly List<ObjectContainer> RemoveMaps = new();
    private readonly List<MsbEntity> RemoveParent = new();
    private readonly List<int> RemoveParentIndex = new();
    private readonly bool SetSelection;
    private RenderScene Scene;

    public DeleteMapObjectsAction(MapEditorScreen editor, List<MsbEntity> objects, bool setSelection)
    {
        Editor = editor;
        Deletables.AddRange(objects);
        SetSelection = setSelection;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

        foreach (MsbEntity obj in Deletables)
        {
            MapContainer m = Editor.GetMapContainerFromMapID(obj.MapID);
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

                RemoveParent.Add((MsbEntity)obj.Parent);
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
            universe.Selection.ClearSelection(Editor);
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = Editor.Universe;

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
            universe.Selection.ClearSelection(Editor);
            foreach (MsbEntity d in Deletables)
            {
                universe.Selection.AddSelection(Editor, d);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}

public class ReorderContainerObjectsAction : ViewportAction
{
    private MapEditorScreen Editor;
    private readonly List<ObjectContainer> Containers = new();
    private readonly bool SetSelection;
    private readonly List<Entity> SourceObjects = new();
    private readonly List<int> TargetIndices = new();
    private int[] UndoIndices;

    public ReorderContainerObjectsAction(MapEditorScreen editor, List<Entity> src, List<int> targets, bool setSelection)
    {
        Editor = editor;
        SourceObjects.AddRange(src);
        TargetIndices.AddRange(targets);
        SetSelection = setSelection;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

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
            universe.Selection.ClearSelection(Editor);
            foreach (Entity c in SourceObjects)
            {
                universe.Selection.AddSelection(Editor, c);
            }
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        var universe = Editor.Universe;

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
            universe.Selection.ClearSelection(Editor);
            foreach (Entity c in SourceObjects)
            {
                universe.Selection.AddSelection(Editor, c);
            }
        }

        return ActionEvent.NoEvent;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}

public class ChangeEntityHierarchyAction : ViewportAction
{
    private MapEditorScreen Editor;

    private readonly bool SetSelection;
    private readonly List<Entity> SourceObjects = new();
    private readonly List<int> TargetIndices = new();
    private readonly List<Entity> TargetObjects = new();
    private int[] UndoIndices;
    private Entity[] UndoObjects;

    public ChangeEntityHierarchyAction(MapEditorScreen editor, List<Entity> src, List<Entity> targetEnts, List<int> targets,
        bool setSelection)
    {
        Editor = editor;

        SourceObjects.AddRange(src);
        TargetObjects.AddRange(targetEnts);
        TargetIndices.AddRange(targets);
        SetSelection = setSelection;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

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
            universe.Selection.ClearSelection(Editor);
            foreach (Entity c in SourceObjects)
            {
                universe.Selection.AddSelection(Editor, c);
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

    public override string GetEditMessage()
    {
        return "";
    }
}

public class ChangeMapObjectType : ViewportAction
{
    private MapEditorScreen Editor;
    private readonly List<MsbEntity> Entities = new();
    private readonly List<MapObjectChange> MapObjectChanges = new();
    private readonly string MsbParamstr;
    private readonly Type MsbType;
    private readonly string[] NewTypes;
    private readonly string[] OldTypes;
    private readonly bool SetSelection;

    /// <summary>
    ///     Change selected map objects from one type to another. Only works for map objects of the same overarching type, such
    ///     as Parts or Regions.
    ///     Data for properties absent in targeted type will be lost, but will be restored for undo/redo.
    /// </summary>
    public ChangeMapObjectType(MapEditorScreen editor, Type msbType, List<MsbEntity> selectedEnts, string[] oldTypes,
        string[] newTypes, string msbParamStr, bool setSelection)
    {
        Editor = editor;
        MsbType = msbType;
        Entities.AddRange(selectedEnts);
        OldTypes = oldTypes;
        NewTypes = newTypes;
        SetSelection = setSelection;
        MsbParamstr = msbParamStr;

        var universe = Editor.Universe;

        // Go through applicable map entities and create WrappedObject with the new type for each.
        // Store entity, old obj, and new obj to be used when changing Entity's WrappedObject (including restoring the exact same objs in cases of undo/redo).
        for (var iType = 0; iType < OldTypes.Length; iType++)
        {
            // Get desired types for the current game's MSB
            Type sourceType = MsbType.GetNestedType(MsbParamstr).GetNestedType(OldTypes[iType]);
            Type targetType = MsbType.GetNestedType(MsbParamstr).GetNestedType(NewTypes[iType]);
            Type partType = MsbType.GetNestedType(MsbParamstr);

            foreach (MsbEntity ent in Entities)
            {
                Type currentType = ent.WrappedObject.GetType();
                if (currentType == sourceType)
                {
                    MapContainer map = Editor.GetMapContainerFromMapID(ent.MapID);
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

    public override ActionEvent Execute(bool isRedo = false)
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
        var universe = Editor.Universe;

        foreach (MapObjectChange mapChangeObj in MapObjectChanges)
        {
            // Restore old, stored WrappedObject to entity.
            mapChangeObj.Entity.WrappedObject = mapChangeObj.OldObject;
        }

        if (SetSelection)
        {
            universe.Selection.ClearSelection(Editor);
            foreach (MsbEntity ent in Entities)
            {
                universe.Selection.AddSelection(Editor, ent);
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    private record MapObjectChange(object OldObject, object NewObject, MsbEntity Entity);

    public override string GetEditMessage()
    {
        return "";
    }
}

public class CompoundAction : ViewportAction
{
    private readonly List<ViewportAction> Actions;

    private Action<bool> PostExecutionAction;

    public CompoundAction(List<ViewportAction> actions)
    {
        Actions = actions;
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var evt = ActionEvent.NoEvent;
        foreach (ViewportAction act in Actions)
        {
            if (act != null)
            {
                evt |= act.Execute(isRedo);
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
        foreach (ViewportAction act in Actions)
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

    public override string GetEditMessage()
    {
        return "";
    }
}


public class MapActionGroupCompoundAction : ViewportAction
{
    private MapEditorScreen Editor;
    private readonly List<MapActionGroup> Actions;

    private Action<bool> PostExecutionAction;

    public MapActionGroupCompoundAction(MapEditorScreen editor, List<MapActionGroup> actions)
    {
        Editor = editor;
        Actions = actions;
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var evt = ActionEvent.NoEvent;

        foreach (var group in Actions)
        {
            var name = group.MapID;
            var alias = AliasUtils.GetMapNameAlias(Editor.Project, name);
            if (alias != null)
                name = $"{name} {alias}";

            var tAction = group.Actions;

            foreach (ViewportAction act in tAction)
            {
                if (act != null)
                {
                    evt |= act.Execute(isRedo);
                }
            }
            
            TaskLogs.AddLog($"Applied MSB mass edit affecting {tAction.Count} map objects for {name}");
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

        foreach (var group in Actions)
        {
            var name = group.MapID;
            var alias = AliasUtils.GetMapNameAlias(Editor.Project, name);
            if (alias != null)
                name = $"{name} {alias}";

            var tAction = group.Actions;

            foreach (ViewportAction act in tAction)
            {
                if (act != null)
                {
                    evt |= act.Undo();
                }
            }

            TaskLogs.AddLog($"Reverted MSB mass edit affecting {tAction.Count} map objects for {name}");
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        Editor.MassEditHandler.EditLog.ClearLogSource();

        return evt;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}

public class ReplicateMapObjectsAction : ViewportAction
{
    private static readonly Regex TrailIDRegex = new(@"_(?<id>\d+)$");
    private readonly List<MsbEntity> Clonables = new();
    private readonly List<ObjectContainer> CloneMaps = new();
    private readonly List<MsbEntity> Clones = new();
    private MapEditorScreen Editor;

    private int idxCache;

    private int iterationCount;
    private float squareTopCount;
    private float squareRightCount;
    private float squareLeftCount;
    private float squareBottomCount;

    public enum SquareSide
    {
        Top,
        Left,
        Right,
        Bottom
    }

    private SquareSide currentSquareSide;

    public ReplicateMapObjectsAction(MapEditorScreen editor, List<MsbEntity> objects)
    {
        Editor = editor;
        Clonables.AddRange(objects);
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        if (isRedo)
        {
            Editor.EditorActionManager.Clear();

            return ActionEvent.NoEvent;
        }

        idxCache = -1;

        if (CFG.Current.Replicator_Mode_Line)
            iterationCount = CFG.Current.Replicator_Line_Clone_Amount;

        if (CFG.Current.Replicator_Mode_Circle)
            iterationCount = CFG.Current.Replicator_Circle_Size;

        if (CFG.Current.Replicator_Mode_Square)
        {
            iterationCount = CFG.Current.Replicator_Square_Size * 4 - 1;
            currentSquareSide = SquareSide.Bottom;

            squareTopCount = CFG.Current.Replicator_Square_Size;
            squareLeftCount = CFG.Current.Replicator_Square_Size - 1;
            squareRightCount = CFG.Current.Replicator_Square_Size;
            squareBottomCount = CFG.Current.Replicator_Square_Size;
        }

        var objectnames = new Dictionary<string, HashSet<string>>();
        Dictionary<MapContainer, HashSet<MsbEntity>> mapPartEntities = new();

        var clonesCached = Clones.Count() > 0;

        for (var k = 0; k < iterationCount; k++)
        {
            for (var i = 0; i < Clonables.Count(); i++)
            {
                if (Clonables[i].MapID == null)
                {
                    TaskLogs.AddLog($"Failed to dupe {Clonables[i].Name}, as it had no defined MapID",
                        LogLevel.Warning);
                    continue;
                }

                MapContainer m;
                m = Editor.GetMapContainerFromMapID(Clonables[i].MapID);

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
                    MsbEntity newobj = clonesCached ? Clones[i] : (MsbEntity)Clonables[i].Clone();

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

                    if (idxCache == -1)
                    {
                        idxCache = m.Objects.IndexOf(Clonables[i]) + 1;
                    }
                    else
                    {
                        idxCache = idxCache + 1;
                    }

                    m.Objects.Insert(idxCache, newobj);

                    if (Clonables[i].Parent != null)
                    {
                        var idx = Clonables[i].Parent.ChildIndex(Clonables[i]);
                        Clonables[i].Parent.AddChild(newobj, idx + 1);
                    }

                    // Apply transform changes
                    ApplyReplicateTransform(newobj, k);
                    ApplyScrambleTransform(newobj);

                    // Apply other property changes
                    if (CFG.Current.Replicator_Increment_Entity_ID)
                    {
                        ViewportActionCommon.SetUniqueEntityID(Editor, newobj, m);
                    }
                    if (CFG.Current.Replicator_Increment_InstanceID)
                    {
                        ViewportActionCommon.SetUniqueInstanceID(Editor, newobj, m);
                    }
                    if (CFG.Current.Replicator_Increment_PartNames)
                    {
                        ViewportActionCommon.SetSelfPartNames(Editor, newobj, m);
                    }
                    if (CFG.Current.Replicator_Clear_Entity_ID)
                    {
                        ViewportActionCommon.ClearEntityID(Editor, newobj, m);
                    }
                    if (CFG.Current.Replicator_Clear_Entity_Group_IDs)
                    {
                        ViewportActionCommon.ClearEntityGroupID(Editor, newobj, m);
                    }

                    newobj.UpdateRenderModel(Editor);
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
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    private void ApplyReplicateTransform(MsbEntity sel, int iteration)
    {
        if (sel.WrappedObject is not IMsbPart or IMsbRegion)
            return;

        Transform objT = sel.GetLocalTransform();

        var newTransform = Transform.Default;
        var newPos = objT.Position;
        var newRot = objT.Rotation;
        var newScale = objT.Scale;

        if (CFG.Current.Replicator_Mode_Line)
        {
            var posOffset = CFG.Current.Replicator_Line_Position_Offset * (1 + iteration);

            if (CFG.Current.Replicator_Line_Offset_Direction_Flipped)
            {
                posOffset = posOffset * -1;
            }

            if (CFG.Current.Replicator_Line_Position_Offset_Axis_X)
            {
                newPos = new Vector3(newPos[0] + posOffset, newPos[1], newPos[2]);
            }

            if (CFG.Current.Replicator_Line_Position_Offset_Axis_Y)
            {
                newPos = new Vector3(newPos[0], newPos[1] + posOffset, newPos[2]);
            }

            if (CFG.Current.Replicator_Line_Position_Offset_Axis_Z)
            {
                newPos = new Vector3(newPos[0], newPos[1], newPos[2] + posOffset);
            }
        }

        if (CFG.Current.Replicator_Mode_Circle)
        {
            double angleIncrement = 360 / CFG.Current.Replicator_Circle_Size;

            double radius = CFG.Current.Replicator_Circle_Radius;
            double angle = angleIncrement * iteration;
            double rad = angle * (Math.PI / 180);

            double x = radius * Math.Cos(rad) * 180 / Math.PI;
            double z = radius * Math.Sin(rad) * 180 / Math.PI;

            newPos = new Vector3(newPos[0] + (float)x, newPos[1], newPos[2] + (float)z);
        }

        if (CFG.Current.Replicator_Mode_Square)
        {
            if (currentSquareSide == SquareSide.Bottom && squareBottomCount <= 0)
            {
                currentSquareSide = SquareSide.Left;
            }
            else if (currentSquareSide == SquareSide.Left && squareLeftCount <= 0)
            {
                currentSquareSide = SquareSide.Top;
            }
            else if (currentSquareSide == SquareSide.Top && squareTopCount <= 0)
            {
                currentSquareSide = SquareSide.Right;
            }
            else if (currentSquareSide == SquareSide.Right && squareRightCount <= 0)
            {
            }

            // Bottom
            if (currentSquareSide == SquareSide.Bottom)
            {
                float width_increment = CFG.Current.Replicator_Square_Width / CFG.Current.Replicator_Square_Size * squareBottomCount;
                float x = newPos[0] - width_increment;

                newPos = new Vector3(x, newPos[1], newPos[2]);

                squareBottomCount--;
            }

            // Left
            if (currentSquareSide == SquareSide.Left)
            {
                float width_increment = CFG.Current.Replicator_Square_Width;
                float x = newPos[0] - width_increment;

                float height_increment = CFG.Current.Replicator_Square_Depth / CFG.Current.Replicator_Square_Size * squareLeftCount;
                float z = newPos[2] + height_increment;

                newPos = new Vector3(x, newPos[1], z);

                squareLeftCount--;
            }

            // Top
            if (currentSquareSide == SquareSide.Top)
            {
                float width_increment = CFG.Current.Replicator_Square_Width / CFG.Current.Replicator_Square_Size * squareTopCount;
                float x = newPos[0] - width_increment;

                float height_increment = CFG.Current.Replicator_Square_Depth;
                float z = newPos[2] + height_increment;

                newPos = new Vector3(x, newPos[1], z);

                squareTopCount--;
            }

            // Right
            if (currentSquareSide == SquareSide.Right)
            {
                float height_increment = CFG.Current.Replicator_Square_Depth / CFG.Current.Replicator_Square_Size * squareRightCount;
                float z = newPos[2] + height_increment;

                newPos = new Vector3(newPos[0], newPos[1], z);

                squareRightCount--;
            }
        }

        if (CFG.Current.Replicator_Mode_Sphere)
        {
            double angleIncrement = 360 / CFG.Current.Replicator_Circle_Size;

            double radius = CFG.Current.Replicator_Circle_Radius;
            double angle = angleIncrement * iteration;
            double rad = angle * (Math.PI / 180);

            double x = radius * Math.Cos(rad) * 180 / Math.PI;
            double z = radius * Math.Sin(rad) * 180 / Math.PI;

            newPos = new Vector3(newPos[0] + (float)x, newPos[1], newPos[2] + (float)z);
        }

        newTransform.Position = newPos;
        newTransform.Rotation = newRot;
        newTransform.Scale = newScale;

        if (Editor.Project.ProjectType == ProjectType.DS2S || Editor.Project.ProjectType == ProjectType.DS2)
        {
            if (sel.Type == MsbEntityType.DS2Generator &&
                sel.WrappedObject is MergedParamRow mp)
            {
                Param.Row loc = mp.GetRow("generator-loc");
                loc.GetCellHandleOrThrow("PositionX").Value = newTransform.Position[0];
                loc.GetCellHandleOrThrow("PositionY").Value = newTransform.Position[1];
                loc.GetCellHandleOrThrow("PositionZ").Value = newTransform.Position[2];
                loc.GetCellHandleOrThrow("RotationX").Value = (float)(newTransform.Rotation[0] * (180 / Math.PI));
                loc.GetCellHandleOrThrow("RotationY").Value = (float)(newTransform.Rotation[1] * (180 / Math.PI));
                loc.GetCellHandleOrThrow("RotationZ").Value = (float)(newTransform.Rotation[2] * (180 / Math.PI));
            }
        }
        else
        {
            sel.SetPropertyValue("Position", newPos);
        }
    }

    public void ApplyScrambleTransform(MsbEntity newobj)
    {
        if (CFG.Current.Replicator_Apply_Scramble_Configuration)
        {
            Transform scrambledTransform = Editor.ActionHandler.GetScrambledTransform(newobj);

            if (Editor.Project.ProjectType == ProjectType.DS2S || Editor.Project.ProjectType == ProjectType.DS2)
            {
                if (newobj.Type == MsbEntityType.DS2Generator &&
                newobj.WrappedObject is MergedParamRow mp)
                {
                    Param.Row loc = mp.GetRow("generator-loc");
                    loc.GetCellHandleOrThrow("PositionX").Value = scrambledTransform.Position[0];
                    loc.GetCellHandleOrThrow("PositionY").Value = scrambledTransform.Position[1];
                    loc.GetCellHandleOrThrow("PositionZ").Value = scrambledTransform.Position[2];
                    loc.GetCellHandleOrThrow("RotationX").Value = (float)(scrambledTransform.Rotation[0] * (180 / Math.PI));
                    loc.GetCellHandleOrThrow("RotationY").Value = (float)(scrambledTransform.Rotation[1] * (180 / Math.PI));
                    loc.GetCellHandleOrThrow("RotationZ").Value = (float)(scrambledTransform.Rotation[2] * (180 / Math.PI));
                }
            }
            else
            {
                newobj.SetPropertyValue("Position", scrambledTransform.Position);

                if (newobj.IsRotationPropertyRadians("Rotation"))
                {
                    if (newobj.IsRotationXZY("Rotation"))
                    {
                        newobj.SetPropertyValue("Rotation", scrambledTransform.EulerRotationXZY);
                    }
                    else
                    {
                        newobj.SetPropertyValue("Rotation", scrambledTransform.EulerRotation);
                    }
                }
                else
                {
                    if (newobj.IsRotationXZY("Rotation"))
                    {
                        newobj.SetPropertyValue("Rotation", scrambledTransform.EulerRotationXZY * Utils.Rad2Deg);
                    }
                    else
                    {
                        newobj.SetPropertyValue("Rotation", scrambledTransform.EulerRotation * Utils.Rad2Deg);
                    }

                    newobj.SetPropertyValue("Rotation", scrambledTransform.EulerRotation * Utils.Rad2Deg);
                }

                newobj.SetPropertyValue("Scale", scrambledTransform.Scale);

            }
        }
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Clones.Count(); i++)
        {
            CloneMaps[i].Objects.Remove(Clones[i]);
            if (Clones[i].Parent != null)
            {
                Clones[i].Parent.RemoveChild(Clones[i]);
            }

            if (Clones[i].RenderSceneMesh != null)
            {
                Clones[i].RenderSceneMesh.AutoRegister = false;
                Clones[i].RenderSceneMesh.UnregisterWithScene();
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}

public enum OrderMoveDir
{
    Up,
    Down,
    Top,
    Bottom
}

public class OrderMapObjectsAction : ViewportAction
{
    private MapEditorScreen Editor;

    private List<MsbEntity> selection = new();
    private List<Entity> storedObjectOrder = new();

    private OrderMoveDir MoveSelectionDir;

    public OrderMapObjectsAction(MapEditorScreen editor, List<MsbEntity> objects, OrderMoveDir moveDir)
    {
        Editor = editor;
        selection.AddRange(objects);

        MoveSelectionDir = moveDir;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var universe = Editor.Universe;

        // TODO: allow this to work with multi-selections
        // Will require more rigorous validation of the indices
        if (selection.Count > 1)
        {
            PlatformUtils.Instance.MessageBox("You can only order one map object at a time.", "Smithbox", MessageBoxButtons.OK);
        }
        else
        {
            // Ignore if no selection is present
            if (selection.Count > 0)
            {
                var curSel = selection.First();
                var ent = (Entity)curSel;

                // Ignore if the selection is a BTL.Light
                if (ent.WrappedObject is BTL.Light)
                    return ActionEvent.NoEvent;

                MapContainer mapRoot = Editor.GetMapContainerFromMapID(curSel.MapID);

                // Ignore usage if the selection is the map root itself
                if (mapRoot == null)
                {
                    return ActionEvent.ObjectAddedRemoved;
                }

                // Store previous object order for undo if needed
                foreach (var entry in mapRoot.Objects)
                {
                    storedObjectOrder.Add(entry);
                }

                // Get the 'sub-category' for this map object
                var classType = ent.WrappedObject.GetType();

                if (mapRoot != null)
                {
                    int indexStart = -1;
                    int indexEnd = -1;
                    bool isStart = false;
                    bool foundEnd = false;

                    // Find the sub-category start and end indices
                    for (int i = 0; i < mapRoot.Objects.Count; i++)
                    {
                        var curChild = mapRoot.Objects[i];

                        // Grab the end index for this sub-category (if start has already been found)
                        if (!foundEnd && isStart && curChild.WrappedObject.GetType() != classType)
                        {
                            foundEnd = true;
                            indexEnd = i - 1;
                        }

                        // Grab the start index for this sub-category
                        if (curChild.WrappedObject.GetType() == classType)
                        {
                            if (!isStart)
                            {
                                isStart = true;
                                indexStart = i;
                            }
                        }
                    }

                    // If we escape the previous loop without setting the indexEnd,
                    // then it must be the final index
                    if (indexEnd == -1)
                    {
                        indexEnd = mapRoot.Objects.Count - 1;
                    }

                    // Move Up
                    if (MoveSelectionDir == OrderMoveDir.Up)
                    {
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (curSel == mapRoot.Objects[i])
                            {
                                if (i < mapRoot.Objects.Count)
                                {
                                    if (i == indexStart)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        var prevEntry = mapRoot.Objects[i - 1];
                                        mapRoot.Objects[i - 1] = curSel;
                                        mapRoot.Objects[i] = prevEntry;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Move Down
                    if (MoveSelectionDir == OrderMoveDir.Down)
                    {
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (curSel == mapRoot.Objects[i])
                            {
                                if (i < mapRoot.Objects.Count)
                                {
                                    if (i == indexEnd)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        var nextEntry = mapRoot.Objects[i + 1];
                                        mapRoot.Objects[i + 1] = curSel;
                                        mapRoot.Objects[i] = nextEntry;
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // Move to Top
                    if (MoveSelectionDir == OrderMoveDir.Top)
                    {
                        int rootIndex = 0;

                        // Find the index for the moved selection
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (i < mapRoot.Objects.Count && mapRoot.Objects[i] == curSel)
                            {
                                rootIndex = i;
                            }
                        }

                        // For all entries above it, move them down
                        for (int i = rootIndex; i >= indexStart; i--)
                        {
                            if (i < mapRoot.Objects.Count && i > 0)
                            {
                                mapRoot.Objects[i] = mapRoot.Objects[i - 1];
                            }
                        }
                        // Set top index to selection
                        mapRoot.Objects[indexStart] = curSel;
                    }

                    // Move to Bottom
                    if (MoveSelectionDir == OrderMoveDir.Bottom)
                    {
                        int rootIndex = 0;

                        // Find the index for the moved selection
                        for (int i = indexStart; i <= indexEnd; i++)
                        {
                            if (i < mapRoot.Objects.Count && mapRoot.Objects[i] == curSel)
                            {
                                rootIndex = i;
                            }
                        }

                        // For all entries below it, move them up
                        for (int i = rootIndex; i <= indexEnd; i++)
                        {
                            mapRoot.Objects[i] = mapRoot.Objects[i + 1];
                        }
                        // Set top index to selection
                        mapRoot.Objects[indexEnd] = curSel;
                    }
                }
            }
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override ActionEvent Undo()
    {
        var universe = Editor.Universe;

        if (selection.Count > 0)
        {
            var curSel = selection.First();

            MapContainer mapRoot = Editor.GetMapContainerFromMapID(curSel.MapID);
            mapRoot.Objects = storedObjectOrder;
        }

        return ActionEvent.ObjectAddedRemoved;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}

public class RenameObjectsAction(List<MsbEntity> entities, List<string> newNames, bool reference) : ViewportAction
{
    List<string> oldNames = entities.Select(e => e.Name).ToList();

    void Rename(MsbEntity entity, string name)
    {
        if (reference)
        {
            ViewportActionCommon.SetNameHandleDuplicate(
                entity.ContainingMap,
                entity.ContainingMap.Objects
                    .Where(e => e.WrappedObject is IMsbEntry)
                    .Select(e => e as MsbEntity),
                entity,
                name
            );
        }
        else
        {
            entity.Name = name;
        }
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var (entity, name) in entities.Zip(newNames))
        {
            Rename(entity, name);
        }
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var (entity, name) in entities.Zip(oldNames))
        {
            Rename(entity, name);
        }
        return ActionEvent.NoEvent;
    }

    public override string GetEditMessage()
    {
        return "";
    }
}