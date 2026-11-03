using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Reads the lighting of Armored Core VI, Elden Ring and Nightreign maps from their GPARAMs: the sun, hemisphere
/// ambient and exposure (<see cref="GparamLighting" />).
/// The map's GPARAMs only hold what differs from the shared ones, so each field is looked up in the chosen file
/// (mAA_BB_0000 by default), then mapbase, then default. Within a file a field holds values by ID, numbered
/// separately in each group: LightSet's ID 0 is the scene's lighting and 100 the lighting of models, with ID 0 used
/// when a file has no value for the ID asked for. Each ID can hold several values through the day, keyed by
/// TimeOfDay, which are interpolated.
/// See Documentation/Full GI/Gparam Lighting.md.
/// </summary>
public class GparamLightingBank
{
    public ProjectEntry Project;

    /// <summary>The LightSet value ID of the scene's lighting</summary>
    public const int SceneLightSetId = 0;

    /// <summary>The LightSet value ID of the lighting of models, which adds the fill lights</summary>
    public const int ModelLightSetId = 100;

    private const string LightSetKey = "LightSet ParamEditor";
    private const string ToneMapKey = "Tone Map[Yebis]";
    private const string EnvironmentConfigKey = "EnvironmentConfig ParamEditor";
    private const string SettingInfoKey = "Setting Info[Yebis]";
    private const string ShadowKey = "ShadowParam ParamEditor";

    // The screen effects' groups, see GparamPostEffects
    private const string BloomKey = "Bloom[Yebis]";
    private const string GlowKey = "Glow Filter-Editor";
    private const string LightShaftKey = "LightShaft[Yebis]";
    private const string VolumetricFogKey = "VolumetricFog ParamEditor";
    private const string SsaoKey = "SSAO Editor";
    private const string SsrKey = "SSR ParamEditor";
    private const string AntiAliasKey = "Anti-Alias-Editor";
    private const string ColorGradingKey = "ColorGrading[Yebis]";
    private const string WetKey = "Wet ParamEditor";

    // The shared GPARAMs a field is looked for in when the map's own file doesn't have it
    private static readonly string[] SharedFiles = { "mapbase", "default" };

    // Each GPARAM read, null if it is missing or couldn't be read
    private readonly Dictionary<string, GPARAM> Files = new(StringComparer.OrdinalIgnoreCase);

    public GparamLightingBank(ProjectEntry project)
    {
        Project = project;
    }

    public bool CanUse()
    {
        if (Project.Descriptor.ProjectType is ProjectType.AC6 or ProjectType.ER or ProjectType.NR)
            return true;

        return false;
    }

    /// <summary>
    /// The area whose GPARAMs a map uses, mAA_BB. Elden Ring maps can take another map's
    /// (GparamRefSettings: row AABBCCDD, RefTargetMapId, e.g. every catacomb m30-m31 takes m30_00), and the open
    /// world tiles (m60, m61) share mAA_00. Null for a map ID that isn't mAA_BB_CC_DD.
    /// </summary>
    public string GetArea(string mapId)
    {
        if (mapId == null || mapId.Length < 12 || !int.TryParse($"{mapId.Substring(1, 2)}{mapId.Substring(4, 2)}{mapId.Substring(7, 2)}{mapId.Substring(10, 2)}", out var rowId))
            return mapId != null && mapId.Length >= 6 ? mapId.Substring(0, 6) : null;

        var target = rowId;

        if (CharacterComposition.GetRow(Project, "GparamRefSettings", rowId) is { } row &&
            CharacterComposition.HasField(row, "RefTargetMapId") &&
            CharacterComposition.GetValue(row, "RefTargetMapId") is var refTarget && refTarget >= 0)
        {
            target = refTarget;
        }

        var aa = target / 1000000;
        var bb = target / 10000 % 100;

        if (aa is 60 or 61)
            bb = 0;

        return $"m{aa:D2}_{bb:D2}";
    }

    /// <summary>
    /// The map's own GPARAM, the plain one of its area (mAA_BB_0000)
    /// </summary>
    public string GetDefaultFile(string mapId)
    {
        var area = GetArea(mapId);

        return area == null ? null : $"{area}_0000";
    }

    /// <summary>
    /// The GPARAM to look a map's lighting up in first: the chosen file if the project has it, from any area, else
    /// the map's own; then, if a weather is chosen and there is a file for it, that file's weather version
    /// (mAA_BB_CCWW, WW being WeatherParam's GparamId)
    /// </summary>
    public string ChooseFile(string mapId, string chosen)
    {
        var own = GetDefaultFile(mapId);

        if (own == null)
            return null;

        var file = !string.IsNullOrEmpty(chosen) && HasFile(chosen) ? chosen : own;

        var weather = CFG.Current.Viewport_Map_Lighting_Weather;

        if (weather is > 0 and < 100 && file.Length == 11)
        {
            var withWeather = $"{file.Substring(0, 9)}{weather:D2}";

            if (HasFile(withWeather))
                return withWeather;
        }

        return file;
    }

    /// <summary>
    /// Whether the project has a GPARAM of this name
    /// </summary>
    public bool HasFile(string name)
    {
        return Project.Locator.GparamFiles.Entries
            .Any(e => e.Filename.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// The file of a scene GPARAM sub ID in a map's area, mAA_BB_0N00 (N -1 is 0), as the collisions' scene GPARAM
    /// picks it (GparamSubID_Base; Unk18 in Nightreign). Null if the area has no such file.
    /// </summary>
    public string GetSubIdFile(string mapId, int subId)
    {
        var area = GetArea(mapId);

        if (area == null || subId > 99)
            return null;

        var file = $"{area}_{Math.Max(subId, 0):D2}00";

        return ListFiles(mapId).Contains(file, StringComparer.OrdinalIgnoreCase) ? file : null;
    }

    /// <summary>
    /// The scene GPARAM sub ID of a part, -1 if it has none or the part has no scene GPARAM. Only collisions have one.
    /// </summary>
    public static int? GetSceneSubId(object part)
    {
        var sceneProperty = part?.GetType().GetProperties()
            .FirstOrDefault(p => p.PropertyType.Name.Contains("SceneGparam", StringComparison.Ordinal));

        var scene = sceneProperty?.GetValue(part);

        if (scene == null)
            return null;

        var subId = scene.GetType().GetProperty("GparamSubID_Base") ?? scene.GetType().GetProperty("Unk18");

        return subId?.GetValue(scene) is IConvertible value ? Convert.ToInt32(value) : null;
    }

    /// <summary>
    /// The weathers a map can be lit with: the WW of its area's mAA_BB_CCWW files, and the weathers of the shared
    /// overlays (m00_00_WWWW_weatheroutdoor), above 0
    /// </summary>
    public List<int> ListWeathers(string mapId)
    {
        var weathers = new SortedSet<int>();

        foreach (var file in ListFiles(mapId))
        {
            if (file.Length == 11 && int.TryParse(file.Substring(9, 2), out var weather) && weather > 0)
                weathers.Add(weather);
        }

        foreach (var entry in Project.Locator.GparamFiles.Entries)
        {
            var name = entry.Filename;

            if (name.StartsWith(WeatherOverlayArea, StringComparison.OrdinalIgnoreCase) &&
                name.EndsWith(WeatherOutdoorSuffix, StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(name.Substring(WeatherOverlayArea.Length, 4), out var weather) && weather > 0)
                weathers.Add(weather);
        }

        return weathers.ToList();
    }

    // The game's m%02u_%02u_%04u_WeatherOutdoor and _WeatherBase, which Elden Ring only has for m00_00, the WWWW being
    // WeatherParam's GparamId. Base has only the Wet group, and only 0000 of it exists.
    private const string WeatherOverlayArea = "m00_00_";
    private const string WeatherOutdoorSuffix = "_weatheroutdoor";
    private const string WeatherBaseSuffix = "_weatherbase";

    /// <summary>
    /// The shared weather GPARAMs read before a map's own for <paramref name="weather" />, nearest first: its
    /// m00_00_WWWW_weatheroutdoor, then m00_00_WWWW_weatherbase or, failing that, m00_00_0000_weatherbase. Empty
    /// for no weather, or with the outdoor weather switched off.
    /// </summary>
    public List<string> GetWeatherOverlays(int weather)
    {
        var overlays = new List<string>();

        if (weather <= 0 || !CFG.Current.Viewport_Map_Lighting_Weather_Outdoor)
            return overlays;

        var outdoor = $"{WeatherOverlayArea}{weather:D4}{WeatherOutdoorSuffix}";
        if (HasFile(outdoor))
            overlays.Add(outdoor);

        var weatherBase = $"{WeatherOverlayArea}{weather:D4}{WeatherBaseSuffix}";
        var defaultBase = $"{WeatherOverlayArea}0000{WeatherBaseSuffix}";

        if (HasFile(weatherBase))
            overlays.Add(weatherBase);
        else if (HasFile(defaultBase))
            overlays.Add(defaultBase);

        return overlays;
    }

    /// <summary>
    /// How wet surfaces are in <paramref name="weather" />: 1 if a WeatherParam row with that GparamId wets them
    /// (WetTime above 0, the seconds they take to soak in the game), else 0
    /// </summary>
    public float GetWetness(int weather)
    {
        if (weather <= 0 || Project.Handler.ParamData?.PrimaryBank?.Params is not { } bank ||
            !bank.TryGetValue("WeatherParam", out var param))
            return 0.0f;

        var wets = param.Rows.Any(r =>
            CharacterComposition.HasField(r, "GparamId") && CharacterComposition.GetValue(r, "GparamId") == weather &&
            CharacterComposition.HasField(r, "WetTime") && CharacterComposition.GetFloat(r, "WetTime") > 0.0f);

        return wets ? 1.0f : 0.0f;
    }

    /// <summary>
    /// The names of the WeatherParam rows with this GparamId, for the weather menu
    /// </summary>
    public string GetWeatherName(int gparamId)
    {
        if (Project.Handler.ParamData?.PrimaryBank?.Params is not { } bank ||
            !bank.TryGetValue("WeatherParam", out var param))
            return null;

        var names = param.Rows
            .Where(r => CharacterComposition.HasField(r, "GparamId") && CharacterComposition.GetValue(r, "GparamId") == gparamId)
            .Select(r => string.IsNullOrWhiteSpace(r.Name) ? r.ID.ToString() : r.Name)
            .Distinct()
            .ToList();

        return names.Count > 0 ? string.Join(", ", names) : null;
    }

    /// <summary>
    /// The GPARAMs of a map's area, mAA_BB_*, the plain ones before those for events and weather
    /// </summary>
    public List<string> ListFiles(string mapId)
    {
        var area = GetArea(mapId);

        if (area == null)
            return new List<string>();

        var prefix = $"{area}_";

        return Project.Locator.GparamFiles.Entries
            .Select(e => e.Filename)
            .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n.Length)
            .ThenBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Every GPARAM of the project, by name, for lighting a map with another area's
    /// </summary>
    public List<string> ListAllFiles()
    {
        return Project.Locator.GparamFiles.Entries
            .Select(e => e.Filename)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Forget the GPARAMs read, so edits to them are picked up the next time the lighting is resolved
    /// </summary>
    public void ClearCache()
    {
        lock (Files)
        {
            Files.Clear();
        }

        ViewportLighting = null;
    }

    // The last lighting given to the viewports, and what it was resolved from
    private GparamLighting ViewportLighting;
    private string ViewportLightingMap;

    /// <summary>
    /// The scene's lighting for the Map Editor's viewports, with the model set in <see cref="GparamLighting.Models" />:
    /// that of the first of the loaded maps (by name) with its own GPARAM, with the file and time of day the viewport
    /// menu chose. Null if map lighting is off or no loaded map has a GPARAM. Only resolved again when one of those
    /// changes.
    /// </summary>
    public GparamLighting ResolveForViewport(IEnumerable<MapContainer> maps)
    {
        if (!CanUse() || !CFG.Current.Viewport_Map_Lighting)
            return null;

        var files = Project.Locator.GparamFiles.Entries;

        var map = maps
            .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(m => files.Any(e => e.Filename.Equals(GetDefaultFile(m.Name), StringComparison.OrdinalIgnoreCase)));

        if (map == null)
            return null;

        return ResolveCached(map.Name, CFG.Current.Viewport_Map_Lighting_File);
    }

    /// <summary>
    /// The scene's lighting of one map, with the model set in <see cref="GparamLighting.Models" />, looked up in
    /// <paramref name="chosen" /> first as <see cref="ChooseFile" /> picks it, at the viewport menu's time of day.
    /// Only resolved again when the map, the file or the time changes.
    /// </summary>
    public GparamLighting ResolveCached(string mapId, string chosen)
    {
        var file = ChooseFile(mapId, chosen);
        var hours = CFG.Current.Viewport_Map_Lighting_Hours;
        var overlays = GetWeatherOverlays(CFG.Current.Viewport_Map_Lighting_Weather);

        var cached = ViewportLighting;

        if (cached != null && ViewportLightingMap == mapId &&
            string.Equals(cached.File, file, StringComparison.OrdinalIgnoreCase) && cached.Hours == hours &&
            cached.WeatherOverlays.SequenceEqual(overlays, StringComparer.OrdinalIgnoreCase))
            return cached;

        cached = Resolve(mapId, file, SceneLightSetId, hours);
        cached.Models = Resolve(mapId, file, ModelLightSetId, hours);

        ViewportLightingMap = mapId;
        ViewportLighting = cached;

        return cached;
    }

    private GPARAM GetFile(string name)
    {
        lock (Files)
        {
            if (Files.TryGetValue(name, out var cached))
                return cached;
        }

        GPARAM gparam = null;

        var entry = Project.Locator.GparamFiles.Entries
            .FirstOrDefault(e => e.Filename.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (entry != null)
        {
            // The project's own, which the Map Editor's bank reads too, so this works without the Map Editor
            var fs = Project.VFS.FS;

            try
            {
                if (fs.FileExists(entry.Path))
                {
                    gparam = GPARAM.Read(fs.ReadFile(entry.Path).Value);
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("MAP_Lighting_Failed_Read_GPARAM", entry.Path), e);
            }
        }

        lock (Files)
        {
            Files[name] = gparam;
        }

        return gparam;
    }

    /// <summary>
    /// The lighting of a map, looked up in the chosen weather's shared overlays first (see
    /// <see cref="GetWeatherOverlays" />), then <paramref name="file" /> (the map's own GPARAM if null or empty), then
    /// mapbase, then default, with <paramref name="lightSetId" /> for the LightSet group and ID 0 for the others,
    /// at <paramref name="hours" /> on the GPARAMs' TimeOfDay scale
    /// </summary>
    public GparamLighting Resolve(string mapId, string file, int lightSetId, float hours)
    {
        if (string.IsNullOrEmpty(file))
            file = GetDefaultFile(mapId);

        var weather = CFG.Current.Viewport_Map_Lighting_Weather;

        var lighting = new GparamLighting
        {
            File = file ?? "",
            LightSetId = lightSetId,
            Hours = hours,
            WeatherOverlays = GetWeatherOverlays(weather)
        };

        if (!CanUse())
            return lighting;

        var chain = new List<(string Name, GPARAM Gparam)>();

        // The weather's overlays are sparse, and go over the map's own: what they leave out comes from the map
        foreach (var name in lighting.WeatherOverlays.Concat(new[] { file }).Concat(SharedFiles))
        {
            if (string.IsNullOrEmpty(name) || chain.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                continue;

            var gparam = GetFile(name);

            if (gparam != null)
                chain.Add((name, gparam));
        }

        var reader = new FieldReader(chain, hours, lighting.Sources);

        // LightSet
        ReadLight(reader, lightSetId, lighting.Sun, "Directional Light Angle0", "Directional Light DiffColor0",
            "Directional Light SpecColor0", null, null);
        // Light1 has no switch; an intensity of 0 leaves it out
        ReadLight(reader, lightSetId, lighting.Light1, "Directional Light Angle1", "Directional Light DiffColor1",
            "Directional Light SpecColor1", null, "Directional LightLight1 FollowedCamera");
        ReadLight(reader, lightSetId, lighting.LocalLight1, "Directional Light LocalLight Angle1",
            "Directional Light LocalLight DiffColor1", "Directional Light LocalLight SpecColor1",
            "Directional Enable LocalLight1", null);
        ReadLight(reader, lightSetId, lighting.LocalLight2, "Directional Light LocalLight Angle2",
            "Directional Light LocalLight DiffColor2", "Directional Light LocalLight SpecColor2",
            "Directional Enable LocalLight2", null);

        lighting.HemiUp = reader.Get(LightSetKey, "Directional Light Hemi Color Up", lightSetId, lighting.HemiUp);
        lighting.HemiDown = reader.Get(LightSetKey, "Directional Light Hemi Color Down", lightSetId, lighting.HemiDown);

        // Tone Map
        lighting.Exposure = reader.Get(ToneMapKey, "GS Presents - ToneMap-Exposure", 0, lighting.Exposure);
        lighting.AutoExposure = reader.Get(ToneMapKey, "GS Presents - ToneMap-Enable AutoExposure", 0, lighting.AutoExposure);
        lighting.MiddleGray = reader.Get(ToneMapKey, "GS Presents - ToneMap-AutoExposure Middle Gray", 0, lighting.MiddleGray);
        lighting.ToneMapMode = reader.Get(ToneMapKey, "GS Presents - ToneMap-ToneMapMode", 0, lighting.ToneMapMode);
        lighting.ReinhardContrast = reader.Get(ToneMapKey, "GS Presents - ToneMap-ReinhardContrast", 0, lighting.ReinhardContrast);
        lighting.ReinhardWhiteBalance = reader.Get(ToneMapKey, "GS Presents - ToneMap-ReinhardWhiteBalance", 0, lighting.ReinhardWhiteBalance);

        // Shadows
        var shadow = lighting.Shadow;
        shadow.Near = reader.Get(ShadowKey, "ShadowParam - Shadow: Near", 0, shadow.Near);
        shadow.CascadeSplits = new Vector3(
            reader.Get(ShadowKey, "ShadowParam - Shadow: Cascade Dist 0->1", 0, shadow.CascadeSplits.X),
            reader.Get(ShadowKey, "ShadowParam - Shadow: Cascade Dist 1->2", 0, shadow.CascadeSplits.Y),
            reader.Get(ShadowKey, "ShadowParam - Shadow: Cascade Dist 2->3", 0, shadow.CascadeSplits.Z));
        shadow.FarFadeStart = reader.Get(ShadowKey, "ShadowParam - Shadow: Far Fade Start", 0, shadow.FarFadeStart);
        shadow.FarFadeDistance = reader.Get(ShadowKey, "ShadowParam - Shadow: Far Fade Dist", 0, shadow.FarFadeDistance);
        shadow.CascadeFadeRangeScale = reader.Get(ShadowKey, "ShadowParam - Shadow: Cascade Fade Range Scale", 0, shadow.CascadeFadeRangeScale);
        shadow.DepthBias = reader.Get(ShadowKey, "ShadowParam - Depth Bias", 0, shadow.DepthBias);
        shadow.SlopeScaledDepthBias = reader.Get(ShadowKey, "ShadowParam - Slope Scaled Depth Bias", 0, shadow.SlopeScaledDepthBias);
        shadow.CascadeCount = reader.Get(ShadowKey, "ShadowParam Cascade Count", 0, shadow.CascadeCount);
        shadow.AlignTexel = reader.Get(ShadowKey, "ShadowParam - AlignTexel", 0, shadow.AlignTexel);

        // EnvironmentConfig
        lighting.EnvMapHue = reader.Get(EnvironmentConfigKey, "EnvironmentConfig EnvMapColorConvert Hue", 0, lighting.EnvMapHue);
        lighting.EnvMapSaturate = reader.Get(EnvironmentConfigKey, "EnvironmentConfig EnvMapColorConvert Saturate", 0, lighting.EnvMapSaturate);
        lighting.EnvMapShift = reader.Get(EnvironmentConfigKey, "EnvironmentConfig EnvMapColorConvert Shift", 0, lighting.EnvMapShift);
        lighting.EnvMapScale = reader.Get(EnvironmentConfigKey, "EnvironmentConfig EnvMapColorConvert Scale", 0, lighting.EnvMapScale);

        // Setting Info
        lighting.BackgroundColor = reader.Get(SettingInfoKey, "GS - BG Color-Data", 0, lighting.BackgroundColor);

        ReadPostEffects(reader, lightSetId, lighting.PostEffects);

        // The LUTs are looked up in the area of the file read first, whose IDs they are
        // Wet's values are in every weather's overlays; whether the weather wets anything is the WeatherParam's
        lighting.PostEffects.Wet.Wetness = GetWetness(weather);

        var grading = lighting.PostEffects.ColorGrading;
        var lutArea = file != null && file.Length >= 6 && file[0] is 'm' or 'M' ? file.Substring(0, 6) : GetArea(mapId);
        grading.Luts = GetLuts();
        grading.Slot = grading.Luts?.Find(lutArea, grading.LutSourceId) ?? -1;
        grading.NextSlot = grading.Luts?.Find(lutArea, grading.NextLutSourceId) ?? -1;

        return lighting;
    }

    private ColorGradingLuts Luts;
    private bool LutsLoaded;

    /// <summary>The game's color grading LUTs, read once, see <see cref="ColorGradingLuts" />. Null if there are none.</summary>
    private ColorGradingLuts GetLuts()
    {
        lock (Files)
        {
            if (LutsLoaded)
                return Luts;
        }

        var fs = Project.VFS.FS;
        var luts = ColorGradingLuts.Load(
            path => fs.FileExists(path) ? fs.ReadFile(path) : null,
            (path, e) => Smithbox.LogError(this, LOC.Get("MAP_Lighting_Failed_Read_LUT", path), e));

        lock (Files)
        {
            Luts = luts.Count > 0 ? luts : null;
            LutsLoaded = true;
            return Luts;
        }
    }

    /// <summary>
    /// The screen effects' groups, at ID 0, except the LightSet's SSAO weights, which are per light set
    /// </summary>
    private static void ReadPostEffects(FieldReader reader, int lightSetId, GparamPostEffects effects)
    {
        var exposure = effects.AutoExposure;
        exposure.LightAdaptSpeed = reader.Get(ToneMapKey, "GS Presents - ToneMap-AutoExposure LightAdapt Speed", 0, exposure.LightAdaptSpeed);
        exposure.DarkAdaptSpeed = reader.Get(ToneMapKey, "GS Presents - ToneMap-AutoExposure DarkAdapt Speed", 0, exposure.DarkAdaptSpeed);
        exposure.CenterWeight = reader.Get(ToneMapKey, "GS Presents - ToneMap-AutoExposure Center Weight", 0, exposure.CenterWeight);
        exposure.AdaptionMax = reader.Get(ToneMapKey, "GS Presents - ToneMap-AutoExposure Adaption Max", 0, exposure.AdaptionMax);
        exposure.AdaptionMin = reader.Get(ToneMapKey, "GS Presents - ToneMap-AutoExposure Adaption Min", 0, exposure.AdaptionMin);

        // The enable's key has full-width brackets in the files
        var bloom = effects.Bloom;
        bloom.Enabled = reader.Get(BloomKey, "Gs Presents【Bloom】 Enable", 0, bloom.Enabled);
        bloom.NumLevels = reader.Get(BloomKey, "Gs Presents-GlareNumLevels", 0, bloom.NumLevels);
        bloom.Luminance = reader.Get(BloomKey, "Gs Presents-Glare Luminance", 0, bloom.Luminance);
        bloom.Threshold = reader.Get(BloomKey, "Gs Presents-Glare Threshold", 0, bloom.Threshold);
        bloom.Alpha = reader.Get(BloomKey, "Gs Presents-GlareBloom Alpha", 0, bloom.Alpha);
        bloom.GaussianRadiusScale = reader.Get(BloomKey, "Gs Presents-GlareBloom GaussianRadiusScale", 0, bloom.GaussianRadiusScale);
        bloom.SaturateControl = reader.Get(BloomKey, "Gs Presents-GlareBloom Sturate Ctrl", 0, bloom.SaturateControl);
        bloom.Smooth = reader.Get(BloomKey, "Gs Presents-GlareBloom Smooth", 0, bloom.Smooth);

        var glow = effects.Glow;
        glow.Enabled = reader.Get(GlowKey, "Glow Filter-Glow Enable", 0, glow.Enabled);
        glow.NumLevels = reader.Get(GlowKey, "Glow Filter - Glow NumLevels", 0, glow.NumLevels);
        glow.BlurSmoothness = reader.Get(GlowKey, "Glow Filter - Blur Smoothness", 0, glow.BlurSmoothness);
        glow.FinalColorScale = reader.Get(GlowKey, "New Glow Filter-Glow - FinalColorScale", 0, glow.FinalColorScale);
        glow.BlurSampleScale = reader.Get(GlowKey, "Glow Filter-Glow - BlurSampleScale", 0, glow.BlurSampleScale);
        glow.BlurRgbScale = reader.Get(GlowKey, "Glow Filter-Glow - Blur RgbScale", 0, glow.BlurRgbScale);
        glow.BlurAlphaScale = reader.Get(GlowKey, "Glow Filter-Glow - Blur AlphaScale", 0, glow.BlurAlphaScale);

        var shaft = effects.LightShaft;
        shaft.Enabled = reader.Get(LightShaftKey, "GS Presents - LightShaft Enable", 0, shaft.Enabled);
        shaft.Angle = reader.Get(LightShaftKey, "GS Presents - LightShaft Angle", 0, shaft.Angle, isAngle: true);
        shaft.EnableRefLight = reader.Get(LightShaftKey, "GS Presents - LightShaft EnableRefLight", 0, shaft.EnableRefLight);
        shaft.LightBankId = reader.Get(LightShaftKey, "GS Presents - LightShaft LightBankId", 0, shaft.LightBankId);
        shaft.LightId = reader.Get(LightShaftKey, "GS Presents - LightShaft LightId", 0, shaft.LightId);
        shaft.HighQualitySkyMask = reader.Get(LightShaftKey, "GS Presents - LightShaft HighQualitySkyMask", 0, shaft.HighQualitySkyMask);
        shaft.SkyMaskMode = reader.Get(LightShaftKey, "GS Presents - LightShaft SkyMaskMode", 0, shaft.SkyMaskMode);
        shaft.MaskDepthThreshold = reader.Get(LightShaftKey, "GS Presents - LightShaft MaskDepthThreshold", 0, shaft.MaskDepthThreshold);
        shaft.SkyMaskSizeAngle = reader.Get(LightShaftKey, "GS Presents - LightShaft SkyMaskSizeAngle", 0, shaft.SkyMaskSizeAngle);
        shaft.SkyMaskCenterPower = reader.Get(LightShaftKey, "GS Presents - LightShaft SkyMaskCenterPower", 0, shaft.SkyMaskCenterPower);
        shaft.SkyMultiplyColor = reader.Get(LightShaftKey, "GS Presents - Sky Multiply Color", 0, shaft.SkyMultiplyColor);
        shaft.SkyAddColor = reader.Get(LightShaftKey, "GS Presents - Sky Add Color", 0, shaft.SkyAddColor);
        shaft.SkyColorSaturate = reader.Get(LightShaftKey, "GS Presents - LightShaft SkyColorSaturate", 0, shaft.SkyColorSaturate);
        shaft.SkyColorThreshold = reader.Get(LightShaftKey, "GS Presents - LightShaft SkyColorThreshold", 0, shaft.SkyColorThreshold);
        shaft.ExposureInfluenceAddColor = reader.Get(LightShaftKey, "GS Presents - ExposureInfluenceRate AddColor", 0, shaft.ExposureInfluenceAddColor);
        shaft.ParallelBeginAngle = reader.Get(LightShaftKey, "GS Presents - Parallel mode begin ang", 0, shaft.ParallelBeginAngle);
        shaft.ParallelEndAngle = reader.Get(LightShaftKey, "GS Presents - Parallel mode end ang", 0, shaft.ParallelEndAngle);
        shaft.FadeOutBeginAngle = reader.Get(LightShaftKey, "GS Presents - Fade out begin ang", 0, shaft.FadeOutBeginAngle);
        shaft.FadeOutEndAngle = reader.Get(LightShaftKey, "GS Presents - Fade out end ang", 0, shaft.FadeOutEndAngle);
        shaft.BlurRange = reader.Get(LightShaftKey, "GS Presents - ShaftBlurRange", 0, shaft.BlurRange);
        shaft.BlurRangeParallel = reader.Get(LightShaftKey, "GS Presents - LightShaft ShaftBlurRange for Parallel", 0, shaft.BlurRangeParallel);
        shaft.BlurFalloff = reader.Get(LightShaftKey, "GS Presents - LightShaft ShaftBlurFallOff", 0, shaft.BlurFalloff);
        shaft.BlurCount = reader.Get(LightShaftKey, "GS Presents - LightShaft ShaftBlurCount", 0, shaft.BlurCount);

        var fog = effects.VolumetricFog;
        fog.Enabled = reader.Get(VolumetricFogKey, "VolumetricFog Enable", 0, fog.Enabled);
        fog.VolumeWidth = reader.Get(VolumetricFogKey, "VolumetricFog VolumeWidth", 0, fog.VolumeWidth);
        fog.VolumeDepth = reader.Get(VolumetricFogKey, "VolumetricFog VolumeDepth", 0, fog.VolumeDepth);
        fog.NearDepth = reader.Get(VolumetricFogKey, "VolumetricFog NearDepth", 0, fog.NearDepth);
        fog.FarDepth = reader.Get(VolumetricFogKey, "VolumetricFog FarDepth", 0, fog.FarDepth);
        fog.FogEndDepth = reader.Get(VolumetricFogKey, "VolumetricFog FogEndDepth", 0, fog.FogEndDepth);
        fog.EnableAnalyticalFog = reader.Get(VolumetricFogKey, "VolumetricFog EnableAnalyticalFog", 0, fog.EnableAnalyticalFog);
        fog.FixedDensity = reader.Get(VolumetricFogKey, "VolumetricFog Global FixedDensity", 0, fog.FixedDensity);
        fog.ScatteringG = reader.Get(VolumetricFogKey, "VolumetricFog Global ScatteringG", 0, fog.ScatteringG);
        fog.LocalLightScale = reader.Get(VolumetricFogKey, "VolumetricFog Global LocalLightScale", 0, fog.LocalLightScale);
        fog.LocalLightDistScale = reader.Get(VolumetricFogKey, "VolumetricFog Global LocalLightDistScale", 0, fog.LocalLightDistScale);
        fog.HeightFogDistribution = reader.Get(VolumetricFogKey, "VolumetricFog Global HeightFogDistribution", 0, fog.HeightFogDistribution);
        fog.HeightFogOffset = reader.Get(VolumetricFogKey, "VolumetricFog Global HeightFogOffset", 0, fog.HeightFogOffset);
        fog.HeightFogDensityScale = reader.Get(VolumetricFogKey, "VolumetricFog Global HeightFogDensity Scale", 0, fog.HeightFogDensityScale);
        fog.AbsorptionScale = reader.Get(VolumetricFogKey, "VolumetricFog Global AbsorptionScale", 0, fog.AbsorptionScale);
        fog.SkyColor = reader.Get(VolumetricFogKey, "VolumetricFog SkyLight SkyColor", 0, fog.SkyColor);
        fog.SkyHeightAtten = reader.Get(VolumetricFogKey, "VolumetricFog SkyLight HeightAtten", 0, fog.SkyHeightAtten);
        fog.SkyStartDistance = reader.Get(VolumetricFogKey, "VolumetricFog SkyLight StartDist", 0, fog.SkyStartDistance);
        fog.DirLightBankId = reader.Get(VolumetricFogKey, "VolumetricFog DirLight LightBankId", 0, fog.DirLightBankId);
        fog.DirLightId = reader.Get(VolumetricFogKey, "VolumetricFog DirLight LightId", 0, fog.DirLightId);
        fog.DirectColor = reader.Get(VolumetricFogKey, "VolumetricFog DirLight DirectColor", 0, fog.DirectColor);
        fog.OppositeColor = reader.Get(VolumetricFogKey, "VolumetricFog DirLight OppositeColor", 0, fog.OppositeColor);
        fog.DirHeightAtten = reader.Get(VolumetricFogKey, "VolumetricFog DirLight HeightAtten", 0, fog.DirHeightAtten);
        fog.DirStartDistance = reader.Get(VolumetricFogKey, "VolumetricFog DirLight StartDist", 0, fog.DirStartDistance);
        fog.ShadowSampleCount = reader.Get(VolumetricFogKey, "VolumetricFog DirLight ShadowSampleCount", 0, fog.ShadowSampleCount);
        fog.SampleCount = reader.Get(VolumetricFogKey, "VolumetricFog Config SampleCount", 0, fog.SampleCount);

        var ssao = effects.Ssao;
        ssao.UseNormal = reader.Get(SsaoKey, "SSAO - Edit-UseNormal", 0, ssao.UseNormal);
        ssao.UseReprojection = reader.Get(SsaoKey, "SSAO - Edit-UseReProj", 0, ssao.UseReprojection);
        ssao.SampleOffset = reader.Get(SsaoKey, "SSAO - Edit-Ver2 Sample Offset", 0, ssao.SampleOffset);
        ssao.Scale = reader.Get(SsaoKey, "SSAO - Edit-Ver2 AO Scale", 0, ssao.Scale);
        ssao.Offset = reader.Get(SsaoKey, "SSAO - Edit-Ver2 AO Offset", 0, ssao.Offset);
        ssao.KernelBaseDistance = reader.Get(SsaoKey, "SSAO - Edit-Ver2 Kernel Scale Base Dist", 0, ssao.KernelBaseDistance);
        ssao.KernelMin = reader.Get(SsaoKey, "SSAO - Edit-Ver2 Kernel Scale Min", 0, ssao.KernelMin);
        ssao.KernelMax = reader.Get(SsaoKey, "SSAO - Edit-Ver2 Kernel Scale Max", 0, ssao.KernelMax);
        ssao.DepthFadeStart = reader.Get(SsaoKey, "SSAO - Edit-Ver2 DepthFadeStart", 0, ssao.DepthFadeStart);
        ssao.DepthFadeRange = reader.Get(SsaoKey, "SSAO - Edit-Ver2 DepthFadeRange", 0, ssao.DepthFadeRange);
        ssao.PrecisionDepth = reader.Get(SsaoKey, "SSAO - Edit-Ver2 PrecisionDepth", 0, ssao.PrecisionDepth);
        ssao.DepthRejectRate = reader.Get(SsaoKey, "SSAO - Edit-Ver2 Depth Reject Rate", 0, ssao.DepthRejectRate);
        ssao.GeomBentOffset = reader.Get(SsaoKey, "SSAO - Edit-Ver2 Geom Bent Offset", 0, ssao.GeomBentOffset);
        ssao.LightWeights = new Vector4(
            reader.Get(LightSetKey, "Directional Light SsaoWeight0", lightSetId, ssao.LightWeights.X),
            reader.Get(LightSetKey, "Directional Light SsaoWeight1", lightSetId, ssao.LightWeights.Y),
            reader.Get(LightSetKey, "Directional Light SsaoWeight2", lightSetId, ssao.LightWeights.Z),
            reader.Get(LightSetKey, "Directional Light SsaoWeight3", lightSetId, ssao.LightWeights.W));

        var ssr = effects.Ssr;
        ssr.Enabled = reader.Get(SsrKey, "SSR Enable", 0, ssr.Enabled);
        ssr.TraverseMode = reader.Get(SsrKey, "SSR TraverseMode", 0, ssr.TraverseMode);
        ssr.ShininessRejectThreshold = reader.Get(SsrKey, "SSR Shininess Reject Threshold", 0, ssr.ShininessRejectThreshold);
        ssr.DepthRejectThreshold = reader.Get(SsrKey, "SSR Depth Reject Threshold", 0, ssr.DepthRejectThreshold);
        ssr.FadeToViewerStart = reader.Get(SsrKey, "SSR Fade To Viewer Start", 0, ssr.FadeToViewerStart);
        ssr.FresnelRejectThreshold = reader.Get(SsrKey, "SSR Fresnel Reject Threshold", 0, ssr.FresnelRejectThreshold);
        ssr.StepScaleMin = reader.Get(SsrKey, "SSR Ray Trace Step Scale Min", 0, ssr.StepScaleMin);
        ssr.StepScaleMax = reader.Get(SsrKey, "SSR Ray Trace Step Scale Max", 0, ssr.StepScaleMax);
        ssr.MipScale = reader.Get(SsrKey, "SSR Display Mip Scale", 0, ssr.MipScale);
        ssr.MipOffset = reader.Get(SsrKey, "SSR Display Mip Offset", 0, ssr.MipOffset);
        ssr.DepthFadeRange = reader.Get(SsrKey, "SSR Depth Fade Range", 0, ssr.DepthFadeRange);
        ssr.CrackDetectFactor = reader.Get(SsrKey, "SSR Geom Crack Detect Factor", 0, ssr.CrackDetectFactor);
        ssr.BlurSigma = reader.Get(SsrKey, "SSR Tex Blur Sigma", 0, ssr.BlurSigma);
        ssr.BlurSigmaMip = reader.Get(SsrKey, "SSR Tex Blur Sigma for Mip", 0, ssr.BlurSigmaMip);

        var aa = effects.AntiAlias;
        aa.Enabled = reader.Get(AntiAliasKey, "Yebis-Anti-Alias Enable", 0, aa.Enabled);
        aa.Type = reader.Get(AntiAliasKey, "AntiAlias - AntiAliasType", 0, aa.Type);
        aa.FxaaAlpha = reader.Get(AntiAliasKey, "AntiAlias - fFxaaAlpha", 0, aa.FxaaAlpha);
        aa.FxaaReduceMin = reader.Get(AntiAliasKey, "AntiAlias - FxaaReduceMin", 0, aa.FxaaReduceMin);
        aa.FxaaReduceMul = reader.Get(AntiAliasKey, "AntiAlias - FxaaReduceMul", 0, aa.FxaaReduceMul);
        aa.FxaaSpanMax = reader.Get(AntiAliasKey, "AntiAlias - FxaaSpanMax", 0, aa.FxaaSpanMax);
        aa.FeedbackWeight = reader.Get(AntiAliasKey, "AntiAlias - FeedbackWeight", 0, aa.FeedbackWeight);
        aa.JitterCount = reader.Get(AntiAliasKey, "AntiAlias - JitterCount", 0, aa.JitterCount);
        aa.SharpenIntensity = reader.Get(AntiAliasKey, "AntiAlias - SharpenIntensity", 0, aa.SharpenIntensity);
        aa.SharpenRangeMin = reader.Get(AntiAliasKey, "AntiAlias - SharpenRangeMin", 0, aa.SharpenRangeMin);
        aa.SharpenRangeMax = reader.Get(AntiAliasKey, "AntiAlias - SharpenRangeMax", 0, aa.SharpenRangeMax);
        aa.BorderDepth = new Vector3(
            reader.Get(AntiAliasKey, "AntiAlias - FBWeightBorderDepth(0)", 0, aa.BorderDepth.X),
            reader.Get(AntiAliasKey, "AntiAlias - FBWeightBorderDepth(1)", 0, aa.BorderDepth.Y),
            reader.Get(AntiAliasKey, "AntiAlias - FBWeightBorderDepth(2)", 0, aa.BorderDepth.Z));
        aa.CancelByDepth = new Vector4(
            reader.Get(AntiAliasKey, "AntiAlias - FBWeightCancelByDepth(0)", 0, aa.CancelByDepth.X),
            reader.Get(AntiAliasKey, "AntiAlias - FBWeightCancelByDepth(1)", 0, aa.CancelByDepth.Y),
            reader.Get(AntiAliasKey, "AntiAlias - FBWeightCancelByDepth(2)", 0, aa.CancelByDepth.Z),
            reader.Get(AntiAliasKey, "AntiAlias - FBWeightCancelByDepth(3)", 0, aa.CancelByDepth.W));

        var grading = effects.ColorGrading;
        grading.Enabled = reader.Get(ColorGradingKey, "Yebis-ColorGrading-Enable", 0, grading.Enabled);
        (grading.LutSourceId, grading.NextLutSourceId, grading.Blend) =
            reader.GetKeys(ColorGradingKey, "Yebis-LutSourceId", 0, grading.LutSourceId);
        grading.FadeTime = reader.Get(ColorGradingKey, "ColorGrading FadeTime", 0, grading.FadeTime);

        var wet = effects.Wet;
        wet.OverrideAlbedo = reader.Get(WetKey, "Wet-OverrideAlbedo", 0, wet.OverrideAlbedo);
        wet.OverrideShininess = reader.Get(WetKey, "Wet-OverrideShininess", 0, wet.OverrideShininess);
        wet.AlbedoRate = reader.Get(WetKey, "Wet-AlbedoRate", 0, wet.AlbedoRate);
        wet.ShininessRate = reader.Get(WetKey, "Wet-ShininessRate", 0, wet.ShininessRate);
    }

    private static void ReadLight(FieldReader reader, int id, GparamDirectionalLight light,
        string angleKey, string diffuseKey, string specularKey, string enableKey, string followKey)
    {
        light.Angle = reader.Get(LightSetKey, angleKey, id, light.Angle, isAngle: true);
        light.Diffuse = reader.Get(LightSetKey, diffuseKey, id, light.Diffuse);
        light.Specular = reader.Get(LightSetKey, specularKey, id, light.Specular);

        if (enableKey != null)
            light.Enabled = reader.Get(LightSetKey, enableKey, id, light.Enabled);

        if (followKey != null)
            light.FollowCamera = reader.Get(LightSetKey, followKey, id, light.FollowCamera);
    }

    /// <summary>
    /// The value of a field for a value ID at <paramref name="hours" />, interpolated between its TimeOfDay keys as
    /// <see cref="Resolve" /> does. False if the field has no value of that ID, or not one of the type asked for.
    /// </summary>
    public static bool TryGetValue<T>(GPARAM.IField field, int id, float hours, T fallback, out T result, bool isAngle = false)
    {
        result = fallback;

        var values = field.Values.Where(v => v.ID == id).ToList();

        if (values.Count == 0)
            return false;

        return FieldReader.TryConvert(FieldReader.Interpolate(values, hours, isAngle), fallback, out result);
    }

    public static string FormatColor(Vector4 c) => $"({c.X:0.###}, {c.Y:0.###}, {c.Z:0.###}) x {c.W:0.###}";

    public static string FormatAngle(Vector2 a) => $"({a.X:0.###}, {a.Y:0.###})";

    /// <summary>
    /// Looks fields up along the chain of GPARAMs, and interpolates their values through the day
    /// </summary>
    private class FieldReader
    {
        private readonly List<(string Name, GPARAM Gparam)> Chain;
        private readonly float Hours;
        private readonly Dictionary<string, string> Sources;

        public FieldReader(List<(string Name, GPARAM Gparam)> chain, float hours, Dictionary<string, string> sources)
        {
            Chain = chain;
            Hours = hours;
            Sources = sources;
        }

        public T Get<T>(string groupKey, string fieldKey, int id, T fallback, bool isAngle = false)
        {
            foreach (var (name, gparam) in Chain)
            {
                var field = gparam.Params.FirstOrDefault(p => p.Key == groupKey)?.Fields
                    .FirstOrDefault(f => f.Key == fieldKey);

                if (field == null)
                    continue;

                var usedId = id;
                var values = field.Values.Where(v => v.ID == id).ToList();

                if (values.Count == 0 && id != 0)
                {
                    usedId = 0;
                    values = field.Values.Where(v => v.ID == 0).ToList();
                }

                if (values.Count == 0)
                    continue;

                var value = Interpolate(values, Hours, isAngle);

                if (!TryConvert(value, fallback, out T result))
                    continue;

                Sources[fieldKey] = $"{name} #{usedId}";
                return result;
            }

            return fallback;
        }

        /// <summary>
        /// The values of a field at the keys either side of the time of day, and how far between them it is, for a
        /// value that can't be blended itself, such as an ID whose two things are blended instead. Looked up along
        /// the chain as <see cref="Get{T}" /> does.
        /// </summary>
        public (T Previous, T Next, float Blend) GetKeys<T>(string groupKey, string fieldKey, int id, T fallback)
        {
            foreach (var (name, gparam) in Chain)
            {
                var field = gparam.Params.FirstOrDefault(p => p.Key == groupKey)?.Fields
                    .FirstOrDefault(f => f.Key == fieldKey);

                if (field == null)
                    continue;

                var usedId = id;
                var values = field.Values.Where(v => v.ID == id).ToList();

                if (values.Count == 0 && id != 0)
                {
                    usedId = 0;
                    values = field.Values.Where(v => v.ID == 0).ToList();
                }

                if (values.Count == 0)
                    continue;

                var (previous, next, t) = Neighbours(values, Hours);

                if (!TryConvert(previous.Value, fallback, out T a) || !TryConvert(next.Value, fallback, out T b))
                    continue;

                Sources[fieldKey] = $"{name} #{usedId}";
                return (a, b, t);
            }

            return (fallback, fallback, 0.0f);
        }

        /// <summary>
        /// The keys either side of a time of day, wrapping from the last key of the day round to the first, and how
        /// far from the first to the second it is. The same key twice, at 0, when there is only one.
        /// </summary>
        private static (GPARAM.IFieldValue Previous, GPARAM.IFieldValue Next, float Blend) Neighbours(
            List<GPARAM.IFieldValue> values, float hours)
        {
            if (values.Count == 1)
                return (values[0], values[0], 0.0f);

            var keys = values.OrderBy(v => v.TimeOfDay).ToList();

            hours = ((hours % 24.0f) + 24.0f) % 24.0f;

            var previous = keys.LastOrDefault(k => k.TimeOfDay <= hours) ?? keys[^1];
            var next = keys.FirstOrDefault(k => k.TimeOfDay > hours) ?? keys[0];

            var span = ((next.TimeOfDay - previous.TimeOfDay) % 24.0f + 24.0f) % 24.0f;

            if (span <= 0.0f || ReferenceEquals(previous, next))
                return (previous, previous, 0.0f);

            var t = (((hours - previous.TimeOfDay) % 24.0f + 24.0f) % 24.0f) / span;

            return (previous, next, t);
        }

        /// <summary>
        /// The value at a time of day: between the keys either side of it, wrapping from the last key of the day
        /// round to the first. Numbers and vectors are blended; anything else takes the earlier key.
        /// </summary>
        internal static object Interpolate(List<GPARAM.IFieldValue> values, float hours, bool isAngle)
        {
            var (previous, next, t) = Neighbours(values, hours);

            if (ReferenceEquals(previous, next))
                return previous.Value;

            return Lerp(previous.Value, next.Value, t, isAngle);
        }

        private static object Lerp(object a, object b, float t, bool isAngle)
        {
            switch (a)
            {
                case float fa when b is float fb:
                    return fa + (fb - fa) * t;
                case Vector2 va when b is Vector2 vb:
                    if (isAngle)
                    {
                        // The yaw takes the short way round
                        var yaw = vb.Y - va.Y;
                        yaw -= MathF.Round(yaw / MathF.Tau) * MathF.Tau;
                        return new Vector2(va.X + (vb.X - va.X) * t, va.Y + yaw * t);
                    }
                    return Vector2.Lerp(va, vb, t);
                case Vector3 va when b is Vector3 vb:
                    return Vector3.Lerp(va, vb, t);
                case Vector4 va when b is Vector4 vb:
                    return Vector4.Lerp(va, vb, t);
                case Color ca when b is Color cb:
                    return Vector4.Lerp(ToVector(ca), ToVector(cb), t);
                default:
                    return a;
            }
        }

        private static Vector4 ToVector(Color c) => new(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);

        internal static bool TryConvert<T>(object value, T fallback, out T result)
        {
            result = fallback;

            if (value is T t)
            {
                result = t;
                return true;
            }

            if (value is Color c && typeof(T) == typeof(Vector4))
            {
                result = (T)(object)ToVector(c);
                return true;
            }

            // Switches are stored as bools, or as ints in some groups (e.g. Enable AutoExposure)
            if (typeof(T) == typeof(bool) && value is IConvertible)
            {
                try
                {
                    result = (T)(object)(Convert.ToDouble(value) != 0.0);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            if (value is IConvertible && fallback is IConvertible)
            {
                try
                {
                    result = (T)Convert.ChangeType(value, typeof(T));
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }
    }
}
