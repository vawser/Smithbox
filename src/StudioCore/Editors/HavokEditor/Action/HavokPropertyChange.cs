using StudioCore.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.HavokEditor.Action;

public class HavokPropertyChange : EditorAction
{
    private readonly object ChangedObject;
    private readonly object SourceObject;
    private readonly List<FieldChange> Changes = new();
    private Action<bool> PostExecutionAction;

    public HavokPropertyChange(object changed)
    {
        ChangedObject = changed;
    }

    public HavokPropertyChange(FieldInfo prop, object sourceObj, object changed, object newval)
    {
        ChangedObject = changed;
        SourceObject = sourceObj;
        var change = new FieldChange();
        change.Field = prop;
        change.OldValue = prop.GetValue(sourceObj);
        change.NewValue = newval;
        change.ArrayIndex = -1;
        Changes.Add(change);
    }

    public HavokPropertyChange(FieldInfo prop, object sourceObj, object changed, object newval, int arrayIndex)
    {
        ChangedObject = changed;
        SourceObject = sourceObj;
        var change = new FieldChange();
        change.Field = prop;

        var value = change.Field.GetValue(sourceObj);
        if (arrayIndex != -1 && prop.FieldType.IsArray)
        {
            var a = (Array)value;
            change.OldValue = a.GetValue(arrayIndex);
        }
        else if (value != null &&
            value.GetType().IsGenericType &&
            arrayIndex != -1 && value is IList list)
        {
            change.OldValue = list[arrayIndex];
        }
        else
        {
            change.OldValue = prop.GetValue(sourceObj);
        }

        change.NewValue = newval;
        change.ArrayIndex = arrayIndex;
        Changes.Add(change);
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute()
    {
        foreach (FieldChange change in Changes)
        {
            var value = change.Field.GetValue(SourceObject);

            Type valType = null;

            if (value != null)
                valType = value.GetType();

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
                change.Field.SetValue(SourceObject, change.NewValue);
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
        foreach (FieldChange change in Changes)
        {
            var value = change.Field.GetValue(SourceObject);

            Type valType = null;

            if (value != null)
                valType = value.GetType();

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
                change.Field.SetValue(SourceObject, change.OldValue);
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return ActionEvent.NoEvent;
    }

    private class FieldChange
    {
        public int ArrayIndex;
        public object NewValue;
        public object OldValue;
        public FieldInfo Field;
    }
}