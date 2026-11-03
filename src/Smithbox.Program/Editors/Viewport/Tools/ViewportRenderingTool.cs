using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// The tool window section that configures how an editor's viewport renders: its materials, lighting and, in the
/// Map Editor, the environment map, irradiance volumes and the map's GPARAM lighting. Each is a tab. The values
/// apply to the editor's current viewport, or to every viewport for those kept in the settings.
/// </summary>
public class ViewportRenderingTool
{
    private readonly Func<VulkanViewport> GetViewport;

    private float GrassDensity = CFG.Current.MapEditor_Grass_Density;

    // The Map Lighting tab's GPARAM list filter
    private string GparamFilter = "";

    public ViewportRenderingTool(Func<VulkanViewport> getViewport)
    {
        GetViewport = getViewport;
    }

    public void OnToolWindow()
    {
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

    private void DisplayTabs(VulkanViewport viewport)
    {
        ImGui.BeginTabBar("viewportRenderingTabs");

        if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Menubar_Header_Materials")}##viewportRenderingMaterials"))
        {
            MaterialsTab();

            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Menubar_Header_Scene_Lighting")}##viewportRenderingSceneLighting"))
        {
            SceneLightingTab(viewport);

            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Rendering_Tab_Tone_Map")}##viewportRenderingToneMap"))
        {
            ToneMapTab(viewport);

            ImGui.EndTabItem();
        }

        if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Rendering_Tab_Shadows")}##viewportRenderingShadows"))
        {
            ShadowsTab(viewport);

            ImGui.EndTabItem();
        }

        // Map Editor
        if (viewport.Owner is MapUniverse mapUniverse)
        {
            if (mapUniverse.View.IrradianceVolumeBank.CanUse())
            {
                if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Menubar_Header_Global_Illumination")}##viewportRenderingGlobalIllumination"))
                {
                    GlobalIlluminationTab(viewport, mapUniverse.View);

                    ImGui.EndTabItem();
                }
            }

            if (mapUniverse.View.GparamLightingBank.CanUse())
            {
                if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Menubar_Header_Map_Lighting")}##viewportRenderingMapLighting"))
                {
                    MapLightingTab(mapUniverse.View);

                    ImGui.EndTabItem();
                }
            }

            if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Rendering_Tab_BTL_Lights")}##viewportRenderingBtlLights"))
            {
                BtlLightsTab(mapUniverse.View);

                ImGui.EndTabItem();
            }
        }

        if (ImGui.BeginTabItem($"{LOC.Get("VIEWPORT_Menubar_Header_Parts")}##viewportRenderingParts"))
        {
            // Applies at once, as the collision shaders read it every frame
            DPI.ApplyInputWidth();
            ImGui.SliderFloat($"{LOC.Get("PREF_Viewport_Collision_Opacity")}##collisionOpacity",
                ref CFG.Current.Viewport_Collision_Opacity, 0.0f, 1.0f);
            GUI.Tooltip(LOC.Get("PREF_Viewport_Collision_Opacity_TT"));

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
        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_BTL_Near_Camera")}##btlNearCamera",
            ref CFG.Current.Viewport_BTL_Lights_Near_Camera);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_BTL_Near_Camera_TT"));

        ImGui.BeginDisabled(!CFG.Current.Viewport_BTL_Lights_Near_Camera);

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Rendering_BTL_Radius")}##btlRadius",
            ref CFG.Current.Viewport_BTL_Light_Radius, 1.0f, 1000.0f, "%.0f m");
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_BTL_Radius_TT"));

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_BTL_Enable_Distance")}##btlEnableDistance",
            ref CFG.Current.Viewport_BTL_Light_Use_Enable_Distance);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_BTL_Enable_Distance_TT"));

        ImGui.EndDisabled();

        DPI.ApplyInputWidth();
        ImGui.SliderInt($"{LOC.Get("VIEWPORT_Rendering_BTL_Max_Count")}##btlMaxCount",
            ref CFG.Current.Viewport_BTL_Light_Max_Count, 0, SceneLightTable.Capacity);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_BTL_Max_Count_TT"));

        // With map lighting, the lights take the GPARAM's exposure, as the sun does
        var exposureFromMap = SceneRenderer.MapLighting != null;

        ImGui.BeginDisabled(exposureFromMap);
        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("PREF_Viewport_Light_Intensity_Scale")}##btlIntensityScale",
            ref CFG.Current.Viewport_Light_Intensity_Scale, 0.1f, 50.0f);
        ImGui.EndDisabled();
        GUI.Tooltip(exposureFromMap
            ? LOC.Get("VIEWPORT_Rendering_BTL_Intensity_Map_TT",
                SceneRenderer.MapLighting.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale)
            : LOC.Get("PREF_Viewport_Light_Intensity_Scale_TT"));

        ImGui.Separator();

        var collector = view.ViewportWindow.LightCollector;
        ImGui.Text(LOC.Get("VIEWPORT_Rendering_BTL_Status", collector.ShadedCount, collector.InRangeCount));
    }

    /// <summary>
    /// The sun, the light multipliers and the camera light of the viewport
    /// </summary>
    private void SceneLightingTab(VulkanViewport viewport)
    {
        // The map's GPARAMs give the sun instead, see the Map Lighting tab
        var sunFromMap = viewport.ViewPipeline.ActiveMapLighting != null;

        if (sunFromMap)
        {
            ImGui.Text(LOC.Get("VIEWPORT_Menubar_Scene_Sun_From_Map"));
        }

        ImGui.BeginDisabled(sunFromMap);
        ImGui.SliderFloat4(
            $"{LOC.Get("VIEWPORT_Menubar_Scene_Light_Direction")}##lightDirection",
            ref viewport.ViewPipeline.SceneParams.LightDirection, -1, 1);
        ImGui.EndDisabled();

        ImGui.SliderFloat(
            $"{LOC.Get("VIEWPORT_Menubar_Scene_Direct_Light_Mult")}##directLightMult",
            ref viewport.ViewPipeline.SceneParams.DirectLightMult, 0, 3);

        ImGui.SliderFloat(
            $"{LOC.Get("VIEWPORT_Menubar_Scene_Indirect_Light_Mult")}##indirectLightMult",
            ref viewport.ViewPipeline.SceneParams.IndirectLightMult, 0, 3);

        ImGui.SliderFloat(
            $"{LOC.Get("VIEWPORT_Menubar_Scene_Light_Brightness")}##brightness",
            ref viewport.ViewPipeline.SceneParams.SceneBrightness, 0, 5);

        ImGui.Separator();

        // The sun is the light the direction above belongs to
        ImGui.BeginDisabled(sunFromMap);

        var sunColor = new Vector3(
            viewport.ViewPipeline.SceneParams.SunColor.X,
            viewport.ViewPipeline.SceneParams.SunColor.Y,
            viewport.ViewPipeline.SceneParams.SunColor.Z);

        if (ImGui.ColorEdit3("Sun Color##sunColor", ref sunColor))
        {
            viewport.ViewPipeline.SceneParams.SunColor.X = sunColor.X;
            viewport.ViewPipeline.SceneParams.SunColor.Y = sunColor.Y;
            viewport.ViewPipeline.SceneParams.SunColor.Z = sunColor.Z;
        }

        ImGui.SliderFloat("Sun Intensity##sunIntensity", ref viewport.ViewPipeline.SceneParams.SunColor.W, 0, 5);

        ImGui.EndDisabled();

        // A light that follows the camera, for lighting up dark interiors
        SceneLight cameraLight = viewport.ViewPipeline.CameraLight;

        var cameraLightEnabled = cameraLight.Enabled;
        if (ImGui.Checkbox("Camera Light##cameraLightEnabled", ref cameraLightEnabled))
        {
            cameraLight.Enabled = cameraLightEnabled;
        }

        if (cameraLight.Enabled)
        {
            var color = cameraLight.Color;
            if (ImGui.ColorEdit3("Camera Light Color##cameraLightColor", ref color))
            {
                cameraLight.Color = color;
            }

            var intensity = cameraLight.Intensity;
            if (ImGui.SliderFloat("Camera Light Intensity##cameraLightIntensity", ref intensity, 0, 50))
            {
                cameraLight.Intensity = intensity;
            }

            var range = cameraLight.Range;
            if (ImGui.SliderFloat("Camera Light Range##cameraLightRange", ref range, 1, 100))
            {
                cameraLight.Range = range;
            }
        }
    }

    /// <summary>
    /// The baked irradiance volumes of the loaded maps (AC6, ER, NR): the bake to use, and how many of each map's
    /// EnvMapEffectBoxes are linked to a volume
    /// </summary>
    private void GlobalIlluminationTab(VulkanViewport viewport, MapEditorView view)
    {
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
            : LOC.Get("VIEWPORT_Menubar_GI_Auto_Exposure_TT", SceneRenderer.IrradianceVolumes.AutoExposure));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_GI_Intensity")}##giIntensity", ref CFG.Current.Viewport_GI_Intensity,
            0.0f, CFG.Current.Viewport_GI_Auto_Exposure || exposureFromMap ? 4.0f : 200.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Intensity_TT"));

        // With the bakes following the map lighting's time of day, the bake settings below aren't used
        var bakesFollowTime = view.IrradianceVolumeBank.BakesFollowTime;

        if (bakesFollowTime)
        {
            var (from, to, ratio) = GparamLighting.TimezonesAt(CFG.Current.Viewport_Map_Lighting_Hours);
            ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_Bakes_Follow_Time", from, to, ratio));
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

        if (CFG.Current.Viewport_GI_Bake_Blend)
        {
            DPI.ApplyInputWidth();
            if (ImGui.InputInt($"{LOC.Get("VIEWPORT_Menubar_GI_Blend_Bake")}##giBlendBake", ref CFG.Current.Viewport_GI_Blend_Bake))
            {
                CFG.Current.Viewport_GI_Blend_Bake = Math.Clamp(CFG.Current.Viewport_GI_Blend_Bake, 0, 99);
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Blend_Bake_TT"));

            DPI.ApplyInputWidth();
            ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_GI_Blend_Ratio")}##giBlendRatio", ref CFG.Current.Viewport_GI_Blend_Ratio, 0.0f, 1.0f);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Blend_Ratio_TT"));
        }

        ImGui.EndDisabled();

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_GI_Normalize_Reflections")}##giNormalizeReflections", ref CFG.Current.Viewport_GI_Normalize_Reflections);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Normalize_Reflections_TT"));

        ImGui.Separator();

        var unlinked = 0;
        var skipped = 0;
        var anyMap = false;

        foreach (var wrapper in view.Project.Handler.MapData.PrimaryBank.Maps.Values)
        {
            var map = wrapper.MapContainer;

            if (map == null || map.LoadState != MapContentLoadState.Loaded)
                continue;

            anyMap = true;

            var summary = view.IrradianceVolumeBank.Summarise(map);
            var volumes = view.IrradianceVolumeBank.GetMap(map.Name);

            ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_Map_Status",
                map.Name, volumes?.CountInBake(CFG.Current.Viewport_GI_Bake) ?? 0, CFG.Current.Viewport_GI_Bake,
                summary.Linked, summary.Boxes));

            unlinked += summary.Unlinked;
            skipped += summary.SkippedByGame;
        }

        if (!anyMap)
        {
            ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_No_Maps"));
        }

        // Slot 0 holds the default volume
        ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_GPU_Textures", Math.Max(0, SceneRenderer.GlobalVolumeTexturePool.AllocatedTextureCount - 1)));
        ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_Shaded_Volumes", view.IrradianceVolumeBank.ShadedVolumeCount, IrradianceVolumeTable.Capacity));
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_GI_Shaded_Volumes_TT"));

        ImGui.Separator();

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
    }

    private static readonly int[] ShadowResolutions = { 1024, 2048, 4096 };

    /// <summary>
    /// How the HDR scene is taken to the screen: the map GPARAM's Tone Map group under map lighting, or a fixed curve.
    /// Applies at once, as the tone map is drawn every frame.
    /// </summary>
    private void ToneMapTab(VulkanViewport viewport)
    {
        var pipeline = viewport.ViewPipeline;

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

        if (pipeline != null)
        {
            var (op, whitePoint) = ToneMapPass.Resolve(pipeline.ActiveMapLighting);
            ImGui.Text(LOC.Get("VIEWPORT_Rendering_Tone_Map_Active", op, whitePoint));
        }
    }

    /// <summary>
    /// The sun's cascaded shadows, whose splits, fade and bias come from the map GPARAM's Shadows group under map
    /// lighting. Applies at once, as the cascades are placed every frame.
    /// </summary>
    private void ShadowsTab(VulkanViewport viewport)
    {
        var pipeline = viewport.ViewPipeline;

        ImGui.Checkbox($"{LOC.Get("VIEWPORT_Rendering_Shadows")}##shadows", ref CFG.Current.Viewport_Shadows);
        GUI.Tooltip(LOC.Get("VIEWPORT_Rendering_Shadows_TT"));

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

    /// <summary>
    /// The lighting the loaded maps' GPARAMs give (AC6, ER, NR), for the scene and for models, and where each value
    /// came from, and what the viewports are lit with, see Gparam Lighting.md
    /// </summary>
    private void MapLightingTab(MapEditorView view)
    {
        var bank = view.GparamLightingBank;

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

        var active = SceneRenderer.MapLighting;

        if (CFG.Current.Viewport_Map_Lighting)
        {
            ImGui.Text(active != null
                ? LOC.Get("VIEWPORT_Menubar_Map_Lighting_Active", active.File, active.Exposure * CFG.Current.Viewport_Map_Lighting_Exposure_Scale)
                : LOC.Get("VIEWPORT_Menubar_Map_Lighting_None"));
        }

        ImGui.Separator();

        var maps = view.Project.Handler.MapData.PrimaryBank.Maps.Values
            .Select(w => w.MapContainer)
            .Where(m => m != null && m.LoadState == MapContentLoadState.Loaded)
            .ToList();

        // The files of every loaded map's area first, then any other of the project's, which lights every map
        var files = maps.SelectMany(m => bank.ListFiles(m.Name)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var ownLabel = LOC.Get("VIEWPORT_Menubar_Map_Lighting_File_Own");
        var chosen = CFG.Current.Viewport_Map_Lighting_File;

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
                    CFG.Current.Viewport_Map_Lighting_File = file;
                }
            }

            if (ImGui.Selectable($"{ownLabel}##mapLightingFileOwn", string.IsNullOrEmpty(chosen)))
            {
                CFG.Current.Viewport_Map_Lighting_File = "";
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

        // Weathers: the WW of the area's mAA_BB_CCWW files, named by the WeatherParam rows that use them
        var weathers = maps.SelectMany(m => bank.ListWeathers(m.Name)).Distinct().OrderBy(w => w).ToList();

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
        }

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Hours")}##mapLightingHours",
            ref CFG.Current.Viewport_Map_Lighting_Hours, 0.0f, 24.0f, "%.2f");
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Hours_TT"));

        // The timezones' keys, as the game's cutscenes pick them
        for (var i = 0; i < GparamLighting.TimeKeys.Length; i++)
        {
            if (i > 0)
                ImGui.SameLine();

            if (ImGui.SmallButton($"{LOC.Get(GparamLighting.TimezoneNames[i])}##mapLightingTimezone{i}"))
            {
                CFG.Current.Viewport_Map_Lighting_Hours = GparamLighting.TimeKeys[i];
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Timezone_TT", GparamLighting.TimeKeys[i], i));
        }

        if (view.Project.Descriptor.ProjectType is ProjectType.ER && view.IrradianceVolumeBank.CanUse())
        {
            ImGui.Checkbox($"{LOC.Get("VIEWPORT_Menubar_Map_Lighting_Follow_Bakes")}##mapLightingFollowBakes",
                ref CFG.Current.Viewport_Map_Lighting_Follow_Bakes);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Follow_Bakes_TT"));
        }

        ImGui.Separator();

        if (maps.Count == 0)
        {
            ImGui.Text(LOC.Get("VIEWPORT_Menubar_GI_No_Maps"));
            return;
        }

        var hours = CFG.Current.Viewport_Map_Lighting_Hours;
        var off = LOC.Get("VIEWPORT_Menubar_Map_Lighting_Off");

        string Light(GparamDirectionalLight light) => light.Enabled && light.Diffuse.W > 0.0f
            ? $"{GparamLightingBank.FormatColor(light.Diffuse)} {GparamLightingBank.FormatAngle(light.Angle)}{(light.FollowCamera ? " cam" : "")}"
            : off;

        foreach (var map in maps)
        {
            var file = bank.ChooseFile(map.Name, chosen);
            var scene = bank.Resolve(map.Name, file, GparamLightingBank.SceneLightSetId, hours);
            var model = bank.Resolve(map.Name, file, GparamLightingBank.ModelLightSetId, hours);

            ImGui.Text(LOC.Get("VIEWPORT_Menubar_Map_Lighting_Map", map.Name, scene.File));

            if (!ImGui.BeginTable($"##mapLighting_{map.Name}", 4,
                ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.SizingFixedFit))
                continue;

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

            Row("VIEWPORT_Menubar_Map_Lighting_Row_Exposure", exposure, exposure, "GS Presents - ToneMap-Exposure");
            Row("VIEWPORT_Menubar_Map_Lighting_Row_Auto_Exposure", autoExposure, autoExposure,
                "GS Presents - ToneMap-Enable AutoExposure");
            Row("VIEWPORT_Menubar_Map_Lighting_Row_EnvMap", envMap, envMap, "EnvironmentConfig EnvMapColorConvert Saturate");
            Row("VIEWPORT_Menubar_Map_Lighting_Row_Background", background, background, "GS - BG Color-Data");

            ImGui.EndTable();
        }
    }

    /// <summary>
    /// What the flver shaders do. These apply at once, as the shaders read them every frame
    /// </summary>
    private void MaterialsTab()
    {
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Animation", ref CFG.Current.Viewport_Enable_Material_Animation);

        ImGui.Separator();

        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Wind", ref CFG.Current.Viewport_Material_Wind);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Vertex_Animation", ref CFG.Current.Viewport_Material_Vertex_Animation);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Sway", ref CFG.Current.Viewport_Material_Sway);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Birds", ref CFG.Current.Viewport_Material_Birds);

        ImGui.Separator();

        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Layer_Blending", ref CFG.Current.Viewport_Material_Layer_Blending);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Overlays", ref CFG.Current.Viewport_Material_Overlays);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Magma", ref CFG.Current.Viewport_Material_Magma);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Emissive", ref CFG.Current.Viewport_Material_Emissive);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_UV_Scroll", ref CFG.Current.Viewport_Material_UV_Scroll);

        ImGui.Separator();

        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Unlit", ref CFG.Current.Viewport_Material_Unlit);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Water", ref CFG.Current.Viewport_Material_Water);
        MaterialToggle("VIEWPORT_Menubar_Setting_Material_Fur", ref CFG.Current.Viewport_Material_Fur);

        ImGui.Separator();

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Setting_Wind_Strength")}##windStrength", ref CFG.Current.Viewport_Wind_Strength, 0.0f, 5.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Wind_Strength_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Setting_Wind_Direction")}##windDirection", ref CFG.Current.Viewport_Wind_Direction, 0.0f, 360.0f);
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Wind_Direction_TT"));

        DPI.ApplyInputWidth();
        ImGui.SliderFloat("Grass Density", ref GrassDensity, 0.1f, 4f);

        // Each change rebuilds the grass, so it waits for the slider to be let go
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            CFG.Current.MapEditor_Grass_Density = GrassDensity;
        }

    }

    private static void MaterialToggle(string key, ref bool value)
    {
        ImGui.Checkbox($"{LOC.Get(key)}##{key}", ref value);
        GUI.Tooltip(LOC.Get($"{key}_TT"));
    }
}
