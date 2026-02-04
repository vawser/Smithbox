using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class DeleteFmgEntry : EditorAction
{
    private TextEditorView Parent;

    private FMG Fmg;
    private FMG.Entry Entry;
    private FMG.Entry OldEntry;
    private int InsertionIndex;

    private TextContainerWrapper Info;

    public DeleteFmgEntry(TextEditorView view, TextContainerWrapper info, FMG currentFmg, FMG.Entry entry)
    {
        Parent = view;
        Info = info;
        Fmg = currentFmg;
        Entry = entry;
        OldEntry = entry.Clone();
        InsertionIndex = Fmg.Entries.Count;

        for (int i = 0; i < Fmg.Entries.Count; i++)
        {
            var curEntry = Fmg.Entries[i];

            if (curEntry.ID == entry.ID)
            {
                InsertionIndex = i;
            }
        }
    }

    public override ActionEvent Execute()
    {
        Fmg.Entries.RemoveAt(InsertionIndex);
        Info.IsModified = true;

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Fmg.Entries.Insert(InsertionIndex, OldEntry);
        Info.IsModified = false;

        Parent.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}