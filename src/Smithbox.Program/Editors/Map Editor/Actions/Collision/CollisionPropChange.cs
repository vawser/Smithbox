using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace StudioCore.Editors.MapEditor;

public class CollisionPropChange : ViewportAction
{
    private MapEditorView View;

    private readonly List<PropertyChange> Changes = new();

    public CollisionPropChange(MapEditorView view, FieldInfo prop, object obj, object newval,
        int index = -1, int classIndex = -1)
    {
        View = view;

        var propObj = PropFinderUtil.FindFieldObject(prop, obj, index, classIndex, false);
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

            if (index != -1 && prop.FieldType.IsArray)
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

            if (change.Property.FieldType.IsArray && change.ArrayIndex != -1)
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

            if (change.Property.FieldType.IsArray && change.ArrayIndex != -1)
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

        return ActionEvent.NoEvent;
    }

    private class PropertyChange
    {
        public int ArrayIndex;
        public object ChangedObj;
        public object NewValue;
        public object OldValue;
        public FieldInfo Property;
    }
}
