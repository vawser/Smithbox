using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class ChangeFmgEntryID : EditorAction
{
    private TextEditorView Parent;
    private FMG.Entry Entry;
    private int NewID;
    private int OldID;

    private TextContainerWrapper Info;

    public ChangeFmgEntryID(TextEditorView view, TextContainerWrapper info, FMG.Entry entry, int newId)
    {
        Parent = view;
        Info = info;
        Entry = entry;
        NewID = newId;
        OldID = entry.ID;
    }

    public override ActionEvent Execute()
    {
        Entry.ID = NewID;
        Info.IsModified = true;

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.ID = OldID;
        Info.IsModified = false;

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}
