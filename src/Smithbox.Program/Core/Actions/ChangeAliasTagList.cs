using StudioCore.Editor;
using System.Collections.Generic;

namespace StudioCore.Core;

public class ChangeAliasTagList : EditorAction
{
    private readonly List<string> TagSource;
    private readonly string CurrentEntry;
    private readonly string NewEntry;
    private readonly string StoredEntry;
    private readonly TagListChange ChangeType;
    private readonly int Index;

    public ChangeAliasTagList(List<string> tagSource, string curEntry, string newEntry, TagListChange changeType, int index = 0)
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
            case TagListChange.Add:
                TagSource.Insert(Index, NewEntry);
                break;
            case TagListChange.Remove:
                TagSource.RemoveAt(Index);
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case TagListChange.Add:
                TagSource.RemoveAt(Index);
                break;
            case TagListChange.Remove:
                TagSource.Insert(Index, StoredEntry);
                break;
        }

        return ActionEvent.NoEvent;
    }

    public enum TagListChange
    {
        Add,
        Remove
    }
}