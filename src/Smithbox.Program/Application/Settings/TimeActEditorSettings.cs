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
///     The Time Act Editor's settings (<c>CFG.Current.TimeActEditor</c>). Saved in Configuration.json under the same TimeActEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class TimeActEditorSettings : ICfgSection
{
    public bool TimeActEditor_Allow_Unmodified_Save = false;
    public bool TimeActEditor_Save_Krak_As_Deflate = false;
    public bool TimeActEditor_Allow_Automatic_Save = true;
    public string TimeActEditor_Export_Directory = "";

    public float TimeActEditor_Graph_Frame_Width = 10f;
    public bool TimeActEditor_DisplayDummies = true;
    public bool TimeActEditor_DisplayNodes = true;
    public bool TimeActEditor_DisplayDummyLabels = false;
    public bool TimeActEditor_DisplayNodeLabels = false;
    public bool TimeActEditor_Graph_Display_Outside_Playback = true;
    public bool TimeActEditor_Graph_Display_Event_Names = true;
    public bool TimeActEditor_Display_Vanilla_Markers = true;
    public bool TimeActEditor_Display_Reverse_Links = true;
    public bool TimeActEditor_Auto_Play_Animation = false;
    public bool TimeActEditor_Display_Event_Markers = true;
    public bool TimeActEditor_Display_Attack_Capsules = true;
    public bool TimeActEditor_Apply_Root_Motion = true;
    public bool TimeActEditor_Display_Root_Motion = true;
    public float TimeActEditor_Root_Motion_Scale_Ground = 1f;
    public float TimeActEditor_Root_Motion_Scale_Up = 1f;
    public float TimeActEditor_Root_Motion_Scale_Turn = 1f;
    public bool TimeActEditor_Sim_Speed_Events = true;
    public bool TimeActEditor_Sim_State_Info = true;
    public bool TimeActEditor_Sim_Model_Masks = true;
    public bool TimeActEditor_Sim_Weapons = true;
    public int TimeActEditor_Weapon_Style = 1;
    public bool TimeActEditor_Sim_Turning = true;
    public float TimeActEditor_Turn_Target_Angle = 0f;
    public bool TimeActEditor_Sim_Rumble_Cam = true;
    public bool TimeActEditor_Sim_Additive_Anims = true;
    public bool TimeActEditor_Sim_Blending = true;
    public bool TimeActEditor_Sim_Behavior_Layers = true;
    public bool TimeActEditor_Blend_On_Animation_Change = false;
    public bool TimeActEditor_Sim_Opacity = true;
    public bool TimeActEditor_Assume_SpEffects = true;
    public int TimeActEditor_Floor_Material = 1;
    public bool TimeActEditor_Sim_Magic = true;
    public int TimeActEditor_Cast_Magic_ID = -1;
    public bool TimeActEditor_Cast_Magic_Charged = false;
    public bool TimeActEditor_Play_Event_Sounds = true;
    public bool TimeActEditor_Display_Effects = true;
    public float TimeActEditor_Event_Marker_Size = 1f;
    public TimeActAliasDisplayMode TimeActEditor_Animation_List_Alias_Display = TimeActAliasDisplayMode.Wrap;
    public TimeActFieldNameMode TimeActEditor_Field_Name_Mode = TimeActFieldNameMode.Annotation;
    public bool TimeActEditor_Hide_Obsolete_Fields = false;

    // Time Act Merge (Tools window > Data): the source kind (TimeActMergeSourceType), how conflicts start
    // (TimeActMergeConflictMode), the removal and clip options, and where the last mod folder or file was picked from
    public int TimeActEditor_Merge_Source_Type = 0;
    public int TimeActEditor_Merge_Conflict_Mode = 0;
    public bool TimeActEditor_Merge_Include_Removals = false;
    public bool TimeActEditor_Merge_Include_Clips = true;
    public string TimeActEditor_Merge_Last_Folder = "";

    public ViewportGridSet TimeActEditor_Grids = new();
}
