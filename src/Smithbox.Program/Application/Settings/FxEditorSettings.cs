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
///     The FX Editor's settings (<c>CFG.Current.FxEditor</c>). Saved in Configuration.json under the same FxEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class FxEditorSettings : ICfgSection
{
    public bool FxEditor_Allow_Automatic_Save = true;

    // Effect List
    public bool FxEditor_EffectList_Display_Aliases = true;
    public bool FxEditor_EffectList_Group_By_Binder = false;

    // Action Inspector
    public bool FxEditor_EffectTree_Highlight_Selected = true;
    public bool FxEditor_EffectTree_Show_Summaries = true;
    public bool FxEditor_EffectTree_Show_Checks = true;
    public bool FxEditor_Inspector_Show_Raw = false;
    public bool FxEditor_Inspector_Show_Unknown = true;
    public bool FxEditor_Inspector_Show_Descriptions = true;

    // Viewport and playback
    public bool FxEditor_Viewport_Display_Emitter_Shapes = true;
    public bool FxEditor_Viewport_Display_Node_Axes = true;
    public bool FxEditor_Viewport_Display_Particle_Markers = false;
    public bool FxEditor_Viewport_Display_Billboards = true;
    public bool FxEditor_Viewport_Display_Screen_Effects = true;
    public bool FxEditor_Viewport_Display_Lens_Flares = true;
    public bool FxEditor_Viewport_Display_Gpu_Particles = true;
    public float FxEditor_Viewport_Billboard_Brightness = 1.0f;
    public bool FxEditor_Viewport_Display_Models = true;
    public bool FxEditor_Viewport_Display_Lights = true;
    public bool FxEditor_Viewport_Display_Light_Markers = true;
    public float FxEditor_Viewport_Light_Brightness = 1.0f;
    public float FxEditor_Playback_Speed = 1.0f;
    public bool FxEditor_Playback_Loop = true;
    public float FxEditor_Playback_Length = 5.0f;
    public int FxEditor_Preview_Motion = 0;
    public float FxEditor_Preview_Motion_Speed = 5.0f;
    public float FxEditor_Preview_Swing_Radius = 1.0f;
    public float FxEditor_Preview_Swing_Speed = 360.0f;
    public bool FxEditor_Preview_Motion_Follow = true;
    public bool FxEditor_Viewport_Auto_Frame = true;
    public bool FxEditor_Live_Preview = false;
    public bool FxEditor_InGame_AutoRefresh = true;
    public float FxEditor_InGame_RefreshSeconds = 3f; // 0.5 to 30
    public bool FxEditor_InGame_PlayingFirst = true;
    public int FxEditor_InGame_AfterReload = 0; // FxReloader.AfterReload: 0 nothing, 1 reload world, 2 quit out
    public bool FxEditor_InGame_RestartAfterReload = true;
    public bool FxEditor_Preview_Wind = false;
    public float FxEditor_Preview_Wind_Yaw = 0;
    public float FxEditor_Preview_Wind_Strength = 10;
    public bool FxEditor_Preview_Effect_Forces = true;

    // Effects are small, so the FX Editor shows a 1 m grid from the start
    public ViewportGridSet FxEditor_Grids = new()
    {
        Primary = new() { Display = true, Size = 50, SectionSize = 1, FollowCamera = true }
    };
}
