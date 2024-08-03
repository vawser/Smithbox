using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

public class ActionHandler
{
    private TimeActEditorScreen Screen;
    private ActionManager EditorActionManager;

    public ActionHandler(TimeActEditorScreen screen, ActionManager manager)
    {
        Screen = screen;
        EditorActionManager = manager;
    }
}
