using ImGuiNET;
using StudioCore.Editor;
using StudioCore.GraphicsEditor;
using StudioCore.Interface;
using StudioCore.ParticleEditor;
using StudioCore.Platform;
using StudioCore.UserProject;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParticleEditor.Toolbar;

public enum ParticleToolbarAction
{
    None
}

public class ParticleToolbar
{
    public static ActionManager EditorActionManager;

    public ParticleToolbar(ActionManager actionManager)
    {
        EditorActionManager = actionManager;
    }
}