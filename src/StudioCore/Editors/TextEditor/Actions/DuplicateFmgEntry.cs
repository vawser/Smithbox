using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class DuplicateFmgEntry : EditorAction
{
    private FMG Fmg;
    private FMG.Entry Entry;
    private FMG.Entry NewEntry;
    private int InsertionIndex;

    public DuplicateFmgEntry(FMG currentFmg, FMG.Entry entry, int newID)
    {
        Fmg = currentFmg;
        Entry = entry;
        NewEntry = entry.Clone();
        NewEntry.ID = newID;
        InsertionIndex = -1;

        for (int i = 0; i < Fmg.Entries.Count; i++)
        {
            var curEntry = Fmg.Entries[i];

            if(curEntry.ID == entry.ID)
            {
                InsertionIndex = i;
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