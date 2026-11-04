using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public unsafe struct SceneParam
{
    public Matrix4x4 Projection;
    public Matrix4x4 View;
    public Vector4 EyePosition;
    public Vector4 LightDirection;
    public fixed int CursorPosition[4];
    public uint EnvMap;
    public float AmbientLightMult;
    public float DirectLightMult;
    public float IndirectLightMult;
    public float EmissiveMapMult;
    public float SceneBrightness;
    public float SimpleFlver_Brightness;
    public float SimpleFlver_Saturation;
    public Vector4 SelectionColor;
    public Vector4 OutlineColor;
    public uint EnableDithering;
    public uint EnableTinting;
    public float DitherOpacity;

    // Lighting. These follow the fields above exactly as the shaders lay them out, with LightCount filling
    // the last four bytes of the 16 byte row that DitherOpacity is in.

    /// <summary>How many of <see cref="Lights" /> are in use</summary>
    public uint LightCount;

    /// <summary>Color (rgb) and intensity (a) of the sun, whose direction is <see cref="LightDirection" /></summary>
    public Vector4 SunColor;

    /// <summary>
    ///     Up to <see cref="SceneLightList.MaxLights" /> lights, each <see cref="SceneLightData.FloatCount" /> floats
    ///     laid out as <see cref="SceneLightData" />.
    /// </summary>
    public fixed float Lights[SceneLightList.MaxLights * SceneLightData.FloatCount];

    /// <summary>
    ///     Seconds of animation, see <see cref="MaterialClock" />. Kept at the end so that the shaders which
    ///     don't use it can leave it out, along with the lights before it.
    /// </summary>
    public float MaterialTime;

    /// <summary>Seconds a gust of wind takes to come around</summary>
    public float WindPeriod;

    /// <summary>What is switched on, see <see cref="MaterialFeatures" /></summary>
    public uint MaterialFeatures;

    /// <summary>The index of <see cref="SceneDepth" /> in the global texture pool, or 0 if there is none to read</summary>
    public uint SceneDepthTexture;

    /// <summary>xy is the world XZ direction the wind blows in, z its steady force and w the force of its waves</summary>
    public Vector4 Wind;

    /// <summary>How opaque collision and navmesh faces are, see <see cref="CollisionTransparency" /></summary>
    public float CollisionOpacity;

    /// <summary>The world height nav volumes are cut at, see <see cref="NavVolumeSlice" /></summary>
    public float NavVolumeSliceHeight;

    /// <summary>Whether nav volumes are cut, and shown with a cross-section, see <see cref="NavVolumeSlice" /></summary>
    public uint NavVolumeSliceFlags;

    /// <summary>The first entry of the <see cref="IrradianceVolumeTable" /> in the material buffer</summary>
    public uint IrradianceVolumeStart;

    /// <summary>How many irradiance volumes to light with, 0 outside the Map Editor</summary>
    public uint IrradianceVolumeCount;

    /// <summary>Scales the volumes' light, which is in the game's units, to the viewport's</summary>
    public float IrradianceVolumeExposure;

    /// <summary>1: scale the environment map's reflections by the volumes' light</summary>
    public uint IrradianceVolumeFlags;

    /// <summary>
    ///     1: the sun, and the ambient light and reflections outside the irradiance volumes, come from the map's
    ///     GPARAMs, see <see cref="GparamLighting" />. 2: the hemisphere also adds to the volumes' light.
    /// </summary>
    public uint MapLightingFlags;

    /// <summary>
    ///     Color (rgb) and intensity (a) of the sun's highlights. The same as <see cref="SunColor" /> unless the map's
    ///     GPARAM gives its own.
    /// </summary>
    public Vector4 SunSpecColor;

    /// <summary>The GPARAM's hemisphere light from above and from below, rgb already multiplied by intensity and exposure</summary>
    public Vector4 HemiUp;
    public Vector4 HemiDown;

    /// <summary>The GPARAM's EnvMapColorConvert scale for reflections, rgb already multiplied by w</summary>
    public Vector4 EnvMapScale;

    /// <summary>The GPARAM's EnvMapColorConvert saturation for reflections: 0 grey, 1 unchanged</summary>
    public float EnvMapSaturate;

    // Fills the last 16 byte row, as the shaders lay the struct out
    public float MapLightingPad0;
    public float MapLightingPad1;
    public float MapLightingPad2;

    /// <summary>
    ///     The model set of the map's GPARAM LightSet, which lights characters when <see cref="MapLightingFlags" />
    ///     has 4: its sun, then Light1, LocalLight1 and LocalLight2. Lights that are off have an intensity of 0.
    /// </summary>
    public MapDirectionalLightData ModelLight0;
    public MapDirectionalLightData ModelLight1;
    public MapDirectionalLightData ModelLight2;
    public MapDirectionalLightData ModelLight3;

    /// <summary>The model set's hemisphere light, as <see cref="HemiUp" /> and <see cref="HemiDown" /></summary>
    public Vector4 ModelHemiUp;
    public Vector4 ModelHemiDown;

    /// <summary>The first entry of <see cref="SceneRenderer.MapLights" /> in the material buffer</summary>
    public uint MapLightStart;

    /// <summary>How many of the map's lights to light with, 0 outside the Map Editor</summary>
    public uint MapLightCount;

    /// <summary>
    ///     The first entry of <see cref="SceneRenderer.Lightmaps" /> in the material buffer. Sits in what was padding,
    ///     so only FlverShader.frag, which reads it, names it; the other shaders' copies still see a pad.
    /// </summary>
    public uint LightmapStart;

    // Fills the last 16 byte row, as the shaders lay the struct out
    public uint MapLightPad1;

    /// <summary>
    ///     World to shadow map clip space of each cascade, see <see cref="ShadowCascades" />. The shadow map's depth
    ///     is reversed, as the viewport's is: 1 nearest the sun.
    /// </summary>
    public Matrix4x4 ShadowMatrix0;
    public Matrix4x4 ShadowMatrix1;
    public Matrix4x4 ShadowMatrix2;
    public Matrix4x4 ShadowMatrix3;

    /// <summary>How far from the camera, along its view, each cascade ends</summary>
    public Vector4 ShadowSplits;

    /// <summary>The width in meters of one shadow map texel in each cascade</summary>
    public Vector4 ShadowTexelSizes;

    /// <summary>Each cascade's shadow map depth per meter towards the sun</summary>
    public Vector4 ShadowDepthScales;

    /// <summary>The index of the shadow map in the global texture pool, or 0 for no shadows</summary>
    public uint ShadowTexture;

    /// <summary>How many cascades are drawn</summary>
    public uint ShadowCascadeCount;

    /// <summary>Where shadows start to fade out, and where they are gone</summary>
    public float ShadowFadeStart;
    public float ShadowFadeEnd;

    /// <summary>The GPARAM's depth bias and slope scaled depth bias, with the viewport's scale on both</summary>
    public float ShadowDepthBias;
    public float ShadowSlopeBias;

    /// <summary>The share of each cascade's range over which it blends into the next</summary>
    public float ShadowCascadeFade;

    /// <summary>How much of the sun a shadow takes away, 1 for all of it</summary>
    public float ShadowStrength;

    /// <summary>The shadow map's size in texels, one side of the whole map that holds the four cascades</summary>
    public float ShadowMapSize;

    // Fills the last 16 byte row, as the shaders lay the struct out
    public float ShadowPad0;
    public float ShadowPad1;
    public float ShadowPad2;

    /// <summary>
    ///     The GPARAM Wet group in a weather that wets: rgb the albedo wet surfaces lean towards, a how far (AlbedoRate
    ///     times the wetness). 0 in a, so nothing changes, when it is dry.
    /// </summary>
    public Vector4 WetAlbedo;

    /// <summary>x: the shininess wet surfaces lean towards, y: how far (ShininessRate times the wetness)</summary>
    public Vector4 WetShininess;

    // The volumetric fog, for transparent draws to fog themselves with, see VolumetricFogPass. The fog's own pass
    // lays it over the opaque draws.

    /// <summary>The fog volume's index in the volume texture pool, 0 when there is no fog this frame</summary>
    public uint FogVolumeTexture;

    /// <summary>The volume's near depth, log(far / near), and its slices</summary>
    public float FogNear;
    public float FogLogRatio;
    public float FogSlices;

    /// <summary>x: far depth, y: the fog's end depth past it (0 for no analytic part), zw: the target's size</summary>
    public Vector4 FogRange;

    /// <summary>The fog's density, sky, sun and phase, as <see cref="VolumetricFogParam" /> has them</summary>
    public Vector4 FogDensity;
    public Vector4 FogExtinction;
    public Vector4 FogSky;
    public Vector4 FogDirect;
    public Vector4 FogOpposite;
    public Vector4 FogSun;

    // What the rain reaches, see RainOcclusion: a depth map of the scene seen straight down round the camera

    /// <summary>World to the rain map's texture space: xy -1..1 across the square, z the depth, 1 at the top</summary>
    public Matrix4x4 RainMatrix;

    /// <summary>The rain map's index in the global texture pool, 0 when wet surfaces don't read it</summary>
    public uint RainTexture;

    /// <summary>The rain map's side in texels</summary>
    public float RainMapSize;

    /// <summary>Meters per rain map texel</summary>
    public float RainTexelSize;

    /// <summary>The rain map's depth per meter of height</summary>
    public float RainDepthScale;
}

/// <summary>A directional light of the map's GPARAM, as the shaders read it</summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MapDirectionalLightData
{
    /// <summary>xyz: the direction the light travels in</summary>
    public Vector4 Direction;

    /// <summary>rgb: color, a: intensity with the exposure</summary>
    public Vector4 Color;

    /// <summary>The color and intensity of the light's highlights</summary>
    public Vector4 SpecColor;
}
