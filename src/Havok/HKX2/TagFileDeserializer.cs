using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace HKX2
{
    /// <summary>
    /// Reads a Havok binary tagfile (TAG0) into the HKX2 classes. This is the format used by Sekiro (SDK 20160200),
    /// whose class layouts differ from the DS3 packfiles the HKX2 classes were generated from. The tagfile describes
    /// its own layout, so fields are matched to the HKX2 classes by name instead of being read in order.
    /// Fields the tagfile doesn't have are left at their defaults, and fields HKX2 doesn't have are skipped.
    /// </summary>
    public class TagFileDeserializer
    {
        private const int OptionalFormat = 1 << 0;
        private const int OptionalSubType = 1 << 1;
        private const int OptionalVersion = 1 << 2;
        private const int OptionalSizeAlign = 1 << 3;
        private const int OptionalFlags = 1 << 4;
        private const int OptionalMembers = 1 << 5;
        private const int OptionalInterfaces = 1 << 6;

        private const int KindBool = 2;
        private const int KindString = 3;
        private const int KindPointer = 6;
        private const int KindRecord = 7;
        private const int KindArray = 8;

        private static readonly Dictionary<string, Type> HavokClasses = typeof(IHavokObject).Assembly.GetTypes()
            .Where(t => typeof(IHavokObject).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
            .GroupBy(t => t.Name)
            .ToDictionary(g => g.Key, g => g.First());

        private static readonly Regex IndexedFieldName = new(@"^(.+)_(\d+)$", RegexOptions.Compiled);

        private byte[] _data;
        private long _dataStart;
        private readonly List<Item> _items = new();
        private readonly Dictionary<int, IHavokObject> _objects = new();

        /// <summary>
        /// True if the data starts with a TAG0 section.
        /// </summary>
        public static bool IsTagFile(ReadOnlySpan<byte> data)
        {
            return data.Length >= 8 && Encoding.ASCII.GetString(data.Slice(4, 4)) == "TAG0";
        }

        /// <summary>
        /// The types of a compendium (TCM0), which tagfiles that store no types of their own refer to.
        /// Map collision binders hold one alongside the collision files.
        /// </summary>
        public sealed class Compendium
        {
            internal List<TagType> Types;
        }

        /// <summary>
        /// Reads the types stored in a compendium (TCM0).
        /// </summary>
        public static Compendium ReadCompendium(byte[] data)
        {
            var reader = new TagFileDeserializer { _data = data };

            Section root = reader.ReadSection(0);
            if (root.Id != "TCM0")
                throw new InvalidOperationException($"Expected a TCM0 compendium, found {root.Id}.");

            foreach (Section section in reader.EnumerateSections(root))
            {
                if (section.Id == "TYPE")
                    return new Compendium { Types = reader.ReadTypes(section) };
            }

            throw new InvalidOperationException("Compendium has no TYPE section.");
        }

        /// <summary>
        /// Reads the tagfile and returns its root object, normally an hkRootLevelContainer.
        /// </summary>
        public IHavokObject Deserialize(byte[] data, Compendium compendium = null)
        {
            _data = data;
            _items.Clear();
            _objects.Clear();

            Section root = ReadSection(0);
            if (root.Id != "TAG0")
                throw new InvalidOperationException($"Expected a TAG0 tagfile, found {root.Id}.");

            List<TagType> types = null;
            Section? index = null;

            foreach (Section section in EnumerateSections(root))
            {
                switch (section.Id)
                {
                    case "DATA":
                        _dataStart = section.Start;
                        break;
                    case "TYPE":
                        types = ReadTypes(section);
                        break;
                    case "TCRF":
                        types = compendium?.Types ??
                                throw new InvalidOperationException("Tagfile refers to a compendium, but none was given.");
                        break;
                    case "INDX":
                        index = section;
                        break;
                }
            }

            if (types == null || index == null)
                throw new InvalidOperationException("Tagfile is missing its type or index section.");

            foreach (Section section in EnumerateSections(index.Value))
            {
                if (section.Id == "ITEM")
                    ReadItems(section, types);
            }

            // Item 0 is the null item, so the first object is the root
            for (var i = 1; i < _items.Count; i++)
            {
                if (_items[i].IsPointer)
                    return ReadObject(i, typeof(IHavokObject));
            }

            return null;
        }

        #region Objects

        /// <summary>
        /// Reads the object an item points to, reusing it if it has already been read so shared references stay shared.
        /// </summary>
        private IHavokObject ReadObject(int itemIndex, Type expected)
        {
            if (itemIndex <= 0 || itemIndex >= _items.Count)
                return null;

            if (_objects.TryGetValue(itemIndex, out IHavokObject existing))
                return expected.IsInstanceOfType(existing) ? existing : null;

            Item item = _items[itemIndex];
            Type classType = ResolveClass(item.Type, expected);
            if (classType == null)
                return null;

            var obj = (IHavokObject)Activator.CreateInstance(classType);
            _objects[itemIndex] = obj;
            FillRecord(obj, item.Type, item.Offset);
            return obj;
        }

        /// <summary>
        /// Finds the HKX2 class for a tagfile type, falling back to its parents for classes HKX2 doesn't know.
        /// </summary>
        private static Type ResolveClass(TagType type, Type expected)
        {
            for (TagType t = type; t != null; t = t.Parent)
            {
                if (HavokClasses.TryGetValue(t.Name, out Type classType) && expected.IsAssignableFrom(classType))
                    return classType;
            }

            if (!expected.IsAbstract && !expected.IsInterface && typeof(IHavokObject).IsAssignableFrom(expected))
                return expected;

            return null;
        }

        private void FillRecord(object obj, TagType type, long offset)
        {
            foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!TryLocateField(type, field.Name, out TagType fieldType, out long fieldOffset))
                    continue;

                object value = ReadValue(field.FieldType, fieldType, offset + fieldOffset);
                if (value != null)
                    field.SetValue(obj, value);
            }
        }

        /// <summary>
        /// Finds where an HKX2 field lives in the record. HKX2 flattens fixed size arrays into numbered
        /// fields (m_codecParms_0, m_codecParms_1, ...), which are elements of one field in the tagfile.
        /// </summary>
        private static bool TryLocateField(TagType type, string name, out TagType fieldType, out long fieldOffset)
        {
            TagField field = type.FindField(name);
            if (field != null)
            {
                fieldType = field.Type;
                fieldOffset = field.Offset;
                return true;
            }

            Match match = IndexedFieldName.Match(name);
            if (match.Success)
            {
                field = type.FindField(match.Groups[1].Value);
                if (field != null)
                {
                    var elementIndex = int.Parse(match.Groups[2].Value);
                    TagType elementType = field.Type.SubType ?? field.Type;
                    fieldType = elementType;
                    fieldOffset = field.Offset + (long)elementIndex * elementType.Size;
                    return true;
                }
            }

            fieldType = null;
            fieldOffset = 0;
            return false;
        }

        private object ReadValue(Type target, TagType type, long pos)
        {
            if (target == typeof(bool))
                return _data[pos] != 0;
            if (target == typeof(byte))
                return _data[pos];
            if (target == typeof(sbyte))
                return (sbyte)_data[pos];
            if (target == typeof(short))
                return BitConverter.ToInt16(_data, (int)pos);
            if (target == typeof(ushort))
                return BitConverter.ToUInt16(_data, (int)pos);
            if (target == typeof(int))
                return (int)ReadInteger(type, pos);
            if (target == typeof(uint))
                return (uint)ReadInteger(type, pos);
            if (target == typeof(long))
                return ReadInteger(type, pos);
            if (target == typeof(ulong))
                return (ulong)ReadInteger(type, pos);
            if (target == typeof(float))
                return type.Size == 2 ? (float)BitConverter.ToHalf(_data, (int)pos) : BitConverter.ToSingle(_data, (int)pos);
            if (target == typeof(Vector4))
                return ReadVector4(pos);
            if (target == typeof(Quaternion))
            {
                Vector4 v = ReadVector4(pos);
                return new Quaternion(v.X, v.Y, v.Z, v.W);
            }
            if (target == typeof(Matrix4x4))
                return ReadMatrix(type, pos);
            if (target == typeof(string))
                return ReadString(type, pos);
            if (target.IsEnum)
                return Enum.ToObject(target, ReadInteger(type, pos));

            if (target.IsGenericType && target.GetGenericTypeDefinition() == typeof(List<>))
                return ReadList(target, type, pos);

            if (typeof(IHavokObject).IsAssignableFrom(target))
            {
                if (type.Kind == KindPointer)
                    return ReadObject((int)BitConverter.ToUInt64(_data, (int)pos), target);

                if (type.Kind == KindRecord)
                {
                    Type classType = ResolveClass(type, target);
                    if (classType == null)
                        return null;

                    var obj = Activator.CreateInstance(classType);
                    FillRecord(obj, type, pos);
                    return obj;
                }
            }

            return null;
        }

        /// <summary>
        /// Reads an integer at the size the tagfile stores it, since enums are often stored narrower than HKX2 declares them.
        /// </summary>
        private long ReadInteger(TagType type, long pos)
        {
            switch (type.Size)
            {
                case 1: return _data[pos];
                case 2: return BitConverter.ToInt16(_data, (int)pos);
                case 8: return BitConverter.ToInt64(_data, (int)pos);
                default: return BitConverter.ToInt32(_data, (int)pos);
            }
        }

        private Vector4 ReadVector4(long pos)
        {
            var p = (int)pos;
            return new Vector4(
                BitConverter.ToSingle(_data, p),
                BitConverter.ToSingle(_data, p + 4),
                BitConverter.ToSingle(_data, p + 8),
                BitConverter.ToSingle(_data, p + 12));
        }

        private Matrix4x4 ReadMatrix(TagType type, long pos)
        {
            // hkMatrix3 and hkRotation are three columns, hkMatrix4 and hkTransform are four
            var p = (int)pos;
            float F(int i) => BitConverter.ToSingle(_data, p + i * 4);

            if (type.Size >= 64)
            {
                return new Matrix4x4(
                    F(0), F(1), F(2), F(3),
                    F(4), F(5), F(6), F(7),
                    F(8), F(9), F(10), F(11),
                    F(12), F(13), F(14), F(15));
            }

            return new Matrix4x4(
                F(0), F(1), F(2), F(3),
                F(4), F(5), F(6), F(7),
                F(8), F(9), F(10), F(11),
                0, 0, 0, 1);
        }

        private string ReadString(TagType type, long pos)
        {
            if (type.Kind != KindString && type.Kind != KindPointer)
                return null;

            var index = (int)BitConverter.ToUInt64(_data, (int)pos);
            if (index <= 0 || index >= _items.Count)
                return null;

            Item item = _items[index];
            var length = Array.IndexOf(_data, (byte)0, (int)item.Offset, item.Count) - (int)item.Offset;
            return Encoding.ASCII.GetString(_data, (int)item.Offset, length < 0 ? item.Count : length);
        }

        private object ReadList(Type target, TagType type, long pos)
        {
            Type elementTarget = target.GetGenericArguments()[0];
            var list = (IList)Activator.CreateInstance(target);

            // hkArray is an item index followed by the size and flags, hkRelArray just the item index
            var index = type.Name == "hkRelArray"
                ? (int)BitConverter.ToUInt32(_data, (int)pos)
                : (int)BitConverter.ToUInt64(_data, (int)pos);

            if (index <= 0 || index >= _items.Count)
                return list;

            Item item = _items[index];
            if (item.Type == null)
                return list;

            var stride = item.Type.Size;
            for (var i = 0; i < item.Count; i++)
            {
                list.Add(ReadValue(elementTarget, item.Type, item.Offset + (long)i * stride));
            }

            return list;
        }

        #endregion

        #region Items

        private void ReadItems(Section section, List<TagType> types)
        {
            for (long pos = section.Start; pos + 12 <= section.End; pos += 12)
            {
                var typeAndFlags = BitConverter.ToUInt32(_data, (int)pos);
                var typeIndex = (int)(typeAndFlags & 0xFFFFFF);
                var kind = (int)(typeAndFlags >> 28);

                _items.Add(new Item
                {
                    Type = typeIndex > 0 && typeIndex < types.Count ? types[typeIndex] : null,
                    IsPointer = kind == 1,
                    Offset = _dataStart + BitConverter.ToInt32(_data, (int)pos + 4),
                    Count = BitConverter.ToInt32(_data, (int)pos + 8)
                });
            }
        }

        private struct Item
        {
            public TagType Type;
            public bool IsPointer;
            public long Offset;
            public int Count;
        }

        #endregion

        #region Types

        private List<TagType> ReadTypes(Section typeSection)
        {
            List<string> typeStrings = new();
            List<string> fieldStrings = new();
            List<TagType> types = new();
            Section? body = null;

            foreach (Section section in EnumerateSections(typeSection))
            {
                switch (section.Id)
                {
                    case "TSTR":
                    case "TST1":
                        typeStrings = ReadStrings(section);
                        break;
                    case "FSTR":
                    case "FST1":
                        fieldStrings = ReadStrings(section);
                        break;
                    case "TNAM":
                    case "TNA1":
                        types = ReadTypeNames(section, typeStrings);
                        break;
                    case "TBOD":
                    case "TBDY":
                        body = section;
                        break;
                }
            }

            if (body != null)
                ReadTypeBodies(body.Value, types, fieldStrings);

            return types;
        }

        private List<string> ReadStrings(Section section)
        {
            List<string> strings = new();
            var start = (int)section.Start;

            for (var i = start; i < section.End; i++)
            {
                if (_data[i] == 0)
                {
                    strings.Add(Encoding.ASCII.GetString(_data, start, i - start));
                    start = i + 1;
                }
            }

            return strings;
        }

        private List<TagType> ReadTypeNames(Section section, List<string> typeStrings)
        {
            long pos = section.Start;
            var count = (int)ReadVarUInt(ref pos);

            // Index 0 is the null type, which isn't stored
            List<TagType> types = new(count);
            for (var i = 0; i < count; i++)
                types.Add(new TagType());

            for (var i = 1; i < count; i++)
            {
                types[i].Name = typeStrings[(int)ReadVarUInt(ref pos)];

                var templateCount = (int)ReadVarUInt(ref pos);
                for (var j = 0; j < templateCount; j++)
                {
                    ReadVarUInt(ref pos);
                    ReadVarUInt(ref pos);
                }
            }

            return types;
        }

        private void ReadTypeBodies(Section section, List<TagType> types, List<string> fieldStrings)
        {
            long pos = section.Start;

            while (pos < section.End)
            {
                var typeIndex = (int)ReadVarUInt(ref pos);
                if (typeIndex == 0)
                    continue; // Padding at the end of the section

                TagType type = types[typeIndex];

                var parentIndex = (int)ReadVarUInt(ref pos);
                if (parentIndex > 0)
                    type.Parent = types[parentIndex];

                var optionals = (int)ReadVarUInt(ref pos);

                if ((optionals & OptionalFormat) != 0)
                    type.OwnFormat = (int)ReadVarUInt(ref pos);

                if ((optionals & OptionalSubType) != 0)
                {
                    var subTypeIndex = (int)ReadVarUInt(ref pos);
                    type.OwnSubType = subTypeIndex > 0 ? types[subTypeIndex] : null;
                }

                if ((optionals & OptionalVersion) != 0)
                    ReadVarUInt(ref pos);

                if ((optionals & OptionalSizeAlign) != 0)
                {
                    type.OwnSize = (int)ReadVarUInt(ref pos);
                    ReadVarUInt(ref pos);
                }

                if ((optionals & OptionalFlags) != 0)
                    ReadVarUInt(ref pos);

                if ((optionals & OptionalMembers) != 0)
                {
                    // The two high bytes are the number of properties, which have no storage
                    var fieldCount = (int)(ReadVarUInt(ref pos) & 0xFFFF);
                    for (var i = 0; i < fieldCount; i++)
                    {
                        TagField field = new() { Name = fieldStrings[(int)ReadVarUInt(ref pos)] };
                        ReadVarUInt(ref pos);
                        field.Offset = (int)ReadVarUInt(ref pos);
                        field.Type = types[(int)ReadVarUInt(ref pos)];
                        type.Fields.Add(field);
                    }
                }

                if ((optionals & OptionalInterfaces) != 0)
                {
                    var interfaceCount = (int)ReadVarUInt(ref pos);
                    for (var i = 0; i < interfaceCount; i++)
                    {
                        ReadVarUInt(ref pos);
                        ReadVarUInt(ref pos);
                    }
                }
            }
        }

        // The leading bits of the first byte give the length, the rest of the bits are big endian
        private ulong ReadVarUInt(ref long pos)
        {
            byte first = _data[pos++];

            if ((first & 0x80) == 0)
                return first;

            if ((first & 0xC0) == 0x80)
                return (ulong)(first & 0x3F) << 8 | _data[pos++];

            if ((first & 0xE0) == 0xC0)
            {
                ulong value = (ulong)(first & 0x1F) << 16 | (ulong)_data[pos++] << 8;
                return value | _data[pos++];
            }

            if ((first & 0xF8) == 0xE0)
            {
                ulong value = (ulong)(first & 0x07) << 24 | (ulong)_data[pos++] << 16 | (ulong)_data[pos++] << 8;
                return value | _data[pos++];
            }

            if ((first & 0xF8) == 0xE8)
            {
                ulong value = (ulong)(first & 0x07) << 32 | (ulong)_data[pos++] << 24 | (ulong)_data[pos++] << 16;
                value |= (ulong)_data[pos++] << 8;
                return value | _data[pos++];
            }

            throw new InvalidOperationException($"Unsupported variable length integer prefix 0x{first:X2}.");
        }

        internal sealed class TagType
        {
            public string Name = "";
            public TagType Parent;
            public int? OwnFormat;
            public TagType OwnSubType;
            public int? OwnSize;
            public readonly List<TagField> Fields = new();

            public int Size => OwnSize ?? Parent?.Size ?? 0;
            public int Format => OwnFormat ?? Parent?.Format ?? 0;
            public int Kind => Format & 0xF;
            public TagType SubType => OwnSubType ?? Parent?.SubType;

            public TagField FindField(string name)
            {
                var bare = name.StartsWith("m_") ? name.Substring(2) : name;

                for (TagType type = this; type != null; type = type.Parent)
                {
                    foreach (TagField field in type.Fields)
                    {
                        if (field.Name == name || field.Name == bare || field.Name == "m_" + bare)
                            return field;
                    }
                }

                return null;
            }

            public override string ToString() => Name;
        }

        internal sealed class TagField
        {
            public string Name = "";
            public int Offset;
            public TagType Type;
        }

        #endregion

        #region Sections

        private Section ReadSection(long position)
        {
            // Big endian, with the top bits used as flags
            var pos = (int)position;
            var length = (_data[pos] & 0x0F) << 24 | _data[pos + 1] << 16 | _data[pos + 2] << 8 | _data[pos + 3];

            return new Section
            {
                Id = Encoding.ASCII.GetString(_data, pos + 4, 4),
                Start = position + 8,
                End = position + length
            };
        }

        private IEnumerable<Section> EnumerateSections(Section parent)
        {
            long pos = parent.Start;

            while (pos + 8 <= parent.End)
            {
                Section section = ReadSection(pos);
                if (section.End <= pos)
                    yield break;

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
    }
}
