using ImGuiNET;
using SoapstoneLib;
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
using StudioCore.Editors.TextureViewer;
using StudioCore.TextureViewer;
using System.IO;
using StudioCore.Platform;

namespace StudioCore.Interface.Windows;

public class SettingsWindow
{
    public bool MenuOpenState;

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
            DisplaySettings_TextureViewer();
            DisplaySettings_AssetBrowser();
            DisplaySettings_Interface();

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    private void DisplaySettings_System()
    {
        if (ImGui.BeginTabItem("System"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Check for new versions of Smithbox during startup",
                    ref CFG.Current.System_Check_Program_Update);
                ImguiUtils.ShowHoverTooltip("When enabled Smithbox will automatically check for new versions upon program start.");

                ImGui.SliderFloat("Frame Rate", ref CFG.Current.System_Frame_Rate, 20.0f, 240.0f);
                ImguiUtils.ShowHoverTooltip("Adjusts the frame rate of the viewport.");

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
                ImguiUtils.ShowHoverTooltip("Enable this if you are attempting to mod files that are 'encrypted'.");

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

            if(ImGui.CollapsingHeader("Resources"))
            {
                ImGui.Checkbox("Alias Banks - Editor Mode", ref CFG.Current.AliasBank_EditorMode);
                ImguiUtils.ShowHoverTooltip("If enabled, editing the name and tags for alias banks will commit the changes to the Smithbox base version instead of the mod-specific version.");

                if (FeatureFlags.EnableEditor_TimeAct)
                {
                    ImGui.Checkbox("Time Act Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_TimeAct);
                    ImguiUtils.ShowHoverTooltip("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
                }

                if (FeatureFlags.EnableEditor_Cutscene)
                {
                    ImGui.Checkbox("Cutscene Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_Cutscene);
                    ImguiUtils.ShowHoverTooltip("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
                }

                if (FeatureFlags.EnableEditor_Material)
                {
                    ImGui.Checkbox("Material Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_Material);
                    ImguiUtils.ShowHoverTooltip("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
                }

                if (FeatureFlags.EnableEditor_Particle)
                {
                    ImGui.Checkbox("Particle Editor - Automatic Resource Loading", ref CFG.Current.AutoLoadBank_Particle);
                    ImguiUtils.ShowHoverTooltip("If enabled, the resource bank required for this editor will be loaded at startup.\n\nIf disabled, the user will have to press the Load button within the editor to load the resources.\n\nThe benefit if disabled is that the RAM usage and startup time of Smithbox will be decreased.");
                }
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_AssetBrowser()
    {
        if (ImGui.BeginTabItem("Asset Browser"))
        {
            // General
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display aliases in browser list", ref CFG.Current.AssetBrowser_ShowAliasesInBrowser);
                ImguiUtils.ShowHoverTooltip("Show the aliases for each entry within the browser list as part of their displayed name.");

                ImGui.Checkbox("Display tags in browser list", ref CFG.Current.AssetBrowser_ShowTagsInBrowser);
                ImguiUtils.ShowHoverTooltip("Show the tags for each entry within the browser list as part of their displayed name.");

                ImGui.Checkbox("Display low-detail parts in browser list", ref CFG.Current.AssetBrowser_ShowLowDetailParts);
                ImguiUtils.ShowHoverTooltip("Show the _l (low-detail) part entries in the Model Editor instance of the Asset Browser.");
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
                ImguiUtils.ShowHoverTooltip("Enabling this option will allow DSMS to render the textures of models within the viewport.\n\nNote, this feature is in an alpha state.");

                ImGui.Checkbox("Enable frustum culling", ref CFG.Current.Viewport_Frustum_Culling);
                ImguiUtils.ShowHoverTooltip("Enabling this option will cause entities outside of the camera frustum to be culled.");

                //ImGui.ColorEdit3("Viewport Background Color", ref CFG.Current.Viewport_BackgroundColor);
                //ImguiUtils.ShowHoverTooltip("Change the background color in the viewport. Requires a restart of Smithbox to take effect.");

            }

            if (ImGui.CollapsingHeader("Rendering", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputFloat("Default Model Render: Brightness", ref CFG.Current.Viewport_DefaultRender_Brightness);
                ImguiUtils.ShowHoverTooltip("Change the brightness modifier for the Default Model Rendering shader.");
                ImGui.InputFloat("Default Model Render: Saturation", ref CFG.Current.Viewport_DefaultRender_Saturation);
                ImguiUtils.ShowHoverTooltip("Change the saturation modifier for the Default Model Rendering shader.");


                ImGui.Checkbox("Enable selection outline", ref CFG.Current.Viewport_Enable_Selection_Outline);
                ImguiUtils.ShowHoverTooltip("Enabling this option will cause a selection outline to appear on selected objects.");

                ImGui.ColorEdit3("Selection Color", ref CFG.Current.Viewport_DefaultRender_SelectColor);

                if (ImGui.Button("Reset##ResetRenderProperties"))
                {
                    CFG.Current.Viewport_DefaultRender_Brightness = 1.0f;
                    CFG.Current.Viewport_DefaultRender_Saturation = 0.5f;
                }
                ImguiUtils.ShowHoverTooltip("Resets all of the values within this section to their default values.");
            }

            if (ImGui.CollapsingHeader("Camera"))
            {
                if (ImGui.Button("Reset##ViewportCamera"))
                {
                    CFG.Current.Viewport_Camera_FOV = CFG.Default.Viewport_Camera_FOV;

                    CFG.Current.Viewport_RenderDistance_Max = CFG.Default.Viewport_RenderDistance_Max;

                    Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Slow = CFG.Default.Viewport_Camera_MoveSpeed_Slow;
                    CFG.Current.Viewport_Camera_MoveSpeed_Slow = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Slow;
                    CFG.Current.Viewport_Camera_Sensitivity = CFG.Default.Viewport_Camera_Sensitivity;

                    Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Normal = CFG.Default.Viewport_Camera_MoveSpeed_Normal;
                    CFG.Current.Viewport_Camera_MoveSpeed_Normal = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Normal;

                    Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Fast = CFG.Default.Viewport_Camera_MoveSpeed_Fast;
                    CFG.Current.Viewport_Camera_MoveSpeed_Fast = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Fast;
                }
                ImguiUtils.ShowHoverTooltip("Resets all of the values within this section to their default values.");

                var cam_fov = CFG.Current.Viewport_Camera_FOV;

                if (ImGui.SliderFloat("Camera FOV", ref cam_fov, 40.0f, 140.0f))
                    CFG.Current.Viewport_Camera_FOV = cam_fov;
                ImguiUtils.ShowHoverTooltip("Set the field of view used by the camera within DSMS.");

                var cam_sensitivity = CFG.Current.Viewport_Camera_Sensitivity;

                if (ImGui.SliderFloat("Camera sensitivity", ref cam_sensitivity, 0.0f, 0.1f))
                    CFG.Current.Viewport_Camera_Sensitivity = cam_sensitivity;
                ImguiUtils.ShowHoverTooltip("Mouse sensitivty for turning the camera.");

                var farClip = CFG.Current.Viewport_RenderDistance_Max;

                if (ImGui.SliderFloat("Map max render distance", ref farClip, 10.0f, 500000.0f))
                    CFG.Current.Viewport_RenderDistance_Max = farClip;
                ImguiUtils.ShowHoverTooltip("Set the maximum distance at which entities will be rendered within the DSMS viewport.");

                if (ImGui.SliderFloat("Map camera speed (slow)",
                        ref Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Slow, 0.1f, 9999.0f))
                    CFG.Current.Viewport_Camera_MoveSpeed_Slow = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Slow;
                ImguiUtils.ShowHoverTooltip("Set the speed at which the camera will move when the Left or Right Shift key is pressed whilst moving.");

                if (ImGui.SliderFloat("Map camera speed (normal)",
                        ref Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Normal, 0.1f, 9999.0f))
                    CFG.Current.Viewport_Camera_MoveSpeed_Normal = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Normal;
                ImguiUtils.ShowHoverTooltip("Set the speed at which the camera will move whilst moving normally.");

                if (ImGui.SliderFloat("Map camera speed (fast)",
                        ref Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Fast, 0.1f, 9999.0f))
                    CFG.Current.Viewport_Camera_MoveSpeed_Fast = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Fast;
                ImguiUtils.ShowHoverTooltip("Set the speed at which the camera will move when the Left or Right Control key is pressed whilst moving.");
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
                ImguiUtils.ShowHoverTooltip("Reset the values within this section to their default values.");

                ImGui.Text("Please restart the program for changes to take effect.");

                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    @"Try smaller increments (+25%%) at first, as high values will cause issues.");

                if (ImGui.InputInt("Renderables", ref CFG.Current.Viewport_Limit_Renderables, 0, 0))
                    if (CFG.Current.Viewport_Limit_Renderables < CFG.Default.Viewport_Limit_Renderables)
                        CFG.Current.Viewport_Limit_Renderables = CFG.Default.Viewport_Limit_Renderables;
                ImguiUtils.ShowHoverTooltip("This value constrains the number of renderable entities that are allowed. Exceeding this value will throw an exception.");

                Utils.ImGui_InputUint("Indirect Draw buffer", ref CFG.Current.Viewport_Limit_Buffer_Indirect_Draw);
                ImguiUtils.ShowHoverTooltip("This value constrains the size of the indirect draw buffer. Exceeding this value will throw an exception.");

                Utils.ImGui_InputUint("FLVER Bone buffer", ref CFG.Current.Viewport_Limit_Buffer_Flver_Bone);
                ImguiUtils.ShowHoverTooltip("This value constrains the size of the FLVER bone buffer. Exceeding this value will throw an exception.");
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
                ImguiUtils.ShowHoverTooltip("Resets all of the values within this section to their default values.");

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


            // Map Object Display Presets
            if (ImGui.CollapsingHeader("Display Presets"))
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
                ImguiUtils.ShowHoverTooltip("Reset the values within this section to their default values.");

                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_01);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_02);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_03);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_04);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_05);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_06);
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
                
                ImGui.Checkbox("Enable map load on double-click", ref CFG.Current.MapEditor_Enable_Map_Load_on_Double_Click);
                ImguiUtils.ShowHoverTooltip("This option will cause double-clicking on a map in the map object list to load it.");

                ImGui.Checkbox("Exclude loaded maps from search filter", ref CFG.Current.MapEditor_Always_List_Loaded_Maps);
                ImguiUtils.ShowHoverTooltip("This option will cause loaded maps to always be visible within the map list, ignoring the search filter.");

                if (Project.Config != null)
                {
                    if (Project.Type is ProjectType.ER)
                    {
                        ImGui.Checkbox("Enable Elden Ring auto map offset", ref CFG.Current.Viewport_Enable_ER_Auto_Map_Offset);
                        ImguiUtils.ShowHoverTooltip("");
                    }
                }
            }

            // Scene View
            if (ImGui.CollapsingHeader("Map Object List"))
            {
                ImGui.Checkbox("Display list sorting type", ref CFG.Current.MapEditor_MapObjectList_ShowListSortingType);
                ImguiUtils.ShowHoverTooltip("Display the list sorting type combo box.");

                ImGui.Checkbox("Display map object list search", ref CFG.Current.MapEditor_MapObjectList_ShowMapIdSearch);
                ImguiUtils.ShowHoverTooltip("Display the map object list search text box.");

                ImGui.Checkbox("Display map names", ref CFG.Current.MapEditor_MapObjectList_ShowMapNames);
                ImguiUtils.ShowHoverTooltip("Map names will be displayed within the scene view list.");

                ImGui.Checkbox("Display character names", ref CFG.Current.MapEditor_MapObjectList_ShowCharacterNames);
                ImguiUtils.ShowHoverTooltip("Characters names will be displayed within the scene view list.");

                ImGui.Checkbox("Display asset names", ref CFG.Current.MapEditor_MapObjectList_ShowAssetNames);
                ImguiUtils.ShowHoverTooltip("Asset/object names will be displayed within the scene view list.");

                ImGui.Checkbox("Display map piece names", ref CFG.Current.MapEditor_MapObjectList_ShowMapPieceNames);
                ImguiUtils.ShowHoverTooltip("Map piece names will be displayed within the scene view list.");

                ImGui.Checkbox("Display treasure names", ref CFG.Current.MapEditor_MapObjectList_ShowTreasureNames);
                ImguiUtils.ShowHoverTooltip("Treasure itemlot names will be displayed within the scene view list.");
            }

            // Property View
            if (ImGui.CollapsingHeader("Properties"))
            {
                ImGui.Checkbox("Display community names", ref CFG.Current.MapEditor_Enable_Commmunity_Names);
                ImguiUtils.ShowHoverTooltip("The MSB property fields will be given crowd-sourced names instead of the canonical name.");

                ImGui.Checkbox("Display community descriptions", ref CFG.Current.MapEditor_Enable_Commmunity_Hints);
                ImguiUtils.ShowHoverTooltip("The MSB property fields will be given crowd-sourced descriptions.");

                ImGui.Checkbox("Display property info", ref CFG.Current.MapEditor_Enable_Property_Info);
                ImguiUtils.ShowHoverTooltip("The MSB property fields show the property info, such as minimum and maximum values, when right-clicked.");

                ImGui.Checkbox("Display property class info", ref CFG.Current.MapEditor_Enable_Property_Property_Class_Info);
                ImguiUtils.ShowHoverTooltip("The MSB property view will display information relating to the map object's class.");

                ImGui.Checkbox("Display property references", ref CFG.Current.MapEditor_Enable_Property_Property_References);
                ImguiUtils.ShowHoverTooltip("The MSB property view will display references by and for the selected map object.");

                ImGui.Checkbox("Display property filter", ref CFG.Current.MapEditor_Enable_Property_Filter);
                ImguiUtils.ShowHoverTooltip("The MSB property filter combo-box will be visible.");

                ImGui.Checkbox("Display param quick links", ref CFG.Current.MapEditor_Enable_Param_Quick_Links);
                ImguiUtils.ShowHoverTooltip("The param quick links at the top of the MSB property view will be visible.");
            }

            // Substitutions
            if (ImGui.CollapsingHeader("Substitutions"))
            {
                ImGui.Checkbox("Substitute c0000 entity", ref CFG.Current.MapEditor_Substitute_PseudoPlayer_Model);
                ImguiUtils.ShowHoverTooltip("The c0000 enemy that represents the player-like enemies will be given a visual model substitution so it is visible.");

                ImGui.InputText("##modelString", ref CFG.Current.MapEditor_Substitute_PseudoPlayer_ChrID, 255);
                ImguiUtils.ShowHoverTooltip("The Chr ID of the model you want to use as the replacement.");
            }

            // Grid
            if (ImGui.CollapsingHeader("Viewport Grid"))
            {
                ImGui.SliderInt("Grid size", ref CFG.Current.MapEditor_Viewport_Grid_Size, 100, 1000);
                ImguiUtils.ShowHoverTooltip("The overall maximum size of the grid.\nThe grid will only update upon restarting DSMS after changing this value.");

                ImGui.SliderInt("Grid increment", ref CFG.Current.MapEditor_Viewport_Grid_Square_Size, 1, 100);
                ImguiUtils.ShowHoverTooltip("The increment size of the grid.");

                var height = CFG.Current.MapEditor_Viewport_Grid_Height;

                ImGui.InputFloat("Grid height", ref height);
                ImguiUtils.ShowHoverTooltip("The height at which the horizontal grid sits.");

                if (height < -10000)
                    height = -10000;

                if (height > 10000)
                    height = 10000;

                CFG.Current.MapEditor_Viewport_Grid_Height = height;

                ImGui.SliderFloat("Grid height increment", ref CFG.Current.MapEditor_Viewport_Grid_Height_Increment, 0.1f, 100);
                ImguiUtils.ShowHoverTooltip("The amount to lower or raise the viewport grid height via the shortcuts.");

                ImGui.ColorEdit3("Grid color", ref CFG.Current.MapEditor_Viewport_Grid_Color);

                if (ImGui.Button("Reset"))
                {
                    CFG.Current.MapEditor_Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
                    CFG.Current.MapEditor_Viewport_Grid_Size = 1000;
                    CFG.Current.MapEditor_Viewport_Grid_Square_Size = 10;
                    CFG.Current.MapEditor_Viewport_Grid_Height = 0;
                }
                ImguiUtils.ShowHoverTooltip("Resets all of the values within this section to their default values.");
            }

            // Selection Groups
            if (ImGui.CollapsingHeader("Selection Groups", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Frame selection group on select", ref CFG.Current.MapEditor_SelectionGroup_FrameSelection);
                ImguiUtils.ShowHoverTooltip("Frame the selection group entities automatically in the viewport when selecting a group.");

                ImGui.Checkbox("Enable group auto-creation", ref CFG.Current.MapEditor_SelectionGroup_AutoCreation);
                ImguiUtils.ShowHoverTooltip("The selection group will be given the name of the first entity within the selection as the group name and no tags, bypassing the creation prompt.");

                ImGui.Checkbox("Enable group deletion prompt", ref CFG.Current.MapEditor_SelectionGroup_ConfirmDelete);
                ImguiUtils.ShowHoverTooltip("Display the confirmation dialog when deleting a group.");

                ImGui.Checkbox("Show keybind in selection group name", ref CFG.Current.MapEditor_SelectionGroup_ShowKeybind);
                ImguiUtils.ShowHoverTooltip("Append the keybind hint to the selection group name.");

                ImGui.Checkbox("Show tags in selection group name", ref CFG.Current.MapEditor_SelectionGroup_ShowTags);
                ImguiUtils.ShowHoverTooltip("Append the tags to the selection group name.");
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }



    private void DisplaySettings_ModelEditor()
    {
        if (ImGui.BeginTabItem("Model Editor"))
        {
            // Property View
            if (ImGui.CollapsingHeader("Properties", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display community names", ref CFG.Current.ModelEditor_Enable_Commmunity_Names);
                ImguiUtils.ShowHoverTooltip("The FLVER property fields will be given crowd-sourced names instead of the canonical name.");

                ImGui.Checkbox("Display community descriptions", ref CFG.Current.ModelEditor_Enable_Commmunity_Hints);
                ImguiUtils.ShowHoverTooltip("The FLVER property fields will be given crowd-sourced descriptions.");

            }

            // Scene View
            if (ImGui.CollapsingHeader("Model Hierarchy", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display material names with meshes", ref CFG.Current.ModelEditor_DisplayMatNameOnMesh);
                ImguiUtils.ShowHoverTooltip("Display the material name that a mesh uses by the scene tree name.");

                ImGui.Checkbox("Display dummy polygon reference ids", ref CFG.Current.ModelEditor_DisplayDmyPolyReferenceID);
                ImguiUtils.ShowHoverTooltip("Display the reference ID of a dummy polygon by the scene tree name.");
            }

            // Grid
            if (ImGui.CollapsingHeader("Viewport Grid"))
            {
                ImGui.SliderInt("Grid size", ref CFG.Current.ModelEditor_Viewport_Grid_Size, 100, 1000);
                ImguiUtils.ShowHoverTooltip("The overall maximum size of the grid.\nThe grid will only update upon restarting DSMS after changing this value.");

                ImGui.SliderInt("Grid increment", ref CFG.Current.ModelEditor_Viewport_Grid_Square_Size, 1, 100);
                ImguiUtils.ShowHoverTooltip("The increment size of the grid.");

                var height = CFG.Current.ModelEditor_Viewport_Grid_Height;

                ImGui.InputFloat("Grid height", ref height);
                ImguiUtils.ShowHoverTooltip("The height at which the horizontal grid sits.");

                if (height < -10000)
                    height = -10000;

                if (height > 10000)
                    height = 10000;

                CFG.Current.ModelEditor_Viewport_Grid_Height = height;

                ImGui.SliderFloat("Grid height increment", ref CFG.Current.ModelEditor_Viewport_Grid_Height_Increment, 0.1f, 100);
                ImguiUtils.ShowHoverTooltip("The amount to lower or raise the viewport grid height via the shortcuts.");

                ImGui.ColorEdit3("Grid color", ref CFG.Current.ModelEditor_Viewport_Grid_Color);

                if (ImGui.Button("Reset"))
                {
                    CFG.Current.ModelEditor_Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
                    CFG.Current.ModelEditor_Viewport_Grid_Size = 1000;
                    CFG.Current.ModelEditor_Viewport_Grid_Square_Size = 10;
                    CFG.Current.ModelEditor_Viewport_Grid_Height = 0;
                }
                ImguiUtils.ShowHoverTooltip("Resets all of the values within this section to their default values.");
            }

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
                if(ImGui.Checkbox("Use project meta", ref CFG.Current.Param_UseProjectMeta))
                {
                    if (CFG.Current.Param_UseProjectMeta)
                    {
                        ParamBank.CreateProjectMeta();
                    }

                    ParamBank.ReloadParams();
                }
                ImguiUtils.ShowHoverTooltip("Use project-specific Paramdex meta instead of Smithbox's base version.");

                ImGui.Checkbox("Use compact param editor", ref CFG.Current.UI_CompactParams);
                ImguiUtils.ShowHoverTooltip("Reduces the line height within the the Param Editor screen.");

                ImGui.Checkbox("Show advanced options in massedit popup", ref CFG.Current.Param_AdvancedMassedit);
                ImguiUtils.ShowHoverTooltip("Show additional options for advanced users within the massedit popup.");

                ImGui.Checkbox("Pinned rows stay visible", ref CFG.Current.Param_PinnedRowsStayVisible);
                ImguiUtils.ShowHoverTooltip("Pinned rows will stay visible when you scroll instead of only being pinned to the top of the list.");
            }

            // Params
            if (ImGui.CollapsingHeader("Params"))
            {
                if (ImGui.Checkbox("Sort params alphabetically", ref CFG.Current.Param_AlphabeticalParams))
                    UICache.ClearCaches();
                ImguiUtils.ShowHoverTooltip("Sort the Param View list alphabetically.");
            }

            // Rows
            if (ImGui.CollapsingHeader("Rows"))
            {
                ImGui.Checkbox("Disable line wrapping", ref CFG.Current.Param_DisableLineWrapping);
                ImguiUtils.ShowHoverTooltip("Disable the row names from wrapping within the Row View list.");

                ImGui.Checkbox("Disable row grouping", ref CFG.Current.Param_DisableRowGrouping);
                ImguiUtils.ShowHoverTooltip("Disable the grouping of connected rows in certain params, such as ItemLotParam within the Row View list.");
            }

            // Fields
            if (ImGui.CollapsingHeader("Fields"))
            {
                ImGui.Checkbox("Show community field names first", ref CFG.Current.Param_MakeMetaNamesPrimary);
                ImguiUtils.ShowHoverTooltip("Crowd-sourced names will appear before the canonical name in the Field View list.");

                ImGui.Checkbox("Show secondary field names", ref CFG.Current.Param_ShowSecondaryNames);
                ImguiUtils.ShowHoverTooltip("The crowd-sourced name (or the canonical name if the above option is enabled) will appear after the initial name in the Field View list.");

                ImGui.Checkbox("Show field data offsets", ref CFG.Current.Param_ShowFieldOffsets);
                ImguiUtils.ShowHoverTooltip("The field offset within the .PARAM file will be show to the left in the Field View List.");

                ImGui.Checkbox("Hide field references", ref CFG.Current.Param_HideReferenceRows);
                ImguiUtils.ShowHoverTooltip("Hide the generated param references for fields that link to other params.");

                ImGui.Checkbox("Hide field enums", ref CFG.Current.Param_HideEnums);
                ImguiUtils.ShowHoverTooltip("Hide the crowd-sourced namelist for index-based enum fields.");

                ImGui.Checkbox("Allow field reordering", ref CFG.Current.Param_AllowFieldReorder);
                ImguiUtils.ShowHoverTooltip("Allow the field order to be changed by an alternative order as defined within the Paramdex META file.");

                ImGui.Checkbox("Hide padding fields", ref CFG.Current.Param_HidePaddingFields);
                ImguiUtils.ShowHoverTooltip("Hides fields that are considered 'padding' in the property editor view.");

                ImGui.Checkbox("Show color preview", ref CFG.Current.Param_ShowColorPreview);
                ImguiUtils.ShowHoverTooltip("Show color preview in field column if applicable.");

                ImGui.Checkbox("Show graph visualisation", ref CFG.Current.Param_ShowGraphVisualisation);
                ImguiUtils.ShowHoverTooltip("Show graph visualisation in field column if applicable.");
            }

            // Values
            if (ImGui.CollapsingHeader("Values"))
                {
                    ImGui.Checkbox("Show inverted percentages as traditional percentages", ref CFG.Current.Param_ShowTraditionalPercentages);
                ImguiUtils.ShowHoverTooltip("Displays field values that utilise the (1 - x) pattern as traditional percentages (e.g. -20 instead of 1.2).");
            }

            // Context Menu
            if (ImGui.CollapsingHeader("Row Context Menu"))
            {
                ImGui.Checkbox("Display row name input", ref CFG.Current.Param_RowContextMenu_NameInput);
                ImguiUtils.ShowHoverTooltip("Display a row name input within the right-click context menu.");

                ImGui.Checkbox("Display row shortcut tools", ref CFG.Current.Param_RowContextMenu_ShortcutTools);
                ImguiUtils.ShowHoverTooltip("Show the shortcut tools in the right-click row context menu.");

                ImGui.Checkbox("Display row pin options", ref CFG.Current.Param_RowContextMenu_PinOptions);
                ImguiUtils.ShowHoverTooltip("Show the pin options in the right-click row context menu.");

                ImGui.Checkbox("Display row compare options", ref CFG.Current.Param_RowContextMenu_CompareOptions);
                ImguiUtils.ShowHoverTooltip("Show the compare options in the right-click row context menu.");

                ImGui.Checkbox("Display row reverse lookup option", ref CFG.Current.Param_RowContextMenu_ReverseLoopup);
                ImguiUtils.ShowHoverTooltip("Show the reverse lookup option in the right-click row context menu.");

                ImGui.Checkbox("Display row copy id option", ref CFG.Current.Param_RowContextMenu_CopyID);
                ImguiUtils.ShowHoverTooltip("Show the copy id option in the right-click row context menu.");
            }

            // Context Menu
            if (ImGui.CollapsingHeader("Field Context Menu"))
            {

                ImGui.Checkbox("Split context menu", ref CFG.Current.Param_FieldContextMenu_Split);
                ImguiUtils.ShowHoverTooltip("Split the field context menu into separate menus for separate right-click locations.");

                ImGui.Checkbox("Display field name", ref CFG.Current.Param_FieldContextMenu_Name);
                ImguiUtils.ShowHoverTooltip("Display the field name in the context menu.");

                ImGui.Checkbox("Display field description", ref CFG.Current.Param_FieldContextMenu_Description);
                ImguiUtils.ShowHoverTooltip("Display the field description in the context menu.");

                ImGui.Checkbox("Display field property info", ref CFG.Current.Param_FieldContextMenu_PropertyInfo);
                ImguiUtils.ShowHoverTooltip("Display the field property info in the context menu.");

                ImGui.Checkbox("Display field pin options", ref CFG.Current.Param_FieldContextMenu_PinOptions);
                ImguiUtils.ShowHoverTooltip("Display the field pin options in the context menu.");

                ImGui.Checkbox("Display field compare options", ref CFG.Current.Param_FieldContextMenu_CompareOptions);
                ImguiUtils.ShowHoverTooltip("Display the field compare options in the context menu.");

                ImGui.Checkbox("Display field value distribution option", ref CFG.Current.Param_FieldContextMenu_ValueDistribution);
                ImguiUtils.ShowHoverTooltip("Display the field value distribution option in the context menu.");

                ImGui.Checkbox("Display field add options", ref CFG.Current.Param_FieldContextMenu_AddOptions);
                ImguiUtils.ShowHoverTooltip("Display the field add to searchbar and mass edit options in the context menu.");

                ImGui.Checkbox("Display field reference search", ref CFG.Current.Param_FieldContextMenu_ReferenceSearch);
                ImguiUtils.ShowHoverTooltip("Display the field reference search in the context menu.");

                ImGui.Checkbox("Display field mass edit options", ref CFG.Current.Param_FieldContextMenu_MassEdit);
                ImguiUtils.ShowHoverTooltip("Display the field mass edit options in the context menu.");

                ImGui.Checkbox("Display full mass edit submenu", ref CFG.Current.Param_FieldContextMenu_FullMassEdit);
                ImguiUtils.ShowHoverTooltip("If enabled, the right-click context menu for fields shows a comprehensive editing popup for the massedit feature.\nIf disabled, simply shows a shortcut to the manual massedit entry element.\n(The full menu is still available from the manual popup)");
            }

            // Context Menu
            if (ImGui.CollapsingHeader("Image Preview"))
            {

                ImGui.Text("Image Preview Scale:");
                ImGui.DragFloat("##imagePreviewScale", ref CFG.Current.Param_FieldContextMenu_ImagePreviewScale, 0.1f, 0.1f, 10.0f);
                ImguiUtils.ShowHoverTooltip("Scale of the previewed image.");

                ImGui.Checkbox("Display image preview in field context menu", ref CFG.Current.Param_FieldContextMenu_ImagePreview_ContextMenu);
                ImguiUtils.ShowHoverTooltip("Display image preview of any image index fields if possible within the field context menu.");

                ImGui.Checkbox("Display image preview in field column", ref CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn);
                ImguiUtils.ShowHoverTooltip("Display image preview of any image index fields if possible at the bottom of the field column.");
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
                ImguiUtils.ShowHoverTooltip("Show the original FMG file names within the Text Editor file list.");

                if (ImGui.Checkbox("Separate related FMGs and entries", ref CFG.Current.FMG_NoGroupedFmgEntries))
                    Smithbox.EditorHandler.TextEditor.OnProjectChanged();
                ImguiUtils.ShowHoverTooltip("If enabled then FMG entries will not be grouped automatically.");

                if (ImGui.Checkbox("Separate patch FMGs", ref CFG.Current.FMG_NoFmgPatching))
                    Smithbox.EditorHandler.TextEditor.OnProjectChanged();
                ImguiUtils.ShowHoverTooltip("If enabled then FMG files added from DLCs will not be grouped with vanilla FMG files.");
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_GparamEditor()
    {
        if (ImGui.BeginTabItem("Gparam Editor"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Display aliases in file list", ref CFG.Current.Interface_Display_Alias_for_Gparam);
                ImguiUtils.ShowHoverTooltip("Toggle the display of the aliases in the file list.");
            }

            if (ImGui.CollapsingHeader("Groups", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show add button for missing groups", ref CFG.Current.Gparam_DisplayAddGroups);
                ImguiUtils.ShowHoverTooltip("Show the Add button for groups that are not present.");

                ImGui.Checkbox("Show empty groups", ref CFG.Current.Gparam_DisplayEmptyGroups);
                ImguiUtils.ShowHoverTooltip("Display empty groups in the group list.");
            }

            if (ImGui.CollapsingHeader("Fields", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show add button for missing fields", ref CFG.Current.Gparam_DisplayAddFields);
                ImguiUtils.ShowHoverTooltip("Show the Add button for fields that are not present.");
            }

            if (ImGui.CollapsingHeader("Values", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show color edit for 4 digit properties", ref CFG.Current.Gparam_DisplayColorEditForVector4Fields);
                ImguiUtils.ShowHoverTooltip("Show the color edit tool for 4 digit properties.");
            }

            if (ImGui.CollapsingHeader("Color Edit", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if(ImGui.Checkbox("Show color as Integer RGB", ref CFG.Current.Gparam_ColorEdit_RGB))
                {
                    CFG.Current.Gparam_ColorEdit_Decimal = false;
                    CFG.Current.Gparam_ColorEdit_HSV = false;
                }
                ImguiUtils.ShowHoverTooltip("Show the color data as Integer RGB color (0 to 255)");

                if (ImGui.Checkbox("Show color as Decimal RGB", ref CFG.Current.Gparam_ColorEdit_Decimal))
                {
                    CFG.Current.Gparam_ColorEdit_RGB = false;
                    CFG.Current.Gparam_ColorEdit_HSV = false;
                }
                ImguiUtils.ShowHoverTooltip("Show the color data as Decimal RGB color (0.0 to 1.0)");

                if (ImGui.Checkbox("Show color as HSV", ref CFG.Current.Gparam_ColorEdit_HSV))
                {
                    CFG.Current.Gparam_ColorEdit_RGB = false;
                    CFG.Current.Gparam_ColorEdit_Decimal = false;
                }
                ImguiUtils.ShowHoverTooltip("Show the color data as Hue, Saturation, Value color (0.0 to 1.0)");
            }

            if (ImGui.CollapsingHeader("Quick Edit", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputText("Filter: ID", ref CFG.Current.Gparam_QuickEdit_ID, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'ID' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Filter: Time of Day", ref CFG.Current.Gparam_QuickEdit_TimeOfDay, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Time of Day' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Filter: Value", ref CFG.Current.Gparam_QuickEdit_Value, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Value' filter argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Command: Set", ref CFG.Current.Gparam_QuickEdit_Set, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Set' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Command: Addition", ref CFG.Current.Gparam_QuickEdit_Add, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Addition' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Command: Subtract", ref CFG.Current.Gparam_QuickEdit_Subtract, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Subtract' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Command: Multiply", ref CFG.Current.Gparam_QuickEdit_Multiply, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Multiply' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Command: Set by Row", ref CFG.Current.Gparam_QuickEdit_SetByRow, 255);
                ImguiUtils.ShowHoverTooltip("The text string to detect for the 'Set By Row' command argument.\nWarning: if multiple arguments have the same string, it will cause issues.");

                ImGui.InputText("Delimiter", ref CFG.Current.Gparam_QuickEdit_Chain, 255);
                ImguiUtils.ShowHoverTooltip("The text string to split filter and commands.");

                if (ImGui.Button("Reset to Default"))
                {
                    CFG.Current.Gparam_QuickEdit_Chain = "+";

                    CFG.Current.Gparam_QuickEdit_ID = "id";
                    CFG.Current.Gparam_QuickEdit_TimeOfDay = "tod";
                    CFG.Current.Gparam_QuickEdit_Value = "value";

                    CFG.Current.Gparam_QuickEdit_Set = "set";
                    CFG.Current.Gparam_QuickEdit_Add = "add";
                    CFG.Current.Gparam_QuickEdit_Subtract = "sub";
                    CFG.Current.Gparam_QuickEdit_Multiply = "mult";
                    CFG.Current.Gparam_QuickEdit_SetByRow = "setbyrow";
                }
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_TextureViewer()
    {
        if (ImGui.BeginTabItem("Texture Viewer"))
        {
            if (ImGui.CollapsingHeader("File List", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show character names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Characters);
                ImguiUtils.ShowHoverTooltip("Show matching character aliases within the file list.");

                ImGui.Checkbox("Show asset names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Assets);
                ImguiUtils.ShowHoverTooltip("Show matching asset/object aliases within the file list.");

                ImGui.Checkbox("Show part names", ref CFG.Current.TextureViewer_FileList_ShowAliasName_Parts);
                ImguiUtils.ShowHoverTooltip("Show matching part aliases within the file list.");

                ImGui.Checkbox("Show low detail entries", ref CFG.Current.TextureViewer_FileList_ShowLowDetail_Entries);
                ImguiUtils.ShowHoverTooltip("Show the low-detail texture containers.");
            }

            if (ImGui.CollapsingHeader("Texture List", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Show particle names", ref CFG.Current.TextureViewer_TextureList_ShowAliasName_Particles);
                ImguiUtils.ShowHoverTooltip("Show matching particle aliases within the texture list.");
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_Interface()
    {
        if (ImGui.BeginTabItem("User Interface"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("Wrap alias text", ref CFG.Current.System_WrapAliasDisplay);
                ImguiUtils.ShowHoverTooltip("Makes the alias text display wrap instead of being cut off.");

                ImGui.Checkbox("Show tooltips", ref CFG.Current.System_Show_UI_Tooltips);
                ImguiUtils.ShowHoverTooltip("This is a tooltip.");

                ImGui.SliderFloat("UI scale", ref CFG.Current.System_UI_Scale, 0.5f, 4.0f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    // Round to 0.05
                    CFG.Current.System_UI_Scale = (float)Math.Round(CFG.Current.System_UI_Scale * 20) / 20;
                    Smithbox.UIScaleChanged?.Invoke(null, EventArgs.Empty);
                    Smithbox.FontRebuildRequest = true;
                }
                ImguiUtils.ShowHoverTooltip("Adjusts the scale of the user interface throughout all of Smithbox.");

            }

            // Additional Language Fonts
            if (ImGui.CollapsingHeader("Additional Language Fonts"))
            {
                if (ImGui.Checkbox("Chinese", ref CFG.Current.System_Font_Chinese))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Chinese font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Korean", ref CFG.Current.System_Font_Korean))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Korean font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Thai", ref CFG.Current.System_Font_Thai))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Thai font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Vietnamese", ref CFG.Current.System_Font_Vietnamese))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Vietnamese font.\nAdditional fonts take more VRAM and increase startup time.");

                if (ImGui.Checkbox("Cyrillic", ref CFG.Current.System_Font_Cyrillic))
                    Smithbox.FontRebuildRequest = true;
                ImguiUtils.ShowHoverTooltip("Include Cyrillic font.\nAdditional fonts take more VRAM and increase startup time.");
            }


            if (ImGui.CollapsingHeader("Theme", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Text("Current Theme");

                if (ImGui.ListBox("##themeSelect", ref CFG.Current.SelectedTheme, UI.LoadedThemeNames, UI.LoadedThemeNames.Length))
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
                ImGui.SameLine();

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
            }

            ImGui.EndTabItem();
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
}
