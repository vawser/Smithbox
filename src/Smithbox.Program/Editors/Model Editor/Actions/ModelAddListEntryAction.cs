using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.ModelEditor;

public class ModelAddListEntryAction : ViewportAction
{
    private readonly object ChangedObject;
    private readonly int Index;
    private readonly object NewValue;
    private readonly PropertyInfo Property;
    private readonly IList DirectList;
    private readonly Entity TargetEnt;
    private Action<bool> PostExecutionAction;

    public ModelAddListEntryAction(Entity ent, PropertyInfo prop, object changed, object newValue, int index)
    {
        TargetEnt = ent;
        ChangedObject = changed;
        Property = prop;
        NewValue = newValue;
        Index = index;
    }
    public ModelAddListEntryAction(Entity ent, IList list, object newValue, int index)
    {
        TargetEnt = ent;
        DirectList = list;
        NewValue = newValue;
        Index = index;
    }

    private IList GetList()
    {
        return Property != null ? (IList)Property.GetValue(ChangedObject) : DirectList;
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var list = GetList();

        if (Index >= list.Count)
            list.Add(NewValue);
        else
            list.Insert(Index, NewValue);

        if (PostExecutionAction != null)
            PostExecutionAction.Invoke(false);

        if (TargetEnt != null)
            TargetEnt.CachedAliasName = null;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        var list = GetList();
        list.RemoveAt(Index);

        if (PostExecutionAction != null)
            PostExecutionAction.Invoke(true);

        if (TargetEnt != null)
            TargetEnt.CachedAliasName = null;

        return ActionEvent.NoEvent;
    }
}