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
        TempScale = CFG.Current.System_UI_Scale;
        CurrentThemeName = CFG.Current.SelectedTheme;
    }

    public static void ResetViewportGeneralCFG()
    {
        CFG.Current.System_Frame_Rate = CFG.Default.System_Frame_Rate;
    }

    public static void ResetViewportRenderingCFG()
    {
        CFG.Current.Viewport_Enable_Rendering = CFG.Default.Viewport_Enable_Rendering;
        CFG.Current.Viewport_Enable_Texturing = CFG.Default.Viewport_Enable_Texturing;
        CFG.Current.Viewport_Enable_Culling = CFG.Default.Viewport_Enable_Culling;
        CFG.Current.Viewport_Limit_Renderables = CFG.Default.Viewport_Limit_Renderables;
        CFG.Current.Viewport_Limit_Buffer_Indirect_Draw = CFG.Default.Viewport_Limit_Buffer_Indirect_Draw;
        CFG.Current.Viewport_Limit_Buffer_Flver_Bone = CFG.Default.Viewport_Limit_Buffer_Flver_Bone;
    }

    public static void ResetViewportModelRenderingCFG()
    {
        CFG.Current.Viewport_Enable_Model_Masks = CFG.Default.Viewport_Enable_Model_Masks;
        CFG.Current.Viewport_Enable_LOD_Facesets = CFG.Default.Viewport_Enable_LOD_Facesets;
        CFG.Current.Viewport_DefaultRender_Brightness = CFG.Default.Viewport_DefaultRender_Brightness;
        CFG.Current.Viewport_DefaultRender_Saturation = CFG.Default.Viewport_DefaultRender_Saturation;
    }
    public static void ResetViewportSelectionCFG()
    {
        CFG.Current.Viewport_Enable_Selection_Outline = CFG.Default.Viewport_Enable_Selection_Outline;
        CFG.Current.Viewport_Enable_BoxSelection = CFG.Default.Viewport_Enable_BoxSelection;
        CFG.Current.Viewport_BS_DistThresFactor = CFG.Default.Viewport_BS_DistThresFactor;
    }

    public static void ResetViewportColoringCFG()
    {;
        CFG.Current.GFX_Renderable_Default_Wireframe_Alpha = CFG.Default.GFX_Renderable_Default_Wireframe_Alpha;

        CFG.Current.GFX_Renderable_Collision_Color = CFG.Default.GFX_Renderable_Collision_Color;
        CFG.Current.GFX_Renderable_ConnectCollision_Color = CFG.Default.GFX_Renderable_ConnectCollision_Color;
        CFG.Current.GFX_Renderable_Navmesh_Color = CFG.Default.GFX_Renderable_Navmesh_Color;
        CFG.Current.GFX_Renderable_NavmeshGate_Color = CFG.Default.GFX_Renderable_NavmeshGate_Color;

        CFG.Current.GFX_Renderable_Box_BaseColor = CFG.Default.GFX_Renderable_Box_BaseColor;
        CFG.Current.GFX_Renderable_Box_HighlightColor = CFG.Default.GFX_Renderable_Box_HighlightColor;
        CFG.Current.GFX_Renderable_Box_Alpha = CFG.Default.GFX_Renderable_Box_Alpha;

        CFG.Current.GFX_Renderable_Cylinder_BaseColor = CFG.Default.GFX_Renderable_Cylinder_BaseColor;
        CFG.Current.GFX_Renderable_Cylinder_HighlightColor = CFG.Default.GFX_Renderable_Cylinder_HighlightColor;
        CFG.Current.GFX_Renderable_Cylinder_Alpha = CFG.Default.GFX_Renderable_Cylinder_Alpha;

        CFG.Current.GFX_Renderable_Sphere_BaseColor = CFG.Default.GFX_Renderable_Sphere_BaseColor;
        CFG.Current.GFX_Renderable_Sphere_HighlightColor = CFG.Default.GFX_Renderable_Sphere_HighlightColor;
        CFG.Current.GFX_Renderable_Sphere_Alpha = CFG.Default.GFX_Renderable_Sphere_Alpha;

        CFG.Current.GFX_Renderable_Point_BaseColor = CFG.Default.GFX_Renderable_Point_BaseColor;
        CFG.Current.GFX_Renderable_Point_HighlightColor = CFG.Default.GFX_Renderable_Point_HighlightColor;
        CFG.Current.GFX_Renderable_Point_Alpha = CFG.Default.GFX_Renderable_Point_Alpha;

        CFG.Current.GFX_Renderable_DummyPoly_BaseColor = CFG.Default.GFX_Renderable_DummyPoly_BaseColor;
        CFG.Current.GFX_Renderable_DummyPoly_HighlightColor = CFG.Default.GFX_Renderable_DummyPoly_HighlightColor;
        CFG.Current.GFX_Renderable_DummyPoly_Alpha = CFG.Default.GFX_Renderable_DummyPoly_Alpha;

        CFG.Current.GFX_Renderable_BonePoint_BaseColor = CFG.Default.GFX_Renderable_BonePoint_BaseColor;
        CFG.Current.GFX_Renderable_BonePoint_HighlightColor = CFG.Default.GFX_Renderable_BonePoint_HighlightColor;
        CFG.Current.GFX_Renderable_BonePoint_Alpha = CFG.Default.GFX_Renderable_BonePoint_Alpha;

        CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor = CFG.Default.GFX_Renderable_ModelMarker_Chr_BaseColor;
        CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor = CFG.Default.GFX_Renderable_ModelMarker_Chr_HighlightColor;
        CFG.Current.GFX_Renderable_ModelMarker_Chr_Alpha = CFG.Default.GFX_Renderable_ModelMarker_Chr_Alpha;

        CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor = CFG.Default.GFX_Renderable_ModelMarker_Object_BaseColor;
        CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor = CFG.Default.GFX_Renderable_ModelMarker_Object_HighlightColor;
        CFG.Current.GFX_Renderable_ModelMarker_Object_Alpha = CFG.Default.GFX_Renderable_ModelMarker_Object_Alpha;

        CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor = CFG.Default.GFX_Renderable_ModelMarker_Player_BaseColor;
        CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor = CFG.Default.GFX_Renderable_ModelMarker_Player_HighlightColor;
        CFG.Current.GFX_Renderable_ModelMarker_Player_Alpha = CFG.Default.GFX_Renderable_ModelMarker_Player_Alpha;

        CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor = CFG.Default.GFX_Renderable_ModelMarker_Other_BaseColor;
        CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor = CFG.Default.GFX_Renderable_ModelMarker_Other_HighlightColor;
        CFG.Current.GFX_Renderable_ModelMarker_Other_Alpha = CFG.Default.GFX_Renderable_ModelMarker_Other_Alpha;

        CFG.Current.GFX_Renderable_PointLight_BaseColor = CFG.Default.GFX_Renderable_PointLight_BaseColor;
        CFG.Current.GFX_Renderable_PointLight_HighlightColor = CFG.Default.GFX_Renderable_PointLight_HighlightColor;
        CFG.Current.GFX_Renderable_PointLight_Alpha = CFG.Default.GFX_Renderable_PointLight_Alpha;

        CFG.Current.GFX_Renderable_SpotLight_BaseColor = CFG.Default.GFX_Renderable_SpotLight_BaseColor;
        CFG.Current.GFX_Renderable_SpotLight_HighlightColor = CFG.Default.GFX_Renderable_SpotLight_HighlightColor;
        CFG.Current.GFX_Renderable_SpotLight_Alpha = CFG.Default.GFX_Renderable_SpotLight_Alpha;

        CFG.Current.GFX_Renderable_DirectionalLight_BaseColor = CFG.Default.GFX_Renderable_DirectionalLight_BaseColor;
        CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor = CFG.Default.GFX_Renderable_DirectionalLight_HighlightColor;
        CFG.Current.GFX_Renderable_DirectionalLight_Alpha = CFG.Default.GFX_Renderable_DirectionalLight_Alpha;

        CFG.Current.GFX_Renderable_AutoInvadeSphere_BaseColor = CFG.Default.GFX_Renderable_AutoInvadeSphere_BaseColor;
        CFG.Current.GFX_Renderable_AutoInvadeSphere_HighlightColor = CFG.Default.GFX_Renderable_AutoInvadeSphere_HighlightColor;

        CFG.Current.GFX_Renderable_LevelConnectorSphere_BaseColor = CFG.Default.GFX_Renderable_LevelConnectorSphere_BaseColor;
        CFG.Current.GFX_Renderable_LevelConnectorSphere_HighlightColor = CFG.Default.GFX_Renderable_LevelConnectorSphere_HighlightColor;

        CFG.Current.GFX_Gizmo_X_BaseColor = CFG.Default.GFX_Gizmo_X_BaseColor;
        CFG.Current.GFX_Gizmo_X_HighlightColor = CFG.Default.GFX_Gizmo_X_HighlightColor;

        CFG.Current.GFX_Gizmo_Y_BaseColor = CFG.Default.GFX_Gizmo_Y_BaseColor;
        CFG.Current.GFX_Gizmo_Y_HighlightColor = CFG.Default.GFX_Gizmo_Y_HighlightColor;

        CFG.Current.GFX_Gizmo_Z_BaseColor = CFG.Default.GFX_Gizmo_Z_BaseColor;
        CFG.Current.GFX_Gizmo_Z_HighlightColor = CFG.Default.GFX_Gizmo_Z_HighlightColor;

        CFG.Current.GFX_Wireframe_Color_Variance = CFG.Default.GFX_Wireframe_Color_Variance;
    }

    public static void ResetViewportDisplayPresetCFG()
    {
        CFG.Current.SceneFilter_Preset_01.Name = CFG.Default.SceneFilter_Preset_01.Name;
        CFG.Current.SceneFilter_Preset_01.Filters = CFG.Default.SceneFilter_Preset_01.Filters;
        CFG.Current.SceneFilter_Preset_02.Name = CFG.Default.SceneFilter_Preset_02.Name;
        CFG.Current.SceneFilter_Preset_02.Filters = CFG.Default.SceneFilter_Preset_02.Filters;
        CFG.Current.SceneFilter_Preset_03.Name = CFG.Default.SceneFilter_Preset_03.Name;
        CFG.Current.SceneFilter_Preset_03.Filters = CFG.Default.SceneFilter_Preset_03.Filters;
        CFG.Current.SceneFilter_Preset_04.Name = CFG.Default.SceneFilter_Preset_04.Name;
        CFG.Current.SceneFilter_Preset_04.Filters = CFG.Default.SceneFilter_Preset_04.Filters;
        CFG.Current.SceneFilter_Preset_05.Name = CFG.Default.SceneFilter_Preset_05.Name;
        CFG.Current.SceneFilter_Preset_05.Filters = CFG.Default.SceneFilter_Preset_05.Filters;
        CFG.Current.SceneFilter_Preset_06.Name = CFG.Default.SceneFilter_Preset_06.Name;
        CFG.Current.SceneFilter_Preset_06.Filters = CFG.Default.SceneFilter_Preset_06.Filters;
    }

}
