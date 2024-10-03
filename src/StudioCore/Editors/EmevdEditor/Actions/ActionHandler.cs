using StudioCore.EmevdEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.EMEVD;

namespace StudioCore.Editors.EmevdEditor.Actions;

public class ActionHandler
{
    private EmevdEditorScreen Screen;

    public ActionHandler(EmevdEditorScreen screen)
    {
        Screen = screen;
    }
}
