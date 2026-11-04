using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// The tool window section that configures how an editor's viewport renders: its materials, lighting and, in the
/// Map Editor, the environment map, irradiance volumes and the map's GPARAM lighting. The Model and Time Act
/// Editors get the irradiance volume and GPARAM lighting tabs too, for the map they borrow them from
/// (<see cref="ViewportEnvironment" />). Each is a tab. The values apply to the editor's current viewport, or to
/// every viewport for those kept in the settings.
/// </summary>
public class ViewportRenderingTool
{
    private readonly Func<VulkanViewport> GetViewport;

    private float GrassDensity = CFG.Current.MapEditor_Grass_Density;

    // The Map Lighting tab's GPARAM list filter
    private string GparamFilter = "";

    /// <summary>
    /// Set to open the section the next time the tool window is drawn, as the viewport's Settings menu does
    /// </summary>
    public bool FocusRequested;

    // The tabs' less-used sections, which each tab shows or hides with the eye on its header. They are shared by the
    // tabs, so showing the status in one shows it in all of them.
    private bool ShowStatus = true;
    private bool ShowResolvedValues = false;
    private bool ShowDebug = false;

    public ViewportRenderingTool(Func<VulkanViewport> getViewport)
    {
        GetViewport = getViewport;
    }

    public void OnToolWindow()
    {
        if (FocusRequested)
        {
            ImGui.SetNextItemOpen(true);
            FocusRequested = false;
        }

        if (ToolSections.Header($"{LOC.Get("VIEWPORT_Rendering_Tool_Header")}##viewportRenderingTool"))
        {
            ImGui.BeginChild("viewportRenderingSection", ImGuiChildFlags.Borders);

            var viewport = GetViewport();

            if (viewport == null)
            {
                ImGui.Text(LOC.Get("VIEWPORT_Rendering_Tool_No_Viewport"));
            }
            else
            {
                DisplayTabs(viewport);
            }

            ImGui.EndChild();
        }
    }

    /// <summary>
    /// The environment a Model or Time Act Editor viewport borrows from a map, null for other viewports and for
    /// project types without map lighting
    /// </summary>
    private static ViewportEnvironment GetEnvironment(VulkanViewport viewport)
    {
        var project = viewport.Owner switch
        {
            ModelUniverse model => model.Project,
            TimeActUniverse timeAct => timeAct.Project,
            _ => null
        };

        return project?.Handler.ExistingViewportEnvironment is { } environment && environment.CanUse()
            ? environment
            : null;
    }

    /// <summary>
    /// The tabs, lighting first as it is what is changed most. Tone mapping is drawn in the Post Effects tab, as
    /// it is the last step of that chain, and the collision opacity in the Materials tab.
    /// </summary>
    private void DisplayTabs(VulkanViewport viewport)
    {
        // A narrow tool window can't fit every tab, so they scroll and can be picked from a list instead
        ImGui.BeginTabBar("viewportRenderingTabs", ImGuiTabBarFlags.FittingPolicyScroll | ImGuiTabBarFlags.ListPopupButton);

        var mapView = (viewport.Owner as MapUniverse)?.View;
        var environment = mapView == null ? GetEnvironment(viewport) : null;

        // The GPARAMs the Post Effects tab reads, and the maps it lists, where there are any
        GparamLightingBank postBank = null;
        var postMaps = new List<(string MapId, string File)>();

        if (mapView != null && mapView.GparamLightingBank.CanUse())
        {
            postBank = mapView.GparamLightingBank;
            var chosen = CFG.Current.Viewport_Map_Lighting_File;
            postMaps = mapView.Project.Handler.MapData.PrimaryBank.Maps.Values
                .Select(w => w.MapContainer)
                .Where(m => m != null && m.LoadState == MapContentLoadState.Loaded)
                .Select(m => (m.Name, postBank.ChooseFile(m.Name, chosen)))
                .ToList();
        }
        else if (environment != null)
        {
            postBank = environment.LightingBank;

            if (environment.MapId != null)
            {
                postMaps.Add((environment.MapId,
                    postBank.ChooseFile(environment.MapId, CFG.Current.Viewport_Environment_Lighting_File)));
            }
        }

        // Map Lighting
        if (mapView != null && mapView.GparamLightingBank.CanUse())
        {
            if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Menubar_Header_Map_Lighting")}##viewportRenderingMapLighting"))
            {
                MapLightingTab(viewport, mapView);

                ImGui.EndTabItem();
            }
        }
        else if (environment != null)
        {
            if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Menubar_Header_Map_Lighting")}##viewportRenderingMapLighting"))
            {
                EnvironmentLightingTab(viewport, environment);

                ImGui.EndTabItem();
            }
        }

        // Scene Lighting
        if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Menubar_Header_Scene_Lighting")}##viewportRenderingSceneLighting"))
        {
            SceneLightingTab(viewport);

            ImGui.EndTabItem();
        }

        // Global Illumination
        if (mapView != null && mapView.IrradianceVolumeBank.CanUse())
        {
            if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Menubar_Header_Global_Illumination")}##viewportRenderingGlobalIllumination"))
            {
                GlobalIlluminationTab(viewport, mapView);

                ImGui.EndTabItem();
            }
        }
        else if (environment?.Table != null)
        {
            if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Menubar_Header_Global_Illumination")}##viewportRenderingGlobalIllumination"))
            {
                EnvironmentGlobalIlluminationTab(viewport, environment);

                ImGui.EndTabItem();
            }
        }

        // BTL Lights
        if (mapView != null)
        {
            if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Rendering_Tab_BTL_Lights")}##viewportRenderingBtlLights"))
            {
                BtlLightsTab(mapView);

                ImGui.EndTabItem();
            }
        }

        // Shadows
        if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Rendering_Tab_Shadows")}##viewportRenderingShadows"))
        {
            ShadowsTab(viewport);

            ImGui.EndTabItem();
        }

        // Post Effects, with the tone map
        if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Rendering_Tab_Post_Effects")}##viewportRenderingPostEffects"))
        {
            PostEffectsTab(viewport, postBank, postMaps);

            ImGui.EndTabItem();
        }

        // Materials, with the collision opacity
        if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Menubar_Header_Materials")}##viewportRenderingMaterials"))
        {
            MaterialsTab();

            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    /// <summary>
    /// Which of the loaded maps' BTL lights light the viewports, and how brightly. These apply at once, as the
    /// lights are gathered every frame.
    /// </summary>
    private void BtlLightsTab(MapEditorView view)
    {
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Gathering"), LOC.Get("VIEWPORT_Rendering_Section_Gathering_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_BTL_Near_Camera")}##btlNearCamera",
            ref CFG.Current.Viewport_BTL_Lights_Near_Camera);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_BTL_Near_Camera_TT"));

        ImGui.BeginDisabled(!CFG.Current.Viewport_BTL_Lights_Near_Camera);
        ImGui.Indent();

        DPI.ApplyInputWidth(IndentedWidth);
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_BTL_Radius")}##btlRadius",
            ref CFG.Current.Viewport_BTL_Light_Radius, 1.0f, 1000.0f, "%.0f m");
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_BTL_Radius_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_BTL_Enable_Distance")}##btlEnableDistance",
            ref CFG.Current.Viewport_BTL_Light_Use_Enable_Distance);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_BTL_Enable_Distance_TT"));

        ImGui.Unindent();
        ImGui.EndDisabled();

        DPI.ApplyInputWidth();
        ImGui.SliderInt($"{LOC.Get("VIEWPORT_Rendering_BTL_Max_Count")}##btlMaxCount",
            ref CFG.Current.Viewport_BTL_Light_Max_Count, 0, SceneLightTable.Capacity);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_BTL_Max_Count_TT"));

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Intensity"), LOC.Get("VIEWPORT_Rendering_Section_Intensity_TT"));

        // With map lighting, the lights take the GPARAM's exposure, as the sun does
        var exposureFromMap = SceneRenderer.MapLighting != null;

        if (exposureFromMap)
        {
            ImGui.TextDisabled(LOC.Get("VIEWPORT_Rendering_BTL_Intensity_From_Map"));
        }

        ImGui.BeginDisabled(exposureFromMap);
        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("PREF_Viewport_Light_Intensity_Scale")}##btlIntensityScale",
            ref CFG.Current.Viewport_Light_Intensity_Scale, 0.1f, 50.0f);
        ImGui.EndDisabled();
        GUI.Tooltip(exposureFromMap
            ? LOC.Get("VIEWPORT_Rendering_BTL_Intensity_Map_TT",
                SceneRenderer.MapLighting.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale)
            : LOC.Get("PREF_Viewport_Light_Intensity_Scale_TT"));

        if (StatusHeader())
        {
            var collector = view.ViewportWindow.LightCollector;
            ImGui.Text(LOC.Get("VIEWPORT_Rendering_BTL_Status", collector.ShadedCount, collector.InRangeCount));
        }
    }

    /// <summary>
    /// The sun, the light multipliers and the camera light of the viewport
    /// </summary>
    private void SceneLightingTab(VulkanViewport viewport)
    {
        var sceneParams = viewport.ViewPipeline.SceneParams;

        // The map's GPARAMs give the sun instead, see the Map Lighting tab
        var sunFromMap = viewport.ViewPipeline.ActiveMapLighting != null;

        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Sun"), LOC.Get("VIEWPORT_Rendering_Section_Sun_TT"));

        if (sunFromMap)
        {
            ImGui.TextDisabled(LOC.Get("VIEWPORT_Menubar_Scene_Sun_From_Map"));
        }

        ImGui.BeginDisabled(sunFromMap);

        // A direction, so only its first three components are edited
        var direction = new Vector3(sceneParams.LightDirection.X, sceneParams.LightDirection.Y, sceneParams.LightDirection.Z);

        DPI.ApplyInputWidth();
        if (ImGui.SliderFloat3($"{LOC.Get("VIEWPORT_Menubar_Scene_Light_Direction")}##lightDirection", ref direction, -1, 1))
        {
            viewport.ViewPipeline.SceneParams.LightDirection.X = direction.X;
            viewport.ViewPipeline.SceneParams.LightDirection.Y = direction.Y;
            viewport.ViewPipeline.SceneParams.LightDirection.Z = direction.Z;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Scene_Light_Direction_TT"));

        var sunColor = new Vector3(sceneParams.SunColor.X, sceneParams.SunColor.Y, sceneParams.SunColor.Z);

        DPI.ApplyInputWidth();
        if (ImGui.ColorEdit3($"{LOC.Get("VIEWPORT_Rendering_Sun_Color")}##sunColor", ref sunColor))
        {
            viewport.ViewPipeline.SceneParams.SunColor.X = sunColor.X;
            viewport.ViewPipeline.SceneParams.SunColor.Y = sunColor.Y;
            viewport.ViewPipeline.SceneParams.SunColor.Z = sunColor.Z;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Sun_Color_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_Sun_Intensity")}##sunIntensity",
            ref viewport.ViewPipeline.SceneParams.SunColor.W, 0, 5);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Sun_Intensity_TT"));

        ImGui.EndDisabled();

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Multipliers"), LOC.Get("VIEWPORT_Rendering_Section_Multipliers_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat(
            $"{LOC.Get("VIEWPORT_Menubar_Scene_Direct_Light_Mult")}##directLightMult",
            ref viewport.ViewPipeline.SceneParams.DirectLightMult, 0, 3);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Scene_Direct_Light_Mult_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat(
            $"{LOC.Get("VIEWPORT_Menubar_Scene_Indirect_Light_Mult")}##indirectLightMult",
            ref viewport.ViewPipeline.SceneParams.IndirectLightMult, 0, 3);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Scene_Indirect_Light_Mult_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat(
            $"{LOC.Get("VIEWPORT_Menubar_Scene_Light_Brightness")}##brightness",
            ref viewport.ViewPipeline.SceneParams.SceneBrightness, 0, 5);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Scene_Light_Brightness_TT"));

        // A light that follows the camera, for lighting up dark interiors
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Camera_Light"), LOC.Get("VIEWPORT_Rendering_Section_Camera_Light_TT"));

        SceneLight cameraLight = viewport.ViewPipeline.CameraLight;

        var cameraLightEnabled = cameraLight.Enabled;
        if (ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Camera_Light")}##cameraLightEnabled", ref cameraLightEnabled))
        {
            cameraLight.Enabled = cameraLightEnabled;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Camera_Light_TT"));

        ImGui.BeginDisabled(!cameraLight.Enabled);
        ImGui.Indent();

        var color = cameraLight.Color;
        DPI.ApplyInputWidth(IndentedWidth);
        if (ImGui.ColorEdit3($"{LOC.Get("VIEWPORT_Rendering_Camera_Light_Color")}##cameraLightColor", ref color))
        {
            cameraLight.Color = color;
        }

        var intensity = cameraLight.Intensity;
        DPI.ApplyInputWidth(IndentedWidth);
        if (ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_Camera_Light_Intensity")}##cameraLightIntensity", ref intensity, 0, 50))
        {
            cameraLight.Intensity = intensity;
        }

        var range = cameraLight.Range;
        DPI.ApplyInputWidth(IndentedWidth);
        if (ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_Camera_Light_Range")}##cameraLightRange", ref range, 1, 100, "%.0f m"))
        {
            cameraLight.Range = range;
        }

        ImGui.Unindent();
        ImGui.EndDisabled();
    }

    // The width of a slider under an indented toggle, which the indent would otherwise push past the labels
    private const float IndentedWidth = 200.0f;

    /// <summary>
    /// The header of a tab's Status section, true when the section is shown
    /// </summary>
    private bool StatusHeader()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("VIEWPORT_Rendering_Section_Status"), LOC.Get("VIEWPORT_Rendering_Section_Status_TT"),
            ref ShowStatus);

        return ShowStatus;
    }

    /// <summary>
    /// The header of a tab's Resolved Values section, the per-map GPARAM tables, true when the section is shown
    /// </summary>
    private bool ResolvedValuesHeader()
    {
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("VIEWPORT_Rendering_Section_Resolved_Values"),
            LOC.Get("VIEWPORT_Rendering_Section_Resolved_Values_TT"), ref ShowResolvedValues);

        return ShowResolvedValues;
    }

    /// <summary>
    /// The header of a tab's Debug section, true when the section is shown. It is shown anyway while a debug view is
    /// on, so the view can't be left on where nothing says so.
    /// </summary>
    private bool DebugHeader(bool debugViewOn)
    {
        GUI.Spacer();
        GUI.ConditionalHeader(LOC.Get("VIEWPORT_Rendering_Section_Debug"), LOC.Get("VIEWPORT_Rendering_Section_Debug_TT"),
            ref ShowDebug);

        return ShowDebug || debugViewOn;
    }

    /// <summary>
    /// The baked irradiance volumes of the loaded maps (AC6, ER, NR): the bake to use, and how many of each map's
    /// EnvMapEffectBoxes are linked to a volume
    /// </summary>
    private void GlobalIlluminationTab(VulkanViewport viewport, MapEditorView view)
    {
        GlobalIlluminationSettings(viewport, view.IrradianceVolumeBank.BakesFollowTime);

        var maps = view.Project.Handler.MapData.PrimaryBank.Maps.Values
            .Select(w => w.MapContainer)
            .Where(m => m != null && m.LoadState == MapContentLoadState.Loaded)
            .ToList();

        var summaries = maps.Select(m => (Map: m, Summary: view.IrradianceVolumeBank.Summarise(m))).ToList();
        var unlinked = summaries.Sum(s => s.Summary.Unlinked);
        var skipped = summaries.Sum(s => s.Summary.SkippedByGame);

        // Unlinked boxes are a problem to fix, so they are always shown, with the way to fix them
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Linking"), LOC.Get("VIEWPORT_Rendering_Section_Linking_TT"));

        if (unlinked > 0 || skipped > 0)
        {
            ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_Unlinked", unlinked, skipped));
        }

        if (ImGui.Button($"{LOC.Get("VIEWPORT_Menubar_GI_Open_Tool")}##giOpenTool"))
        {
            CFG.Current.Interface_MapEditor_ToolWindow = true;
            CFG.Current.Interface_MapEditor_Tool_Validation = true;
            view.IrradianceVolumeIdTool.FocusRequested = true;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Open_Tool_TT"));

        if (StatusHeader())
        {
            if (maps.Count == 0)
            {
                ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_No_Maps"));
            }

            foreach (var (map, summary) in summaries)
            {
                var volumes = view.IrradianceVolumeBank.GetMap(map.Name);

                ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_Map_Status",
                    map.Name, volumes?.CountInBake(CFG.Current.Viewport_GI_Bake) ?? 0, CFG.Current.Viewport_GI_Bake,
                    summary.Linked, summary.Boxes));
            }

            // Slot 0 holds the default volume
            ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_GPU_Textures", Math.Max(0, SceneRenderer.GlobalVolumeTexturePool.AllocatedTextureCount - 1)));
            ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_Shaded_Volumes", view.IrradianceVolumeBank.ShadedVolumeCount, IrradianceVolumeTable.Capacity));
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Shaded_Volumes_TT"));
        }
    }

    /// <summary>
    /// The Model and Time Act Editors' irradiance volume: the settings every viewport shares, then the map it is
    /// borrowed from, which of its volumes, and the size of the box it is stretched over
    /// </summary>
    private void EnvironmentGlobalIlluminationTab(VulkanViewport viewport, ViewportEnvironment environment)
    {
        GlobalIlluminationSettings(viewport, environment.VolumeBank.BakesFollowTime);

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Source"), LOC.Get("VIEWPORT_Rendering_Section_Volume_Source_TT"));

        EnvironmentMapCombo(environment);

        var volumes = environment.MapId != null ? environment.VolumeBank.GetMap(environment.MapId) : null;

        var chosen = CFG.Current.Viewport_Environment_Volume;
        var autoLabel = LOC.Get("VIEWPORT_Rendering_Environment_Volume_Auto", environment.MedianVolumeId);

        DPI.ApplyInputWidth();
        if (ImGui.BeginCombo($"{LOC.Get("VIEWPORT_Rendering_Environment_Volume")}##environmentVolume",
            chosen >= 0 && volumes?.HasVolume(chosen) == true ? $"{chosen:D4}" : autoLabel, ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.Selectable($"{autoLabel}##environmentVolumeAuto", chosen < 0))
            {
                CFG.Current.Viewport_Environment_Volume = -1;
            }

            if (volumes != null)
            {
                // Each volume with the bakes it has and its average light, as the median is picked by it
                foreach (var group in volumes.Infos.GroupBy(i => i.Key.Id).OrderBy(g => g.Key))
                {
                    var light = group.Average(i => i.Value.ShLuminance.W);
                    var label = LOC.Get("VIEWPORT_Rendering_Environment_Volume_Entry", group.Key, group.Count(), light);

                    if (ImGui.Selectable($"{label}##environmentVolume_{group.Key}", chosen == group.Key))
                    {
                        CFG.Current.Viewport_Environment_Volume = group.Key;
                    }
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Environment_Volume_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_Environment_Volume_Size")}##environmentVolumeSize",
            ref CFG.Current.Viewport_Environment_Volume_Size, 1.0f, 500.0f, "%.0f m");
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Environment_Volume_Size_TT"));

        if (StatusHeader())
        {
            if (environment.MapId == null || volumes == null || environment.VolumeId < 0)
            {
                ImGui.Text(LOC.Get("VIEWPORT_Rendering_Environment_No_Volume"));
            }
            else if (environment.VolumeShaded)
            {
                ImGui.Text(LOC.Get("VIEWPORT_Rendering_Environment_Volume_Status",
                    environment.MapId, environment.VolumeId, environment.Bakes.Bake));
            }
            else
            {
                ImGui.Text(LOC.Get("VIEWPORT_Rendering_Environment_Volume_Loading",
                    environment.MapId, environment.VolumeId, environment.Bakes.Bake));
            }
        }
    }

    // The map list's filter
    private string EnvironmentMapFilter = "";

    /// <summary>
    /// The map the Model and Time Act Editors' viewports borrow their lighting and volume from
    /// </summary>
    private void EnvironmentMapCombo(ViewportEnvironment environment)
    {
        var defaultMap = ViewportEnvironment.GetDefaultMap(environment.Project.Descriptor.ProjectType);
        var defaultLabel = LOC.Get("VIEWPORT_Rendering_Environment_Map_Default", defaultMap ?? "-");
        var chosen = CFG.Current.Viewport_Environment_Map;

        DPI.ApplyInputWidth();
        if (ImGui.BeginCombo($"{LOC.Get("VIEWPORT_Rendering_Environment_Map")}##environmentMap",
            string.IsNullOrEmpty(chosen) ? defaultLabel : chosen, ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.IsWindowAppearing())
                ImGui.SetKeyboardFocusHere();

            ImGui.InputTextWithHint("##environmentMapFilter", LOC.Get("VIEWPORT_Menubar_Map_Lighting_File_Filter"),
                ref EnvironmentMapFilter, 255);

            if (ImGui.Selectable($"{defaultLabel}##environmentMapDefault", string.IsNullOrEmpty(chosen)))
            {
                CFG.Current.Viewport_Environment_Map = "";
            }

            foreach (var map in environment.ListMaps())
            {
                if (!string.IsNullOrEmpty(EnvironmentMapFilter) &&
                    !map.Contains(EnvironmentMapFilter, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (ImGui.Selectable($"{map}##environmentMap_{map}", map.Equals(chosen, StringComparison.OrdinalIgnoreCase)))
                {
                    CFG.Current.Viewport_Environment_Map = map;

                    // Another map's volumes have other IDs
                    CFG.Current.Viewport_Environment_Volume = -1;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Environment_Map_TT", environment.MapId ?? "-"));
    }

    /// <summary>
    /// The Global Illumination settings every editor's viewports share
    /// </summary>
    private static void GlobalIlluminationSettings(VulkanViewport viewport, bool bakesFollowTime)
    {
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Settings"), LOC.Get("VIEWPORT_Rendering_Section_GI_Settings_TT"));

        // These apply at once, as they are passed in the scene params every frame
        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_GI_Enable")}##giEnable", ref CFG.Current.Viewport_GI_Enable);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Enable_TT"));

        // With map lighting, the volumes take the GPARAM's exposure, as the sun does
        var exposureFromMap = viewport.ViewPipeline.ActiveMapLighting != null;

        ImGui.BeginDisabled(exposureFromMap);
        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_GI_Auto_Exposure")}##giAutoExposure", ref CFG.Current.Viewport_GI_Auto_Exposure);
        ImGui.EndDisabled();
        GUI.Tooltip(exposureFromMap
            ? LOC.Get("VIEWPORT_Menubar_GI_Auto_Exposure_Map_TT")
            : LOC.Get("VIEWPORT_Menubar_GI_Auto_Exposure_TT", viewport.ViewPipeline.ActiveVolumeTable.AutoExposure));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_GI_Intensity")}##giIntensity", ref CFG.Current.Viewport_GI_Intensity,
            0.0f, CFG.Current.Viewport_GI_Auto_Exposure || exposureFromMap ? 4.0f : 200.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Intensity_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_GI_Normalize_Reflections")}##giNormalizeReflections", ref CFG.Current.Viewport_GI_Normalize_Reflections);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Normalize_Reflections_TT"));

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Bakes"), LOC.Get("VIEWPORT_Rendering_Section_Bakes_TT"));

        // With the bakes following the map lighting's time of day, the bake settings below aren't used
        if (bakesFollowTime)
        {
            var (from, to, ratio) = GparamLighting.TimezonesAt(CFG.Current.Viewport_Map_Lighting_Hours);
            ImGui.TextDisabled(LOC.Get("VIEWPORT_Menubar_GI_Bakes_Follow_Time", from, to, ratio));
        }

        ImGui.BeginDisabled(bakesFollowTime);

        DPI.ApplyInputWidth();
        if (ImGui.InputInt($"{LOC.Get("VIEWPORT_Menubar_GI_Bake")}##giBake", ref CFG.Current.Viewport_GI_Bake))
        {
            CFG.Current.Viewport_GI_Bake = Math.Clamp(CFG.Current.Viewport_GI_Bake, 0, 99);
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Bake_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_GI_Bake_Blend")}##giBakeBlend", ref CFG.Current.Viewport_GI_Bake_Blend);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Bake_Blend_TT"));

        ImGui.BeginDisabled(!CFG.Current.Viewport_GI_Bake_Blend);
        ImGui.Indent();

        DPI.ApplyInputWidth(IndentedWidth);
        if (ImGui.InputInt($"{LOC.Get("VIEWPORT_Menubar_GI_Blend_Bake")}##giBlendBake", ref CFG.Current.Viewport_GI_Blend_Bake))
        {
            CFG.Current.Viewport_GI_Blend_Bake = Math.Clamp(CFG.Current.Viewport_GI_Blend_Bake, 0, 99);
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Blend_Bake_TT"));

        DPI.ApplyInputWidth(IndentedWidth);
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_GI_Blend_Ratio")}##giBlendRatio", ref CFG.Current.Viewport_GI_Blend_Ratio, 0.0f, 1.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Blend_Ratio_TT"));

        ImGui.Unindent();
        ImGui.EndDisabled();

        ImGui.EndDisabled();
    }

    private static readonly int[] ShadowResolutions = { 1024, 2048, 4096 };

    /// <summary>
    /// How the HDR scene is taken to the screen: the map GPARAM's Tone Map group under map lighting, or a fixed curve.
    /// Applies at once, as the tone map is drawn every frame. The curve in use is in the Post Effects tab's status.
    /// </summary>
    private static void ToneMapSection()
    {
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Tab_Tone_Map"), LOC.Get("VIEWPORT_Rendering_Section_Tone_Map_TT"));

        string[] curves =
        {
            LOC.Get("VIEWPORT_Rendering_Tone_Map_Gparam"), LOC.Get("VIEWPORT_Rendering_Tone_Map_Clamp"),
            LOC.Get("VIEWPORT_Rendering_Tone_Map_Reinhard"), LOC.Get("VIEWPORT_Rendering_Tone_Map_Filmic")
        };

        DPI.ApplyInputWidth();
        ImGui.Combo($"{LOC.Get("VIEWPORT_Rendering_Tone_Map")}##toneMap", ref CFG.Current.Viewport_ToneMap, curves, curves.Length);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Tone_Map_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_Tone_Map_Exposure")}##toneMapExposure",
            ref CFG.Current.Viewport_ToneMap_Exposure, 0.0f, 4.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Tone_Map_Exposure_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_Tone_Map_White_Point")}##toneMapWhitePoint",
            ref CFG.Current.Viewport_ToneMap_White_Point, 0.5f, 8.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Tone_Map_White_Point_TT"));
    }

    /// <summary>
    /// The sun's cascaded shadows, whose splits, fade and bias come from the map GPARAM's Shadows group under map
    /// lighting. Applies at once, as the cascades are placed every frame.
    /// </summary>
    private void ShadowsTab(VulkanViewport viewport)
    {
        var pipeline = viewport.ViewPipeline;

        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Shadow_Map"), LOC.Get("VIEWPORT_Rendering_Section_Shadow_Map_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Shadows")}##shadows", ref CFG.Current.Viewport_Shadows);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Shadows_TT"));

        ImGui.BeginDisabled(!CFG.Current.Viewport_Shadows);

        var resolution = Array.IndexOf(ShadowResolutions, CFG.Current.Viewport_Shadow_Resolution);
        string[] resolutionNames = ShadowResolutions.Select(r => r.ToString()).ToArray();

        DPI.ApplyInputWidth();
        if (ImGui.Combo($"{LOC.Get("VIEWPORT_Rendering_Shadow_Resolution")}##shadowResolution", ref resolution,
                resolutionNames, resolutionNames.Length))
        {
            CFG.Current.Viewport_Shadow_Resolution = ShadowResolutions[Math.Clamp(resolution, 0, ShadowResolutions.Length - 1)];
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Shadow_Resolution_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_Shadow_Strength")}##shadowStrength",
            ref CFG.Current.Viewport_Shadow_Strength, 0.0f, 1.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Shadow_Strength_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_Shadow_Bias_Scale")}##shadowBiasScale",
            ref CFG.Current.Viewport_Shadow_Bias_Scale, 0.0f, 8.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Shadow_Bias_Scale_TT"));

        // Read from the parts when their render models are updated, so it takes a reload to apply to all of them
        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Shadows_Msb_Flags")}##shadowsMsbFlags",
            ref CFG.Current.Viewport_Shadows_Use_Msb_Flags);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Shadows_Msb_Flags_TT"));

        ImGui.EndDisabled();

        if (StatusHeader())
        {
            var shadows = pipeline?.Shadows;

            if (shadows != null && shadows.ActiveCount > 0)
            {
                var splits = shadows.Parameters.Splits;
                var ends = new[] { splits.X, splits.Y, splits.Z, splits.W }.Take(shadows.ActiveCount)
                    .Select(s => s.ToString("0.#"));
                ImGui.Text(LOC.Get("VIEWPORT_Rendering_Shadow_Active", shadows.ActiveCount, string.Join(", ", ends)));
            }
            else
            {
                ImGui.Text(LOC.Get("VIEWPORT_Rendering_Shadow_Inactive"));
            }
        }
    }

    /// <summary>
    /// The lighting the loaded maps' GPARAMs give (AC6, ER, NR), for the scene and for models, and where each value
    /// came from, and what the viewports are lit with, see Gparam Lighting.md
    /// </summary>
    private void MapLightingTab(VulkanViewport viewport, MapEditorView view)
    {
        var bank = view.GparamLightingBank;

        MapLightingSettings();

        var maps = view.Project.Handler.MapData.PrimaryBank.Maps.Values
            .Select(w => w.MapContainer)
            .Where(m => m != null && m.LoadState == MapContentLoadState.Loaded)
            .ToList();

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Source"), LOC.Get("VIEWPORT_Rendering_Section_Lighting_Source_TT"));

        // The files of every loaded map's area first, then any other of the project's, which lights every map
        var files = maps.SelectMany(m => bank.ListFiles(m.Name)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var chosen = CFG.Current.Viewport_Map_Lighting_File;

        LightingFileCombo(bank, files, chosen, file => CFG.Current.Viewport_Map_Lighting_File = file);

        // The file the selected collision's scene GPARAM picks, as the game does when the player stands on it
        var collisionFile = view.ViewportSelection.GetSelection()
            .OfType<Entity>()
            .Select(e => GparamLightingBank.GetSceneSubId(e.WrappedObject) is { } subId && e.Container is MapContainer map
                ? bank.GetSubIdFile(map.Name, subId)
                : null)
            .FirstOrDefault(f => f != null);

        ImGui.BeginDisabled(collisionFile == null);
        if (ImGui.Button($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_From_Collision")}##mapLightingFromCollision"))
        {
            CFG.Current.Viewport_Map_Lighting_File = collisionFile;
        }
        ImGui.EndDisabled();
        GUI.Tooltip(collisionFile != null
            ? LOC.Get("VIEWPORT_Menubar_Map_Lighting_From_Collision_TT", collisionFile)
            : LOC.Get("VIEWPORT_Menubar_Map_Lighting_From_Collision_None_TT"));

        WeatherCombo(bank, maps.SelectMany(m => bank.ListWeathers(m.Name)).Distinct().OrderBy(w => w).ToList());

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Time_Of_Day"), LOC.Get("VIEWPORT_Rendering_Section_Time_Of_Day_TT"));

        TimeOfDaySettings(view.Project, view.IrradianceVolumeBank.CanUse());

        LightingDebugSection();

        MapLightingStatus(viewport);

        if (ResolvedValuesHeader())
        {
            if (maps.Count == 0)
            {
                ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_No_Maps"));
            }

            foreach (var map in maps)
            {
                LightingTable(bank, map.Name, bank.ChooseFile(map.Name, chosen));
            }
        }
    }

    /// <summary>
    /// The Model and Time Act Editors' map lighting: the settings every viewport shares, then the map it is borrowed
    /// from, and the file, weather and time of day it is read at
    /// </summary>
    private void EnvironmentLightingTab(VulkanViewport viewport, ViewportEnvironment environment)
    {
        var bank = environment.LightingBank;

        MapLightingSettings();

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Source"), LOC.Get("VIEWPORT_Rendering_Section_Lighting_Source_TT"));

        EnvironmentMapCombo(environment);

        var mapId = environment.MapId;
        var chosen = CFG.Current.Viewport_Environment_Lighting_File;

        if (mapId == null)
        {
            ImGui.TextDisabled(LOC.Get("VIEWPORT_Rendering_Environment_No_Map"));
        }
        else
        {
            LightingFileCombo(bank, bank.ListFiles(mapId), chosen, file => CFG.Current.Viewport_Environment_Lighting_File = file);

            WeatherCombo(bank, bank.ListWeathers(mapId));
        }

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Time_Of_Day"), LOC.Get("VIEWPORT_Rendering_Section_Time_Of_Day_TT"));

        TimeOfDaySettings(environment.Project, environment.Table != null);

        LightingDebugSection();

        MapLightingStatus(viewport);

        if (mapId != null && ResolvedValuesHeader())
        {
            LightingTable(bank, mapId, bank.ChooseFile(mapId, chosen));
        }
    }

    /// <summary>
    /// The map lighting settings every editor's viewports share
    /// </summary>
    private static void MapLightingSettings()
    {
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Settings"), LOC.Get("VIEWPORT_Rendering_Section_Map_Lighting_Settings_TT"));

        // These apply at once, as the lighting is resolved every frame
        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Enable")}##mapLightingEnable", ref CFG.Current.Viewport_Map_Lighting);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Enable_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Exposure_Scale")}##mapLightingExposureScale",
            ref CFG.Current.Viewport_Map_Lighting_Exposure_Scale, 0.0f, 4.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Exposure_Scale_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Hemi_Inside_Volumes")}##mapLightingHemiInsideVolumes",
            ref CFG.Current.Viewport_Map_Lighting_Hemi_Inside_Volumes);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Hemi_Inside_Volumes_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Background")}##mapLightingBackground",
            ref CFG.Current.Viewport_Map_Lighting_Background);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Background_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Model_Lights")}##mapLightingModelLights",
            ref CFG.Current.Viewport_Map_Lighting_Model_Lights);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Model_Lights_TT", GparamLightingBank.ModelLightSetId));

        // DS3's lightmaps, applied when a map is next loaded (the materials pick their shader then)
        if (GparamProfile.Current.HasDolLightmaps)
        {
            ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Lightmaps")}##mapLightingLightmaps",
                ref CFG.Current.Viewport_Map_Lightmaps);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Lightmaps_TT"));

            ImGui.BeginDisabled(!CFG.Current.Viewport_Map_Lightmaps);
            ImGui.Indent();
            DPI.ApplyInputWidth(IndentedWidth);
            ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Lightmap_Intensity")}##mapLightingLightmapIntensity",
                ref CFG.Current.Viewport_Map_Lightmap_Intensity, 0.0f, 32.0f, "%.2f", ImGuiSliderFlags.Logarithmic);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Lightmap_Intensity_TT"));
            ImGui.Unindent();
            ImGui.EndDisabled();
        }
    }

    /// <summary>
    /// The lighting debug views, hidden unless asked for or one is on
    /// </summary>
    private void LightingDebugSection()
    {
        if (!DebugHeader(CFG.Current.Viewport_Lighting_Debug_View != 0))
            return;

        string[] debugViews =
        {
            LOC.Get("VIEWPORT_Menubar_Map_Lighting_Debug_Off"),
            LOC.Get("VIEWPORT_Menubar_Map_Lighting_Debug_Source"),
            LOC.Get("VIEWPORT_Menubar_Map_Lighting_Debug_Ambient"),
            LOC.Get("VIEWPORT_Menubar_Map_Lighting_Debug_Sun_Mask")
        };

        DPI.ApplyInputWidth();
        ImGui.Combo($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Debug_View")}##mapLightingDebugView",
            ref CFG.Current.Viewport_Lighting_Debug_View, debugViews, debugViews.Length);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Debug_View_TT"));
    }

    /// <summary>
    /// The GPARAM the viewport is lit with, and its exposure
    /// </summary>
    private void MapLightingStatus(VulkanViewport viewport)
    {
        if (!StatusHeader())
            return;

        var active = viewport.ViewPipeline.ActiveMapLighting;

        ImGui.Text(!CFG.Current.Viewport_Map_Lighting
            ? LOC.Get("VIEWPORT_Rendering_Map_Lighting_Disabled")
            : active != null
                ? LOC.Get("VIEWPORT_Menubar_Map_Lighting_Active", active.File, active.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale)
                : LOC.Get("VIEWPORT_Menubar_Map_Lighting_None"));
    }

    /// <summary>
    /// The GPARAM to look the lighting up in first: the map's own, one of <paramref name="files" />, or any other
    /// of the project's
    /// </summary>
    private void LightingFileCombo(GparamLightingBank bank, List<string> files, string chosen, Action<string> choose)
    {
        var ownLabel = LOC.Get("VIEWPORT_Menubar_Map_Lighting_File_Own");

        DPI.ApplyInputWidth();
        if (ImGui.BeginCombo($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_File")}##mapLightingFile",
            string.IsNullOrEmpty(chosen) ? ownLabel : chosen, ImGuiComboFlags.HeightLarge))
        {
            if (ImGui.IsWindowAppearing())
                ImGui.SetKeyboardFocusHere();

            ImGui.InputTextWithHint("##mapLightingFileFilter", LOC.Get("VIEWPORT_Menubar_Map_Lighting_File_Filter"),
                ref GparamFilter, 255);

            bool Matches(string file) => string.IsNullOrEmpty(GparamFilter) ||
                file.Contains(GparamFilter, StringComparison.OrdinalIgnoreCase);

            void FileSelectable(string file)
            {
                if (ImGui.Selectable($"{file}##mapLightingFile_{file}", file.Equals(chosen, StringComparison.OrdinalIgnoreCase)))
                {
                    choose(file);
                }
            }

            if (ImGui.Selectable($"{ownLabel}##mapLightingFileOwn", string.IsNullOrEmpty(chosen)))
            {
                choose("");
            }

            foreach (var file in files.Where(Matches))
            {
                FileSelectable(file);
            }

            var others = bank.ListAllFiles()
                .Where(f => !files.Contains(f, StringComparer.OrdinalIgnoreCase) && Matches(f))
                .ToList();

            if (others.Count > 0)
            {
                ImGui.Separator();
                ImGui.TextDisabled(LOC.Get("VIEWPORT_Menubar_Map_Lighting_File_Other_Areas"));

                foreach (var file in others)
                {
                    FileSelectable(file);
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_File_TT"));
    }

    /// <summary>
    /// The weathers: the WW of the area's mAA_BB_CCWW files, named by the WeatherParam rows that use them
    /// </summary>
    private static void WeatherCombo(GparamLightingBank bank, List<int> weathers)
    {
        if (weathers.Count > 0)
        {
            string WeatherLabel(int weather) => weather <= 0
                ? LOC.Get("VIEWPORT_Menubar_Map_Lighting_Weather_None")
                : bank.GetWeatherName(weather) is { } name ? $"{weather:D2}: {name}" : $"{weather:D2}";

            DPI.ApplyInputWidth();
            if (ImGui.BeginCombo($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Weather")}##mapLightingWeather",
                WeatherLabel(CFG.Current.Viewport_Map_Lighting_Weather)))
            {
                foreach (var weather in new[] { 0 }.Concat(weathers))
                {
                    if (ImGui.Selectable($"{WeatherLabel(weather)}##mapLightingWeather_{weather}",
                        CFG.Current.Viewport_Map_Lighting_Weather == weather))
                    {
                        CFG.Current.Viewport_Map_Lighting_Weather = weather;
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Weather_TT"));

            ImGui.BeginDisabled(CFG.Current.Viewport_Map_Lighting_Weather <= 0);
            ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Weather_Outdoor")}##mapLightingWeatherOutdoor",
                ref CFG.Current.Viewport_Map_Lighting_Weather_Outdoor);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Weather_Outdoor_TT"));
            ImGui.EndDisabled();
        }
    }

    /// <summary>
    /// The time of day the GPARAMs are read at, and in Elden Ring whether the volumes' bakes follow it
    /// </summary>
    private static void TimeOfDaySettings(ProjectEntry project, bool hasVolumes)
    {
        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Hours")}##mapLightingHours",
            ref CFG.Current.Viewport_Map_Lighting_Hours, 0.0f, 24.0f, "%.2f");
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Hours_TT"));

        // The timezones' keys, as the game's cutscenes pick them. Sekiro keys its own hours (0, 2, 4, 6, 12, 18) and
        // DS3 and BB have one key, so they have the slider alone.
        var timezones = GparamProfile.For(project.Descriptor.ProjectType).HasTimezones ? GparamLighting.TimeKeys.Length : 0;

        for (var i = 0; i < timezones; i++)
        {
            if (i > 0)
                ImGui.SameLine();

            if (ImGui.SmallButton($"{LOC.Get(GparamLighting.TimezoneNames[i])}##mapLightingTimezone{i}"))
            {
                CFG.Current.Viewport_Map_Lighting_Hours = GparamLighting.TimeKeys[i];
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Timezone_TT", GparamLighting.TimeKeys[i], i));
        }

        if (project.Descriptor.ProjectType is ProjectType.ER && hasVolumes)
        {
            ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Follow_Bakes")}##mapLightingFollowBakes",
                ref CFG.Current.Viewport_Map_Lighting_Follow_Bakes);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Follow_Bakes_TT"));
        }
    }

    /// <summary>
    /// What a map's GPARAMs give, for the scene and for models, and where each value came from
    /// </summary>
    private static void LightingTable(GparamLightingBank bank, string mapId, string file)
    {
        var hours = CFG.Current.Viewport_Map_Lighting_Hours;
        var off = LOC.Get("VIEWPORT_Menubar_Map_Lighting_Off");

        string Light(GparamDirectionalLight light) => light.Enabled && light.Diffuse.W > 0.0f
            ? $"{GparamLightingBank.FormatColor(light.Diffuse)} {GparamLightingBank.FormatAngle(light.Angle)}{(light.FollowCamera ? " cam" : "")}"
            : off;

        var scene = bank.Resolve(mapId, file, GparamLightingBank.SceneLightSetId, hours);
        var model = bank.Resolve(mapId, file, GparamLightingBank.ModelLightSetId, hours);

        ImGui.Text(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Map", mapId, scene.File));

        if (!ImGui.BeginTable($"##mapLighting_{mapId}", 4,
            ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
            return;

        ImGui.TableSetupColumn(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Column_Value"));
        ImGui.TableSetupColumn(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Column_Scene", GparamLightingBank.SceneLightSetId));
        ImGui.TableSetupColumn(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Column_Models", GparamLightingBank.ModelLightSetId));
        ImGui.TableSetupColumn(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Column_Source"));
        ImGui.TableHeadersRow();

        void Row(string key, string sceneText, string modelText, string field)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(LOC.Get(key));
            ImGui.TableNextColumn();
            ImGui.Text(sceneText);
            ImGui.TableNextColumn();
            ImGui.Text(modelText);
            ImGui.TableNextColumn();
            ImGui.Text(scene.Sources.TryGetValue(field, out var source) ? source : LOC.Get("VIEWPORT_Menubar_Map_Lighting_Default"));
        }

        Row("VIEWPORT_Menubar_Map_Lighting_Row_Sun",
            GparamLightingBank.FormatColor(scene.Sun.Diffuse), GparamLightingBank.FormatColor(model.Sun.Diffuse),
            "Directional Light DiffColor0");
        Row("VIEWPORT_Menubar_Map_Lighting_Row_Sun_Specular",
            GparamLightingBank.FormatColor(scene.Sun.Specular), GparamLightingBank.FormatColor(model.Sun.Specular),
            "Directional Light SpecColor0");
        Row("VIEWPORT_Menubar_Map_Lighting_Row_Sun_Angle",
            GparamLightingBank.FormatAngle(scene.Sun.Angle), GparamLightingBank.FormatAngle(model.Sun.Angle),
            "Directional Light Angle0");
        Row("VIEWPORT_Menubar_Map_Lighting_Row_Light1", Light(scene.Light1), Light(model.Light1),
            "Directional Light DiffColor1");
        Row("VIEWPORT_Menubar_Map_Lighting_Row_LocalLight1", Light(scene.LocalLight1), Light(model.LocalLight1),
            "Directional Light LocalLight DiffColor1");
        Row("VIEWPORT_Menubar_Map_Lighting_Row_LocalLight2", Light(scene.LocalLight2), Light(model.LocalLight2),
            "Directional Light LocalLight DiffColor2");
        Row("VIEWPORT_Menubar_Map_Lighting_Row_Hemi_Up",
            GparamLightingBank.FormatColor(scene.HemiUp), GparamLightingBank.FormatColor(model.HemiUp),
            "Directional Light Hemi Color Up");
        Row("VIEWPORT_Menubar_Map_Lighting_Row_Hemi_Down",
            GparamLightingBank.FormatColor(scene.HemiDown), GparamLightingBank.FormatColor(model.HemiDown),
            "Directional Light Hemi Color Down");

        // The groups below have no model set, so both columns are the same
        var exposure = $"{scene.Exposure:0.###}";
        var autoExposure = scene.AutoExposure ? $"{scene.MiddleGray:0.###}" : off;
        var envMap = $"{scene.EnvMapSaturate:0.###}, {GparamLightingBank.FormatColor(scene.EnvMapScale)}";
        var background = GparamLightingBank.FormatColor(scene.BackgroundColor);

        Row("VIEWPORT_Menubar_Map_Lighting_Row_Exposure", exposure, exposure, bank.Profile.ExposureField);
        Row("VIEWPORT_Menubar_Map_Lighting_Row_Auto_Exposure", autoExposure, autoExposure,
            bank.Profile.AutoExposureField);
        Row("VIEWPORT_Menubar_Map_Lighting_Row_EnvMap", envMap, envMap, "EnvironmentConfig EnvMapColorConvert Saturate");
        Row("VIEWPORT_Menubar_Map_Lighting_Row_Background", background, background, "GS - BG Color-Data");

        ImGui.EndTable();
    }

    /// <summary>
    /// How the image is finished: the tone map, then the screen effects the maps' GPARAMs ask for
    /// (Documentation/Post Processing/Plan.md), which of them the viewport draws, and per map what its files give and
    /// where each value came from. Without a GPARAM bank only the tone map is offered.
    /// </summary>
    private void PostEffectsTab(VulkanViewport viewport, GparamLightingBank bank, List<(string MapId, string File)> maps)
    {
        ToneMapSection();

        if (bank != null)
        {
            PostEffectsSettings();
        }

        PostEffectsStatus(viewport, bank != null);

        if (bank != null && ResolvedValuesHeader())
        {
            if (maps.Count == 0)
            {
                ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_No_Maps"));
            }

            foreach (var (mapId, file) in maps)
            {
                PostEffectsTable(bank, mapId, file);
            }
        }
    }

    /// <summary>
    /// The post effect toggles, grouped by what they act on. These apply at once, as the chain runs every frame.
    /// </summary>
    private void PostEffectsSettings()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Tab_Post_Effects"), LOC.Get("VIEWPORT_Rendering_Section_Post_Effects_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Effects_Enable")}##postEffectsEnable", ref CFG.Current.Viewport_Post_Effects);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Effects_Enable_TT"));

        ImGui.BeginDisabled(!CFG.Current.Viewport_Post_Effects);

        // Image: exposure, bloom, glow and grading
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Image"), LOC.Get("VIEWPORT_Rendering_Section_Image_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Auto_Exposure_Enable")}##postAutoExposure",
            ref CFG.Current.Viewport_Post_Auto_Exposure);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Auto_Exposure_Enable_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Bloom_Enable")}##postBloom", ref CFG.Current.Viewport_Post_Bloom);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Bloom_Enable_TT"));

        IndentedSlider(CFG.Current.Viewport_Post_Bloom, "VIEWPORT_Rendering_Post_Bloom_Strength", "postBloomStrength",
            ref CFG.Current.Viewport_Post_Bloom_Strength, 0.0f, 2.0f);

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Glow_Enable")}##postGlow", ref CFG.Current.Viewport_Post_Glow);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Glow_Enable_TT"));

        IndentedSlider(CFG.Current.Viewport_Post_Glow, "VIEWPORT_Rendering_Post_Glow_Strength", "postGlowStrength",
            ref CFG.Current.Viewport_Post_Glow_Strength, 0.0f, 2.0f);

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Color_Grading_Enable")}##postColorGrading",
            ref CFG.Current.Viewport_Post_Color_Grading);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Color_Grading_Enable_TT"));

        // Anti-aliasing
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Anti_Aliasing"), LOC.Get("VIEWPORT_Rendering_Section_Anti_Aliasing_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Taa_Enable")}##postTaa", ref CFG.Current.Viewport_Post_Taa);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Taa_Enable_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Fxaa_Enable")}##postFxaa", ref CFG.Current.Viewport_Post_Fxaa);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Fxaa_Enable_TT"));

        // Atmosphere: fog and light shafts
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Atmosphere"), LOC.Get("VIEWPORT_Rendering_Section_Atmosphere_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Volumetric_Fog_Enable")}##postVolumetricFog",
            ref CFG.Current.Viewport_Post_Volumetric_Fog);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Volumetric_Fog_Enable_TT"));

        ImGui.BeginDisabled(!CFG.Current.Viewport_Post_Volumetric_Fog);
        ImGui.Indent();

        DPI.ApplyInputWidth(IndentedWidth);
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_Post_Fog_Density_Scale")}##postFogDensityScale",
            ref CFG.Current.Viewport_Post_Fog_Density_Scale, 0.0f, 2.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Fog_Density_Scale_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Fog_Shadows_Enable")}##postFogShadows",
            ref CFG.Current.Viewport_Post_Fog_Shadows);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Fog_Shadows_Enable_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Fog_Local_Lights_Enable")}##postFogLocalLights",
            ref CFG.Current.Viewport_Post_Fog_Local_Lights);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Fog_Local_Lights_Enable_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Fog_Transparents_Enable")}##postFogTransparents",
            ref CFG.Current.Viewport_Post_Fog_Transparents);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Fog_Transparents_Enable_TT"));

        ImGui.Unindent();
        ImGui.EndDisabled();

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Light_Shafts_Enable")}##postLightShafts",
            ref CFG.Current.Viewport_Post_Light_Shafts);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Light_Shafts_Enable_TT"));

        // Surfaces: wetness, occlusion and reflections
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Surfaces"), LOC.Get("VIEWPORT_Rendering_Section_Surfaces_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Wet_Enable")}##postWet", ref CFG.Current.Viewport_Post_Wet);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Wet_Enable_TT"));

        ImGui.BeginDisabled(!CFG.Current.Viewport_Post_Wet);
        ImGui.Indent();
        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Wet_Rain_Mask_Enable")}##postWetRainMask",
            ref CFG.Current.Viewport_Post_Wet_Rain_Mask);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Wet_Rain_Mask_Enable_TT"));
        ImGui.Unindent();
        ImGui.EndDisabled();

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Ssao_Enable")}##postSsao", ref CFG.Current.Viewport_Post_Ssao);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Ssao_Enable_TT"));

        IndentedSlider(CFG.Current.Viewport_Post_Ssao, "VIEWPORT_Rendering_Post_Ssao_Strength", "postSsaoStrength",
            ref CFG.Current.Viewport_Post_Ssao_Strength, 0.0f, 2.0f);

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Post_Ssr_Enable")}##postSsr", ref CFG.Current.Viewport_Post_Ssr);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Ssr_Enable_TT"));

        IndentedSlider(CFG.Current.Viewport_Post_Ssr, "VIEWPORT_Rendering_Post_Ssr_Strength", "postSsrStrength",
            ref CFG.Current.Viewport_Post_Ssr_Strength, 0.0f, 1.0f);

        // The surface views, hidden unless asked for or one is on
        if (DebugHeader(CFG.Current.Viewport_Post_Surface_View != 0))
        {
            string[] surfaceViews =
            {
                LOC.Get("VIEWPORT_Rendering_Post_Surface_View_None"),
                LOC.Get("VIEWPORT_Rendering_Post_Surface_View_Normals"),
                LOC.Get("VIEWPORT_Rendering_Post_Surface_View_Roughness"),
                LOC.Get("VIEWPORT_Rendering_Post_Surface_View_Specular"),
                LOC.Get("VIEWPORT_Rendering_Post_Surface_View_Indirect"),
                LOC.Get("VIEWPORT_Rendering_Post_Surface_View_Occlusion"),
                LOC.Get("VIEWPORT_Rendering_Post_Surface_View_Reflection"),
                LOC.Get("VIEWPORT_Rendering_Post_Surface_View_Screen_Reflections"),
                LOC.Get("VIEWPORT_Rendering_Post_Surface_View_Glow")
            };

            DPI.ApplyInputWidth();
            ImGui.Combo($"{LOC.Get("VIEWPORT_Rendering_Post_Surface_View")}##postSurfaceView",
                ref CFG.Current.Viewport_Post_Surface_View, surfaceViews, surfaceViews.Length);
            GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Surface_View_TT"));
        }

        ImGui.EndDisabled();
    }

    /// <summary>
    /// The tone map curve in use and, with post effects, the effects drawn last frame and their read-backs
    /// </summary>
    private void PostEffectsStatus(VulkanViewport viewport, bool hasPostEffects)
    {
        var pipeline = viewport.ViewPipeline;

        if (pipeline == null || !StatusHeader())
            return;

        var (op, whitePoint) = ToneMapPass.Resolve(pipeline.ActiveMapLighting);
        ImGui.Text(LOC.Get("VIEWPORT_Rendering_Tone_Map_Active", op, whitePoint));

        if (!hasPostEffects)
            return;

        // The fog is drawn with the scene, before the chain
        var active = new List<string>();

        if (pipeline.Fog.Drawn)
            active.Add(pipeline.Fog.Name);

        active.AddRange(pipeline.PostProcess.LastActive);

        if (pipeline.Fxaa.IsActive(pipeline.ActiveMapLighting))
            active.Add("FXAA");

        ImGui.Text(active.Count > 0
            ? LOC.Get("VIEWPORT_Rendering_Post_Effects_Active", string.Join(", ", active))
            : LOC.Get("VIEWPORT_Rendering_Post_Effects_None_Active"));

        // Read back from the GPU, a few frames behind
        if (pipeline.AutoExposure.Applied)
        {
            var state = pipeline.AutoExposure.LastState;
            ImGui.Text(LOC.Get("VIEWPORT_Rendering_Post_Auto_Exposure_Status", $"{state.MeanLog:0.##}",
                $"{state.Ev:+0.00;-0.00}", $"{state.TargetEv:+0.00;-0.00}"));
            GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Auto_Exposure_Status_TT"));
        }

        if (pipeline.Rain is { Active: true, HasContent: true } rain)
        {
            ImGui.Text(LOC.Get("VIEWPORT_Rendering_Post_Wet_Rain_Mask_Status", $"{rain.Center.X:0}",
                $"{rain.Center.Z:0}", rain.DrawCount));
            GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Post_Wet_Rain_Mask_Status_TT"));
        }
    }

    /// <summary>
    /// A strength slider under its effect's toggle, disabled while the effect is off
    /// </summary>
    private static void IndentedSlider(bool enabled, string key, string id, ref float value, float min, float max)
    {
        ImGui.BeginDisabled(!enabled);
        ImGui.Indent();
        DPI.ApplyInputWidth(IndentedWidth);
        ImGui.SliderFloat($"{LOC.Get(key)}##{id}", ref value, min, max);
        GUI.Tooltip(LOC.Get($"{key}_TT"));
        ImGui.Unindent();
        ImGui.EndDisabled();
    }

    /// <summary>
    /// What a map's GPARAMs give for each screen effect, at ID 0 and the menu's time of day
    /// </summary>
    private static void PostEffectsTable(GparamLightingBank bank, string mapId, string file)
    {
        var lighting = bank.Resolve(mapId, file, GparamLightingBank.SceneLightSetId, CFG.Current.Viewport_Map_Lighting_Hours);
        var effects = lighting.PostEffects;
        var off = LOC.Get("VIEWPORT_Menubar_Map_Lighting_Off");

        ImGui.Text(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Map", mapId, lighting.File));

        if (!ImGui.BeginTable($"##postEffects_{mapId}", 3,
            ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
            return;

        ImGui.TableSetupColumn(LOC.Get("VIEWPORT_Rendering_Post_Effects_Column_Effect"));
        ImGui.TableSetupColumn(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Column_Value"));
        ImGui.TableSetupColumn(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Column_Source"));
        ImGui.TableHeadersRow();

        void Row(string key, string value, string field)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.Text(LOC.Get(key));
            ImGui.TableNextColumn();
            ImGui.Text(value);
            ImGui.TableNextColumn();
            ImGui.Text(lighting.Sources.TryGetValue(field, out var source) ? source : LOC.Get("VIEWPORT_Menubar_Map_Lighting_Default"));
        }

        string Color(Vector4 c) => GparamLightingBank.FormatColor(c);

        var exposure = effects.AutoExposure;
        Row("VIEWPORT_Rendering_Post_Row_Auto_Exposure",
            lighting.AutoExposure
                ? LOC.Get("VIEWPORT_Rendering_Post_Auto_Exposure_Value", $"{lighting.MiddleGray:0.###}",
                    $"{exposure.AdaptionMin:0.###}", $"{exposure.AdaptionMax:0.###}",
                    $"{exposure.LightAdaptSpeed:0.###}", $"{exposure.DarkAdaptSpeed:0.###}")
                : off,
            bank.Profile.AutoExposureField);

        var bloom = effects.Bloom;
        Row("VIEWPORT_Rendering_Post_Row_Bloom",
            bloom.Enabled
                ? LOC.Get("VIEWPORT_Rendering_Post_Bloom_Value", bloom.NumLevels, $"{bloom.Threshold:0.###}",
                    $"{bloom.Luminance:0.###}", $"{bloom.Alpha:0.###}")
                : off,
            "Gs Presents【Bloom】 Enable");

        var glow = effects.Glow;
        Row("VIEWPORT_Rendering_Post_Row_Glow",
            glow.Enabled
                ? LOC.Get("VIEWPORT_Rendering_Post_Glow_Value", glow.NumLevels, $"{glow.FinalColorScale:0.###}",
                    $"{glow.BlurRgbScale:0.###}", $"{glow.BlurAlphaScale:0.###}")
                : off,
            "Glow Filter-Glow Enable");

        var shaft = effects.LightShaft;
        var shaftFrom = shaft.EnableRefLight
            ? LOC.Get("VIEWPORT_Rendering_Post_Light_Shaft_From_Light", shaft.LightBankId, shaft.LightId)
            : GparamLightingBank.FormatAngle(shaft.Angle);
        Row("VIEWPORT_Rendering_Post_Row_Light_Shaft",
            shaft.Enabled
                ? LOC.Get("VIEWPORT_Rendering_Post_Light_Shaft_Value", shaftFrom, $"{shaft.SkyMaskSizeAngle:0.###}",
                    $"{shaft.BlurRange:0.###}", shaft.BlurCount)
                : off,
            "GS Presents - LightShaft Enable");

        var fog = effects.VolumetricFog;
        Row("VIEWPORT_Rendering_Post_Row_Volumetric_Fog",
            fog.Enabled
                ? LOC.Get("VIEWPORT_Rendering_Post_Volumetric_Fog_Value", fog.VolumeWidth, fog.VolumeDepth,
                    $"{fog.NearDepth:0.###}", $"{fog.FarDepth:0.###}")
                : off,
            "VolumetricFog Enable");
        Row("VIEWPORT_Rendering_Post_Row_Height_Fog",
            LOC.Get("VIEWPORT_Rendering_Post_Height_Fog_Value", $"{fog.FixedDensity:G3}", $"{fog.HeightFogDensityScale:G3}",
                $"{fog.HeightFogDistribution:G3}", $"{fog.HeightFogOffset:0.###}", $"{fog.ScatteringG:0.###}",
                $"{fog.GameScale:0.###}"),
            "VolumetricFog Global HeightFogDensity Scale");
        Row("VIEWPORT_Rendering_Post_Row_Fog_Sky", Color(fog.SkyColor), "VolumetricFog SkyLight SkyColor");
        Row("VIEWPORT_Rendering_Post_Row_Fog_Sun",
            LOC.Get("VIEWPORT_Rendering_Post_Fog_Sun_Value", Color(fog.DirectColor), Color(fog.OppositeColor)),
            "VolumetricFog DirLight DirectColor");

        var ssao = effects.Ssao;
        Row("VIEWPORT_Rendering_Post_Row_Ssao",
            LOC.Get("VIEWPORT_Rendering_Post_Ssao_Value", $"{ssao.Scale:0.###}", $"{ssao.Offset:0.###}",
                $"{ssao.KernelMin:0.###}", $"{ssao.KernelMax:0.###}", $"{ssao.DepthFadeStart:0.###}"),
            "SSAO - Edit-Ver2 AO Scale");
        var weights = ssao.LightWeights;
        Row("VIEWPORT_Rendering_Post_Row_Ssao_Weights",
            $"({weights.X:0.###}, {weights.Y:0.###}, {weights.Z:0.###}, {weights.W:0.###})",
            "Directional Light SsaoWeight0");

        var ssr = effects.Ssr;
        Row("VIEWPORT_Rendering_Post_Row_Ssr",
            ssr.Enabled
                ? LOC.Get("VIEWPORT_Rendering_Post_Ssr_Value", $"{ssr.ShininessRejectThreshold:0.###}",
                    $"{ssr.FresnelRejectThreshold:0.###}", $"{ssr.FadeToViewerStart:0.###}")
                : off,
            "SSR Enable");

        var aa = effects.AntiAlias;
        Row("VIEWPORT_Rendering_Post_Row_Anti_Alias",
            aa.Enabled
                ? LOC.Get("VIEWPORT_Rendering_Post_Anti_Alias_Value", aa.Type, $"{aa.FxaaSpanMax:0.###}", aa.JitterCount,
                    $"{aa.FeedbackWeight:0.###}")
                : off,
            "AntiAlias - AntiAliasType");

        // Each ID with the texture it was found as, then the blend from the first towards the second
        var grading = effects.ColorGrading;

        string LutName(int id, int slot) => slot >= 0 && grading.Luts != null
            ? $"{id} ({grading.Luts.Entries[slot].Owner}_CGrading{id:D4})"
            : LOC.Get("VIEWPORT_Rendering_Post_Color_Grading_Missing", id);

        Row("VIEWPORT_Rendering_Post_Row_Color_Grading",
            grading.Enabled && grading.LutSourceId >= 0
                ? grading.NextLutSourceId != grading.LutSourceId
                    ? LOC.Get("VIEWPORT_Rendering_Post_Color_Grading_Blend", LutName(grading.LutSourceId, grading.Slot),
                        LutName(grading.NextLutSourceId, grading.NextSlot), $"{grading.Blend:0.##}")
                    : LOC.Get("VIEWPORT_Rendering_Post_Color_Grading_Value", LutName(grading.LutSourceId, grading.Slot))
                : off,
            "Yebis-LutSourceId");

        var wet = effects.Wet;
        Row("VIEWPORT_Rendering_Post_Row_Wet",
            wet.AlbedoRate > 0.0f || wet.ShininessRate > 0.0f
                ? LOC.Get("VIEWPORT_Rendering_Post_Wet_Value", $"{wet.AlbedoRate:0.###}", Color(wet.OverrideAlbedo),
                    $"{wet.ShininessRate:0.###}", $"{wet.OverrideShininess:0.###}", $"{wet.Wetness:0.##}")
                : off,
            "Wet-AlbedoRate");

        ImGui.EndTable();
    }

    /// <summary>
    /// What the flver shaders do, and how the parts are drawn. These apply at once, as the shaders read them every
    /// frame.
    /// </summary>
    private void MaterialsTab()
    {
        // Editor mode switches the material features off, all but layer blending, and pauses the animation, so the
        // other toggles and the wind's settings can't change anything there. The grass is geometry, so its density
        // still applies.
        var gameMode = RenderMode.IsGame;

        if (!gameMode)
        {
            ImGui.TextDisabled(LOC.Get("VIEWPORT_Rendering_Materials_Game_Mode_Only"));
        }

        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Animation"), LOC.Get("VIEWPORT_Rendering_Section_Animation_TT"));

        ImGui.BeginDisabled(!gameMode);

        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Animation", ref CFG.Current.Viewport_Enable_Material_Animation);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Wind", ref CFG.Current.Viewport_Material_Wind);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Vertex_Animation", ref CFG.Current.Viewport_Material_Vertex_Animation);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Sway", ref CFG.Current.Viewport_Material_Sway);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Birds", ref CFG.Current.Viewport_Material_Birds);

        ImGui.EndDisabled();

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Surface_Features"), LOC.Get("VIEWPORT_Rendering_Section_Surface_Features_TT"));

        // Layer blending applies in Editor mode too, so it stays usable there
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Layer_Blending", ref CFG.Current.Viewport_Material_Layer_Blending);

        ImGui.BeginDisabled(!gameMode);

        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Overlays", ref CFG.Current.Viewport_Material_Overlays);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Magma", ref CFG.Current.Viewport_Material_Magma);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Emissive", ref CFG.Current.Viewport_Material_Emissive);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_UV_Scroll", ref CFG.Current.Viewport_Material_UV_Scroll);

        ImGui.EndDisabled();

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Shader_Types"), LOC.Get("VIEWPORT_Rendering_Section_Shader_Types_TT"));

        // Shader types apply in Editor mode too, so they stay usable there
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Unlit", ref CFG.Current.Viewport_Material_Unlit);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Water", ref CFG.Current.Viewport_Material_Water);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Fur", ref CFG.Current.Viewport_Material_Fur);

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Wind"), LOC.Get("VIEWPORT_Rendering_Section_Wind_TT"));

        ImGui.BeginDisabled(!gameMode);

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Setting_Wind_Strength")}##windStrength", ref CFG.Current.Viewport_Wind_Strength, 0.0f, 5.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Wind_Strength_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Setting_Wind_Direction")}##windDirection", ref CFG.Current.Viewport_Wind_Direction, 0.0f, 360.0f, "%.0f deg");
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Wind_Direction_TT"));

        ImGui.EndDisabled();

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("VIEWPORT_Rendering_Section_Display"), LOC.Get("VIEWPORT_Rendering_Section_Display_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_Grass_Density")}##grassDensity", ref GrassDensity, 0.1f, 4f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Grass_Density_TT"));

        // Each change rebuilds the grass, so it waits for the slider to be let go
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            CFG.Current.MapEditor_Grass_Density = GrassDensity;
        }

        // Applies at once, as the collision shaders read it every frame
        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("PREF_Viewport_Collision_Opacity")}##collisionOpacity",
            ref CFG.Current.Viewport_Collision_Opacity, 0.0f, 1.0f);
        GUI.Tooltip(LOC.Get("PREF_Viewport_Collision_Opacity_TT"));
    }

    private static void MaterialToggle(string key, ref bool value)
    {
        ImGui.Checkbox($"{LOC.Get(key)}##{key}", ref value);
        GUI.Tooltip(LOC.Get($"{key}_TT"));
    }
}
