using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace StudioCore.Editors.HavokEditor;

public class ViewportHavokChangeField : ViewportAction
{
    private readonly List<FieldChange> Changes = new();

    public ViewportHavokChangeField(FieldInfo prop, object obj, object newval,
        int index = -1, int classIndex = -1)
    {
        var propObj = PropFinderUtil.FindFieldObject(prop, obj, index, classIndex, false);
        if (propObj != null)
        {
            var change = new FieldChange
            {
                ChangedObj = propObj,
                Field = prop,
                NewValue = newval,
                ArrayIndex = index
            };

            var value = change.Field.GetValue(propObj);

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
        foreach (FieldChange change in Changes)
        {
            var value = change.Field.GetValue(change.ChangedObj);

            Type valType = null;

            if (value != null)
            {
                valType = value.GetType();
            }

            if (change.Field.FieldType.IsArray && change.ArrayIndex != -1)
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
                change.Field.SetValue(change.ChangedObj, change.NewValue);
            }
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (FieldChange change in Changes)
        {
            var value = change.Field.GetValue(change.ChangedObj);

            Type valType = null;

            if (value != null)
            {
                valType = value.GetType();
            }

            if (change.Field.FieldType.IsArray && change.ArrayIndex != -1)
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
                change.Field.SetValue(change.ChangedObj, change.OldValue);
            }
        }

        return ActionEvent.NoEvent;
    }

    private class FieldChange
    {
        public int ArrayIndex;
        public object ChangedObj;
        public object NewValue;
        public object OldValue;
        public FieldInfo Field;
    }
}

public class HavokChangeField : EditorAction
{
    private readonly List<FieldChange> Changes = new();

    public HavokChangeField(FieldInfo prop, object obj, object newval,
        int index = -1, int classIndex = -1)
    {
        var propObj = PropFinderUtil.FindFieldObject(prop, obj, index, classIndex, false);
        if (propObj != null)
        {
            var change = new FieldChange
            {
                ChangedObj = propObj,
                Field = prop,
                NewValue = newval,
                ArrayIndex = index
            };

            var value = change.Field.GetValue(propObj);

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

    public override ActionEvent Execute()
    {
        foreach (FieldChange change in Changes)
        {
            var value = change.Field.GetValue(change.ChangedObj);

            Type valType = null;

            if (value != null)
            {
                valType = value.GetType();
            }

            if (change.Field.FieldType.IsArray && change.ArrayIndex != -1)
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
                change.Field.SetValue(change.ChangedObj, change.NewValue);
            }
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (FieldChange change in Changes)
        {
            var value = change.Field.GetValue(change.ChangedObj);

            Type valType = null;

            if (value != null)
            {
                valType = value.GetType();
            }

            if (change.Field.FieldType.IsArray && change.ArrayIndex != -1)
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
                change.Field.SetValue(change.ChangedObj, change.OldValue);
            }
        }

        return ActionEvent.NoEvent;
    }

    private class FieldChange
    {
        public int ArrayIndex;
        public object ChangedObj;
        public object NewValue;
        public object OldValue;
        public FieldInfo Field;
    }
}
