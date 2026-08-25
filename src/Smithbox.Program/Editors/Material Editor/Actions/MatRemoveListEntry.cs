using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.MaterialEditor;

public class MatRemoveListEntry : EditorAction
{
    private readonly object ChangedObject;
    private readonly int Index;
    private object OldValue;
    private readonly PropertyInfo Property;
    private Action<bool> PostExecutionAction;

    public MatRemoveListEntry(PropertyInfo prop, object changed, int index)
    {
        ChangedObject = changed;
        Property = prop;
        Index = index;
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute()
    {
        var list = (IList)Property.GetValue(ChangedObject);
        OldValue = list[Index];
        list.RemoveAt(Index);

        if (PostExecutionAction != null)
            PostExecutionAction.Invoke(false);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        var list = (IList)Property.GetValue(ChangedObject);
        list.Insert(Index, OldValue);

        if (PostExecutionAction != null)
            PostExecutionAction.Invoke(true);

        return ActionEvent.NoEvent;
    }
}