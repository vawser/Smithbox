using StudioCore.Editors.Common;


namespace StudioCore.Application;

public class ChangeEnumField : EditorAction
{
    private readonly ProjectEnumEntry Entry;
    private readonly object NewValue;
    private readonly object OldValue;
    private readonly ProjectEnumFieldType ChangeType;

    public ChangeEnumField(ProjectEnumEntry curEntry, object oldValue, object newValue, ProjectEnumFieldType changeType)
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
            case ProjectEnumFieldType.DisplayName:
                Entry.DisplayName = $"{NewValue}";
                break;
            case ProjectEnumFieldType.Name:
                Entry.Name = $"{NewValue}";
                break;
            case ProjectEnumFieldType.Description:
                Entry.Description = $"{NewValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case ProjectEnumFieldType.DisplayName:
                Entry.DisplayName = $"{OldValue}";
                break;
            case ProjectEnumFieldType.Name:
                Entry.Name = $"{OldValue}";
                break;
            case ProjectEnumFieldType.Description:
                Entry.Description = $"{OldValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }
}