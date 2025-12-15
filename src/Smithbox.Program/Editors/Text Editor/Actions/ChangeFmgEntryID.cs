using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class ChangeFmgEntryID : EditorAction
{
    private TextEditorScreen Editor;
    private FMG.Entry Entry;
    private int NewID;
    private int OldID;

    private TextContainerWrapper Info;

    public ChangeFmgEntryID(TextEditorScreen editor, TextContainerWrapper info, FMG.Entry entry, int newId)
    {
        Editor = editor;
        Info = info;
        Entry = entry;
        NewID = newId;
        OldID = entry.ID;
    }

    public override ActionEvent Execute()
    {
        Entry.ID = NewID;
        Info.IsModified = true;

        Editor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.ID = OldID;
        Info.IsModified = false;

        Editor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}
