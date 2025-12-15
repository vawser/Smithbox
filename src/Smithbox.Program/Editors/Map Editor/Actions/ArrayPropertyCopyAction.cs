using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

/// <summary>
///     Copies values from one array to another without affecting references.
/// </summary>
public class ArrayPropertyCopyAction : ViewportAction
{
    private readonly List<PropertyChange> Changes = new();
    private Action<bool> PostExecutionAction;

    public ArrayPropertyCopyAction(Array source, Array target)
    {
        for (var i = 0; i < target.Length; i++)
        {
            PropertyChange change = new()
            {
                ChangedObj = target,
                OldVal = target.GetValue(i),
                NewVal = source.GetValue(i),
                ArrayIndex = i
            };
            Changes.Add(change);
        }
    }

    public ArrayPropertyCopyAction(Array source, IEnumerable<Array> targetList)
    {
        foreach (Array target in targetList)
        {
            for (var i = 0; i < target.Length; i++)
            {
                PropertyChange change = new()
                {
                    ChangedObj = target,
                    OldVal = target.GetValue(i),
                    NewVal = source.GetValue(i),
                    ArrayIndex = i
                };
                Changes.Add(change);
            }
        }
    }

    public void SetPostExecutionAction(Action<bool> action)
    {
        PostExecutionAction = action;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (PropertyChange change in Changes)
        {
            change.ChangedObj.SetValue(change.NewVal, change.ArrayIndex);
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
            change.ChangedObj.SetValue(change.OldVal, change.ArrayIndex);
        }

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return ActionEvent.NoEvent;
    }

    public override string GetEditMessage()
    {
        return "";
    }

    private class PropertyChange
    {
        public int ArrayIndex;
        public Array ChangedObj;
        public object NewVal;
        public object OldVal;
    }
}
