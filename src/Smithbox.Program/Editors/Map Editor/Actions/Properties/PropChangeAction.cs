using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;


public class PropChangeAction : ViewportAction
{
    private readonly object ChangedObject;
    private readonly List<PropertyChange> Changes = new();
    private Action<bool> PostExecutionAction;

    private Entity TargetEnt;

    private LightAtlasEdit AtlasEdit;

    public PropChangeAction(object changed)
    {
        ChangedObject = changed;
    }

    /// <summary>
    /// Used to build up the changes with AddPropertyChange, for an object belonging to the entity
    /// </summary>
    public PropChangeAction(Entity ent, object changed)
    {
        TargetEnt = ent;
        ChangedObject = changed;
    }

    public PropChangeAction(Entity ent, PropertyInfo prop, object changed, object newval, string entityName = "")
    {
        TargetEnt = ent;
        ChangedObject = changed;
        var change = new PropertyChange();
        change.Property = prop;
        change.OldValue = prop.GetValue(ChangedObject);
        change.NewValue = newval;
        change.ArrayIndex = -1;
        Changes.Add(change);
    }

    public PropChangeAction(Entity ent, PropertyInfo prop, int index, object changed, object newval, string entityName = "")
    {
        TargetEnt = ent;
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

        if(TargetEnt != null)
            TargetEnt.CachedAliasName = null;
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

        if (TargetEnt != null)
            TargetEnt.CachedAliasName = null;

        MapContainer.MarkEntityDirty(TargetEnt);

        RefreshModelIfNeeded();

        if (HasNameChange(out var nameChange))
            RenameEntity((string)nameChange.OldValue, (string)nameChange.NewValue);

        if (HasModelChange())
        {
            if (AtlasEdit == null)
            {
                AtlasEdit = LightAtlasEdit.ForModelChange(new[] { TargetEnt });
            }
            else
            {
                AtlasEdit.Redo();
            }
        }

        return ActionEvent.NoEvent;
    }

    private bool HasNameChange(out PropertyChange nameChange)
    {
        nameChange = Changes.Find(c => c.Property.Name == "Name" && c.OldValue is string);

        return nameChange != null && TargetEnt is MsbEntity && ChangedObject == TargetEnt.WrappedObject;
    }

    private bool HasModelChange()
    {
        return TargetEnt is MsbEntity && ChangedObject == TargetEnt.WrappedObject
            && Changes.Any(c => c.Property.Name == "ModelName" && !Equals(c.OldValue, c.NewValue));
    }

    /// <summary>
    /// The name is written to the wrapped object here, so drop the cached name and update anything that points to it.
    /// </summary>
    private void RenameEntity(string oldName, string newName)
    {
        TargetEnt.Name = null;

        (TargetEnt.Container as MapContainer)?.LightAtlasResolver.AutomaticAdjustment(TargetEnt, oldName, newName);
    }

    /// <summary>
    /// If this action changed ModelName, rebuild the entity's render model.
    /// </summary>
    private void RefreshModelIfNeeded()
    {
        if (TargetEnt is MsbEntity msbEnt && Changes.Any(c => c.Property.Name == "ModelName" ||
                CharacterComposition.IsAppearanceProperty(c.Property.Name)))
        {
            msbEnt.RefreshModel();
        }

        // Only rebuilds if the grass fields have changed
        (TargetEnt as MsbEntity)?.RefreshGrass();
        (TargetEnt as MsbEntity)?.RefreshEdgePoints();
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

        if (TargetEnt != null)
            TargetEnt.CachedAliasName = null;

        MapContainer.MarkEntityDirty(TargetEnt);

        RefreshModelIfNeeded();

        if (HasNameChange(out var nameChange))
            RenameEntity((string)nameChange.NewValue, (string)nameChange.OldValue);

        AtlasEdit?.Undo();

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
