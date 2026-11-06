using System.Runtime.CompilerServices;
using System.Buffers.Binary;
using System.Numerics;
using System.Text;
using Havok.Reflection;
using SoulsFormats;

namespace Havok.Formats.Packfile;

/// <summary>
/// Reads and writes Havok packfiles (magic 0x57E0E057) for any class set whose registry describes the packfile layout
/// (hk2014 now; the 32-bit SDKs in Phase 5). Objects are found through the data section's fixups: virtual fixups give
/// each object's class, global fixups its pointers, local fixups its arrays and strings.
///
/// Writing follows HKX2's PackFileSerializer exactly (object order, the nested queues for arrays and strings, padding,
/// fixup order, the fixed hk_2014 header), so a DS3 file it reproduced byte for byte still is. Unlike HKX2, hkRelArray
/// and hkSimpleArray members are read and written.
/// </summary>
public sealed class HavokPackfileSerializer
{
    private const uint Magic0 = 0x57E0E057;
    private const uint Magic1 = 0x10C0C010;

    private readonly Dictionary<string, HavokType> _typesByClassName = new();

    public HavokPackfileSerializer(IHavokClassSet classSet)
    {
        ClassSet = classSet;

        // packfile class names are flat: hkRootLevelContainerNamedVariant is the registry's hkRootLevelContainer::NamedVariant
        foreach (HavokType type in classSet.Registry.Types.Where(t => t.Kind == HavokType.TypeKind.Record))
        {
            _typesByClassName.TryAdd(ClassName(type), type);
        }
    }

    public IHavokClassSet ClassSet { get; }

    /// <summary>
    /// The objects of the last file read, by their offset in its data section (for comparisons with other readers).
    /// </summary>
    public IReadOnlyDictionary<uint, object> LastObjects { get; private set; } = new Dictionary<uint, object>();

    private HavokTypeRegistry Registry => ClassSet.Registry;

    /// <summary>
    /// The name a class has in a packfile's class-name section.
    /// </summary>
    public static string ClassName(HavokType type) => type.Identity.Replace("::", "");

    #region Reading

    /// <summary>
    /// The container parts of a file that don't come from its objects: the header (with its extended part) and the
    /// __types__ section (hkClass reflection some games ship, e.g. DS2S), kept per root object so a rewrite
    /// reproduces them.
    /// </summary>
    private sealed record SourceContainer(byte[] Header, byte[] SectionHeaderExtra, byte[] TypesData, byte[] TypesLocal,
        byte[] TypesGlobal, byte[] TypesVirtual, int PointerSize, bool BigEndian, byte[] ClassNames,
        IReadOnlyDictionary<string, uint> TypeClasses, int[] DataPartLengths, string[] SectionOrder, bool GlobalsInMemberOrder);

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, SourceContainer> SourcesRead = new();

    // each hkArray's capacity and flags word as read (Havok 5.5 sets 0x40000000 on some), kept for its list
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, StrongBox<uint>> ArrayFlagsRead = new();

    // the bytes of each object's unserialized members as read, where they weren't all zero
    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<object, Dictionary<string, byte[]>> UnserializedRead = new();

    /// <summary>
    /// Write back the header and __types__ section a root object was read with. When false, or for objects that weren't
    /// read, write HKX2's: the fixed DS3 hk_2014 header and an empty __types__ section.
    /// </summary>
    public bool PreserveSource { get; init; } = true;

    // Unserialized hkArray members are written as empty arrays (DONT_DEALLOCATE), as Havok's own packfiles hold them
    // (DS1, DeS and DS3 alike: DS3 byte identity 668 -> 693). HKX2 wrote zeros, which its mode keeps.
    private bool NonSerializedArrayFlags => PreserveSource;

    private sealed class Section
    {
        public string Tag = "";
        public byte[] Data = Array.Empty<byte>();
        public byte[] LocalRaw = Array.Empty<byte>(), GlobalRaw = Array.Empty<byte>(), VirtualRaw = Array.Empty<byte>();
        public readonly Dictionary<uint, uint> Local = new();
        public readonly Dictionary<uint, (uint Section, uint Dst)> Global = new();
        public readonly Dictionary<uint, (uint Section, uint NameOffset)> Virtual = new();
    }

    private sealed class ReadState
    {
        public required Section[] Sections;
        public required int DataSection;
        public required Dictionary<uint, string> ClassNames;
        public required bool BigEndian;
        public required int PointerSize;
        public readonly Dictionary<uint, object> Objects = new();
    }

    /// <summary>
    /// Reads a packfile and returns its root object.
    /// </summary>
    public object Read(Memory<byte> memory)
    {
        byte[] data = memory.ToArray();
        bool bigEndian = data[0x11] == 0;
        var br = new BinaryReaderEx(bigEndian, data);

        if (br.ReadUInt32() != Magic0 || br.ReadUInt32() != Magic1) throw new InvalidDataException("Not a packfile.");
        br.ReadInt32(); // user tag
        int version = br.ReadInt32();
        int pointerSize = br.ReadByte();
        br.ReadByte(); // endian
        br.ReadByte(); // padding option
        br.ReadByte(); // base class
        int sectionCount = br.ReadInt32();
        int contentsSection = br.ReadInt32();
        int contentsOffset = br.ReadInt32();
        br.ReadInt32(); // contents class name section
        br.ReadInt32(); // contents class name offset
        br.Position = 0x40;
        int sectionHeaderExtra = 0;
        if (version >= 0x0B)
        {
            short sectionOffset = br.GetInt16(0x3E);
            br.Position = 0x40 + sectionOffset;
            sectionHeaderExtra = 16;
        }

        var sections = new Section[sectionCount];
        for (int i = 0; i < sectionCount; i++)
        {
            var section = new Section { Tag = br.ReadFixStr(19) };
            br.ReadByte();
            uint start = br.ReadUInt32();
            uint local = br.ReadUInt32();
            uint global = br.ReadUInt32();
            uint virt = br.ReadUInt32();
            uint exports = br.ReadUInt32();
            br.ReadUInt32(); // imports
            br.ReadUInt32(); // end
            br.Position += sectionHeaderExtra;

            section.Data = data.AsSpan((int)start, (int)local).ToArray();
            section.LocalRaw = data.AsSpan((int)(start + local), (int)(global - local)).ToArray();
            section.GlobalRaw = data.AsSpan((int)(start + global), (int)(virt - global)).ToArray();
            section.VirtualRaw = data.AsSpan((int)(start + virt), (int)(exports - virt)).ToArray();
            for (uint p = start + local; p + 8 <= start + global; p += 8)
            {
                uint src = br.GetUInt32(p);
                if (src != 0xFFFFFFFF) section.Local[src] = br.GetUInt32(p + 4);
            }

            for (uint p = start + global; p + 12 <= start + virt; p += 12)
            {
                uint src = br.GetUInt32(p);
                if (src != 0xFFFFFFFF) section.Global[src] = (br.GetUInt32(p + 4), br.GetUInt32(p + 8));
            }

            for (uint p = start + virt; p + 12 <= start + exports; p += 12)
            {
                uint src = br.GetUInt32(p);
                if (src != 0xFFFFFFFF) section.Virtual[src] = (br.GetUInt32(p + 4), br.GetUInt32(p + 8));
            }

            sections[i] = section;
        }

        // class names: signature, 0x09, name; virtual fixups refer to the name's offset
        var classNames = new Dictionary<uint, string>();
        Section names = sections.First(s => s.Tag == "__classnames__");
        for (int p = 0; p + 5 < names.Data.Length && BinaryPrimitives.ReadUInt32LittleEndian(names.Data.AsSpan(p)) != 0xFFFFFFFF;)
        {
            p += 5;
            int s = p;
            while (p < names.Data.Length && names.Data[p] != 0) p++;
            classNames[(uint)s] = Encoding.ASCII.GetString(names.Data, s, p - s);
            p++;
        }
        int classNamesEnd = 0;
        for (int p = 0; p + 5 < names.Data.Length && BinaryPrimitives.ReadUInt32LittleEndian(names.Data.AsSpan(p)) != 0xFFFFFFFF;)
        {
            p += 5;
            while (p < names.Data.Length && names.Data[p] != 0) p++;
            classNamesEnd = ++p;
        }

        var state = new ReadState
        {
            Sections = sections, DataSection = contentsSection, ClassNames = classNames, BigEndian = bigEndian,
            PointerSize = pointerSize
        };
        object root = ReadObject(state, (uint)contentsSection, (uint)contentsOffset)
                      ?? throw new InvalidDataException("The root object could not be read.");
        LastObjects = state.Objects;

        int sectionsStart = version >= 0x0B ? 0x40 + br.GetInt16(0x3E) : 0x40;
        Section? types = sections.FirstOrDefault(s => s.Tag == "__types__");
        // the hkClass objects of __types__ by name: hkVariant class pointers point at them
        var typeClasses = new Dictionary<string, uint>();
        if (types != null)
            foreach (var (src, (_, nameOffset)) in types.Virtual)
                if (classNames.GetValueOrDefault(nameOffset) == "hkClass" && types.Local.TryGetValue(src, out uint nameAt))
                    typeClasses.TryAdd(CString(types.Data, nameAt), src);
        byte[] extra = sectionHeaderExtra == 0
            ? Array.Empty<byte>()
            : data.AsSpan(sectionsStart + 0x30, sectionHeaderExtra).ToArray();
        SourcesRead.AddOrUpdate(root, new SourceContainer(data[..sectionsStart], extra,
            types?.Data ?? Array.Empty<byte>(), types?.LocalRaw ?? Array.Empty<byte>(),
            types?.GlobalRaw ?? Array.Empty<byte>(), types?.VirtualRaw ?? Array.Empty<byte>(), pointerSize, bigEndian,
            names.Data[..classNamesEnd], typeClasses,
            [sections[contentsSection].LocalRaw.Length, sections[contentsSection].GlobalRaw.Length, sections[contentsSection].VirtualRaw.Length],
            sections.Select(s => s.Tag).ToArray(), !SortedBySource(sections[contentsSection].GlobalRaw, br)));
        return root;
    }

    /// <summary>
    /// Whether a global fixup table's rows are in source-offset order. Havok 5.5 writes them in member order instead, where
    /// an array's element pointers come at the array member, before any pointer member declared after it
    /// (hkaRagdollInstance: rigidBodies' elements, then skeleton at +44).
    /// </summary>
    private static bool SortedBySource(byte[] table, BinaryReaderEx br)
    {
        var rows = new BinaryReaderEx(br.BigEndian, table);
        uint previous = 0;
        for (int p = 0; p + 12 <= table.Length; p += 12)
        {
            uint src = rows.GetUInt32(p);
            if (src == 0xFFFFFFFF) continue;
            if (src < previous) return false;
            previous = src;
        }
        return true;
    }

    /// <summary>
    /// The object at an offset of a section, read once. Its class comes from the virtual fixup there.
    /// </summary>
    private object? ReadObject(ReadState state, uint sectionIndex, uint offset)
    {
        if (sectionIndex != state.DataSection) return null; // e.g. hkVariant class pointers into __types__
        if (state.Objects.TryGetValue(offset, out object? existing)) return existing;

        Section section = state.Sections[sectionIndex];
        if (!section.Virtual.TryGetValue(offset, out var virt))
            throw new InvalidDataException($"No class for the object at 0x{offset:X}.");
        string className = state.ClassNames[virt.NameOffset];
        HavokType type = _typesByClassName.TryGetValue(className, out HavokType? t)
            ? t
            : throw new KeyNotFoundException($"The class {className} is not in the {ClassSet.Name} registry.");

        IHavokFieldAccess access = ClassSet.Instantiate(type);
        state.Objects[offset] = access.Object; // before the fields, for objects that point back
        ReadFields(state, section, type, offset, access);
        return access.Object;
    }

    private void ReadFields(ReadState state, Section section, HavokType type, uint offset, IHavokFieldAccess access)
    {
        foreach (HavokType.Member field in type.Fields)
        {
            if (field.NonSerializable)
            {
                // kept as bytes, when not all zero, so a rewrite gives them back (DeS keeps a subpart's material stride here)
                var raw = section.Data.AsSpan((int)(offset + field.Offset), field.Type.Size);
                if (raw.ContainsAnyExcept((byte)0))
                    UnserializedRead.GetOrCreateValue(access.Object)[field.Name] = raw.ToArray();
                continue;
            }
            object? value = ReadValue(state, section, field.Type, offset + (uint)field.Offset);
            if (!access.TrySetField(field.Name, value))
                throw new InvalidOperationException($"Unable to set field \"{field.Name}\" of \"{type.Identity}\" to a {value?.GetType().Name ?? "null"} ({field.Type.Identity}, {field.Type.Kind}).");

            // the class set may store a copy of the list read: the array's flags word follows it
            if (value != null && ArrayFlagsRead.TryGetValue(value, out var flags) && access.TryGetField(field.Name, out object? stored) &&
                stored != null && !ReferenceEquals(stored, value))
                ArrayFlagsRead.AddOrUpdate(stored, flags);
        }
    }

    private object? ReadValue(ReadState state, Section section, HavokType type, uint p)
    {
        byte[] d = section.Data;
        bool be = state.BigEndian;
        switch (type.Kind)
        {
            case HavokType.TypeKind.Bool:
                return d[p] != 0;
            case HavokType.TypeKind.Int:
                // hkUlong is pointer sized (32-bit on the old SDKs) and a ulong in every class set
                return type.Name == "hkUlong" ? Convert.ToUInt64(ReadInt(d, p, type.Format, be)) : ReadInt(d, p, type.Format, be);
            case HavokType.TypeKind.Float:
                // boxed per arm: a switch over float and double arms would widen every value to double
                return type.Size switch
                {
                    2 => (object)BitConverter.Int32BitsToSingle(U16(d, p, be) << 16),
                    8 => (object)BitConverter.Int64BitsToDouble((long)U64(d, p, be)),
                    _ => (object)BitConverter.Int32BitsToSingle((int)U32(d, p, be))
                };
            case HavokType.TypeKind.String:
                return section.Local.TryGetValue(p, out uint str) ? CString(d, str) : null;
            case HavokType.TypeKind.Pointer:
                return section.Global.TryGetValue(p, out var target) ? ReadObject(state, target.Section, target.Dst) : null;
            case HavokType.TypeKind.Record:
            {
                IHavokFieldAccess access = ClassSet.Instantiate(type);
                ReadFields(state, section, type, p, access);
                return access.Object;
            }
            case HavokType.TypeKind.Array:
                return ReadArray(state, section, type, p);
            default:
                throw new NotImplementedException($"Reading {type.Kind} ({type.Identity}) is not supported.");
        }
    }

    private object ReadArray(ReadState state, Section section, HavokType type, uint p)
    {
        byte[] d = section.Data;
        bool be = state.BigEndian;

        if (type.Type == typeof(Vector4)) return new Vector4(F(d, p, be), F(d, p + 4, be), F(d, p + 8, be), F(d, p + 12, be));
        if (type.Type == typeof(Quaternion)) return new Quaternion(F(d, p, be), F(d, p + 4, be), F(d, p + 8, be), F(d, p + 12, be));
        if (type.Type == typeof(Matrix4x4)) return Matrix(d, p, be, 4);
        if (type.Type is { } m3 && m3 == ClassSet.Matrix3x3Type) return ClassSet.ToMatrix3x3(Matrix(d, p, be, 3));

        HavokType element = type.SubType ?? throw new InvalidDataException($"{type.Identity} has no element type.");
        int stride = ElementSize(element, state.PointerSize);
        var list = (System.Collections.IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(ElementClrType(element)))!;

        switch (type.Name)
        {
            case "hkArray":
            {
                int count = (int)U32(d, p + (uint)state.PointerSize, be);
                ArrayFlagsRead.AddOrUpdate(list, new StrongBox<uint>(U32(d, p + (uint)state.PointerSize + 4, be)));
                if (count > 0 && section.Local.TryGetValue(p, out uint dst))
                    for (int i = 0; i < count; i++) list.Add(AsElement(ReadValue(state, section, element, dst + (uint)(i * stride)), element));
                return list;
            }
            case "hkSimpleArray":
            {
                int count = (int)U32(d, p + (uint)state.PointerSize, be);
                if (count > 0 && section.Local.TryGetValue(p, out uint dst))
                    for (int i = 0; i < count; i++) list.Add(AsElement(ReadValue(state, section, element, dst + (uint)(i * stride)), element));
                return list;
            }
            case "hkRelArray":
            {
                // a 16-bit count and a 16-bit offset from the field to the data, which follows the object
                int count = U16(d, p, be);
                int offset = U16(d, p + 2, be);
                for (int i = 0; i < count; i++) list.Add(AsElement(ReadValue(state, section, element, p + (uint)offset + (uint)(i * stride)), element));
                return list;
            }
            case "T[N]":
            {
                int count = type.Format >> 8;
                Array array = Array.CreateInstance(ElementClrType(element), count);
                for (int i = 0; i < count; i++) array.SetValue(AsElement(ReadValue(state, section, element, p + (uint)(i * stride)), element), i);
                return array;
            }
            default:
                throw new NotImplementedException($"Reading arrays of type {type.Name} is not supported.");
        }
    }

    /// <summary>
    /// A value read for a list or array element, as the element type: ints become the enum of an hkEnum element.
    /// </summary>
    private static object? AsElement(object? value, HavokType element) =>
        value is not null && element.Type is { IsEnum: true } enumType && !enumType.IsInstanceOfType(value)
            ? Enum.ToObject(enumType, value)
            : value;

    private static int ElementSize(HavokType element, int pointerSize) =>
        element.Kind is HavokType.TypeKind.Pointer or HavokType.TypeKind.String ? pointerSize : element.Size;

    /// <summary>
    /// The C# element type of a list or array, as the class set's fields declare it.
    /// </summary>
    private static Type ElementClrType(HavokType element) => element.Kind switch
    {
        HavokType.TypeKind.String => typeof(string),
        HavokType.TypeKind.Pointer => element.SubType?.Type ?? typeof(object),
        _ => element.Type ?? throw new InvalidDataException($"{element.Identity} has no C# type.")
    };

    private static object ReadInt(byte[] d, uint p, int format, bool be)
    {
        bool signed = (format & 0x200) != 0;
        return ((format >> 10) / 8, signed) switch
        {
            (1, true) => (sbyte)d[p],
            (1, false) => d[p],
            (2, true) => (short)U16(d, p, be),
            (2, false) => (ushort)U16(d, p, be),
            (4, true) => (int)U32(d, p, be),
            (4, false) => U32(d, p, be),
            (8, true) => (long)U64(d, p, be),
            _ => U64(d, p, be)
        };
    }

    private static int U16(byte[] d, uint p, bool be) =>
        be ? BinaryPrimitives.ReadUInt16BigEndian(d.AsSpan((int)p)) : BinaryPrimitives.ReadUInt16LittleEndian(d.AsSpan((int)p));

    private static uint U32(byte[] d, uint p, bool be) =>
        be ? BinaryPrimitives.ReadUInt32BigEndian(d.AsSpan((int)p)) : BinaryPrimitives.ReadUInt32LittleEndian(d.AsSpan((int)p));

    private static ulong U64(byte[] d, uint p, bool be) =>
        be ? BinaryPrimitives.ReadUInt64BigEndian(d.AsSpan((int)p)) : BinaryPrimitives.ReadUInt64LittleEndian(d.AsSpan((int)p));

    private static float F(byte[] d, uint p, bool be) => BitConverter.Int32BitsToSingle((int)U32(d, p, be));

    private static Matrix4x4 Matrix(byte[] d, uint p, bool be, int rows)
    {
        Matrix4x4 m = new();
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < 4; j++)
                m[i, j] = F(d, p + (uint)(16 * i + 4 * j), be);
        return m;
    }

    private static string CString(byte[] d, uint p)
    {
        int end = (int)p;
        while (end < d.Length && d[end] != 0) end++;
        return PackfileText.Decode(d.AsSpan((int)p, end - (int)p));
    }

    #endregion

    #region Writing

    private sealed class Fixup
    {
        public uint Src;
        public uint Dst;
    }

    private readonly ReferenceEqualityComparer _byReference = ReferenceEqualityComparer.Instance;

    // HKX2's write state: queues of objects to write and of local data (array contents, strings), nested by depth
    private List<Queue<object>> _serializationQueues = new();
    private int _currSerializationQueue;
    private HashSet<object> _serialized = null!;
    private HashSet<object> _pendingVirtuals = null!;
    private Dictionary<string, uint> _classNameOffsets = new();
    private List<(uint Src, uint NameOffset)> _virtualFixups = new();
    private Dictionary<object, List<uint>> _pendingGlobals = null!;
    private Dictionary<object, uint> _globalLookup = null!;
    private List<(uint Src, uint Section, uint Dst)> _globalFixups = new();

    // the member order pointers were written in: a node per object, with each array's contents as a child node at the
    // array member (they're written later, from the local queues); items are pointer positions or child nodes
    private sealed class OrderNode { public readonly List<object> Items = new(); }
    private OrderNode _orderRoot = new();
    private OrderNode _orderCurrent = new();

    private SourceContainer? _source;
    // the sections in the order written: the source's (Havok 5.1 puts __data__ before __types__), else this
    private static readonly string[] DefaultSectionOrder = ["__classnames__", "__types__", "__data__"];
    private string[] _sectionOrder = DefaultSectionOrder;
    private uint TypesSection => (uint)Array.IndexOf(_sectionOrder, "__types__");
    private uint DataSection => (uint)Array.IndexOf(_sectionOrder, "__data__");
    private uint ClassNamesSection => (uint)Array.IndexOf(_sectionOrder, "__classnames__");
    private List<Queue<Action>> _localWriteQueues = new();
    private int _currWriteQueue;
    private List<Fixup> _localFixups = new();
    private BinaryWriterEx _data = null!;
    private BinaryWriterEx _classes = null!;

    // pointer size and byte order of the file being written: the source's, else 64-bit little-endian
    private int _ps = 8;
    private bool _be;

    /// <summary>
    /// Writes a packfile. A root that was read is written with its source's header, pointer size, byte order, class-name
    /// section and __types__ section (<see cref="PreserveSource" />); anything else as a 64-bit little-endian hk_2014
    /// packfile, as DS3/BB/DS2S/SDT use.
    /// </summary>
    public byte[] Write(object root)
    {
        SourceContainer? source = PreserveSource && SourcesRead.TryGetValue(root, out SourceContainer? read) ? read : null;
        _ps = source?.PointerSize ?? 8;
        _be = source?.BigEndian ?? false;

        _serializationQueues = new List<Queue<object>> { new() };
        _serializationQueues[0].Enqueue(root);
        _currSerializationQueue = 0;
        _serialized = new HashSet<object>(_byReference);
        _pendingVirtuals = new HashSet<object>(_byReference) { root };
        _classNameOffsets = new Dictionary<string, uint>();
        _virtualFixups = new List<(uint, uint)>();
        _pendingGlobals = new Dictionary<object, List<uint>>(_byReference);
        _globalLookup = new Dictionary<object, uint>(_byReference);
        _globalFixups = new List<(uint, uint, uint)>();
        _orderRoot = new OrderNode();
        _orderCurrent = _orderRoot;
        _source = source;
        _sectionOrder = source?.SectionOrder is { Length: 3 } order && DefaultSectionOrder.All(order.Contains) ? order : DefaultSectionOrder;
        _localWriteQueues = new List<Queue<Action>> { new() };
        _currWriteQueue = 0;
        _localFixups = new List<Fixup>();
        _data = new BinaryWriterEx(_be);
        _classes = new BinaryWriterEx(_be);

        if (source is { ClassNames.Length: > 0 })
        {
            // the source's class-name section as it was (old SDKs list every class of the file, used or not), so names
            // keep their offsets; classes it lacks are appended
            _classes.WriteBytes(source.ClassNames);
            for (int p = 0; p + 5 < source.ClassNames.Length;)
            {
                p += 5;
                int s = p;
                while (source.ClassNames[p] != 0) p++;
                _classNameOffsets[Encoding.ASCII.GetString(source.ClassNames, s, p - s)] = (uint)s;
                p++;
            }
        }
        else
        {
            // class names Havok always has
            WriteClassName("hkClass", 0x33D42383);
            WriteClassName("hkClassMember", 0xB0EFA719);
            WriteClassName("hkClassEnum", 0x8A3609CF);
            WriteClassName("hkClassEnumItem", 0xCE6F8A6C);
        }

        while (_serializationQueues.Count > 1 || _serializationQueues[0].Count > 0)
        {
            Queue<object> sq = _serializationQueues[^1];
            while (sq.Count == 0 && _serializationQueues.Count > 1)
            {
                _serializationQueues.RemoveAt(_serializationQueues.Count - 1);
                sq = _serializationQueues[^1];
            }

            if (sq.Count == 0) continue;

            object obj = sq.Dequeue();
            _currSerializationQueue = _serializationQueues.Count - 1;
            if (_serialized.Contains(obj)) continue;

            HavokType type = Registry.GetType(obj.GetType())
                             ?? throw new ArgumentException($"{obj.GetType()} is not in the {ClassSet.Name} registry.");

            if (_pendingVirtuals.Remove(obj))
            {
                string className = ClassName(type);
                if (!_classNameOffsets.ContainsKey(className))
                {
                    WriteClassName(className, type.Hash ?? throw new InvalidDataException($"{type.Identity} has no signature."));
                }

                uint position = (uint)_data.Position;
                _virtualFixups.Add((position, _classNameOffsets[className]));
                if (_pendingGlobals.Remove(obj, out List<uint>? sources))
                {
                    foreach (uint src in sources) _globalFixups.Add((src, DataSection, position));
                }

                _globalLookup.Add(obj, position);
            }

            long objectStart = _data.Position;
            var objectOrder = new OrderNode();
            _orderRoot.Items.Add(objectOrder);
            _orderCurrent = objectOrder;
            WriteRecord(type, obj);
            WriteRelArrayData(type, obj, objectStart);
            _serialized.Add(obj);
            _data.Pad(16);

            // local data: array contents and strings, deepest queue first, padding as each queue empties
            while (_localWriteQueues.Count > 1 || _localWriteQueues[0].Count > 0)
            {
                Queue<Action> q = _localWriteQueues[^1];
                while (q.Count == 0 && _localWriteQueues.Count > 1)
                {
                    _localWriteQueues.RemoveAt(_localWriteQueues.Count - 1);
                    q = _localWriteQueues[^1];
                    _data.Pad(16);
                }

                if (q.Count == 0) continue;

                Action action = q.Dequeue();
                _currWriteQueue = _localWriteQueues.Count - 1;
                action();
            }

            _data.Pad(16);
        }

        while (_classes.Position % 16 != 0) _classes.WriteByte(0xFF);
        byte[] classData = _classes.FinishBytes();
        byte[] objectData = _data.FinishBytes();

        HavokType rootType = Registry.GetType(root.GetType())!;

        var bw = new BinaryWriterEx(_be);
        if (source != null)
        {
            // the header as it was read, with the root's class name offset in the new class-name section
            bw.WriteBytes(source.Header);
            long end = bw.Position;
            bw.Position = 0x24;
            bw.WriteUInt32(_classNameOffsets[ClassName(rootType)]);
            bw.Position = end;
        }
        else
        {
            bw.WriteUInt32(Magic0);
            bw.WriteUInt32(Magic1);
            bw.WriteInt32(0); // user tag
            bw.WriteInt32(0x0B); // version
            bw.WriteByte(8); // pointer size
            bw.WriteByte(1); // little endian
            bw.WriteByte(0); // padding option
            bw.WriteByte(1); // base class
            bw.WriteInt32(3); // sections
            bw.WriteInt32(2); // contents section
            bw.WriteInt32(0); // contents offset
            bw.WriteInt32(0); // contents class name section
            bw.WriteInt32(0x4B); // contents class name offset: the root's name, after the four built-in class names
            bw.WriteFixStr("hk_2014.1.0-r1", 16, 0xFF);
            bw.WriteInt32(0); // flags
            bw.WriteInt16(21);
            bw.WriteInt16(16); // section offset
            bw.WriteInt32(20);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
        }

        byte[] Table<T>(IEnumerable<T> rows, Action<BinaryWriterEx, T> write)
        {
            var t = new BinaryWriterEx(_be);
            foreach (T row in rows) write(t, row);
            return t.FinishBytes();
        }

        var sections = new (string Tag, byte[] Data, byte[] Local, byte[] Global, byte[] Virtual)[]
        {
            ("__classnames__", classData, Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>()),
            source != null
                ? ("__types__", source.TypesData, source.TypesLocal, source.TypesGlobal, source.TypesVirtual)
                : ("__types__", Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>(), Array.Empty<byte>()),
            ("__data__", objectData,
                Table(_localFixups.OrderBy(x => x.Dst), (t, x) => { t.WriteUInt32(x.Src); t.WriteUInt32(x.Dst); }),
                Table(GlobalFixupsInOrder(source),(t, x) => { t.WriteUInt32(x.Src); t.WriteUInt32(x.Section); t.WriteUInt32(x.Dst); }),
                Table(_virtualFixups, (t, x) => { t.WriteUInt32(x.Src); t.WriteUInt32(ClassNamesSection); t.WriteUInt32(x.NameOffset); }))
        }.OrderBy(s => Array.IndexOf(_sectionOrder, s.Tag)).ToArray();

        for (int i = 0; i < sections.Length; i++)
        {
            bw.WriteFixStr(sections[i].Tag, 19);
            bw.WriteByte(0xFF);
            for (int j = 0; j < 7; j++) bw.ReserveUInt32($"section{i}_{j}");
            if (source != null) bw.WriteBytes(source.SectionHeaderExtra);
            else for (int j = 0; j < 4; j++) bw.WriteUInt32(0xFFFFFFFF);
        }

        for (int i = 0; i < sections.Length; i++)
        {
            var s = sections[i];
            uint start = (uint)bw.Position;
            bw.FillUInt32($"section{i}_0", start);
            bw.WriteBytes(s.Data);
            PadFF(bw);
            bw.FillUInt32($"section{i}_1", (uint)bw.Position - start);
            // the fixup tables: padded to 16, and to the source's length where it was longer (Havok 5.5 leaves empty rows)
            int[]? sourceLengths = i == DataSection ? source?.DataPartLengths : null;
            bw.WriteBytes(s.Local);
            PadFF(bw, s.Local.Length, sourceLengths?[0]);
            bw.FillUInt32($"section{i}_2", (uint)bw.Position - start);
            bw.WriteBytes(s.Global);
            PadFF(bw, s.Global.Length, sourceLengths?[1]);
            bw.FillUInt32($"section{i}_3", (uint)bw.Position - start);
            bw.WriteBytes(s.Virtual);
            PadFF(bw, s.Virtual.Length, sourceLengths?[2]);
            uint end = (uint)bw.Position - start;
            bw.FillUInt32($"section{i}_4", end);
            bw.FillUInt32($"section{i}_5", end);
            bw.FillUInt32($"section{i}_6", end);
        }

        return bw.FinishBytes();
    }

    /// <summary>
    /// The global fixup rows: by source offset (HKX2, and every source whose table was sorted), or in member order where
    /// the source's table was (Havok 5.5).
    /// </summary>
    private IEnumerable<(uint Src, uint Section, uint Dst)> GlobalFixupsInOrder(SourceContainer? source)
    {
        if (source is not { GlobalsInMemberOrder: true }) return _globalFixups.OrderBy(x => x.Src);
        var rank = new Dictionary<uint, int>();
        void Walk(OrderNode node)
        {
            foreach (object item in node.Items)
            {
                if (item is OrderNode child) Walk(child);
                else rank.TryAdd((uint)item, rank.Count);
            }
        }
        Walk(_orderRoot);
        return _globalFixups.OrderBy(x => rank.TryGetValue(x.Src, out int r) ? r : int.MaxValue).ThenBy(x => x.Src);
    }

    private static void PadFF(BinaryWriterEx bw)
    {
        while (bw.Position % 16 != 0) bw.WriteByte(0xFF);
    }

    private static void PadFF(BinaryWriterEx bw, int written, int? sourceLength)
    {
        long start = bw.Position - written;
        PadFF(bw);
        if (sourceLength is { } length && written <= length)
            while (bw.Position - start < length) bw.WriteByte(0xFF);
    }

    private void WriteClassName(string name, uint signature)
    {
        uint offset = (uint)_classes.Position;
        _classes.WriteUInt32(signature);
        _classes.WriteByte(0x09);
        _classes.WriteASCII(name, true);
        _classNameOffsets[name] = offset + 5;
    }

    /// <summary>
    /// The local write queue at the current depth. HKX2 indexes it directly, which throws when queues collapsed
    /// below the depth the last action ran at (one DS3 behavior file); growing the list instead only changes files
    /// that couldn't be written before.
    /// </summary>
    private Queue<Action> CurrentWriteQueue
    {
        get
        {
            while (_localWriteQueues.Count <= _currWriteQueue) _localWriteQueues.Add(new Queue<Action>());
            return _localWriteQueues[_currWriteQueue];
        }
    }

    private void PushWriteQueue()
    {
        _currWriteQueue++;
        if (_currWriteQueue == _localWriteQueues.Count) _localWriteQueues.Add(new Queue<Action>());
    }

    private void PopWriteQueue() => _currWriteQueue--;

    private void PushSerializationQueue()
    {
        _currSerializationQueue++;
        if (_currSerializationQueue == _serializationQueues.Count) _serializationQueues.Add(new Queue<object>());
    }

    private void PopSerializationQueue() => _currSerializationQueue--;

    /// <summary>
    /// Writes a record at the current position: its fields at their offsets, zeros between, to its size.
    /// </summary>
    private void WriteRecord(HavokType type, object obj)
    {
        long start = _data.Position;
        IHavokFieldAccess access = ClassSet.Access(obj, type);
        foreach (HavokType.Member field in type.Fields)
        {
            if (_data.Position - start > field.Offset)
                throw new InvalidOperationException($"Wrote past the start of \"{field.Name}\" in \"{type.Identity}\".");
            while (_data.Position - start < field.Offset) _data.WriteByte(0);
            if (field.NonSerializable)
            {
                // the bytes read, else zeros, but an unserialized hkArray as an empty array: no data, DONT_DEALLOCATE
                if (PreserveSource && UnserializedRead.TryGetValue(obj, out var kept) && kept.TryGetValue(field.Name, out byte[]? bytes))
                    _data.WriteBytes(bytes);
                else if (field.Type.Name == "hkArray" && NonSerializedArrayFlags)
                {
                    WritePointerZero();
                    _data.WriteUInt32(0);
                    _data.WriteUInt32(0x80000000);
                    for (int i = _ps + 8; i < field.Type.Size; i++) _data.WriteByte(0);
                }
                else for (int i = 0; i < field.Type.Size; i++) _data.WriteByte(0);
                continue;
            }

            if (!access.TryGetField(field.Name, out object? value))
                throw new InvalidOperationException($"Unable to get field \"{field.Name}\" of \"{type.Identity}\".");

            // an hkVariant's class points at the object's hkClass in the __types__ section kept from the source
            if (value is null && field.Name == "class" && type.Name == "hkVariant" && _source is { TypeClasses.Count: > 0 } &&
                access.TryGetField("object", out object? variantObject) && variantObject != null &&
                Registry.GetType(variantObject.GetType()) is { } variantType &&
                _source.TypeClasses.TryGetValue(ClassName(variantType), out uint classAt))
            {
                _globalFixups.Add(((uint)_data.Position, TypesSection, classAt));
                _orderCurrent.Items.Add((uint)_data.Position);
                WritePointerZero();
                continue;
            }

            WriteValue(field.Type, value, start + field.Offset);
        }

        while (_data.Position - start < type.Size) _data.WriteByte(0);
    }

    private void WriteValue(HavokType type, object? value, long fieldPosition)
    {
        switch (type.Kind)
        {
            case HavokType.TypeKind.Bool:
                _data.WriteByte((bool)value! ? (byte)1 : (byte)0);
                break;
            case HavokType.TypeKind.Int:
                WriteInt(type.Format, value!);
                break;
            case HavokType.TypeKind.Float:
                if (type.Size == 2) _data.WriteUInt16((ushort)(BitConverter.SingleToInt32Bits((float)value!) >> 16));
                else if (type.Size == 8) _data.WriteDouble((double)value!);
                else _data.WriteSingle((float)value!);
                break;
            case HavokType.TypeKind.String:
                WriteStringPointer((string?)value);
                break;
            case HavokType.TypeKind.Pointer:
                WriteObjectPointer(value);
                break;
            case HavokType.TypeKind.Record:
                WriteRecord(type, value ?? type.Instantiate());
                break;
            case HavokType.TypeKind.Array:
                WriteArray(type, value);
                break;
            default:
                throw new NotImplementedException($"Writing {type.Kind} ({type.Identity}) is not supported.");
        }
    }

    private void WriteInt(int format, object boxed)
    {
        dynamic value = boxed;
        bool signed = (format & 0x200) != 0;
        switch ((format >> 10) / 8, signed)
        {
            case (1, true): _data.WriteSByte((sbyte)value); break;
            case (1, false): _data.WriteByte((byte)value); break;
            case (2, true): _data.WriteInt16((short)value); break;
            case (2, false): _data.WriteUInt16((ushort)value); break;
            case (4, true): _data.WriteInt32((int)value); break;
            case (4, false): _data.WriteUInt32((uint)value); break;
            case (8, true): _data.WriteInt64((long)value); break;
            default: _data.WriteUInt64((ulong)value); break;
        }
    }

    private void WriteObjectPointer(object? target)
    {
        if (target is null)
        {
            WritePointerZero();
            return;
        }

        uint position = (uint)_data.Position;
        _orderCurrent.Items.Add(position);
        if (_globalLookup.TryGetValue(target, out uint written))
        {
            _globalFixups.Add((position, DataSection, written));
        }
        else
        {
            if (!_pendingGlobals.ContainsKey(target))
            {
                _pendingGlobals.Add(target, new List<uint>());
                PushSerializationQueue();
                _serializationQueues[_currSerializationQueue].Enqueue(target);
                PopSerializationQueue();
                _pendingVirtuals.Add(target);
            }

            _pendingGlobals[target].Add(position);
        }

        WritePointerZero();
    }

    private void WritePointerZero()
    {
        if (_ps == 4) _data.WriteUInt32(0);
        else _data.WriteUInt64(0);
    }

    private void WriteStringPointer(string? value)
    {
        WritePointerZero();
        if (value is null) return;

        var fixup = new Fixup { Src = (uint)_data.Position - (uint)_ps };
        _localFixups.Add(fixup);
        CurrentWriteQueue.Enqueue(() =>
        {
            fixup.Dst = (uint)_data.Position;
            _data.WriteBytes(PackfileText.Encode(value));
            _data.WriteByte(0);
            _data.Pad(16);
        });
    }

    private void WriteArray(HavokType type, object? value)
    {
        if (type.Type == typeof(Vector4))
        {
            var v = (Vector4)value!;
            _data.WriteSingle(v.X); _data.WriteSingle(v.Y); _data.WriteSingle(v.Z); _data.WriteSingle(v.W);
            return;
        }

        if (type.Type == typeof(Quaternion))
        {
            var q = (Quaternion)value!;
            _data.WriteSingle(q.X); _data.WriteSingle(q.Y); _data.WriteSingle(q.Z); _data.WriteSingle(q.W);
            return;
        }

        if (type.Type == typeof(Matrix4x4))
        {
            WriteMatrix((Matrix4x4)value!, 4);
            return;
        }

        if (type.Type is { } m3 && m3 == ClassSet.Matrix3x3Type)
        {
            WriteMatrix(ClassSet.FromMatrix3x3(value!), 3);
            return;
        }

        HavokType element = type.SubType!;
        switch (type.Name)
        {
            case "hkArray":
            case "hkSimpleArray":
            {
                var list = value as System.Collections.IList;
                int count = list?.Count ?? 0;
                uint fieldStart = (uint)_data.Position;
                WritePointerZero();
                _data.WriteUInt32((uint)count);

                // hkArray: capacity and DONT_DEALLOCATE (16 bytes). hkSimpleArray is just the pointer and count (12).
                if (type.Name == "hkArray")
                    _data.WriteUInt32(PreserveSource && list != null && ArrayFlagsRead.TryGetValue(list, out var flags) && (flags.Value & 0x3FFFFFFF) == count
                        ? flags.Value
                        : (uint)count | 0x80000000);
                if (count == 0) return;

                var fixup = new Fixup { Src = fieldStart };
                _localFixups.Add(fixup);
                var contentsOrder = new OrderNode();
                _orderCurrent.Items.Add(contentsOrder);
                CurrentWriteQueue.Enqueue(() =>
                {
                    fixup.Dst = (uint)_data.Position;
                    OrderNode outer = _orderCurrent;
                    _orderCurrent = contentsOrder;
                    PushWriteQueue(); // anything the elements queue comes first
                    foreach (object? e in list!) WriteValue(element, e, _data.Position);
                    PopWriteQueue();
                    _orderCurrent = outer;
                });

                // HKX2 pads after arrays of records and of matrices/quaternions, not after other arrays
                if (PadsAfterArray(element)) CurrentWriteQueue.Enqueue(() => _data.Pad(16));
                return;
            }
            case "hkRelArray":
                // size and offset are filled in when the data is written after the object (WriteRelArrayData)
                _data.WriteUInt32(0);
                return;
            case "T[N]":
            {
                foreach (object? e in (Array)value!) WriteValue(element, e, _data.Position);
                return;
            }
            default:
                throw new NotImplementedException($"Writing arrays of type {type.Name} is not supported.");
        }
    }

    private bool PadsAfterArray(HavokType element) =>
        element.Kind == HavokType.TypeKind.Record ||
        element.Type == typeof(Quaternion) || element.Type == typeof(Matrix4x4) ||
        (element.Type is { } m3 && m3 == ClassSet.Matrix3x3Type);

    /// <summary>
    /// hkRelArray data follows the object it belongs to; each field holds its count and its offset from the field.
    /// </summary>
    private void WriteRelArrayData(HavokType type, object obj, long objectStart)
    {
        IHavokFieldAccess? access = null;
        foreach (HavokType.Member field in type.Fields.Where(f => f.Type.Name == "hkRelArray" && !f.NonSerializable))
        {
            access ??= ClassSet.Access(obj, type);
            access.TryGetField(field.Name, out object? value);
            if (value is not System.Collections.IList { Count: > 0 } list) continue;

            HavokType element = field.Type.SubType!;
            _data.Pad(Math.Max(1, element.Alignment));
            long dataStart = _data.Position;
            foreach (object? e in list) WriteValue(element, e, _data.Position);
            long end = _data.Position;

            long fieldPosition = objectStart + field.Offset;
            _data.Position = fieldPosition;
            _data.WriteUInt16((ushort)list.Count);
            _data.WriteUInt16((ushort)(dataStart - fieldPosition));
            _data.Position = end;
        }
    }

    private void WriteMatrix(Matrix4x4 m, int rows)
    {
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < 4; j++)
                _data.WriteSingle(m[i, j]);
    }

    #endregion
}

/// <summary>
/// Packfile strings are bytes: ASCII mostly, Shift-JIS in some (DeS scene and node names). They decode as Shift-JIS
/// where that gives the same bytes back, else as Latin-1, so every string writes back as it was read.
/// </summary>
internal static class PackfileText
{
    private static readonly Encoding ShiftJis;

    static PackfileText()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        ShiftJis = Encoding.GetEncoding(932, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
    }

    public static string Decode(ReadOnlySpan<byte> bytes)
    {
        if (!bytes.ContainsAnyInRange((byte)0x80, (byte)0xFF)) return Encoding.ASCII.GetString(bytes);
        try
        {
            string s = ShiftJis.GetString(bytes);
            if (ShiftJis.GetBytes(s).AsSpan().SequenceEqual(bytes)) return s;
        }
        catch (DecoderFallbackException) { }
        return Encoding.Latin1.GetString(bytes);
    }

    public static byte[] Encode(string value)
    {
        if (Ascii.IsValid(value)) return Encoding.ASCII.GetBytes(value);
        try
        {
            byte[] bytes = ShiftJis.GetBytes(value);
            if (ShiftJis.GetString(bytes) == value) return bytes;
        }
        catch (EncoderFallbackException) { }
        return Encoding.Latin1.GetBytes(value);
    }
}
