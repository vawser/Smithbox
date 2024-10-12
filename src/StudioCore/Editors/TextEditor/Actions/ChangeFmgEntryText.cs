using SoulsFormats;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class ChangeFmgEntryText : EditorAction
{
    private FMG.Entry Entry;
    private string NewText;
    private string OldText;

    public ChangeFmgEntryText(FMG.Entry entry, string newText)
    {
        Entry = entry;
        NewText = newText;
        OldText = entry.Text;
    }

    public override ActionEvent Execute()
    {
        Entry.Text = NewText;

        Smithbox.EditorHandler.TextEditor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Text = OldText;

        Smithbox.EditorHandler.TextEditor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}
