using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
///     One lighting set, as the shaders read it from <c>sceneLighting</c> in SceneLighting.glsl (std430, set 0,
///     binding 1, beside the scene params). Entry 0 of a pipeline's <see cref="SceneRenderPipeline.SceneLightingBuffer" />
///     is the view's own lighting: the map's GPARAM lighting where it lights the scene, otherwise the viewport's sun.
///     The other entries are for per-part lighting sets (Demon's Souls' LightIDs), which are not filled yet.
///     Intensities are folded in: colour rgb with intensity × exposure in a, the hemispheres' rgb pre-multiplied.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct SceneLightingData
{
    /// <summary>The size in bytes; every member is a whole number of 16 byte rows, so std140 and std430 agree</summary>
    public const int Size = 304;

    /// <summary>The entries each pipeline's buffer holds: entry 0 and up to 127 per-part sets</summary>
    public const int MaxEntries = 128;

    /// <summary>The sun: its direction (the way the light travels), colour and highlights</summary>
    public MapDirectionalLightData Sun;

    /// <summary>The hemisphere light, rgb already multiplied by intensity and exposure</summary>
    public Vector4 HemiUp;
    public Vector4 HemiDown;

    /// <summary>
    ///     The model set of the map's GPARAM LightSet, for characters when SceneParam.MapLightingFlags has 4: its sun,
    ///     Light1, LocalLight1 and LocalLight2, and its hemisphere
    /// </summary>
    public MapDirectionalLightData ModelLight0;
    public MapDirectionalLightData ModelLight1;
    public MapDirectionalLightData ModelLight2;
    public MapDirectionalLightData ModelLight3;

    public Vector4 ModelHemiUp;
    public Vector4 ModelHemiDown;
}
