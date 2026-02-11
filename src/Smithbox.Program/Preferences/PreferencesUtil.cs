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
        TempScale = CFG.Current.Interface_UI_Scale;
        CurrentThemeName = CFG.Current.Interface_Selected_Theme;
    }

    public static void ResetViewportGeneralCFG()
    {
        CFG.Current.Viewport_Frame_Rate = CFG.Default.Viewport_Frame_Rate;
    }

    public static void ResetViewportRenderingCFG()
    {
        CFG.Current.Viewport_Enable_Rendering = CFG.Default.Viewport_Enable_Rendering;
        CFG.Current.Viewport_Enable_Texturing = CFG.Default.Viewport_Enable_Texturing;
        CFG.Current.Viewport_Enable_Culling = CFG.Default.Viewport_Enable_Culling;
        CFG.Current.Viewport_Limit_Buffer_Indirect_Draw = CFG.Default.Viewport_Limit_Buffer_Indirect_Draw;
        CFG.Current.Viewport_Limit_Buffer_Flver_Bone = CFG.Default.Viewport_Limit_Buffer_Flver_Bone;
    }

    public static void ResetViewportModelRenderingCFG()
    {
        CFG.Current.Viewport_Enable_Model_Masks = CFG.Default.Viewport_Enable_Model_Masks;
        CFG.Current.Viewport_Enable_LOD_Facesets = CFG.Default.Viewport_Enable_LOD_Facesets;
        CFG.Current.Viewport_Untextured_Model_Brightness = CFG.Default.Viewport_Untextured_Model_Brightness;
        CFG.Current.Viewport_Untextured_Model_Saturation = CFG.Default.Viewport_Untextured_Model_Saturation;
    }
    public static void ResetViewportSelectionCFG()
    {
        CFG.Current.Viewport_Enable_Selection_Tint = CFG.Default.Viewport_Enable_Selection_Tint;
        CFG.Current.Viewport_Enable_Box_Selection = CFG.Default.Viewport_Enable_Box_Selection;
    }

    public static void ResetViewportColoringCFG()
    {;
        CFG.Current.GFX_Renderable_Default_Wireframe_Alpha = CFG.Default.GFX_Renderable_Default_Wireframe_Alpha;

        CFG.Current.Viewport_Collision_Color = CFG.Default.Viewport_Collision_Color;
        CFG.Current.Viewport_Connect_Collision_Color = CFG.Default.Viewport_Connect_Collision_Color;
        CFG.Current.Viewport_Navmesh_Color = CFG.Default.Viewport_Navmesh_Color;
        CFG.Current.Viewport_Navmesh_Gate_Color = CFG.Default.Viewport_Navmesh_Gate_Color;

        CFG.Current.Viewport_Box_Region_Base_Color = CFG.Default.Viewport_Box_Region_Base_Color;
        CFG.Current.Viewport_Box_Region_Highlight_Color = CFG.Default.Viewport_Box_Region_Highlight_Color;
        CFG.Current.Viewport_Box_Region_Alpha = CFG.Default.Viewport_Box_Region_Alpha;

        CFG.Current.Viewport_Cylinder_Region_Base_Color = CFG.Default.Viewport_Cylinder_Region_Base_Color;
        CFG.Current.Viewport_Cylinder_Region_Highlight_Color = CFG.Default.Viewport_Cylinder_Region_Highlight_Color;
        CFG.Current.Viewport_Cylinder_Region_Alpha = CFG.Default.Viewport_Cylinder_Region_Alpha;

        CFG.Current.Viewport_Sphere_Region_Base_Color = CFG.Default.Viewport_Sphere_Region_Base_Color;
        CFG.Current.Viewport_Sphere_Region_Highlight_Color = CFG.Default.Viewport_Sphere_Region_Highlight_Color;
        CFG.Current.Viewport_Sphere_Region_Alpha = CFG.Default.Viewport_Sphere_Region_Alpha;

        CFG.Current.Viewport_Point_Region_Base_Color = CFG.Default.Viewport_Point_Region_Base_Color;
        CFG.Current.Viewport_Point_Region_Highlight_Color = CFG.Default.Viewport_Point_Region_Highlight_Color;
        CFG.Current.Viewport_Point_Region_Alpha = CFG.Default.Viewport_Point_Region_Alpha;

        CFG.Current.Viewport_Dummy_Polygon_Base_Color = CFG.Default.Viewport_Dummy_Polygon_Base_Color;
        CFG.Current.Viewport_Dummy_Polygon_Highlight_Color = CFG.Default.Viewport_Dummy_Polygon_Highlight_Color;
        CFG.Current.Viewport_Dummy_Polygon_Alpha = CFG.Default.Viewport_Dummy_Polygon_Alpha;

        CFG.Current.Viewport_Bone_Marker_Base_Color = CFG.Default.Viewport_Bone_Marker_Base_Color;
        CFG.Current.Viewport_Bone_Marker_Highlight_Color = CFG.Default.Viewport_Bone_Marker_Highlight_Color;
        CFG.Current.Viewport_Bone_Marker_Alpha = CFG.Default.Viewport_Bone_Marker_Alpha;

        CFG.Current.Viewport_Character_Marker_Base_Color = CFG.Default.Viewport_Character_Marker_Base_Color;
        CFG.Current.Viewport_Character_Marker_Highlight_Color = CFG.Default.Viewport_Character_Marker_Highlight_Color;
        CFG.Current.Viewport_Character_Marker_Alpha = CFG.Default.Viewport_Character_Marker_Alpha;

        CFG.Current.Viewport_Object_Marker_Base_Color = CFG.Default.Viewport_Object_Marker_Base_Color;
        CFG.Current.Viewport_Object_Marker_Highlight_Color = CFG.Default.Viewport_Object_Marker_Highlight_Color;
        CFG.Current.Viewport_Object_Marker_Alpha = CFG.Default.Viewport_Object_Marker_Alpha;

        CFG.Current.Viewport_Player_Marker_Base_Color = CFG.Default.Viewport_Player_Marker_Base_Color;
        CFG.Current.Viewport_Player_Marker_Highlight_Color = CFG.Default.Viewport_Player_Marker_Highlight_Color;
        CFG.Current.Viewport_Player_Marker_Alpha = CFG.Default.Viewport_Player_Marker_Alpha;

        CFG.Current.Viewport_Other_Marker_Base_Color = CFG.Default.Viewport_Other_Marker_Base_Color;
        CFG.Current.Viewport_Other_Marker_Highlight_Color = CFG.Default.Viewport_Other_Marker_Highlight_Color;
        CFG.Current.Viewport_Other_Marker_Alpha = CFG.Default.Viewport_Other_Marker_Alpha;

        CFG.Current.Viewport_Point_Light_Base_Color = CFG.Default.Viewport_Point_Light_Base_Color;
        CFG.Current.Viewport_Point_Light_Highlight_Color = CFG.Default.Viewport_Point_Light_Highlight_Color;
        CFG.Current.Viewport_Point_Light_Alpha = CFG.Default.Viewport_Point_Light_Alpha;

        CFG.Current.Viewport_Spot_Light_Base_Color = CFG.Default.Viewport_Spot_Light_Base_Color;
        CFG.Current.Viewport_Splot_Light_Highlight_Color = CFG.Default.Viewport_Splot_Light_Highlight_Color;
        CFG.Current.Viewport_Spot_Light_Alpha = CFG.Default.Viewport_Spot_Light_Alpha;

        CFG.Current.Viewport_Directional_Light_Base_Color = CFG.Default.Viewport_Directional_Light_Base_Color;
        CFG.Current.Viewport_Directional_Light_Highlight_Color = CFG.Default.Viewport_Directional_Light_Highlight_Color;
        CFG.Current.Viewport_Directional_Light_Alpha = CFG.Default.Viewport_Directional_Light_Alpha;

        CFG.Current.Viewport_Auto_Invade_Marker_Base_Color = CFG.Default.Viewport_Auto_Invade_Marker_Base_Color;
        CFG.Current.Viewport_Auto_Invade_Marker_Highlight_Color = CFG.Default.Viewport_Auto_Invade_Marker_Highlight_Color;

        CFG.Current.Viewport_Level_Connector_Marker_Base_Color = CFG.Default.Viewport_Level_Connector_Marker_Base_Color;
        CFG.Current.Viewport_Level_Connector_Marker_Highlight_Color = CFG.Default.Viewport_Level_Connector_Marker_Highlight_Color;

        CFG.Current.Viewport_Gizmo_X_Base_Color = CFG.Default.Viewport_Gizmo_X_Base_Color;
        CFG.Current.Viewport_Gizmo_X_Highlight_Color = CFG.Default.Viewport_Gizmo_X_Highlight_Color;

        CFG.Current.Viewport_Gizmo_Y_Base_Color = CFG.Default.Viewport_Gizmo_Y_Base_Color;
        CFG.Current.Viewport_Gizmo_Y_Highlight_Color = CFG.Default.Viewport_Gizmo_Y_Highlight_Color;

        CFG.Current.Viewport_Gizmo_Z_Base_Color = CFG.Default.Viewport_Gizmo_Z_Base_Color;
        CFG.Current.Viewport_Gizmo_Z_Highlight_Color = CFG.Default.Viewport_Gizmo_Z_Highlight_Color;

        CFG.Current.Viewport_Wireframe_Color_Variance = CFG.Default.Viewport_Wireframe_Color_Variance;
    }

    public static void ResetViewportDisplayPresetCFG()
    {
        CFG.Current.Viewport_Filter_Preset_1.Name = CFG.Default.Viewport_Filter_Preset_1.Name;
        CFG.Current.Viewport_Filter_Preset_1.Filters = CFG.Default.Viewport_Filter_Preset_1.Filters;
        CFG.Current.Viewport_Filter_Preset_2.Name = CFG.Default.Viewport_Filter_Preset_2.Name;
        CFG.Current.Viewport_Filter_Preset_2.Filters = CFG.Default.Viewport_Filter_Preset_2.Filters;
        CFG.Current.Viewport_Filter_Preset_3.Name = CFG.Default.Viewport_Filter_Preset_3.Name;
        CFG.Current.Viewport_Filter_Preset_3.Filters = CFG.Default.Viewport_Filter_Preset_3.Filters;
        CFG.Current.Viewport_Filter_Preset_4.Name = CFG.Default.Viewport_Filter_Preset_4.Name;
        CFG.Current.Viewport_Filter_Preset_4.Filters = CFG.Default.Viewport_Filter_Preset_4.Filters;
        CFG.Current.Viewport_Filter_Preset_5.Name = CFG.Default.Viewport_Filter_Preset_5.Name;
        CFG.Current.Viewport_Filter_Preset_5.Filters = CFG.Default.Viewport_Filter_Preset_5.Filters;
        CFG.Current.Viewport_Filter_Preset_6.Name = CFG.Default.Viewport_Filter_Preset_6.Name;
        CFG.Current.Viewport_Filter_Preset_6.Filters = CFG.Default.Viewport_Filter_Preset_6.Filters;
    }

}
