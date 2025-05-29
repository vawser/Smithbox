using StudioCore.Editor;
using StudioCore.Formats.JSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace StudioCore.Core;

public class ChangeEnumField : EditorAction
{
    private readonly ProjectEnumEntry Entry;
    private readonly object NewValue;
    private readonly object OldValue;
    private readonly EnumField ChangeType;

    public ChangeEnumField(ProjectEnumEntry curEntry, object oldValue, object newValue, EnumField changeType)
    {
        Entry = curEntry;
        NewValue = newValue;
        OldValue = oldValue;
        ChangeType = changeType;
    }

    public override ActionEvent Execute()
    {
        switch (ChangeType)
        {
            case EnumField.DisplayName:
                Entry.DisplayName = $"{NewValue}";
                break;
            case EnumField.Name:
                Entry.Name = $"{NewValue}";
                break;
            case EnumField.Description:
                Entry.Description = $"{NewValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case EnumField.DisplayName:
                Entry.DisplayName = $"{OldValue}";
                break;
            case EnumField.Name:
                Entry.Name = $"{OldValue}";
                break;
            case EnumField.Description:
                Entry.Description = $"{OldValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }

    public enum EnumField
    {
        DisplayName = 0,
        Name = 1,
        Description = 2
    }
}