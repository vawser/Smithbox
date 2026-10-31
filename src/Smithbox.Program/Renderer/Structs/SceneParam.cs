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

    // Keeps the next field on a 16 byte row
    public fixed float MaterialTimePadding[2];

    /// <summary>xy is the world XZ direction the wind blows in, z its steady force and w the force of its waves</summary>
    public Vector4 Wind;
}
