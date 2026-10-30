#nullable enable
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Renderer;

/// <summary>
///     Reads the Havok binary tagfile container (TAG0) without any per-version class definitions.
///     The container describes its own layout, so one reader covers every SDK version the games use:
///     20150100 (DS1R), 20160200 (SDT), 20180100 (ER, NR) and 20190100 (AC6).
///     HKLib's serializer is bound to 20180100, which is why it is not used here.
/// </summary>
public sealed class HavokTagfile
{
    private const int OptionalFormat = 1 << 0;
    private const int OptionalSubType = 1 << 1;
    private const int OptionalVersion = 1 << 2;
    private const int OptionalSizeAlign = 1 << 3;
    private const int OptionalFlags = 1 << 4;
    private const int OptionalMembers = 1 << 5;
    private const int OptionalInterfaces = 1 << 6;

    private readonly byte[] _data;
    private readonly List<Item> _items = new();
    private long _dataStart;

    private HavokTagfile(byte[] data)
    {
        _data = data;
    }

    public static bool IsTagfile(ReadOnlySpan<byte> data)
    {
        return data.Length >= 8 && Encoding.ASCII.GetString(data.Slice(4, 4)) == "TAG0";
    }

    /// <summary>
    ///     Reads the types stored in a compendium (TCM0), which the tagfiles of later games refer to.
    /// </summary>
    public static List<TagType> ReadCompendium(byte[] data)
    {
        Section root = ReadSection(data, 0);
        if (root.Id != "TCM0")
        {
            throw new InvalidOperationException($"Expected a TCM0 compendium, found {root.Id}.");
        }

        foreach (Section section in EnumerateSections(data, root))
        {
            if (section.Id == "TYPE")
            {
                return ReadTypes(data, section);
            }
        }

        throw new InvalidOperationException("Compendium has no TYPE section.");
    }

    public static HavokTagfile Read(byte[] data, List<TagType>? compendiumTypes)
    {
        HavokTagfile file = new(data);
        Section root = ReadSection(data, 0);
        if (root.Id != "TAG0")
        {
            throw new InvalidOperationException($"Expected a TAG0 tagfile, found {root.Id}.");
        }

        List<TagType>? types = null;
        Section? index = null;

        foreach (Section section in EnumerateSections(data, root))
        {
            switch (section.Id)
            {
                case "DATA":
                    file._dataStart = section.Start;
                    break;
                case "TYPE":
                    types = ReadTypes(data, section);
                    break;
                case "TCRF":
                    types = compendiumTypes ??
                            throw new InvalidOperationException("Tagfile refers to a compendium, but none was found.");
                    break;
                case "INDX":
                    index = section;
                    break;
            }
        }

        if (types == null || index == null)
        {
            throw new InvalidOperationException("Tagfile is missing its type or index section.");
        }

        foreach (Section section in EnumerateSections(data, index.Value))
        {
            if (section.Id == "ITEM")
            {
                file.ReadItems(section, types);
            }
        }

        return file;
    }

    /// <summary>
    ///     Finds every object of the given class. Going through the item list avoids walking the
    ///     root level container, whose layout is not needed for anything else.
    /// </summary>
    public IEnumerable<TagObject> FindObjects(string typeName)
    {
        foreach (Item item in _items)
        {
            if (item.Type == null || !item.IsPointer)
            {
                continue;
            }

            for (TagType? type = item.Type; type != null; type = type.Parent)
            {
                if (type.Name == typeName)
                {
                    yield return new TagObject(this, item.Type, item.Offset);
                    break;
                }
            }
        }
    }

    private void ReadItems(Section section, List<TagType> types)
    {
        for (long pos = section.Start; pos + 12 <= section.End; pos += 12)
        {
            uint typeAndFlags = BitConverter.ToUInt32(_data, (int)pos);
            int typeIndex = (int)(typeAndFlags & 0xFFFFFF);
            int kind = (int)(typeAndFlags >> 28);
            int offset = BitConverter.ToInt32(_data, (int)pos + 4);
            int count = BitConverter.ToInt32(_data, (int)pos + 8);

            _items.Add(new Item
            {
                Type = typeIndex > 0 && typeIndex < types.Count ? types[typeIndex] : null,
                IsPointer = kind == 1,
                Offset = _dataStart + offset,
                Count = count
            });
        }
    }

    private Item GetItem(long fieldOffset)
    {
        // Pointers hold an index into the item list rather than an address
        var index = BitConverter.ToUInt64(_data, (int)fieldOffset);
        return index < (ulong)_items.Count ? _items[(int)index] : default;
    }

    #region Types

    private static List<TagType> ReadTypes(byte[] data, Section typeSection)
    {
        List<string> typeStrings = new();
        List<string> fieldStrings = new();
        List<TagType> types = new();
        Section? body = null;

        foreach (Section section in EnumerateSections(data, typeSection))
        {
            switch (section.Id)
            {
                case "TSTR":
                case "TST1":
                    typeStrings = ReadStrings(data, section);
                    break;
                case "FSTR":
                case "FST1":
                    fieldStrings = ReadStrings(data, section);
                    break;
                case "TNAM":
                case "TNA1":
                    types = ReadTypeNames(data, section, typeStrings);
                    break;
                case "TBOD":
                case "TBDY":
                    body = section;
                    break;
            }
        }

        if (body != null)
        {
            ReadTypeBodies(data, body.Value, types, fieldStrings);
        }

        return types;
    }

    private static List<string> ReadStrings(byte[] data, Section section)
    {
        List<string> strings = new();
        var start = (int)section.Start;

        for (var i = start; i < section.End; i++)
        {
            if (data[i] == 0)
            {
                strings.Add(Encoding.ASCII.GetString(data, start, i - start));
                start = i + 1;
            }
        }

        return strings;
    }

    private static List<TagType> ReadTypeNames(byte[] data, Section section, List<string> typeStrings)
    {
        long pos = section.Start;
        var count = (int)ReadVarUInt(data, ref pos);

        // Index 0 is the null type, which is not stored
        List<TagType> types = new(count);
        for (var i = 0; i < count; i++)
        {
            types.Add(new TagType());
        }

        for (var i = 1; i < count; i++)
        {
            types[i].Name = typeStrings[(int)ReadVarUInt(data, ref pos)];

            var templateCount = (int)ReadVarUInt(data, ref pos);
            for (var j = 0; j < templateCount; j++)
            {
                ReadVarUInt(data, ref pos);
                ReadVarUInt(data, ref pos);
            }
        }

        return types;
    }

    private static void ReadTypeBodies(byte[] data, Section section, List<TagType> types, List<string> fieldStrings)
    {
        long pos = section.Start;

        while (pos < section.End)
        {
            var typeIndex = (int)ReadVarUInt(data, ref pos);
            if (typeIndex == 0)
            {
                // Padding at the end of the section
                continue;
            }

            TagType type = types[typeIndex];

            var parentIndex = (int)ReadVarUInt(data, ref pos);
            if (parentIndex > 0)
            {
                type.Parent = types[parentIndex];
            }

            var optionals = (int)ReadVarUInt(data, ref pos);

            if ((optionals & OptionalFormat) != 0)
            {
                type.OwnFormat = (int)ReadVarUInt(data, ref pos);
            }

            if ((optionals & OptionalSubType) != 0)
            {
                var subTypeIndex = (int)ReadVarUInt(data, ref pos);
                type.OwnSubType = subTypeIndex > 0 ? types[subTypeIndex] : null;
            }

            if ((optionals & OptionalVersion) != 0)
            {
                ReadVarUInt(data, ref pos);
            }

            if ((optionals & OptionalSizeAlign) != 0)
            {
                type.OwnSize = (int)ReadVarUInt(data, ref pos);
                ReadVarUInt(data, ref pos);
            }

            if ((optionals & OptionalFlags) != 0)
            {
                ReadVarUInt(data, ref pos);
            }

            if ((optionals & OptionalMembers) != 0)
            {
                // The two high bytes are the number of properties, which have no storage
                var fieldCount = (int)(ReadVarUInt(data, ref pos) & 0xFFFF);
                for (var i = 0; i < fieldCount; i++)
                {
                    TagField field = new()
                    {
                        Name = fieldStrings[(int)ReadVarUInt(data, ref pos)]
                    };

                    ReadVarUInt(data, ref pos);
                    field.Offset = (int)ReadVarUInt(data, ref pos);
                    field.Type = types[(int)ReadVarUInt(data, ref pos)];

                    type.Fields.Add(field);
                }
            }

            if ((optionals & OptionalInterfaces) != 0)
            {
                var interfaceCount = (int)ReadVarUInt(data, ref pos);
                for (var i = 0; i < interfaceCount; i++)
                {
                    ReadVarUInt(data, ref pos);
                    ReadVarUInt(data, ref pos);
                }
            }
        }
    }

    // The leading bits of the first byte give the length, the rest of the bits are big endian
    private static ulong ReadVarUInt(byte[] data, ref long pos)
    {
        byte first = data[pos++];

        if ((first & 0x80) == 0)
        {
            return first;
        }

        if ((first & 0xC0) == 0x80)
        {
            return (ulong)(first & 0x3F) << 8 | data[pos++];
        }

        if ((first & 0xE0) == 0xC0)
        {
            ulong value = (ulong)(first & 0x1F) << 16 | (ulong)data[pos++] << 8;
            return value | data[pos++];
        }

        if ((first & 0xF8) == 0xE0)
        {
            ulong value = (ulong)(first & 0x07) << 24 | (ulong)data[pos++] << 16 | (ulong)data[pos++] << 8;
            return value | data[pos++];
        }

        if ((first & 0xF8) == 0xE8)
        {
            ulong value = (ulong)(first & 0x07) << 32 | (ulong)data[pos++] << 24 | (ulong)data[pos++] << 16;
            value |= (ulong)data[pos++] << 8;
            return value | data[pos++];
        }

        throw new InvalidOperationException($"Unsupported variable length integer prefix 0x{first:X2}.");
    }

    #endregion

    #region Sections

    private static Section ReadSection(byte[] data, long position)
    {
        // Big endian, with the top bits used as flags
        var pos = (int)position;
        var length = (data[pos] & 0x0F) << 24 | data[pos + 1] << 16 | data[pos + 2] << 8 | data[pos + 3];

        return new Section
        {
            Id = Encoding.ASCII.GetString(data, pos + 4, 4),
            Start = position + 8,
            End = position + length
        };
    }

    private static IEnumerable<Section> EnumerateSections(byte[] data, Section parent)
    {
        long pos = parent.Start;

        while (pos + 8 <= parent.End)
        {
            Section section = ReadSection(data, pos);
            if (section.End <= pos)
            {
                yield break;
            }

            yield return section;
            pos = section.End;
        }
    }

    private struct Section
    {
        public string Id;
        public long Start;
        public long End;
    }

    #endregion

    private struct Item
    {
        public TagType? Type;
        public bool IsPointer;
        public long Offset;
        public int Count;
    }

    public sealed class TagType
    {
        public string Name = "";
        public TagType? Parent;
        public int? OwnFormat;
        public TagType? OwnSubType;
        public int? OwnSize;
        public readonly List<TagField> Fields = new();

        public int Size => OwnSize ?? Parent?.Size ?? 0;

        public TagField? FindField(string name)
        {
            for (TagType? type = this; type != null; type = type.Parent)
            {
                foreach (TagField field in type.Fields)
                {
                    if (field.Name == name || field.Name == "m_" + name)
                    {
                        return field;
                    }
                }
            }

            return null;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public sealed class TagField
    {
        public string Name = "";
        public int Offset;
        public TagType Type = null!;
    }

    /// <summary>
    ///     A view of a record in the data section. Absent fields read as defaults, since
    ///     SDK versions differ in the fields they keep.
    /// </summary>
    public readonly struct TagObject
    {
        private readonly HavokTagfile _file;
        private readonly long _offset;

        public TagType Type { get; }

        public TagObject(HavokTagfile file, TagType type, long offset)
        {
            _file = file;
            Type = type;
            _offset = offset;
        }

        private bool TryGetField(string name, out long offset)
        {
            TagField? field = Type.FindField(name);
            offset = field != null ? _offset + field.Offset : 0;
            return field != null;
        }

        public int GetInt32(string name)
        {
            return TryGetField(name, out var pos) ? BitConverter.ToInt32(_file._data, (int)pos) : 0;
        }

        public byte GetByte(string name)
        {
            return TryGetField(name, out var pos) ? _file._data[pos] : (byte)0;
        }

        public float GetSingle(string name)
        {
            return TryGetField(name, out var pos) ? BitConverter.ToSingle(_file._data, (int)pos) : 0;
        }

        public string GetString(string name)
        {
            if (!TryGetField(name, out var pos))
            {
                return "";
            }

            return _file.ReadCString(pos);
        }

        public short[] GetInt16Array(string name)
        {
            if (!TryGetField(name, out var pos))
            {
                return Array.Empty<short>();
            }

            Item item = _file.GetItem(pos);
            var result = new short[item.Count];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = BitConverter.ToInt16(_file._data, (int)item.Offset + i * 2);
            }

            return result;
        }

        public byte[] GetByteArray(string name)
        {
            if (!TryGetField(name, out var pos))
            {
                return Array.Empty<byte>();
            }

            Item item = _file.GetItem(pos);
            var result = new byte[item.Count];
            Array.Copy(_file._data, item.Offset, result, 0, item.Count);
            return result;
        }

        /// <summary>
        ///     Reads an array of a record type, as (reader for element, element count).
        /// </summary>
        public TagObject[] GetObjectArray(string name)
        {
            if (!TryGetField(name, out var pos))
            {
                return Array.Empty<TagObject>();
            }

            Item item = _file.GetItem(pos);
            if (item.Type == null)
            {
                return Array.Empty<TagObject>();
            }

            var result = new TagObject[item.Count];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = new TagObject(_file, item.Type, item.Offset + i * item.Type.Size);
            }

            return result;
        }

        /// <summary>
        ///     Reads the hkQsTransform array of a field: translation, rotation and scale, each four floats.
        /// </summary>
        public HavokTransformData[] GetTransformArray(string name)
        {
            if (!TryGetField(name, out var pos))
            {
                return Array.Empty<HavokTransformData>();
            }

            Item item = _file.GetItem(pos);
            var result = new HavokTransformData[item.Count];
            byte[] data = _file._data;

            for (var i = 0; i < result.Length; i++)
            {
                var o = (int)item.Offset + i * 48;
                float F(int index)
                {
                    return BitConverter.ToSingle(data, o + index * 4);
                }

                result[i] = new HavokTransformData
                {
                    Translation = new Vector3(F(0), F(1), F(2)),
                    Rotation = new Quaternion(F(4), F(5), F(6), F(7)),
                    Scale = new Vector3(F(8), F(9), F(10))
                };
            }

            return result;
        }
    }

    private string ReadCString(long fieldOffset)
    {
        Item item = GetItem(fieldOffset);
        if (item.Count <= 0)
        {
            return "";
        }

        var length = Array.IndexOf(_data, (byte)0, (int)item.Offset, item.Count) - (int)item.Offset;
        return Encoding.ASCII.GetString(_data, (int)item.Offset, length < 0 ? item.Count : length);
    }
}
