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

                var arrayEntry = a.GetValue(change.ArrayIndex);
                var arrayType = arrayEntry.GetType();

                if (arrayType == typeof(long))
                {
                    if (long.TryParse($"{change.NewValue}", out var longVal))
                    {
                        a.SetValue(longVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(ulong))
                {
                    if (ulong.TryParse($"{change.NewValue}", out var ulongVal))
                    {
                        a.SetValue(ulongVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(int))
                {
                    if (int.TryParse($"{change.NewValue}", out var intVal))
                    {
                        a.SetValue(intVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(uint))
                {
                    if (uint.TryParse($"{change.NewValue}", out var uintVal))
                    {
                        a.SetValue(uintVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(short))
                {
                    if (short.TryParse($"{change.NewValue}", out var shortVal))
                    {
                        a.SetValue(shortVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(ushort))
                {
                    if (ushort.TryParse($"{change.NewValue}", out var ushortVal))
                    {
                        a.SetValue(ushortVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(byte))
                {
                    if (byte.TryParse($"{change.NewValue}", out var byteVal))
                    {
                        a.SetValue(byteVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(sbyte))
                {
                    if (sbyte.TryParse($"{change.NewValue}", out var sbyteVal))
                    {
                        a.SetValue(sbyteVal, change.ArrayIndex);
                    }
                }
                else
                {
                    a.SetValue(change.NewValue, change.ArrayIndex);
                }
            }
            else if (value != null && valType != null && valType.IsGenericType && change.ArrayIndex != -1 && value is IList list)
            {
                list[change.ArrayIndex] = change.NewValue;
            }
            else
            {
                if (valType == typeof(long))
                {
                    if (long.TryParse($"{change.NewValue}", out var longVal))
                    {
                        change.Property.SetValue(change.ChangedObj, longVal);
                    }
                }
                else if (valType == typeof(ulong))
                {
                    if (ulong.TryParse($"{change.NewValue}", out var ulongVal))
                    {
                        change.Property.SetValue(change.ChangedObj, ulongVal);
                    }
                }
                else if (valType == typeof(int))
                {
                    if (int.TryParse($"{change.NewValue}", out var intVal))
                    {
                        change.Property.SetValue(change.ChangedObj, intVal);
                    }
                }
                else if (valType == typeof(uint))
                {
                    if (uint.TryParse($"{change.NewValue}", out var uintVal))
                    {
                        change.Property.SetValue(change.ChangedObj, uintVal);
                    }
                }
                else if (valType == typeof(short))
                {
                    if (short.TryParse($"{change.NewValue}", out var shortVal))
                    {
                        change.Property.SetValue(change.ChangedObj, shortVal);
                    }
                }
                else if (valType == typeof(ushort))
                {
                    if (ushort.TryParse($"{change.NewValue}", out var ushortVal))
                    {
                        change.Property.SetValue(change.ChangedObj, ushortVal);
                    }
                }
                else if (valType == typeof(byte))
                {
                    if (byte.TryParse($"{change.NewValue}", out var byteVal))
                    {
                        change.Property.SetValue(change.ChangedObj, byteVal);
                    }
                }
                else if (valType == typeof(sbyte))
                {
                    if (sbyte.TryParse($"{change.NewValue}", out var sbyteVal))
                    {
                        change.Property.SetValue(change.ChangedObj, sbyteVal);
                    }
                }
                else
                {
                    change.Property.SetValue(change.ChangedObj, change.NewValue);
                }
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
            var value = change.Property.GetValue(change.ChangedObj);

            Type valType = null;

            if (value != null)
            {
                valType = value.GetType();
            }

            if (change.Property.PropertyType.IsArray && change.ArrayIndex != -1)
            {
                var a = (Array)value;

                var arrayEntry = a.GetValue(change.ArrayIndex);
                var arrayType = arrayEntry.GetType();

                if (arrayType == typeof(long))
                {
                    if (long.TryParse($"{change.OldValue}", out var longVal))
                    {
                        a.SetValue(longVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(ulong))
                {
                    if (ulong.TryParse($"{change.OldValue}", out var ulongVal))
                    {
                        a.SetValue(ulongVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(int))
                {
                    if (int.TryParse($"{change.OldValue}", out var intVal))
                    {
                        a.SetValue(intVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(uint))
                {
                    if (uint.TryParse($"{change.OldValue}", out var uintVal))
                    {
                        a.SetValue(uintVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(short))
                {
                    if (short.TryParse($"{change.OldValue}", out var shortVal))
                    {
                        a.SetValue(shortVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(ushort))
                {
                    if (ushort.TryParse($"{change.OldValue}", out var ushortVal))
                    {
                        a.SetValue(ushortVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(byte))
                {
                    if (byte.TryParse($"{change.OldValue}", out var byteVal))
                    {
                        a.SetValue(byteVal, change.ArrayIndex);
                    }
                }
                else if (arrayType == typeof(sbyte))
                {
                    if (sbyte.TryParse($"{change.OldValue}", out var sbyteVal))
                    {
                        a.SetValue(sbyteVal, change.ArrayIndex);
                    }
                }
                else
                {
                    a.SetValue(change.OldValue, change.ArrayIndex);
                }
            }
            else if (value != null && valType != null && valType.IsGenericType && change.ArrayIndex != -1 && value is IList list)
            {
                list[change.ArrayIndex] = change.OldValue;
            }
            else
            {
                if (valType == typeof(long))
                {
                    if (long.TryParse($"{change.OldValue}", out var longVal))
                    {
                        change.Property.SetValue(change.ChangedObj, longVal);
                    }
                }
                else if (valType == typeof(ulong))
                {
                    if (ulong.TryParse($"{change.OldValue}", out var ulongVal))
                    {
                        change.Property.SetValue(change.ChangedObj, ulongVal);
                    }
                }
                else if (valType == typeof(int))
                {
                    if (int.TryParse($"{change.OldValue}", out var intVal))
                    {
                        change.Property.SetValue(change.ChangedObj, intVal);
                    }
                }
                else if (valType == typeof(uint))
                {
                    if (uint.TryParse($"{change.OldValue}", out var uintVal))
                    {
                        change.Property.SetValue(change.ChangedObj, uintVal);
                    }
                }
                else if (valType == typeof(short))
                {
                    if (short.TryParse($"{change.OldValue}", out var shortVal))
                    {
                        change.Property.SetValue(change.ChangedObj, shortVal);
                    }
                }
                else if (valType == typeof(ushort))
                {
                    if (ushort.TryParse($"{change.OldValue}", out var ushortVal))
                    {
                        change.Property.SetValue(change.ChangedObj, ushortVal);
                    }
                }
                else if (valType == typeof(byte))
                {
                    if (byte.TryParse($"{change.OldValue}", out var byteVal))
                    {
                        change.Property.SetValue(change.ChangedObj, byteVal);
                    }
                }
                else if (valType == typeof(sbyte))
                {
                    if (sbyte.TryParse($"{change.OldValue}", out var sbyteVal))
                    {
                        change.Property.SetValue(change.ChangedObj, sbyteVal);
                    }
                }
                else
                {
                    change.Property.SetValue(change.ChangedObj, change.OldValue);
                }
            }
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return ActionEvent.NoEvent;
    }

    private Action<bool> PostExecutionAction;

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
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
