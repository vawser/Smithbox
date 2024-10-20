using HKLib.hk2018.hkAsyncThreadPool;
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

    private TextContainerWrapper Info;

    public ChangeFmgEntryText(TextContainerWrapper info, FMG.Entry entry, string newText)
    {
        Info = info;
        Entry = entry;
        NewText = newText;
        OldText = entry.Text;
    }

    public override ActionEvent Execute()
    {
        Entry.Text = NewText;
        Info.IsModified = true;

        Smithbox.EditorHandler.TextEditor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Text = OldText;
        Info.IsModified = false;

        Smithbox.EditorHandler.TextEditor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}
