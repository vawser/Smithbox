using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace StudioCore.Renderer;

/// <summary>
///     The color grading LUTs, which the GPARAM ColorGrading[Yebis] group's LutSourceId picks. In Elden Ring they are
///     in <c>/map/mAA/common/mAA_cgrading.tpf.dcx</c> (Sekiro and DS3: <c>/map/mAA/mAA_cgrading.tpf.dcx</c>), one TPF
///     per area, as textures named <c>mAA_BB_CGradingNNNN</c>:
///     NNNN is the LutSourceId and mAA_BB the area that owns it. Each is a 16 x 16 x 16 cube laid out as a 16 x 256
///     RGBA8 image, sixteen 16 x 16 slices down it: red across, green down a slice, blue from slice to slice. The
///     IDs are shared between areas (m10_00_0900 uses m60_00's 0001-0005, m20_00 uses m61_00's 5000-5003), so every
///     area's are loaded, and an ID is looked up in its own area first.
///     <para>
///         All of them are packed side by side into <see cref="Atlas" />, 16 pixels across each, so the tone map
///         binds one texture and picks two LUTs by their slot.
///     </para>
/// </summary>
public class ColorGradingLuts
{
    /// <summary>The cube's size on each axis</summary>
    public const int Size = 16;

    public const int Height = Size * Size;

    // A DDS file's header, before the pixels
    private const int DdsHeaderSize = 128;

    private static readonly Regex NamePattern = new(@"^(m\d{2}_\d{2})_CGrading(\d{4})$", RegexOptions.IgnoreCase);

    /// <summary>Each slot's owner (mAA_BB) and LutSourceId, in the order they are in <see cref="Atlas" /></summary>
    public List<(string Owner, int Id)> Entries { get; } = new();

    /// <summary>The LUTs side by side: <see cref="Width" /> x <see cref="Height" /> RGBA8</summary>
    public byte[] Atlas { get; private set; } = Array.Empty<byte>();

    public int Width => Math.Max(Entries.Count, 1) * Size;

    public int Count => Entries.Count;

    /// <summary>
    ///     Reads the LUTs from <paramref name="read" />, which returns a file's bytes or null if there is none. Areas
    ///     m00 to m99 are tried, at <paramref name="pathFormat" /> with {0} the area number
    ///     (<see cref="GparamProfile.LutPathFormat" />: under <c>common/</c> in Elden Ring, AC6 and Nightreign, in the
    ///     area's folder itself in Sekiro and DS3, with the same textures).
    /// </summary>
    public static ColorGradingLuts Load(string pathFormat, Func<string, Memory<byte>?> read, Action<string, Exception> logError)
    {
        var luts = new ColorGradingLuts();
        var pixels = new List<byte[]>();

        for (var area = 0; area < 100; area++)
        {
            var path = string.Format(pathFormat, area);
            Memory<byte>? bytes;

            try
            {
                bytes = read(path);
            }
            catch (Exception e)
            {
                logError(path, e);
                continue;
            }

            if (bytes == null)
                continue;

            TPF tpf;

            try
            {
                tpf = TPF.Read(bytes.Value);
            }
            catch (Exception e)
            {
                logError(path, e);
                continue;
            }

            foreach (var texture in tpf.Textures)
            {
                var match = NamePattern.Match(texture.Name);
                var data = texture.Bytes.ToArray();

                // Only the uncompressed 16 x 256 RGBA8 layout described above
                if (!match.Success || data.Length < DdsHeaderSize + Size * Height * 4 ||
                    BitConverter.ToInt32(data, 0) != 0x20534444 ||
                    BitConverter.ToInt32(data, 12) != Height || BitConverter.ToInt32(data, 16) != Size ||
                    BitConverter.ToInt32(data, 88) != 32 || BitConverter.ToUInt32(data, 92) != 0xFF)
                    continue;

                luts.Entries.Add((match.Groups[1].Value.ToLowerInvariant(), int.Parse(match.Groups[2].Value)));
                pixels.Add(data.AsSpan(DdsHeaderSize, Size * Height * 4).ToArray());
            }
        }

        luts.Pack(pixels);
        return luts;
    }

    private void Pack(List<byte[]> pixels)
    {
        var width = Width;
        Atlas = new byte[width * Height * 4];

        for (var slot = 0; slot < pixels.Count; slot++)
        {
            for (var row = 0; row < Height; row++)
            {
                Buffer.BlockCopy(pixels[slot], row * Size * 4, Atlas, (row * width + slot * Size) * 4, Size * 4);
            }
        }
    }

    /// <summary>
    ///     The slot of LUT <paramref name="id" /> as <paramref name="area" /> (mAA_BB) uses it: its own, then one of the
    ///     same mAA, then any area's. -1 if there is none.
    /// </summary>
    public int Find(string area, int id)
    {
        if (id < 0)
            return -1;

        var best = -1;
        var bestRank = int.MaxValue;
        var prefix = area != null && area.Length >= 3 ? area.Substring(0, 3) : null;

        for (var slot = 0; slot < Entries.Count; slot++)
        {
            var (owner, entryId) = Entries[slot];

            if (entryId != id)
                continue;

            var rank = string.Equals(owner, area, StringComparison.OrdinalIgnoreCase) ? 0
                : prefix != null && owner.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ? 1
                : 2;

            if (rank < bestRank)
            {
                best = slot;
                bestRank = rank;
            }
        }

        return best;
    }
}
