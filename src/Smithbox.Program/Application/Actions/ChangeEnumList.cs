using StudioCore.Editors.Common;

namespace StudioCore.Application;

public class ChangeEnumList : EditorAction
{
    private readonly ProjectEnumEntry EnumSource;
    private readonly ProjectEnumOption CurrentEntry;
    private readonly ProjectEnumOption NewEntry;
    private readonly ProjectEnumOption StoredEntry;
    private readonly ProjectEnumListOperation ChangeType;
    private readonly int Index;

    public ChangeEnumList(ProjectEnumEntry enumSource, ProjectEnumOption curEntry, ProjectEnumOption newEntry, ProjectEnumListOperation changeType, int index = 0)
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
            case ProjectEnumListOperation.Add:
                EnumSource.Options.Insert(Index, NewEntry);
                break;
            case ProjectEnumListOperation.Remove:
                EnumSource.Options.RemoveAt(Index);
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case ProjectEnumListOperation.Add:
                EnumSource.Options.RemoveAt(Index);
                break;
            case ProjectEnumListOperation.Remove:
                EnumSource.Options.Insert(Index, StoredEntry);
                break;
        }

        return ActionEvent.NoEvent;
    }
}