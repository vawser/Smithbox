using System.Buffers.Binary;
using System.Text;

/// <summary>
/// What the file says about itself, read without any of the three libraries.
/// </summary>
public sealed class SniffInfo
{
    public string Container = "Unknown";   // Packfile, Tagfile, Compendium, OldTagfile, Unknown
    public string Sdk = "";                // packfile ContentsVersionString or tagfile SDKV
    public int HeaderVersion = -1;         // packfile header version (5, 8, 11)
    public int PointerSize = -1;
    public bool BigEndian;
    public bool UsesCompendium;            // tagfile has TCRF instead of TYPE
    public ulong CompendiumRef;            // tagfile: the TCRF id
    public List<ulong> CompendiumIds = new(); // compendium: the TCID ids
    public string Magic = "";
    public List<string> Classes = new();   // packfile class-name section; tagfile TSTR strings (fallback)

    public string FormatKey =>
        Container switch
        {
            "Packfile" => $"Packfile v{HeaderVersion} {Sdk} {PointerSize * 8}-bit {(BigEndian ? "BE" : "LE")}",
            "Tagfile" => $"Tagfile {Sdk}{(UsesCompendium ? " +compendium" : "")}",
            "Compendium" => $"Compendium {Sdk}",
            _ => $"{Container} {Magic}"
        };
}

public static class Sniffer
{
    public static SniffInfo Sniff(byte[] data)
    {
        var info = new SniffInfo();
        if (data.Length < 8)
            return info;

        info.Magic = $"{BinaryPrimitives.ReadUInt32LittleEndian(data):X8}";
        var magicLE = BinaryPrimitives.ReadUInt32LittleEndian(data);

        // The packfile magics are palindromes, so they read the same in either byte order
        if (magicLE == 0x57E0E057)
        {
            SniffPackfile(data, info);
            return info;
        }

        if (magicLE == 0xCAB00D1E || magicLE == 0x1E0DB0CA || magicLE == 0xD011FACE || magicLE == 0xCEFA11D0)
        {
            info.Container = "OldTagfile";
            return info;
        }

        var id = Encoding.ASCII.GetString(data, 4, 4);
        if (id is "TAG0" or "TCM0")
        {
            info.Container = id == "TAG0" ? "Tagfile" : "Compendium";
            SniffTagfile(data, info);
        }

        return info;
    }

    private static void SniffPackfile(byte[] data, SniffInfo info)
    {
        info.Container = "Packfile";
        info.BigEndian = data[0x11] == 0;
        int I32(int o) => info.BigEndian ? BinaryPrimitives.ReadInt32BigEndian(data.AsSpan(o)) : BinaryPrimitives.ReadInt32LittleEndian(data.AsSpan(o));
        short I16(int o) => info.BigEndian ? BinaryPrimitives.ReadInt16BigEndian(data.AsSpan(o)) : BinaryPrimitives.ReadInt16LittleEndian(data.AsSpan(o));

        info.HeaderVersion = I32(0x0C);
        info.PointerSize = data[0x10];
        info.Sdk = ReadFixStr(data, 0x28, 16);

        // Class-name section is the first section
        var sectionStart = 0x40;
        if (info.HeaderVersion >= 0x0B)
        {
            sectionStart = 0x40 + I16(0x3E);
        }

        try
        {
            var dataStart = I32(sectionStart + 20);
            var dataLength = I32(sectionStart + 24);
            var pos = dataStart;
            var end = dataStart + dataLength;
            while (pos + 5 < end)
            {
                var sig = (uint)I32(pos);
                if (sig == 0xFFFFFFFF)
                    break;
                pos += 5; // signature + 0x09
                var s = pos;
                while (pos < end && data[pos] != 0) pos++;
                info.Classes.Add(Encoding.ASCII.GetString(data, s, pos - s));
                pos++;
            }
        }
        catch
        {
            info.Classes.Add("<class names unreadable>");
        }
    }

    private static void SniffTagfile(byte[] data, SniffInfo info)
    {
        foreach (var (id, start, size) in Sections(data, 0))
        {
            switch (id)
            {
                case "SDKV":
                    info.Sdk = ReadFixStr(data, start + 8, size - 8);
                    break;
                case "TCRF":
                    info.UsesCompendium = true;
                    if (size >= 16)
                        info.CompendiumRef = BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(start + 8));
                    break;
                case "TCID":
                    for (var p = start + 8; p + 8 <= start + size; p += 8)
                        info.CompendiumIds.Add(BinaryPrimitives.ReadUInt64LittleEndian(data.AsSpan(p)));
                    break;
                case "TYPE":
                    foreach (var (cid, cstart, csize) in Sections(data, start))
                    {
                        if (cid != "TSTR")
                            continue;
                        var pos = cstart + 8;
                        var end = cstart + csize;
                        while (pos < end)
                        {
                            var s = pos;
                            while (pos < end && data[pos] != 0) pos++;
                            if (pos > s)
                                info.Classes.Add(Encoding.ASCII.GetString(data, s, pos - s));
                            pos++;
                        }
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// The child sections of the section at <paramref name="parent"/>.
    /// </summary>
    private static IEnumerable<(string Id, int Start, int Size)> Sections(byte[] data, int parent)
    {
        var parentSize = (int)(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(parent)) & 0x3FFFFFFF);
        var pos = parent + 8;
        var end = Math.Min(parent + parentSize, data.Length);
        while (pos + 8 <= end)
        {
            var size = (int)(BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(pos)) & 0x3FFFFFFF);
            if (size < 8)
                yield break;
            yield return (Encoding.ASCII.GetString(data, pos + 4, 4), pos, size);
            pos += size;
        }
    }

    private static string ReadFixStr(byte[] data, int offset, int length)
    {
        length = Math.Min(length, data.Length - offset);
        var end = offset;
        while (end < offset + length && data[end] != 0 && data[end] != 0xFF) end++;
        return Encoding.ASCII.GetString(data, offset, end - offset);
    }
}
