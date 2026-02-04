using StudioCore.Editors.Common;

namespace StudioCore.Editors.TextEditor;

public class SortFmgList : EditorAction
{
    private TextFmgWrapper Wrapper;

    public SortFmgList(TextFmgWrapper wrapper)
    {
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