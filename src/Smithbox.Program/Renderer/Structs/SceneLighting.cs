using StudioCore.Application;
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
    public const int Size = 368;

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

    /// <summary>
    ///     Demon's Souls' env cubemaps (<see cref="GparamEnvLighting" />): their scales, rgb multiplied by intensity
    ///     and exposure, w 1 when env lighting is on. 0 everywhere else, so no material is env lit.
    /// </summary>
    public Vector4 EnvDiffuseScale;
    public Vector4 EnvSpecularScale;

    /// <summary>The env cubemaps in the cube texture pool: the diffuse, then the four specular, <see cref="NoTexture" /> if not loaded</summary>
    public uint EnvDiffuseTexture;
    public uint EnvSpecular0;
    public uint EnvSpecular1;
    public uint EnvSpecular2;
    public uint EnvSpecular3;

    // Fills the last 16 byte row
    public uint EnvPad0;
    public uint EnvPad1;
    public uint EnvPad2;

    /// <summary>An env texture that isn't on the GPU, which the shader leaves out</summary>
    public const uint NoTexture = uint.MaxValue;

    /// <summary>
    ///     Sets the env fields from <paramref name="env" /> at <paramref name="exposure" />: on when the lighting has
    ///     env cubemaps and the viewport's DrawParam env lighting option is on, otherwise off (the materials are then
    ///     lit with the sun, as everything else). The textures are looked up each time, so they show once loaded.
    /// </summary>
    public void SetEnv(GparamEnvLighting env, float exposure)
    {
        var on = env is { Enabled: true } && CFG.Current.Viewport_Map_DrawParam_Env_Lighting;

        EnvDiffuseScale = on ? EnvScale(env.DiffuseColor, exposure) : Vector4.Zero;
        EnvSpecularScale = on ? EnvScale(env.SpecularColor, exposure) : Vector4.Zero;
        EnvDiffuseTexture = on ? CubeTexture(env.DiffuseTexture) : NoTexture;
        EnvSpecular0 = on ? CubeTexture(env.SpecularTextures[0]) : NoTexture;
        EnvSpecular1 = on ? CubeTexture(env.SpecularTextures[1]) : NoTexture;
        EnvSpecular2 = on ? CubeTexture(env.SpecularTextures[2]) : NoTexture;
        EnvSpecular3 = on ? CubeTexture(env.SpecularTextures[3]) : NoTexture;
    }

    private static Vector4 EnvScale(Vector4 color, float exposure)
    {
        return new Vector4(new Vector3(color.X, color.Y, color.Z) * color.W * exposure, 1.0f);
    }

    /// <summary>A cubemap's index in the cube texture pool by its texture name, if it is on the GPU</summary>
    public static uint CubeTexture(string name)
    {
        if (string.IsNullOrEmpty(name) || !ResourceManager.DefaultPool.TryFind<TextureResource>(name, out var handle))
            return NoTexture;

        var gpu = handle?.Get()?.GPUTexture;

        if (gpu == null || !gpu.Resident || gpu.Pool != SceneRenderer.GlobalCubeTexturePool)
            return NoTexture;

        return gpu.TexHandle;
    }
}
