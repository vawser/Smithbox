using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Application;

public class ChangeAliasList : EditorAction
{
    private readonly List<AliasEntry> AliasSource;
    private readonly AliasEntry CurrentEntry;
    private readonly AliasEntry NewEntry;
    private readonly AliasEntry StoredEntry;
    private readonly ProjectAliasListOperation ChangeType;
    private readonly int Index;

    public ChangeAliasList(List<AliasEntry> aliasSource, AliasEntry curEntry, AliasEntry newEntry, ProjectAliasListOperation changeType, int index = 0)
    {
        AliasSource = aliasSource;
        CurrentEntry = curEntry;
        NewEntry = newEntry;
        ChangeType = changeType;
        Index = index;

        StoredEntry = new AliasEntry();
        StoredEntry.ID = curEntry.ID;
        StoredEntry.Name = curEntry.Name;
        StoredEntry.Tags = curEntry.Tags;
    }

    public override ActionEvent Execute()
    {
        switch (ChangeType)
        {
            case ProjectAliasListOperation.Add:
                AliasSource.Insert(Index, NewEntry);
                break;
            case ProjectAliasListOperation.Remove:
                AliasSource.RemoveAt(Index);
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case ProjectAliasListOperation.Add:
                AliasSource.RemoveAt(Index);
                break;
            case ProjectAliasListOperation.Remove:
                AliasSource.Insert(Index, StoredEntry);
                break;
        }

        return ActionEvent.NoEvent;
    }
}