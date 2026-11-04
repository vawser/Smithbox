using StudioCore.Application;
using System.Collections.Generic;

namespace StudioCore.Renderer;

/// <summary>How a game's Tone Map group gives the exposure</summary>
public enum GparamExposureModel
{
    /// <summary>
    ///     <c>GS Presents - ToneMap-Exposure</c> as a linear multiplier (AC6, ER, NR: 1.75-4.5), auto exposure from the
    ///     GS Presents fields
    /// </summary>
    GsLinear,

    /// <summary>
    ///     <c>GS Presents - ToneMap-Exposure</c> in stops (Sekiro: -8 to 1.5, mostly 0), so 2^x is the multiplier;
    ///     auto exposure from the GS Presents fields
    /// </summary>
    GsStops,

    /// <summary>
    ///     The Yebis fields (DS3, BB), where the GS Presents exposure is 0: <c>Yebis-ToneMapExposure</c> linear (1.5-5.7),
    ///     <c>Yebis-AutoExposure</c>, <c>Yebis-MiddleGray</c> linear (0.01-0.028 in DS3), <c>Yebis-Min/MaxExposure</c> as
    ///     linear bounds of the exposure, and <c>Yebis-ToneType</c> for the curve
    /// </summary>
    Yebis
}

/// <summary>
///     What differs between the games whose maps are lit from GPARAMs: which files a field falls back to, how the Tone
///     Map group gives the exposure, which screen effects the game has, and where its color grading LUTs are.
///     See Documentation/Older Games/Analysis.md.
/// </summary>
public sealed class GparamProfile
{
    /// <summary>The game's map GPARAMs are read for the viewports' lighting</summary>
    public bool Supported { get; private init; }

    /// <summary>The shared GPARAMs a field is looked for in when the map's own file doesn't have it</summary>
    public string[] SharedFiles { get; private init; } = { "mapbase", "default" };

    public GparamExposureModel Exposure { get; private init; } = GparamExposureModel.GsLinear;

    /// <summary>The field the exposure is read from, for the lighting table's source column</summary>
    public string ExposureField => Exposure == GparamExposureModel.Yebis
        ? "Yebis-ToneMapExposure"
        : "GS Presents - ToneMap-Exposure";

    /// <summary>The field auto exposure is switched on by, for the lighting table's source column</summary>
    public string AutoExposureField => Exposure == GparamExposureModel.Yebis
        ? "Yebis-AutoExposure"
        : "GS Presents - ToneMap-Enable AutoExposure";

    /// <summary>The game has SSR ParamEditor (SDT onwards)</summary>
    public bool HasSsr { get; private init; } = true;

    /// <summary>The game has VolumetricFog ParamEditor (SDT onwards)</summary>
    public bool HasVolumetricFog { get; private init; } = true;

    /// <summary>The game's LightShaft[Yebis] has the GS Presents fields LightShaftPass reads (SDT onwards). DS3 and BB have the older Yebis ones.</summary>
    public bool HasGsLightShaft { get; private init; } = true;

    /// <summary>The game has TAA, Anti-Alias Type 5 (SDT onwards). DS3 and BB use only type 4.</summary>
    public bool HasTaa { get; private init; } = true;

    /// <summary>
    ///     The GPARAMs key their values by the seven timezones of <see cref="GparamLighting.TimeKeys" />, so the
    ///     viewport menu offers them as stops
    /// </summary>
    public bool HasTimezones { get; private init; }

    /// <summary>
    ///     The game lights its models with BTPB light probes (DS3, Bloodborne), which the shaders use in place of the
    ///     GPARAM hemisphere inside the probe grids (see LightProbeGridBank). DS3's shaders have no hemisphere term.
    /// </summary>
    public bool HasLightProbes { get; private init; }

    /// <summary>
    ///     The game lights its map pieces with the DOL lightmaps of map/mAA/gi_env_mAA.tpfbhd (DS3), see DolLightmapBank
    /// </summary>
    public bool HasDolLightmaps { get; private init; }

    /// <summary>
    ///     A scale on the DOL lightmaps' light, for the part of it the game's code sets (how the TPF's FloatStruct becomes
    ///     FC_TexHdrScaleParam) and that isn't traced. 1 until matched against the game.
    /// </summary>
    public float LightmapScale { get; private init; } = 1.0f;

    /// <summary>
    ///     A scale on the volumetric fog's density (after its GameScale), for the part of it the game's code sets and
    ///     that isn't traced. Sekiro: 0.1, matched by eye against the game in m11_00_00_00 (2026-11-04).
    /// </summary>
    public float FogDensityScale { get; private init; } = 1.0f;

    /// <summary>
    ///     A scale on the bloom (over GlareBloom Alpha), for g_vBloomFinalColor, which the game's code sets and that
    ///     isn't traced. Sekiro: 0.5, matched by eye against the game in m11_00_00_00 (2026-11-04).
    /// </summary>
    public float BloomScale { get; private init; } = 1.0f;

    /// <summary>
    ///     The path of an area's color grading TPF, with {0} the area number (see <see cref="ColorGradingLuts" />). Null if
    ///     the game's LUTs can't be read.
    /// </summary>
    public string LutPathFormat { get; private init; }

    private GparamPostEffects DefaultEffects;

    /// <summary>
    ///     The screen effects when there is no GPARAM to read, and the starting values a GPARAM's fields replace: the
    ///     effects the game doesn't have are off. Shared; not to be changed.
    /// </summary>
    public GparamPostEffects Defaults => DefaultEffects ??= CreateDefaults();

    /// <summary>A fresh copy of <see cref="Defaults" />, to resolve a GPARAM into</summary>
    public GparamPostEffects CreateDefaults()
    {
        var effects = new GparamPostEffects();

        if (!HasSsr)
            effects.Ssr.Enabled = false;

        if (!HasVolumetricFog)
            effects.VolumetricFog.Enabled = false;

        if (!HasGsLightShaft)
            effects.LightShaft.Enabled = false;

        // Type 4, which DS3 and BB use, is drawn as FXAA
        if (!HasTaa)
            effects.AntiAlias.Type = 4;

        return effects;
    }

    private const string LutInCommon = "/map/m{0:D2}/common/m{0:D2}_cgrading.tpf.dcx";
    private const string LutInArea = "/map/m{0:D2}/m{0:D2}_cgrading.tpf.dcx";

    private static readonly Dictionary<ProjectType, GparamProfile> Profiles = new();

    private static readonly GparamProfile Unknown = new();

    /// <summary>
    ///     The games before Sekiro, which have no SSR, volumetric fog or TAA, so they aren't given them when there is no
    ///     GPARAM either
    /// </summary>
    private static readonly GparamProfile Older = new()
    {
        HasSsr = false,
        HasVolumetricFog = false,
        HasGsLightShaft = false,
        HasTaa = false
    };

    public static GparamProfile For(ProjectType? type)
    {
        if (type == null)
            return Unknown;

        lock (Profiles)
        {
            if (!Profiles.TryGetValue(type.Value, out var profile))
            {
                profile = Create(type.Value);
                Profiles[type.Value] = profile;
            }

            return profile;
        }
    }

    /// <summary>The profile of the selected project, for the renderer's passes</summary>
    public static GparamProfile Current => For(Smithbox.Orchestrator?.SelectedProject?.Descriptor.ProjectType);

    private static GparamProfile Create(ProjectType type) => type switch
    {
        ProjectType.AC6 or ProjectType.ER or ProjectType.NR => new GparamProfile
        {
            Supported = true,
            HasTimezones = true,
            LutPathFormat = LutInCommon
        },
        // Sekiro has Elden Ring's groups, with the exposure in stops and the LUTs beside the area's map folders
        ProjectType.SDT => new GparamProfile
        {
            Supported = true,
            Exposure = GparamExposureModel.GsStops,
            LutPathFormat = LutInArea,
            FogDensityScale = 0.1f,
            BloomScale = 0.5f
        },
        ProjectType.DS3 => new GparamProfile
        {
            Supported = true,
            HasLightProbes = true,
            HasDolLightmaps = true,
            Exposure = GparamExposureModel.Yebis,
            HasSsr = false,
            HasVolumetricFog = false,
            HasGsLightShaft = false,
            HasTaa = false,
            LutPathFormat = LutInArea
        },
        // Bloodborne's LUTs are PS4 textures without a DDS header, which ColorGradingLuts can't read; it has no mapbase
        ProjectType.BB => new GparamProfile
        {
            Supported = true,
            SharedFiles = new[] { "default" },
            HasLightProbes = true,
            Exposure = GparamExposureModel.Yebis,
            HasSsr = false,
            HasVolumetricFog = false,
            HasGsLightShaft = false,
            HasTaa = false
        },
        ProjectType.DES or ProjectType.DS1 or ProjectType.DS1R or ProjectType.DS2 or ProjectType.DS2S => Older,
        _ => Unknown
    };
}
