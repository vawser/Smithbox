using StudioCore.TalkEditor;

namespace StudioCore.Editors.EsdEditor;

public class EsdFilters
{
    private EsdEditorScreen Screen;
    private EsdPropertyDecorator Decorator;
    private EsdSelectionManager Selection;

    public EsdFilters(EsdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }
}

