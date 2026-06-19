using Octokit;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

public class MsbPropertyChange : EditorAction
{
    private readonly List<PropertyChange> Changes = new();

    public MsbPropertyChange(PropertyInfo propInfo, object targetEntry, object oldValue, object newValue, int index)
    {
        var propObj = PropFinderUtil.FindPropertyObject(propInfo, targetEntry, index, -1, false);
        if (propObj != null)
        {
            var change = new PropertyChange
            {
                ChangedObj = propObj,
                Property = propInfo,
                NewValue = newValue,
                ArrayIndex = index
            };

            var value = change.Property.GetValue(propObj);

            Type valType = null;

            if (value != null)
            {
                valType = value.GetType();
            }

            if (index != -1 && propInfo.PropertyType.IsArray)
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
                change.OldValue = propInfo.GetValue(propObj);
            }

            Changes.Add(change);
        }
    }

    public override ActionEvent Execute()
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
