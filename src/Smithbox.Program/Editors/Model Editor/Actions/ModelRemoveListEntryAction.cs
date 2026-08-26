using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.ModelEditor;

public class ModelRemoveListEntryAction : ViewportAction
{
    private readonly object ChangedObject;
    private readonly int Index;
    private object OldValue;
    private readonly PropertyInfo Property;
    private readonly IList DirectList;
    private readonly Entity TargetEnt;
    private Action<bool> PostExecutionAction;

    public ModelRemoveListEntryAction(Entity ent, PropertyInfo prop, object changed, int index)
    {
        TargetEnt = ent;
        ChangedObject = changed;
        Property = prop;
        Index = index;
    }
    public ModelRemoveListEntryAction(Entity ent, IList list, int index)
    {
        TargetEnt = ent;
        DirectList = list;
        Index = index;
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }
    private IList GetList()
    {
        return Property != null ? (IList)Property.GetValue(ChangedObject) : DirectList;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var list = GetList();
        OldValue = list[Index];
        list.RemoveAt(Index);

        if (PostExecutionAction != null)
            PostExecutionAction.Invoke(false);

        if (TargetEnt != null)
            TargetEnt.CachedAliasName = null;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        var list = GetList();
        list.Insert(Index, OldValue);

        if (PostExecutionAction != null)
            PostExecutionAction.Invoke(true);

        if (TargetEnt != null)
            TargetEnt.CachedAliasName = null;

        return ActionEvent.NoEvent;
    }
}