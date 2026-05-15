using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class ChangeFmgEntryText : EditorAction
{
    private TextEditorView Parent;
    private FMG.Entry Entry;
    private string NewText;
    private string OldText;

    private TextContainerWrapper Info;

    private bool IsSyncAction = false;
    private bool IgnoreDiffCheck = false;

    public ChangeFmgEntryText(TextEditorView view, TextContainerWrapper info, FMG.Entry entry, string newText, bool isSyncAction = false, bool ignoreDiffCheck = false)
    {
        Parent = view;
        Info = info;
        Entry = entry;
        NewText = newText;
        OldText = entry.Text;

        IsSyncAction = isSyncAction;
        IgnoreDiffCheck = ignoreDiffCheck;
    }

    public override ActionEvent Execute()
    {
        Entry.Text = NewText;
        if (IsSyncAction)
        {
            if (CFG.Current.TextEditor_Language_Sync_Apply_Prefix)
            {
                Entry.Text = $"{CFG.Current.TextEditor_Language_Sync_Prefix}{NewText}";
            }
        }

        Info.IsModified = true;

        if (!IgnoreDiffCheck)
        {
            Parent.DifferenceManager.TrackFmgDifferences();
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Text = OldText;
        Info.IsModified = false;

        if (!IgnoreDiffCheck)
        {
            Parent.DifferenceManager.TrackFmgDifferences();
        }

        return ActionEvent.NoEvent;
    }
}
