using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

public class MultipleEntityPropertyChangeAction : ViewportAction
{
    private EditorScreen Editor;

    private readonly HashSet<Entity> ChangedEnts = new();
    private readonly List<PropertyChange> Changes = new();

    public bool UpdateRenderModel = false;
    public bool ClearName { get; set; }

    public MultipleEntityPropertyChangeAction(EditorScreen editor, PropertyInfo prop, HashSet<Entity> changedEnts, object newval,
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
