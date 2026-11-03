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

    /// <summary>1: the sun comes from the map's GPARAMs, see <see cref="GparamLighting" /></summary>
    public uint MapLightingFlags;

    /// <summary>
    ///     Color (rgb) and intensity (a) of the sun's highlights. The same as <see cref="SunColor" /> unless the map's
    ///     GPARAM gives its own.
    /// </summary>
    public Vector4 SunSpecColor;
}
