using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
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
        RenderMenu();
        FilterMenu();
        GizmoMenu();

        ImGui.EndMenuBar();
    }

    public void SceneParamsGui()
    {
        ImGui.SliderFloat4(
            $"{LOC.Get("VIEWPORT_Menubar_Scene_Light_Direction")}##lightDirection", 
            ref Parent.ViewPipeline.SceneParams.LightDirection, -1, 1);

        ImGui.SliderFloat(
            $"{LOC.Get("VIEWPORT_Menubar_Scene_Direct_Light_Mult")}##directLightMult",
            ref Parent.ViewPipeline.SceneParams.DirectLightMult, 0, 3);

        ImGui.SliderFloat(
            $"{LOC.Get("VIEWPORT_Menubar_Scene_Indirect_Light_Mult")}##indirectLightMult",
            ref Parent.ViewPipeline.SceneParams.IndirectLightMult, 0, 3);

        ImGui.SliderFloat(
            $"{LOC.Get("VIEWPORT_Menubar_Scene_Light_Brightness")}##brightness",
            ref Parent.ViewPipeline.SceneParams.SceneBrightness, 0, 5);
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
            UIHelper.ShowActiveStatus(CFG.Current.Viewport_DisplayControls);
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Controls_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Profiling")}##profilingToggle"))
            {
                CFG.Current.Viewport_Display_Profiling = !CFG.Current.Viewport_Display_Profiling;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.Viewport_Display_Profiling);
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Profiling_TT"));

            if (Owner is MapUniverse mapUniverse)
            {
                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Translation_Increment")}##translationIncrementToggle"))
                {
                    CFG.Current.Viewport_DisplayTranslationIncrement = !CFG.Current.Viewport_DisplayTranslationIncrement;
                    Parent.DelayPicking();
                }
                UIHelper.ShowActiveStatus(CFG.Current.Viewport_DisplayTranslationIncrement);
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Translation_Increment_TT"));

                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Rotation_Increment")}##rotationIncrementToggle"))
                {
                    CFG.Current.Viewport_DisplayRotationIncrement = !CFG.Current.Viewport_DisplayRotationIncrement;
                    Parent.DelayPicking();
                }
                UIHelper.ShowActiveStatus(CFG.Current.Viewport_DisplayRotationIncrement);
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Rotation_Increment_TT"));

                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Viewport_Tooltip")}##viewportTooltipToggle"))
                {
                    CFG.Current.QuickView_DisplayTooltip = !CFG.Current.QuickView_DisplayTooltip;
                    Parent.DelayPicking();
                }
                UIHelper.ShowActiveStatus(CFG.Current.QuickView_DisplayTooltip);
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Viewport_Tooltip_TT"));

                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Placement_Orb")}##placementOrbToggle"))
                {
                    CFG.Current.DisplayPlacementOrb = !CFG.Current.DisplayPlacementOrb;
                    Parent.DelayPicking();
                }
                UIHelper.ShowActiveStatus(CFG.Current.DisplayPlacementOrb);
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Placement_Orb_TT"));
            }

            ImGui.EndMenu();
        }
    }

    public void CameraMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Camera")}##cameraMenuHeader"))
        {
            UIHelper.SimpleHeader(
                LOC.Get("VIEWPORT_Menubar_View_Mode"),
                LOC.Get("VIEWPORT_Menubar_View_Mode_TT"));

            var previewName = LOC.Get(Parent.ViewportCamera.ViewMode.GetDisplayName());

            if (ImGui.BeginCombo("##inputValue", previewName))
            {
                foreach (var entry in Enum.GetValues(typeof(ViewMode)))
                {
                    var type = (ViewMode)entry;

                    var displayName = LOC.Get(type.GetDisplayName());

                    if (ImGui.Selectable(displayName))
                    {
                        Parent.ViewportCamera.SetProjectionType((ViewMode)entry);
                    }
                }
                ImGui.EndCombo();
            }

            UIHelper.SimpleHeader(
                LOC.Get("VIEWPORT_Menubar_View_Parameters"),
                LOC.Get("VIEWPORT_Menubar_View_Parameters_TT"));

            // Perspective
            if (Parent.ViewportCamera.ViewMode is ViewMode.Perspective)
            {
                // Near Clipping Distance
                var nearClip = CFG.Current.Viewport_Perspective_Near_Clip;
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_View_Near_Clip")}##nearClip", 
                    ref nearClip, 0.01f, 100.0f))
                {
                    if (nearClip < 0.01f)
                    {
                        nearClip = 0.01f;
                    }
                    if (nearClip > 1000000.0f)
                    {
                        nearClip = 1000000.0f;
                    }

                    CFG.Current.Viewport_Perspective_Near_Clip = nearClip;
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_View_Near_Clip_TT"));

                // Far Clipping Distance
                var farClip = CFG.Current.Viewport_Perspective_Far_Clip;
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_View_Far_Clip")}##farClip", 
                    ref farClip, 0.01f, 1000000.0f))
                {
                    if (farClip < 0.01f)
                    {
                        farClip = 0.01f;
                    }
                    if (farClip > 1000000.0f)
                    {
                        farClip = 1000000.0f;
                    }

                    CFG.Current.Viewport_Perspective_Far_Clip = farClip;
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_View_Far_Clip_TT"));

                // FOV
                var cam_fov = CFG.Current.Viewport_Camera_FOV;
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Camera_FOV")}##cameraFov", 
                    ref cam_fov, 40.0f, 140.0f))
                {
                    if (cam_fov < 0.0f)
                    {
                        cam_fov = 1.0f;
                    }
                    if (cam_fov > 360f)
                    {
                        cam_fov = 360f;
                    }

                    CFG.Current.Viewport_Camera_FOV = cam_fov;
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_FOV_TT"));

                // Sensitivity
                var cam_sensitivity = CFG.Current.Viewport_Camera_Sensitivity;
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Camera_Sensitivity")}##cameraSensitivity",
                    ref cam_sensitivity, 0.0f, 0.1f))
                {
                    if (cam_sensitivity < 0.0f)
                    {
                        cam_sensitivity = 0.0f;
                    }
                    if (cam_sensitivity > 1f)
                    {
                        cam_sensitivity = 1f;
                    }

                    CFG.Current.Viewport_Camera_Sensitivity = cam_sensitivity;
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Sensitivity_TT"));

                // Camera Speed (Slow)
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Camera_Speed_Slow")}##cameraSpeedSlow",
                    ref Parent.ViewportCamera.CameraMoveSpeed_Slow, 0.1f, 9999.0f))
                {
                    CFG.Current.Viewport_Camera_MoveSpeed_Slow = Parent.ViewportCamera.CameraMoveSpeed_Slow;
                }
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (CFG.Current.Viewport_Camera_MoveSpeed_Slow < 0.0f)
                    {
                        CFG.Current.Viewport_Camera_MoveSpeed_Slow = 0.0f;
                    }
                    if (CFG.Current.Viewport_Camera_MoveSpeed_Slow > 9999.0f)
                    {
                        CFG.Current.Viewport_Camera_MoveSpeed_Slow = 9999.0f;
                    }
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Speed_Slow_TT"));

                // Camera Speed (Normal)
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Camera_Speed_Normal")}##cameraSpeedNormal",
                    ref Parent.ViewportCamera.CameraMoveSpeed_Normal, 0.1f, 9999.0f))
                {
                    CFG.Current.Viewport_Camera_MoveSpeed_Normal = Parent.ViewportCamera.CameraMoveSpeed_Normal;
                }
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (CFG.Current.Viewport_Camera_MoveSpeed_Normal < 0.0f)
                    {
                        CFG.Current.Viewport_Camera_MoveSpeed_Normal = 0.0f;
                    }
                    if (CFG.Current.Viewport_Camera_MoveSpeed_Normal > 9999.0f)
                    {
                        CFG.Current.Viewport_Camera_MoveSpeed_Normal = 9999.0f;
                    }
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Speed_Normal_TT"));

                // Camera Speed (Fast)
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Camera_Speed_Fast")}##cameraSpeedFast",
                    ref Parent.ViewportCamera.CameraMoveSpeed_Fast, 0.1f, 9999.0f))
                {
                    CFG.Current.Viewport_Camera_MoveSpeed_Fast = Parent.ViewportCamera.CameraMoveSpeed_Fast;
                }
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (CFG.Current.Viewport_Camera_MoveSpeed_Fast < 0.0f)
                    {
                        CFG.Current.Viewport_Camera_MoveSpeed_Fast = 0.0f;
                    }
                    if (CFG.Current.Viewport_Camera_MoveSpeed_Fast > 9999.0f)
                    {
                        CFG.Current.Viewport_Camera_MoveSpeed_Fast = 9999.0f;
                    }
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Camera_Speed_Fast_TT"));
            }

            // Orthographic / Oblique
            if (Parent.ViewportCamera.ViewMode is ViewMode.Orthographic or ViewMode.Oblique)
            {
                // Near Clipping Distance
                var nearClip = CFG.Current.Viewport_Orthographic_Near_Clip;
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Ortho_Near_Clip")}##orthoNearClip",
                    ref nearClip, -1000000.0f, 1000000.0f))
                {
                    if (nearClip < -1000000.0f)
                    {
                        nearClip = -1000000.0f;
                    }
                    if (nearClip > 1000000.0f)
                    {
                        nearClip = 1000000.0f;
                    }

                    CFG.Current.Viewport_Orthographic_Near_Clip = nearClip;
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Ortho_Near_Clip_TT"));

                // Far Clipping Distance
                var farClip = CFG.Current.Viewport_Orthographic_Far_Clip;
                if (ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Ortho_Far_Clip")}##orthoFarClip",
                    ref farClip, -1000000.0f, 1000000.0f))
                {
                    if (farClip < -1000000.0f)
                    {
                        farClip = -1000000.0f;
                    }
                    if (farClip > 1000000.0f)
                    {
                        farClip = 1000000.0f;
                    }

                    CFG.Current.Viewport_Orthographic_Far_Clip = farClip;
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Ortho_Far_Clip_TT"));

                // Orthographic Size
                ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Ortho_Pan_Sensitivity")}##orthoPandSensitivity",
                    ref Parent.ViewportCamera.PanSensitivity, 1.0f, 100.0f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (Parent.ViewportCamera.PanSensitivity < 1.0f)
                        Parent.ViewportCamera.PanSensitivity = 1.0f;

                    if (Parent.ViewportCamera.PanSensitivity > 100.0f)
                        Parent.ViewportCamera.PanSensitivity = 100.0f;

                    // Update the default
                    CFG.Current.Viewport_MousePan_Sensitivity = Parent.ViewportCamera.PanSensitivity;
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Ortho_Pan_Sensitivity_TT"));
            }

            // Orthographic
            if (Parent.ViewportCamera.ViewMode is ViewMode.Orthographic)
            {
                // Orthographic Size
                ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Ortho_Size")}##orthoSize",
                    ref Parent.ViewportCamera.OrthographicSize, 0.1f, 1000.0f);
                if(ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (Parent.ViewportCamera.OrthographicSize < 0.1f)
                        Parent.ViewportCamera.OrthographicSize = 0.1f;

                    if (Parent.ViewportCamera.OrthographicSize > 1000.0f)
                        Parent.ViewportCamera.OrthographicSize = 1000.0f;

                    // Update the default
                    CFG.Current.Viewport_DefaultOrthographicSize = Parent.ViewportCamera.OrthographicSize;
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Ortho_Size_TT"));
            }

            // Oblique
            if (Parent.ViewportCamera.ViewMode is ViewMode.Oblique)
            {
                // Oblique Angle
                ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Oblique_Angle")}##obliqueAngle",
                    ref Parent.ViewportCamera.ObliqueAngle, 0.0f, 90.0f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (Parent.ViewportCamera.ObliqueAngle < 0.0f)
                        Parent.ViewportCamera.ObliqueAngle = 0.0f;

                    if (Parent.ViewportCamera.ObliqueAngle > 90.0f)
                        Parent.ViewportCamera.ObliqueAngle = 90.0f;

                    // Update the default
                    CFG.Current.Viewport_DefaultObliqueAngle = Parent.ViewportCamera.ObliqueAngle;
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Oblique_Angle_TT"));

                // Oblique Scaling
                ImGui.SliderFloat(
                    $"{LOC.Get("VIEWPORT_Menubar_Oblique_Scaling")}##obliqueScaling",
                    ref Parent.ViewportCamera.ObliqueScaling, 0.0f, 1.0f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (Parent.ViewportCamera.ObliqueScaling < 0.0f)
                        Parent.ViewportCamera.ObliqueScaling = 0.0f;

                    if (Parent.ViewportCamera.ObliqueScaling > 1.0f)
                        Parent.ViewportCamera.ObliqueScaling = 1.0f;

                    // Update the default
                    CFG.Current.Viewport_DefaultObliqueScaling = Parent.ViewportCamera.ObliqueScaling;
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Oblique_Scaling_TT"));
            }

            if (ImGui.Selectable($"{LOC.Get("VIEWPORT_Menubar_Action_Reset_Camera_Settings")}##resetAction"))
            {
                CFG.Current.Viewport_Camera_FOV = CFG.Default.Viewport_Camera_FOV;
                CFG.Current.Viewport_Camera_Sensitivity = CFG.Default.Viewport_Camera_Sensitivity;

                CFG.Current.Viewport_Perspective_Near_Clip = CFG.Default.Viewport_Perspective_Near_Clip;
                CFG.Current.Viewport_Perspective_Far_Clip = CFG.Default.Viewport_Perspective_Far_Clip;

                CFG.Current.Viewport_Camera_MoveSpeed_Slow = CFG.Default.Viewport_Camera_MoveSpeed_Slow;
                CFG.Current.Viewport_Camera_Sensitivity = CFG.Default.Viewport_Camera_Sensitivity;
                CFG.Current.Viewport_Camera_MoveSpeed_Normal = CFG.Default.Viewport_Camera_MoveSpeed_Normal;
                CFG.Current.Viewport_Camera_MoveSpeed_Fast = CFG.Default.Viewport_Camera_MoveSpeed_Fast;

                CFG.Current.Viewport_Orthographic_Near_Clip = CFG.Default.Viewport_Orthographic_Near_Clip;
                CFG.Current.Viewport_Orthographic_Far_Clip = CFG.Default.Viewport_Orthographic_Far_Clip;

                CFG.Current.Viewport_MousePan_Sensitivity = CFG.Default.Viewport_MousePan_Sensitivity;

                CFG.Current.Viewport_DefaultObliqueAngle = CFG.Default.Viewport_DefaultObliqueAngle;
                CFG.Current.Viewport_DefaultObliqueScaling = CFG.Default.Viewport_DefaultObliqueScaling;

            }

            ImGui.EndMenu();
        }
    }

    public void RenderMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Render")}##renderMenuHeader"))
        {
            if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Scene_Lighting")}##sceneLightingMenuHeader"))
            {
                SceneParamsGui();

                ImGui.EndMenu();
            }

            // Map Editor
            if (Owner is MapUniverse mapUniverse)
            {
                if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Environment_Map")}##envMapMenuHeader"))
                {
                    if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Action_Default")}##defaultAction"))
                    {
                        Parent.SetEnvMap(0);
                        Parent.DelayPicking();
                    }

                    ImGui.EndMenu();
                }
            }

            ImGui.EndMenu();
        }
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
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Rendering_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Texturing")}##textureToggle", CFG.Current.Viewport_Enable_Texturing))
            {
                CFG.Current.Viewport_Enable_Texturing = !CFG.Current.Viewport_Enable_Texturing;
                Parent.DelayPicking();
            }
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Texturing_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Culling")}##cullingToggle", CFG.Current.Viewport_Enable_Culling))
            {
                CFG.Current.Viewport_Enable_Culling = !CFG.Current.Viewport_Enable_Culling;
                Parent.DelayPicking();
            }
            UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Culling_TT"));

            if (Owner is MapUniverse mapUniverse)
            {
                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Model_Masks")}##modelMaskToggle", CFG.Current.Viewport_Enable_Model_Masks))
                {
                    CFG.Current.Viewport_Enable_Model_Masks = !CFG.Current.Viewport_Enable_Model_Masks;
                    Parent.DelayPicking();
                }
                UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Model_Masks_TT"));

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

                    UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Orb_Distance_TT"));

                    ImGui.EndMenu();
                }
            }

            if (Owner is ModelUniverse modelUniverse)
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

                    UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Dummy_Size_TT"));

                    if(ImGui.IsItemDeactivatedAfterEdit())
                    {
                        RenderableHelper.UpdateProxySizes();
                        modelUniverse.View.ViewportWindow.UpdateDisplayNodes();
                    }

                    ImGui.DragFloat(
                        $"{LOC.Get("VIEWPORT_Menubar_Setting_Node_Size")}##nodeMeshSize",
                        ref CFG.Current.NodeMeshSize, 0.1f, 0.0001f, 1f);

                    UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Node_Size_TT"));

                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        RenderableHelper.UpdateProxySizes();
                        modelUniverse.View.ViewportWindow.UpdateDisplayNodes();
                    }

                    ImGui.EndMenu();
                }
            }

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
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_ML_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
                {
                    name = LOC.Get("VIEWPORT_Menubar_ML_Assets");
                }
            }

            if (ImGui.MenuItem($"{name}##objectToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Objects = !CFG.Current.MapEditor_ModelLoad_Objects;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_MapPieces);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Characters")}##characterToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Characters = !CFG.Current.MapEditor_ModelLoad_Characters;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Collisions")}##collisionToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Collisions = !CFG.Current.MapEditor_ModelLoad_Collisions;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Collisions);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Navmeshes")}##navmeshToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Navmeshes = !CFG.Current.MapEditor_ModelLoad_Navmeshes;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Navmeshes);


            ImGui.EndMenu();
        }
        UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Model_Load_Header_TT"));
    }

    public void MapTextureLoadMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Texture_Load_Header_TT")}##textureLoadMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Map_Pieces")}##mapPieceToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_MapPieces = !CFG.Current.MapEditor_TextureLoad_MapPieces;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_TL_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
                {
                    name = LOC.Get("VIEWPORT_Menubar_TL_Assets");
                }
            }

            if (ImGui.MenuItem($"{name}##objectToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_Objects = !CFG.Current.MapEditor_TextureLoad_Objects;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Objects);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Characters")}##characterToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_Characters = !CFG.Current.MapEditor_TextureLoad_Characters;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Misc")}##miscToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_Misc = !CFG.Current.MapEditor_TextureLoad_Misc;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Misc);

            ImGui.EndMenu();
        }
        UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Texture_Load_Header_TT"));
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
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_ML_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
                {
                    name = LOC.Get("VIEWPORT_Menubar_ML_Assets");
                }
            }

            if (ImGui.MenuItem($"{name}##objectToggle"))
            {
                CFG.Current.ModelEditor_ModelLoad_Objects = !CFG.Current.ModelEditor_ModelLoad_Objects;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_MapPieces);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Characters")}##characterToggle"))
            {
                CFG.Current.ModelEditor_ModelLoad_Characters = !CFG.Current.ModelEditor_ModelLoad_Characters;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Parts")}##partsToggle"))
            {
                CFG.Current.ModelEditor_ModelLoad_Parts = !CFG.Current.ModelEditor_ModelLoad_Parts;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Parts);

            ImGui.EndMenu();
        }
        UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Model_Load_Header_TT"));
    }

    public void ModelTextureLoadMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Texture_Load_Header_TT")}##textureLoadMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Map_Pieces")}##mapPieceToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_MapPieces = !CFG.Current.ModelEditor_TextureLoad_MapPieces;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_TL_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
                {
                    name = LOC.Get("VIEWPORT_Menubar_TL_Assets");
                }
            }

            if (ImGui.MenuItem(name))
            {
                CFG.Current.ModelEditor_TextureLoad_Objects = !CFG.Current.ModelEditor_TextureLoad_Objects;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Objects);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Characters")}##characterToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_Characters = !CFG.Current.ModelEditor_TextureLoad_Characters;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Parts")}##partsToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_Parts = !CFG.Current.ModelEditor_TextureLoad_Parts;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Parts);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Misc")}##miscToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_Misc = !CFG.Current.ModelEditor_TextureLoad_Misc;
                Parent.DelayPicking();
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Misc);

            ImGui.EndMenu();
        }
        UIHelper.Tooltip(LOC.Get("VIEWPORT_Menubar_Texture_Load_Header_TT"));
    }
}
