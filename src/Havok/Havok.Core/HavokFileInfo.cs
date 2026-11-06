using System.Buffers.Binary;
using System.Text;

namespace Havok;

/// <summary>
/// What a Havok file says about itself, read from its header without decoding any objects.
/// </summary>
public sealed class HavokFileInfo
{
    public HavokContainer Container { get; init; }

    public HavokSdk Sdk { get; init; }

    /// <summary>
    /// The packfile ContentsVersionString or tagfile/compendium SDKV, as stored.
    /// </summary>
    public string SdkVersion { get; init; } = "";

    /// <summary>
    /// Packfile header version (4, 5, 8, 9 or 11); -1 for other containers.
    /// </summary>
    public int HeaderVersion { get; init; } = -1;

    /// <summary>
    /// Packfile pointer size in bytes; tagfiles are always 8.
    /// </summary>
    public int PointerSize { get; init; }

    public bool BigEndian { get; init; }

    /// <summary>
    /// Tagfile: its types are in a compendium (TCRF section) rather than in the file (TYPE section).
    /// </summary>
    public bool UsesCompendium { get; init; }

    /// <summary>
    /// Tagfile: the id of the compendium it refers to (TCRF), or 0.
    /// </summary>
    public ulong CompendiumRef { get; init; }

    /// <summary>
    /// Compendium: the ids tagfiles use to refer to it (TCID).
    /// </summary>
    public IReadOnlyList<ulong> CompendiumIds { get; init; } = Array.Empty<ulong>();

    /// <summary>
    /// Packfile: the class names in the class-name section, in file order.
    /// </summary>
    public IReadOnlyList<string> ClassNames { get; init; } = Array.Empty<string>();

    /// <summary>
    /// First four bytes, little-endian, for reporting unknown files.
    /// </summary>
    public uint Magic { get; init; }

    public override string ToString() => Container switch
    {
        HavokContainer.Packfile => $"Packfile v{HeaderVersion} {SdkVersion} {PointerSize * 8}-bit {(BigEndian ? "BE" : "LE")}",
        HavokContainer.Tagfile => $"Tagfile {SdkVersion}{(UsesCompendium ? " +compendium" : "")}",
        HavokContainer.Compendium => $"Compendium {SdkVersion}",
        _ => $"{Container} {Magic:X8}"
    };

    private const uint PackfileMagic = 0x57E0E057;

    /// <summary>
    /// Reads the header of a decompressed Havok file. Never throws: unrecognised or truncated data gives
    /// <see cref="HavokContainer.Unknown" /> or as much as could be read.
    /// </summary>
    public static HavokFileInfo Read(ReadOnlySpan<byte> data)
    {
        try
        {
            return ReadUnchecked(data);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Truncated or corrupt: a section or string runs past the end of the data
            return new HavokFileInfo { Magic = data.Length >= 4 ? BinaryPrimitives.ReadUInt32LittleEndian(data) : 0 };
        }
    }

    private static HavokFileInfo ReadUnchecked(ReadOnlySpan<byte> data)
    {
        if (data.Length < 8)
            return new HavokFileInfo();

        uint magic = BinaryPrimitives.ReadUInt32LittleEndian(data);

        // The packfile magics are palindromes, so they read the same in either byte order
        if (magic == PackfileMagic)
            return ReadPackfile(data, magic);

        if (magic is 0xCAB00D1E or 0x1E0DB0CA or 0xD011FACE or 0xCEFA11D0)
            return new HavokFileInfo { Container = HavokContainer.LegacyTagfile, Magic = magic };

        string id = Encoding.ASCII.GetString(data.Slice(4, 4));
        if (id is "TAG0" or "TCM0")
            return ReadTagfile(data, magic, id == "TAG0" ? HavokContainer.Tagfile : HavokContainer.Compendium);

        if (LooksLikeXml(data))
            return new HavokFileInfo { Container = HavokContainer.Xml, Magic = magic };

        return new HavokFileInfo { Magic = magic };
    }

    private static HavokFileInfo ReadPackfile(ReadOnlySpan<byte> data, uint magic)
    {
        if (data.Length < 0x40)
            return new HavokFileInfo { Container = HavokContainer.Packfile, Magic = magic };

        bool bigEndian = data[0x11] == 0;
        int I32(ReadOnlySpan<byte> d, int o) => bigEndian ? BinaryPrimitives.ReadInt32BigEndian(d[o..]) : BinaryPrimitives.ReadInt32LittleEndian(d[o..]);
        short I16(ReadOnlySpan<byte> d, int o) => bigEndian ? BinaryPrimitives.ReadInt16BigEndian(d[o..]) : BinaryPrimitives.ReadInt16LittleEndian(d[o..]);

        int headerVersion = I32(data, 0x0C);
        string sdkVersion = ReadFixStr(data, 0x28, 16);

        // The class-name section is the first section. From header version 11 the section headers start after an
        // extended header whose size is at 0x3E.
        int sectionStart = headerVersion >= 0x0B ? 0x40 + I16(data, 0x3E) : 0x40;
        var classNames = new List<string>();
        if (sectionStart + 28 <= data.Length)
        {
            int dataStart = I32(data, sectionStart + 20);
            int dataLength = I32(data, sectionStart + 24);
            int pos = dataStart;
            int end = Math.Min(dataStart + dataLength, data.Length);
            while (dataStart >= 0 && pos + 5 < end)
            {
                if ((uint)I32(data, pos) == 0xFFFFFFFF)
                    break;
                pos += 5; // signature, then 0x09
                int s = pos;
                while (pos < end && data[pos] != 0) pos++;
                classNames.Add(Encoding.ASCII.GetString(data[s..pos]));
                pos++;
            }
        }

        return new HavokFileInfo
        {
            Container = HavokContainer.Packfile,
            Magic = magic,
            HeaderVersion = headerVersion,
            PointerSize = data[0x10],
            BigEndian = bigEndian,
            SdkVersion = sdkVersion,
            Sdk = HavokSdkExtensions.FromVersionString(sdkVersion),
            ClassNames = classNames
        };
    }

    private static HavokFileInfo ReadTagfile(ReadOnlySpan<byte> data, uint magic, HavokContainer container)
    {
        string sdkVersion = "";
        bool usesCompendium = false;
        ulong compendiumRef = 0;
        var compendiumIds = new List<ulong>();

        foreach (var (id, start, size) in ChildSections(data, 0))
        {
            switch (id)
            {
                case "SDKV":
                    sdkVersion = ReadFixStr(data, start + 8, size - 8);
                    break;
                case "TCRF":
                    usesCompendium = true;
                    if (size >= 16)
                        compendiumRef = BinaryPrimitives.ReadUInt64LittleEndian(data[(start + 8)..]);
                    break;
                case "TCID":
                    for (int p = start + 8; p + 8 <= start + size; p += 8)
                        compendiumIds.Add(BinaryPrimitives.ReadUInt64LittleEndian(data[p..]));
                    break;
            }
        }

        return new HavokFileInfo
        {
            Container = container,
            Magic = magic,
            PointerSize = 8,
            SdkVersion = sdkVersion,
            Sdk = HavokSdkExtensions.FromVersionString(sdkVersion),
            UsesCompendium = usesCompendium,
            CompendiumRef = compendiumRef,
            CompendiumIds = compendiumIds
        };
    }

    /// <summary>
    /// The child sections of a tagfile section: (id, start offset, size including the 8-byte header).
    /// Section headers are a big-endian uint (2 flag bits, 30 size bits) followed by a 4-character id.
    /// </summary>
    private static List<(string Id, int Start, int Size)> ChildSections(ReadOnlySpan<byte> data, int parent)
    {
        var list = new List<(string, int, int)>();
        int parentSize = (int)(BinaryPrimitives.ReadUInt32BigEndian(data[parent..]) & 0x3FFFFFFF);
        int pos = parent + 8;
        int end = Math.Min(parent + parentSize, data.Length);
        while (pos + 8 <= end)
        {
            int size = (int)(BinaryPrimitives.ReadUInt32BigEndian(data[pos..]) & 0x3FFFFFFF);
            if (size < 8)
                break;
            list.Add((Encoding.ASCII.GetString(data.Slice(pos + 4, 4)), pos, size));
            pos += size;
        }
        return list;
    }

    private static string ReadFixStr(ReadOnlySpan<byte> data, int offset, int length)
    {
        if (offset < 0 || offset >= data.Length)
            return "";
        length = Math.Min(length, data.Length - offset);
        int end = offset;
        while (end < offset + length && data[end] != 0 && data[end] != 0xFF) end++;
        return Encoding.ASCII.GetString(data[offset..end]);
    }

    private static bool LooksLikeXml(ReadOnlySpan<byte> data)
    {
        int i = data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF ? 3 : 0;
        while (i < data.Length && data[i] is (byte)' ' or (byte)'\r' or (byte)'\n' or (byte)'\t') i++;
        return i < data.Length && data[i] == '<';
    }
}
