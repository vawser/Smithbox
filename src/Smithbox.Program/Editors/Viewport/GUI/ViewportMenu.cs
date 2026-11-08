using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;
using Veldrid;

namespace StudioCore.Editors.Viewport;

public class ViewportMenu
{
    public IUniverse Owner;
    public VulkanViewport Parent;

    public ViewportMenu(VulkanViewport parent)
    {
        Parent = parent;
        Owner = parent.Owner;
    }

    public void Draw()
    {
        ImGui.BeginMenuBar();

        SettingsMenu();
        OverlayMenu();
        CameraMenu();
        FilterMenu();
        GizmoMenu();

        ImGui.EndMenuBar();
    }

    public void OverlayMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Overlay")}##overlayMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Controls")}##controlsToggle"))
            {
                CFG.Current.Viewport_DisplayControls = !CFG.Current.Viewport_DisplayControls;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.Viewport_DisplayControls);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Controls_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Profiling")}##profilingToggle"))
            {
                CFG.Current.Viewport_Display_Profiling = !CFG.Current.Viewport_Display_Profiling;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.Viewport_Display_Profiling);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Profiling_TT"));

            if (Owner is MapUniverse mapUniverse)
            {
                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Active_Map")}##activeMapToggle"))
                {
                    CFG.Current.Viewport_DisplayActiveMap = !CFG.Current.Viewport_DisplayActiveMap;
                    Parent.DelayPicking();
                }
                GUI.ShowActiveStatus(CFG.Current.Viewport_DisplayActiveMap);
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Active_Map_TT"));

                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Translation_Increment")}##translationIncrementToggle"))
                {
                    CFG.Current.Viewport_DisplayTranslationIncrement = !CFG.Current.Viewport_DisplayTranslationIncrement;
                    Parent.DelayPicking();
                }
                GUI.ShowActiveStatus(CFG.Current.Viewport_DisplayTranslationIncrement);
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Translation_Increment_TT"));

                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Rotation_Increment")}##rotationIncrementToggle"))
                {
                    CFG.Current.Viewport_DisplayRotationIncrement = !CFG.Current.Viewport_DisplayRotationIncrement;
                    Parent.DelayPicking();
                }
                GUI.ShowActiveStatus(CFG.Current.Viewport_DisplayRotationIncrement);
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Rotation_Increment_TT"));

                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Viewport_Tooltip")}##viewportTooltipToggle"))
                {
                    CFG.Current.QuickView_DisplayTooltip = !CFG.Current.QuickView_DisplayTooltip;
                    Parent.DelayPicking();
                }
                GUI.ShowActiveStatus(CFG.Current.QuickView_DisplayTooltip);
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Viewport_Tooltip_TT"));

                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Compass")}##compassToggle"))
                {
                    CFG.Current.DisplayCompass = !CFG.Current.DisplayCompass;
                    Parent.DelayPicking();
                }
                GUI.ShowActiveStatus(CFG.Current.DisplayCompass);
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Compass_TT"));
            }

            ImGui.EndMenu();
        }
    }

    public void CameraMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Camera")}##cameraMenuHeader"))
        {
            GUI.SimpleHeader(
                LOC.Get("VIEWPORT_Menubar_View_Mode"),
                LOC.Get("VIEWPORT_Menubar_View_Mode_TT"));

            var previewName = LOC.Get(Parent.ViewportCamera.ViewMode.GetDisplayName());

            var camera = Parent.ViewportCamera;

            if (ImGui.BeginCombo("##viewModeCombo", previewName))
            {
                foreach (var entry in Enum.GetValues(typeof(ViewMode)))
                {
                    var type = (ViewMode)entry;

                    var displayName = LOC.Get(type.GetDisplayName());

                    if (ImGui.Selectable(displayName, camera.ViewMode == type))
                    {
                        camera.SetProjectionType(type);
                    }
                }
                ImGui.EndCombo();
            }

            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("VIEWPORT_Menubar_View_Parameters"),
                LOC.Get("VIEWPORT_Menubar_View_Parameters_TT"));

            // AlwaysClamp keeps typed (ctrl+click) values inside each slider's range, so no clamping is needed after

            // Perspective
            if (camera.ViewMode is ViewMode.Perspective)
            {
                // Near Clipping Distance
                var nearClip = CFG.Current.Viewport_Perspective_Near_Clip;
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_View_Near_Clip")}##nearClip",
                    ref nearClip, 0.01f, 100.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
                {
                    CFG.Current.Viewport_Perspective_Near_Clip = nearClip;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_View_Near_Clip_TT"));

                // Far Clipping Distance
                var farClip = CFG.Current.Viewport_Perspective_Far_Clip;
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_View_Far_Clip")}##farClip",
                    ref farClip, 0.01f, 1000000.0f, "%.0f", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
                {
                    CFG.Current.Viewport_Perspective_Far_Clip = farClip;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_View_Far_Clip_TT"));

                // FOV
                var cam_fov = CFG.Current.Viewport_Camera_FOV;
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Camera_FOV")}##cameraFov",
                    ref cam_fov, 40.0f, 140.0f, "%.0f", ImGuiSliderFlags.AlwaysClamp))
                {
                    CFG.Current.Viewport_Camera_FOV = cam_fov;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_FOV_TT"));

                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("VIEWPORT_Menubar_Camera_Movement"),
                    LOC.Get("VIEWPORT_Menubar_Camera_Movement_TT"));

                // Sensitivity
                var cam_sensitivity = CFG.Current.Viewport_Camera_Sensitivity;
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Camera_Sensitivity")}##cameraSensitivity",
                    ref cam_sensitivity, 0.0f, 0.1f, "%.3f", ImGuiSliderFlags.AlwaysClamp))
                {
                    CFG.Current.Viewport_Camera_Sensitivity = cam_sensitivity;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Sensitivity_TT"));

                // The speeds span four orders of magnitude, so the sliders are logarithmic to keep the low end usable
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Camera_Speed_Slow")}##cameraSpeedSlow",
                    ref camera.CameraMoveSpeed_Slow, 0.1f, 9999.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
                {
                    CFG.Current.Viewport_Camera_MoveSpeed_Slow = camera.CameraMoveSpeed_Slow;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Speed_Slow_TT"));

                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Camera_Speed_Normal")}##cameraSpeedNormal",
                    ref camera.CameraMoveSpeed_Normal, 0.1f, 9999.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
                {
                    CFG.Current.Viewport_Camera_MoveSpeed_Normal = camera.CameraMoveSpeed_Normal;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Speed_Normal_TT"));

                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Camera_Speed_Fast")}##cameraSpeedFast",
                    ref camera.CameraMoveSpeed_Fast, 0.1f, 9999.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
                {
                    CFG.Current.Viewport_Camera_MoveSpeed_Fast = camera.CameraMoveSpeed_Fast;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Speed_Fast_TT"));
            }

            // Orthographic / Oblique
            if (camera.ViewMode is ViewMode.Orthographic or ViewMode.Oblique)
            {
                // A slider across two million units can't be moved finely, so these are dragged instead
                var nearClip = CFG.Current.Viewport_Orthographic_Near_Clip;
                if (ImGui.DragFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Ortho_Near_Clip")}##orthoNearClip",
                    ref nearClip, 10.0f, -1000000.0f, 1000000.0f, "%.0f", ImGuiSliderFlags.AlwaysClamp))
                {
                    CFG.Current.Viewport_Orthographic_Near_Clip = nearClip;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Ortho_Near_Clip_TT"));

                var farClip = CFG.Current.Viewport_Orthographic_Far_Clip;
                if (ImGui.DragFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Ortho_Far_Clip")}##orthoFarClip",
                    ref farClip, 10.0f, -1000000.0f, 1000000.0f, "%.0f", ImGuiSliderFlags.AlwaysClamp))
                {
                    CFG.Current.Viewport_Orthographic_Far_Clip = farClip;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Ortho_Far_Clip_TT"));

                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Ortho_Pan_Sensitivity")}##orthoPandSensitivity",
                    ref camera.PanSensitivity, 1.0f, 100.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp))
                {
                    // Update the default
                    CFG.Current.Viewport_MousePan_Sensitivity = camera.PanSensitivity;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Ortho_Pan_Sensitivity_TT"));
            }

            // Orthographic
            if (camera.ViewMode is ViewMode.Orthographic)
            {
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Ortho_Size")}##orthoSize",
                    ref camera.OrthographicSize, 0.1f, 1000.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
                {
                    // Update the default
                    CFG.Current.Viewport_DefaultOrthographicSize = camera.OrthographicSize;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Ortho_Size_TT"));
            }

            // Oblique
            if (camera.ViewMode is ViewMode.Oblique)
            {
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Oblique_Angle")}##obliqueAngle",
                    ref camera.ObliqueAngle, 0.0f, 90.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp))
                {
                    // Update the default
                    CFG.Current.Viewport_DefaultObliqueAngle = camera.ObliqueAngle;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Oblique_Angle_TT"));

                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Oblique_Scaling")}##obliqueScaling",
                    ref camera.ObliqueScaling, 0.0f, 1.0f, "%.2f", ImGuiSliderFlags.AlwaysClamp))
                {
                    // Update the default
                    CFG.Current.Viewport_DefaultObliqueScaling = camera.ObliqueScaling;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Oblique_Scaling_TT"));
            }

            // Camera Clip: drops geometry within a radius of the camera, in every view mode
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("VIEWPORT_Menubar_Camera_Clip"),
                LOC.Get("VIEWPORT_Menubar_Camera_Clip_TT"));

            var clipEnabled = CFG.Current.Viewport_Camera_Clip_Enabled;
            if (ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_Camera_Clip_Enabled")}##cameraClipEnabled", ref clipEnabled))
            {
                CFG.Current.Viewport_Camera_Clip_Enabled = clipEnabled;
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Clip_Enabled_TT"));

            var clipRadius = CFG.Current.Viewport_Camera_Clip_Radius;
            if (ImGui.SliderFloat(
                $"{LOC.Get("VIEWPORT_Menubar_Camera_Clip_Radius")}##cameraClipRadius",
                ref clipRadius, 0.1f, 1000.0f, "%.1f", ImGuiSliderFlags.AlwaysClamp | ImGuiSliderFlags.Logarithmic))
            {
                CFG.Current.Viewport_Camera_Clip_Radius = clipRadius;
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Clip_Radius_TT"));

            ImGui.Separator();

            if (ImGui.Selectable($"{LOC.Get("VIEWPORT_Menubar_Action_Reset_Camera_Settings")}##resetAction"))
            {
                ResetCameraSettings();
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Action_Reset_Camera_Settings_TT"));

            ImGui.EndMenu();
        }
    }

    /// <summary>
    ///     Shows the owning editor's tool window with its Viewport Rendering section open
    /// </summary>
    private void OpenRenderingTool()
    {
        switch (Owner)
        {
            case MapUniverse map:
                CFG.Current.Interface_MapEditor_ToolWindow = true;
                CFG.Current.Interface_MapEditor_Tool_ViewportRendering = true;
                map.View.RenderingTool.FocusRequested = true;
                break;
            case ModelUniverse model:
                CFG.Current.Interface_ModelEditor_ToolWindow = true;
                CFG.Current.Interface_ModelEditor_Tool_ViewportRendering = true;
                model.View.RenderingTool.FocusRequested = true;
                break;
            case TimeActUniverse timeAct:
                CFG.Current.Interface_TimeActEditor_ToolWindow = true;
                CFG.Current.Interface_TimeActEditor_Tool_ViewportRendering = true;
                timeAct.View.RenderingTool.FocusRequested = true;
                break;
            case GparamUniverse gparam:
                CFG.Current.Interface_GparamEditor_ToolWindow = true;
                CFG.Current.Interface_GparamEditor_Tool_ViewportRendering = true;
                gparam.View.RenderingTool.FocusRequested = true;
                break;
        }
    }

    /// <summary>
    ///     Puts the camera settings back to their defaults. The speed, pan, size and oblique sliders edit the camera's
    ///     own fields, which are only read from the settings when the camera is made, so those are reset too.
    /// </summary>
    private void ResetCameraSettings()
    {
        CFG.Current.Viewport_Camera_FOV = CFG.Default.Viewport_Camera_FOV;
        CFG.Current.Viewport_Camera_Sensitivity = CFG.Default.Viewport_Camera_Sensitivity;

        CFG.Current.Viewport_Perspective_Near_Clip = CFG.Default.Viewport_Perspective_Near_Clip;
        CFG.Current.Viewport_Perspective_Far_Clip = CFG.Default.Viewport_Perspective_Far_Clip;

        CFG.Current.Viewport_Camera_Clip_Enabled = CFG.Default.Viewport_Camera_Clip_Enabled;
        CFG.Current.Viewport_Camera_Clip_Radius = CFG.Default.Viewport_Camera_Clip_Radius;

        CFG.Current.Viewport_Camera_MoveSpeed_Slow = CFG.Default.Viewport_Camera_MoveSpeed_Slow;
        CFG.Current.Viewport_Camera_MoveSpeed_Normal = CFG.Default.Viewport_Camera_MoveSpeed_Normal;
        CFG.Current.Viewport_Camera_MoveSpeed_Fast = CFG.Default.Viewport_Camera_MoveSpeed_Fast;

        CFG.Current.Viewport_Orthographic_Near_Clip = CFG.Default.Viewport_Orthographic_Near_Clip;
        CFG.Current.Viewport_Orthographic_Far_Clip = CFG.Default.Viewport_Orthographic_Far_Clip;

        CFG.Current.Viewport_MousePan_Sensitivity = CFG.Default.Viewport_MousePan_Sensitivity;
        CFG.Current.Viewport_DefaultOrthographicSize = CFG.Default.Viewport_DefaultOrthographicSize;

        CFG.Current.Viewport_DefaultObliqueAngle = CFG.Default.Viewport_DefaultObliqueAngle;
        CFG.Current.Viewport_DefaultObliqueScaling = CFG.Default.Viewport_DefaultObliqueScaling;

        var camera = Parent.ViewportCamera;
        camera.CameraMoveSpeed_Slow = CFG.Current.Viewport_Camera_MoveSpeed_Slow;
        camera.CameraMoveSpeed_Normal = CFG.Current.Viewport_Camera_MoveSpeed_Normal;
        camera.CameraMoveSpeed_Fast = CFG.Current.Viewport_Camera_MoveSpeed_Fast;
        camera.PanSensitivity = CFG.Current.Viewport_MousePan_Sensitivity;
        camera.OrthographicSize = CFG.Current.Viewport_DefaultOrthographicSize;
        camera.ObliqueAngle = CFG.Current.Viewport_DefaultObliqueAngle;
        camera.ObliqueScaling = CFG.Current.Viewport_DefaultObliqueScaling;
    }

    public void FilterMenu()
    {
        // Map Editor
        if (Owner is MapUniverse mapUniverse)
        {
            mapUniverse.View.Editor.FilterMenu();
        }

        // Model Editor
        if (Owner is ModelUniverse modelUniverse)
        {
            modelUniverse.View.ViewportFilters.Display();
        }

        // Time Act Editor
        if (Owner is TimeActUniverse timeActUniverse)
        {
            timeActUniverse.View.ViewportFilters.Display();
        }
    }

    public void GizmoMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Gizmos")}##gizmosMenuHeader"))
        {
            GizmoState.OnMenu(Parent);

            ImGui.EndMenu();
        }
    }


    public void SettingsMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Settings")}##settingsMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Rendering")}##renderToggle", CFG.Current.Viewport_Enable_Rendering))
            {
                CFG.Current.Viewport_Enable_Rendering = !CFG.Current.Viewport_Enable_Rendering;
                Parent.DelayPicking();
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Rendering_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Texturing")}##textureToggle", CFG.Current.Viewport_Enable_Texturing))
            {
                CFG.Current.Viewport_Enable_Texturing = !CFG.Current.Viewport_Enable_Texturing;
                Parent.DelayPicking();
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Texturing_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Culling")}##cullingToggle", CFG.Current.Viewport_Enable_Culling))
            {
                CFG.Current.Viewport_Enable_Culling = !CFG.Current.Viewport_Enable_Culling;
                Parent.DelayPicking();
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Culling_TT"));

            ImGui.Separator();

            RenderModeMenu();

            ImGui.Separator();

            // The same switches as the Rendering tool's tabs; the pipeline reads them every frame. GI and map
            // lighting are only offered where the viewport has them (AC6, ER, NR). Editor mode overrides them all,
            // so they are shown but can't be changed there.
            var pipeline = Parent.ViewPipeline;

            // Disabled items show no tooltip, so say why they're greyed out
            if (!RenderMode.IsGame)
            {
                ImGui.TextDisabled(LOC.Get("VIEWPORT_Menubar_Setting_Game_Mode_Only"));
            }

            ImGui.BeginDisabled(!RenderMode.IsGame);

            if (pipeline?.UseIrradianceVolumes == true)
            {
                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Global_Illumination")}##giToggle", CFG.Current.Viewport_GI_Enable))
                {
                    CFG.Current.Viewport_GI_Enable = !CFG.Current.Viewport_GI_Enable;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Global_Illumination_TT"));
            }

            if (pipeline?.UseMapLighting == true)
            {
                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Lighting")}##lightingToggle", CFG.Current.Viewport_Map_Lighting))
                {
                    CFG.Current.Viewport_Map_Lighting = !CFG.Current.Viewport_Map_Lighting;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Lighting_TT"));
            }

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Shadows")}##shadowsToggle", CFG.Current.Viewport_Shadows))
            {
                CFG.Current.Viewport_Shadows = !CFG.Current.Viewport_Shadows;
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Shadows_TT"));

            ImGui.EndDisabled();

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Open_Rendering_Tool")}##openRenderingTool"))
            {
                OpenRenderingTool();
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Open_Rendering_Tool_TT"));

            if (Owner is MapUniverse mapUniverse)
            {
                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Model_Masks")}##modelMaskToggle", CFG.Current.Viewport_Enable_Model_Masks))
                {
                    CFG.Current.Viewport_Enable_Model_Masks = !CFG.Current.Viewport_Enable_Model_Masks;
                    Parent.DelayPicking();
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Model_Masks_TT"));

                ImGui.Separator();

                MapModelLoadMenu();
                MapTextureLoadMenu();

                ImGui.Separator();

                if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Quick_View")}##viewportTooltipMenuHeader"))
                {
                    mapUniverse.View.AutomaticPreviewTool.HandleQuickViewProperties();

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Placement_Orb")}##placementOrbMenuHeader"))
                {
                    ImGui.DragFloat(
                        $"{LOC.Get("VIEWPORT_Menubar_Setting_Orb_Distance")}##orbDistance", 
                        ref CFG.Current.PlacementOrb_Distance, 0.1f, 1f, 100f);

                    GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Orb_Distance_TT"));

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Compass")}##compassMenuHeader"))
                {
                    ImGui.DragInt(
                        $"{LOC.Get("VIEWPORT_Menubar_Setting_Compass_Size")}##compassSize",
                        ref CFG.Current.CompassSize, 1, 16, 512);

                    GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Compass_Size_TT"));

                    ImGui.DragFloat(
                        $"{LOC.Get("VIEWPORT_Menubar_Setting_Compass_North_Offset")}##compassNorthOffset",
                        ref CFG.Current.CompassNorthOffset, 0.1f, -360.0f, 360f);

                    GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Compass_North_Offset_TT"));

                    ImGui.EndMenu();
                }
            }

            if (Owner is ModelUniverse modelUniverse)
            {
                ModelSettingsMenu(modelUniverse.View.ViewportWindow.UpdateDisplayNodes);
            }

            if (Owner is TimeActUniverse timeActUniverse)
            {
                ModelSettingsMenu(timeActUniverse.View.ViewportWindow.UpdateDisplayNodes);
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    ///     The render mode (Editor's fullbright or Game's full lighting) and what the textured image shows, each a
    ///     pair of items of which one is checked
    /// </summary>
    private void RenderModeMenu()
    {
        var mode = RenderMode.Current;

        if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Render_Mode_Editor")}##renderModeEditor", "", mode == ViewportRenderMode.Editor))
        {
            CFG.Current.Viewport_Render_Mode = (int)ViewportRenderMode.Editor;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Render_Mode_Editor_TT"));

        if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Render_Mode_Game")}##renderModeGame", "", mode == ViewportRenderMode.Game))
        {
            CFG.Current.Viewport_Render_Mode = (int)ViewportRenderMode.Game;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Render_Mode_Game_TT"));

        ImGui.Separator();

        var texturing = RenderMode.Texturing;

        if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Texturing_Diffuse")}##texturingDiffuse", "", texturing == ViewportTexturingMode.Diffuse))
        {
            CFG.Current.Viewport_Texturing_Mode = (int)ViewportTexturingMode.Diffuse;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Texturing_Diffuse_TT"));

        if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Texturing_Normal")}##texturingNormal", "", texturing == ViewportTexturingMode.Normal))
        {
            CFG.Current.Viewport_Texturing_Mode = (int)ViewportTexturingMode.Normal;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Texturing_Normal_TT"));
    }

    private void ModelSettingsMenu(Action updateDisplayNodes)
    {
        ImGui.Separator();

        ModelModelLoadMenu();
        ModelTextureLoadMenu();

        ImGui.Separator();

        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Display_Nodes")}##displayNodeMenuHeader"))
        {
            ImGui.DragFloat(
                $"{LOC.Get("VIEWPORT_Menubar_Setting_Dummy_Size")}##dummyMeshSize",
                ref CFG.Current.DummyMeshSize, 0.1f, 0.0001f, 1f);

            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Dummy_Size_TT"));

            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                RenderableHelper.UpdateProxySizes();
                updateDisplayNodes();
            }

            ImGui.DragFloat(
                $"{LOC.Get("VIEWPORT_Menubar_Setting_Node_Size")}##nodeMeshSize",
                ref CFG.Current.NodeMeshSize, 0.1f, 0.0001f, 1f);

            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Node_Size_TT"));

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                RenderableHelper.UpdateProxySizes();
                updateDisplayNodes();
            }

            ImGui.DragFloat(
                $"{LOC.Get("VIEWPORT_Menubar_Setting_Label_Scale")}##labelScale",
                ref CFG.Current.ModelEditor_LabelScale, 0.05f, 0.25f, 5f);

            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Label_Scale_TT"));

            ImGui.EndMenu();
        }
    }

    public void MapModelLoadMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Model_Load_Header")}##modelLoadMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Map_Pieces")}##mapPieceToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_MapPieces = !CFG.Current.MapEditor_ModelLoad_MapPieces;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_ML_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER 
                    or ProjectType.AC6 
                    or ProjectType.NR
                    or ProjectType.DB_NT
                    or ProjectType.DB)
                {
                    name = LOC.Get("VIEWPORT_Menubar_ML_Assets");
                }
            }

            if (ImGui.MenuItem($"{name}##objectToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Objects = !CFG.Current.MapEditor_ModelLoad_Objects;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Objects);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Characters")}##characterToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Characters = !CFG.Current.MapEditor_ModelLoad_Characters;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Collisions")}##collisionToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Collisions = !CFG.Current.MapEditor_ModelLoad_Collisions;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Collisions);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Navmeshes")}##navmeshToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Navmeshes = !CFG.Current.MapEditor_ModelLoad_Navmeshes;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Navmeshes);


            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Model_Load_Header_TT"));
    }

    public void MapTextureLoadMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Texture_Load_Header")}##textureLoadMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Map_Pieces")}##mapPieceToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_MapPieces = !CFG.Current.MapEditor_TextureLoad_MapPieces;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_TL_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER 
                    or ProjectType.AC6 
                    or ProjectType.NR
                    or ProjectType.DB_NT
                    or ProjectType.DB)
                {
                    name = LOC.Get("VIEWPORT_Menubar_TL_Assets");
                }
            }

            if (ImGui.MenuItem($"{name}##objectToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_Objects = !CFG.Current.MapEditor_TextureLoad_Objects;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Objects);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Characters")}##characterToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_Characters = !CFG.Current.MapEditor_TextureLoad_Characters;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Misc")}##miscToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_Misc = !CFG.Current.MapEditor_TextureLoad_Misc;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Misc);

            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Texture_Load_Header_TT"));
    }

    public void ModelModelLoadMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Model_Load_Header")}##modelLoadMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Map_Pieces")}##mapPieceToggle"))
            {
                CFG.Current.ModelEditor_ModelLoad_MapPieces = !CFG.Current.ModelEditor_ModelLoad_MapPieces;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_ML_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER 
                    or ProjectType.AC6 
                    or ProjectType.NR
                    or ProjectType.DB_NT
                    or ProjectType.DB)
                {
                    name = LOC.Get("VIEWPORT_Menubar_ML_Assets");
                }
            }

            if (ImGui.MenuItem($"{name}##objectToggle"))
            {
                CFG.Current.ModelEditor_ModelLoad_Objects = !CFG.Current.ModelEditor_ModelLoad_Objects;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Objects);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Characters")}##characterToggle"))
            {
                CFG.Current.ModelEditor_ModelLoad_Characters = !CFG.Current.ModelEditor_ModelLoad_Characters;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Parts")}##partsToggle"))
            {
                CFG.Current.ModelEditor_ModelLoad_Parts = !CFG.Current.ModelEditor_ModelLoad_Parts;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Parts);

            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Model_Load_Header_TT"));
    }

    public void ModelTextureLoadMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Texture_Load_Header")}##textureLoadMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Map_Pieces")}##mapPieceToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_MapPieces = !CFG.Current.ModelEditor_TextureLoad_MapPieces;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_TL_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER 
                    or ProjectType.AC6 
                    or ProjectType.NR
                    or ProjectType.DB_NT
                    or ProjectType.DB)
                {
                    name = LOC.Get("VIEWPORT_Menubar_TL_Assets");
                }
            }

            if (ImGui.MenuItem($"{name}##objectToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_Objects = !CFG.Current.ModelEditor_TextureLoad_Objects;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Objects);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Characters")}##characterToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_Characters = !CFG.Current.ModelEditor_TextureLoad_Characters;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Parts")}##partsToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_Parts = !CFG.Current.ModelEditor_TextureLoad_Parts;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Parts);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Misc")}##miscToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_Misc = !CFG.Current.ModelEditor_TextureLoad_Misc;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Misc);

            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Texture_Load_Header_TT"));
    }
}
