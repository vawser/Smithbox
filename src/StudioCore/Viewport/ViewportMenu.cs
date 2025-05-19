using Hexa.NET.ImGui;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Interface;
using StudioCore.Scene;
using StudioCore.Scene.DebugPrimitives;
using StudioCore.Scene.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.ViewportNS;

public class ViewportMenu
{
    public Viewport Parent;
    public Smithbox BaseEditor;

    public ViewportMenu(Smithbox baseEditor, Viewport parent)
    {
        this.BaseEditor = baseEditor;
        Parent = parent;
    }

    public void Draw()
    {
        ImGui.BeginMenuBar();

        OverlayMenu();
        CameraMenu();
        RenderMenu();
        GizmoMenu();

        // Map Editor
        if (Parent.ViewportType is ViewportType.MapEditor)
        {
            Parent.MapEditor.FilterMenu();
        }

        // Model Editor
        if (Parent.ViewportType is ViewportType.ModelEditor)
        {
            Parent.ModelEditor.FilterMenu();
        }

        SettingsMenu();

        ImGui.EndMenuBar();
    }

    public void SceneParamsGui()
    {
        ImGui.SliderFloat4("Light Direction", ref Parent.ViewPipeline.SceneParams.LightDirection, -1, 1);
        ImGui.SliderFloat("Direct Light Mult", ref Parent.ViewPipeline.SceneParams.DirectLightMult, 0, 3);
        ImGui.SliderFloat("Indirect Light Mult", ref Parent.ViewPipeline.SceneParams.IndirectLightMult, 0, 3);
        ImGui.SliderFloat("Brightness", ref Parent.ViewPipeline.SceneParams.SceneBrightness, 0, 5);
    }

    public void OverlayMenu()
    {
        if (ImGui.BeginMenu("Overlay"))
        {
            if (ImGui.MenuItem("Controls"))
            {
                CFG.Current.Viewport_DisplayControls = !CFG.Current.Viewport_DisplayControls;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Viewport_DisplayControls);

            if (ImGui.MenuItem("Profiling"))
            {
                CFG.Current.Viewport_Profiling = !CFG.Current.Viewport_Profiling;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Viewport_Profiling);

            if (Parent.ViewportType is ViewportType.MapEditor)
            {
                if (ImGui.MenuItem("Movement Increment"))
                {
                    CFG.Current.Viewport_DisplayMovementIncrement = !CFG.Current.Viewport_DisplayMovementIncrement;
                }
                UIHelper.ShowActiveStatus(CFG.Current.Viewport_DisplayMovementIncrement);

                if (ImGui.MenuItem("Rotation Increment"))
                {
                    CFG.Current.Viewport_DisplayRotationIncrement = !CFG.Current.Viewport_DisplayRotationIncrement;
                }
                UIHelper.ShowActiveStatus(CFG.Current.Viewport_DisplayRotationIncrement);

                if (ImGui.MenuItem("Quick View Tooltip"))
                {
                    CFG.Current.QuickView_DisplayTooltip = !CFG.Current.QuickView_DisplayTooltip;
                }
                UIHelper.ShowActiveStatus(CFG.Current.QuickView_DisplayTooltip);

                if (ImGui.MenuItem("Placement Orb"))
                {
                    CFG.Current.DisplayPlacementOrb = !CFG.Current.DisplayPlacementOrb;
                }
                UIHelper.ShowActiveStatus(CFG.Current.DisplayPlacementOrb);
            }

            ImGui.EndMenu();
        }
    }

    public void CameraMenu()
    {
        if (ImGui.BeginMenu("Camera"))
        {
            //if (ImGui.BeginMenu("View Mode"))
            //{
            //    if (ImGui.MenuItem("Perspective", Parent.ViewMode == ViewMode.Perspective))
            //    {
            //        Parent.ViewMode = ViewMode.Perspective;
            //    }
            //    if (ImGui.MenuItem("Orthographic", Parent.ViewMode == ViewMode.Orthographic))
            //    {
            //        Parent.ViewMode = ViewMode.Orthographic;
            //    }

            //    ImGui.EndMenu();
            //}

            //ImGui.Separator();

            if (ImGui.BeginMenu("Camera Settings"))
            {
                // FOV
                var cam_fov = CFG.Current.Viewport_Camera_FOV;
                if (ImGui.SliderFloat("Camera FOV", ref cam_fov, 40.0f, 140.0f))
                {
                    CFG.Current.Viewport_Camera_FOV = cam_fov;
                }
                UIHelper.Tooltip("Set the field of view used by the camera.");

                // Sensitivity
                var cam_sensitivity = CFG.Current.Viewport_Camera_Sensitivity;
                if (ImGui.SliderFloat("Camera sensitivity", ref cam_sensitivity, 0.0f, 0.1f))
                {
                    CFG.Current.Viewport_Camera_Sensitivity = cam_sensitivity;
                }
                UIHelper.Tooltip("Mouse sensitivty for turning the camera.");

                // Near Clipping Distance
                var nearClip = CFG.Current.Viewport_RenderDistance_Min;
                if (ImGui.SliderFloat("Near clipping distance", ref nearClip, 0.1f, 100.0f))
                {
                    CFG.Current.Viewport_RenderDistance_Min = nearClip;
                }
                UIHelper.Tooltip("Set the minimum distance at which entities will be rendered within the viewport.");

                // Far Clipping Distance
                var farClip = CFG.Current.Viewport_RenderDistance_Max;
                if (ImGui.SliderFloat("Far clipping distance", ref farClip, 10.0f, 1000000.0f))
                {
                    CFG.Current.Viewport_RenderDistance_Max = farClip;
                }
                UIHelper.Tooltip("Set the maximum distance at which entities will be rendered within the viewport.");

                // Camera Speed (Slow)
                if (ImGui.SliderFloat("Camera speed (slow)", ref Parent.ViewportCamera.CameraMoveSpeed_Slow, 0.1f, 9999.0f))
                {
                    CFG.Current.Viewport_Camera_MoveSpeed_Slow = Parent.ViewportCamera.CameraMoveSpeed_Slow;
                }
                UIHelper.Tooltip("Set the speed at which the camera will move when the Left or Right Shift key is pressed whilst moving.");

                // Camera Speed (Normal
                if (ImGui.SliderFloat("Camera speed (normal)", ref Parent.ViewportCamera.CameraMoveSpeed_Normal, 0.1f, 9999.0f))
                {
                    CFG.Current.Viewport_Camera_MoveSpeed_Normal = Parent.ViewportCamera.CameraMoveSpeed_Normal;
                }
                UIHelper.Tooltip("Set the speed at which the camera will move whilst moving normally.");

                // Camera Speed (Fast)
                if (ImGui.SliderFloat("Camera speed (fast)", ref Parent.ViewportCamera.CameraMoveSpeed_Fast, 0.1f, 9999.0f))
                {
                    CFG.Current.Viewport_Camera_MoveSpeed_Fast = Parent.ViewportCamera.CameraMoveSpeed_Fast;
                }
                UIHelper.Tooltip("Set the speed at which the camera will move when the Left or Right Control key is pressed whilst moving.");

                ImGui.Separator();

                if (ImGui.Selectable("Reset camera settings"))
                {
                    CFG.Current.Viewport_Camera_FOV = CFG.Default.Viewport_Camera_FOV;
                    CFG.Current.Viewport_RenderDistance_Max = CFG.Default.Viewport_RenderDistance_Max;
                    CFG.Current.Viewport_Camera_MoveSpeed_Slow = CFG.Default.Viewport_Camera_MoveSpeed_Slow;
                    CFG.Current.Viewport_Camera_Sensitivity = CFG.Default.Viewport_Camera_Sensitivity;
                    CFG.Current.Viewport_Camera_MoveSpeed_Normal = CFG.Default.Viewport_Camera_MoveSpeed_Normal;
                    CFG.Current.Viewport_Camera_MoveSpeed_Fast = CFG.Default.Viewport_Camera_MoveSpeed_Fast;
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }
    public void RenderMenu()
    {
        if (ImGui.BeginMenu("Render"))
        {
            if (Parent.ViewportType is ViewportType.MapEditor)
            {
                if (ImGui.MenuItem("Viewport Grid"))
                {
                    CFG.Current.Interface_MapEditor_Viewport_Grid = !CFG.Current.Interface_MapEditor_Viewport_Grid;
                    CFG.Current.MapEditor_Viewport_RegenerateMapGrid = true;
                }
                UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Viewport_Grid);

                if (ImGui.BeginMenu("Environment Map"))
                {
                    if (ImGui.MenuItem("Default"))
                    {
                        Parent.SetEnvMap(0);
                    }

                    ImGui.EndMenu();
                }
            }

            if (Parent.ViewportType is ViewportType.ModelEditor)
            {
                if (ImGui.MenuItem("Viewport Grid"))
                {
                    CFG.Current.Interface_ModelEditor_Viewport_Grid = !CFG.Current.Interface_ModelEditor_Viewport_Grid;
                    CFG.Current.ModelEditor_Viewport_RegenerateMapGrid = true;
                }
                UIHelper.ShowActiveStatus(CFG.Current.Interface_ModelEditor_Viewport_Grid);
            }

            if (ImGui.BeginMenu("Scene Lighting"))
            {
                SceneParamsGui();

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void GizmoMenu()
    {
        if (ImGui.BeginMenu("Gizmos"))
        {
            if (ImGui.BeginMenu("Mode"))
            {
                if (ImGui.MenuItem("Translate", KeyBindings.Current.VIEWPORT_GizmoTranslationMode.HintText))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Translate;
                }
                UIHelper.Tooltip($"Set the gizmo to Translation mode.");

                if (ImGui.MenuItem("Rotate", KeyBindings.Current.VIEWPORT_GizmoRotationMode.HintText))
                {
                    Gizmos.Mode = Gizmos.GizmosMode.Rotate;
                }
                UIHelper.Tooltip($"Set the gizmo to Rotation mode.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Space"))
            {
                if (ImGui.MenuItem("Local", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.Local;
                }
                UIHelper.Tooltip($"Place the gizmo origin based on the selection's local position.");

                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoSpaceMode.HintText))
                {
                    Gizmos.Space = Gizmos.GizmosSpace.World;
                }
                UIHelper.Tooltip($"Place the gizmo origin based on the selection's world position.");

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Origin"))
            {
                if (ImGui.MenuItem("World", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.World;
                }
                UIHelper.Tooltip($"Orient the gizmo origin based on the world position.");

                if (ImGui.MenuItem("Bounding Box", KeyBindings.Current.VIEWPORT_GizmoOriginMode.HintText))
                {
                    Gizmos.Origin = Gizmos.GizmosOrigin.BoundingBox;
                }
                UIHelper.Tooltip($"Orient the gizmo origin based on the bounding box.");

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void SettingsMenu()
    {
        if (ImGui.BeginMenu("Settings"))
        {
            if (ImGui.MenuItem("Enable rendering", CFG.Current.Viewport_Enable_Rendering))
            {
                CFG.Current.Viewport_Enable_Rendering = !CFG.Current.Viewport_Enable_Rendering;
            }
            UIHelper.Tooltip($"Whether to render objects in the viewport.");

            if (ImGui.MenuItem("Enable texturing", CFG.Current.Viewport_Enable_Texturing))
            {
                CFG.Current.Viewport_Enable_Texturing = !CFG.Current.Viewport_Enable_Texturing;
            }
            UIHelper.Tooltip($"Whether to render textures in the viewport.");

            if (ImGui.MenuItem("Enable culling", CFG.Current.Viewport_Enable_Culling))
            {
                CFG.Current.Viewport_Enable_Culling = !CFG.Current.Viewport_Enable_Culling;
            }
            UIHelper.Tooltip($"Whether to cull objects in the viewport outside of the camera frustum.");

            if (Parent.ViewportType is ViewportType.MapEditor)
            {
                ImGui.Separator();

                if (ImGui.BeginMenu("Quick View"))
                {
                    Parent.MapEditor.QuickView.HandleQuickViewProperties();

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Placement Orb"))
                {
                    ImGui.DragFloat("Orb Distance", ref CFG.Current.PlacementOrb_Distance, 0.1f, 1f, 100f);
                    UIHelper.Tooltip($"Determines the distance in front of the camera the placement orb is.");

                    ImGui.EndMenu();
                }
            }

            ImGui.EndMenu();
        }
    }
}
