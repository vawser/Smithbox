namespace StudioCore.Editors.Common;

/// <summary>
/// Where a running game keeps its event flags, as chunks of 1000 flags: each chunk has the ID of its first flag, its
/// address in the game, and its place in the monitor's snapshot. Chunks close together in the game are read as one
/// region. Built by <see cref="EventFlagMonitor"/> from the game's own tables, so it is exact for any version or mod.
/// Two encodings:
/// <list type="bullet">
/// <item>ByteMsb (ER, NR, AC6): 125 bytes per chunk, flag n is bit 7 − n % 8 of byte n / 8.</item>
/// <item>DwordMsb (DS3, SDT): 128 bytes per chunk as 32 little-endian u32s, flag n is bit 31 − n % 32 of u32 n / 32.</item>
/// </list>
/// </summary>
public sealed class EventFlagLayout
{
    public enum BitOrder { ByteMsb, DwordMsb }

    public sealed record Chunk(long BaseFlag, int Offset, nint Address);

    public readonly record struct Region(nint Address, int Offset, int Length);

    public const int FlagsPerChunk = 1000;

    /// <summary>Chunks further apart than this in the game are read separately</summary>
    private const int MergeGap = 4096;

    public BitOrder Order { get; }
    public int ChunkBytes { get; }

    /// <summary>The snapshot's size: every region, end to end</summary>
    public int Size { get; }

    /// <summary>Chunks in snapshot order (one per address; several flag ranges can share one)</summary>
    public IReadOnlyList<Chunk> Chunks => chunks;

    /// <summary>The regions to read, in snapshot order</summary>
    public IReadOnlyList<Region> Regions => regions;

    /// <summary>The flag ranges, by first flag (in ID order)</summary>
    public IReadOnlyList<long> BaseFlags => baseFlags;

    /// <summary>Values that stay the same while this layout is valid; a change means it must be built again</summary>
    public byte[] Identity { get; }

    /// <summary>Chunks the game has whose flag IDs couldn't be worked out (not shown)</summary>
    public int SkippedChunks { get; }

    private readonly Chunk[] chunks;
    private readonly int[] chunkOffsets;
    private readonly Region[] regions;
    private readonly Dictionary<long, Chunk> byBase = new();
    private readonly long[] baseFlags;

    public EventFlagLayout(BitOrder order, IEnumerable<(long BaseFlag, nint Address)> source, byte[] identity, int skipped)
    {
        Order = order;
        ChunkBytes = order == BitOrder.ByteMsb ? 125 : 128;
        Identity = identity;
        SkippedChunks = skipped;

        // One chunk per address, in address order; the first flag range at an address keeps it
        var byAddress = new SortedDictionary<long, List<long>>();
        foreach (var (baseFlag, address) in source)
        {
            if (address == 0)
                continue;
            if (!byAddress.TryGetValue(address, out var bases))
                byAddress[address] = bases = new List<long>();
            bases.Add(baseFlag);
        }

        var chunkList = new List<Chunk>();
        var regionList = new List<Region>();
        var offset = 0;
        nint regionStart = 0, regionEnd = 0;
        var regionOffset = 0;

        foreach (var (addressValue, bases) in byAddress)
        {
            var address = (nint)addressValue;

            if (regionList.Count == 0 && regionEnd == 0 || address - regionEnd > MergeGap || address < regionEnd)
            {
                if (regionEnd != 0)
                    regionList.Add(new Region(regionStart, regionOffset, (int)(regionEnd - regionStart)));
                regionStart = address;
                regionOffset = offset;
            }
            else
            {
                // Keep the gap in the snapshot so offsets follow the game's addresses within a region
                offset += (int)(address - regionEnd);
            }

            var chunk = new Chunk(bases[0], offset, address);
            chunkList.Add(chunk);
            foreach (var b in bases)
                byBase.TryAdd(b, chunk);

            offset += ChunkBytes;
            regionEnd = address + ChunkBytes;
        }

        if (regionEnd != 0)
            regionList.Add(new Region(regionStart, regionOffset, (int)(regionEnd - regionStart)));

        chunks = chunkList.ToArray();
        chunkOffsets = chunks.Select(c => c.Offset).ToArray();
        regions = regionList.ToArray();
        Size = offset;
        baseFlags = byBase.Keys.OrderBy(k => k).ToArray();
    }

    public int ChunkCount => chunks.Length;

    /// <summary>The chunk holding a byte of the snapshot; null in a gap between chunks</summary>
    public Chunk ChunkAt(int index)
    {
        var i = Array.BinarySearch(chunkOffsets, index);
        if (i < 0)
            i = ~i - 1;
        if (i < 0)
            return null;

        var chunk = chunks[i];
        return index < chunk.Offset + ChunkBytes ? chunk : null;
    }

    public Chunk ChunkOf(long baseFlag) => byBase.TryGetValue(baseFlag, out var c) ? c : null;

    /// <summary>Whether the game stores the flag</summary>
    public bool IsStored(long flag) => flag >= 0 && byBase.ContainsKey(flag / FlagsPerChunk * FlagsPerChunk);

    /// <summary>The snapshot byte and bit mask of a flag; false when the game doesn't store it</summary>
    public bool Locate(long flag, out int index, out byte mask)
    {
        index = 0;
        mask = 0;
        if (flag < 0 || !byBase.TryGetValue(flag / FlagsPerChunk * FlagsPerChunk, out var chunk))
            return false;

        var n = (int)(flag % FlagsPerChunk);
        if (Order == BitOrder.ByteMsb)
        {
            index = chunk.Offset + n / 8;
            mask = (byte)(0x80 >> (n % 8));
        }
        else
        {
            var bit = 31 - (n & 31);
            index = chunk.Offset + (n >> 5) * 4 + bit / 8;
            mask = (byte)(1 << (bit % 8));
        }
        return true;
    }

    /// <summary>The game address of a snapshot byte (0 in a gap)</summary>
    public nint AddressOf(int index)
    {
        var chunk = ChunkAt(index);
        return chunk == null ? 0 : chunk.Address + (index - chunk.Offset);
    }

    /// <summary>
    /// The flag stored at bit (1 &lt;&lt; bit) of a snapshot byte, given the chunk; -1 past the chunk's last flag
    /// (the padding of a DwordMsb chunk)
    /// </summary>
    public long FlagAt(Chunk chunk, int index, int bit)
    {
        var o = index - chunk.Offset;
        int n;
        if (Order == BitOrder.ByteMsb)
            n = o * 8 + (7 - bit);
        else
            n = (o / 4) * 32 + 31 - ((o % 4) * 8 + bit);

        return n >= 0 && n < FlagsPerChunk ? chunk.BaseFlag + n : -1;
    }

    public bool Get(byte[] data, long flag) => Locate(flag, out var index, out var mask) && index < data.Length && (data[index] & mask) != 0;
}
