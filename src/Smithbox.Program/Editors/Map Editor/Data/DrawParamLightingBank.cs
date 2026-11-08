using Andre.Formats;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The rows of a map area's DrawParam banks that its lighting is read from, by row ID
/// </summary>
public record struct DrawParamRows(int Light, int ToneMap, int Shadow, int ToneCorrect, int Fog);

/// <summary>
/// Lights the Map Editor's viewports of a game without GPARAMs from its DrawParam banks (Demon's Souls), filling the
/// same <see cref="GparamLighting" /> the renderer reads for the GPARAM games.
/// <para>
/// A map mAA_BB_CC_DD uses the banks of its area, the params mAA_LightBank, mAA_ToneMapBank... of
/// param/drawparam/aAA_drawparam.parambnd, which the Param Editor's bank already holds. Map parts pick a row of each
/// bank by their LightID, ToneMapID, ShadowID, ToneCorrectID (MSBD.Part). The whole viewport is lit with one row per
/// bank: the one most of the map's pieces use, or the one the viewport menu chose. Fields are read by name and
/// converted to float, so the s32 fields of DES and the s16 ones of DS1 read alike.
/// </para>
/// See Documentation/Claude/Older Games/DES DrawParam Lighting Plan.md.
/// </summary>
public class DrawParamLightingBank
{
    public ProjectEntry Project;

    public const string LightBank = "LightBank";
    public const string ToneMapBank = "ToneMapBank";
    public const string ShadowBank = "ShadowBank";
    public const string ToneCorrectBank = "ToneCorrectBank";
    public const string FogBank = "FogBank";

    /// <summary>The banks read, in the order the menu offers them</summary>
    public static readonly string[] Banks = { LightBank, ToneMapBank, ShadowBank, ToneCorrectBank, FogBank };

    public DrawParamLightingBank(ProjectEntry project)
    {
        Project = project;
    }

    public GparamProfile Profile => GparamProfile.For(Project.Descriptor.ProjectType);

    public bool CanUse()
    {
        return Profile.DrawParamLighting && Project.Handler.ParamData?.PrimaryBank?.Params != null;
    }

    /// <summary>The area prefix of a map's DrawParam params: m01 for m01_00_00_00</summary>
    public static string GetArea(string mapId)
    {
        return mapId != null && mapId.Length >= 3 ? mapId.Substring(0, 3).ToLowerInvariant() : null;
    }

    /// <summary>A bank of a map's area, e.g. m01_LightBank; null if the project has none</summary>
    public Param GetBank(string mapId, string bank)
    {
        var area = GetArea(mapId);
        var bankParams = Project.Handler.ParamData?.PrimaryBank?.Params;

        if (area == null || bankParams == null)
            return null;

        return bankParams.TryGetValue($"{area}_{bank}", out var param) ? param : null;
    }

    /// <summary>The viewport menu's chosen row of a bank, -1 for the most-used one</summary>
    public static ref int ChosenRow(string bank)
    {
        switch (bank)
        {
            case ToneMapBank: return ref CFG.Current.Viewport_Map_DrawParam_ToneMap_Row;
            case ShadowBank: return ref CFG.Current.Viewport_Map_DrawParam_Shadow_Row;
            case ToneCorrectBank: return ref CFG.Current.Viewport_Map_DrawParam_ToneCorrect_Row;
            case FogBank: return ref CFG.Current.Viewport_Map_DrawParam_Fog_Row;
            default: return ref CFG.Current.Viewport_Map_DrawParam_Light_Row;
        }
    }

    /// <summary>The row of a bank a part picks</summary>
    public static int GetPartRow(MSBD.Part part, string bank) => bank switch
    {
        ToneMapBank => part.ToneMapID,
        ShadowBank => part.ShadowID,
        ToneCorrectBank => part.ToneCorrectID,
        FogBank => part.FogID,
        _ => part.LightID
    };

    public static int Get(DrawParamRows rows, string bank) => bank switch
    {
        ToneMapBank => rows.ToneMap,
        ShadowBank => rows.Shadow,
        ToneCorrectBank => rows.ToneCorrect,
        FogBank => rows.Fog,
        _ => rows.Light
    };

    // The most-used IDs of each map, worked out again every second or so, as parts can be edited
    private readonly Dictionary<string, (DrawParamRows Rows, long Time)> MostUsed = new();
    private readonly Stopwatch Clock = Stopwatch.StartNew();

    /// <summary>The row of each bank most of a map's pieces pick, the rows the map is lit with by default</summary>
    public DrawParamRows GetMostUsedRows(MapContainer map)
    {
        var now = Clock.ElapsedMilliseconds;

        if (MostUsed.TryGetValue(map.Name, out var cached) && now - cached.Time < 1000)
            return cached.Rows;

        var pieces = map.Parts
            .Select(e => e.WrappedObject)
            .OfType<MSBD.Part.MapPiece>()
            .ToList();

        int MostUsedId(string bank) => pieces.Count == 0
            ? 0
            : pieces.GroupBy(p => GetPartRow(p, bank)).OrderByDescending(g => g.Count()).ThenBy(g => g.Key).First().Key;

        var rows = new DrawParamRows(MostUsedId(LightBank), MostUsedId(ToneMapBank), MostUsedId(ShadowBank),
            MostUsedId(ToneCorrectBank), MostUsedId(FogBank));

        MostUsed[map.Name] = (rows, now);

        return rows;
    }

    /// <summary>The rows a map is lit with: per bank, the menu's choice if that row exists, else the most-used one</summary>
    public DrawParamRows ChooseRows(MapContainer map)
    {
        var mostUsed = GetMostUsedRows(map);

        int Choose(string bank)
        {
            var chosen = ChosenRow(bank);
            return chosen >= 0 && GetBank(map.Name, bank)?[chosen] != null ? chosen : Get(mostUsed, bank);
        }

        return new DrawParamRows(Choose(LightBank), Choose(ToneMapBank), Choose(ShadowBank), Choose(ToneCorrectBank),
            Choose(FogBank));
    }

    // The last lighting given to the viewports, and what it was resolved from
    private GparamLighting ViewportLighting;
    private (string Map, DrawParamRows Rows, int Hash) ViewportKey;

    /// <summary>
    /// The scene's lighting for the Map Editor's viewports, with the model set in <see cref="GparamLighting.Models" />:
    /// that of the first of the loaded maps (by name) whose area has a LightBank. Null if map lighting is off or no
    /// loaded map has DrawParam. Resolved again only when the map, the rows or the rows' values change, so the Param
    /// Editor's edits show at once.
    /// </summary>
    public GparamLighting ResolveForViewport(IEnumerable<MapContainer> maps)
    {
        if (!CanUse() || !CFG.Current.Viewport_Map_Lighting)
            return null;

        var map = GetLitMap(maps);

        return map == null ? null : ResolveCached(map);
    }

    /// <summary>The map of those loaded that the viewport is lit by: the first by name whose area has a LightBank</summary>
    public MapContainer GetLitMap(IEnumerable<MapContainer> maps)
    {
        return maps
            .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(m => GetBank(m.Name, LightBank) != null);
    }

    /// <summary>The lighting of one map with the rows <see cref="ChooseRows" /> picks, cached as <see cref="ResolveForViewport" /> describes</summary>
    public GparamLighting ResolveCached(MapContainer map)
    {
        var rows = ChooseRows(map);
        var hash = new HashCode();

        foreach (var bank in Banks)
            AddToHash(ref hash, GetBank(map.Name, bank)?[Get(rows, bank)]);

        var key = (map.Name, rows, hash.ToHashCode());

        if (ViewportLighting != null && ViewportKey == key)
            return ViewportLighting;

        ViewportLighting = Resolve(map.Name, rows);
        ViewportKey = key;

        return ViewportLighting;
    }

    // The per-part sets: each distinct (area, LightID) other than the scene's own, with what it was resolved from
    private readonly List<GparamLighting> PartSets = new();
    private readonly Dictionary<(string Area, int LightId), (int Index, int Hash)> PartSetIndex = new();
    private readonly HashSet<MeshRenderableProxy> LitProxies = new();

    /// <summary>
    /// How many parts are lit by a set of their own, and how many sets there are, as last assigned
    /// </summary>
    public (int Parts, int Sets) PartLightingCounts { get; private set; }

    /// <summary>
    /// Lights each loaded part by the LightBank row its own LightID picks (Phase 4): the parts whose row is the one the
    /// whole scene is lit with keep the scene's lighting, the others name a set of <see cref="SceneRenderer.LightSets" />
    /// in their mesh (<see cref="MeshRenderableProxy.LightSet" />). The sets differ from the scene only in their lights:
    /// the exposure, shadows, fog and tone correction stay the scene's. Off, everything goes back to the scene's
    /// lighting: when per-part lighting is switched off, when map lighting is off, and when the menu chose a LightBank
    /// row, which then lights everything. Returns the sets, set 1 first, for <see cref="SceneRenderer.MapLightSets" />.
    /// Called every frame.
    /// </summary>
    public IReadOnlyList<GparamLighting> UpdatePartLighting(IReadOnlyList<MapContainer> maps, GparamLighting scene)
    {
        var litMap = scene != null ? GetLitMap(maps) : null;

        if (litMap == null || !CFG.Current.Viewport_Map_DrawParam_Per_Part || ChosenRow(LightBank) >= 0)
        {
            ClearPartLighting();
            return PartSets;
        }

        var sceneKey = (GetArea(litMap.Name), ChooseRows(litMap).Light);
        var used = new HashSet<(string, int)>();
        var stillLit = new HashSet<MeshRenderableProxy>();
        var parts = 0;

        // Each row's values hashed once this frame, however many parts pick it
        var rowHashes = new Dictionary<(string, int), int>();

        foreach (var map in maps)
        {
            var area = GetArea(map.Name);
            var lightBank = GetBank(map.Name, LightBank);

            if (lightBank == null)
                continue;

            foreach (var entity in map.Parts)
            {
                if (entity.WrappedObject is not MSBD.Part part || entity.RenderSceneMesh is not MeshRenderableProxy proxy)
                    continue;

                var key = (area, (int)part.LightID);
                var row = lightBank[part.LightID];
                byte set = 0;

                if (key != sceneKey && row != null)
                {
                    if (!rowHashes.TryGetValue(key, out var rowHash))
                    {
                        var hash = new HashCode();
                        AddToHash(ref hash, row);
                        rowHash = hash.ToHashCode();
                        rowHashes[key] = rowHash;
                    }

                    if (!PartSetIndex.TryGetValue(key, out var entry) || entry.Hash != rowHash)
                    {
                        var index = PartSetIndex.TryGetValue(key, out var existing) ? existing.Index : PartSets.Count;

                        if (index < LightSetTable.MaxSets)
                        {
                            var lighting = Resolve(map.Name, new DrawParamRows(part.LightID, 0, 0, 0, 0));

                            if (index == PartSets.Count)
                                PartSets.Add(lighting);
                            else
                                PartSets[index] = lighting;

                            entry = (index, rowHash);
                            PartSetIndex[key] = entry;
                        }
                        else
                        {
                            entry = (-1, rowHash);
                        }
                    }

                    if (entry.Index >= 0)
                    {
                        set = (byte)(entry.Index + 1);
                        used.Add(key);
                        parts++;
                    }
                }

                proxy.LightSet = set;

                if (set != 0)
                    stillLit.Add(proxy);
            }
        }

        // Parts of maps no longer loaded, or now on the scene's row, go back to the scene's lighting
        foreach (var proxy in LitProxies)
        {
            if (!stillLit.Contains(proxy))
                proxy.LightSet = 0;
        }

        LitProxies.Clear();
        LitProxies.UnionWith(stillLit);

        // Sets no part uses any more are rebuilt from nothing once all of them are unused, so indices stay stable
        if (used.Count == 0 && PartSets.Count > 0)
        {
            PartSets.Clear();
            PartSetIndex.Clear();
        }

        PartLightingCounts = (parts, used.Count);

        return PartSets;
    }

    private void ClearPartLighting()
    {
        foreach (var proxy in LitProxies)
            proxy.LightSet = 0;

        LitProxies.Clear();
        PartSets.Clear();
        PartSetIndex.Clear();
        PartLightingCounts = (0, 0);
    }

    private static void AddToHash(ref HashCode hash, Param.Row row)
    {
        if (row == null)
            return;

        hash.Add(row.ID);

        // Through the columns: Row.Cells builds a new list each time, and this runs every frame. Padding fields
        // come back as a new byte array each time, which would hash differently every frame.
        foreach (var column in row.Columns)
        {
            if (column.GetValue(row) is var value and not Array)
                hash.Add(value);
        }
    }

    /// <summary>
    /// The lighting of a map from the given rows of its area's banks, with the model set in
    /// <see cref="GparamLighting.Models" />.
    /// <para>
    /// Colours are 0-255 with colA an intensity in percent, so a GPARAM-style colour is rgb / 255 with w = colA / 100.
    /// Rotations are degrees, the light's elevation then yaw (see <see cref="Angle" />).
    /// </para>
    /// <list type="bullet">
    /// <item>LightBank: light 0 is the sun, lights 1 and 2 the fills (Light1, LocalLight1); the _s light's colour is
    /// the sun's specular, its own direction unused for now. _u and _d are the hemisphere, for the scene and models
    /// alike. DES's _du/_dd pair is unmapped.</item>
    /// <item>ToneMapBank: the exposure, its adaptation and the bloom, see <see cref="ApplyToneMap" />.</item>
    /// <item>ShadowBank: the shadows' own light, their density and range, see <see cref="ApplyShadow" />.</item>
    /// <item>ToneCorrectBank: the colour correction, see <see cref="ApplyToneCorrect" />.</item>
    /// </list>
    /// </summary>
    public GparamLighting Resolve(string mapId, DrawParamRows rows)
    {
        var profile = Profile;
        var area = GetArea(mapId);

        var lighting = new GparamLighting
        {
            PostEffects = profile.CreateDefaults()
        };

        // DES has a bloom (ToneMapBank) but none of the GPARAM games' glow filter
        lighting.PostEffects.Glow.Enabled = false;

        lighting.File = $"{area} Light {rows.Light}, ToneMap {rows.ToneMap}, Shadow {rows.Shadow}, ToneCorrect {rows.ToneCorrect}, Fog {rows.Fog}";

        string Source(string bank, Param.Row row) => $"{area}_{bank} #{row.ID} '{row.Name}'";

        var lightRow = GetBank(mapId, LightBank)?[rows.Light];
        if (lightRow != null)
        {
            ApplyLights(lightRow, lighting);
            ApplyEnv(lightRow, area, lighting.Env);
            foreach (var field in LightSourceFields)
                lighting.Sources[field] = Source(LightBank, lightRow);
        }

        var toneMapRow = GetBank(mapId, ToneMapBank)?[rows.ToneMap];
        if (toneMapRow != null)
        {
            ApplyToneMap(toneMapRow, lighting);
            lighting.Sources[profile.ExposureField] = Source(ToneMapBank, toneMapRow);
            lighting.Sources[profile.AutoExposureField] = Source(ToneMapBank, toneMapRow);
        }

        var shadowRow = GetBank(mapId, ShadowBank)?[rows.Shadow];
        if (shadowRow != null)
            ApplyShadow(shadowRow, lighting.Shadow);

        var toneCorrectRow = GetBank(mapId, ToneCorrectBank)?[rows.ToneCorrect];
        if (toneCorrectRow != null)
            ApplyToneCorrect(toneCorrectRow, lighting.ToneCorrection);

        var fogRow = GetBank(mapId, FogBank)?[rows.Fog];
        if (fogRow != null)
            ApplyFog(fogRow, lighting.PostEffects.DistanceFog);

        // The model set, the game's HemDir3 shaders (MTD g_LightingType 1: characters and parts): the three lights and
        // the _du/_dd hemisphere. The scene's set is that of the HemEnv shaders (g_LightingType 3, the map), which take
        // the _u/_d hemisphere and the env cubemaps and leave the lights out; the shader does that per material.
        var models = new GparamLighting
        {
            File = lighting.File,
            PostEffects = lighting.PostEffects,
            Shadow = lighting.Shadow,
            ToneCorrection = lighting.ToneCorrection
        };

        if (lightRow != null)
        {
            ApplyLights(lightRow, models);
            models.HemiUp = Color(lightRow, "_du");
            models.HemiDown = Color(lightRow, "_dd");
        }

        models.Exposure = lighting.Exposure;
        models.AutoExposure = lighting.AutoExposure;
        models.MiddleGray = lighting.MiddleGray;

        lighting.Models = models;

        return lighting;
    }

    /// <summary>The GPARAM field keys the lighting table names a source for, all from the LightBank row</summary>
    private static readonly string[] LightSourceFields =
    {
        "Directional Light DiffColor0", "Directional Light SpecColor0", "Directional Light Angle0",
        "Directional Light DiffColor1", "Directional Light LocalLight DiffColor1",
        "Directional Light LocalLight DiffColor2", "Directional Light Hemi Color Up",
        "Directional Light Hemi Color Down"
    };

    private static void ApplyLights(Param.Row row, GparamLighting lighting)
    {
        ReadLight(row, "0", lighting.Sun);
        ReadLight(row, "1", lighting.Light1);
        ReadLight(row, "2", lighting.LocalLight1);
        lighting.LocalLight2.Enabled = false;

        // The specular light's colour; DES gives it its own direction, which the renderer has no place for yet
        lighting.Sun.Specular = Color(row, "_s");

        lighting.HemiUp = Color(row, "_u");
        lighting.HemiDown = Color(row, "_d");
    }

    /// <summary>
    /// LightBank's env cubemaps: envDif and envSpc_0-3 are IDs of the cubemaps EnvDif_mAA_NNN and EnvSpc_mAA_NNN in
    /// the area's map/mAA/mAA_9999.tpf (every ID of every row resolves), envDif_col* and envSpc_col* their scales,
    /// colA a percentage as elsewhere. The four specular maps are one light rig at four specular powers, and each
    /// material reads the one its MTD's g_EnvSpcSlotNo names.
    /// </summary>
    private static void ApplyEnv(Param.Row row, string area, GparamEnvLighting env)
    {
        env.Enabled = true;
        env.DiffuseTexture = $"EnvDif_{area}_{(int)Get(row, "envDif"):000}";

        for (var i = 0; i < 4; i++)
            env.SpecularTextures[i] = $"EnvSpc_{area}_{(int)Get(row, $"envSpc_{i}", i):000}";

        env.DiffuseColor = EnvColor(row, "envDif_");
        env.SpecularColor = EnvColor(row, "envSpc_");
    }

    /// <summary>An env scale, prefix colR/G/B/A, as rgb / 255 and w = colA / 100</summary>
    private static Vector4 EnvColor(Param.Row row, string prefix)
    {
        return new Vector4(
            Get(row, $"{prefix}colR", 255.0f) / 255.0f,
            Get(row, $"{prefix}colG", 255.0f) / 255.0f,
            Get(row, $"{prefix}colB", 255.0f) / 255.0f,
            Get(row, $"{prefix}colA", 100.0f) / 100.0f);
    }

    private static void ReadLight(Param.Row row, string index, GparamDirectionalLight light)
    {
        light.Angle = Angle(Get(row, $"degRotX_{index}"), Get(row, $"degRotY_{index}"));
        light.Diffuse = Color(row, $"_{index}");
        light.Specular = Vector4.Zero;
        light.Enabled = light.Diffuse.W > 0.0f;
        light.FollowCamera = false;
    }

    /// <summary>
    /// ToneMapBank: grayKeyValue is the key, minAdaptedLum and maxAdapredLum (sic) bound the adapted average luminance,
    /// adaptSpeed is how fast it adapts (per second), bloomBegin and bloomMul start and scale the bloom (percent).
    /// </summary>
    private static void ApplyToneMap(Param.Row row, GparamLighting lighting)
    {
        var key = Math.Max(Get(row, "grayKeyValue", 0.18f), 1e-4f);
        var minLum = Math.Max(Get(row, "minAdaptedLum", 0.1f), 1e-4f);
        var maxLum = Math.Max(Get(row, "maxAdapredLum", 0.2f), minLum);

        // The exposure key / L for L clamped to [min, max], as AutoExposure.comp's base x 2^ev: the base is the
        // middle of the range, and ev runs from key / max to key / min about it
        var baseExposure = key / MathF.Sqrt(minLum * maxLum);
        lighting.Exposure = baseExposure;
        lighting.AutoExposure = true;
        lighting.MiddleGray = MathF.Log2(key / baseExposure);

        var adapt = lighting.PostEffects.AutoExposure;
        adapt.AdaptionMin = MathF.Log2(key / maxLum / baseExposure);
        adapt.AdaptionMax = MathF.Log2(key / minLum / baseExposure);

        // AutoExposure.comp moves by speed per 1/30 s, so a rate of k per second is 1 - e^(-k / 30)
        var speed = 1.0f - MathF.Exp(-Math.Max(Get(row, "adaptSpeed", 1.0f), 0.0f) / 30.0f);
        adapt.LightAdaptSpeed = speed;
        adapt.DarkAdaptSpeed = speed;

        // Scaled so DES's usual 20 / 50 give the GPARAM defaults (threshold 1, alpha 0.25); a guess to match by eye
        var bloom = lighting.PostEffects.Bloom;
        var bloomMul = Get(row, "bloomMul", 50.0f);
        bloom.Enabled = bloomMul > 0.0f;
        bloom.Threshold = Get(row, "bloomBegin", 20.0f) / 20.0f;
        bloom.Alpha = bloomMul / 200.0f;
    }

    /// <summary>
    /// ShadowBank: the shadows' own light (lightDegRotX/Y, an elevation and yaw as LightBank's), how dark they are
    /// (densityRatio, percent), and how far they reach: from beginDist to endDist, fading out from fadeBeginDist over
    /// fadeDist. DES shadows are short: m01's end at 10 m, m02's at 40. The rows' colour (colR/G/B, 0-80) is unmapped.
    /// </summary>
    private static void ApplyShadow(Param.Row row, GparamShadow shadow)
    {
        shadow.LightAngle = Angle(Get(row, "lightDegRotX"), Get(row, "lightDegRotY"));
        shadow.Density = Math.Clamp(Get(row, "densityRatio", 100.0f) / 100.0f, 0.0f, 1.0f);

        var near = Math.Max(Get(row, "beginDist"), 0.1f);
        var endDist = Get(row, "endDist", 300.0f);
        var fadeBegin = Get(row, "fadeBeginDist", 9999.0f);
        var fadeDist = Get(row, "fadeDist", -1.0f);

        // Most rows end where the fade does (m02: 30 + 10 = 40); where they don't, the end is the nearer of the two
        var end = endDist > 0.0f ? endDist : fadeBegin + Math.Max(fadeDist, 0.0f);
        var fadeStart = Math.Min(fadeBegin, end);

        if (fadeStart >= end && fadeDist > 0.0f)
            fadeStart = Math.Max(end - fadeDist, near);

        if (end <= near)
        {
            shadow.CascadeCount = 0;
            return;
        }

        shadow.Near = near;
        shadow.FarFadeStart = fadeStart;
        shadow.FarFadeDistance = end - fadeStart;

        // ER's splits are for kilometres; these ranges are tens of metres, so the cascades split the range itself
        shadow.CascadeSplits = new Vector3(end * 0.1f, end * 0.3f, end * 0.6f);
    }

    /// <summary>
    /// ToneCorrectBank: brightness and contrast per channel, saturation and hue (degrees, 0 or 356 in DES's rows).
    /// Taken as a correction of the display colour, see ToneMap.frag.
    /// </summary>
    private static void ApplyToneCorrect(Param.Row row, GparamToneCorrection correction)
    {
        correction.Enabled = true;
        correction.Brightness = new Vector3(Get(row, "brightnessR", 1.0f), Get(row, "brightnessG", 1.0f), Get(row, "brightnessB", 1.0f));
        correction.Contrast = new Vector3(Get(row, "contrastR", 1.0f), Get(row, "contrastG", 1.0f), Get(row, "contrastB", 1.0f));
        correction.Saturation = Get(row, "saturation", 1.0f);
        correction.Hue = Get(row, "hue");
    }

    /// <summary>
    /// FogBank: fogBeginZ and fogEndZ in meters (begin can be negative, so the fog has started at the camera), degRotW
    /// taken as the density at the end in percent (0-200 in DES's rows; degRotZ is always 0), and the colour.
    /// The colour is taken as a display colour, so squared into the target's linear light as the viewport's gamma of
    /// 2 has it, with colA its intensity in percent. A reading of the names, not the game's shader.
    /// </summary>
    private static void ApplyFog(Param.Row row, GparamDistanceFog fog)
    {
        fog.Begin = Get(row, "fogBeginZ");
        fog.End = Get(row, "fogEndZ", 100.0f);
        fog.MaxDensity = Math.Clamp(Get(row, "degRotW", 100.0f) / 100.0f, 0.0f, 1.0f);

        var r = Get(row, "colR", 255.0f) / 255.0f;
        var g = Get(row, "colG", 255.0f) / 255.0f;
        var b = Get(row, "colB", 255.0f) / 255.0f;
        fog.Color = new Vector4(r * r, g * g, b * b, Get(row, "colA", 100.0f) / 100.0f);

        fog.Enabled = fog.MaxDensity > 0.0f && fog.End > fog.Begin;
    }

    /// <summary>The colour colR/G/B/A + suffix as rgb / 255 and w = colA / 100</summary>
    private static Vector4 Color(Param.Row row, string suffix)
    {
        return new Vector4(
            Get(row, $"colR{suffix}", 255.0f) / 255.0f,
            Get(row, $"colG{suffix}", 255.0f) / 255.0f,
            Get(row, $"colB{suffix}", 255.0f) / 255.0f,
            Get(row, $"colA{suffix}", 0.0f) / 100.0f);
    }

    /// <summary>
    /// A DrawParam rotation as a GPARAM angle, whose pitch is that of the light's travel (negative: downwards).
    /// DrawParam's pitch is the light's elevation: the outdoor suns are 10 to 50, ShadowBank's light 35 to 70, and
    /// Stonefang's (m06) orange light, the lava's, is -60, from below. So the pitch is negated. The yaw is taken as it
    /// is, which is for checking in the app.
    /// </summary>
    private static Vector2 Angle(float pitchDegrees, float yawDegrees)
    {
        const float toRadians = MathF.PI / 180.0f;

        return new Vector2(-pitchDegrees * toRadians, yawDegrees * toRadians);
    }

    /// <summary>A field of a row by its internal name, as a float; <paramref name="fallback" /> if the row has no such field</summary>
    public static float Get(Param.Row row, string field, float fallback = 0.0f)
    {
        foreach (var column in row.Columns)
        {
            if (!column.Def.InternalName.Equals(field, StringComparison.OrdinalIgnoreCase))
                continue;

            try
            {
                return Convert.ToSingle(column.GetValue(row));
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        return fallback;
    }
}
