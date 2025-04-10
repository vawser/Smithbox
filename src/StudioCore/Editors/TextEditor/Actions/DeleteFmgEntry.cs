using SoulsFormats;
using StudioCore.Editor;

namespace StudioCore.Editors.TextEditor;

public class DeleteFmgEntry : EditorAction
{
    private FMG Fmg;
    private FMG.Entry Entry;
    private FMG.Entry OldEntry;
    private int InsertionIndex;

    private TextContainerWrapper Info;

    public DeleteFmgEntry(TextContainerWrapper info, FMG currentFmg, FMG.Entry entry)
    {
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

        Smithbox.EditorHandler.TextEditor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Fmg.Entries.Insert(InsertionIndex, OldEntry);
        Info.IsModified = false;

        Smithbox.EditorHandler.TextEditor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}