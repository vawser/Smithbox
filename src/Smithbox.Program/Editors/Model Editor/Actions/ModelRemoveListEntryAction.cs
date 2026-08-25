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
    private readonly Entity TargetEnt;
    private Action<bool> PostExecutionAction;

    public ModelRemoveListEntryAction(Entity ent, PropertyInfo prop, object changed, int index)
    {
        TargetEnt = ent;
        ChangedObject = changed;
        Property = prop;
        Index = index;
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        var list = (IList)Property.GetValue(ChangedObject);
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
        var list = (IList)Property.GetValue(ChangedObject);
        list.Insert(Index, OldValue);

        if (PostExecutionAction != null)
            PostExecutionAction.Invoke(true);

        if (TargetEnt != null)
            TargetEnt.CachedAliasName = null;

        return ActionEvent.NoEvent;
    }
}