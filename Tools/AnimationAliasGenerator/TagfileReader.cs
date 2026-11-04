using SoulsFormats;
using System.Text;

/// <summary>
/// A minimal reader for Havok tagfiles (TAG0, 2016 onwards). The layout of every type is described by the file
/// itself, so objects are read by member name and offset rather than through generated classes. This copes with
/// versions and class layouts that the class libraries don't know, such as Sekiro's 2016 files and Nightreign's
/// changed selectors. Only what is needed to read names, ints and object arrays is supported.
/// </summary>
public sealed class TagfileReader
{
    public sealed class TagType
    {
        public string Name = "";
        public TagType Parent;
        public int Format;
        public TagType SubType;
        public int Size;
        public int Alignment;
        public List<TagMember> Members = new();

        public TagMember FindMember(string name)
        {
            for (var type = this; type != null; type = type.Parent)
            {
                var member = type.Members.FirstOrDefault(e => e.Name == name || e.Name == $"m_{name}");

                if (member != null)
                    return member;
            }

            return null;
        }

        public bool IsOrInherits(string name)
        {
            for (var type = this; type != null; type = type.Parent)
            {
                if (type.Name == name)
                    return true;
            }

            return false;
        }

        public override string ToString() => Name;
    }

    public sealed record TagMember(string Name, int Offset, TagType Type);

    public sealed record TagItem(TagType Type, long Offset, int Count);

    private readonly BinaryReaderEx Reader;

    public List<TagType> Types { get; private set; } = new();
    public List<TagItem> Items { get; } = new();

    public string SdkVersion { get; private set; }

    private long DataOffset;

    public TagfileReader(byte[] bytes, List<TagType> compendiumTypes = null)
    {
        Reader = new BinaryReaderEx(false, bytes);

        var end = EnterSection("TAG0");

        while (Reader.Position < end)
        {
            var (id, sectionEnd) = PeekSection();

            switch (id)
            {
                case "SDKV":
                    EnterSection("SDKV");
                    SdkVersion = Reader.ReadASCII(8);
                    break;
                case "DATA":
                    EnterSection("DATA");
                    DataOffset = Reader.Position;
                    break;
                case "TYPE":
                    EnterSection("TYPE");
                    Types = ReadTypes(sectionEnd);
                    break;
                case "TCRF":
                    Types = compendiumTypes ?? throw new InvalidDataException("The file refers to a compendium that wasn't loaded");
                    break;
                case "INDX":
                    EnterSection("INDX");
                    ReadIndex(sectionEnd);
                    break;
            }

            Reader.Position = sectionEnd;
        }
    }

    private TagfileReader(byte[] bytes, bool compendium)
    {
        Reader = new BinaryReaderEx(false, bytes);
    }

    /// <summary>
    /// Reads the types of a compendium (TCM0), for files that refer to them instead of holding their own
    /// </summary>
    public static List<TagType> ReadCompendiumTypes(byte[] bytes)
    {
        var reader = new TagfileReader(bytes, true);

        var end = reader.EnterSection("TCM0");

        while (reader.Reader.Position < end)
        {
            var (id, sectionEnd) = reader.PeekSection();

            if (id == "TYPE")
            {
                reader.EnterSection("TYPE");
                return reader.ReadTypes(sectionEnd);
            }

            reader.Reader.Position = sectionEnd;
        }

        throw new InvalidDataException("No types in compendium");
    }

    #region Sections
    private long EnterSection(string id)
    {
        var (found, end) = PeekSection();

        if (found != id)
            throw new InvalidDataException($"Expected section {id}, found {found}");

        Reader.Position += 8;

        return end;
    }

    private (string Id, long End) PeekSection()
    {
        var start = Reader.Position;
        var bytes = Reader.GetBytes(start, 8);
        var length = ((bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3]) & 0x3FFFFFFF;

        return (Encoding.ASCII.GetString(bytes, 4, 4), start + length);
    }
    #endregion

    #region Types
    private List<TagType> ReadTypes(long end)
    {
        List<string> typeStrings = null;
        List<string> fieldStrings = null;
        List<TagType> types = null;

        while (Reader.Position < end)
        {
            var (id, sectionEnd) = PeekSection();
            Reader.Position += 8;

            switch (id)
            {
                case "TSTR":
                case "TST1":
                    typeStrings = ReadStrings(sectionEnd);
                    break;
                case "FSTR":
                case "FST1":
                    fieldStrings = ReadStrings(sectionEnd);
                    break;
                case "TNAM":
                case "TNA1":
                    types = ReadTypeNames(typeStrings);
                    break;
                case "TBOD":
                case "TBDY":
                    ReadTypeBodies(types, fieldStrings, sectionEnd);
                    break;
            }

            Reader.Position = sectionEnd;
        }

        return types ?? throw new InvalidDataException("No type names");
    }

    private List<string> ReadStrings(long end)
    {
        var strings = new List<string>();
        var current = new StringBuilder();

        while (Reader.Position < end)
        {
            var b = Reader.ReadByte();

            if (b == 0)
            {
                strings.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append((char)b);
            }
        }

        return strings;
    }

    private List<TagType> ReadTypeNames(List<string> typeStrings)
    {
        var count = (int)ReadVarUInt();

        var types = new List<TagType>(count) { new TagType { Name = "NULL" } };

        for (var i = 1; i < count; i++)
        {
            types.Add(new TagType());
        }

        for (var i = 1; i < count; i++)
        {
            types[i].Name = typeStrings[(int)ReadVarUInt()];

            var templateCount = (int)ReadVarUInt();

            for (var j = 0; j < templateCount; j++)
            {
                ReadVarUInt();
                ReadVarUInt();
            }
        }

        return types;
    }

    [Flags]
    private enum Optional
    {
        Format = 1 << 0,
        SubType = 1 << 1,
        Version = 1 << 2,
        SizeAlign = 1 << 3,
        Flags = 1 << 4,
        Members = 1 << 5,
        Interfaces = 1 << 6,
        Attributes = 1 << 7
    }

    private void ReadTypeBodies(List<TagType> types, List<string> fieldStrings, long end)
    {
        while (Reader.Position < end)
        {
            var typeIndex = (int)ReadVarUInt();

            // Padding at the end of the section
            if (typeIndex == 0)
                break;

            var type = types[typeIndex];

            var parentIndex = (int)ReadVarUInt();

            if (parentIndex > 0)
                type.Parent = types[parentIndex];

            var optionals = (Optional)ReadVarUInt();

            if (optionals.HasFlag(Optional.Format))
                type.Format = (int)ReadVarUInt();

            if (optionals.HasFlag(Optional.SubType))
                type.SubType = types[(int)ReadVarUInt()];

            if (optionals.HasFlag(Optional.Version))
                ReadVarUInt();

            if (optionals.HasFlag(Optional.SizeAlign))
            {
                type.Size = (int)ReadVarUInt();
                type.Alignment = (int)ReadVarUInt();
            }

            if (optionals.HasFlag(Optional.Flags))
                ReadVarUInt();

            if (optionals.HasFlag(Optional.Members))
            {
                var counts = (uint)ReadVarUInt();
                var fieldCount = (int)(counts & 0xFFFF);

                for (var j = 0; j < fieldCount; j++)
                {
                    var name = fieldStrings[(int)ReadVarUInt()];
                    ReadVarUInt(); // flags
                    var offset = (int)ReadVarUInt();
                    var memberType = types[(int)ReadVarUInt()];

                    type.Members.Add(new TagMember(name, offset, memberType));
                }
            }

            if (optionals.HasFlag(Optional.Interfaces))
            {
                var interfaceCount = (int)ReadVarUInt();

                for (var j = 0; j < interfaceCount; j++)
                {
                    ReadVarUInt();
                    ReadVarUInt();
                }
            }

            if (optionals.HasFlag(Optional.Attributes))
                throw new NotSupportedException($"Type attributes on {type.Name}");
        }
    }

    /// <summary>
    /// Havok's variable length integers: the high bits of the first byte give the length
    /// </summary>
    private ulong ReadVarUInt()
    {
        var prefix = Reader.GetByte(Reader.Position);
        var length = HKLib.Serialization.hk2018.Binary.Util.HavokVarUInt.GetLength(prefix);

        return HKLib.Serialization.hk2018.Binary.Util.HavokVarUInt.ToUInt64(Reader.ReadBytes(length));
    }
    #endregion

    #region Index
    private void ReadIndex(long end)
    {
        while (Reader.Position < end)
        {
            var (id, sectionEnd) = PeekSection();
            Reader.Position += 8;

            if (id == "ITEM")
            {
                while (Reader.Position < sectionEnd)
                {
                    var typeAndFlags = Reader.ReadUInt32();
                    var typeIndex = (int)(typeAndFlags & 0xFFFFFF);
                    var offset = Reader.ReadInt32();
                    var count = Reader.ReadInt32();

                    Items.Add(new TagItem(Types[typeIndex], DataOffset + offset, count));
                }
            }

            Reader.Position = sectionEnd;
        }
    }
    #endregion

    #region Values
    /// <summary>
    /// The objects of the items whose type is or inherits from the named type, as their positions in DATA
    /// </summary>
    public IEnumerable<(TagType Type, long Offset)> FindObjects(string typeName)
    {
        foreach (var item in Items)
        {
            if (item.Type == null || item.Type.Name == "NULL" || !item.Type.IsOrInherits(typeName))
                continue;

            for (var i = 0; i < item.Count; i++)
            {
                yield return (item.Type, item.Offset + (long)i * item.Type.Size);
            }
        }
    }

    public string ReadString(TagType type, long offset, string memberName)
    {
        var member = type.FindMember(memberName);

        if (member == null)
            return null;

        var itemIndex = Reader.GetUInt64(offset + member.Offset);

        if (itemIndex == 0 || itemIndex >= (ulong)Items.Count)
            return null;

        var item = Items[(int)itemIndex];
        var bytes = Reader.GetBytes(item.Offset, Math.Max(item.Count - 1, 0));

        return Encoding.ASCII.GetString(bytes);
    }

    public int? ReadInt32(TagType type, long offset, string memberName)
    {
        var member = type.FindMember(memberName);

        if (member == null)
            return null;

        return Reader.GetInt32(offset + member.Offset);
    }

    /// <summary>
    /// Reads an array of pointers to objects, giving the actual type and position of each object
    /// </summary>
    public List<(TagType Type, long Offset)> ReadObjectArray(TagType type, long offset, string memberName)
    {
        var result = new List<(TagType, long)>();

        var member = type.FindMember(memberName);

        if (member == null)
            return result;

        var arrayIndex = Reader.GetUInt64(offset + member.Offset);

        if (arrayIndex == 0 || arrayIndex >= (ulong)Items.Count)
            return result;

        var array = Items[(int)arrayIndex];

        for (var i = 0; i < array.Count; i++)
        {
            var objectIndex = Reader.GetUInt64(array.Offset + i * 8L);

            if (objectIndex == 0 || objectIndex >= (ulong)Items.Count)
                continue;

            var target = Items[(int)objectIndex];

            result.Add((target.Type, target.Offset));
        }

        return result;
    }
    #endregion
}
