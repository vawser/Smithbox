using StudioCore.Editors.Common;
using StudioCore.Editors.HavokEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.MapEditor;

public class MapHavokAddListEntryAction : ViewportAction
{
    private readonly object ChangedObject;
    private readonly int Index;
    private readonly object NewValue;
    private readonly FieldInfo Property;
    private Action<bool> PostExecutionAction;

    public MapHavokAddListEntryAction(FieldInfo prop, object changed, object newValue, int index)
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

    public override ActionEvent Execute(bool isRedo = false)
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