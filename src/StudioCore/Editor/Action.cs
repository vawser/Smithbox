using Andre.Formats;
using DotNext;
using Google.Protobuf.WellKnownTypes;
using HKX2;
using SoulsFormats;
using StudioCore.Editors.ParamEditor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editor;

/// <summary>
///     An action that can be performed by the user in the editor that represents
///     a single atomic editor action that affects the state of the map. Each action
///     should have enough information to apply the action AND undo the action, as
///     these actions get pushed to a stack for undo/redo
/// </summary>
public abstract class EditorAction
{
    public abstract ActionEvent Execute();
    public abstract ActionEvent Undo();
}

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

public class AddParamsAction : EditorAction
{
    private readonly bool appOnly;
    private readonly List<Param.Row> Clonables = new();
    private readonly List<Param.Row> Clones = new();
    private readonly int InsertIndex;
    private readonly Param Param;
    private readonly List<Param.Row> Removed = new();
    private readonly List<int> RemovedIndex = new();
    private readonly bool replParams;
    private string ParamString;

    public AddParamsAction(Param param, string pstring, List<Param.Row> rows, bool appendOnly, bool replaceParams,
        int index = -1)
    {
        Param = param;
        Clonables.AddRange(rows);
        ParamString = pstring;
        appOnly = appendOnly;
        replParams = replaceParams;
        InsertIndex = index;
    }

    public override ActionEvent Execute()
    {
        foreach (Param.Row row in Clonables)
        {
            var newrow = new Param.Row(row);
            if (InsertIndex > -1)
            {
                newrow.Name = row.Name != null ? row.Name + "_1" : "";
                Param.InsertRow(InsertIndex, newrow);
            }
            else
            {
                if (Param[row.ID] != null)
                {
                    if (replParams)
                    {
                        Param.Row existing = Param[row.ID];
                        RemovedIndex.Add(Param.IndexOfRow(existing));
                        Removed.Add(existing);
                        Param.RemoveRow(existing);
                    }
                    else
                    {
                        newrow.Name = row.Name != null ? row.Name + "_1" : "";
                        var newID = row.ID + 1;
                        while (Param[newID] != null)
                        {
                            newID++;
                        }

                        newrow.ID = newID;
                        Param.InsertRow(Param.IndexOfRow(Param[newID - 1]) + 1, newrow);
                    }
                }

                if (Param[row.ID] == null)
                {
                    newrow.Name = row.Name != null ? row.Name : "";
                    if (appOnly)
                    {
                        Param.AddRow(newrow);
                    }
                    else
                    {
                        var index = 0;
                        foreach (Param.Row r in Param.Rows)
                        {
                            if (r.ID > newrow.ID)
                            {
                                break;
                            }

                            index++;
                        }

                        Param.InsertRow(index, newrow);
                    }
                }
            }

            Clones.Add(newrow);
        }

        // Refresh diff cache
        TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
            TaskManager.RequeueType.Repeat, true,
            TaskLogs.LogPriority.Low,
            () => ParamBank.RefreshAllParamDiffCaches(false)));
        return ActionEvent.NoEvent;
    }

    public List<Param.Row> GetResultantRows()
    {
        return Clones;
    }

    public override ActionEvent Undo()
    {
        for (var i = 0; i < Clones.Count(); i++)
        {
            Param.RemoveRow(Clones[i]);
        }

        for (var i = Removed.Count() - 1; i >= 0; i--)
        {
            Param.InsertRow(RemovedIndex[i], Removed[i]);
        }

        Clones.Clear();
        RemovedIndex.Clear();
        Removed.Clear();
        return ActionEvent.NoEvent;
    }
}

public class DeleteParamsAction : EditorAction
{
    private readonly List<Param.Row> Deletables = new();
    private readonly Param Param;
    private readonly List<int> RemoveIndices = new();
    private readonly bool SetSelection = false;

    public DeleteParamsAction(Param param, List<Param.Row> rows)
    {
        Param = param;
        Deletables.AddRange(rows);
    }

    public override ActionEvent Execute()
    {
        foreach (Param.Row row in Deletables)
        {
            RemoveIndices.Add(Param.IndexOfRow(row));
            Param.RemoveRowAt(RemoveIndices.Last());
        }

        if (SetSelection)
        {
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (var i = Deletables.Count() - 1; i >= 0; i--)
        {
            Param.InsertRow(RemoveIndices[i], Deletables[i]);
        }

        if (SetSelection)
        {
        }

        // Refresh diff cache
        TaskManager.Run(new TaskManager.LiveTask("Param - Check Differences",
            TaskManager.RequeueType.Repeat, true,
            TaskLogs.LogPriority.Low,
            () => ParamBank.RefreshAllParamDiffCaches(false)));
        return ActionEvent.NoEvent;
    }
}

public class DuplicateFMGEntryAction : EditorAction
{
    private readonly FMGBank.EntryGroup EntryGroup;
    private FMGBank.EntryGroup NewEntryGroup;

    public DuplicateFMGEntryAction(FMGBank.EntryGroup entryGroup)
    {
        EntryGroup = entryGroup;
    }

    public override ActionEvent Execute()
    {
        NewEntryGroup = EntryGroup.DuplicateFMGEntries();
        NewEntryGroup.SetNextUnusedID();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        NewEntryGroup.DeleteEntries();
        return ActionEvent.NoEvent;
    }
}

public class DeleteFMGEntryAction : EditorAction
{
    private FMGBank.EntryGroup BackupEntryGroup = new();
    private FMGBank.EntryGroup EntryGroup;

    public DeleteFMGEntryAction(FMGBank.EntryGroup entryGroup)
    {
        EntryGroup = entryGroup;
    }

    public override ActionEvent Execute()
    {
        BackupEntryGroup = EntryGroup.CloneEntryGroup();
        EntryGroup.DeleteEntries();
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        EntryGroup = BackupEntryGroup;
        EntryGroup.ImplementEntryGroup();
        return ActionEvent.NoEvent;
    }
}

public class CompoundAction : EditorAction
{
    private readonly List<EditorAction> Actions;

    private Action<bool> PostExecutionAction;

    public CompoundAction(List<EditorAction> actions)
    {
        Actions = actions;
    }

    public bool HasActions => Actions.Any();

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute()
    {
        var evt = ActionEvent.NoEvent;
        for (var i = 0; i < Actions.Count; i++)
        {
            EditorAction act = Actions[i];
            if (act != null)
            {
                evt |= act.Execute();
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
        for (var i = Actions.Count - 1; i >= 0; i--)
        {
            EditorAction act = Actions[i];
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
}


/// <summary>
/// GPARAM Editor Actions
/// </summary>
/// 
public class GparamValueChangeAction : EditorAction
{
    public enum ValueChangeType
    {
        Set,
        Addition,
        Subtraction,
        Multiplication
    }

    private readonly List<GparamValueChange> Changes = new();

    public GparamValueChangeAction(SoulsFormats.GPARAM.IField field, SoulsFormats.GPARAM.IFieldValue fieldValue, object newValue, int index, ValueChangeType valueChangeType)
    {
        var change = new GparamValueChange();
        change.Index = index;
        change.Field = field;
        change.OldValue = fieldValue.Value;
        change.NewValue = newValue;
        change.ValueChangeType = valueChangeType;
        Changes.Add(change);
    }

    public override ActionEvent Execute()
    {
        foreach (GparamValueChange change in Changes)
        {
            if (change.Field is GPARAM.IntField intField)
            {
                var assignedValue = (int)change.NewValue;
                var result = assignedValue;

                if (change.ValueChangeType == ValueChangeType.Set)
                {
                    result = assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Addition)
                {
                    var currVal = intField.Values[change.Index].Value;
                    result = currVal + assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Subtraction)
                {
                    var currVal = intField.Values[change.Index].Value;
                    result = currVal - assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Multiplication)
                {
                    var currVal = intField.Values[change.Index].Value;
                    result = currVal * assignedValue;
                }

                if (result > int.MaxValue)
                    result = int.MaxValue;

                if (result < int.MinValue)
                    result = int.MinValue;

                intField.Values[change.Index].Value = result;
            }
            if (change.Field is GPARAM.UintField uintField)
            {
                var assignedValue = (uint)change.NewValue;
                var result = assignedValue;

                if (change.ValueChangeType == ValueChangeType.Set)
                {
                    result = assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Addition)
                {
                    var currVal = uintField.Values[change.Index].Value;
                    result = currVal + assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Subtraction)
                {
                    var currVal = uintField.Values[change.Index].Value;
                    result = currVal - assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Multiplication)
                {
                    var currVal = uintField.Values[change.Index].Value;
                    result = currVal * assignedValue;
                }

                if (result > uint.MaxValue)
                    result = uint.MaxValue;

                if (result < uint.MinValue)
                    result = uint.MinValue;

                uintField.Values[change.Index].Value = result;
            }
            if (change.Field is GPARAM.ShortField shortField)
            {
                var assignedValue = (short)change.NewValue;
                var result = assignedValue;

                if (change.ValueChangeType == ValueChangeType.Set)
                {
                    result = assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Addition)
                {
                    var currVal = shortField.Values[change.Index].Value;
                    result = (short)(currVal + assignedValue);
                }
                if (change.ValueChangeType == ValueChangeType.Subtraction)
                {
                    var currVal = shortField.Values[change.Index].Value;
                    result = (short)(currVal - assignedValue);
                }
                if (change.ValueChangeType == ValueChangeType.Multiplication)
                {
                    var currVal = shortField.Values[change.Index].Value;
                    result = (short)(currVal * assignedValue);
                }

                if (result > short.MaxValue)
                    result = short.MaxValue;

                if (result < short.MinValue)
                    result = short.MinValue;

                shortField.Values[change.Index].Value = result;
            }
            if (change.Field is GPARAM.SbyteField sbyteField)
            {
                var assignedValue = (sbyte)change.NewValue;
                var result = assignedValue;

                if (change.ValueChangeType == ValueChangeType.Set)
                {
                    result = assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Addition)
                {
                    var currVal = sbyteField.Values[change.Index].Value;
                    result = (sbyte)(currVal + assignedValue);
                }
                if (change.ValueChangeType == ValueChangeType.Subtraction)
                {
                    var currVal = sbyteField.Values[change.Index].Value;
                    result = (sbyte)(currVal - assignedValue);
                }
                if (change.ValueChangeType == ValueChangeType.Multiplication)
                {
                    var currVal = sbyteField.Values[change.Index].Value;
                    result = (sbyte)(currVal * assignedValue);
                }

                if (result > sbyte.MaxValue)
                    result = sbyte.MaxValue;

                if (result < sbyte.MinValue)
                    result = sbyte.MinValue;

                sbyteField.Values[change.Index].Value = result;
            }
            if (change.Field is GPARAM.ByteField byteField)
            {
                var assignedValue = (byte)change.NewValue;
                var result = assignedValue;

                if (change.ValueChangeType == ValueChangeType.Set)
                {
                    result = assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Addition)
                {
                    var currVal = byteField.Values[change.Index].Value;
                    result = (byte)(currVal + assignedValue);
                }
                if (change.ValueChangeType == ValueChangeType.Subtraction)
                {
                    var currVal = byteField.Values[change.Index].Value;
                    result = (byte)(currVal - assignedValue);
                }
                if (change.ValueChangeType == ValueChangeType.Multiplication)
                {
                    var currVal = byteField.Values[change.Index].Value;
                    result = (byte)(currVal * assignedValue);
                }

                if (result > byte.MaxValue)
                    result = byte.MaxValue;

                if (result < byte.MinValue)
                    result = byte.MinValue;

                byteField.Values[change.Index].Value = result;
            }
            if (change.Field is GPARAM.BoolField boolField)
            {
                var assignedValue = (int)change.NewValue;

                bool result = true;

                if(assignedValue == 0)
                {
                    result = false;
                }

                boolField.Values[change.Index].Value = result;
            }
            if (change.Field is GPARAM.FloatField floatField)
            {
                var assignedValue = (float)change.NewValue;
                var result = assignedValue;

                if (change.ValueChangeType == ValueChangeType.Set)
                {
                    result = assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Addition)
                {
                    var currVal = floatField.Values[change.Index].Value;
                    result = (float)(currVal + assignedValue);
                }
                if (change.ValueChangeType == ValueChangeType.Subtraction)
                {
                    var currVal = floatField.Values[change.Index].Value;
                    result = (float)(currVal - assignedValue);
                }
                if (change.ValueChangeType == ValueChangeType.Multiplication)
                {
                    var currVal = floatField.Values[change.Index].Value;
                    result = (float)(currVal * assignedValue);
                }

                if (result > float.MaxValue)
                    result = float.MaxValue;

                if (result < float.MinValue)
                    result = float.MinValue;

                floatField.Values[change.Index].Value = result;
            }
            if (change.Field is GPARAM.Vector2Field vector2Field)
            {
                var assignedValue = (Vector2)change.NewValue;
                var result = assignedValue;

                if (change.ValueChangeType == ValueChangeType.Set)
                {
                    result = assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Addition)
                {
                    var currVal = vector2Field.Values[change.Index].Value;
                    var newX = currVal.X = currVal.X + assignedValue.X;
                    var newY = currVal.Y = currVal.Y + assignedValue.Y;
                    result = new Vector2(newX, newY);
                }
                if (change.ValueChangeType == ValueChangeType.Subtraction)
                {
                    var currVal = vector2Field.Values[change.Index].Value;
                    var newX = currVal.X = currVal.X - assignedValue.X;
                    var newY = currVal.Y = currVal.Y - assignedValue.Y;
                    result = new Vector2(newX, newY);
                }
                if (change.ValueChangeType == ValueChangeType.Multiplication)
                {
                    var currVal = vector2Field.Values[change.Index].Value;
                    var newX = currVal.X = currVal.X * assignedValue.X;
                    var newY = currVal.Y = currVal.Y * assignedValue.Y;
                    result = new Vector2(newX, newY);
                }

                if (result[0] > float.MaxValue)
                    result[0] = float.MaxValue;

                if (result[0] < float.MinValue)
                    result[0] = float.MinValue;

                if (result[1] > float.MaxValue)
                    result[1] = float.MaxValue;

                if (result[1] < float.MinValue)
                    result[1] = float.MinValue;

                vector2Field.Values[change.Index].Value = result;
            }
            if (change.Field is GPARAM.Vector3Field vector3Field)
            {
                var assignedValue = (Vector3)change.NewValue;
                var result = assignedValue;

                if (change.ValueChangeType == ValueChangeType.Set)
                {
                    result = assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Addition)
                {
                    var currVal = vector3Field.Values[change.Index].Value;
                    var newX = currVal.X = currVal.X + assignedValue.X;
                    var newY = currVal.Y = currVal.Y + assignedValue.Y;
                    var newZ = currVal.Z = currVal.Z + assignedValue.Z;
                    result = new Vector3(newX, newY, newZ);
                }
                if (change.ValueChangeType == ValueChangeType.Subtraction)
                {
                    var currVal = vector3Field.Values[change.Index].Value;
                    var newX = currVal.X = currVal.X - assignedValue.X;
                    var newY = currVal.Y = currVal.Y - assignedValue.Y;
                    var newZ = currVal.Z = currVal.Z - assignedValue.Z;
                    result = new Vector3(newX, newY, newZ);
                }
                if (change.ValueChangeType == ValueChangeType.Multiplication)
                {
                    var currVal = vector3Field.Values[change.Index].Value;
                    var newX = currVal.X = currVal.X * assignedValue.X;
                    var newY = currVal.Y = currVal.Y * assignedValue.Y;
                    var newZ = currVal.Z = currVal.Z * assignedValue.Z;
                    result = new Vector3(newX, newY, newZ);
                }

                if (result[0] > float.MaxValue)
                    result[0] = float.MaxValue;

                if (result[0] < float.MinValue)
                    result[0] = float.MinValue;

                if (result[1] > float.MaxValue)
                    result[1] = float.MaxValue;

                if (result[1] < float.MinValue)
                    result[1] = float.MinValue;

                if (result[2] > float.MaxValue)
                    result[2] = float.MaxValue;

                if (result[2] < float.MinValue)
                    result[2] = float.MinValue;

                vector3Field.Values[change.Index].Value = result;
            }
            if (change.Field is GPARAM.Vector4Field vector4Field)
            {
                var assignedValue = (Vector4)change.NewValue;
                var result = assignedValue;

                if (change.ValueChangeType == ValueChangeType.Set)
                {
                    result = assignedValue;
                }
                if (change.ValueChangeType == ValueChangeType.Addition)
                {
                    var currVal = vector4Field.Values[change.Index].Value;
                    var newX = currVal.X = currVal.X + assignedValue.X;
                    var newY = currVal.Y = currVal.Y + assignedValue.Y;
                    var newZ = currVal.Z = currVal.Z + assignedValue.Z;
                    var newW = currVal.W = currVal.W + assignedValue.W;
                    result = new Vector4(newX, newY, newZ, newW);
                }
                if (change.ValueChangeType == ValueChangeType.Subtraction)
                {
                    var currVal = vector4Field.Values[change.Index].Value;
                    var newX = currVal.X = currVal.X - assignedValue.X;
                    var newY = currVal.Y = currVal.Y - assignedValue.Y;
                    var newZ = currVal.Z = currVal.Z - assignedValue.Z;
                    var newW = currVal.W = currVal.W - assignedValue.W;
                    result = new Vector4(newX, newY, newZ, newW);
                }
                if (change.ValueChangeType == ValueChangeType.Multiplication)
                {
                    var currVal = vector4Field.Values[change.Index].Value;
                    var newX = currVal.X = currVal.X * assignedValue.X;
                    var newY = currVal.Y = currVal.Y * assignedValue.Y;
                    var newZ = currVal.Z = currVal.Z * assignedValue.Z;
                    var newW = currVal.W = currVal.W * assignedValue.W;
                    result = new Vector4(newX, newY, newZ, newW);
                }

                if (result[0] > float.MaxValue)
                    result[0] = float.MaxValue;

                if (result[0] < float.MinValue)
                    result[0] = float.MinValue;

                if (result[1] > float.MaxValue)
                    result[1] = float.MaxValue;

                if (result[1] < float.MinValue)
                    result[1] = float.MinValue;

                if (result[2] > float.MaxValue)
                    result[2] = float.MaxValue;

                if (result[2] < float.MinValue)
                    result[2] = float.MinValue;

                if (result[3] > float.MaxValue)
                    result[3] = float.MaxValue;

                if (result[3] < float.MinValue)
                    result[3] = float.MinValue;

                vector4Field.Values[change.Index].Value = result;
            }
            if (change.Field is GPARAM.ColorField colorField)
            {
                var assignedValue = (Color)change.NewValue;

                colorField.Values[change.Index].Value = assignedValue;
            }
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (GparamValueChange change in Changes)
        {
            if (change.Field is GPARAM.IntField intField)
            {
                intField.Values[change.Index].Value = (int)change.OldValue;
            }
            if (change.Field is GPARAM.UintField uintField)
            {
                uintField.Values[change.Index].Value = (uint)change.OldValue;
            }
            if (change.Field is GPARAM.ShortField shortField)
            {
                shortField.Values[change.Index].Value = (short)change.OldValue;
            }
            if (change.Field is GPARAM.SbyteField sbyteField)
            {
                sbyteField.Values[change.Index].Value = (sbyte)change.OldValue;
            }
            if (change.Field is GPARAM.ByteField byteField)
            {
                byteField.Values[change.Index].Value = (byte)change.OldValue;
            }
            if (change.Field is GPARAM.BoolField boolField)
            {
                boolField.Values[change.Index].Value = (bool)change.OldValue;
            }
            if (change.Field is GPARAM.FloatField floatField)
            {
                floatField.Values[change.Index].Value = (float)change.OldValue;
            }
            if (change.Field is GPARAM.Vector2Field vector2Field)
            {
                vector2Field.Values[change.Index].Value = (Vector2)change.OldValue;
            }
            if (change.Field is GPARAM.Vector3Field vector3Field)
            {
                vector3Field.Values[change.Index].Value = (Vector3)change.OldValue;
            }
            if (change.Field is GPARAM.Vector4Field vector4Field)
            {
                vector4Field.Values[change.Index].Value = (Vector4)change.OldValue;
            }
            if (change.Field is GPARAM.ColorField colorField)
            {
                colorField.Values[change.Index].Value = (Color)change.OldValue;
            }
        }

        return ActionEvent.NoEvent;
    }

    private class GparamValueChange
    {
        public int Index;
        public object NewValue;
        public object OldValue;
        public ValueChangeType ValueChangeType;
        public SoulsFormats.GPARAM.IField Field;
    }
}

public class GparamTimeOfDayChangeAction : EditorAction
{
    private readonly List<GparamValueChange> Changes = new();

    public GparamTimeOfDayChangeAction(SoulsFormats.GPARAM.IField field, SoulsFormats.GPARAM.IFieldValue fieldValue, object newValue, int index)
    {
        var change = new GparamValueChange();
        change.Index = index;
        change.Field = field;
        change.OldValue = fieldValue.Unk04;
        change.NewValue = newValue;
        Changes.Add(change);
    }

    public override ActionEvent Execute()
    {
        foreach (GparamValueChange change in Changes)
        {
            if (change.Field is GPARAM.IntField intField)
            {
                intField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is GPARAM.UintField uintField)
            {
                uintField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is GPARAM.ShortField shortField)
            {
                shortField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is GPARAM.SbyteField sbyteField)
            {
                sbyteField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is GPARAM.ByteField byteField)
            {
                byteField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is GPARAM.BoolField boolField)
            {
                boolField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is GPARAM.FloatField floatField)
            {
                floatField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is GPARAM.Vector2Field vector2Field)
            {
                vector2Field.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is GPARAM.Vector3Field vector3Field)
            {
                vector3Field.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is GPARAM.Vector4Field vector4Field)
            {
                vector4Field.Values[change.Index].Unk04 = (float)change.NewValue;
            }
            if (change.Field is GPARAM.ColorField colorField)
            {
                colorField.Values[change.Index].Unk04 = (float)change.NewValue;
            }
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (GparamValueChange change in Changes)
        {
            if (change.Field is GPARAM.IntField intField)
            {
                intField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is GPARAM.UintField uintField)
            {
                uintField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is GPARAM.ShortField shortField)
            {
                shortField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is GPARAM.SbyteField sbyteField)
            {
                sbyteField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is GPARAM.ByteField byteField)
            {
                byteField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is GPARAM.BoolField boolField)
            {
                boolField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is GPARAM.FloatField floatField)
            {
                floatField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is GPARAM.Vector2Field vector2Field)
            {
                vector2Field.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is GPARAM.Vector3Field vector3Field)
            {
                vector3Field.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is GPARAM.Vector4Field vector4Field)
            {
                vector4Field.Values[change.Index].Unk04 = (float)change.OldValue;
            }
            if (change.Field is GPARAM.ColorField colorField)
            {
                colorField.Values[change.Index].Unk04 = (float)change.OldValue;
            }
        }

        return ActionEvent.NoEvent;
    }

    private class GparamValueChange
    {
        public int Index;
        public object NewValue;
        public object OldValue;
        public SoulsFormats.GPARAM.IField Field;
    }
}

public class GparamBatchChangeAction : EditorAction
{
    private readonly List<GparamValueChangeAction> ChangeActions = new();

    public GparamBatchChangeAction(List<GparamValueChangeAction> actions)
    {
        ChangeActions = actions;
    }

    public override ActionEvent Execute()
    {
        foreach (GparamValueChangeAction action in ChangeActions)
        {
            action.Execute();
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (GparamValueChangeAction action in ChangeActions)
        {
            action.Undo();
        }

        return ActionEvent.NoEvent;
    }
}
