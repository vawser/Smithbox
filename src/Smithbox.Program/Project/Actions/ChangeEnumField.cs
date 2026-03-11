using StudioCore.Editors.Common;
using StudioCore.Editors.ParamEditor;
using System.Linq;


namespace StudioCore.Application;

public class ChangeEnumField : EditorAction
{
    private readonly ParamEnumEntry Entry;
    private readonly object NewValue;
    private readonly object OldValue;
    private readonly ProjectEnumFieldType ChangeType;

    public ChangeEnumField(ParamEnumEntry curEntry, object oldValue, object newValue, ProjectEnumFieldType changeType)
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
            case ProjectEnumFieldType.Text:
                var curLang = CFG.Current.ParamEditor_Annotation_Language;

                if(Entry.Names.Any(e => e.Language == curLang))
                {
                    var nameEntry = Entry.Names.FirstOrDefault(e => e.Language == curLang);
                    nameEntry.Text = $"{NewValue}";
                }
                break;
            case ProjectEnumFieldType.Key:
                Entry.Key = $"{NewValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        switch (ChangeType)
        {
            case ProjectEnumFieldType.Text:
                var curLang = CFG.Current.ParamEditor_Annotation_Language;

                if (Entry.Names.Any(e => e.Language == curLang))
                {
                    var nameEntry = Entry.Names.FirstOrDefault(e => e.Language == curLang);
                    nameEntry.Text = $"{OldValue}";
                }
                break;
            case ProjectEnumFieldType.Key:
                Entry.Key = $"{OldValue}";
                break;
        }

        return ActionEvent.NoEvent;
    }
}