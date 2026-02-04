using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class AddAssociatedEntry : EditorAction
{
    private TextEditorView Parent;

    private FMG Fmg;
    private FMG.Entry NewEntry;
    private int InsertionIndex;

    private TextContainerWrapper Info;

    public AddAssociatedEntry(TextEditorView view, TextContainerWrapper info, FMG targetFmg, FMG.Entry newEntry)
    {
        Parent = view;
        Info = info;
        Fmg = targetFmg;
        NewEntry = newEntry;
        InsertionIndex = 0;

        for (int i = 0; i < Fmg.Entries.Count; i++)
        {
            var curEntry = Fmg.Entries[i];

            // Insert below this entry
            if (curEntry.ID > NewEntry.ID)
            {
                InsertionIndex = i;
                break;
            }
        }
    }

    public override ActionEvent Execute()
    {
        Fmg.Entries.Insert(InsertionIndex, NewEntry);
        Info.IsModified = true;

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Fmg.Entries.RemoveAt(InsertionIndex);
        Info.IsModified = false;

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}