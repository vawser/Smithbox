using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokAddListEntryAction : EditorAction
{
    private readonly object ChangedObject;
    private readonly int Index;
    private readonly object NewValue;
    private readonly FieldInfo Property;
    private Action<bool> PostExecutionAction;

    public HavokAddListEntryAction(FieldInfo prop, object changed, object newValue, int index)
    {
        ChangedObject = changed;
        Property = prop;
        NewValue = newValue;
        Index = index;
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute()
    {
        var list = (IList)Property.GetValue(ChangedObject);

        if (Index >= list.Count)
            list.Add(NewValue);
        else
            list.Insert(Index, NewValue);

        if (PostExecutionAction != null)
            PostExecutionAction.Invoke(false);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        var list = (IList)Property.GetValue(ChangedObject);
        list.RemoveAt(Index);

        if (PostExecutionAction != null)
            PostExecutionAction.Invoke(true);

        return ActionEvent.NoEvent;
    }
}