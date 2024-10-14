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

    public AddFmgEntry(FMG.Entry sourceEntry, FMG.Entry newEntry, int newID)
    {
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

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Fmg.Entries.RemoveAt(InsertionIndex);

        return ActionEvent.NoEvent;
    }
}