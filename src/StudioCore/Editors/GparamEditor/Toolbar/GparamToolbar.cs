using ImGuiNET;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Toolbar;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.GparamEditor.Toolbar
{
    public enum GparamToolbarAction
    {
        None
    }

    public class GparamToolbar
    {
        public static ActionManager EditorActionManager;

        public GparamToolbar(ActionManager actionManager)
        {
            EditorActionManager = actionManager;
        }
    }
}
