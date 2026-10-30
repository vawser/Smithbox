using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace StudioCore.Editors.MaterialEditor;

public class MatAddListEntry : EditorAction
{
    private readonly object ChangedObject;
    private readonly int Index;
    private readonly object NewValue;
    private readonly PropertyInfo Property;
    private Action<bool> PostExecutionAction;
    private readonly MaterialFileWrapper FileWrapper;

    public MatAddListEntry(MaterialFileWrapper fileWrapper, PropertyInfo prop, object changed, object newValue, int index)
    {
        FileWrapper = fileWrapper;
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

        if (FileWrapper != null)
            FileWrapper.IsDirty = true;

        if (PostExecutionAction != null)
            PostExecutionAction.Invoke(false);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        var list = (IList)Property.GetValue(ChangedObject);
        list.RemoveAt(Index);

        if (FileWrapper != null)
            FileWrapper.IsDirty = true;

        if (PostExecutionAction != null)
            PostExecutionAction.Invoke(true);

        return ActionEvent.NoEvent;
    }
}