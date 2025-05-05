using Andre.Formats;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ParamEditor.META;
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
    private ParamEditorScreen Editor;
    private readonly bool appOnly;
    private readonly List<Param.Row> Clonables = new();
    private readonly List<Param.Row> Clones = new();
    private readonly int InsertIndex;
    private readonly Param Param;
    private readonly List<Param.Row> Removed = new();
    private readonly List<int> RemovedIndex = new();
    private readonly bool replParams;
    private string ParamString;
    private int IdOffset;
    private bool IsDuplicate;

    public AddParamsAction(ParamEditorScreen editor, Param param, string pstring, List<Param.Row> rows, bool appendOnly, bool replaceParams,
        int index = -1, int idOffset = 1, bool isDuplicate = false)
    {
        Editor = editor;
        Param = param;
        Clonables.AddRange(rows);
        ParamString = pstring;
        appOnly = appendOnly;
        replParams = replaceParams;
        InsertIndex = index;
        IdOffset = idOffset;
        IsDuplicate = isDuplicate;
    }

    public override ActionEvent Execute()
    {
        foreach (Param.Row row in Clonables)
        {
            var newrow = new Param.Row(row);

            var paramMeta = Editor.Project.ParamData.GetParamMeta(row.Def);

            // Only apply for Duplicate action
            if (IsDuplicate)
            {
                foreach (var cell in newrow.Cells)
                {
                    var meta = Editor.Project.ParamData.GetParamFieldMeta(paramMeta, cell.Def);
                    var adjust = false;

                    if (meta.DeepCopyTargetType != null)
                    {
                        // Attack
                        if (CFG.Current.Param_Toolbar_Duplicate_AffectAttackField)
                        {
                            adjust = true;
                        }

                        // Bullet
                        if (CFG.Current.Param_Toolbar_Duplicate_AffectBulletField)
                        {
                            adjust = true;
                        }

                        // Behavior
                        if (CFG.Current.Param_Toolbar_Duplicate_AffectBehaviorField)
                        {
                            adjust = true;
                        }

                        // SpEffect
                        if (CFG.Current.Param_Toolbar_Duplicate_AffectSpEffectField)
                        {
                            adjust = true;
                        }

                        // Origin
                        if (CFG.Current.Param_Toolbar_Duplicate_AffectSourceField)
                        {
                            adjust = true;
                        }

                        if (adjust)
                        {
                            // Assuming ints here
                            int id = (int)cell.Value;

                            // Ignore if value is -1
                            if (id != -1)
                            {
                                id = id + IdOffset;

                                cell.Value = id;
                            }
                        }
                    }
                }
            }

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
                        var rowIdOffset = IdOffset;
                        if (rowIdOffset <= 0)
                            rowIdOffset = 1;

                        newrow.Name = row.Name != null ? row.Name + "_1" : "";
                        var newID = row.ID + rowIdOffset;
                        while (Param[newID] != null)
                        {
                            newID += rowIdOffset;
                        }

                        newrow.ID = newID;
                        Param.InsertRow(Param.IndexOfRow(Param[newID - rowIdOffset]) + 1, newrow);
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

        Editor.Project.ParamData.RefreshParamDifferenceCacheTask();

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
    private ParamEditorScreen Editor;
    private readonly List<Param.Row> Deletables = new();
    private readonly Param Param;
    private readonly List<int> RemoveIndices = new();
    private readonly bool SetSelection = false;

    public DeleteParamsAction(ParamEditorScreen editor, Param param, List<Param.Row> rows)
    {
        Editor = editor;
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

        Editor.Project.ParamData.RefreshParamDifferenceCacheTask();

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

