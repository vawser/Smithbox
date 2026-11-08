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
/// Lights the Map Editor's viewports of a game without GPARAMs from its DrawParam banks (Demon's Souls), filling the
/// same <see cref="GparamLighting" /> the renderer reads for the GPARAM games.
/// <para>
/// A map mAA_BB_CC_DD uses the banks of its area, the params mAA_LightBank, mAA_ToneMapBank... of
/// param/drawparam/aAA_drawparam.parambnd, which the Param Editor's bank already holds. Map parts pick a row of each
/// bank by their LightID, ToneMapID... (MSBD.Part). Phase 1 lights the whole viewport with one row per bank: the one
/// most of the map's pieces use, or the one the viewport menu chose. Fields are read by name and converted to float,
/// so the s32 fields of DES and the s16 ones of DS1 read alike.
/// </para>
/// See Documentation/Claude/Older Games/DES DrawParam Lighting Plan.md.
/// </summary>
public class DrawParamLightingBank
{
    public ProjectEntry Project;

    public const string LightBank = "LightBank";
    public const string ToneMapBank = "ToneMapBank";

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

    // The most-used IDs of each map, worked out again every second or so, as parts can be edited
    private readonly Dictionary<string, (int Light, int ToneMap, long Time)> MostUsed = new();
    private readonly Stopwatch Clock = Stopwatch.StartNew();

    /// <summary>
    /// The LightBank and ToneMapBank IDs most of a map's pieces use, the rows Phase 1 lights the map with
    /// </summary>
    public (int Light, int ToneMap) GetMostUsedIds(MapContainer map)
    {
        var now = Clock.ElapsedMilliseconds;

        if (MostUsed.TryGetValue(map.Name, out var cached) && now - cached.Time < 1000)
            return (cached.Light, cached.ToneMap);

        var pieces = map.Parts
            .Select(e => e.WrappedObject)
            .OfType<MSBD.Part.MapPiece>()
            .ToList();

        int MostUsedId(Func<MSBD.Part.MapPiece, byte> get) => pieces.Count == 0
            ? 0
            : pieces.GroupBy(get).OrderByDescending(g => g.Count()).ThenBy(g => g.Key).First().Key;

        var light = MostUsedId(p => p.LightID);
        var toneMap = MostUsedId(p => p.ToneMapID);

        MostUsed[map.Name] = (light, toneMap, now);

        return (light, toneMap);
    }

    /// <summary>The row a map is lit with from a bank: the menu's choice if that row exists, else the most-used one</summary>
    public int ChooseRow(MapContainer map, string bank)
    {
        var chosen = bank == LightBank ? CFG.Current.Viewport_Map_DrawParam_Light_Row : CFG.Current.Viewport_Map_DrawParam_ToneMap_Row;
        var param = GetBank(map.Name, bank);

        if (chosen >= 0 && param?[chosen] != null)
            return chosen;

        var (light, toneMap) = GetMostUsedIds(map);

        return bank == LightBank ? light : toneMap;
    }

    // The last lighting given to the viewports, and what it was resolved from
    private GparamLighting ViewportLighting;
    private (string Map, int Light, int ToneMap, int Hash) ViewportKey;

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

        var map = maps
            .OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(m => GetBank(m.Name, LightBank) != null);

        if (map == null)
            return null;

        return ResolveCached(map);
    }

    /// <summary>The lighting of one map with the rows <see cref="ChooseRow" /> picks, cached as <see cref="ResolveForViewport" /> describes</summary>
    public GparamLighting ResolveCached(MapContainer map)
    {
        var lightId = ChooseRow(map, LightBank);
        var toneMapId = ChooseRow(map, ToneMapBank);
        var lightRow = GetBank(map.Name, LightBank)?[lightId];
        var toneMapRow = GetBank(map.Name, ToneMapBank)?[toneMapId];

        var key = (map.Name, lightId, toneMapId, HashRows(lightRow, toneMapRow));

        if (ViewportLighting != null && ViewportKey == key)
            return ViewportLighting;

        ViewportLighting = Resolve(map.Name, lightId, toneMapId);
        ViewportKey = key;

        return ViewportLighting;
    }

    private static int HashRows(Param.Row a, Param.Row b)
    {
        var hash = new HashCode();

        foreach (var row in new[] { a, b })
        {
            if (row == null)
                continue;

            hash.Add(row.ID);

            // Through the columns: Row.Cells builds a new list each time, and this runs every frame
            // Padding fields come back as a new byte array each time, which would hash differently every frame
            foreach (var column in row.Columns)
            {
                if (column.GetValue(row) is var value and not Array)
                    hash.Add(value);
            }
        }

        return hash.ToHashCode();
    }

    /// <summary>
    /// The lighting of a map from LightBank row <paramref name="lightId" /> and ToneMapBank row
    /// <paramref name="toneMapId" /> of its area, with the model set in <see cref="GparamLighting.Models" />.
    /// <para>
    /// Colours are 0-255 with colA an intensity in percent, so a GPARAM-style colour is rgb / 255 with w = colA / 100.
    /// Rotations are degrees, the light's elevation then yaw (see <see cref="Angle" />).
    /// </para>
    /// <list type="bullet">
    /// <item>Light 0 is the sun, lights 1 and 2 the fills (Light1, LocalLight1); the _s light's colour is the sun's
    /// specular, its own direction unused for now.</item>
    /// <item>_u and _d are the hemisphere, for the scene and models alike. DES's _du/_dd pair is unmapped.</item>
    /// <item>ToneMapBank's Reinhard adaptation, key / clamp(average luminance, min, max), becomes the base exposure
    /// key / sqrt(min x max) with auto exposure between the two bounds, see <see cref="ApplyToneMap" />.</item>
    /// </list>
    /// </summary>
    public GparamLighting Resolve(string mapId, int lightId, int toneMapId)
    {
        var profile = Profile;
        var area = GetArea(mapId);

        var lighting = new GparamLighting
        {
            PostEffects = profile.CreateDefaults()
        };

        // DES has a bloom (ToneMapBank) but none of the GPARAM games' glow filter
        lighting.PostEffects.Glow.Enabled = false;

        var lightBank = GetBank(mapId, LightBank);
        var lightRow = lightBank?[lightId];
        var toneMapRow = GetBank(mapId, ToneMapBank)?[toneMapId];

        lighting.File = $"{area}_{LightBank} {lightId}, {area}_{ToneMapBank} {toneMapId}";

        if (lightRow != null)
        {
            var source = $"{area}_{LightBank} #{lightId} '{lightRow.Name}'";

            ApplyLights(lightRow, lighting);
            foreach (var field in LightSourceFields)
                lighting.Sources[field] = source;
        }

        if (toneMapRow != null)
        {
            ApplyToneMap(toneMapRow, lighting);

            var source = $"{area}_{ToneMapBank} #{toneMapId} '{toneMapRow.Name}'";
            lighting.Sources[profile.ExposureField] = source;
            lighting.Sources[profile.AutoExposureField] = source;
        }

        // The model set: the same lights, which characters take all of (the scene takes only the sun and hemisphere)
        var models = new GparamLighting
        {
            File = lighting.File,
            PostEffects = lighting.PostEffects
        };

        if (lightRow != null)
            ApplyLights(lightRow, models);

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
