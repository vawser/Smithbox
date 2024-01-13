using ImGuiNET;
using SoapstoneLib;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.MsbEditor;
using StudioCore.ParamEditor;
using StudioCore.Scene;
using StudioCore.TextEditor;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Reflection;
using Veldrid;

namespace StudioCore.Settings;

public class SettingsMenu
{
    private KeyBind _currentKeyBind;
    public bool FontRebuildRequest;
    public bool MenuOpenState;
    public ModelEditorScreen ModelEditor;
    public MsbEditorScreen MsbEditor;
    public ParamEditorScreen ParamEditor;
    public ProjectSettings ProjSettings = null;
    public TextEditorScreen TextEditor;

    public void SaveSettings()
    {
        CFG.Save();
    }

    private void DisplaySettings_System()
    {
        if (ImGui.BeginTabItem("System"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("When enabled Smithbox will automatically check for new versions upon program start.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Check for new versions of Smithbox during startup",
                    ref CFG.Current.EnableCheckProgramUpdate);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("This is a tooltip.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show UI tooltips", ref CFG.Current.ShowUITooltips);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Adjusts the scale of the user interface throughout all of Smithbox.");
                    ImGui.SameLine();
                }
                ImGui.SliderFloat("UI scale", ref CFG.Current.UIScale, 0.5f, 4.0f);

                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    // Round to 0.05
                    CFG.Current.UIScale = (float)Math.Round(CFG.Current.UIScale * 20) / 20;
                    FontRebuildRequest = true;
                }

                ImGui.SameLine();
                if (ImGui.Button("Reset"))
                {
                    CFG.Current.UIScale = CFG.Default.UIScale;
                    FontRebuildRequest = true;
                }
            }

            if (ImGui.CollapsingHeader("Soapstone Server"))
            {
                var running = SoapstoneServer.GetRunningPort() is int port
                    ? $"running on port {port}"
                    : "not running";
                ImGui.Text(
                    $"The server is {running}.\nIt is not accessible over the network, only to other programs on this computer.\nPlease restart the program for changes to take effect.");
                ImGui.Checkbox("Enable cross-editor features", ref CFG.Current.EnableSoapstone);
            }

            // Additional Language Fonts
            if (ImGui.CollapsingHeader("Additional Language Fonts"))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Include Chinese font.\nAdditional fonts take more VRAM and increase startup time.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Chinese", ref CFG.Current.FontChinese))
                    FontRebuildRequest = true;

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Include Korean font.\nAdditional fonts take more VRAM and increase startup time.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Korean", ref CFG.Current.FontKorean))
                    FontRebuildRequest = true;

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Include Thai font.\nAdditional fonts take more VRAM and increase startup time.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Thai", ref CFG.Current.FontThai))
                    FontRebuildRequest = true;

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Include Vietnamese font.\nAdditional fonts take more VRAM and increase startup time.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Vietnamese", ref CFG.Current.FontVietnamese))
                    FontRebuildRequest = true;

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Include Cyrillic font.\nAdditional fonts take more VRAM and increase startup time.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Cyrillic", ref CFG.Current.FontCyrillic))
                    FontRebuildRequest = true;
            }

            if (ImGui.CollapsingHeader("Project", ImGuiTreeNodeFlags.DefaultOpen))
                if (ProjSettings == null || ProjSettings.ProjectName == null)
                {
                    if (CFG.Current.ShowUITooltips)
                    {
                        ShowHelpMarker("No project has been loaded yet.");
                        ImGui.SameLine();
                    }
                    ImGui.Text("No project loaded");
                }
                else
                    if (TaskManager.AnyActiveTasks())
                    {
                        if (CFG.Current.ShowUITooltips)
                        {
                            ShowHelpMarker("DSMS must finished all program tasks before it can load a project.");
                            ImGui.SameLine();
                        }
                        ImGui.Text("Waiting for program tasks to finish...");
                    }
                    else
                    {
                        if (CFG.Current.ShowUITooltips)
                        {
                            ShowHelpMarker("This is the currently loaded project.");
                            ImGui.SameLine();
                        }
                        ImGui.Text($@"Project: {ProjSettings.ProjectName}");

                        if (ImGui.Button("Open project settings file"))
                        {
                            var projectPath = CFG.Current.LastProjectFile;
                            Process.Start("explorer.exe", projectPath);
                        }

                        var useLoose = ProjSettings.UseLooseParams;
                        if (ProjSettings.GameType is GameType.DarkSoulsIISOTFS or GameType.DarkSoulsIII)
                        {
                            if (CFG.Current.ShowUITooltips)
                            {
                                ShowHelpMarker("Loose params means the .PARAM files will be saved outside of the regulation.bin file.\n\nFor Dark Souls II: Scholar of the First Sin, it is recommended that you enable this if add any additional rows.");
                                ImGui.SameLine();
                            }

                            if (ImGui.Checkbox("Use loose params", ref useLoose))
                                ProjSettings.UseLooseParams = useLoose;
                        }

                        var usepartial = ProjSettings.PartialParams;
                        if (FeatureFlags.EnablePartialParam || usepartial)
                        {
                            if (CFG.Current.ShowUITooltips)
                            {
                                ShowHelpMarker("Partial params.");
                                ImGui.SameLine();
                            }

                            if (ProjSettings.GameType == GameType.EldenRing &&
                            ImGui.Checkbox("Partial params", ref usepartial))
                                ProjSettings.PartialParams = usepartial;
                        }
                    }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_MapEditor()
    {
        if (ImGui.BeginTabItem("Map Editor"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Enabling this option will cause entities outside of the camera frustrum to be culled.\n\nDisable this if working with the grid.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Enable frustrum culling", ref CFG.Current.EnableFrustrumCulling);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Enabling this option will cause a selection outline to appear on selected objects.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Enable selection outline", ref CFG.Current.Map_EnableSelectionOutline);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Enabling this option will allow DSMS to render the textures of models within the viewport.\n\nNote, this feature is in an alpha state.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Enable texturing", ref CFG.Current.EnableTexturing);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("This option will cause loaded maps to always be visible within the map list, ignoring the search filter.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Exclude loaded maps from search filter", ref CFG.Current.Map_AlwaysListLoadedMaps);

                if (ProjSettings != null)
                    if (ProjSettings.GameType is GameType.EldenRing)
                    {
                        if (CFG.Current.ShowUITooltips)
                        {
                            ShowHelpMarker("");
                            ImGui.SameLine();
                        }
                        ImGui.Checkbox("Enable Elden Ring auto map offset", ref CFG.Current.EnableEldenRingAutoMapOffset);
                    }
            }

            if (ImGui.CollapsingHeader("Selection"))
            {
                var arbitrary_rotation_x = CFG.Current.Map_ArbitraryRotation_X_Shift;
                var arbitrary_rotation_y = CFG.Current.Map_ArbitraryRotation_Y_Shift;
                var camera_radius_offset = CFG.Current.Map_MoveSelectionToCamera_Radius;

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Set the angle increment amount used by Arbitary Rotation in the X-axis.");
                    ImGui.SameLine();
                }
                if (ImGui.InputFloat("Rotation increment degrees: Roll", ref arbitrary_rotation_x))
                    CFG.Current.Map_ArbitraryRotation_X_Shift = Math.Clamp(arbitrary_rotation_x, -180.0f, 180.0f);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Set the angle increment amount used by Arbitary Rotation in the Y-axis.");
                    ImGui.SameLine();
                }
                if (ImGui.InputFloat("Rotation increment degrees: Yaw", ref arbitrary_rotation_y))
                {
                    CFG.Current.Map_ArbitraryRotation_Y_Shift = Math.Clamp(arbitrary_rotation_y, -180.0f, 180.0f);
                    ;
                }

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Set the distance at which the current select is offset from the camera when using the Move Selection to Camera action.");
                    ImGui.SameLine();
                }
                if (ImGui.DragFloat("Move selection to camera (offset distance)", ref camera_radius_offset))
                    CFG.Current.Map_MoveSelectionToCamera_Radius = camera_radius_offset;
            }

            if (ImGui.CollapsingHeader("Camera"))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Resets all of the values within this section to their default values.");
                    ImGui.SameLine();
                }
                if (ImGui.Button("Reset##ViewportCamera"))
                {
                    CFG.Current.GFX_Camera_FOV = CFG.Default.GFX_Camera_FOV;

                    CFG.Current.GFX_RenderDistance_Max = CFG.Default.GFX_RenderDistance_Max;

                    MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow = CFG.Default.GFX_Camera_MoveSpeed_Slow;
                    CFG.Current.GFX_Camera_MoveSpeed_Slow = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow;

                    MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal = CFG.Default.GFX_Camera_MoveSpeed_Normal;
                    CFG.Current.GFX_Camera_MoveSpeed_Normal = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal;

                    MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast = CFG.Default.GFX_Camera_MoveSpeed_Fast;
                    CFG.Current.GFX_Camera_MoveSpeed_Fast = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast;
                }

                var cam_fov = CFG.Current.GFX_Camera_FOV;

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Set the field of view used by the camera within DSMS.");
                    ImGui.SameLine();
                }
                if (ImGui.SliderFloat("Camera FOV", ref cam_fov, 40.0f, 140.0f))
                    CFG.Current.GFX_Camera_FOV = cam_fov;

                var farClip = CFG.Current.GFX_RenderDistance_Max;
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Set the maximum distance at which entities will be rendered within the DSMS viewport.");
                    ImGui.SameLine();
                }
                if (ImGui.SliderFloat("Map max render distance", ref farClip, 10.0f, 500000.0f))
                    CFG.Current.GFX_RenderDistance_Max = farClip;

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Set the speed at which the camera will move when the Left or Right Shift key is pressed whilst moving.");
                    ImGui.SameLine();
                }
                if (ImGui.SliderFloat("Map camera speed (slow)",
                        ref MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow, 0.1f, 999.0f))
                    CFG.Current.GFX_Camera_MoveSpeed_Slow = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow;

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Set the speed at which the camera will move whilst moving normally.");
                    ImGui.SameLine();
                }
                if (ImGui.SliderFloat("Map camera speed (normal)",
                        ref MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal, 0.1f, 999.0f))
                    CFG.Current.GFX_Camera_MoveSpeed_Normal = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal;

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Set the speed at which the camera will move when the Left or Right Control key is pressed whilst moving.");
                    ImGui.SameLine();
                }
                if (ImGui.SliderFloat("Map camera speed (fast)",
                        ref MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast, 0.1f, 999.0f))
                    CFG.Current.GFX_Camera_MoveSpeed_Fast = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast;
            }

            if (ImGui.CollapsingHeader("Limits"))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Reset the values within this section to their default values.");
                    ImGui.SameLine();
                }
                if (ImGui.Button("Reset##MapLimits"))
                {
                    CFG.Current.GFX_Limit_Renderables = CFG.Default.GFX_Limit_Renderables;
                    CFG.Current.GFX_Limit_Buffer_Indirect_Draw = CFG.Default.GFX_Limit_Buffer_Indirect_Draw;
                    CFG.Current.GFX_Limit_Buffer_Flver_Bone = CFG.Default.GFX_Limit_Buffer_Flver_Bone;
                }

                ImGui.Text("Please restart the program for changes to take effect.");

                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    @"Try smaller increments (+25%%) at first, as high values will cause issues.");

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("This value constrains the number of renderable entities that are allowed. Exceeding this value will throw an exception.");
                    ImGui.SameLine();
                }
                if (ImGui.InputInt("Renderables", ref CFG.Current.GFX_Limit_Renderables, 0, 0))
                    if (CFG.Current.GFX_Limit_Renderables < CFG.Default.GFX_Limit_Renderables)
                        CFG.Current.GFX_Limit_Renderables = CFG.Default.GFX_Limit_Renderables;

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("This value constrains the size of the indirect draw buffer. Exceeding this value will throw an exception.");
                    ImGui.SameLine();
                }
                Utils.ImGui_InputUint("Indirect Draw buffer", ref CFG.Current.GFX_Limit_Buffer_Indirect_Draw);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("This value constrains the size of the FLVER bone buffer. Exceeding this value will throw an exception.");
                    ImGui.SameLine();
                }
                Utils.ImGui_InputUint("FLVER Bone buffer", ref CFG.Current.GFX_Limit_Buffer_Flver_Bone);
            }

            if (ImGui.CollapsingHeader("Grid"))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Enable the viewport grid when in the Map Editor.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Enable viewport grid", ref CFG.Current.Map_EnableViewportGrid);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("The overall maximum size of the grid.\nThe grid will only update upon restarting DSMS after changing this value.");
                    ImGui.SameLine();
                }
                ImGui.SliderInt("Grid size", ref CFG.Current.Map_ViewportGrid_TotalSize, 100, 1000);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("The increment size of the grid.");
                    ImGui.SameLine();
                }
                ImGui.SliderInt("Grid increment", ref CFG.Current.Map_ViewportGrid_IncrementSize, 1, 100);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("The height at which the horizontal grid sits.");
                    ImGui.SameLine();
                }
                ImGui.SliderFloat("Grid height", ref CFG.Current.Map_ViewportGrid_Offset, -1000, 1000);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("The amount to lower or raise the viewport grid height via the shortcuts.");
                    ImGui.SameLine();
                }
                ImGui.SliderFloat("Grid height increment", ref CFG.Current.Map_ViewportGrid_ShortcutIncrement, 0.1f, 100);

                ImGui.ColorEdit3("Grid color", ref CFG.Current.GFX_Viewport_Grid_Color);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Resets all of the values within this section to their default values.");
                    ImGui.SameLine();
                }
                if (ImGui.Button("Reset"))
                {
                    CFG.Current.GFX_Viewport_Grid_Color = Utils.GetDecimalColor(Color.Red);
                    CFG.Current.Map_ViewportGrid_TotalSize = 1000;
                    CFG.Current.Map_ViewportGrid_IncrementSize = 10;
                    CFG.Current.Map_ViewportGrid_Offset = 0;
                }
            }

            if (ImGui.CollapsingHeader("Wireframes"))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Resets all of the values within this section to their default values.");
                    ImGui.SameLine();
                }
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

                    // Gizmos
                    CFG.Current.GFX_Gizmo_X_BaseColor = new Vector3(0.952f, 0.211f, 0.325f);
                    CFG.Current.GFX_Gizmo_X_HighlightColor = new Vector3(1.0f, 0.4f, 0.513f);

                    CFG.Current.GFX_Gizmo_Y_BaseColor = new Vector3(0.525f, 0.784f, 0.082f);
                    CFG.Current.GFX_Gizmo_Y_HighlightColor = new Vector3(0.713f, 0.972f, 0.270f);

                    CFG.Current.GFX_Gizmo_Z_BaseColor = new Vector3(0.219f, 0.564f, 0.929f);
                    CFG.Current.GFX_Gizmo_Z_HighlightColor = new Vector3(0.407f, 0.690f, 1.0f);

                    // Color Variance
                    CFG.Current.GFX_Wireframe_Color_Variance = CFG.Default.GFX_Wireframe_Color_Variance;
                }

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

                // Gizmos
                ImGui.ColorEdit3("Gizmo - X Axis - base color", ref CFG.Current.GFX_Gizmo_X_BaseColor);
                ImGui.ColorEdit3("Gizmo - X Axis - highlight color", ref CFG.Current.GFX_Gizmo_X_HighlightColor);

                ImGui.ColorEdit3("Gizmo - Y Axis - base color", ref CFG.Current.GFX_Gizmo_Y_BaseColor);
                ImGui.ColorEdit3("Gizmo - Y Axis - highlight color", ref CFG.Current.GFX_Gizmo_Y_HighlightColor);

                ImGui.ColorEdit3("Gizmo - Z Axis - base color", ref CFG.Current.GFX_Gizmo_Z_BaseColor);
                ImGui.ColorEdit3("Gizmo - Z Axis - highlight color", ref CFG.Current.GFX_Gizmo_Z_HighlightColor);
            }

            if (ImGui.CollapsingHeader("Map Object Display Presets"))
            {
                ImGui.Text("Configure each of the six display presets available.");

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Reset the values within this section to their default values.");
                    ImGui.SameLine();
                }
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

    private void DisplaySettings_ModelEditor()
    {
        if (ImGui.BeginTabItem("Model Editor"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {

            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_ParamEditor()
    {
        if (ImGui.BeginTabItem("Param Editor"))
        {
            if (ImGui.CollapsingHeader("General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Reduces the line height within the the Param Editor screen.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Use compact param editor", ref CFG.Current.UI_CompactParams);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Show additional options within the MassEdit context menu.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show advanced massedit options", ref CFG.Current.Param_AdvancedMassedit);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Show the shortcut tools in the right-click context menu.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show shortcut tools in context menus", ref CFG.Current.Param_ShowHotkeysInContextMenu);
            }

            if (ImGui.CollapsingHeader("Params"))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Sort the Param View list alphabetically.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Sort params alphabetically", ref CFG.Current.Param_AlphabeticalParams))
                    UICache.ClearCaches();
            }

            if (ImGui.CollapsingHeader("Rows"))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Disable the row names from wrapping within the Row View list.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Disable line wrapping", ref CFG.Current.Param_DisableLineWrapping);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Disable the grouping of connected rows in certain params, such as ItemLotParam within the Row View list.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Disable row grouping", ref CFG.Current.Param_DisableRowGrouping);
            }

            if (ImGui.CollapsingHeader("Fields"))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Crowd-sourced names will appear before the canonical name in the Field View list.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show community field names first", ref CFG.Current.Param_MakeMetaNamesPrimary);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("The crowd-sourced name (or the canonical name if the above option is enabled) will appear after the initial name in the Field View list.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show secondary field names", ref CFG.Current.Param_ShowSecondaryNames);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("The field offset within the .PARAM file will be show to the left in the Field View List.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show field data offsets", ref CFG.Current.Param_ShowFieldOffsets);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Hide the generated param references for fields that link to other params.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Hide field references", ref CFG.Current.Param_HideReferenceRows);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Hide the crowd-sourced namelist for index-based enum fields.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Hide field enums", ref CFG.Current.Param_HideEnums);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Allow the field order to be changed by an alternative order as defined within the Paramdex META file.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Allow field reordering", ref CFG.Current.Param_AllowFieldReorder);
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
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Show the original FMG file names within the Text Editor file list.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show original FMG names", ref CFG.Current.FMG_ShowOriginalNames);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("If enabled then FMG entries will not be grouped automatically.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Separate related FMGs and entries", ref CFG.Current.FMG_NoGroupedFmgEntries))
                    TextEditor.OnProjectChanged(ProjSettings);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("If enabled then FMG files added from DLCs will not be grouped with vanilla FMG files.");
                    ImGui.SameLine();
                }
                if (ImGui.Checkbox("Separate patch FMGs", ref CFG.Current.FMG_NoFmgPatching))
                    TextEditor.OnProjectChanged(ProjSettings);
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_Browsers()
    {
        if (ImGui.BeginTabItem("Browsers"))
        {
            if (ImGui.CollapsingHeader("Asset Browser", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Show the tags for each entry within the browser list as part of their displayed name.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show tags", ref CFG.Current.AssetBrowser_ShowTagsInBrowser);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Update the Name field of the entity upon double-click. By default only the ModelName field is updated.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Update entity name on selection", ref CFG.Current.AssetBrowser_UpdateSelectionName);

                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Suspends the browser list when moving in the Map Editor, preventing hitching.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Suspend browser list when in viewport", ref CFG.Current.AssetBrowser_SuspendListWhenInViewport);
            }

            if (ImGui.CollapsingHeader("Flag ID Browser"))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Show the tags for each entry within the browser list as part of their displayed name.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show tags", ref CFG.Current.EventFlagBrowser_ShowTagsInBrowser);
            }

            if (ImGui.CollapsingHeader("Particle ID Browser"))
            {
                if (CFG.Current.ShowUITooltips)
                {
                    ShowHelpMarker("Show the tags for each entry within the browser list as part of their displayed name.");
                    ImGui.SameLine();
                }
                ImGui.Checkbox("Show tags", ref CFG.Current.FxrBrowser_ShowTagsInBrowser);
            }

            ImGui.EndTabItem();
        }
    }

    private void DisplaySettings_Keybinds()
    {
        if (ImGui.BeginTabItem("Keybinds"))
        {
            if (ImGui.IsAnyItemActive())
                _currentKeyBind = null;

            FieldInfo[] binds = KeyBindings.Current.GetType().GetFields();
            foreach (FieldInfo bind in binds)
            {
                var bindVal = (KeyBind)bind.GetValue(KeyBindings.Current);
                ImGui.Text(bind.Name);

                ImGui.SameLine();
                ImGui.Indent(250f);

                var keyText = bindVal.HintText;
                if (keyText == "")
                    keyText = "[None]";

                if (_currentKeyBind == bindVal)
                {
                    ImGui.Button("Press Key <Esc - Clear>");
                    if (InputTracker.GetKeyDown(Key.Escape))
                    {
                        bind.SetValue(KeyBindings.Current, new KeyBind());
                        _currentKeyBind = null;
                    }
                    else
                    {
                        KeyBind newkey = InputTracker.GetNewKeyBind();
                        if (newkey != null)
                        {
                            bind.SetValue(KeyBindings.Current, newkey);
                            _currentKeyBind = null;
                        }
                    }
                }
                else if (ImGui.Button($"{keyText}##{bind.Name}"))
                    _currentKeyBind = bindVal;

                ImGui.Indent(-250f);
            }

            ImGui.Separator();

            if (ImGui.Button("Restore defaults"))
                KeyBindings.ResetKeyBinds();

            ImGui.EndTabItem();
        }
    }
    public void Display()
    {
        var scale = Smithbox.GetUIScale();
        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(900.0f, 800.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0f, 0f, 0f, 0.98f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, new Vector4(0.25f, 0.25f, 0.25f, 1.0f));
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(20.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Settings Menu##Popup", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.BeginTabBar("#SettingsMenuTabBar");
            ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.3f, 0.3f, 0.6f, 0.4f));
            ImGui.PushItemWidth(300f);

            // Settings Order
            DisplaySettings_System();
            DisplaySettings_MapEditor();
            //DisplaySettings_ModelEditor();
            DisplaySettings_ParamEditor();
            DisplaySettings_TextEditor();
            DisplaySettings_Browsers();
            DisplaySettings_Keybinds();

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(2);
    }

    public void ShowHelpMarker(string desc)
    {
        ImGui.TextDisabled("(?)");
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(450.0f);
            ImGui.TextUnformatted(desc);
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
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
