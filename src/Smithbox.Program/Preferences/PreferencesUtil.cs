using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public static class PreferencesUtil
{
    // Some fields used in prefs that need to be filled once at session start.
    public static float TempScale;
    public static string NewThemeName = "";
    public static string CurrentThemeName = "";

    public static void Setup()
    {
        TempScale = CFG.Current.Interface.Interface_UI_Scale;
        CurrentThemeName = CFG.Current.Interface.Interface_Selected_Theme;
    }

    public static void ResetViewportGeneralCFG()
    {
        CFG.Current.Viewport.Viewport_Frame_Rate = CFG.Default.Viewport.Viewport_Frame_Rate;
    }

    public static void ResetViewportRenderingCFG()
    {
        CFG.Current.Viewport.Viewport_Enable_Rendering = CFG.Default.Viewport.Viewport_Enable_Rendering;
        CFG.Current.Viewport.Viewport_Enable_Texturing = CFG.Default.Viewport.Viewport_Enable_Texturing;
        CFG.Current.Viewport.Viewport_Enable_Culling = CFG.Default.Viewport.Viewport_Enable_Culling;
        CFG.Current.Viewport.Viewport_Limit_Buffer_Indirect_Draw = CFG.Default.Viewport.Viewport_Limit_Buffer_Indirect_Draw;
        CFG.Current.Viewport.Viewport_Limit_Buffer_Flver_Bone = CFG.Default.Viewport.Viewport_Limit_Buffer_Flver_Bone;
    }

    public static void ResetViewportModelRenderingCFG()
    {
        CFG.Current.Viewport.Viewport_Enable_Model_Masks = CFG.Default.Viewport.Viewport_Enable_Model_Masks;
        CFG.Current.Viewport.Viewport_Enable_LOD_Facesets = CFG.Default.Viewport.Viewport_Enable_LOD_Facesets;
        CFG.Current.Viewport.Viewport_Flat_Model_Brightness = CFG.Default.Viewport.Viewport_Flat_Model_Brightness;
        CFG.Current.Viewport.Viewport_Flat_Model_Saturation = CFG.Default.Viewport.Viewport_Flat_Model_Saturation;
    }
    public static void ResetViewportSelectionCFG()
    {
        CFG.Current.Viewport.Viewport_Enable_Selection_Tint = CFG.Default.Viewport.Viewport_Enable_Selection_Tint;
        CFG.Current.Viewport.Viewport_Enable_Selection_Outline = CFG.Default.Viewport.Viewport_Enable_Selection_Outline;
        CFG.Current.Viewport.Viewport_Enable_Box_Selection = CFG.Default.Viewport.Viewport_Enable_Box_Selection;
        CFG.Current.Viewport.Viewport_Frame_Distance = CFG.Default.Viewport.Viewport_Frame_Distance;
        CFG.Current.Viewport.Viewport_Frame_Offset = CFG.Default.Viewport.Viewport_Frame_Offset;
    }

    public static void ResetViewportColoringCFG()
    {;
        CFG.Current.Viewport.Viewport_Bg_Color = CFG.Default.Viewport.Viewport_Bg_Color;
        CFG.Current.Viewport.Viewport_Selection_Outline_Color = CFG.Default.Viewport.Viewport_Selection_Outline_Color;
        CFG.Current.Viewport.Viewport_Selection_Tint_Color = CFG.Default.Viewport.Viewport_Selection_Tint_Color;
        CFG.Current.Viewport.Viewport_Untextured_Selection_Tint_Color = CFG.Default.Viewport.Viewport_Untextured_Selection_Tint_Color;
        CFG.Current.Viewport.Viewport_Selection_Tint_Strength = CFG.Default.Viewport.Viewport_Selection_Tint_Strength;

        CFG.Current.Graphics.GFX_Renderable_Default_Wireframe_Alpha = CFG.Default.Graphics.GFX_Renderable_Default_Wireframe_Alpha;

        CFG.Current.Viewport.Viewport_Collision_Color = CFG.Default.Viewport.Viewport_Collision_Color;
        CFG.Current.Viewport.Viewport_High_Collision_Color = CFG.Default.Viewport.Viewport_High_Collision_Color;
        CFG.Current.Viewport.Viewport_FallProtection_Collision_Color = CFG.Default.Viewport.Viewport_FallProtection_Collision_Color;
        CFG.Current.Viewport.Viewport_Connect_Collision_Color = CFG.Default.Viewport.Viewport_Connect_Collision_Color;
        CFG.Current.Viewport.Viewport_Navmesh_Color = CFG.Default.Viewport.Viewport_Navmesh_Color;
        CFG.Current.Viewport.Viewport_Navmesh_Gate_Color = CFG.Default.Viewport.Viewport_Navmesh_Gate_Color;
        CFG.Current.Viewport.Viewport_Collision_Opacity = CFG.Default.Viewport.Viewport_Collision_Opacity;

        CFG.Current.Viewport.Viewport_Box_Region_Base_Color = CFG.Default.Viewport.Viewport_Box_Region_Base_Color;
        CFG.Current.Viewport.Viewport_Box_Region_Highlight_Color = CFG.Default.Viewport.Viewport_Box_Region_Highlight_Color;

        CFG.Current.Viewport.Viewport_Cylinder_Region_Base_Color = CFG.Default.Viewport.Viewport_Cylinder_Region_Base_Color;
        CFG.Current.Viewport.Viewport_Cylinder_Region_Highlight_Color = CFG.Default.Viewport.Viewport_Cylinder_Region_Highlight_Color;

        CFG.Current.Viewport.Viewport_Sphere_Region_Base_Color = CFG.Default.Viewport.Viewport_Sphere_Region_Base_Color;
        CFG.Current.Viewport.Viewport_Sphere_Region_Highlight_Color = CFG.Default.Viewport.Viewport_Sphere_Region_Highlight_Color;

        CFG.Current.Viewport.Viewport_Point_Region_Base_Color = CFG.Default.Viewport.Viewport_Point_Region_Base_Color;
        CFG.Current.Viewport.Viewport_Point_Region_Highlight_Color = CFG.Default.Viewport.Viewport_Point_Region_Highlight_Color;

        CFG.Current.Viewport.Viewport_Region_Selection_Fill_Alpha = CFG.Default.Viewport.Viewport_Region_Selection_Fill_Alpha;

        CFG.Current.Viewport.Viewport_Dummy_Polygon_Base_Color = CFG.Default.Viewport.Viewport_Dummy_Polygon_Base_Color;
        CFG.Current.Viewport.Viewport_Dummy_Polygon_Highlight_Color = CFG.Default.Viewport.Viewport_Dummy_Polygon_Highlight_Color;
        CFG.Current.Viewport.Viewport_Dummy_Polygon_Alpha = CFG.Default.Viewport.Viewport_Dummy_Polygon_Alpha;

        CFG.Current.Viewport.Viewport_Bone_Marker_Base_Color = CFG.Default.Viewport.Viewport_Bone_Marker_Base_Color;
        CFG.Current.Viewport.Viewport_Bone_Marker_Highlight_Color = CFG.Default.Viewport.Viewport_Bone_Marker_Highlight_Color;
        CFG.Current.Viewport.Viewport_Bone_Marker_Alpha = CFG.Default.Viewport.Viewport_Bone_Marker_Alpha;

        CFG.Current.Viewport.Viewport_Character_Marker_Base_Color = CFG.Default.Viewport.Viewport_Character_Marker_Base_Color;
        CFG.Current.Viewport.Viewport_Character_Marker_Highlight_Color = CFG.Default.Viewport.Viewport_Character_Marker_Highlight_Color;
        CFG.Current.Viewport.Viewport_Character_Marker_Alpha = CFG.Default.Viewport.Viewport_Character_Marker_Alpha;

        CFG.Current.Viewport.Viewport_Object_Marker_Base_Color = CFG.Default.Viewport.Viewport_Object_Marker_Base_Color;
        CFG.Current.Viewport.Viewport_Object_Marker_Highlight_Color = CFG.Default.Viewport.Viewport_Object_Marker_Highlight_Color;
        CFG.Current.Viewport.Viewport_Object_Marker_Alpha = CFG.Default.Viewport.Viewport_Object_Marker_Alpha;

        CFG.Current.Viewport.Viewport_Player_Marker_Base_Color = CFG.Default.Viewport.Viewport_Player_Marker_Base_Color;
        CFG.Current.Viewport.Viewport_Player_Marker_Highlight_Color = CFG.Default.Viewport.Viewport_Player_Marker_Highlight_Color;
        CFG.Current.Viewport.Viewport_Player_Marker_Alpha = CFG.Default.Viewport.Viewport_Player_Marker_Alpha;

        CFG.Current.Viewport.Viewport_Other_Marker_Base_Color = CFG.Default.Viewport.Viewport_Other_Marker_Base_Color;
        CFG.Current.Viewport.Viewport_Other_Marker_Highlight_Color = CFG.Default.Viewport.Viewport_Other_Marker_Highlight_Color;
        CFG.Current.Viewport.Viewport_Other_Marker_Alpha = CFG.Default.Viewport.Viewport_Other_Marker_Alpha;

        CFG.Current.Viewport.Viewport_Point_Light_Highlight_Color = CFG.Default.Viewport.Viewport_Point_Light_Highlight_Color;

        CFG.Current.Viewport.Viewport_Spot_Light_Highlight_Color = CFG.Default.Viewport.Viewport_Spot_Light_Highlight_Color;

        CFG.Current.Viewport.Viewport_Directional_Light_Highlight_Color = CFG.Default.Viewport.Viewport_Directional_Light_Highlight_Color;

        CFG.Current.Viewport.Viewport_Light_Intensity_Scale = CFG.Default.Viewport.Viewport_Light_Intensity_Scale;

        CFG.Current.Viewport.Viewport_Auto_Invade_Marker_Base_Color = CFG.Default.Viewport.Viewport_Auto_Invade_Marker_Base_Color;
        CFG.Current.Viewport.Viewport_Auto_Invade_Marker_Highlight_Color = CFG.Default.Viewport.Viewport_Auto_Invade_Marker_Highlight_Color;
        CFG.Current.Viewport.Viewport_MultiPlayer_Wall_Base_Color = CFG.Default.Viewport.Viewport_MultiPlayer_Wall_Base_Color;
        CFG.Current.Viewport.Viewport_MultiPlayer_Wall_Highlight_Color = CFG.Default.Viewport.Viewport_MultiPlayer_Wall_Highlight_Color;
        CFG.Current.Viewport.Viewport_Map_Limit_Wall_Base_Color = CFG.Default.Viewport.Viewport_Map_Limit_Wall_Base_Color;
        CFG.Current.Viewport.Viewport_Fog_Volume_Base_Color = CFG.Default.Viewport.Viewport_Fog_Volume_Base_Color;
        CFG.Current.Viewport.Viewport_Fog_Volume_Highlight_Color = CFG.Default.Viewport.Viewport_Fog_Volume_Highlight_Color;
        CFG.Current.Viewport.Viewport_Map_Limit_Wall_Highlight_Color = CFG.Default.Viewport.Viewport_Map_Limit_Wall_Highlight_Color;

        CFG.Current.Viewport.Viewport_Level_Connector_Marker_Base_Color = CFG.Default.Viewport.Viewport_Level_Connector_Marker_Base_Color;
        CFG.Current.Viewport.Viewport_Level_Connector_Marker_Highlight_Color = CFG.Default.Viewport.Viewport_Level_Connector_Marker_Highlight_Color;

        CFG.Current.Viewport.Viewport_Gizmo_X_Base_Color = CFG.Default.Viewport.Viewport_Gizmo_X_Base_Color;
        CFG.Current.Viewport.Viewport_Gizmo_X_Highlight_Color = CFG.Default.Viewport.Viewport_Gizmo_X_Highlight_Color;

        CFG.Current.Viewport.Viewport_Gizmo_Y_Base_Color = CFG.Default.Viewport.Viewport_Gizmo_Y_Base_Color;
        CFG.Current.Viewport.Viewport_Gizmo_Y_Highlight_Color = CFG.Default.Viewport.Viewport_Gizmo_Y_Highlight_Color;

        CFG.Current.Viewport.Viewport_Gizmo_Z_Base_Color = CFG.Default.Viewport.Viewport_Gizmo_Z_Base_Color;
        CFG.Current.Viewport.Viewport_Gizmo_Z_Highlight_Color = CFG.Default.Viewport.Viewport_Gizmo_Z_Highlight_Color;

        CFG.Current.Viewport.Viewport_Wireframe_Color_Variance = CFG.Default.Viewport.Viewport_Wireframe_Color_Variance;
    }

    public static void ResetViewportDisplayPresetCFG()
    {
        CFG.Current.Viewport.Viewport_Filter_Preset_1.Name = CFG.Default.Viewport.Viewport_Filter_Preset_1.Name;
        CFG.Current.Viewport.Viewport_Filter_Preset_1.Filters = CFG.Default.Viewport.Viewport_Filter_Preset_1.Filters;
        CFG.Current.Viewport.Viewport_Filter_Preset_2.Name = CFG.Default.Viewport.Viewport_Filter_Preset_2.Name;
        CFG.Current.Viewport.Viewport_Filter_Preset_2.Filters = CFG.Default.Viewport.Viewport_Filter_Preset_2.Filters;
        CFG.Current.Viewport.Viewport_Filter_Preset_3.Name = CFG.Default.Viewport.Viewport_Filter_Preset_3.Name;
        CFG.Current.Viewport.Viewport_Filter_Preset_3.Filters = CFG.Default.Viewport.Viewport_Filter_Preset_3.Filters;
        CFG.Current.Viewport.Viewport_Filter_Preset_4.Name = CFG.Default.Viewport.Viewport_Filter_Preset_4.Name;
        CFG.Current.Viewport.Viewport_Filter_Preset_4.Filters = CFG.Default.Viewport.Viewport_Filter_Preset_4.Filters;
        CFG.Current.Viewport.Viewport_Filter_Preset_5.Name = CFG.Default.Viewport.Viewport_Filter_Preset_5.Name;
        CFG.Current.Viewport.Viewport_Filter_Preset_5.Filters = CFG.Default.Viewport.Viewport_Filter_Preset_5.Filters;
        CFG.Current.Viewport.Viewport_Filter_Preset_6.Name = CFG.Default.Viewport.Viewport_Filter_Preset_6.Name;
        CFG.Current.Viewport.Viewport_Filter_Preset_6.Filters = CFG.Default.Viewport.Viewport_Filter_Preset_6.Filters;
    }

}
