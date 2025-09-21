using DotNext.Collections.Generic;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextEditor;

public class SortFmgList : EditorAction
{
    private TextEditorScreen Editor;

    private TextFmgWrapper Wrapper;

    public SortFmgList(TextEditorScreen editor, TextFmgWrapper wrapper)
    {
        Editor = editor;
        Wrapper = wrapper;
    }

    public override ActionEvent Execute()
    {
        Wrapper.File.Entries.Sort();

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        return ActionEvent.NoEvent;
    }
}