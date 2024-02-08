using StudioCore.AnimationEditor;
using StudioCore.CutsceneEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.GraphicsEditor;
using StudioCore.Interface.Windows;
using StudioCore.MaterialEditor;
using StudioCore.MsbEditor;
using StudioCore.ParticleEditor;
using StudioCore.ScriptEditor;
using StudioCore.TalkEditor;
using StudioCore.TextEditor;
using StudioCore.TextureViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface;
public static class WindowContainer
{
    public static SettingsWindow SettingsWindow { get; set; }
    public static HelpWindow HelpWindow { get; set; }
    public static EventFlagWindow EventFlagWindow { get; set; }
    public static DebugWindow DebugWindow { get; set; }
    public static MapNameWindow MapNameWindow { get; set; }
    public static KeybindWindow KeybindWindow { get; set; }
}

