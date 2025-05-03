using StudioCore.TalkEditor;

namespace StudioCore.Editors.EsdEditor.Framework;

/// <summary>
/// Handles the tool view for this editor.
/// </summary>
public class EsdActionHandler
{
    private EsdEditorScreen Screen;
    private EsdPropertyDecorator Decorator;
    private EsdSelectionManager Selection;

    public EsdActionHandler(EsdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }
}
