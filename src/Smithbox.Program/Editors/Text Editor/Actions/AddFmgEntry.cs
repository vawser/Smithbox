using SoulsFormats;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class AddFmgEntry : EditorAction
{
    private TextEditorView Parent;

    private FMG Fmg;
    private FMG.Entry NewEntry;
    private int InsertionIndex;

    private TextContainerWrapper Info;

    private bool IgnoreDiffCheck = false;

    public AddFmgEntry(TextEditorView view, TextContainerWrapper info, FMG.Entry sourceEntry, FMG.Entry newEntry, int newID, bool ignoreDiffCheck = false)
    {
        Parent = view;
        Info = info;
        Fmg = sourceEntry.Parent;
        NewEntry = newEntry;
        NewEntry.ID = newID;
        InsertionIndex = -1;

        IgnoreDiffCheck = ignoreDiffCheck;

        for (int i = 0; i < Fmg.Entries.Count; i++)
        {
            var curEntry = Fmg.Entries[i];

            // Insert below this entry
            if (curEntry.ID > newID)
            {
                InsertionIndex = i;
                break;
            }
        }
    }

    public override ActionEvent Execute()
    {
        if (InsertionIndex != -1)
        {
            Fmg.Entries.Insert(InsertionIndex, NewEntry);
        }
        else
        {
            Fmg.Entries.Add(NewEntry);
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
        if (InsertionIndex != -1)
        {
            Fmg.Entries.RemoveAt(InsertionIndex);
        }
        else
        {
            Fmg.Entries.Remove(NewEntry);
        }

        Info.IsModified = false;

        if (!IgnoreDiffCheck)
        {
            Parent.DifferenceManager.TrackFmgDifferences();
        }

        return ActionEvent.NoEvent;
    }
}