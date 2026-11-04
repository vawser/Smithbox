using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
/// One lightmap of a part, as FlverShader.frag's lightmapData reads it: DS3's two DOL textures of a map piece, the
/// part's place in them (BTAB's UV scale and offset for the atlases of instanced pieces) and the HDR scale the first
/// texture's TPF entry gives (its FloatStruct, the shader's FC_TexHdrScaleParam). Entry 0 of the table holds the
/// GPARAM's Dominant Light Map values instead, see <see cref="LightmapTable.SetGlobals" />.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LightmapData
{
    public const int Size = 48;

    /// <summary>DOL 1, the light's color with the sun's shadow in alpha, in the global texture pool</summary>
    public uint ColorTexture;

    /// <summary>DOL 2, the light's main direction in the model's space with its sharpness in alpha</summary>
    public uint DirectionTexture;

    public uint Flags;
    public uint Pad;

    /// <summary>xy: scale of the second UV set, zw: its offset</summary>
    public Vector4 UvScaleOffset;

    /// <summary>rgb: the HDR scale of DOL 1</summary>
    public Vector4 HdrScale;
}

/// <summary>
/// The lightmaps the flver shader reads, in one allocation of the material buffer (set 4), as
/// <see cref="IrradianceVolumeTable" /> is. A static instance names its entry in InstanceData.SkinReference with
/// <see cref="InstanceData.LightmapFlag" />; the shader reads it at <see cref="Start" /> plus the entry, the start
/// passed in the scene params. Entries are handed out and given back as parts come and go; the table is uploaded
/// whole when it changes.
/// </summary>
public class LightmapTable
{
    /// <summary>The most lightmaps at once. DS3's m30_00_00_00 has around 2,500 lightmapped parts.</summary>
    public const int Capacity = 16384;

    private readonly GPUBufferAllocator.GPUBufferHandle _allocation;

    private readonly LightmapData[] _entries = new LightmapData[Capacity];
    private readonly Stack<uint> _free = new();
    private uint _next = 1;
    private bool _dirty = true;

    public LightmapTable(GPUBufferAllocator materialBuffer)
    {
        // Aligned to the entry size, so the start is a whole entry index
        _allocation = materialBuffer.Allocate((uint)(Capacity * LightmapData.Size), LightmapData.Size);
    }

    /// <summary>The index of the first entry in the material buffer, counted in entries</summary>
    public uint Start => _allocation.AllocationStart / LightmapData.Size;

    /// <summary>How many entries are in use</summary>
    public int Count { get; private set; }

    /// <summary>
    /// The GPARAM's Dominant Light Map values, in entry 0: <paramref name="sharpnessOffset" />, added to each
    /// lightmap's sharpness, and <paramref name="color" />, which scales their light
    /// </summary>
    public void SetGlobals(float sharpnessOffset, Vector3 color)
    {
        var globals = new LightmapData
        {
            UvScaleOffset = new Vector4(sharpnessOffset, 0, 0, 0),
            HdrScale = new Vector4(color, 1.0f)
        };

        if (!globals.Equals(_entries[0]))
        {
            _entries[0] = globals;
            _dirty = true;
        }
    }

    /// <summary>An entry for a lightmap, or null if the table is full</summary>
    public uint? Add(LightmapData data)
    {
        uint index;

        if (_free.Count > 0)
            index = _free.Pop();
        else if (_next < Capacity)
            index = _next++;
        else
            return null;

        _entries[index] = data;
        _dirty = true;
        Count++;

        return index;
    }

    public void Remove(uint index)
    {
        if (index == 0 || index >= Capacity)
            return;

        _entries[index] = default;
        _free.Push(index);
        _dirty = true;
        Count--;
    }

    /// <summary>Uploads the table if it changed since the last upload. Called once a frame.</summary>
    public void Upload()
    {
        if (!_dirty)
            return;

        _dirty = false;

        // Only up to the highest entry handed out
        var data = _entries.AsSpan(0, (int)Math.Max(_next, 1)).ToArray();
        _allocation.FillBuffer(data, () => { });
    }
}

/// <summary>
/// A part's lightmap entries by material name, for <see cref="MeshRenderableProxy.Lightmap" />: an instanced piece
/// has one atlas entry per material (BTAB), a unique piece one entry for all its materials.
/// </summary>
public class LightmapBinding
{
    private readonly Dictionary<string, uint> _byMaterial = new(StringComparer.OrdinalIgnoreCase);
    private uint? _all;

    public static LightmapBinding ForAll(uint entry) => new() { _all = entry };

    public void Add(string materialName, uint entry) => _byMaterial[materialName] = entry;

    public IEnumerable<uint> Entries
    {
        get
        {
            if (_all != null)
                yield return _all.Value;

            foreach (var entry in _byMaterial.Values)
                yield return entry;
        }
    }

    public bool TryGetEntry(string? materialName, out uint entry)
    {
        if (_all != null)
        {
            entry = _all.Value;
            return true;
        }

        return _byMaterial.TryGetValue(materialName ?? "", out entry);
    }
}
