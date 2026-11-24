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
///     The Lua Editor's settings (<c>CFG.Current.LuaEditor</c>). Saved in Configuration.json under the same LuaEditor_ keys as
///     before they moved here.
/// </summary>
public sealed class LuaEditorSettings : ICfgSection
{
    ///------------------------------------------------------------
    /// Lua Editor (AI scripts in .luabnd)
    ///------------------------------------------------------------
    public bool LuaEditor_Readable_Names = true;
    public bool LuaEditor_Show_Line_Numbers = true;
    public bool LuaEditor_File_List_Display_Users = true;

    // The AI simulation's TAE playback: the effects and bullets its events spawn, and whether every event is logged
    public bool LuaEditor_TimeAct_Effects = true;
    public bool LuaEditor_TimeAct_Bullets = true;
    public bool LuaEditor_TimeAct_Log_All_Events = false;

    // The Battle Simulation's NPC and Think lists show only the rows of the character the script's filename names
    public bool LuaEditor_Limit_To_Script_Character = true;

    // The Battle Simulation's team role (ai:GetTeamOrder(ORDER_TYPE_Role)): 0 the attacker (fighting alone), 1 rotating
    // between attacking and surrounding (a group taking turns), 2 the user's (the Situation input)
    public int LuaEditor_Team_Role_Mode = 0;
    public float LuaEditor_Team_Attack_Time = 8f;
    public float LuaEditor_Team_Surround_Time = 6f;

    // The Lua Editor's First Person Mode: the user walks as the target, and their actions reach the AI
    public bool LuaEditor_FirstPerson_Crosshair = true;
    public float LuaEditor_FirstPerson_Walk_Speed = 2.5f;
    public float LuaEditor_FirstPerson_Run_Speed = 5.5f;
    public float LuaEditor_FirstPerson_Eye_Height = 1.6f;
    public float LuaEditor_FirstPerson_Jump_Speed = 4.5f;
    public float LuaEditor_FirstPerson_Roll_Distance = 3.0f;
    public float LuaEditor_FirstPerson_Roll_Time = 0.6f;

    // Each action: the interrupt fired as it starts (an INTERUPT_ type without the prefix, empty for none), the
    // SpEffects put on the target (IDs, comma separated) and how long they stay on (seconds)
    public string LuaEditor_FirstPerson_Attack_Interrupt = "FindAttack";
    public string LuaEditor_FirstPerson_Attack_SpEffects = "";
    public float LuaEditor_FirstPerson_Attack_Duration = 0.5f;
    public bool LuaEditor_FirstPerson_Attack_Damaged = true;
    public float LuaEditor_FirstPerson_Attack_Reach = 3.0f;

    public string LuaEditor_FirstPerson_Guard_Interrupt = "TargetIsGuard";
    public string LuaEditor_FirstPerson_Guard_SpEffects = "";
    public float LuaEditor_FirstPerson_Guard_Duration = 0.2f;

    public string LuaEditor_FirstPerson_Estus_Interrupt = "UseItem";
    public string LuaEditor_FirstPerson_Estus_SpEffects = "";
    public float LuaEditor_FirstPerson_Estus_Duration = 2.0f;

    public string LuaEditor_FirstPerson_Roll_Interrupt = "";
    public string LuaEditor_FirstPerson_Roll_SpEffects = "";
    public float LuaEditor_FirstPerson_Roll_Duration = 0.6f;

    public string LuaEditor_FirstPerson_Jump_Interrupt = "";
    public string LuaEditor_FirstPerson_Jump_SpEffects = "";
    public float LuaEditor_FirstPerson_Jump_Duration = 0.8f;

    // The Lua Editor's Viewport Filters: what the simulation viewport shows (none of it changes the simulation)
    public bool LuaEditor_Display_Tae_Effects = true;
    public bool LuaEditor_Display_Bullet_Effects = true;
    public bool LuaEditor_Display_Hit_Effects = true;
    public bool LuaEditor_Display_Bullets = true;
    public bool LuaEditor_Display_Reach = false;
    public bool LuaEditor_Display_Trails = true;
    public bool LuaEditor_Display_Impacts = true;
    public bool LuaEditor_Display_Target = true;

    // The simulation's clips blend into each other: over each clip's TAE Blend event, else this long (seconds)
    public bool LuaEditor_Blend_Animations = true;
    public float LuaEditor_Blend_Default_Time = 0.2f;

    // The Lua Editor's AI simulation: a character and its target a few metres apart, on a 1 m grid from the start
    public ViewportGridSet LuaEditor_Grids = new()
    {
        Primary = new() { Display = true, Size = 50, SectionSize = 1, FollowCamera = true }
    };
}
