using StudioCore.Editors.TimeActEditor;
using StudioCore.EmevdEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.EmevdEditor;

/// <summary>
/// Handles the context menus used by the view classes.
/// </summary>
public class EmevdContextMenu
{
    private EmevdEditorScreen Screen;
    private EmevdPropertyDecorator Decorator;
    private EmevdViewSelection Selection;

    public EmevdContextMenu(EmevdEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.ViewSelection;
    }
}
