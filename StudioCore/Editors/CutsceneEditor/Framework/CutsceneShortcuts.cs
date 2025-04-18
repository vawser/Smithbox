using StudioCore.CutsceneEditor;
using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class CutsceneShortcuts
{
    private CutsceneEditorScreen Screen;
    private CutscenePropertyDecorator Decorator;
    private CutsceneSelectionManager Selection;
    private ActionManager EditorActionManager;

    public CutsceneShortcuts(CutsceneEditorScreen screen)
    {
        EditorActionManager = screen.EditorActionManager;
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }

    /// <summary>
    /// Reset any shortcut state on project change
    /// </summary>
    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// Handle shortcuts for the editor
    /// </summary>
    public void Monitor()
    {

    }
}
