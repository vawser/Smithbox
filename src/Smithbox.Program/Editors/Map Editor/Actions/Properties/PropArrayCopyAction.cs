using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

/// <summary>
///     Copies values from one array to another without affecting references.
/// </summary>
public class PropArrayCopyAction : ViewportAction, IMapHistoryItem
{
    public string Describe()
    {
        return Owner != null
            ? LOC.Get("MAP_History_Copy_Array", MapHistory.NameObject(Owner))
            : LOC.Get("MAP_History_Copy_Array_No_Object");
    }

    private readonly List<PropertyChange> Changes = new();
    private Action<bool> PostExecutionAction;

    // The entity whose data holds the target array, marked as modified on execute and undo so the map is saved
    private readonly Entity Owner;

    public PropArrayCopyAction(Array source, Array target, Entity owner = null)
    {
        Owner = owner;

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

    public PropArrayCopyAction(Array source, IEnumerable<Array> targetList)
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

        MapContainer.MarkEntityDirty(Owner);

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

        MapContainer.MarkEntityDirty(Owner);

        if (PostExecutionAction != null)
        {
            PostExecutionAction.Invoke(true);
        }

        return ActionEvent.NoEvent;
    }

    private class PropertyChange
    {
        public int ArrayIndex;
        public Array ChangedObj;
        public object NewVal;
        public object OldVal;
    }
}
