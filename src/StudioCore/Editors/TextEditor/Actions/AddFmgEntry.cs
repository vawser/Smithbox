using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class AddFmgEntry : EditorAction
{
    private FMG Fmg;
    private FMG.Entry NewEntry;
    private int InsertionIndex;

    private TextContainerInfo Info;

    public AddFmgEntry(TextContainerInfo info, FMG.Entry sourceEntry, FMG.Entry newEntry, int newID)
    {
        Info = info;
        Fmg = sourceEntry.Parent;
        NewEntry = newEntry;
        NewEntry.ID = newID;
        InsertionIndex = -1;

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
        Fmg.Entries.Insert(InsertionIndex, NewEntry);
        Info.IsModified = true;

        Smithbox.EditorHandler.TextEditor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Fmg.Entries.RemoveAt(InsertionIndex);
        Info.IsModified = false;

        Smithbox.EditorHandler.TextEditor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}