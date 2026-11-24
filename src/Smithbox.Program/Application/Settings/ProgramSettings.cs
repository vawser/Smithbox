using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ProjectileEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using static StudioCore.Editors.MapEditor.DuplicateAction;

namespace StudioCore.Application;

/// <summary>
///     The program settings (<c>CFG.Current.Program</c>): file reading, the log windows and start-up warnings. Saved in Configuration.json under the same keys as before they moved here.
/// </summary>
public sealed class ProgramSettings : ICfgSection
{

    // System_
    ///------------------------------------------------------------
    /// System
    ///------------------------------------------------------------
    // Preferences
    public bool System_Ignore_Read_Asserts = false;
    public bool System_Apply_DCX_Heuristic = false;

    // Logger_
    public bool Logger_Enable_Action_Log = true;
    public bool Logger_Enable_Warning_Log = true;
    public int Logger_Action_Fade_Time = 1500;
    public int Logger_Warning_Fade_Time = 1500;
    public bool Logger_Enable_Color_Fade = true;
    public bool Logger_Enable_Log_Popups = true;
    public bool Logger_Enable_Scope_Logging = false;
    public bool Logger_Hide_File_Dictionary_Warnings = true;

    // Block_
    public bool Block_OpenGL_Warning = false;
}
