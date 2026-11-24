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
///     The Cutscene Editor's settings (<c>CFG.Current.CutsceneEditor</c>). Saved in Configuration.json under the same CutsceneEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class CutsceneEditorSettings : ICfgSection
{
    // ---- Cutscene Editor
    public bool CutsceneEditor_Look_Through_Camera = true;

    // The local axes of a cutscene camera bone that it looks and points up along (CutsceneCameraAxis). Forward -Y was
    // found by eye in the app; up +Z is the axis that lines up with world up (0.96 on average over ER s10_00_0020's
    // camera frames, CutsceneSceneCheck --axes). Both stay options, in case another game differs.
    public int CutsceneEditor_Camera_Forward_Axis = 3; // -Y
    public int CutsceneEditor_Camera_Up_Axis = 4; // +Z

    // The camNNNN_FOV track gives a horizontal rather than a vertical field of view
    public bool CutsceneEditor_Camera_Fov_Is_Horizontal = false;

    public bool CutsceneEditor_Load_Models = true;
    public bool CutsceneEditor_Load_Map = true;
    public bool CutsceneEditor_Map_Load_Assets = true;

    // The map's own characters in the backdrop (static, with their NpcParam masks), and drawing only the parts in the
    // draw groups of the collision under the cut camera's start
    public bool CutsceneEditor_Map_Load_Characters = true;
    public bool CutsceneEditor_Map_Use_Draw_Groups = true;
    public bool CutsceneEditor_Display_Camera_Path = true;
    public bool CutsceneEditor_Display_Camera = true;
    public bool CutsceneEditor_Display_Markers = true;

    // Subtitle events: their TalkParam text at the bottom of the viewport, and their voice while the cut plays
    public bool CutsceneEditor_Display_Subtitles = true;
    public bool CutsceneEditor_Play_Voices = true;

    // Fade events (Fade-in, Fade-out, Fade-in/out): their colour laid over the viewport
    public bool CutsceneEditor_Display_Fades = true;

    // Sound events (seId/seType): played as the cut plays past their start
    public bool CutsceneEditor_Play_Sounds = true;

    // Effect events (sfxId): the effect drawn on its actor's dummy poly while the event runs (needs the FX Editor's data)
    public bool CutsceneEditor_Display_Effects = true;
    public bool CutsceneEditor_Loop = true;
    public bool CutsceneEditor_Continue_To_Next_Cut = false;
    public float CutsceneEditor_Playback_Speed = 1.0f;

    // Writes what each cut is built from (actors, cameras, placement, the camera's first frame) to the log
    public bool CutsceneEditor_Diagnostics_Log = false;
}
