using SoulsFormats;
using StudioCore.Editor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor.Actions;

public class AddAssociatedEntry : EditorAction
{
    private TextEditorScreen Editor;

    private FMG Fmg;
    private FMG.Entry NewEntry;
    private int InsertionIndex;

    private TextContainerWrapper Info;

    public AddAssociatedEntry(TextEditorScreen editor, TextContainerWrapper info, FMG targetFmg, FMG.Entry newEntry)
    {
        Editor = editor;
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

        Editor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Fmg.Entries.RemoveAt(InsertionIndex);
        Info.IsModified = false;

        Editor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}