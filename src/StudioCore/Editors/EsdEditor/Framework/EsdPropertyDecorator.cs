using StudioCore.TalkEditor;

namespace StudioCore.Editors.EsdEditor;

public class EsdPropertyDecorator
{
    private EsdEditorScreen Screen;
    private EsdSelectionManager Selection;

    public EsdPropertyDecorator(EsdEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
    }
}

