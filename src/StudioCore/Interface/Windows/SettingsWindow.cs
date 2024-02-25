using ImGuiNET;
using SoapstoneLib;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.UserProject;
using StudioCore.Scene;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using Veldrid;
using StudioCore.Editors;
using StudioCore.Settings;
using SoulsFormats;
using StudioCore.Editors.ParamEditor;

namespace StudioCore.Interface.Windows;

public class SettingsWindow
{
    public bool MenuOpenState;

    public ProjectSettings ProjSettings = null;

    public SettingsWindow()
    {
    }

    public void SaveSettings()
    {
        CFG.Save();
    }
    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    private void DisplaySettings_System()
    {
        if (ImGui.BeginTabItem("System"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Check for new versions of Smithbox during startup",
                    ref CFG.Current.System_Check_Program_Update);
                ImguiUtils.ShowHelpMarker("When enabled Smithbox will automatically check for new versions upon program start.");

                ImGui.Checkbox("Show UI tooltips", ref CFG.Current.System_Show_UI_Tooltips);
                ImguiUtils.ShowHelpMarker("This is a tooltip.");

                ImGui.SliderFloat("UI scale", ref CFG.Current.System_UI_Scale, 0.5f, 4.0f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    // Round to 0.05
                    CFG.Current.System_UI_Scale = (float)Math.Round(CFG.Current.System_UI_Scale * 20) / 20;
                    Smithbox.FontRebuildRequest = true;
                }
                ImguiUtils.ShowHelpMarker("Adjusts the scale of the user interface throughout all of Smithbox.");

                ImGui.SliderFloat("Frame Rate", ref CFG.Current.System_Frame_Rate, 20.0f, 240.0f);
                ImguiUtils.ShowHelpMarker("Adjusts the frame rate of the viewport.");

                // Round FPS to the nearest whole number
                CFG.Current.System_Frame_Rate = (float)Math.Round(CFG.Current.System_Frame_Rate);

                if (ImGui.Button("Reset"))
                {
                    CFG.Current.System_Frame_Rate = CFG.Default.System_Frame_Rate;
                    CFG.Current.System_UI_Scale = CFG.Default.System_UI_Scale;
                    Smithbox.FontRebuildRequest = true;
                }
            }

            if (ImGui.CollapsingHeader("Formats"))
            {
                ImGui.Checkbox("Flexible Unpack", ref CFG.Current.System_FlexibleUnpack);
                ImguiUtils.ShowHelpMarker("Enable this if you are attempting to mod files that are 'encrypted'.");

                BinaryReaderEx.IsFlexible = CFG.Current.System_FlexibleUnpack;
            }

            if (ImGui.CollapsingHeader("Soapstone Server"))
            {
                var running = SoapstoneServer.GetRunningPort() is int port
                    ? $"running on port {port}"
                    : "not running";
                ImGui.Text(
                    $"The server is {running}.\nIt is not accessible over the network, only to other programs on this computer.\nPlease restart the program for changes to take effect.");
                ImGui.Checkbox("Enable cross-editor features", ref CFG.Current.System_Enable_Soapstone_Server);
            }

            // Additional Language Fonts
            if (ImGui.CollapsingHeader("Additional Language Fonts"))
            {
                if (ImGui.Checkbox("Chinese", ref CFG.Current.System_Font_Chinese))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHelpMarker("Include Chinese font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Korean", ref CFG.Current.System_Font_Korean))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHelpMarker("Include Korean font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Thai", ref CFG.Current.System_Font_Thai))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHelpMarker("Include Thai font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Vietnamese", ref CFG.Current.System_Font_Vietnamese))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHelpMarker("Include Vietnamese font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Cyrillic", ref CFG.Current.System_Font_Cyrillic))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHelpMarker("Include Cyrillic font.\nAdditional fonts take more VRAM and increase startup time.");
            }

            if(ImGui.CollapsingHeader("Resources"))
            {
                ImGui.Checkbox("Alias Banks - Editor Mode", ref CFG.Current.AliasBank_EditorMode);
                ImguiUtils.ShowHelpMarker("If enabled, editing the name and tags for alias banks will commit the changes to the Smithbox base version instead of the mod-specific version.");

                if (FeatureFlags.EnableEditor_TimeAct)
                {
                    ImGui.Checkbox("Time Act Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_TimeAct);
                    ImguiUtils.ShowHelpMarker("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
                }

                if (FeatureFlags.EnableEditor_Cutscene)
                {
                    ImGui.Checkbox("Cutscene Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_Cutscene);
                    ImguiUtils.ShowHelpMarker("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
                }

                if (FeatureFlags.EnableEditor_Gparam)
                {
                    ImGui.Checkbox("Gparam Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_Gparam);
                    ImguiUtils.ShowHelpMarker("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
                }

                if (FeatureFlags.EnableEditor_Material)
                {
                    ImGui.Checkbox("Material Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_Material);
                    ImguiUtils.ShowHelpMarker("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
                }

                if (FeatureFlags.EnableEditor_Particle)
                {
                    ImGui.Checkbox("Particle Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_Particle);
                    ImguiUtils.ShowHelpMarker("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
                }

                if (FeatureFlags.EnableEditor_TextureViewer)
                {
                    ImGui.Checkbox("Texture Viewer - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_Textures);
                    ImguiUtils.ShowHelpMarker("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
                }
            }

            if (ImGui.CollapsingHeader("Project"))
            {
                if (ProjSettings == null || ProjSettings.ProjectName == null)
                {
                    ImGui.Text("No project loaded");
                    ImguiUtils.ShowHelpMarker("No project has been loaded yet.");
                }
                else if (TaskManager.AnyActiveTasks())
                {
                    ImGui.Text("Waiting for program tasks to finish...");
                    ImguiUtils.ShowHelpMarker("DSMS must finished all program tasks before it can load a project.");
                }
                else
                {
                    ImGui.Text($@"Project: {ProjSettings.ProjectName}");
                    ImguiUtils.ShowHelpMarker("This is the currently loaded project.");

                    if (ImGui.Button("Open project settings file"))
                    {
                        var projectPath = CFG.Current.LastProjectFile;
                        Process.Start("explorer.exe", projectPath);
                    }

                    var useLoose = ProjSettings.UseLooseParams;
                    if (ProjSettings.GameType is ProjectType.DS2S or ProjectType.DS3)
                    {
                        if (ImGui.Checkbox("Use loose params", ref useLoose))
                            ProjSettings.UseLooseParams = useLoose;
                        ImguiUtils.ShowHelpMarker("Loose params means the .PARAM files will be saved outside of the regulation.bin file.\n\nFor Dark Souls II: Scholar of the First Sin, it is recommended that you enable this if add any additional rows.");
                    }

                    var usepartial = ProjSettings.PartialParams;
                    if (FeatureFlags.EnablePartialParam || usepartial)
                    {
                        if (ProjSettings.GameType == ProjectType.ER &&
                        ImGui.Checkbox("Partial params", ref usepartial))
                            ProjSettings.PartialParams = usepartial;
                        ImguiUtils.ShowHelpMarker("Partial params.");
                    }
                }
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_Viewport()
    {
        if (ImGui.BeginTabItem("Viewport"))
        {
            // General
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Enable model texturing", ref CFG.Current.Viewport_Enable_Texturing);
                ImguiUtils.ShowHelpMarker("Enabling this option will allow DSMS to render the textures of models within the viewport.\n\nNote, this feature is in an alpha state.");

                ImGui.Checkbox("Enable frustrum culling", ref CFG.Current.Viewport_Frustum_Culling);
                ImguiUtils.ShowHelpMarker("Enabling this option will cause entities outside of the camera frustrum to be culled.");
            }

            if (ImGui.CollapsingHeader("Rendering", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputFloat("Default Model Render: Brightness", ref CFG.Current.Viewport_DefaultRender_Brightness);
                ImguiUtils.ShowHelpMarker("Change the brightness modifier for the Default Model Rendering shader.");
                ImGui.InputFloat("Default Model Render: Saturation", ref CFG.Current.Viewport_DefaultRender_Saturation);
                ImguiUtils.ShowHelpMarker("Change the saturation modifier for the Default Model Rendering shader.");


                ImGui.Checkbox("Enable selection outline", ref CFG.Current.Viewport_Enable_Selection_Outline);
                ImguiUtils.ShowHelpMarker("Enabling this option will cause a selection outline to appear on selected objects.");

                ImGui.ColorEdit3("Selection Color", ref CFG.Current.Viewport_DefaultRender_SelectColor);

                if (ImGui.Button("Reset##ResetRenderProperties"))
                {
                    CFG.Current.Viewport_DefaultRender_Brightness = 1.0f;
                    CFG.Current.Viewport_DefaultRender_Saturation = 0.5f;
                }
                ImguiUtils.ShowHelpMarker("Resets all of the values within this section to their default values.");
            }

            if (ImGui.CollapsingHeader("Camera"))
            {
                if (ImGui.Button("Reset##ViewportCamera"))
                {
                    CFG.Current.Viewport_Camera_FOV = CFG.Default.Viewport_Camera_FOV;

                    CFG.Current.Viewport_RenderDistance_Max = CFG.Default.Viewport_RenderDistance_Max;

                    EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow = CFG.Default.Viewport_Camera_MoveSpeed_Slow;
                    CFG.Current.Viewport_Camera_MoveSpeed_Slow = EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow;
                    CFG.Current.Viewport_Camera_Sensitivity = CFG.Default.Viewport_Camera_Sensitivity;

                    EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal = CFG.Default.Viewport_Camera_MoveSpeed_Normal;
                    CFG.Current.Viewport_Camera_MoveSpeed_Normal = EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal;

                    EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast = CFG.Default.Viewport_Camera_MoveSpeed_Fast;
                    CFG.Current.Viewport_Camera_MoveSpeed_Fast = EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast;
                }
                ImguiUtils.ShowHelpMarker("Resets all of the values within this section to their default values.");

                var cam_fov = CFG.Current.Viewport_Camera_FOV;

                if (ImGui.SliderFloat("Camera FOV", ref cam_fov, 40.0f, 140.0f))
                    CFG.Current.Viewport_Camera_FOV = cam_fov;
                ImguiUtils.ShowHelpMarker("Set the field of view used by the camera within DSMS.");

                var cam_sensitivity = CFG.Current.Viewport_Camera_Sensitivity;

                if (ImGui.SliderFloat("Camera sensitivity", ref cam_sensitivity, 0.0f, 0.1f))
                    CFG.Current.Viewport_Camera_Sensitivity = cam_sensitivity;
                ImguiUtils.ShowHelpMarker("Mouse sensitivty for turning the camera.");

                var farClip = CFG.Current.Viewport_RenderDistance_Max;

                if (ImGui.SliderFloat("Map max render distance", ref farClip, 10.0f, 500000.0f))
                    CFG.Current.Viewport_RenderDistance_Max = farClip;
                ImguiUtils.ShowHelpMarker("Set the maximum distance at which entities will be rendered within the DSMS viewport.");

                if (ImGui.SliderFloat("Map camera speed (slow)",
                        ref EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow, 0.1f, 999.0f))
                    CFG.Current.Viewport_Camera_MoveSpeed_Slow = EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow;
                ImguiUtils.ShowHelpMarker("Set the speed at which the camera will move when the Left or Right Shift key is pressed whilst moving.");

                if (ImGui.SliderFloat("Map camera speed (normal)",
                        ref EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal, 0.1f, 999.0f))
                    CFG.Current.Viewport_Camera_MoveSpeed_Normal = EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal;
                ImguiUtils.ShowHelpMarker("Set the speed at which the camera will move whilst moving normally.");

                if (ImGui.SliderFloat("Map camera speed (fast)",
                        ref EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast, 0.1f, 999.0f))
                    CFG.Current.Viewport_Camera_MoveSpeed_Fast = EditorContainer.MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast;
                ImguiUtils.ShowHelpMarker("Set the speed at which the camera will move when the Left or Right Control key is pressed whilst moving.");
            }

            // Limits
            if (ImGui.CollapsingHeader("Limits"))
            {
                if (ImGui.Button("Reset##MapLimits"))
                {
                    CFG.Current.Viewport_Limit_Renderables = CFG.Default.Viewport_Limit_Renderables;
                    CFG.Current.Viewport_Limit_Buffer_Indirect_Draw = CFG.Default.Viewport_Limit_Buffer_Indirect_Draw;
                    CFG.Current.Viewport_Limit_Buffer_Flver_Bone = CFG.Default.Viewport_Limit_Buffer_Flver_Bone;
                }
                ImguiUtils.ShowHelpMarker("Reset the values within this section to their default values.");

                ImGui.Text("Please restart the program for changes to take effect.");

                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    @"Try smaller increments (+25%%) at first, as high values will cause issues.");

                if (ImGui.InputInt("Renderables", ref CFG.Current.Viewport_Limit_Renderables, 0, 0))
                    if (CFG.Current.Viewport_Limit_Renderables < CFG.Default.Viewport_Limit_Renderables)
                        CFG.Current.Viewport_Limit_Renderables = CFG.Default.Viewport_Limit_Renderables;
                ImguiUtils.ShowHelpMarker("This value constrains the number of renderable entities that are allowed. Exceeding this value will throw an exception.");

                Utils.ImGui_InputUint("Indirect Draw buffer", ref CFG.Current.Viewport_Limit_Buffer_Indirect_Draw);
                ImguiUtils.ShowHelpMarker("This value constrains the size of the indirect draw buffer. Exceeding this value will throw an exception.");

                Utils.ImGui_InputUint("FLVER Bone buffer", ref CFG.Current.Viewport_Limit_Buffer_Flver_Bone);
                ImguiUtils.ShowHelpMarker("This value constrains the size of the FLVER bone buffer. Exceeding this value will throw an exception.");
            }

            // Grid
            if (ImGui.CollapsingHeader("Grid"))
            {
                if (ImGui.Button("Regenerate"))
                    CFG.Current.Viewport_RegenerateMapGrid = true;

                ImGui.Checkbox("Enable viewport grid", ref CFG.Current.Viewport_EnableGrid);
                ImguiUtils.ShowHelpMarker("Enable the viewport grid.");

                ImGui.SliderInt("Grid size", ref CFG.Current.Viewport_Grid_Size, 100, 1000);
                ImguiUtils.ShowHelpMarker("The overall maximum size of the grid.\nThe grid will only update upon restarting DSMS after changing this value.");

                ImGui.SliderInt("Grid increment", ref CFG.Current.Viewport_Grid_Square_Size, 1, 100);
                ImguiUtils.ShowHelpMarker("The increment size of the grid.");

                var height = CFG.Current.Viewport_Grid_Height;

                ImGui.InputFloat("Grid height", ref height);
                ImguiUtils.ShowHelpMarker("The height at which the horizontal grid sits.");

                if (height < -10000)
                    height = -10000;

                if (height > 10000)
                    height = 10000;

                CFG.Current.Viewport_Grid_Height = height;

                ImGui.SliderFloat("Grid height increment", ref CFG.Current.Viewport_Grid_Height_Increment, 0.1f, 100);
                ImguiUtils.ShowHelpMarker("The amount to lower or raise the viewport grid height via the shortcuts.");

                ImGui.ColorEdit3("Grid color", ref CFG.Current.Viewport_Grid_Color);

                if (ImGui.Button("Reset"))
                {
                    CFG.Current.Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
                    CFG.Current.Viewport_Grid_Size = 1000;
                    CFG.Current.Viewport_Grid_Square_Size = 10;
                    CFG.Current.Viewport_Grid_Height = 0;
                }
                ImguiUtils.ShowHelpMarker("Resets all of the values within this section to their default values.");
            }

            // Wireframes
            if (ImGui.CollapsingHeader("Wireframes"))
            {
                if (ImGui.Button("Reset"))
                {
                    // Proxies
                    CFG.Current.GFX_Renderable_Box_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_Box_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_Cylinder_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_Cylinder_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_Sphere_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_Sphere_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_Point_BaseColor = Utils.GetDecimalColor(Color.Yellow);
                    CFG.Current.GFX_Renderable_Point_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_DummyPoly_BaseColor = Utils.GetDecimalColor(Color.Yellow);
                    CFG.Current.GFX_Renderable_DummyPoly_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_BonePoint_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_BonePoint_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor = Utils.GetDecimalColor(Color.Firebrick);
                    CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor = Utils.GetDecimalColor(Color.Tomato);

                    CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor = Utils.GetDecimalColor(Color.MediumVioletRed);
                    CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor = Utils.GetDecimalColor(Color.DeepPink);

                    CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor = Utils.GetDecimalColor(Color.DarkOliveGreen);
                    CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor = Utils.GetDecimalColor(Color.OliveDrab);

                    CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor = Utils.GetDecimalColor(Color.Wheat);
                    CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor = Utils.GetDecimalColor(Color.AntiqueWhite);

                    CFG.Current.GFX_Renderable_PointLight_BaseColor = Utils.GetDecimalColor(Color.YellowGreen);
                    CFG.Current.GFX_Renderable_PointLight_HighlightColor = Utils.GetDecimalColor(Color.Yellow);

                    CFG.Current.GFX_Renderable_SpotLight_BaseColor = Utils.GetDecimalColor(Color.Goldenrod);
                    CFG.Current.GFX_Renderable_SpotLight_HighlightColor = Utils.GetDecimalColor(Color.Violet);

                    CFG.Current.GFX_Renderable_DirectionalLight_BaseColor = Utils.GetDecimalColor(Color.Cyan);
                    CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor = Utils.GetDecimalColor(Color.AliceBlue);

                    CFG.Current.GFX_Gizmo_X_BaseColor = new Vector3(0.952f, 0.211f, 0.325f);
                    CFG.Current.GFX_Gizmo_X_HighlightColor = new Vector3(1.0f, 0.4f, 0.513f);

                    CFG.Current.GFX_Gizmo_Y_BaseColor = new Vector3(0.525f, 0.784f, 0.082f);
                    CFG.Current.GFX_Gizmo_Y_HighlightColor = new Vector3(0.713f, 0.972f, 0.270f);

                    CFG.Current.GFX_Gizmo_Z_BaseColor = new Vector3(0.219f, 0.564f, 0.929f);
                    CFG.Current.GFX_Gizmo_Z_HighlightColor = new Vector3(0.407f, 0.690f, 1.0f);

                    CFG.Current.GFX_Wireframe_Color_Variance = CFG.Default.GFX_Wireframe_Color_Variance;
                }
                ImguiUtils.ShowHelpMarker("Resets all of the values within this section to their default values.");

                ImGui.SliderFloat("Wireframe color variance", ref CFG.Current.GFX_Wireframe_Color_Variance, 0.0f, 1.0f);

                // Proxies
                ImGui.ColorEdit3("Box region - base color", ref CFG.Current.GFX_Renderable_Box_BaseColor);
                ImGui.ColorEdit3("Box region - highlight color", ref CFG.Current.GFX_Renderable_Box_HighlightColor);

                ImGui.ColorEdit3("Cylinder region - base color", ref CFG.Current.GFX_Renderable_Cylinder_BaseColor);
                ImGui.ColorEdit3("Cylinder region - highlight color", ref CFG.Current.GFX_Renderable_Cylinder_HighlightColor);

                ImGui.ColorEdit3("Sphere region - base color", ref CFG.Current.GFX_Renderable_Sphere_BaseColor);
                ImGui.ColorEdit3("Sphere region - highlight color", ref CFG.Current.GFX_Renderable_Sphere_HighlightColor);

                ImGui.ColorEdit3("Point region - base color", ref CFG.Current.GFX_Renderable_Point_BaseColor);
                ImGui.ColorEdit3("Point region - highlight color", ref CFG.Current.GFX_Renderable_Point_HighlightColor);

                ImGui.ColorEdit3("Dummy poly - base color", ref CFG.Current.GFX_Renderable_DummyPoly_BaseColor);
                ImGui.ColorEdit3("Dummy poly - highlight color", ref CFG.Current.GFX_Renderable_DummyPoly_HighlightColor);

                ImGui.ColorEdit3("Bone point - base color", ref CFG.Current.GFX_Renderable_BonePoint_BaseColor);
                ImGui.ColorEdit3("Bone point - highlight color", ref CFG.Current.GFX_Renderable_BonePoint_HighlightColor);

                ImGui.ColorEdit3("Chr marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor);
                ImGui.ColorEdit3("Chr marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor);

                ImGui.ColorEdit3("Object marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor);
                ImGui.ColorEdit3("Object marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor);

                ImGui.ColorEdit3("Player marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor);
                ImGui.ColorEdit3("Player marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor);

                ImGui.ColorEdit3("Other marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor);
                ImGui.ColorEdit3("Other marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor);

                ImGui.ColorEdit3("Point light - base color", ref CFG.Current.GFX_Renderable_PointLight_BaseColor);
                ImGui.ColorEdit3("Point light - highlight color", ref CFG.Current.GFX_Renderable_PointLight_HighlightColor);

                ImGui.ColorEdit3("Spot light - base color", ref CFG.Current.GFX_Renderable_SpotLight_BaseColor);
                ImGui.ColorEdit3("Spot light - highlight color", ref CFG.Current.GFX_Renderable_SpotLight_HighlightColor);

                ImGui.ColorEdit3("Directional light - base color", ref CFG.Current.GFX_Renderable_DirectionalLight_BaseColor);
                ImGui.ColorEdit3("Directional light - highlight color", ref CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor);

                ImGui.ColorEdit3("Gizmo - X Axis - base color", ref CFG.Current.GFX_Gizmo_X_BaseColor);
                ImGui.ColorEdit3("Gizmo - X Axis - highlight color", ref CFG.Current.GFX_Gizmo_X_HighlightColor);

                ImGui.ColorEdit3("Gizmo - Y Axis - base color", ref CFG.Current.GFX_Gizmo_Y_BaseColor);
                ImGui.ColorEdit3("Gizmo - Y Axis - highlight color", ref CFG.Current.GFX_Gizmo_Y_HighlightColor);

                ImGui.ColorEdit3("Gizmo - Z Axis - base color", ref CFG.Current.GFX_Gizmo_Z_BaseColor);
                ImGui.ColorEdit3("Gizmo - Z Axis - highlight color", ref CFG.Current.GFX_Gizmo_Z_HighlightColor);
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_MapEditor()
    {
        if (ImGui.BeginTabItem("Map Editor"))
        {
            // General
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Exclude loaded maps from search filter", ref CFG.Current.MapEditor_Always_List_Loaded_Maps);
                ImguiUtils.ShowHelpMarker("This option will cause loaded maps to always be visible within the map list, ignoring the search filter.");

                if (ProjSettings != null)
                    if (ProjSettings.GameType is ProjectType.ER)
                    {
                        ImGui.Checkbox("Enable Elden Ring auto map offset", ref CFG.Current.Viewport_Enable_ER_Auto_Map_Offset);
                        ImguiUtils.ShowHelpMarker("");
                    }
            }

            // Property View
            if (ImGui.CollapsingHeader("Property View"))
            {
                ImGui.Checkbox("Display community names", ref CFG.Current.MapEditor_Enable_Commmunity_Names);
                ImguiUtils.ShowHelpMarker("The MSB property fields will be given crowd-sourced names instead of the canonical name.");

                ImGui.Checkbox("Display community descriptions", ref CFG.Current.MapEditor_Enable_Commmunity_Hints);
                ImguiUtils.ShowHelpMarker("The MSB property fields will be given crowd-sourced descriptions.");

                ImGui.Checkbox("Display property info", ref CFG.Current.MapEditor_Enable_Property_Info);
                ImguiUtils.ShowHelpMarker("The MSB property fields show the property info, such as minimum and maximum values, when right-clicked.");
            }

            // Scene View
            if (ImGui.CollapsingHeader("Scene View"))
            {
                ImGui.Checkbox("Display character names", ref CFG.Current.MapEditor_Show_Character_Names_in_Scene_Tree);
                ImguiUtils.ShowHelpMarker("Characters names will be displayed within the scene view list.");
            }

            // Map Object Display Presets
            if (ImGui.CollapsingHeader("Map Object Display Presets"))
            {
                ImGui.Text("Configure each of the six display presets available.");

                if (ImGui.Button("Reset##DisplayPresets"))
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
                ImguiUtils.ShowHelpMarker("Reset the values within this section to their default values.");

                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_01);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_02);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_03);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_04);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_05);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_06);
            }

            // Toolbar
            if (ImGui.CollapsingHeader("Global Actions"))
            {
                ImGui.Checkbox("Check Duplicate Entity ID", ref CFG.Current.Toolbar_Show_Check_Duplicate_Entity_ID);
                ImguiUtils.ShowHelpMarker("If enabled, the Check Duplicate Entity ID action will be visible in the map toolbar window.");

                ImGui.Checkbox("Patrol Routes", ref CFG.Current.Toolbar_Show_Render_Patrol_Routes);
                ImguiUtils.ShowHelpMarker("If enabled, the Patrol Routes action will be visible in the map toolbar window.");

                ImGui.Checkbox("Generate Navigation Data", ref CFG.Current.Toolbar_Show_Navigation_Data);
                ImguiUtils.ShowHelpMarker("If enabled, the Generate Navigation Data action will be visible in the map toolbar window.");

                ImGui.Checkbox("Toggle Object Visibility by Tag", ref CFG.Current.Toolbar_Show_Toggle_Object_Visibility_by_Tag);
                ImguiUtils.ShowHelpMarker("If enabled, the Toggle Object Visibility by Tag action will be visible in the map toolbar window.");
            }

            if (ImGui.CollapsingHeader("Map Actions"))
            {
                ImGui.Checkbox("Go to in Object List", ref CFG.Current.Toolbar_Show_Go_to_in_Object_List);
                ImguiUtils.ShowHelpMarker("If enabled, the Go to in Object List action will be visible in the map toolbar window.");

                ImGui.Checkbox("Move to Camera", ref CFG.Current.Toolbar_Show_Move_to_Camera);
                ImguiUtils.ShowHelpMarker("If enabled, the Move to Camera action will be visible in the map toolbar window.");

                ImGui.Checkbox("Frame in Viewport", ref CFG.Current.Toolbar_Show_Frame_in_Viewport);
                ImguiUtils.ShowHelpMarker("If enabled, the Frame in Viewport action will be visible in the map toolbar window.");

                ImGui.Checkbox("Toggle Visibility", ref CFG.Current.Toolbar_Show_Toggle_Visibility);
                ImguiUtils.ShowHelpMarker("If enabled, the Toggle Visibility action will be visible in the map toolbar window.");

                ImGui.Checkbox("Create", ref CFG.Current.Toolbar_Show_Create);
                ImguiUtils.ShowHelpMarker("If enabled, the Create action will be visible in the map toolbar window.");

                ImGui.Checkbox("Duplicate", ref CFG.Current.Toolbar_Show_Duplicate);
                ImguiUtils.ShowHelpMarker("If enabled, the Duplicate action will be visible in the map toolbar window.");

                ImGui.Checkbox("Rotate", ref CFG.Current.Toolbar_Show_Rotate);
                ImguiUtils.ShowHelpMarker("If enabled, the Rotate action will be visible in the map toolbar window.");

                ImGui.Checkbox("Toggle Presence", ref CFG.Current.Toolbar_Show_Toggle_Presence);
                ImguiUtils.ShowHelpMarker("If enabled, the Toggle Presence action will be visible in the map toolbar window.");

                ImGui.Checkbox("Scramble", ref CFG.Current.Toolbar_Show_Scramble);
                ImguiUtils.ShowHelpMarker("If enabled, the Scramble action will be visible in the map toolbar window.");

                ImGui.Checkbox("Replicate", ref CFG.Current.Toolbar_Show_Replicate);
                ImguiUtils.ShowHelpMarker("If enabled, the Replicate action will be visible in the map toolbar window.");

                ImGui.Checkbox("Move to Grid", ref CFG.Current.Toolbar_Show_Move_to_Grid);
                ImguiUtils.ShowHelpMarker("If enabled, the Move to Grid action will be visible in the map toolbar window.");
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_ParamEditor()
    {
        if (ImGui.BeginTabItem("Param Editor"))
        {
            // General
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Use compact param editor", ref CFG.Current.UI_CompactParams);
                ImguiUtils.ShowHelpMarker("Reduces the line height within the the Param Editor screen.");

                ImGui.Checkbox("Show advanced options in massedit popup", ref CFG.Current.Param_AdvancedMassedit);
                ImguiUtils.ShowHelpMarker("Show additional options for advanced users within the massedit popup.");

                ImGui.Checkbox("Show shortcut tools in context menus", ref CFG.Current.Param_ShowHotkeysInContextMenu);
                ImguiUtils.ShowHelpMarker("Show the shortcut tools in the right-click context menu.");
            }

            // Params
            if (ImGui.CollapsingHeader("Params"))
            {
                if (ImGui.Checkbox("Sort params alphabetically", ref CFG.Current.Param_AlphabeticalParams))
                    UICache.ClearCaches();
                ImguiUtils.ShowHelpMarker("Sort the Param View list alphabetically.");
            }

            // Rows
            if (ImGui.CollapsingHeader("Rows"))
            {
                ImGui.Checkbox("Disable line wrapping", ref CFG.Current.Param_DisableLineWrapping);
                ImguiUtils.ShowHelpMarker("Disable the row names from wrapping within the Row View list.");

                ImGui.Checkbox("Disable row grouping", ref CFG.Current.Param_DisableRowGrouping);
                ImguiUtils.ShowHelpMarker("Disable the grouping of connected rows in certain params, such as ItemLotParam within the Row View list.");

                ImGui.Checkbox("Enable direct row name editing", ref CFG.Current.Param_QuickNameEdit);
                ImguiUtils.ShowHelpMarker("Enable the ability to change a row name within the right-click context menu.");
            }

            // Fields
            if (ImGui.CollapsingHeader("Fields"))
            {
                ImGui.Checkbox("Show community field names first", ref CFG.Current.Param_MakeMetaNamesPrimary);
                ImguiUtils.ShowHelpMarker("Crowd-sourced names will appear before the canonical name in the Field View list.");

                ImGui.Checkbox("Show secondary field names", ref CFG.Current.Param_ShowSecondaryNames);
                ImguiUtils.ShowHelpMarker("The crowd-sourced name (or the canonical name if the above option is enabled) will appear after the initial name in the Field View list.");

                ImGui.Checkbox("Show field data offsets", ref CFG.Current.Param_ShowFieldOffsets);
                ImguiUtils.ShowHelpMarker("The field offset within the .PARAM file will be show to the left in the Field View List.");

                ImGui.Checkbox("Hide field references", ref CFG.Current.Param_HideReferenceRows);
                ImguiUtils.ShowHelpMarker("Hide the generated param references for fields that link to other params.");

                ImGui.Checkbox("Hide field enums", ref CFG.Current.Param_HideEnums);
                ImguiUtils.ShowHelpMarker("Hide the crowd-sourced namelist for index-based enum fields.");

                ImGui.Checkbox("Allow field reordering", ref CFG.Current.Param_AllowFieldReorder);
                ImguiUtils.ShowHelpMarker("Allow the field order to be changed by an alternative order as defined within the Paramdex META file.");

                ImGui.Checkbox("Field name in context menu", ref CFG.Current.Param_FieldNameInContextMenu);
                ImguiUtils.ShowHelpMarker("Repeat the field name in the context menu.");

                ImGui.Checkbox("Field description in context menu", ref CFG.Current.Param_FieldDescriptionInContextMenu);
                ImguiUtils.ShowHelpMarker("Repeat the field description in the context menu.");

                ImGui.Checkbox("Full massedit popup in context menu", ref CFG.Current.Param_MasseditPopupInContextMenu);
                ImguiUtils.ShowHelpMarker(@"If enabled, the right-click context menu for fields shows a comprehensive editing popup for the massedit feature.\nIf disabled, simply shows a shortcut to the manual massedit entry element.\n(The full menu is still available from the manual popup)");

                ImGui.Checkbox("Split context menu", ref CFG.Current.Param_SplitContextMenu);
                ImguiUtils.ShowHelpMarker("Split the field context menu into separate menus for separate right-click locations.");
            }

            // Values
            if (ImGui.CollapsingHeader("Values"))
                {
                    ImGui.Checkbox("Show inverted percentages as traditional percentages", ref CFG.Current.Param_ShowTraditionalPercentages);
                ImguiUtils.ShowHelpMarker("Displays field values that utilise the (1 - x) pattern as traditional percentages (e.g. -20 instead of 1.2).");
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_TextEditor()
    {
        if (ImGui.BeginTabItem("Text Editor"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show original FMG names", ref CFG.Current.FMG_ShowOriginalNames);
                ImguiUtils.ShowHelpMarker("Show the original FMG file names within the Text Editor file list.");

                if (ImGui.Checkbox("Separate related FMGs and entries", ref CFG.Current.FMG_NoGroupedFmgEntries))
                    EditorContainer.TextEditor.OnProjectChanged(ProjSettings);
                ImguiUtils.ShowHelpMarker("If enabled then FMG entries will not be grouped automatically.");

                if (ImGui.Checkbox("Separate patch FMGs", ref CFG.Current.FMG_NoFmgPatching))
                    EditorContainer.TextEditor.OnProjectChanged(ProjSettings);
                ImguiUtils.ShowHelpMarker("If enabled then FMG files added from DLCs will not be grouped with vanilla FMG files.");
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_GparamEditor()
    {
        if (ImGui.BeginTabItem("Gparam Editor"))
        {
            if (ImGui.CollapsingHeader("Files", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show Map Names", ref CFG.Current.Gparam_DisplayMapNames);
                ImguiUtils.ShowHelpMarker("Show map names in the GPARAM file list.");
            }

            if (ImGui.CollapsingHeader("Groups", ImGuiTreeNodeFlags.DefaultOpen))
            {

                ImGui.Checkbox("Show Add Group options", ref CFG.Current.Gparam_DisplayGroupAdd);
                ImguiUtils.ShowHelpMarker("Show the Add Group options, which allow the addition of GPARAM groups that are not already present within a GPARAM file.");
            }

            if (ImGui.CollapsingHeader("Fields", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show color edit for 4 digit properties", ref CFG.Current.Gparam_DisplayColorEditForVector4Fields);
                ImguiUtils.ShowHelpMarker("Show the color edit tool for 4 digit properties.");

                ImGui.Checkbox("Show Time of Day value", ref CFG.Current.Gparam_DisplayUnk04);
                ImguiUtils.ShowHelpMarker("Show the time of day value for a field.");
                
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_ModelEditor()
    {
        if (ImGui.BeginTabItem("Model Editor"))
        {
            // Property View
            if (ImGui.CollapsingHeader("Property View", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display community names", ref CFG.Current.ModelEditor_Enable_Commmunity_Names);
                ImguiUtils.ShowHelpMarker("The FLVER property fields will be given crowd-sourced names instead of the canonical name.");

                ImGui.Checkbox("Display community descriptions", ref CFG.Current.ModelEditor_Enable_Commmunity_Hints);
                ImguiUtils.ShowHelpMarker("The FLVER property fields will be given crowd-sourced descriptions.");

            }

            // Scene View
            if (ImGui.CollapsingHeader("Scene View", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display material names with meshes", ref CFG.Current.ModelEditor_DisplayMatNameOnMesh);
                ImguiUtils.ShowHelpMarker("Display the material name that a mesh uses by the scene tree name.");

                ImGui.Checkbox("Display dummy polygon reference ids", ref CFG.Current.ModelEditor_DisplayDmyPolyReferenceID);
                ImguiUtils.ShowHelpMarker("Display the reference ID of a dummy polygon by the scene tree name.");
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_Interface()
    {
        if (ImGui.BeginTabItem("User Interface"))
        {
            ImGui.Text("Current Theme");
            

            if(ImGui.ListBox("##themeSelect", ref CFG.Current.SelectedTheme, UI.LoadedThemeNames, UI.LoadedThemeNames.Length))
            {
                UI.SetTheme(false);
            }

            if (ImGui.Button("Reset to Default"))
            {
                UI.ResetInterface();
            }
            ImGui.SameLine();
            if (ImGui.Button("Open Theme Folder"))
            {
                Process.Start("explorer.exe", $"{AppContext.BaseDirectory}\\Assets\\Themes\\");
            }

            if (ImGui.Button("Export Theme"))
            {
                UI.ExportThemeJson();
            }
            ImGui.SameLine();
            ImGui.InputText("##themeName", ref CFG.Current.NewThemeName, 255);

            if (ImGui.CollapsingHeader("Editor Window", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Main Background##ImGui_MainBg", ref CFG.Current.ImGui_MainBg);
                ImGui.ColorEdit4("Child Background##ImGui_ChildBg", ref CFG.Current.ImGui_ChildBg);
                ImGui.ColorEdit4("Popup Background##ImGui_PopupBg", ref CFG.Current.ImGui_PopupBg);
                ImGui.ColorEdit4("Border##ImGui_Border", ref CFG.Current.ImGui_Border);
                ImGui.ColorEdit4("Title Bar Background##ImGui_TitleBarBg", ref CFG.Current.ImGui_TitleBarBg);
                ImGui.ColorEdit4("Title Bar Background (Active)##ImGui_TitleBarBg_Active", ref CFG.Current.ImGui_TitleBarBg_Active);
                ImGui.ColorEdit4("Menu Bar Background##ImGui_MenuBarBg", ref CFG.Current.ImGui_MenuBarBg);
            }

            if (ImGui.CollapsingHeader("Moveable Window", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Main Background##Imgui_Moveable_MainBg", ref CFG.Current.Imgui_Moveable_MainBg);
                ImGui.ColorEdit4("Child Background##Imgui_Moveable_ChildBg", ref CFG.Current.Imgui_Moveable_ChildBg);
                ImGui.ColorEdit4("Header##Imgui_Moveable_Header", ref CFG.Current.Imgui_Moveable_Header);
                ImGui.ColorEdit4("Title Bar Background##Imgui_Moveable_TitleBg", ref CFG.Current.Imgui_Moveable_TitleBg);
                ImGui.ColorEdit4("Title Bar Background (Active)##Imgui_Moveable_TitleBg_Active", ref CFG.Current.Imgui_Moveable_TitleBg_Active);
            }

            if (ImGui.CollapsingHeader("Scrollbar", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Scrollbar Background", ref CFG.Current.ImGui_ScrollbarBg);
                ImGui.ColorEdit4("Scrollbar Grab", ref CFG.Current.ImGui_ScrollbarGrab);
                ImGui.ColorEdit4("Scrollbar Grab (Hover)", ref CFG.Current.ImGui_ScrollbarGrab_Hover);
                ImGui.ColorEdit4("Scrollbar Grab (Active)", ref CFG.Current.ImGui_ScrollbarGrab_Active);
                ImGui.ColorEdit4("Slider Grab", ref CFG.Current.ImGui_SliderGrab);
                ImGui.ColorEdit4("Slider Grab (Active)", ref CFG.Current.ImGui_SliderGrab_Active);
            }

            if (ImGui.CollapsingHeader("Tab", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Tab", ref CFG.Current.ImGui_Tab);
                ImGui.ColorEdit4("Tab (Hover)", ref CFG.Current.ImGui_Tab_Hover);
                ImGui.ColorEdit4("Tab (Active)", ref CFG.Current.ImGui_Tab_Active);
                ImGui.ColorEdit4("Unfocused Tab", ref CFG.Current.ImGui_UnfocusedTab);
                ImGui.ColorEdit4("Unfocused Tab (Active)", ref CFG.Current.ImGui_UnfocusedTab_Active);
            }

            if (ImGui.CollapsingHeader("Button", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Button", ref CFG.Current.ImGui_Button);
                ImGui.ColorEdit4("Button (Hover)", ref CFG.Current.ImGui_Button_Hovered);
                ImGui.ColorEdit4("Button (Active)", ref CFG.Current.ImGui_ButtonActive);
            }

            if (ImGui.CollapsingHeader("Selection", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Selection", ref CFG.Current.ImGui_Selection);
                ImGui.ColorEdit4("Selection (Hover)", ref CFG.Current.ImGui_Selection_Hover);
                ImGui.ColorEdit4("Selection (Active)", ref CFG.Current.ImGui_Selection_Active);
            }

            if (ImGui.CollapsingHeader("Inputs", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Input Background", ref CFG.Current.ImGui_Input_Background);
                ImGui.ColorEdit4("Input Background (Hover)", ref CFG.Current.ImGui_Input_Background_Hover);
                ImGui.ColorEdit4("Input Background (Active)", ref CFG.Current.ImGui_Input_Background_Active);
                ImGui.ColorEdit4("Input Checkmark", ref CFG.Current.ImGui_Input_CheckMark);
                ImGui.ColorEdit4("Input Conflict Background", ref CFG.Current.ImGui_Input_Conflict_Background);
                ImGui.ColorEdit4("Input Vanilla Background", ref CFG.Current.ImGui_Input_Vanilla_Background);
                ImGui.ColorEdit4("Input Default Background", ref CFG.Current.ImGui_Input_Default_Background);
                ImGui.ColorEdit4("Input Auxillary Vanilla Background", ref CFG.Current.ImGui_Input_AuxVanilla_Background);
                ImGui.ColorEdit4("Input Difference Comparison Background", ref CFG.Current.ImGui_Input_DiffCompare_Background);
            }

            if (ImGui.CollapsingHeader("Text", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Default Text", ref CFG.Current.ImGui_Default_Text_Color);
                ImGui.ColorEdit4("Warning Text", ref CFG.Current.ImGui_Warning_Text_Color);
                ImGui.ColorEdit4("Beneficial Text", ref CFG.Current.ImGui_Benefit_Text_Color);
                ImGui.ColorEdit4("Invalid Text", ref CFG.Current.ImGui_Invalid_Text_Color);
                ImGui.ColorEdit4("Param Reference Text", ref CFG.Current.ImGui_ParamRef_Text);
                ImGui.ColorEdit4("Param Reference Missing Text", ref CFG.Current.ImGui_ParamRefMissing_Text);
                ImGui.ColorEdit4("Param Reference Inactive Text", ref CFG.Current.ImGui_ParamRefInactive_Text);
                ImGui.ColorEdit4("Enum Name Text", ref CFG.Current.ImGui_EnumName_Text);
                ImGui.ColorEdit4("Enum Value Text", ref CFG.Current.ImGui_EnumValue_Text);
                ImGui.ColorEdit4("FMG Link Text", ref CFG.Current.ImGui_FmgLink_Text);
                ImGui.ColorEdit4("FMG Reference Text", ref CFG.Current.ImGui_FmgRef_Text);
                ImGui.ColorEdit4("FMG Reference Inactive Text", ref CFG.Current.ImGui_FmgRefInactive_Text);
                ImGui.ColorEdit4("Is Reference Text", ref CFG.Current.ImGui_IsRef_Text);
                ImGui.ColorEdit4("Virtual Reference Text", ref CFG.Current.ImGui_VirtualRef_Text);
                ImGui.ColorEdit4("Reference Text", ref CFG.Current.ImGui_Ref_Text);
                ImGui.ColorEdit4("Auxiliary Conflict Text", ref CFG.Current.ImGui_AuxConflict_Text);
                ImGui.ColorEdit4("Auxiliary Added Text", ref CFG.Current.ImGui_AuxAdded_Text);
                ImGui.ColorEdit4("Primary Changed Text", ref CFG.Current.ImGui_PrimaryChanged_Text);
                ImGui.ColorEdit4("Param Row Text", ref CFG.Current.ImGui_ParamRow_Text);
                ImGui.ColorEdit4("Aliased Name Text", ref CFG.Current.ImGui_AliasName_Text);
            }

            if (ImGui.CollapsingHeader("Miscellaneous", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.ColorEdit4("Display Group: Border Highlight", ref CFG.Current.DisplayGroupEditor_Border_Highlight);
                ImGui.ColorEdit4("Display Group: Active Input Background", ref CFG.Current.DisplayGroupEditor_DisplayActive_Frame);
                ImGui.ColorEdit4("Display Group: Active Checkbox", ref CFG.Current.DisplayGroupEditor_DisplayActive_Checkbox);
                ImGui.ColorEdit4("Draw Group: Active Input Background", ref CFG.Current.DisplayGroupEditor_DrawActive_Frame);
                ImGui.ColorEdit4("Draw Group: Active Checkbox", ref CFG.Current.DisplayGroupEditor_DrawActive_Checkbox);
                ImGui.ColorEdit4("Combined Group: Active Input Background", ref CFG.Current.DisplayGroupEditor_CombinedActive_Frame);
                ImGui.ColorEdit4("Combined Group: Active Checkbox", ref CFG.Current.DisplayGroupEditor_CombinedActive_Checkbox);
            }

            ImGui.EndTabItem();
        }
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();
        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(900.0f, 800.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Settings##Popup", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("#SettingsMenuTabBar");
            ImGui.PushStyleColor(ImGuiCol.Header, CFG.Current.Imgui_Moveable_Header);
            ImGui.PushItemWidth(300f);

            // Settings Order
            DisplaySettings_System();
            DisplaySettings_Viewport();
            DisplaySettings_MapEditor();
            DisplaySettings_ModelEditor();
            DisplaySettings_ParamEditor();
            DisplaySettings_TextEditor();
            DisplaySettings_GparamEditor();
            DisplaySettings_Interface();

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
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
}
