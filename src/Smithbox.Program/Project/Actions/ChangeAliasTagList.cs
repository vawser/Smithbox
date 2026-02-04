using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Application;

public class ChangeAliasTagList : EditorAction
{
    private readonly List<string> TagSource;
    private readonly string CurrentEntry;
    private readonly string NewEntry;
    private readonly string StoredEntry;
    private readonly ProjectAliasTagListOperation ChangeType;
    private readonly int Index;

    public ChangeAliasTagList(List<string> tagSource, string curEntry, string newEntry, ProjectAliasTagListOperation changeType, int index = 0)
    {
        TagSource = tagSource;
        CurrentEntry = curEntry;
        NewEntry = newEntry;
        ChangeType = changeType;
        Index = index;

        StoredEntry = curEntry;
    }

    public override ActionEvent Execute()
    {
        switch (ChangeType)
        {
            case ProjectAliasTagListOperation.Add:
                TagSource.Insert(Index, NewEntry);
                break;
            case ProjectAliasTagListOperation.Remove:
                TagSource.RemoveAt(Index);
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case ProjectAliasTagListOperation.Add:
                TagSource.RemoveAt(Index);
                break;
            case ProjectAliasTagListOperation.Remove:
                TagSource.Insert(Index, StoredEntry);
                break;
        }

        return ActionEvent.NoEvent;
    }
}