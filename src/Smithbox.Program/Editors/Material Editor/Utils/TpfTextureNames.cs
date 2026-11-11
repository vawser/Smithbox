using SoulsFormats;
using System.Buffers.Binary;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Reads only the texture names of a TPF, from its entry table, without the texture data a full read copies.
/// Entries are 20 bytes on PC, more on consoles (width and height, then per platform: Xbox 360 4, PS3 4 or 8,
/// PS4/Xbox One/PS5 8 plus a DXGI format, Switch 2 12 plus a DXGI format), and a float block when flagged. Laid out
/// as SoulsFormats' TPF.Texture reads them. No editor dependencies, so the survey tool can link it.
/// </summary>
public static class TpfTextureNames
{
    private const byte PC = 0, Xbox360 = 1, PS3 = 2, PS4 = 4, Xbone = 5, PS5 = 8, Switch2 = 0xA;

    /// <summary>
    /// A stand-in .tpfbdt header: BXF4Reader checks the data file's 0x30 byte header before it reads the .tpfbhd,
    /// and the names need nothing else from it, so the (large) .tpfbdt isn't read
    /// </summary>
    private static readonly byte[] EmptyBdt4 = MakeEmptyBdt4();

    // BXF3 (DES, DS1, DS1R): "BDF3", an 8 byte version, then 0
    private static readonly byte[] EmptyBdt3 = [(byte)'B', (byte)'D', (byte)'F', (byte)'3', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0];

    private static byte[] MakeEmptyBdt4()
    {
        var bytes = new byte[0x30];
        bytes[0] = (byte)'B'; bytes[1] = (byte)'D'; bytes[2] = (byte)'F'; bytes[3] = (byte)'4';
        BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(0x10), 0x30); // header size
        return bytes;
    }

    /// <summary>
    /// The file names a .tpfbhd lists (BHF4, or BHF3 in the older games), without its .tpfbdt. Null if it isn't one.
    /// In every surveyed game these are the names of the textures inside.
    /// </summary>
    public static List<string> PackedFileNames(Memory<byte> bhd)
    {
        try
        {
            if (bhd.Length < 4 || bhd.Span[0] != 'B' || bhd.Span[1] != 'H' || bhd.Span[2] != 'F')
                return null;

            if (bhd.Span[3] == '3')
            {
                using var reader3 = new BXF3Reader(bhd, EmptyBdt3);
                return reader3.Files.Select(f => f.Name ?? "").ToList();
            }

            using var reader4 = new BXF4Reader(bhd, EmptyBdt4);
            return reader4.Files.Select(f => f.Name ?? "").ToList();
        }
        catch
        {
            return null;
        }
    }

    public static bool IsTpf(ReadOnlySpan<byte> data) =>
        data.Length >= 0x10 && data[0] == 'T' && data[1] == 'P' && data[2] == 'F' && data[3] == 0;

    /// <summary>
    /// The texture names of a TPF (compressed or not), or null if it isn't one or is malformed
    /// </summary>
    public static List<string> Read(Memory<byte> data)
    {
        try
        {
            if (DCX.Is(data))
                data = DCX.Decompress(data);

            if (!IsTpf(data.Span))
                return null;

            var memory = data;
            var platform = memory.Span[0xC];
            var flag2 = memory.Span[0xD];
            var encoding = memory.Span[0xE];
            var bigEndian = platform is Xbox360 or PS3;

            int I32(int at) => bigEndian
                ? BinaryPrimitives.ReadInt32BigEndian(memory.Span.Slice(at, 4))
                : BinaryPrimitives.ReadInt32LittleEndian(memory.Span.Slice(at, 4));

            var count = I32(0x8);
            if (count < 0 || count > 100000)
                return null;

            var br = new BinaryReaderEx(bigEndian, data);
            var names = new List<string>(count);
            var pos = 0x10;

            for (var i = 0; i < count; i++)
            {
                pos += 12; // data offset, data size, format, type, mipmaps, flags

                if (platform != PC)
                {
                    pos += 4; // width, height
                    pos += platform switch
                    {
                        Xbox360 => 4,
                        PS3 => flag2 != 0 ? 8 : 4,
                        PS4 or Xbone or PS5 => 8,
                        Switch2 => 12,
                        _ => 0
                    };
                }

                var nameOffset = I32(pos);
                pos += 4;

                var hasFloats = I32(pos) == 1;
                pos += 4;

                if (platform is PS4 or Xbone or PS5 or Switch2)
                    pos += 4; // DXGI format

                if (hasFloats)
                {
                    var length = I32(pos + 4);
                    if (length < 0)
                        return null;
                    pos += 8 + length;
                }

                if (nameOffset <= 0 || nameOffset >= data.Length)
                    return null;

                names.Add(encoding == 1 ? br.GetUTF16(nameOffset) : br.GetShiftJIS(nameOffset));
            }

            return names;
        }
        catch
        {
            return null;
        }
    }
}
