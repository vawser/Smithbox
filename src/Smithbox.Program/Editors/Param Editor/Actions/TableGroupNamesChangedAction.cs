using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// An undoable change to one param's table group names (a rename, or moving a name to another ID).
/// <para>
/// The names live in the project's table name store, not in the param, so this snapshots that param's name
/// entries before and after the change. Undo / redo swap the snapshots in, which covers any change the
/// <c>change</c> callback makes. If the param had no name group yet, it's created on execute and removed on undo.
/// </para>
/// <para>
/// The names are written to disk with the params on save; this doesn't mark the param dirty, since its row data
/// hasn't changed.
/// </para>
/// </summary>
public class TableGroupNamesChangedAction : EditorAction
{
    private readonly ParamEditorScreen Editor;
    private readonly TableGroupNameStore Store;
    private readonly string ParamKey;
    private readonly Action<TableGroupParamEntry> Change;

    // The param's name group: found or created on the first execute, then reused, so redo re-adds the same object
    private TableGroupParamEntry Group;
    private bool CreatedGroup;

    private List<(int ID, string Name)> Before;
    private List<(int ID, string Name)> After;
    private bool HasExecuted;

    public TableGroupNamesChangedAction(ParamEditorScreen editor, TableGroupNameStore store, string paramKey,
        Action<TableGroupParamEntry> change)
    {
        Editor = editor;
        Store = store;
        ParamKey = paramKey;
        Change = change;
    }

    public override ActionEvent Execute()
    {
        if (Store?.Groups == null)
            return ActionEvent.NoEvent;

        if (!HasExecuted)
        {
            Group = Store.Groups.FirstOrDefault(e => e.Param == ParamKey);

            if (Group == null)
            {
                Group = new TableGroupParamEntry { Param = ParamKey, Entries = new() };
                CreatedGroup = true;
            }

            Group.Entries ??= new();

            Before = Snapshot(Group);

            if (CreatedGroup)
            {
                Store.Groups.Add(Group);
            }

            Change(Group);

            After = Snapshot(Group);
            HasExecuted = true;
        }
        else
        {
            if (CreatedGroup && !Store.Groups.Contains(Group))
            {
                Store.Groups.Add(Group);
            }

            Restore(Group, After);
        }

        RefreshViews();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        if (Store?.Groups == null || Group == null)
            return ActionEvent.NoEvent;

        if (CreatedGroup)
        {
            Store.Groups.Remove(Group);
        }
        else
        {
            Restore(Group, Before);
        }

        RefreshViews();

        return ActionEvent.NoEvent;
    }

    private static List<(int ID, string Name)> Snapshot(TableGroupParamEntry group)
    {
        return group.Entries.Select(e => (e.ID, e.Name)).ToList();
    }

    private static void Restore(TableGroupParamEntry group, List<(int ID, string Name)> snapshot)
    {
        group.Entries = snapshot.Select(e => new TableGroupEntry { ID = e.ID, Name = e.Name }).ToList();
    }

    /// <summary>
    /// The store is shared by every view, so each view's cached name lookup must be rebuilt
    /// </summary>
    private void RefreshViews()
    {
        foreach (var view in Editor.ViewHandler.ParamEditorViews)
        {
            view?.ParamTableWindow.InvalidateTableNames();
        }
    }
}
