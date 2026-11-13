using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Sets the values of one or more group arrays (DrawGroups / DisplayGroups) in a single undo step, each with its
/// own new values, and marks the map that owns each array as modified.
/// </summary>
public class DisplayGroupEditAction : ViewportAction, IMapHistoryItem
{
    public string Describe()
    {
        var owners = new List<Entity>();

        foreach (var change in Changes)
        {
            if (change.Owner != null && !owners.Contains(change.Owner))
                owners.Add(change.Owner);
        }

        return LOC.Get("MAP_History_Display_Groups", MapHistory.NameObjects(owners));
    }

    public class Change
    {
        /// <summary>The live array, edited in place (the renderer holds the same reference).</summary>
        public uint[] Target;
        public uint[] OldValues;
        public uint[] NewValues;

        /// <summary>The map object the array belongs to: a collision, when the groups are inherited through it.</summary>
        public Entity Owner;
    }

    private readonly List<Change> Changes;

    /// <summary>
    /// Runs after every execute, undo and redo, e.g. to refresh groups derived from the edited arrays.
    /// </summary>
    private readonly Action AfterApply;

    public DisplayGroupEditAction(List<Change> changes, Action afterApply = null)
    {
        Changes = changes;
        AfterApply = afterApply;
    }

    public override ActionEvent Execute(bool isRedo = false)
    {
        foreach (var change in Changes)
        {
            Apply(change, change.NewValues);
        }

        AfterApply?.Invoke();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (var change in Changes)
        {
            Apply(change, change.OldValues);
        }

        AfterApply?.Invoke();

        return ActionEvent.NoEvent;
    }

    private static void Apply(Change change, uint[] values)
    {
        for (var i = 0; i < change.Target.Length && i < values.Length; i++)
        {
            change.Target[i] = values[i];
        }

        MapContainer.MarkEntityDirty(change.Owner);
    }
}
