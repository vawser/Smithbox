using ImGuiNET;
using SoapstoneLib;
using StudioCore.Editor;
using StudioCore.MsbEditor;
using StudioCore.ParamEditor;
using StudioCore.Scene;
using StudioCore.TextEditor;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Veldrid;

namespace StudioCore;

public class SettingsMenu
{
    private KeyBind _currentKeyBind;
    public bool FontRebuildRequest;
    public bool MenuOpenState;
    public ModelEditorScreen ModelEditor;
    public MsbEditorScreen MsbEditor;
    public ParamEditorScreen ParamEditor;
    public ProjectSettings? ProjSettings = null;
    public TextEditorScreen TextEditor;

    public void SaveSettings()
    {
        CFG.Save();
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
            {
                preset.Name = nameInput;
            }

            foreach (RenderFilter e in Enum.GetValues(typeof(RenderFilter)))
            {
                var ticked = false;
                if (preset.Filters.HasFlag(e))
                {
                    ticked = true;
                }

                if (ImGui.Checkbox(e.ToString(), ref ticked))
                {
                    if (ticked)
                    {
                        preset.Filters |= e;
                    }
                    else
                    {
                        preset.Filters &= ~e;
                    }
                }
            }

            ImGui.Unindent();
        }

        ImGui.PopID();
    }

    private void DisplayUISettings()
    {
        if (ImGui.BeginTabItem("UI Settings"))
        {
            ImGui.Indent();

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

            ImGui.Checkbox("Compact param editor", ref CFG.Current.UI_CompactParams);

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Additional Language Fonts"))
            {
                ImGui.Indent();

                ImGui.Text("Additional fonts take more VRAM and increase startup time.");
                if (ImGui.Checkbox("Chinese", ref CFG.Current.FontChinese))
                {
                    FontRebuildRequest = true;
                }

                if (ImGui.Checkbox("Korean", ref CFG.Current.FontKorean))
                {
                    FontRebuildRequest = true;
                }

                if (ImGui.Checkbox("Thai", ref CFG.Current.FontThai))
                {
                    FontRebuildRequest = true;
                }

                if (ImGui.Checkbox("Vietnamese", ref CFG.Current.FontVietnamese))
                {
                    FontRebuildRequest = true;
                }

                if (ImGui.Checkbox("Cyrillic", ref CFG.Current.FontCyrillic))
                {
                    FontRebuildRequest = true;
                }

                ImGui.Unindent();
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplayProjectSettings()
    {
        if (ImGui.BeginTabItem("Project Settings"))
        {
            ImGui.Indent();

            if (ProjSettings == null || ProjSettings.ProjectName == null)
            {
                ImGui.Text("No project loaded");
            }
            else
            {
                if (TaskManager.AnyActiveTasks())
                {
                    ImGui.Text("Waiting for program tasks to finish...");
                }
                else
                {
                    ImGui.Text($@"Project: {ProjSettings.ProjectName}");
                    if (ImGui.Button("Open project settings file"))
                    {
                        var projectPath = CFG.Current.LastProjectFile;
                        Process.Start("explorer.exe", projectPath);
                    }

                    var useLoose = ProjSettings.UseLooseParams;
                    if (ProjSettings.GameType is GameType.DarkSoulsIISOTFS or GameType.DarkSoulsIII
                        && ImGui.Checkbox("Use loose params", ref useLoose))
                    {
                        ProjSettings.UseLooseParams = useLoose;
                    }

                    var usepartial = ProjSettings.PartialParams;
                    if ((FeatureFlags.EnablePartialParam || usepartial) &&
                        ProjSettings.GameType == GameType.EldenRing &&
                        ImGui.Checkbox("Partial params", ref usepartial))
                    {
                        ProjSettings.PartialParams = usepartial;
                    }
                }
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplayMapSettings()
    {
        if (ImGui.BeginTabItem("Map Settings"))
        {
            ImGui.Indent();

            if (ImGui.CollapsingHeader("Map Editor"))
            {
                ImGui.Indent();
                ImGui.Checkbox("Enable texturing (alpha)", ref CFG.Current.EnableTexturing);
                ImGui.Checkbox("Exclude loaded maps from search filter", ref CFG.Current.Map_AlwaysListLoadedMaps);
                ImGui.Checkbox("Enable Elden Ring auto map offset", ref CFG.Current.EnableEldenRingAutoMapOffset);
                ImGui.Unindent();
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Selection"))
            {
                ImGui.Indent();

                var arbitrary_rotation_x = CFG.Current.Map_ArbitraryRotation_X_Shift;
                var arbitrary_rotation_y = CFG.Current.Map_ArbitraryRotation_Y_Shift;
                var camera_radius_offset = CFG.Current.Map_MoveSelectionToCamera_Radius;

                if (ImGui.InputFloat("Rotation increment degrees: Roll", ref arbitrary_rotation_x))
                {
                    CFG.Current.Map_ArbitraryRotation_X_Shift = Math.Clamp(arbitrary_rotation_x, -180.0f, 180.0f);
                }

                if (ImGui.InputFloat("Rotation increment degrees: Yaw", ref arbitrary_rotation_y))
                {
                    CFG.Current.Map_ArbitraryRotation_Y_Shift = Math.Clamp(arbitrary_rotation_y, -180.0f, 180.0f);
                    ;
                }

                if (ImGui.DragFloat("Move selection to camera (offset distance)", ref camera_radius_offset))
                {
                    CFG.Current.Map_MoveSelectionToCamera_Radius = camera_radius_offset;
                }

                ImGui.Unindent();
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Camera"))
            {
                ImGui.Indent();
                var cam_fov = CFG.Current.GFX_Camera_FOV;
                if (ImGui.SliderFloat("Camera FOV", ref cam_fov, 40.0f, 140.0f))
                {
                    CFG.Current.GFX_Camera_FOV = cam_fov;
                }

                var farClip = CFG.Current.GFX_RenderDistance_Max;
                if (ImGui.SliderFloat("Map max render distance", ref farClip, 10.0f, 500000.0f))
                {
                    CFG.Current.GFX_RenderDistance_Max = farClip;
                }

                if (ImGui.SliderFloat("Map camera speed (slow)",
                        ref MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow, 0.1f, 999.0f))
                {
                    CFG.Current.GFX_Camera_MoveSpeed_Slow = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Slow;
                }

                if (ImGui.SliderFloat("Map camera speed (normal)",
                        ref MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal, 0.1f, 999.0f))
                {
                    CFG.Current.GFX_Camera_MoveSpeed_Normal = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Normal;
                }

                if (ImGui.SliderFloat("Map camera speed (fast)",
                        ref MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast, 0.1f, 999.0f))
                {
                    CFG.Current.GFX_Camera_MoveSpeed_Fast = MsbEditor.Viewport.WorldView.CameraMoveSpeed_Fast;
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

                ImGui.Unindent();
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Gizmos"))
            {
                ImGui.Indent();

                ImGui.ColorEdit3("X Axis - base color", ref CFG.Current.GFX_Gizmo_X_BaseColor);
                ImGui.ColorEdit3("X Axis - highlight color", ref CFG.Current.GFX_Gizmo_X_HighlightColor);

                ImGui.ColorEdit3("Y Axis - base color", ref CFG.Current.GFX_Gizmo_Y_BaseColor);
                ImGui.ColorEdit3("Y Axis - highlight color", ref CFG.Current.GFX_Gizmo_Y_HighlightColor);

                ImGui.ColorEdit3("Z Axis - base color", ref CFG.Current.GFX_Gizmo_Z_BaseColor);
                ImGui.ColorEdit3("Z Axis - highlight color", ref CFG.Current.GFX_Gizmo_Z_HighlightColor);

                if (ImGui.Button("Reset colors to default"))
                {
                    CFG.Current.GFX_Gizmo_X_BaseColor = new Vector3(0.952f, 0.211f, 0.325f);
                    CFG.Current.GFX_Gizmo_X_HighlightColor = new Vector3(1.0f, 0.4f, 0.513f);

                    CFG.Current.GFX_Gizmo_Y_BaseColor = new Vector3(0.525f, 0.784f, 0.082f);
                    CFG.Current.GFX_Gizmo_Y_HighlightColor = new Vector3(0.713f, 0.972f, 0.270f);

                    CFG.Current.GFX_Gizmo_Z_BaseColor = new Vector3(0.219f, 0.564f, 0.929f);
                    CFG.Current.GFX_Gizmo_Z_HighlightColor = new Vector3(0.407f, 0.690f, 1.0f);
                }

                ImGui.Unindent();
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Map Object Display Presets"))
            {
                ImGui.Indent();

                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_01);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_02);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_03);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_04);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_05);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_06);
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

                ImGui.Unindent();
            }

            ImGui.Separator();

            if (ImGui.CollapsingHeader("Limits"))
            {
                ImGui.Indent();

                ImGui.Text("Please restart the program for changes to take effect.");

                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    @"Try smaller increments (+25%%) at first, as high values will cause issues.");

                if (ImGui.InputInt("Renderables", ref CFG.Current.GFX_Limit_Renderables, 0, 0))
                {
                    if (CFG.Current.GFX_Limit_Renderables < CFG.Default.GFX_Limit_Renderables)
                    {
                        CFG.Current.GFX_Limit_Renderables = CFG.Default.GFX_Limit_Renderables;
                    }
                }

                Utils.ImGui_InputUint("Indirect Draw buffer", ref CFG.Current.GFX_Limit_Buffer_Indirect_Draw);
                Utils.ImGui_InputUint("FLVER Bone buffer", ref CFG.Current.GFX_Limit_Buffer_Flver_Bone);

                if (ImGui.Button("Reset##MapLimits"))
                {
                    CFG.Current.GFX_Limit_Renderables = CFG.Default.GFX_Limit_Renderables;
                    CFG.Current.GFX_Limit_Buffer_Indirect_Draw = CFG.Default.GFX_Limit_Buffer_Indirect_Draw;
                    CFG.Current.GFX_Limit_Buffer_Flver_Bone = CFG.Default.GFX_Limit_Buffer_Flver_Bone;
                }

                ImGui.Unindent();
            }

            ImGui.Separator();

            ImGui.SliderFloat("Wireframe color variance", ref CFG.Current.GFX_Wireframe_Color_Variance, 0.0f, 1.0f);
            ImGui.SameLine();
            if (ImGui.Button("Reset##WireframeColorVariance"))
            {
                CFG.Current.GFX_Wireframe_Color_Variance = CFG.Default.GFX_Wireframe_Color_Variance;
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplayKeybindSettings()
    {
        if (ImGui.BeginTabItem("Keybinds"))
        {
            ImGui.Indent();

            if (ImGui.IsAnyItemActive())
            {
                _currentKeyBind = null;
            }

            FieldInfo[] binds = KeyBindings.Current.GetType().GetFields();
            foreach (FieldInfo bind in binds)
            {
                var bindVal = (KeyBind)bind.GetValue(KeyBindings.Current);
                ImGui.Text(bind.Name);

                ImGui.SameLine();
                ImGui.Indent(250f);

                var keyText = bindVal.HintText;
                if (keyText == "")
                {
                    keyText = "[None]";
                }

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
                {
                    _currentKeyBind = bindVal;
                }

                ImGui.Indent(-250f);
            }

            ImGui.Separator();

            if (ImGui.Button("Restore defaults"))
            {
                KeyBindings.ResetKeyBinds();
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplayParamSettings()
    {
        if (ImGui.BeginTabItem("Param Settings"))
        {
            ImGui.Indent();

            ImGui.Checkbox("Show community field names first", ref CFG.Current.Param_MakeMetaNamesPrimary);
            ImGui.Checkbox("Show secondary field names", ref CFG.Current.Param_ShowSecondaryNames);
            ImGui.Checkbox("Show field data offsets", ref CFG.Current.Param_ShowFieldOffsets);
            ImGui.Checkbox("Hide field references", ref CFG.Current.Param_HideReferenceRows);
            ImGui.Checkbox("Hide field enums", ref CFG.Current.Param_HideEnums);
            ImGui.Checkbox("Allow field reordering", ref CFG.Current.Param_AllowFieldReorder);
            if (ImGui.Checkbox("Sort params alphabetically", ref CFG.Current.Param_AlphabeticalParams))
            {
                UICache.ClearCaches();
            }

            ImGui.Checkbox("Disable row grouping", ref CFG.Current.Param_DisableRowGrouping);
            ImGui.Checkbox("Show hotkey tools in context menus", ref CFG.Current.Param_ShowHotkeysInContextMenu);
            ImGui.Checkbox("Disable line wrapping", ref CFG.Current.Param_DisableLineWrapping);
            ImGui.Checkbox("Show advanced massedit options", ref CFG.Current.Param_AdvancedMassedit);

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplayFmgSettings()
    {
        if (ImGui.BeginTabItem("FMG Text Settings"))
        {
            ImGui.Indent();

            ImGui.Checkbox("Show original FMG names", ref CFG.Current.FMG_ShowOriginalNames);
            if (ImGui.Checkbox("Separate related FMGs and entries", ref CFG.Current.FMG_NoGroupedFmgEntries))
            {
                TextEditor.OnProjectChanged(ProjSettings);
            }

            if (ImGui.Checkbox("Separate patch FMGs", ref CFG.Current.FMG_NoFmgPatching))
            {
                TextEditor.OnProjectChanged(ProjSettings);
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void DisplayMiscSettings()
    {
        if (ImGui.BeginTabItem("Misc Settings"))
        {
            ImGui.Indent();

            if (ImGui.CollapsingHeader("Soapstone Server"))
            {
                ImGui.Indent();

                var running = SoapstoneServer.GetRunningPort() is int port
                    ? $"running on port {port}"
                    : "not running";
                ImGui.Text(
                    $"The server is {running}.\nIt is not accessible over the network, only to other programs on this computer.\nPlease restart the program for changes to take effect.");
                ImGui.Checkbox("Enable cross-editor features", ref CFG.Current.EnableSoapstone);

                ImGui.Unindent();
            }

            ImGui.Separator();

            ImGui.Checkbox("Check for new versions of DSMapStudio during startup",
                ref CFG.Current.EnableCheckProgramUpdate);

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    public void Display()
    {
        var scale = MapStudioNew.GetUIScale();
        if (!MenuOpenState)
        {
            return;
        }

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

            //
            DisplayUISettings();
            //
            DisplayProjectSettings();
            //
            DisplayMapSettings();
            //
            DisplayParamSettings();
            //
            DisplayKeybindSettings();
            //
            DisplayFmgSettings();
            //
            DisplayMiscSettings();
            //

            ImGui.PopItemWidth();
            ImGui.PopStyleColor();
            ImGui.EndTabBar();
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(2);
    }
}
