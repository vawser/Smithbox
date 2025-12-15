using StudioCore.Editors.Common;

namespace StudioCore.Application;

public class ChangeEnumOptionField : EditorAction
{
    private readonly ProjectEnumOption OptionEntry;
    private readonly object NewValue;
    private readonly object OldValue;
    private readonly EnumOptionField ChangeType;

    public ChangeEnumOptionField(ProjectEnumOption curEntry, object oldValue, object newValue, EnumOptionField changeType)
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
            case EnumOptionField.ID:
                OptionEntry.ID = $"{NewValue}";
                break;
            case EnumOptionField.Name:
                OptionEntry.Name = $"{NewValue}";
                break;
            case EnumOptionField.Description:
                OptionEntry.Description = $"{NewValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case EnumOptionField.ID:
                OptionEntry.ID = $"{OldValue}";
                break;
            case EnumOptionField.Name:
                OptionEntry.Name = $"{OldValue}";
                break;
            case EnumOptionField.Description:
                OptionEntry.Description = $"{OldValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }

    public enum EnumOptionField
    {
        ID = 0,
        Name = 1,
        Description = 2
    }
}