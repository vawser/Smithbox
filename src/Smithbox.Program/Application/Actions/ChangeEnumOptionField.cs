using StudioCore.Editors.Common;

namespace StudioCore.Application;

public class ChangeEnumOptionField : EditorAction
{
    private readonly ProjectEnumOption OptionEntry;
    private readonly object NewValue;
    private readonly object OldValue;
    private readonly ProjectEnumOptionFieldType ChangeType;

    public ChangeEnumOptionField(ProjectEnumOption curEntry, object oldValue, object newValue, ProjectEnumOptionFieldType changeType)
    {
        OptionEntry = curEntry;
        NewValue = newValue;
        OldValue = oldValue;
        ChangeType = changeType;
    }

    public override ActionEvent Execute()
    {
        switch (ChangeType)
        {
            case ProjectEnumOptionFieldType.ID:
                OptionEntry.ID = $"{NewValue}";
                break;
            case ProjectEnumOptionFieldType.Name:
                OptionEntry.Name = $"{NewValue}";
                break;
            case ProjectEnumOptionFieldType.Description:
                OptionEntry.Description = $"{NewValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case ProjectEnumOptionFieldType.ID:
                OptionEntry.ID = $"{OldValue}";
                break;
            case ProjectEnumOptionFieldType.Name:
                OptionEntry.Name = $"{OldValue}";
                break;
            case ProjectEnumOptionFieldType.Description:
                OptionEntry.Description = $"{OldValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }
}