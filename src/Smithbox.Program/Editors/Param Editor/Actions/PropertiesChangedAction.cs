using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StudioCore.Editors.ParamEditor;


public class PropertiesChangedAction : EditorAction
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

