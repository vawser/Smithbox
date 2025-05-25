using StudioCore.Editor;
using StudioCore.Formats.JSON;
using System.Collections.Generic;

namespace StudioCore.Core;

public class ChangeAliasList : EditorAction
{
    private readonly List<AliasEntry> AliasSource;
    private readonly AliasEntry CurrentEntry;
    private readonly AliasEntry NewEntry;
    private readonly AliasEntry StoredEntry;
    private readonly AliasListChange ChangeType;
    private readonly int Index;

    public ChangeAliasList(List<AliasEntry> aliasSource, AliasEntry curEntry, AliasEntry newEntry, AliasListChange changeType, int index = 0)
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
            case AliasListChange.Add:
                AliasSource.Insert(Index, NewEntry);
                break;
            case AliasListChange.Remove:
                AliasSource.RemoveAt(Index);
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case AliasListChange.Add:
                AliasSource.RemoveAt(Index);
                break;
            case AliasListChange.Remove:
                AliasSource.Insert(Index, StoredEntry);
                break;
        }

        return ActionEvent.NoEvent;
    }

    public enum AliasListChange
    {
        Add,
        Remove
    }
}