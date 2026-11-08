using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
/// One set of map lighting for the parts that name it, as FlverShader.frag's lightSetData reads it: what the scene
/// params hold for the whole scene (the sun, the hemisphere, the model set's lights and hemisphere), with the intensity
/// and exposure folded in the same way (SceneRenderPipeline), so a part lit by its set looks as the whole scene would
/// lit by it.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LightSetData
{
    public const int Size = 320;

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

    public Vector4 Pad;
}

/// <summary>
/// Per-part map lighting in one allocation of the material buffer (set 4), as <see cref="LightmapTable" /> is: up to
/// <see cref="MaxSets" /> sets, entry 0 unused. A flver instance names its set in bits 24-30 of its material ID
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
    /// Fills the table from <paramref name="sets" /> (set 1 is the first) at <paramref name="exposure" />, and uploads
    /// it if anything changed. Called by each pipeline every frame; the same values upload once.
    /// </summary>
    public void Update(IReadOnlyList<GparamLighting> sets, float exposure)
    {
        var count = Math.Min(sets?.Count ?? 0, MaxSets);
        var changed = count != _count || !_uploaded;

        for (var i = 0; i < count; i++)
        {
            var data = FromLighting(sets[i], exposure);

            if (!changed && data.Equals(_entries[i + 1]))
                continue;

            _entries[i + 1] = data;
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
            ModelHemiDown = Hemi(models.HemiDown, exposure)
        };
    }

    private static Vector4 Hemi(Vector4 color, float exposure)
    {
        return new Vector4(new Vector3(color.X, color.Y, color.Z) * color.W * exposure, 1.0f);
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
