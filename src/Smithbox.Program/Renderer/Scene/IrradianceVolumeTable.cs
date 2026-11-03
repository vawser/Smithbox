using System;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
/// The irradiance volumes the flver shader lights with, in drawing order (coarse first, so finer volumes blend
/// over them). Kept in one allocation of the material buffer, which the shader already binds (set 4), so no
/// descriptor set is needed: the shader reads it from the entry at <see cref="Start" />, for <see cref="Count" />
/// entries, both passed in the scene params.
/// </summary>
public class IrradianceVolumeTable : IDisposable
{
    /// <summary>The most volumes shaded at once. ER maps have around 130 each.</summary>
    public const int Capacity = 512;

    private readonly GPUBufferAllocator.GPUBufferHandle _allocation;

    // The most entries this table holds
    private readonly int _capacity;

    // The last entries uploaded, so an unchanged table isn't uploaded again
    private byte[] _uploaded = Array.Empty<byte>();

    /// <param name="capacity">The most entries the table holds, <see cref="Capacity" /> for the Map Editor's</param>
    public IrradianceVolumeTable(GPUBufferAllocator materialBuffer, int capacity = Capacity)
    {
        _capacity = Math.Clamp(capacity, 1, Capacity);

        // Aligned to the entry size, so the start is a whole entry index
        _allocation = materialBuffer.Allocate((uint)(_capacity * IrradianceVolumeData.Size), IrradianceVolumeData.Size);
    }

    public void Dispose()
    {
        Count = 0;
        _allocation?.Dispose();
    }

    /// <summary>The index of the first entry in the material buffer, counted in entries</summary>
    public uint Start => _allocation.AllocationStart / IrradianceVolumeData.Size;

    /// <summary>How many entries the shader reads. Only updated once their data is on the GPU.</summary>
    public uint Count { get; private set; }

    /// <summary>
    /// The exposure that brings the volumes' average light to the viewport's default ambient light, as the
    /// game's light is in its own HDR units
    /// </summary>
    public float AutoExposure { get; set; } = 1.0f;

    public void Upload(IrradianceVolumeData[] entries, int count)
    {
        count = Math.Min(count, _capacity);

        var bytes = MemoryMarshal.AsBytes(entries.AsSpan(0, count)).ToArray();

        if (bytes.AsSpan().SequenceEqual(_uploaded))
        {
            return;
        }

        _uploaded = bytes;

        if (count == 0)
        {
            Count = 0;
            return;
        }

        var data = entries.AsSpan(0, count).ToArray();
        _allocation.FillBuffer(data, () => Count = (uint)count);
    }
}
