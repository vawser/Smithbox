using System;
using System.Runtime.InteropServices;

namespace StudioCore.Renderer;

/// <summary>
/// The map's lights the flver shader lights with, nearest the camera first, beyond the
/// <see cref="SceneLightList.MaxLights" /> of the scene params. Kept in one allocation of the material buffer,
/// which the shader already binds (set 4), as <see cref="IrradianceVolumeTable" /> is: the shader reads it from the
/// entry at <see cref="Start" />, for <see cref="Count" /> entries, both passed in the scene params.
/// </summary>
public class SceneLightTable
{
    /// <summary>The most lights shaded at once. Each pixel loops over all of them, skipping those out of range.</summary>
    public const int Capacity = 1024;

    private static readonly int EntrySize = Marshal.SizeOf<SceneLightData>();

    private readonly GPUBufferAllocator.GPUBufferHandle _allocation;

    // The last entries uploaded, so an unchanged table isn't uploaded again
    private byte[] _uploaded = Array.Empty<byte>();

    public SceneLightTable(GPUBufferAllocator materialBuffer)
    {
        // Aligned to the entry size, so the start is a whole entry index
        _allocation = materialBuffer.Allocate((uint)(Capacity * EntrySize), EntrySize);
    }

    /// <summary>The index of the first entry in the material buffer, counted in entries</summary>
    public uint Start => _allocation.AllocationStart / (uint)EntrySize;

    /// <summary>How many entries the shader reads. Only updated once their data is on the GPU.</summary>
    public uint Count { get; private set; }

    public void Upload(SceneLightData[] entries, int count)
    {
        count = Math.Min(count, Capacity);

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
