using SoulsFormats;
using System.Buffers.Binary;

namespace StudioCore.Editors.MaterialEditor;

/// <summary>
/// Reads only the material table of a FLVER: the MTD/MATBIN path of each material, without the meshes and
/// vertex buffers a full read builds. FLVER0 (DES) and FLVER2 share the layout this needs: a 0x80 byte header with
/// the dummy count at 0x14, the material count at 0x18 and the Unicode flag at 0x49; 0x40 byte dummies straight
/// after it; then 0x20 byte materials starting with the name and MTD string offsets.
/// No editor dependencies, so the survey tool can link it.
/// </summary>
public static class FlverMaterialNames
{
    private const int HeaderSize = 0x80;
    private const int DummySize = 0x40;
    private const int MaterialSize = 0x20;

    /// <summary>
    /// Whether the data (decompressed) is a FLVER
    /// </summary>
    public static bool IsFlver(ReadOnlySpan<byte> data) =>
        data.Length >= HeaderSize && data[0] == 'F' && data[1] == 'L' && data[2] == 'V' && data[3] == 'E' && data[4] == 'R';

    /// <summary>
    /// The material paths of a FLVER (compressed or not), one per material, or null if it isn't one or is malformed
    /// </summary>
    public static List<string> Read(Memory<byte> data)
    {
        try
        {
            if (DCX.Is(data))
                data = DCX.Decompress(data);

            var span = data.Span;

            if (!IsFlver(span))
                return null;

            var bigEndian = span[6] == 'B';
            var memory = data;

            int I32(int at) => bigEndian
                ? BinaryPrimitives.ReadInt32BigEndian(memory.Span.Slice(at, 4))
                : BinaryPrimitives.ReadInt32LittleEndian(memory.Span.Slice(at, 4));

            var dummyCount = I32(0x14);
            var materialCount = I32(0x18);
            var unicode = span[0x49] != 0;

            if (dummyCount < 0 || materialCount < 0 || materialCount > 4096)
                return null;

            var br = new BinaryReaderEx(bigEndian, data);
            var names = new List<string>(materialCount);
            var start = HeaderSize + dummyCount * DummySize;

            for (var i = 0; i < materialCount; i++)
            {
                var at = start + i * MaterialSize;
                if (at + MaterialSize > span.Length)
                    return null;

                var mtdOffset = I32(at + 4);
                if (mtdOffset <= 0 || mtdOffset >= span.Length)
                {
                    names.Add("");
                    continue;
                }

                names.Add(unicode ? br.GetUTF16(mtdOffset) : br.GetShiftJIS(mtdOffset));
            }

            return names;
        }
        catch
        {
            return null;
        }
    }
}
