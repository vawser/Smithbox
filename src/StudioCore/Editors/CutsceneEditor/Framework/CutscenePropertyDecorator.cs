using StudioCore.CutsceneEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.CutsceneEditor;

public class CutscenePropertyDecorator
{
    private CutsceneEditorScreen Screen;
    private CutsceneSelectionManager Selection;

    public CutscenePropertyDecorator(CutsceneEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
    }
    public void OnProjectChanged()
    {

    }
}
