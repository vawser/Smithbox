using Hexa.NET.ImGui;
using StudioCore.Interface;
using StudioCore.Scene.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Configuration.Settings;

public class ViewportTab
{
    public Smithbox BaseEditor;

    public ViewportTab(Smithbox baseEditor)
    {
        BaseEditor = baseEditor;
    }

    public void Display()
    {
        var defaultButtonSize = new Vector2(ImGui.GetWindowWidth(), 24);

        //---------------------------------------
        // Rendering
        //---------------------------------------
        if (ImGui.CollapsingHeader("Rendering", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Frame Rate
            if (ImGui.SliderFloat("Frame Rate", ref CFG.Current.System_Frame_Rate, 20.0f, 240.0f))
            {
                CFG.Current.System_Frame_Rate = (float)Math.Round(CFG.Current.System_Frame_Rate);
            }
            UIHelper.ShowHoverTooltip("Adjusts the frame rate of the viewport.");

            ImGui.Separator();

            // Toggle Rendering
            ImGui.Checkbox("Enable rendering", ref CFG.Current.Viewport_Enable_Rendering);
            UIHelper.ShowHoverTooltip("Enabling this option will allow Smithbox to render entities in the viewport.");

            // Toggle Texturing
            ImGui.Checkbox("Enable texturing", ref CFG.Current.Viewport_Enable_Texturing);
            UIHelper.ShowHoverTooltip("Enabling this option will allow Smithbox to render the textures of models within the viewport.");

            // Toggle culling
            ImGui.Checkbox("Enable frustum culling", ref CFG.Current.Viewport_Enable_Culling);
            UIHelper.ShowHoverTooltip("Enabling this option will cause entities outside of the camera frustum to be culled.");

            ImGui.Separator();

            if (ImGui.InputInt("Renderables", ref CFG.Current.Viewport_Limit_Renderables, 0, 0))
                if (CFG.Current.Viewport_Limit_Renderables < CFG.Default.Viewport_Limit_Renderables)
                    CFG.Current.Viewport_Limit_Renderables = CFG.Default.Viewport_Limit_Renderables;
            UIHelper.ShowHoverTooltip("This value constrains the number of renderable entities that are allowed. Exceeding this value will throw an exception.");

            Utils.ImGui_InputUint("Indirect Draw buffer", ref CFG.Current.Viewport_Limit_Buffer_Indirect_Draw);
            UIHelper.ShowHoverTooltip("This value constrains the size of the indirect draw buffer. Exceeding this value will throw an exception.");

            Utils.ImGui_InputUint("FLVER Bone buffer", ref CFG.Current.Viewport_Limit_Buffer_Flver_Bone);
            UIHelper.ShowHoverTooltip("This value constrains the size of the FLVER bone buffer. Exceeding this value will throw an exception.");

            ImGui.Separator();

            ImGui.InputFloat("Default Model Render: Brightness", ref CFG.Current.Viewport_DefaultRender_Brightness);
            UIHelper.ShowHoverTooltip("Change the brightness modifier for the Default Model Rendering shader.");
            ImGui.InputFloat("Default Model Render: Saturation", ref CFG.Current.Viewport_DefaultRender_Saturation);
            UIHelper.ShowHoverTooltip("Change the saturation modifier for the Default Model Rendering shader.");

            ImGui.Checkbox("Enable enemy model masks", ref CFG.Current.Viewport_Enable_Model_Masks);
            UIHelper.ShowHoverTooltip("Attempt to display the correct model masks for enemies based on NpcParam.");

            ImGui.Checkbox("Draw LOD facesets", ref CFG.Current.Viewport_Enable_LOD_Facesets);
            UIHelper.ShowHoverTooltip("Render all facesets for all FLVER meshes, including LOD ones.");

            if (ImGui.Button("Reset##ResetRenderProperties", defaultButtonSize))
            {
                ResetRenderingCFG();
            }
            UIHelper.ShowHoverTooltip("Resets all of the values within this section to their default values.");
        }
        //---------------------------------------
        // Visualization
        //---------------------------------------
        if (ImGui.CollapsingHeader("Visualization", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.ColorEdit3("Selection Color", ref CFG.Current.Viewport_DefaultRender_SelectColor);

            ImGui.Checkbox("Enable selection outline", ref CFG.Current.Viewport_Enable_Selection_Outline);
            UIHelper.ShowHoverTooltip("Enabling this option will cause a selection outline to appear on selected objects.");

            ImGui.Checkbox("Enable box selection", ref CFG.Current.Viewport_Enable_BoxSelection);
            UIHelper.ShowHoverTooltip("Click and drag the mouse to select multiple objects. (Ctrl: Subtract, Shift: Add)");

            ImGui.Separator();

            ImGui.ColorEdit3("Box region - base color", ref CFG.Current.GFX_Renderable_Box_BaseColor);
            ImGui.ColorEdit3("Box region - highlight color", ref CFG.Current.GFX_Renderable_Box_HighlightColor);
            ImGui.DragFloat("Box region - transparency when solid", ref CFG.Current.GFX_Renderable_Box_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Cylinder region - base color", ref CFG.Current.GFX_Renderable_Cylinder_BaseColor);
            ImGui.ColorEdit3("Cylinder region - highlight color", ref CFG.Current.GFX_Renderable_Cylinder_HighlightColor);
            ImGui.DragFloat("Cylinder region - transparency when solid", ref CFG.Current.GFX_Renderable_Cylinder_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Sphere region - base color", ref CFG.Current.GFX_Renderable_Sphere_BaseColor);
            ImGui.ColorEdit3("Sphere region - highlight color", ref CFG.Current.GFX_Renderable_Sphere_HighlightColor);
            ImGui.DragFloat("Sphere region - transparency when solid", ref CFG.Current.GFX_Renderable_Sphere_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Point region - base color", ref CFG.Current.GFX_Renderable_Point_BaseColor);
            ImGui.ColorEdit3("Point region - highlight color", ref CFG.Current.GFX_Renderable_Point_HighlightColor);
            ImGui.DragFloat("Point region - transparency when solid", ref CFG.Current.GFX_Renderable_Point_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Dummy poly - base color", ref CFG.Current.GFX_Renderable_DummyPoly_BaseColor);
            ImGui.ColorEdit3("Dummy poly - highlight color", ref CFG.Current.GFX_Renderable_DummyPoly_HighlightColor);
            ImGui.DragFloat("Dummy poly - transparency when solid", ref CFG.Current.GFX_Renderable_DummyPoly_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Bone point - base color", ref CFG.Current.GFX_Renderable_BonePoint_BaseColor);
            ImGui.ColorEdit3("Bone point - highlight color", ref CFG.Current.GFX_Renderable_BonePoint_HighlightColor);
            ImGui.DragFloat("Bone point - transparency when solid", ref CFG.Current.GFX_Renderable_BonePoint_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Chr marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor);
            ImGui.ColorEdit3("Chr marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor);
            ImGui.DragFloat("Chr marker - transparency when solid", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Object marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor);
            ImGui.ColorEdit3("Object marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor);
            ImGui.DragFloat("Object marker - transparency when solid", ref CFG.Current.GFX_Renderable_ModelMarker_Object_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Player marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor);
            ImGui.ColorEdit3("Player marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor);
            ImGui.DragFloat("Player marker - transparency when solid", ref CFG.Current.GFX_Renderable_ModelMarker_Player_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Other marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor);
            ImGui.ColorEdit3("Other marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor);
            ImGui.DragFloat("Other marker - transparency when solid", ref CFG.Current.GFX_Renderable_ModelMarker_Other_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Point light - base color", ref CFG.Current.GFX_Renderable_PointLight_BaseColor);
            ImGui.ColorEdit3("Point light - highlight color", ref CFG.Current.GFX_Renderable_PointLight_HighlightColor);
            ImGui.DragFloat("Point light - transparency when solid", ref CFG.Current.GFX_Renderable_PointLight_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Spot light - base color", ref CFG.Current.GFX_Renderable_SpotLight_BaseColor);
            ImGui.ColorEdit3("Spot light - highlight color", ref CFG.Current.GFX_Renderable_SpotLight_HighlightColor);
            ImGui.DragFloat("Spot light - transparency when solid", ref CFG.Current.GFX_Renderable_SpotLight_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Directional light - base color", ref CFG.Current.GFX_Renderable_DirectionalLight_BaseColor);
            ImGui.ColorEdit3("Directional light - highlight color", ref CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor);
            ImGui.DragFloat("Directional light - transparency when solid", ref CFG.Current.GFX_Renderable_DirectionalLight_Alpha, 1.0f, 1.0f, 100.0f);

            ImGui.ColorEdit3("Gizmo: X Axis - base color", ref CFG.Current.GFX_Gizmo_X_BaseColor);
            ImGui.ColorEdit3("Gizmo: X Axis - highlight color", ref CFG.Current.GFX_Gizmo_X_HighlightColor);

            ImGui.ColorEdit3("Gizmo: Y Axis - base color", ref CFG.Current.GFX_Gizmo_Y_BaseColor);
            ImGui.ColorEdit3("Gizmo: Y Axis - highlight color", ref CFG.Current.GFX_Gizmo_Y_HighlightColor);

            ImGui.ColorEdit3("Gizmo: Z Axis - base color", ref CFG.Current.GFX_Gizmo_Z_BaseColor);
            ImGui.ColorEdit3("Gizmo: Z Axis - highlight color", ref CFG.Current.GFX_Gizmo_Z_HighlightColor);

            ImGui.SliderFloat("Wireframe color variance", ref CFG.Current.GFX_Wireframe_Color_Variance, 0.0f, 1.0f);

            if (ImGui.Button("Reset", defaultButtonSize))
            {
                ResetVisualisationCFG();
            }
            UIHelper.ShowHoverTooltip("Resets all of the values within this section to their default values.");

        }

        //---------------------------------------
        // Camera
        //---------------------------------------
        if (ImGui.CollapsingHeader("Camera", ImGuiTreeNodeFlags.DefaultOpen))
        {
            var cam_fov = CFG.Current.Viewport_Camera_FOV;

            if (ImGui.SliderFloat("Camera FOV", ref cam_fov, 40.0f, 140.0f))
                CFG.Current.Viewport_Camera_FOV = cam_fov;
            UIHelper.ShowHoverTooltip("Set the field of view used by the camera within DSMS.");

            var cam_sensitivity = CFG.Current.Viewport_Camera_Sensitivity;

            if (ImGui.SliderFloat("Camera sensitivity", ref cam_sensitivity, 0.0f, 0.1f))
                CFG.Current.Viewport_Camera_Sensitivity = cam_sensitivity;
            UIHelper.ShowHoverTooltip("Mouse sensitivty for turning the camera.");

            var farClip = CFG.Current.Viewport_RenderDistance_Max;

            if (ImGui.SliderFloat("Map max render distance", ref farClip, 10.0f, 500000.0f))
                CFG.Current.Viewport_RenderDistance_Max = farClip;
            UIHelper.ShowHoverTooltip("Set the maximum distance at which entities will be rendered within the DSMS viewport.");

            if (BaseEditor.ProjectManager.SelectedProject != null)
            {
                var curProject = BaseEditor.ProjectManager.SelectedProject;

                if (curProject.MapEditor != null)
                {
                    var worldView = curProject.MapEditor.MapViewportView.Viewport.WorldView;

                    if (ImGui.SliderFloat("Map camera speed (slow)",
                            ref worldView.CameraMoveSpeed_Slow, 0.1f, 9999.0f))
                        CFG.Current.Viewport_Camera_MoveSpeed_Slow = worldView.CameraMoveSpeed_Slow;
                    UIHelper.ShowHoverTooltip("Set the speed at which the camera will move when the Left or Right Shift key is pressed whilst moving.");

                    if (ImGui.SliderFloat("Map camera speed (normal)",
                            ref worldView.CameraMoveSpeed_Normal, 0.1f, 9999.0f))
                        CFG.Current.Viewport_Camera_MoveSpeed_Normal = worldView.CameraMoveSpeed_Normal;
                    UIHelper.ShowHoverTooltip("Set the speed at which the camera will move whilst moving normally.");

                    if (ImGui.SliderFloat("Map camera speed (fast)",
                            ref worldView.CameraMoveSpeed_Fast, 0.1f, 9999.0f))
                        CFG.Current.Viewport_Camera_MoveSpeed_Fast = worldView.CameraMoveSpeed_Fast;
                    UIHelper.ShowHoverTooltip("Set the speed at which the camera will move when the Left or Right Control key is pressed whilst moving.");
                }

                if (ImGui.Button("Reset##ViewportCamera", defaultButtonSize))
                {
                    ResetCameraCFG();
                }
                UIHelper.ShowHoverTooltip("Resets all of the values within this section to their default values.");
            }
        }

        //---------------------------------------
        // Information Panel
        //---------------------------------------
        if (ImGui.CollapsingHeader("Information Panel", ImGuiTreeNodeFlags.DefaultOpen))
        {
            ImGui.Checkbox("Display information panel", ref CFG.Current.Viewport_Enable_ViewportInfoPanel);
            UIHelper.ShowHoverTooltip("Display the information panel.");

            ImGui.Checkbox("Display degree increment type", ref CFG.Current.Viewport_ViewportInfoPanel_Display_DegreeIncrement);
            UIHelper.ShowHoverTooltip("Display the current degree increment type you are using in the information panel.");

            ImGui.Checkbox("Display movement increment type", ref CFG.Current.Viewport_ViewportInfoPanel_Display_MovementIncrement);
            UIHelper.ShowHoverTooltip("Display the current movement increment type you are using in the information panel.");
        }

        //---------------------------------------
        // Display Presets
        //---------------------------------------
        if (ImGui.CollapsingHeader("Display Presets"))
        {
            ImGui.Text("Configure each of the six display presets available.");

            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_01);
            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_02);
            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_03);
            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_04);
            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_05);
            SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_06);

            if (ImGui.Button("Reset##DisplayPresets", defaultButtonSize))
            {
                ResetSceneFilterPresetCFG();
            }
            UIHelper.ShowHoverTooltip("Reset the values within this section to their default values.");

        }
    }

    private void SettingsRenderFilterPresetEditor(CFG.RenderFilterPreset preset)
    {
        ImGui.PushID($"{preset.Name}##PresetEdit");
        if (ImGui.CollapsingHeader($"{preset.Name}##Header"))
        {
            ImGui.Indent();
            var nameInput = preset.Name;
            ImGui.InputText("Preset Name", ref nameInput, 32);
            if (ImGui.IsItemDeactivatedAfterEdit())
                preset.Name = nameInput;

            foreach (RenderFilter e in Enum.GetValues(typeof(RenderFilter)))
            {
                var ticked = false;
                if (preset.Filters.HasFlag(e))
                    ticked = true;

                if (ImGui.Checkbox(e.ToString(), ref ticked))
                    if (ticked)
                        preset.Filters |= e;
                    else
                        preset.Filters &= ~e;
            }

            ImGui.Unindent();
        }

        ImGui.PopID();
    }

    private void ResetRenderingCFG()
    {
        CFG.Current.System_Frame_Rate = CFG.Default.System_Frame_Rate;

        CFG.Current.Viewport_DefaultRender_Brightness = CFG.Default.Viewport_DefaultRender_Brightness;
        CFG.Current.Viewport_DefaultRender_Saturation = CFG.Default.Viewport_DefaultRender_Saturation;
        CFG.Current.Viewport_Enable_Model_Masks = CFG.Default.Viewport_Enable_Model_Masks;
        CFG.Current.Viewport_Enable_LOD_Facesets = CFG.Default.Viewport_Enable_LOD_Facesets;

        CFG.Current.Viewport_Limit_Renderables = CFG.Default.Viewport_Limit_Renderables;
        CFG.Current.Viewport_Limit_Buffer_Indirect_Draw = CFG.Default.Viewport_Limit_Buffer_Indirect_Draw;
        CFG.Current.Viewport_Limit_Buffer_Flver_Bone = CFG.Default.Viewport_Limit_Buffer_Flver_Bone;
    }

    private void ResetCameraCFG()
    {
        CFG.Current.Viewport_Camera_FOV = CFG.Default.Viewport_Camera_FOV;
        CFG.Current.Viewport_RenderDistance_Max = CFG.Default.Viewport_RenderDistance_Max;
        CFG.Current.Viewport_Camera_MoveSpeed_Slow = CFG.Default.Viewport_Camera_MoveSpeed_Slow;
        CFG.Current.Viewport_Camera_Sensitivity = CFG.Default.Viewport_Camera_Sensitivity;
        CFG.Current.Viewport_Camera_MoveSpeed_Normal = CFG.Default.Viewport_Camera_MoveSpeed_Normal;
        CFG.Current.Viewport_Camera_MoveSpeed_Fast = CFG.Default.Viewport_Camera_MoveSpeed_Fast;
    }

    private void ResetSceneFilterPresetCFG()
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

    private void ResetVisualisationCFG()
    {
        CFG.Current.Viewport_Enable_Selection_Outline = CFG.Default.Viewport_Enable_Selection_Outline;
        CFG.Current.Viewport_DefaultRender_SelectColor = CFG.Default.Viewport_DefaultRender_SelectColor;
        CFG.Current.GFX_Renderable_Default_Wireframe_Alpha = CFG.Default.GFX_Renderable_Default_Wireframe_Alpha;

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

        CFG.Current.GFX_Gizmo_X_BaseColor = CFG.Default.GFX_Gizmo_X_BaseColor;
        CFG.Current.GFX_Gizmo_X_HighlightColor = CFG.Default.GFX_Gizmo_X_HighlightColor;

        CFG.Current.GFX_Gizmo_Y_BaseColor = CFG.Default.GFX_Gizmo_Y_BaseColor;
        CFG.Current.GFX_Gizmo_Y_HighlightColor = CFG.Default.GFX_Gizmo_Y_HighlightColor;

        CFG.Current.GFX_Gizmo_Z_BaseColor = CFG.Default.GFX_Gizmo_Z_BaseColor;
        CFG.Current.GFX_Gizmo_Z_HighlightColor = CFG.Default.GFX_Gizmo_Z_HighlightColor;

        CFG.Current.GFX_Wireframe_Color_Variance = CFG.Default.GFX_Wireframe_Color_Variance;
    }
}
