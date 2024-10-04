using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class CutsceneFilters
{
    private CutsceneEditorScreen Screen;
    private CutscenePropertyDecorator Decorator;
    private CutsceneSelectionManager Selection;

    public CutsceneFilters(CutsceneEditorScreen screen)
    {
        Screen = screen;
        Decorator = screen.Decorator;
        Selection = screen.Selection;
    }
}
