using StudioCore.Editor;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Core;

public class ChangeEnumList : EditorAction
{
    private readonly ProjectEnumEntry EnumSource;
    private readonly ProjectEnumOption CurrentEntry;
    private readonly ProjectEnumOption NewEntry;
    private readonly ProjectEnumOption StoredEntry;
    private readonly EnumListChange ChangeType;
    private readonly int Index;

    public ChangeEnumList(ProjectEnumEntry enumSource, ProjectEnumOption curEntry, ProjectEnumOption newEntry, EnumListChange changeType, int index = 0)
    {
        EnumSource = enumSource;
        CurrentEntry = curEntry;
        NewEntry = newEntry;
        ChangeType = changeType;
        Index = index;

        StoredEntry = new ProjectEnumOption();
        StoredEntry.ID = curEntry.ID;
        StoredEntry.Name = curEntry.Name;
        StoredEntry.Description = curEntry.Description;
    }

    public override ActionEvent Execute()
    {
        switch (ChangeType)
        {
            case EnumListChange.Add:
                EnumSource.Options.Insert(Index, NewEntry);
                break;
            case EnumListChange.Remove:
                EnumSource.Options.RemoveAt(Index);
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case EnumListChange.Add:
                EnumSource.Options.RemoveAt(Index);
                break;
            case EnumListChange.Remove:
                EnumSource.Options.Insert(Index, StoredEntry);
                break;
        }

        return ActionEvent.NoEvent;
    }

    public enum EnumListChange
    {
        Add,
        Remove
    }
}