using Hexa.NET.ImGui;
using StudioCore.Editors.MapEditor;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using StudioCore.Renderer;

namespace StudioCore.Editors.Viewport;

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
            Parent.MapEditor.CollisionMenu();

            if (Parent.MapEditor.Project.ProjectType != ProjectType.DS2S && Parent.MapEditor.Project.ProjectType != ProjectType.DS2)
            {
                if (ImGui.BeginMenu("Patrol Routes"))
                {
                    if (ImGui.MenuItem("Display"))
                    {
                        PatrolDrawManager.Generate(Parent.MapEditor);
                    }
                    UIHelper.Tooltip("Display the connections between patrol route nodes.");

                    if (ImGui.MenuItem("Clear"))
                    {
                        PatrolDrawManager.Clear();
                    }
                    UIHelper.Tooltip("Clear the display of connections between patrol route nodes.");

                    ImGui.EndMenu();
                }
            }
        }

        // Model Editor
        if (Parent.ViewportType is ViewportType.ModelEditor)
        {
            Parent.ModelEditor.ViewportFilters.Display();
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
            UIHelper.Tooltip($"Toggle the display of the Control instructions in the top-left corner.");

            if (ImGui.MenuItem("Profiling"))
            {
                CFG.Current.Viewport_Profiling = !CFG.Current.Viewport_Profiling;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Viewport_Profiling);
            UIHelper.Tooltip($"Toggle the display of the Profiling information in the top-left corner.");

            if (Parent.ViewportType is ViewportType.MapEditor)
            {
                if (ImGui.MenuItem("Movement Increment"))
                {
                    CFG.Current.Viewport_DisplayMovementIncrement = !CFG.Current.Viewport_DisplayMovementIncrement;
                }
                UIHelper.ShowActiveStatus(CFG.Current.Viewport_DisplayMovementIncrement);
                UIHelper.Tooltip($"Toggle the display of the current Movement Increment in the top-left corner.");

                if (ImGui.MenuItem("Rotation Increment"))
                {
                    CFG.Current.Viewport_DisplayRotationIncrement = !CFG.Current.Viewport_DisplayRotationIncrement;
                }
                UIHelper.ShowActiveStatus(CFG.Current.Viewport_DisplayRotationIncrement);
                UIHelper.Tooltip($"Toggle the display of the current Rotation Increment in the top-left corner.");

                if (ImGui.MenuItem("Quick View Tooltip"))
                {
                    CFG.Current.QuickView_DisplayTooltip = !CFG.Current.QuickView_DisplayTooltip;
                }
                UIHelper.ShowActiveStatus(CFG.Current.QuickView_DisplayTooltip);
                UIHelper.Tooltip($"Toggle the display of the Quick View tooltip on hover.");

                if (ImGui.MenuItem("Placement Orb"))
                {
                    CFG.Current.DisplayPlacementOrb = !CFG.Current.DisplayPlacementOrb;
                }
                UIHelper.ShowActiveStatus(CFG.Current.DisplayPlacementOrb);
                UIHelper.Tooltip($"Toggle the display of the placement orb within the viewport.");
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
                if (ImGui.BeginMenu("Environment Map"))
                {
                    if (ImGui.MenuItem("Default"))
                    {
                        Parent.SetEnvMap(0);
                    }

                    ImGui.EndMenu();
                }
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

                MapEditorUtils.UpdateAllEntityModels(BaseEditor, Smithbox.ProjectManager.SelectedProject);
            }
            UIHelper.Tooltip($"Whether to render textures in the viewport.");

            if (ImGui.MenuItem("Enable culling", CFG.Current.Viewport_Enable_Culling))
            {
                CFG.Current.Viewport_Enable_Culling = !CFG.Current.Viewport_Enable_Culling;
            }
            UIHelper.Tooltip($"Whether to cull objects in the viewport outside of the camera frustum.");

            if (Parent.ViewportType is ViewportType.MapEditor)
            {
                if (ImGui.MenuItem("Enable model masks", CFG.Current.Viewport_Enable_Model_Masks))
                {
                    CFG.Current.Viewport_Enable_Model_Masks = !CFG.Current.Viewport_Enable_Model_Masks;
                }
                UIHelper.Tooltip($"Whether to attempt to hide model masks based on entity NpcParam flags.");
            }

            if (Parent.ViewportType is ViewportType.MapEditor)
            {
                ImGui.Separator();

                MapModelLoadMenu();
                MapTextureLoadMenu();

                ImGui.Separator();

                if (ImGui.BeginMenu("Quick View"))
                {
                    Parent.MapEditor.AutomaticPreviewTool.HandleQuickViewProperties();

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu("Placement Orb"))
                {
                    ImGui.DragFloat("Orb Distance", ref CFG.Current.PlacementOrb_Distance, 0.1f, 1f, 100f);
                    UIHelper.Tooltip($"Determines the distance in front of the camera the placement orb is.");

                    ImGui.EndMenu();
                }
            }

            if (Parent.ViewportType is ViewportType.ModelEditor)
            {
                ImGui.Separator();

                ModelModelLoadMenu();
                ModelTextureLoadMenu();

                ImGui.Separator();

                if (ImGui.BeginMenu("Display Nodes"))
                {
                    ImGui.DragFloat("Dummy Size", ref CFG.Current.DummyMeshSize, 0.1f, 0.0001f, 1f);
                    UIHelper.Tooltip($"Determines the radius of the dummy polygon mesh.");
                    if(ImGui.IsItemDeactivatedAfterEdit())
                    {
                        RenderableHelper.UpdateProxySizes();
                        Parent.ModelEditor.UpdateDisplayNodes();
                    }

                    ImGui.DragFloat("Node Size", ref CFG.Current.NodeMeshSize, 0.1f, 0.0001f, 1f);
                    UIHelper.Tooltip($"Determines the radius of the node mesh.");
                    if (ImGui.IsItemDeactivatedAfterEdit())
                    {
                        RenderableHelper.UpdateProxySizes();
                        Parent.ModelEditor.UpdateDisplayNodes();
                    }

                    ImGui.EndMenu();
                }
            }

            ImGui.EndMenu();
        }
    }

    public void MapModelLoadMenu()
    {
        if (ImGui.BeginMenu("Model Load"))
        {
            if (ImGui.MenuItem("Map Pieces"))
            {
                CFG.Current.MapEditor_ModelLoad_MapPieces = !CFG.Current.MapEditor_ModelLoad_MapPieces;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_MapPieces);

            var name = "Objects";
            if (Parent.MapEditor.Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                name = "Assets";
            }

            if (ImGui.MenuItem(name))
            {
                CFG.Current.MapEditor_ModelLoad_Objects = !CFG.Current.MapEditor_ModelLoad_Objects;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_MapPieces);


            if (ImGui.MenuItem("Characters"))
            {
                CFG.Current.MapEditor_ModelLoad_Characters = !CFG.Current.MapEditor_ModelLoad_Characters;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Characters);

            if (ImGui.MenuItem("Collisions"))
            {
                CFG.Current.MapEditor_ModelLoad_Collisions = !CFG.Current.MapEditor_ModelLoad_Collisions;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Collisions);

            if (ImGui.MenuItem("Navmeshes"))
            {
                CFG.Current.MapEditor_ModelLoad_Navmeshes = !CFG.Current.MapEditor_ModelLoad_Navmeshes;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Navmeshes);


            ImGui.EndMenu();
        }
        UIHelper.Tooltip("Toggle which models are loaded during a map load.");
    }

    public void MapTextureLoadMenu()
    {
        if (ImGui.BeginMenu("Texture Load"))
        {
            if (ImGui.MenuItem("Map Pieces"))
            {
                CFG.Current.MapEditor_TextureLoad_MapPieces = !CFG.Current.MapEditor_TextureLoad_MapPieces;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_MapPieces);

            var name = "Objects";
            if (Parent.MapEditor.Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                name = "Assets";
            }

            if (ImGui.MenuItem(name))
            {
                CFG.Current.MapEditor_TextureLoad_Objects = !CFG.Current.MapEditor_TextureLoad_Objects;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Objects);

            if (ImGui.MenuItem("Characters"))
            {
                CFG.Current.MapEditor_TextureLoad_Characters = !CFG.Current.MapEditor_TextureLoad_Characters;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Characters);

            if (ImGui.MenuItem("Miscellaneous"))
            {
                CFG.Current.MapEditor_TextureLoad_Misc = !CFG.Current.MapEditor_TextureLoad_Misc;
            }
            UIHelper.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Misc);

            ImGui.EndMenu();
        }
        UIHelper.Tooltip("Toggle which textures are loaded during a map load.");
    }

    public void ModelModelLoadMenu()
    {
        if (ImGui.BeginMenu("Model Load"))
        {
            if (ImGui.MenuItem("Map Pieces"))
            {
                CFG.Current.ModelEditor_ModelLoad_MapPieces = !CFG.Current.ModelEditor_ModelLoad_MapPieces;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_MapPieces);

            var name = "Objects";
            if (Parent.ModelEditor.Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                name = "Assets";
            }

            if (ImGui.MenuItem(name))
            {
                CFG.Current.ModelEditor_ModelLoad_Objects = !CFG.Current.ModelEditor_ModelLoad_Objects;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_MapPieces);

            if (ImGui.MenuItem("Characters"))
            {
                CFG.Current.ModelEditor_ModelLoad_Characters = !CFG.Current.ModelEditor_ModelLoad_Characters;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Characters);

            if (ImGui.MenuItem("Parts"))
            {
                CFG.Current.ModelEditor_ModelLoad_Parts = !CFG.Current.ModelEditor_ModelLoad_Parts;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Parts);

            //if (ImGui.MenuItem("Collisions"))
            //{
            //    CFG.Current.ModelEditor_ModelLoad_Collisions = !CFG.Current.ModelEditor_ModelLoad_Collisions;
            //}
            //UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Collisions);

            //if (ImGui.MenuItem("Navmeshes"))
            //{
            //    CFG.Current.ModelEditor_ModelLoad_Navmeshes = !CFG.Current.ModelEditor_ModelLoad_Navmeshes;
            //}
            //UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Navmeshes);

            ImGui.EndMenu();
        }
        UIHelper.Tooltip("Toggle which models are loaded during a map load.");
    }

    public void ModelTextureLoadMenu()
    {
        if (ImGui.BeginMenu("Texture Load"))
        {
            if (ImGui.MenuItem("Map Pieces"))
            {
                CFG.Current.ModelEditor_TextureLoad_MapPieces = !CFG.Current.ModelEditor_TextureLoad_MapPieces;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_MapPieces);

            var name = "Objects";
            if (Parent.ModelEditor.Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                name = "Assets";
            }

            if (ImGui.MenuItem(name))
            {
                CFG.Current.ModelEditor_TextureLoad_Objects = !CFG.Current.ModelEditor_TextureLoad_Objects;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Objects);

            if (ImGui.MenuItem("Characters"))
            {
                CFG.Current.ModelEditor_TextureLoad_Characters = !CFG.Current.ModelEditor_TextureLoad_Characters;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Characters);

            if (ImGui.MenuItem("Parts"))
            {
                CFG.Current.ModelEditor_TextureLoad_Parts = !CFG.Current.ModelEditor_TextureLoad_Parts;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Parts);

            if (ImGui.MenuItem("Miscellaneous"))
            {
                CFG.Current.ModelEditor_TextureLoad_Misc = !CFG.Current.ModelEditor_TextureLoad_Misc;
            }
            UIHelper.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Misc);

            ImGui.EndMenu();
        }
        UIHelper.Tooltip("Toggle which textures are loaded during a map load.");
    }
}
