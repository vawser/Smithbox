using StudioCore.Editor;
using StudioCore.Resources.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Core.ProjectNS;

public class ChangeAliasField : EditorAction
{
    private readonly AliasEntry AliasEntry;
    private readonly object NewValue;
    private readonly object OldValue;
    private readonly AliasField ChangeType;
    private readonly int TagIndex;

    public ChangeAliasField(AliasEntry curEntry, object oldValue, object newValue, AliasField changeType, int tagIndex = -1)
    {
        AliasEntry = curEntry;
        NewValue = newValue;
        OldValue = oldValue;
        ChangeType = changeType;
        TagIndex = tagIndex;
    }

    public override ActionEvent Execute()
    {
        switch (ChangeType)
        {
            case AliasField.ID:
                AliasEntry.ID = $"{NewValue}";
                break;
            case AliasField.Name:
                AliasEntry.Name = $"{NewValue}";
                break;
            case AliasField.Tags:
                AliasEntry.Tags[TagIndex] = $"{NewValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case AliasField.ID:
                AliasEntry.ID = $"{OldValue}";
                break;
            case AliasField.Name:
                AliasEntry.Name = $"{OldValue}";
                break;
            case AliasField.Tags:
                AliasEntry.Tags[TagIndex] = $"{OldValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }

    public enum AliasField
    {
        ID = 0,
        Name = 1,
        Tags = 2
    }
}

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