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
///     The Behavior Editor's settings (<c>CFG.Current.BehaviorEditor</c>). Saved in Configuration.json under the same BehaviorEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class BehaviorEditorSettings : ICfgSection
{
    ///------------------------------------------------------------
    /// Behavior Editor (AC6 .bhv, and the other games' Havok behaviors)
    ///------------------------------------------------------------
    public bool BehaviorEditor_Allow_Unmodified_Save = false;
    public bool BehaviorEditor_Allow_Automatic_Save = true;
    public bool BehaviorEditor_Properties_Show_Hex = true;
    public bool BehaviorEditor_Transitions_Game_Order = true;
    public string BehaviorEditor_Merge_Last_Folder = "";
    public bool BehaviorEditor_Outline_Show_Modifiers = false;

    // Havok behaviors: the file list of every behbnd's files
    public bool BehaviorEditor_FileList_Display_Full_Path = false;
    public bool BehaviorEditor_FileList_Display_Aliases = true;
    public bool BehaviorEditor_FileList_Show_Support_Files = false;

    public bool BehaviorEditor_Viewport_Auto_Load_Character = true;
    public bool BehaviorEditor_Viewport_Play_On_Select = true;
    public bool BehaviorEditor_Viewport_Root_Motion = false;
}
