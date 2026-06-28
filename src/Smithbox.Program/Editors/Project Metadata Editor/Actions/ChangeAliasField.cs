using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;

namespace StudioCore.Editors.MetadataEditor;

public class ChangeAliasField : EditorAction
{
    private readonly AliasEntry AliasEntry;
    private readonly object NewValue;
    private readonly object OldValue;
    private readonly ProjectAliasFieldType ChangeType;
    private readonly int TagIndex;

    public ChangeAliasField(AliasEntry curEntry, object oldValue, object newValue, ProjectAliasFieldType changeType, int tagIndex = -1)
    {
        AliasEntry = curEntry;
        NewValue = newValue;
        OldValue = oldValue;
        ChangeType = changeType;
        TagIndex = tagIndex;
    }

    public override ActionEvent Execute()
    {
        switch(ChangeType)
        {
            case ProjectAliasFieldType.ID:
                AliasEntry.ID = $"{NewValue}";
                break;
            case ProjectAliasFieldType.Name:
                AliasEntry.Name = $"{NewValue}";
                break;
            case ProjectAliasFieldType.Tags:
                AliasEntry.Tags = (List<string>)NewValue;
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case ProjectAliasFieldType.ID:
                AliasEntry.ID = $"{OldValue}";
                break;
            case ProjectAliasFieldType.Name:
                AliasEntry.Name = $"{OldValue}";
                break;
            case ProjectAliasFieldType.Tags:
                AliasEntry.Tags = (List<string>)OldValue;
                break;
        }

        return ActionEvent.NoEvent;
    }
}