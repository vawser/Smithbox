using Andre.Formats;
using StudioCore.ParamEditor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
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
