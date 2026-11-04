using System.Numerics;

namespace StudioCore.Renderer;

/// <summary>
/// The screen effects of a map's GPARAMs, at one time of day: what the post-processing passes read. Resolved with
/// <see cref="GparamLighting" />, through the same chain of files, at value ID 0 except where noted. Values that
/// aren't found keep these defaults, which are Elden Ring's mapbase (or m10_00_0000 for the groups mapbase leaves
/// out). Nothing is drawn from these yet; see Documentation/Post Processing/Plan.md for which phase uses each.
/// </summary>
public class GparamPostEffects
{
    public GparamAutoExposure AutoExposure = new();
    public GparamBloom Bloom = new();
    public GparamGlow Glow = new();
    public GparamLightShaft LightShaft = new();
    public GparamVolumetricFog VolumetricFog = new();
    public GparamSsao Ssao = new();
    public GparamSsr Ssr = new();
    public GparamAntiAlias AntiAlias = new();
    public GparamColorGrading ColorGrading = new();
    public GparamWet Wet = new();
}

/// <summary>
/// Tone Map's auto exposure, beside <see cref="GparamLighting.AutoExposure" /> and
/// <see cref="GparamLighting.MiddleGray" />. Adaption Min/Max are taken to be EV around the exposure.
/// </summary>
public class GparamAutoExposure
{
    public float LightAdaptSpeed = 0.25f;
    public float DarkAdaptSpeed = 0.25f;
    public float CenterWeight = 7.0f;
    public float AdaptionMax = 0.4f;
    public float AdaptionMin = -0.5f;
}

/// <summary>Bloom[Yebis], whose fields are named Glare. Some files switch it per value ID rather than at ID 0.</summary>
public class GparamBloom
{
    public bool Enabled = true;
    public int NumLevels = 5;
    public float Luminance = 1.0f;
    public float Threshold = 1.0f;
    public float Alpha = 0.25f;
    public float GaussianRadiusScale = 1.0f;
    public float SaturateControl = 0.0f;
    public float Smooth = 1.0f;
}

/// <summary>Glow Filter-Editor: a second blur chain, which seems to take an emissive mask from the alpha</summary>
public class GparamGlow
{
    public bool Enabled = true;
    public int NumLevels = 7;
    public float BlurSmoothness = 1.2f;
    public float FinalColorScale = 5.0f;
    public float BlurSampleScale = 1.0f;
    public float BlurRgbScale = 1.5f;
    public float BlurAlphaScale = 1.0f;
}

/// <summary>
/// LightShaft[Yebis]. <see cref="Angle" /> is in the LightSet's convention; with <see cref="EnableRefLight" /> the
/// shafts come from the light <see cref="LightBankId" /> / <see cref="LightId" /> names instead.
/// </summary>
public class GparamLightShaft
{
    public bool Enabled = true;
    public Vector2 Angle = new(-1.570796f, 0.0f);
    public bool EnableRefLight = false;
    public int LightBankId = 0;
    public int LightId = 0;
    public bool HighQualitySkyMask = false;
    public int SkyMaskMode = 0;
    public float MaskDepthThreshold = 1000.0f;
    public float SkyMaskSizeAngle = 1.0471976f;
    public float SkyMaskCenterPower = 2.0f;
    public Vector4 SkyMultiplyColor = Vector4.One;
    public Vector4 SkyAddColor = Vector4.Zero;
    public float SkyColorSaturate = 0.0f;
    public float SkyColorThreshold = 0.0f;
    public float ExposureInfluenceAddColor = 0.0f;
    public float ParallelBeginAngle = 0.0f;
    public float ParallelEndAngle = 0.0f;
    public float FadeOutBeginAngle = 0.0f;
    public float FadeOutEndAngle = 0.0f;
    public float BlurRange = 1.5f;
    public float BlurRangeParallel = 0.2f;
    public float BlurFalloff = 1.0f;
    public int BlurCount = 2;
}

/// <summary>
/// VolumetricFog ParamEditor: a froxel volume of <see cref="VolumeWidth" /> cells across and
/// <see cref="VolumeDepth" /> slices between <see cref="NearDepth" /> and <see cref="FarDepth" />, and the global
/// height fog in it. Colors are rgb with an intensity in w, in the game's light units.
/// </summary>
public class GparamVolumetricFog
{
    public bool Enabled = true;
    /// <summary>
    ///     VolumetricFog GameScale: 1 in Nightreign, 1-12 through the day in Sekiro. Applied by the game's code, which
    ///     isn't traced; taken to divide the density (see VolumetricFogPass)
    /// </summary>
    public float GameScale = 1.0f;
    public int VolumeWidth = 96;
    public int VolumeDepth = 24;
    public float NearDepth = 0.1f;
    public float FarDepth = 300.0f;
    public float FogEndDepth = 300.0f;
    public bool EnableAnalyticalFog = false;
    public float FixedDensity = 1e-05f;
    public float ScatteringG = 0.2f;
    public float LocalLightScale = 1.0f;
    public float LocalLightDistScale = 0.5f;
    public float HeightFogDistribution = 0.06f;
    public float HeightFogOffset = 0.0f;
    public float HeightFogDensityScale = 0.004f;
    public Vector3 AbsorptionScale = Vector3.Zero;
    public Vector4 SkyColor = new(0.490196f, 0.54902f, 0.509804f, 0.05f);
    public float SkyHeightAtten = 0.0f;
    public float SkyStartDistance = 3.0f;
    public int DirLightBankId = 160000000;
    public int DirLightId = 0;
    public Vector4 DirectColor = new(0.901961f, 0.980392f, 0.843137f, 0.2f);
    public Vector4 OppositeColor = new(1.0f, 1.0f, 1.0f, 0.05f);
    public float DirHeightAtten = 0.0f;
    public float DirStartDistance = 3.0f;
    public int ShadowSampleCount = 8;
    public int SampleCount = 8;
}

/// <summary>
/// SSAO Editor, which has no switch of its own, and the LightSet's per light SsaoWeight0-3 (sun, Light1,
/// LocalLight1, LocalLight2), read at the LightSet ID
/// </summary>
public class GparamSsao
{
    public bool UseNormal = true;
    public bool UseReprojection = true;
    public float SampleOffset = 5.0f;
    public float Scale = 1.5f;
    public float Offset = -0.25f;
    public float KernelBaseDistance = 100.0f;
    public float KernelMin = 2.0f;
    public float KernelMax = 5.0f;
    public float DepthFadeStart = 3800.0f;
    public float DepthFadeRange = 100.0f;
    public float PrecisionDepth = 4000.0f;
    public float DepthRejectRate = 0.02f;
    public float GeomBentOffset = 0.0f;
    public Vector4 LightWeights = new(1.0f, 0.0f, 0.0f, 0.0f);
}

/// <summary>SSR ParamEditor</summary>
public class GparamSsr
{
    public bool Enabled = true;
    public int TraverseMode = 0;
    public float ShininessRejectThreshold = 0.35f;
    public float DepthRejectThreshold = 10000.0f;
    public float FadeToViewerStart = 0.1f;
    public float FresnelRejectThreshold = 0.15f;
    public float StepScaleMin = 6.0f;
    public float StepScaleMax = 10.0f;
    public float MipScale = 1.0f;
    public float MipOffset = 0.0f;
    public float DepthFadeRange = 10.0f;
    public float CrackDetectFactor = 1.02f;
    public float BlurSigma = 0.0f;
    public float BlurSigmaMip = 4.0f;
}

/// <summary>Anti-Alias-Editor. Elden Ring's mapbase has <see cref="Type" /> 5 and default 4; their meaning isn't known.</summary>
public class GparamAntiAlias
{
    public bool Enabled = true;
    public int Type = 5;
    public float FxaaAlpha = 1.0f;
    public float FxaaReduceMin = 0.0078125f;
    public float FxaaReduceMul = 0.125f;
    public float FxaaSpanMax = 8.0f;
    public float FeedbackWeight = 0.95f;
    public int JitterCount = 8;
    public float SharpenIntensity = 0.25f;
    public float SharpenRangeMin = -2.0f;
    public float SharpenRangeMax = 2.0f;

    /// <summary>FBWeightBorderDepth(0-2): the view depths between the four bands of FBWeightCancelByDepth</summary>
    public Vector3 BorderDepth = new(60.0f, 250.0f, 800.0f);

    /// <summary>FBWeightCancelByDepth(0-3): how much of FeedbackWeight each depth band takes away, nearest first</summary>
    public Vector4 CancelByDepth = new(0.0f, 0.2f, 0.4f, 0.8f);
}

/// <summary>
/// ColorGrading[Yebis]: a LUT by ID, which mapbase leaves off (-1), see <see cref="ColorGradingLuts" />. LutSourceId
/// can change through the day; between two keys the LUTs either side are blended.
/// </summary>
public class GparamColorGrading
{
    public bool Enabled = false;

    /// <summary>The ID at the key before the time of day</summary>
    public int LutSourceId = -1;

    /// <summary>The ID at the key after it, the same as <see cref="LutSourceId" /> with one key</summary>
    public int NextLutSourceId = -1;

    /// <summary>How far from <see cref="LutSourceId" /> towards <see cref="NextLutSourceId" /></summary>
    public float Blend = 0.0f;

    public float FadeTime = 1.0f;

    /// <summary>The LUTs the IDs are looked up in, null if there are none</summary>
    public ColorGradingLuts Luts;

    /// <summary>The slots in <see cref="Luts" /> of the two IDs, -1 where there is no such LUT</summary>
    public int Slot = -1;
    public int NextSlot = -1;
}

/// <summary>
/// Wet ParamEditor. Only Elden Ring's weather overlays (m00_00_WWWW_weatherbase, _weatheroutdoor) set the rates, so
/// they are 0 without a weather. Wet surfaces lean towards OverrideAlbedo by AlbedoRate and towards
/// OverrideShininess by ShininessRate, by <see cref="Wetness" />.
/// </summary>
public class GparamWet
{
    public Vector4 OverrideAlbedo = new(0.0f, 0.0f, 0.0f, 1.0f);
    public float OverrideShininess = 1.0f;
    public float AlbedoRate = 0.0f;
    public float ShininessRate = 0.0f;

    /// <summary>
    /// How wet surfaces are, from the chosen weather's WeatherParam (WetTime above 0), not the GPARAM: every weather's
    /// overlays carry the Wet values, wet or not
    /// </summary>
    public float Wetness = 0.0f;
}
