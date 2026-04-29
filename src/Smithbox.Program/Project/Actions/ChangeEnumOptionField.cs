using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using System.Linq;

namespace StudioCore.Application;

public class ChangeEnumOptionField : EditorAction
{
    private readonly ParamEnumOption OptionEntry;
    private readonly object NewValue;
    private readonly object OldValue;
    private readonly ProjectEnumOptionFieldType ChangeType;

    public ChangeEnumOptionField(ParamEnumOption curEntry, object oldValue, object newValue, ProjectEnumOptionFieldType changeType)
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
                OptionEntry.Key = $"{NewValue}";
                break;
            case ProjectEnumOptionFieldType.Name:
                var curLang = CFG.Current.ParamEditor_Annotation_Language;

                if (OptionEntry.Names.Any(e => e.Language == curLang))
                {
                    var nameEntry = OptionEntry.Names.FirstOrDefault(e => e.Language == curLang);
                    nameEntry.Text = $"{NewValue}";
                }
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case ProjectEnumOptionFieldType.ID:
                OptionEntry.Key = $"{OldValue}";
                break;
            case ProjectEnumOptionFieldType.Name:
                var curLang = CFG.Current.ParamEditor_Annotation_Language;

                if (OptionEntry.Names.Any(e => e.Language == curLang))
                {
                    var nameEntry = OptionEntry.Names.FirstOrDefault(e => e.Language == curLang);
                    nameEntry.Text = $"{OldValue}";
                }
                break;
        }

        return ActionEvent.NoEvent;
    }
}