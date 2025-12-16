using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class MapObjectPropertyChangeAction : ViewportAction
{
    private MapEditorScreen Editor;

    private readonly object ChangedObject;
    private readonly List<PropertyChange> Changes = new();
    private Action<bool> PostExecutionAction;

    private string EditMessage = "";

    private readonly Entity TargetEntity;

    public MapObjectPropertyChangeAction(MapEditorScreen editor, Entity ent, object changed)
    {
        Editor = editor;
        ChangedObject = changed;
        TargetEntity = ent;
    }

    public MapObjectPropertyChangeAction(MapEditorScreen editor, Entity ent, PropertyInfo prop, object changed, object newval, string entityName = "")
    {
        Editor = editor;
        TargetEntity = ent;

        ChangedObject = changed;
        var change = new PropertyChange();
        change.Property = prop;
        change.OldValue = prop.GetValue(ChangedObject);
        change.NewValue = newval;
        change.ArrayIndex = -1;
        Changes.Add(change);

        EditMessage = $"{entityName} -> {prop.Name} was changed to {change.NewValue}";
    }

    public MapObjectPropertyChangeAction(MapEditorScreen editor, Entity ent, PropertyInfo prop, int index, object changed, object newval, string entityName = "")
    {
        Editor = editor;
        TargetEntity = ent;

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

        EditMessage = $"{entityName} -> {prop.Name} was changed to {change.NewValue}";
        if (change.ArrayIndex != -1)
        {
            EditMessage = $"{entityName} -> {prop.Name}[{change.ArrayIndex}] was changed to {change.NewValue}";
        }
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

        TargetEntity.UpdateRenderModel(Editor);

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

        TargetEntity.UpdateRenderModel(Editor);

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
