using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class ChangeFmgEntryText : EditorAction
{
    private TextEditorView Parent;
    private FMG.Entry Entry;
    private string NewText;
    private string OldText;

    private TextContainerWrapper Info;

    public ChangeFmgEntryText(TextEditorView view, TextContainerWrapper info, FMG.Entry entry, string newText)
    {
        Parent = view;
        Info = info;
        Entry = entry;
        NewText = newText;
        OldText = entry.Text;
    }

    public override ActionEvent Execute()
    {
        Entry.Text = NewText;
        Info.IsModified = true;

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Text = OldText;
        Info.IsModified = false;

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}
