using HKLib.hk2018.hkAsyncThreadPool;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class ChangeFmgEntryText : EditorAction
{
    private TextEditorScreen Editor;
    private FMG.Entry Entry;
    private string NewText;
    private string OldText;

    private TextContainerWrapper Info;

    public ChangeFmgEntryText(TextEditorScreen editor, TextContainerWrapper info, FMG.Entry entry, string newText)
    {
        Editor = editor;
        Info = info;
        Entry = entry;
        NewText = newText;
        OldText = entry.Text;
    }

    public override ActionEvent Execute()
    {
        Entry.Text = NewText;
        Info.IsModified = true;

        Editor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Entry.Text = OldText;
        Info.IsModified = false;

        Editor.DifferenceManager.TrackFmgDifferences();

        return ActionEvent.NoEvent;
    }
}
