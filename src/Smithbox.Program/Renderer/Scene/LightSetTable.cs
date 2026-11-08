using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
/// One set of map lighting for the parts that name it, as FlverShader.frag's lightSetData reads it: what the scene
/// params hold for the whole scene (the sun, the hemisphere, the model set's lights and hemisphere), with the intensity
/// and exposure folded in the same way (SceneRenderPipeline), so a part lit by its set looks as the whole scene would
/// lit by it. Also the set's env cubemaps (Demon's Souls), which the scene params have no place for.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LightSetData
{
    public const int Size = 384;

    /// <summary>An env texture that isn't on the GPU, which the shader leaves out</summary>
    public const uint NoTexture = uint.MaxValue;

    public MapDirectionalLightData Sun;

    /// <summary>The hemisphere, rgb already multiplied by intensity and exposure</summary>
    public Vector4 HemiUp;
    public Vector4 HemiDown;

    public MapDirectionalLightData ModelLight0;
    public MapDirectionalLightData ModelLight1;
    public MapDirectionalLightData ModelLight2;
    public MapDirectionalLightData ModelLight3;

    public Vector4 ModelHemiUp;
    public Vector4 ModelHemiDown;

    /// <summary>The env cubemaps' scales, rgb already multiplied by intensity and exposure; w 1 when env lighting is on</summary>
    public Vector4 EnvDiffuseScale;
    public Vector4 EnvSpecularScale;

    /// <summary>The env cubemaps in the cube texture pool: the diffuse, then the four specular, <see cref="NoTexture" /> if not loaded</summary>
    public uint EnvDiffuseTexture;
    public uint EnvSpecular0;
    public uint EnvSpecular1;
    public uint EnvSpecular2;
    public uint EnvSpecular3;
    public uint EnvPad0;
    public uint EnvPad1;
    public uint EnvPad2;

    public Vector4 Pad;
}

/// <summary>
/// Per-part map lighting in one allocation of the material buffer (set 4), as <see cref="LightmapTable" /> is: up to
/// <see cref="MaxSets" /> sets. Entry 0 is the scene's own lighting, which the shader reads only for what the scene
/// params don't hold (the env cubemaps). A flver instance names its set in bits 24-30 of its material ID
/// (<see cref="MeshRenderableProxy.LightSet" />), 0 for the scene's own lighting; the shader reads the set at
/// <see cref="Start" /> plus the index, the start passed in the scene params. Demon's Souls fills it from the
/// LightBank rows its parts pick (DrawParamLightingBank).
/// </summary>
public class LightSetTable
{
    /// <summary>The most sets: the index has 7 bits</summary>
    public const int MaxSets = 127;

    private readonly GPUBufferAllocator.GPUBufferHandle _allocation;

    private readonly LightSetData[] _entries = new LightSetData[MaxSets + 1];
    private int _count;
    private bool _uploaded;

    public LightSetTable(GPUBufferAllocator materialBuffer)
    {
        _allocation = materialBuffer.Allocate((uint)(_entries.Length * LightSetData.Size), LightSetData.Size);
    }

    /// <summary>The index of the first entry in the material buffer, counted in entries</summary>
    public uint Start => _allocation.AllocationStart / LightSetData.Size;

    /// <summary>How many sets are in use</summary>
    public int Count => _count;

    /// <summary>
    /// Fills the table from the scene's lighting (entry 0) and <paramref name="sets" /> (set 1 is the first) at
    /// <paramref name="exposure" />, and uploads it if anything changed. Called by each pipeline every frame; the same
    /// values upload once. The env textures are looked up again each time, so they show once they've loaded.
    /// </summary>
    public void Update(GparamLighting scene, IReadOnlyList<GparamLighting> sets, float exposure)
    {
        var count = Math.Min(sets?.Count ?? 0, MaxSets);
        var changed = count != _count || !_uploaded;

        for (var i = 0; i <= count; i++)
        {
            var lighting = i == 0 ? scene : sets[i - 1];
            var data = lighting != null ? FromLighting(lighting, exposure) : default;

            if (!changed && data.Equals(_entries[i]))
                continue;

            _entries[i] = data;
            changed = true;
        }

        _count = count;

        if (!changed)
            return;

        _uploaded = true;
        _allocation.FillBuffer(_entries.AsSpan(0, count + 1).ToArray(), () => { });
    }

    /// <summary>A set's GPU values, folded as SceneRenderPipeline folds the scene's</summary>
    public static LightSetData FromLighting(GparamLighting lighting, float exposure)
    {
        var models = lighting.Models ?? lighting;
        // Off, the shader lights the env materials as it lights everything else, with the sun
        var env = lighting.Env;
        var envOn = env is { Enabled: true } && CFG.Current.Viewport_Map_DrawParam_Env_Lighting;

        return new LightSetData
        {
            Sun = ToGpu(lighting.Sun, exposure),
            HemiUp = Hemi(lighting.HemiUp, exposure),
            HemiDown = Hemi(lighting.HemiDown, exposure),
            ModelLight0 = ToGpu(models.Sun, exposure),
            ModelLight1 = ToGpu(models.Light1, exposure),
            ModelLight2 = ToGpu(models.LocalLight1, exposure),
            ModelLight3 = ToGpu(models.LocalLight2, exposure),
            ModelHemiUp = Hemi(models.HemiUp, exposure),
            ModelHemiDown = Hemi(models.HemiDown, exposure),
            EnvDiffuseScale = envOn ? EnvScale(env.DiffuseColor, exposure) : Vector4.Zero,
            EnvSpecularScale = envOn ? EnvScale(env.SpecularColor, exposure) : Vector4.Zero,
            EnvDiffuseTexture = envOn ? CubeTexture(env.DiffuseTexture) : LightSetData.NoTexture,
            EnvSpecular0 = envOn ? CubeTexture(env.SpecularTextures[0]) : LightSetData.NoTexture,
            EnvSpecular1 = envOn ? CubeTexture(env.SpecularTextures[1]) : LightSetData.NoTexture,
            EnvSpecular2 = envOn ? CubeTexture(env.SpecularTextures[2]) : LightSetData.NoTexture,
            EnvSpecular3 = envOn ? CubeTexture(env.SpecularTextures[3]) : LightSetData.NoTexture
        };
    }

    private static Vector4 Hemi(Vector4 color, float exposure)
    {
        return new Vector4(new Vector3(color.X, color.Y, color.Z) * color.W * exposure, 1.0f);
    }

    private static Vector4 EnvScale(Vector4 color, float exposure)
    {
        return new Vector4(new Vector3(color.X, color.Y, color.Z) * color.W * exposure, 1.0f);
    }

    /// <summary>A cubemap's index in the cube texture pool by its texture name, if it is on the GPU</summary>
    private static uint CubeTexture(string name)
    {
        if (string.IsNullOrEmpty(name) || !ResourceManager.DefaultPool.TryFind<TextureResource>(name, out var handle))
            return LightSetData.NoTexture;

        var gpu = handle?.Get()?.GPUTexture;

        if (gpu == null || !gpu.Resident || gpu.Pool != SceneRenderer.GlobalCubeTexturePool)
            return LightSetData.NoTexture;

        return gpu.TexHandle;
    }

    private static MapDirectionalLightData ToGpu(GparamDirectionalLight light, float exposure)
    {
        var intensity = light.Enabled ? exposure : 0.0f;

        return new MapDirectionalLightData
        {
            Direction = new Vector4(GparamLighting.AngleToDirection(light.Angle), 0.0f),
            Color = new Vector4(light.Diffuse.X, light.Diffuse.Y, light.Diffuse.Z, light.Diffuse.W * intensity),
            SpecColor = new Vector4(light.Specular.X, light.Specular.Y, light.Specular.Z, light.Specular.W * intensity)
        };
    }
}
