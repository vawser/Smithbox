using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// An AC6 design file (<c>param/asmparam/asmparam.designbnd.dcx</c>, one <c>{id}.design</c> per CharaInitParam
/// assemDataID). It is the AC data block of the save format: a <c>---- begin ----</c> header, named sections
/// (name[16], s32 size, s32 version, 8 bytes padding, data) and a <c>----  end  ----</c> trailer. Most files are
/// wrapped in an <c>ASMC</c> zlib container.
/// <para>
/// The header, trailer and every section are kept as read, so a file round-trips; only the values the tool edits are
/// rewritten. See Claude/Documentation/AC6 Design Files.md.
/// </para>
/// </summary>
public class AcDesign
{
    public const string SectionUgcId = "UgcID";
    public const string SectionDataName = "DataName";
    public const string SectionAcName = "AcName";
    public const string SectionAssemble = "Assemble";
    public const string SectionColoring = "Coloring";

    private const int HeaderSize = 32;
    private const int SectionHeaderSize = 32;
    private const int AsmcHeaderSize = 16;

    public const int ColoringRecordSize = 68;
    public const int ColoringRecordCount = 14;

    /// <summary>
    /// The named colours of a coloring record, at offset 8 (RGBA bytes each)
    /// </summary>
    public const int ColoringMainColorCount = 6;

    /// <summary>
    /// The further colours of a coloring record, at offset 48 (meaning unknown)
    /// </summary>
    public const int ColoringExtraColorCount = 4;

    public class Section
    {
        public string Name;
        public int Version;
        public long Padding;
        public byte[] Data;
    }

    /// <summary>
    /// True when the file was in an ASMC container; it is written back the same way
    /// </summary>
    public bool Compressed;

    /// <summary>
    /// The ASMC container's second field (0x00291222 in every vanilla file)
    /// </summary>
    public int AsmcVersion;

    public byte[] Header = Array.Empty<byte>();
    public byte[] Trailer = Array.Empty<byte>();
    public List<Section> Sections = new();

    public static bool TryRead(byte[] bytes, out AcDesign design, out string error)
    {
        design = null;
        error = null;

        try
        {
            design = Read(bytes);
            return true;
        }
        catch (Exception e)
        {
            error = e.Message;
            return false;
        }
    }

    public static AcDesign Read(byte[] bytes)
    {
        var design = new AcDesign();
        var plain = bytes;

        if (bytes.Length >= AsmcHeaderSize && Encoding.ASCII.GetString(bytes, 0, 4) == "ASMC")
        {
            design.Compressed = true;
            design.AsmcVersion = BitConverter.ToInt32(bytes, 4);

            var compressedSize = BitConverter.ToInt32(bytes, 8);
            var plainSize = BitConverter.ToInt32(bytes, 12);

            if (compressedSize < 0 || AsmcHeaderSize + compressedSize > bytes.Length || plainSize < 0)
                throw new InvalidDataException("ASMC sizes are out of range.");

            using var zlib = new ZLibStream(new MemoryStream(bytes, AsmcHeaderSize, compressedSize), CompressionMode.Decompress);
            plain = new byte[plainSize];
            zlib.ReadExactly(plain);
        }

        if (plain.Length < HeaderSize || !Encoding.ASCII.GetString(plain, 0, 4).StartsWith("----"))
            throw new InvalidDataException("Missing the '---- begin ----' header.");

        design.Header = plain[..HeaderSize];

        var pos = HeaderSize;

        while (true)
        {
            if (pos + 4 > plain.Length)
                throw new InvalidDataException("Missing the '----  end  ----' trailer.");

            if (Encoding.ASCII.GetString(plain, pos, 4) == "----")
                break;

            if (pos + SectionHeaderSize > plain.Length)
                throw new InvalidDataException($"Section header at 0x{pos:X} runs past the end.");

            var section = new Section
            {
                Name = Encoding.ASCII.GetString(plain, pos, 16).TrimEnd('\0'),
                Version = BitConverter.ToInt32(plain, pos + 20),
                Padding = BitConverter.ToInt64(plain, pos + 24)
            };

            var size = BitConverter.ToInt32(plain, pos + 16);

            if (size < 0 || pos + SectionHeaderSize + size > plain.Length)
                throw new InvalidDataException($"Section '{section.Name}' runs past the end.");

            section.Data = plain[(pos + SectionHeaderSize)..(pos + SectionHeaderSize + size)];
            design.Sections.Add(section);

            pos += SectionHeaderSize + size;
        }

        design.Trailer = plain[pos..];

        return design;
    }

    public byte[] Write()
    {
        var ms = new MemoryStream();
        ms.Write(Header);

        foreach (var section in Sections)
        {
            var name = new byte[16];
            Encoding.ASCII.GetBytes(section.Name, 0, Math.Min(section.Name.Length, 15), name, 0);

            ms.Write(name);
            ms.Write(BitConverter.GetBytes(section.Data.Length));
            ms.Write(BitConverter.GetBytes(section.Version));
            ms.Write(BitConverter.GetBytes(section.Padding));
            ms.Write(section.Data);
        }

        ms.Write(Trailer);

        var plain = ms.ToArray();

        if (!Compressed)
            return plain;

        var compressed = new MemoryStream();

        using (var zlib = new ZLibStream(compressed, CompressionLevel.SmallestSize, leaveOpen: true))
            zlib.Write(plain);

        var output = new MemoryStream();
        output.Write(Encoding.ASCII.GetBytes("ASMC"));
        output.Write(BitConverter.GetBytes(AsmcVersion));
        output.Write(BitConverter.GetBytes((int)compressed.Length));
        output.Write(BitConverter.GetBytes(plain.Length));
        output.Write(compressed.ToArray());

        return output.ToArray();
    }

    public Section GetSection(string name)
    {
        return Sections.FirstOrDefault(s => s.Name == name);
    }

    #region Strings

    /// <summary>
    /// A UTF-16 string section (AcName, DataName, UgcID), or null when the file has no such section
    /// </summary>
    public string GetString(string sectionName)
    {
        var section = GetSection(sectionName);

        if (section == null)
            return null;

        var text = Encoding.Unicode.GetString(section.Data);
        var end = text.IndexOf('\0');

        return end >= 0 ? text[..end] : text;
    }

    public void SetString(string sectionName, string value)
    {
        var section = GetSection(sectionName);

        if (section == null)
            return;

        section.Data = Encoding.Unicode.GetBytes((value ?? "") + "\0");
    }

    #endregion

    #region Assemble

    public int AssembleVersion => GetSection(SectionAssemble)?.Version ?? -1;

    public int AssembleSlotCount => (GetSection(SectionAssemble)?.Data.Length ?? 0) / 4;

    /// <summary>
    /// The slot's value as stored: the equip ID with the slot's type in the top four bits, or -1 when empty
    /// </summary>
    public int GetRawSlot(int index)
    {
        var data = GetSection(SectionAssemble)?.Data;

        if (data == null || index * 4 + 4 > data.Length)
            return -1;

        return BitConverter.ToInt32(data, index * 4);
    }

    public void SetRawSlot(int index, int value)
    {
        var data = GetSection(SectionAssemble)?.Data;

        if (data == null || index * 4 + 4 > data.Length)
            return;

        BitConverter.GetBytes(value).CopyTo(data, index * 4);
    }

    /// <summary>
    /// The equip param row ID in the slot, or -1 when empty. A value whose type bits don't match the slot is returned as stored.
    /// </summary>
    public int GetSlotId(AcAssemblySlot slot)
    {
        var raw = GetRawSlot(slot.Index);

        if (raw == -1 || slot.TypePrefix == 0)
            return raw;

        return ((uint)raw >> 28) == slot.TypePrefix ? raw & 0x0FFFFFFF : raw;
    }

    public void SetSlotId(AcAssemblySlot slot, int id)
    {
        if (id < 0)
        {
            SetRawSlot(slot.Index, -1);
            return;
        }

        SetRawSlot(slot.Index, (int)((uint)slot.TypePrefix << 28) | (id & 0x0FFFFFFF));
    }

    #endregion

    #region Coloring

    public bool HasColoring => (GetSection(SectionColoring)?.Data.Length ?? 0) >= ColoringRecordSize * ColoringRecordCount;

    private static int ColorOffset(int record, int color, bool extra)
    {
        return record * ColoringRecordSize + (extra ? 48 : 8) + color * 4;
    }

    /// <summary>
    /// A colour as 0-1 RGBA
    /// </summary>
    public Vector4 GetColor(int record, int color, bool extra = false)
    {
        var data = GetSection(SectionColoring).Data;
        var o = ColorOffset(record, color, extra);

        return new Vector4(data[o], data[o + 1], data[o + 2], data[o + 3]) / 255f;
    }

    public void SetColor(int record, int color, Vector4 value, bool extra = false)
    {
        var data = GetSection(SectionColoring).Data;
        var o = ColorOffset(record, color, extra);

        data[o] = ToByte(value.X);
        data[o + 1] = ToByte(value.Y);
        data[o + 2] = ToByte(value.Z);
        data[o + 3] = ToByte(value.W);
    }

    /// <summary>
    /// Copies a whole coloring record (its colours and every other value in it) over another
    /// </summary>
    public void CopyColoringRecord(int from, int to)
    {
        var data = GetSection(SectionColoring).Data;
        Array.Copy(data, from * ColoringRecordSize, data, to * ColoringRecordSize, ColoringRecordSize);
    }

    private static byte ToByte(float v)
    {
        return (byte)Math.Clamp((int)MathF.Round(v * 255f), 0, 255);
    }

    #endregion
}

/// <summary>
/// One slot of the Assemble section. TypePrefix is the equip type stored in the top four bits of the value.
/// RowField is the CharaInitParam field holding the same part, used to fill a design from its row.
/// </summary>
public record AcAssemblySlot(int Index, string LocKey, string ParamName, uint TypePrefix, string RowField)
{
    /// <summary>
    /// The slots of a version 2 or 3 Assemble section that are known. Index 7 and 14 are always empty in vanilla
    /// designs, and 12 and 13 hold the BackRail Weapon Loader rows (299300, 299100).
    /// </summary>
    public static readonly AcAssemblySlot[] Known =
    {
        new(9, "PARAM_AcAssembly_Slot_RightArm", "EquipParamWeapon", 0, "equip_Wep_Right"),
        new(8, "PARAM_AcAssembly_Slot_LeftArm", "EquipParamWeapon", 0, "equip_Wep_Left"),
        new(11, "PARAM_AcAssembly_Slot_RightBack", "EquipParamWeapon", 0, "equip_Bolt"),
        new(10, "PARAM_AcAssembly_Slot_LeftBack", "EquipParamWeapon", 0, "equip_Arrow"),
        new(0, "PARAM_AcAssembly_Slot_Head", "EquipParamProtector", 1, "equip_Helm"),
        new(1, "PARAM_AcAssembly_Slot_Core", "EquipParamProtector", 1, "equip_Armer"),
        new(2, "PARAM_AcAssembly_Slot_Arms", "EquipParamProtector", 1, "equip_Gaunt"),
        new(3, "PARAM_AcAssembly_Slot_Legs", "EquipParamProtector", 1, "equip_Leg"),
        new(4, "PARAM_AcAssembly_Slot_Booster", "EquipParamBooster", 6, "equip_Booster_Main"),
        new(6, "PARAM_AcAssembly_Slot_Fcs", "EquipParamFcs", 7, "equip_Fcs"),
        new(5, "PARAM_AcAssembly_Slot_Generator", "EquipParamGenerator", 5, "equip_Generator"),
        new(15, "PARAM_AcAssembly_Slot_Expansion", "EquipParamWeapon", 0, "equip_AssaultDrive"),
    };

    public static readonly int[] KnownIndices = Known.Select(s => s.Index).ToArray();
}
