using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;

namespace StudioCore.Application;

public class ChangeEnumList : EditorAction
{
    private readonly ParamEnumEntry EnumSource;
    private readonly ParamEnumOption CurrentEntry;
    private readonly ParamEnumOption NewEntry;
    private readonly ParamEnumOption StoredEntry;
    private readonly ProjectEnumListOperation ChangeType;
    private readonly int Index;

    public ChangeEnumList(ParamEnumEntry enumSource, ParamEnumOption curEntry, ParamEnumOption newEntry, ProjectEnumListOperation changeType, int index = 0)
    {
        EnumSource = enumSource;
        CurrentEntry = curEntry;
        NewEntry = newEntry;
        ChangeType = changeType;
        Index = index;

        StoredEntry = new ParamEnumOption();
        StoredEntry.Key = curEntry.Key;
        StoredEntry.Names = curEntry.Names;
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