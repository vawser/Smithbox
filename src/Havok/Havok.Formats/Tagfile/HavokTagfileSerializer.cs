using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Havok.Reflection;

namespace Havok.Formats.Tagfile;

/// <summary>
/// Reads and writes binary tagfiles (TAG0) and type compendiums (TCM0) for any SDK, through a
/// <see cref="TagfileDialect" />. Merged from HKLib's hk2018 and hk2019 HavokBinarySerializers.
///
/// Reads keep their state per call, so once a compendium is loaded one serializer can read files on several threads at
/// once. A root object remembers (per object, across serializers) the compendium id and TYPE section it was read with,
/// so writing it back can reproduce them.
/// </summary>
public sealed class HavokTagfileSerializer
{
    private const int MemberExtraValueFlag = 0x80;

    /// <summary>
    /// The extra values of members flagged 0x80, per SDK, keyed by type and member name. The registry doesn't store
    /// them; the 2019 seeds are the values every AC6 file uses (from HKLib). Values read from files are added.
    /// HKLib reads them keyed by type name and writes them looked up by type identity; that is kept for parity.
    /// </summary>
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, ulong>> MemberExtraValuesBySdk = new()
    {
        ["20190100"] = new ConcurrentDictionary<string, ulong>
        {
            ["hknpMotionProperties.isExclusive"] = 15,
            ["hknpMaterial.name"] = 4,
            ["hknpMaterial.userData"] = 3,
            ["hknpBodyCinfo.userData"] = 3,
            ["hknpExternMeshShape.geometry"] = 4,
            ["hknpDefaultExternMeshShapeGeometry.geometry"] = 4,
            ["hknpShapeMassProperties.compressedMassProperties"] = 3,
            ["hkPackedVector3.values"] = 3,
            ["hkCompressedMassProperties.majorAxisSpace"] = 3,
            ["hkCompressedMassProperties.inertia"] = 3,
            ["hkCompressedMassProperties.centerOfMass"] = 3
        }
    };

    private static readonly ConditionalWeakTable<object, StrongBox<ulong>> CompendiumIdsRead = new();
    private static readonly ConditionalWeakTable<object, TypeSection> TypeSectionsRead = new();

    private readonly ConcurrentDictionary<string, ulong> _memberExtraValues;
    private HavokCompendium? _compendium;

    public HavokTagfileSerializer(IHavokClassSet classSet, TagfileDialect dialect)
    {
        ClassSet = classSet;
        Dialect = dialect;
        _memberExtraValues = MemberExtraValuesBySdk.GetOrAdd(dialect.SdkVersion, _ => new ConcurrentDictionary<string, ulong>());
    }

    public IHavokClassSet ClassSet { get; }

    public TagfileDialect Dialect { get; }

    private HavokTypeRegistry Registry => ClassSet.Registry;

    /// <summary>
    /// The compendium tagfiles without a TYPE section are read against, and that writes refer to.
    /// </summary>
    public HavokCompendium? Compendium => _compendium;

    public void LoadCompendium(HavokCompendium compendium) => _compendium = compendium;

    public void LoadCompendium(Memory<byte> data) => _compendium = ReadCompendium(data);

    #region Public API

    public HavokCompendium ReadCompendium(Memory<byte> data)
    {
        TagfileReader reader = new(data);
        reader.EnterSection("TCM0");
        IReadOnlyList<ulong> ids = ReadTCID(reader);
        IReadOnlyList<HavokType> types = ReadTYPE(reader, out List<HavokType> hashedTypes, out TypeSection section);
        reader.ExitSection();
        reader.Close();

        return new HavokCompendium(ids.ToList(), types.ToList(), Dialect.Sdk)
        {
            TopLevelTypes = hashedTypes,
            TypeSectionBytes = section.Bytes
        };
    }

    public byte[] WriteCompendium(HavokCompendium compendium)
    {
        TagfileWriter writer = new();
        WriteTCM0(writer, compendium);
        return writer.FinishBytes();
    }

    /// <summary>
    /// Reads a tagfile and returns its root level object.
    /// </summary>
    public object Read(Memory<byte> data)
    {
        return ReadAllObjects(data).FirstOrDefault() ?? throw new InvalidDataException("No root level Havok object found.");
    }

    /// <summary>
    /// Reads a tagfile and returns every Havok object in it, the root level object first.
    /// </summary>
    public IReadOnlyList<object> ReadAllObjects(Memory<byte> data)
    {
        TagfileReader reader = new(data);
        IReadOnlyList<object> objects = ReadTAG0(reader);
        reader.Close();
        return objects.Distinct(ReferenceEqualityComparer.Instance).ToList();
    }

    /// <summary>
    /// Writes a tagfile. Any object can be the root, but games expect an hkRootLevelContainer.
    /// </summary>
    public byte[] Write(object rootLevelObject)
    {
        TagfileWriter writer = new();
        WriteTAG0(writer, rootLevelObject);
        return writer.FinishBytes();
    }

    /// <summary>
    /// The types a tagfile (TAG0 with a TYPE section) or compendium (TCM0) describes itself, built from its own type
    /// bodies and hashes, without the registry. Index 0 is null. Types without a body in the file (2019 files store
    /// bodies for only some types) have only a name and template parameters. Returns an empty list for a tagfile that
    /// refers to a compendium. Used to compare a game's types with a registry and to read files untyped.
    /// </summary>
    public static IReadOnlyList<HavokType?> ReadTypeDescriptions(Memory<byte> data)
    {
        TagfileReader reader = new(data);
        string root = reader.GetSectionId();
        reader.EnterSection(root);
        while (reader.Position < reader.GetSectionEnd())
        {
            string id = reader.GetSectionId();
            if (id != "TYPE")
            {
                reader.EnterSection(id);
                reader.SkipSection();
                continue;
            }

            reader.EnterSection("TYPE");
            reader.EnterSection("TPTR");
            reader.ExitSection();
            IReadOnlyList<string> typeStrings = ReadStrings(reader);
            IReadOnlyList<HavokTypeBuilder> builders = ReadTNA1(reader, typeStrings);
            IReadOnlyList<string> fieldStrings = ReadStrings(reader);
            ReadTBDY(reader, builders, fieldStrings, null);

            if (reader.GetSectionId() == "THSH")
            {
                reader.EnterSection("THSH");
                int numHashes = (int)reader.ReadHavokVarUInt();
                for (int i = 0; i < numHashes; i++)
                {
                    int typeIndex = (int)reader.ReadHavokVarUInt();
                    builders[typeIndex].WithHash(reader.ReadUInt32());
                }

                reader.SkipSection();
            }

            return builders.Select((b, i) => i == 0 ? null : b.Build()).ToList();
        }

        return Array.Empty<HavokType?>();
    }

    #endregion

    #region Type lookup

    private IReadOnlyList<HavokType> GetTypesFromCompendium(ulong compendiumId)
    {
        if (_compendium is null)
        {
            throw new InvalidOperationException(
                $"Encountered reference to compendium with Id {compendiumId} but no compendium loaded.");
        }

        if (!_compendium.Ids.Contains(compendiumId))
        {
            throw new InvalidOperationException(
                $"Encountered reference to compendium with Id {compendiumId} which does not match the loaded compendium");
        }

        return _compendium.Types;
    }

    /// <summary>
    /// The registry type for each type in the file, matched by identity (the file's own type bodies are only read to
    /// work out the identities).
    /// </summary>
    private IReadOnlyList<HavokType> GetTypesFromRegistry(IReadOnlyList<HavokTypeBuilder> typeBuilders)
    {
        HavokType[] types = new HavokType[typeBuilders.Count];

        // the first type is the null type
        for (int i = 1; i < typeBuilders.Count; i++)
        {
            string typeIdentity = typeBuilders[i].Build().Identity;
            types[i] = Registry.GetType(typeIdentity) ??
                       throw new KeyNotFoundException($"The type \"{typeIdentity}\" was not found in the type registry.");
        }

        return types;
    }

    /// <summary>
    /// The indices of the types in the loaded compendium, if it holds every one of them.
    /// </summary>
    private bool TryGetCompendiumIndices(IEnumerable<HavokType> types, out IReadOnlyDictionary<HavokType, int> typeIndices)
    {
        Dictionary<HavokType, int> indices = new();
        typeIndices = indices;
        if (_compendium is null) return false;

        for (int i = 1; i < _compendium.Types.Count; i++)
        {
            if (_compendium.Types[i] is { } type) indices.TryAdd(type, i);
        }

        return types.All(indices.ContainsKey);
    }

    #endregion

    #region Reading

    private IReadOnlyList<object> ReadTAG0(TagfileReader reader)
    {
        reader.EnterSection("TAG0");

        reader.EnterSection("SDKV");
        string sdkVersion = reader.ReadASCII(8);
        if (sdkVersion != Dialect.SdkVersion)
        {
            throw new InvalidDataException($"Unsupported SDK Version. Found: {sdkVersion} | Expected: {Dialect.SdkVersion}");
        }

        reader.ExitSection();

        // DATA is read later, through ITEM
        reader.EnterSection("DATA");
        long dataOffset = reader.Position;
        reader.SkipSection();

        IReadOnlyList<HavokType> types;
        ulong? compendiumId = null;
        TypeSection? typeSection = null;
        switch (reader.GetSectionId())
        {
            case "TYPE":
                types = ReadTYPE(reader, out _, out typeSection);
                break;
            case "TCRF":
                reader.EnterSection("TCRF");
                compendiumId = reader.ReadUInt64();
                reader.AssertUInt64(0);
                reader.AssertUInt64(0);
                reader.ExitSection();
                types = GetTypesFromCompendium(compendiumId.Value);
                break;
            default:
                throw new InvalidDataException("No TYPE section or compendium reference found");
        }

        IReadOnlyList<object> objects = ReadINDX(reader, types, dataOffset);
        reader.ExitSection();

        if (objects.FirstOrDefault() is { } root)
        {
            if (compendiumId is { } id) CompendiumIdsRead.AddOrUpdate(root, new StrongBox<ulong>(id));
            if (typeSection is not null) TypeSectionsRead.AddOrUpdate(root, typeSection);
        }

        return objects;
    }

    private IReadOnlyList<object> ReadINDX(TagfileReader reader, IReadOnlyList<HavokType> types, long dataOffset)
    {
        reader.EnterSection("INDX");

        TagfileReadContext context = new(ClassSet);
        reader.EnterSection("ITEM");
        while (reader.Position < reader.GetSectionEnd())
        {
            uint typeIndexAndFlags = reader.ReadUInt32();
            int typeIndex = (int)(typeIndexAndFlags & 0xFFFFFF);
            IndexItem.ItemKind itemKind = (IndexItem.ItemKind)(typeIndexAndFlags >> 28);
            if (typeIndex != 0 && !Enum.IsDefined(itemKind)) throw new NotImplementedException();

            HavokType type = types[typeIndex];
            long offset = dataOffset + reader.ReadInt32();
            int count = reader.ReadInt32();
            context.AddItem(type, itemKind, offset, count);
        }

        reader.ExitSection();

        List<object> objects = new();
        for (int i = 1; i < context.Items.Count; i++)
        {
            foreach (object obj in context.GetItem((ulong)i, reader))
            {
                if (obj is not null && ClassSet.IsHavokObject(obj))
                {
                    objects.Add(obj);
                }
            }
        }

        // PTCH is only used for in-place loading, and 2019 files leave it out when there are no pointers
        if (reader.Position < reader.GetSectionEnd())
        {
            reader.EnterSection("PTCH");
            reader.SkipSection();
        }

        reader.ExitSection();
        return objects;
    }

    private static IReadOnlyList<ulong> ReadTCID(TagfileReader reader)
    {
        reader.EnterSection("TCID");
        ulong[] ids = reader.ReadUInt64s(reader.GetSectionLength() / 8);
        reader.ExitSection();
        return ids;
    }

    /// <param name="hashedTypes">The types whose hash is stored: the top level types when writing</param>
    /// <param name="section">The section as it was read, to write it back</param>
    private IReadOnlyList<HavokType> ReadTYPE(TagfileReader reader, out List<HavokType> hashedTypes, out TypeSection section)
    {
        long start = reader.Position;
        hashedTypes = new List<HavokType>();
        reader.EnterSection("TYPE");

        // TPTR is space for pointers for in-place loading; its contents are 0
        reader.EnterSection("TPTR");
        reader.ExitSection();

        IReadOnlyList<string> typeStrings = ReadStrings(reader);
        IReadOnlyList<HavokTypeBuilder> typeBuilders = ReadTNA1(reader, typeStrings);

        // the type bodies are only needed for the identities, since types are looked up in the registry
        IReadOnlyList<string> fieldStrings = ReadStrings(reader);
        ReadTBDY(reader, typeBuilders, fieldStrings);

        IReadOnlyList<HavokType> types = GetTypesFromRegistry(typeBuilders);

        if (reader.GetSectionId() == "THSH")
        {
            ReadTHSH(reader, types, hashedTypes);
        }

        reader.EnterSection("TPAD");
        reader.ExitSection();

        reader.ExitSection();

        section = new TypeSection(reader.GetBytes(start, (int)(reader.Position - start)), types);
        return types;
    }

    /// <summary>
    /// A section of null-terminated strings (TSTR/FSTR in 2018, TST1/FST1 padded with 0xFF in 2019).
    /// </summary>
    private static IReadOnlyList<string> ReadStrings(TagfileReader reader)
    {
        reader.EnterSection(reader.GetSectionId());

        List<string> strings = new();
        while (reader.Position < reader.GetSectionEnd() && reader.GetByte(reader.Position) != 0xFF)
        {
            strings.Add(reader.ReadASCII());
        }

        reader.SkipSection();
        return strings;
    }

    private static IReadOnlyList<HavokTypeBuilder> ReadTNA1(TagfileReader reader, IReadOnlyList<string> typeStrings)
    {
        reader.EnterSection("TNA1");
        int numTypes = (int)reader.ReadHavokVarUInt();

        // type 0 is the null type, which is not serialized
        HavokTypeBuilder[] typeBuilders = new HavokTypeBuilder[numTypes];
        typeBuilders[0] = new HavokTypeBuilder().WithName("NULL").WithFlags(HavokType.TypeFlags.NonSerializable);
        for (int i = 1; i < numTypes; i++) typeBuilders[i] = new HavokTypeBuilder();
        for (int i = 1; i < numTypes; i++)
        {
            HavokTypeBuilder builder = typeBuilders[i];
            builder.WithName(typeStrings[(int)reader.ReadHavokVarUInt()]);
            int templateParamCount = (int)reader.ReadHavokVarUInt();

            for (int j = 0; j < templateParamCount; j++)
            {
                string paramName = typeStrings[(int)reader.ReadHavokVarUInt()];
                int paramValue = (int)reader.ReadHavokVarUInt();
                switch (paramName[0])
                {
                    case 't':
                        builder.WithTemplateParameter(paramName[1..], typeBuilders[paramValue]);
                        break;
                    case 'v':
                        builder.WithTemplateParameter(paramName[1..], paramValue);
                        break;
                    default:
                        throw new InvalidDataException(
                            $"Invalid template parameter name prefix encountered. Value: {paramName[0]}");
                }
            }
        }

        reader.ExitSection();
        return typeBuilders;
    }

    private void ReadTBDY(TagfileReader reader, IReadOnlyList<HavokTypeBuilder> builders, IReadOnlyList<string> fieldStrings)
        => ReadTBDY(reader, builders, fieldStrings, _memberExtraValues);

    /// <param name="memberExtraValues">Where to keep the extra values of flagged members; null to drop them</param>
    private static void ReadTBDY(TagfileReader reader, IReadOnlyList<HavokTypeBuilder> builders, IReadOnlyList<string> fieldStrings,
        ConcurrentDictionary<string, ulong>? memberExtraValues)
    {
        reader.EnterSection("TBDY");

        // 2019 files don't store a body for every type, so read to the end of the section
        while (reader.Position < reader.GetSectionEnd())
        {
            int typeIndex = (int)reader.ReadHavokVarUInt();

            // padding at the end of the section
            if (typeIndex == 0) continue;

            HavokTypeBuilder builder = builders[typeIndex];
            int parentIndex = (int)reader.ReadHavokVarUInt();
            if (parentIndex > 0)
            {
                builder.WithParent(builders[parentIndex]);
            }

            HavokType.Optional optionals = (HavokType.Optional)reader.ReadHavokVarUInt();
            builder.WithOptionals(optionals);
            if (optionals.HasFlag(HavokType.Optional.Format))
            {
                builder.WithFormat((int)reader.ReadHavokVarUInt());
            }

            if (optionals.HasFlag(HavokType.Optional.SubType))
            {
                builder.WithSubType(builders[(int)reader.ReadHavokVarUInt()]);
            }

            if (optionals.HasFlag(HavokType.Optional.Version))
            {
                builder.WithVersion(reader.ReadHavokVarUInt());
            }

            if (optionals.HasFlag(HavokType.Optional.SizeAlign))
            {
                builder.WithSizeAlignment((int)reader.ReadHavokVarUInt(), (int)reader.ReadHavokVarUInt());
            }

            if (optionals.HasFlag(HavokType.Optional.Flags))
            {
                builder.WithFlags((HavokType.TypeFlags)reader.ReadHavokVarUInt());
            }

            if (optionals.HasFlag(HavokType.Optional.Members))
            {
                // the two high bytes are the number of properties
                uint numMembers = (uint)reader.ReadHavokVarUInt();
                int numProperties = (int)(numMembers >> 16);
                builder.WithProperties(new HavokType.Member?[numProperties].ToList());

                int numFields = (int)(numMembers & 0xffff);
                for (int j = 0; j < numFields; j++)
                {
                    string name = fieldStrings[(int)reader.ReadHavokVarUInt()];
                    int flags = (int)reader.ReadHavokVarUInt();

                    // the meaning of the extra value is unknown; it is kept so it can be written back
                    if ((flags & MemberExtraValueFlag) != 0)
                    {
                        ulong extraValue = reader.ReadHavokVarUInt();
                        memberExtraValues?.TryAdd($"{builder.Name}.{name}", extraValue);
                        flags &= ~MemberExtraValueFlag;
                    }

                    int offset = (int)reader.ReadHavokVarUInt();
                    int memberTypeIndex = (int)reader.ReadHavokVarUInt();
                    builder.WithField(name, (HavokType.Member.MemberFlags)flags, offset, builders[memberTypeIndex]);
                }
            }

            if (optionals.HasFlag(HavokType.Optional.Interfaces))
            {
                int numInterfaces = (int)reader.ReadHavokVarUInt();
                for (int j = 0; j < numInterfaces; j++)
                {
                    int interfaceTypeIndex = (int)reader.ReadHavokVarUInt();
                    int offset = (int)reader.ReadHavokVarUInt();
                    builder.WithInterface(builders[interfaceTypeIndex], offset);
                }
            }

            if (optionals.HasFlag(HavokType.Optional.Attributes))
            {
                throw new NotImplementedException("Type attributes are not supported.");
            }
        }

        reader.SkipSection();
    }

    private static void ReadTHSH(TagfileReader reader, IReadOnlyList<HavokType> types, List<HavokType> hashedTypes)
    {
        reader.EnterSection("THSH");
        int numHashes = (int)reader.ReadHavokVarUInt();
        for (int i = 0; i < numHashes; i++)
        {
            int typeIndex = (int)reader.ReadHavokVarUInt();
            hashedTypes.Add(types[typeIndex]);

            if (types[typeIndex].Hash is not { } hash)
            {
                throw new InvalidDataException($"Unable to verify hash for type {types[typeIndex].Identity}.");
            }

            if (reader.ReadUInt32() != hash)
            {
                throw new InvalidDataException($"Incorrect hash encountered for type {types[typeIndex].Identity}.");
            }
        }

        reader.SkipSection();
    }

    #endregion

    #region Writing

    private void WriteTAG0(TagfileWriter writer, object rootLevelObject)
    {
        writer.BeginSection("TAG0");

        writer.BeginSection("SDKV");
        writer.WriteASCII(Dialect.SdkVersion);
        writer.EndSection();

        TagfileWriteContext context = new(ClassSet, Dialect);
        long dataOffset = WriteDATA(writer, rootLevelObject, context);

        HashSet<HavokType> topLevelTypes = new();
        foreach (IndexItem item in context.Items.Skip(1))
        {
            topLevelTypes.Add(item.Type);
        }

        foreach (HavokType type in context.Patches.Keys)
        {
            topLevelTypes.Add(type);
        }

        // The types come from the loaded compendium if it has them all, then from the TYPE section the object was read
        // with, and are only written out from the registry when neither has them (each step as the dialect allows)
        IReadOnlyDictionary<HavokType, int> typeIndices;
        if (Dialect.WriteCompendiumReference && TryGetCompendiumIndices(topLevelTypes, out typeIndices))
        {
            ulong compendiumId = CompendiumIdsRead.TryGetValue(rootLevelObject, out StrongBox<ulong>? idRead) &&
                                 _compendium!.Ids.Contains(idRead.Value)
                ? idRead.Value
                : _compendium!.Ids[0];

            writer.BeginSection("TCRF");
            writer.WriteUInt64(compendiumId);
            writer.WriteUInt64s(new ulong[2]);
            writer.EndSection();
        }
        else if (Dialect.ReuseTypeSection && TypeSectionsRead.TryGetValue(rootLevelObject, out TypeSection? typeSection) &&
                 typeSection.TryGetIndices(topLevelTypes, out typeIndices))
        {
            writer.WriteBytes(typeSection.Bytes);
        }
        else
        {
            typeIndices = WriteTYPE(writer, context.Types, topLevelTypes.ToList());
        }

        writer.BeginSection("INDX");
        WriteITEM(writer, typeIndices, context, dataOffset);
        WritePTCH(writer, typeIndices, context, dataOffset);
        writer.EndSection();

        writer.EndSection();
    }

    private long WriteDATA(TagfileWriter writer, object rootLevelObject, TagfileWriteContext context)
    {
        writer.BeginSection("DATA");
        long dataOffset = writer.Position;

        HavokType objectType = Registry.GetType(rootLevelObject.GetType()) ??
                               throw new ArgumentException("No type data found for the provided object",
                                   nameof(rootLevelObject));
        context.Enqueue(objectType, dataOffset, rootLevelObject);

        if (Dialect.SdkItemOrder)
        {
            // Objects which are pointed to are written in the order they are found, each followed by the arrays and
            // strings it owns, breadth first, as the 2019 SDK does
            Queue<int> pointedItems = new();
            pointedItems.Enqueue(1);
            while (pointedItems.TryDequeue(out int pointedIndex))
            {
                Queue<int> ownedItems = new();
                WriteItem(writer, context, pointedIndex, pointedItems, ownedItems);
                while (ownedItems.TryDequeue(out int ownedIndex))
                {
                    WriteItem(writer, context, ownedIndex, pointedItems, ownedItems);
                }
            }
        }
        else
        {
            // in the order the items were enqueued (HKLib 2018)
            for (int i = 1; i < context.Items.Count; i++)
            {
                context.Items[i].WriteObjects(writer, context);
            }
        }

        if (Dialect.PadDataTo16) writer.Pad(16);
        writer.EndSection();
        return dataOffset;
    }

    /// <summary>
    /// Writes an item and sorts the items it refers to into pointed-to objects and owned arrays.
    /// </summary>
    private static void WriteItem(TagfileWriter writer, TagfileWriteContext context, int index,
        Queue<int> pointedItems, Queue<int> ownedItems)
    {
        int firstNewIndex = context.Items.Count;
        context.Items[index].WriteObjects(writer, context);

        for (int i = firstNewIndex; i < context.Items.Count; i++)
        {
            if (context.Items[i].Kind == IndexItem.ItemKind.Pointer) pointedItems.Enqueue(i);
            else ownedItems.Enqueue(i);
        }
    }

    private static void WriteITEM(TagfileWriter writer, IReadOnlyDictionary<HavokType, int> typeIndices,
        TagfileWriteContext context, long dataOffset)
    {
        writer.BeginSection("ITEM");

        // null item
        writer.WriteInt32(0);
        writer.WriteInt32(0);
        writer.WriteInt32(0);

        for (int index = 1; index < context.Items.Count; index++)
        {
            IndexItem item = context.Items[index];
            writer.WriteInt32(typeIndices[item.Type] | ((int)item.Kind << 28));
            writer.WriteInt32((int)(item.Offset - dataOffset));
            writer.WriteInt32(item.Count);
        }

        writer.EndSection();
    }

    private void WritePTCH(TagfileWriter writer, IReadOnlyDictionary<HavokType, int> typeIndices,
        TagfileWriteContext context, long dataOffset)
    {
        if (Dialect.CompactPatches && context.Patches.Count == 0) return;

        writer.BeginSection("PTCH");

        IEnumerable<KeyValuePair<HavokType, List<long>>> patches = Dialect.CompactPatches
            ? context.Patches.OrderBy(x => typeIndices[x.Key])
            : context.Patches;

        foreach ((HavokType type, List<long> positions) in patches)
        {
            writer.WriteInt32(typeIndices[type]);
            writer.WriteInt32(positions.Count);
            foreach (long position in positions.Order())
            {
                writer.WriteUInt32((uint)(position - dataOffset));
            }
        }

        writer.EndSection();
    }

    private void WriteTCM0(TagfileWriter writer, HavokCompendium compendium)
    {
        writer.BeginSection("TCM0");

        writer.BeginSection("TCID");
        foreach (ulong id in compendium.Ids)
        {
            writer.WriteUInt64(id);
        }

        writer.EndSection();

        if (Dialect.ReuseTypeSection && compendium.TypeSectionBytes is { } bytes)
        {
            writer.WriteBytes(bytes);
        }
        else
        {
            // a compendium that was read starts with the null type. Every type the types refer to is written too, as
            // types written out from the registry describe all of their members.
            List<HavokType> types = compendium.Types.Where(x => x is not null).ToList();
            TagfileWriteContext context = new(ClassSet, Dialect);
            foreach (HavokType type in types) context.GetTypesForWrite(type);
            types.AddRange(context.Types.Except(types));

            WriteTYPE(writer, types, compendium.TopLevelTypes);
        }

        writer.EndSection();
    }

    private IReadOnlyDictionary<HavokType, int> WriteTYPE(TagfileWriter writer, IReadOnlyList<HavokType> types,
        IReadOnlyList<HavokType> topLevelTypes)
    {
        writer.BeginSection("TYPE");

        // space for pointers for in-place loading; the null type is not in the list
        writer.BeginSection("TPTR");
        writer.WriteUInt64s(new ulong[types.Count + 1]);
        writer.EndSection();

        IReadOnlyDictionary<string, int> typeStringIndices = WriteTypeStrings(writer, types);
        IReadOnlyDictionary<HavokType, int> typeIndices = WriteTNA1(writer, types, typeStringIndices);
        IReadOnlyDictionary<string, int> fieldStringIndices = WriteFieldStrings(writer, types);
        WriteTBDY(writer, types, typeIndices, fieldStringIndices);
        WriteTHSH(writer, topLevelTypes, typeIndices);

        writer.BeginSection("TPAD");
        writer.Pad(4);
        writer.EndSection();

        writer.EndSection();
        return typeIndices;
    }

    private IReadOnlyDictionary<string, int> WriteTypeStrings(TagfileWriter writer, IReadOnlyList<HavokType> types)
    {
        writer.BeginSection(Dialect.TypeStringsSection);

        Dictionary<string, int> typeStringIndices = new();
        foreach (HavokType type in types)
        {
            if (!typeStringIndices.ContainsKey(type.Name))
            {
                writer.WriteASCII(type.Name, true);
                typeStringIndices.Add(type.Name, typeStringIndices.Count);
            }

            foreach (HavokType.TemplateParameter param in type.TemplateParameters)
            {
                if (typeStringIndices.ContainsKey(param.Name)) continue;

                // the prefix marks a type or value parameter; TNA1 refers to the name without it
                char prefix = param.Type is null ? 'v' : 't';
                writer.WriteASCII(prefix + param.Name, true);
                typeStringIndices.Add(param.Name, typeStringIndices.Count);
            }
        }

        writer.PadWith(4, Dialect.StringPadding);
        writer.EndSection();
        return typeStringIndices;
    }

    private static IReadOnlyDictionary<HavokType, int> WriteTNA1(TagfileWriter writer, IReadOnlyList<HavokType> types,
        IReadOnlyDictionary<string, int> typeStringIndices)
    {
        writer.BeginSection("TNA1");

        Dictionary<HavokType, int> typeIndices = new();
        for (int i = 0; i < types.Count; i++) typeIndices.Add(types[i], i + 1);

        // the count includes the null type, which is not serialized
        writer.WriteHavokVarUInt((ulong)types.Count + 1);

        foreach (HavokType type in types)
        {
            writer.WriteHavokVarUInt((ulong)typeStringIndices[type.Name]);
            writer.WriteHavokVarUInt((ulong)type.TemplateParameters.Count);
            foreach (HavokType.TemplateParameter param in type.TemplateParameters)
            {
                writer.WriteHavokVarUInt((ulong)typeStringIndices[param.Name]);
                writer.WriteHavokVarUInt(param.Value is { } value ? (ulong)value : (ulong)typeIndices[param.Type!]);
            }
        }

        writer.Pad(4);
        writer.EndSection();
        return typeIndices;
    }

    private IReadOnlyDictionary<string, int> WriteFieldStrings(TagfileWriter writer, IReadOnlyList<HavokType> types)
    {
        writer.BeginSection(Dialect.FieldStringsSection);

        Dictionary<string, int> fieldStringIndices = new();
        foreach (HavokType type in types)
        {
            foreach (HavokType.Member member in type.Fields)
            {
                if (fieldStringIndices.ContainsKey(member.Name)) continue;
                writer.WriteASCII(member.Name, true);
                fieldStringIndices.Add(member.Name, fieldStringIndices.Count);
            }
        }

        writer.PadWith(4, Dialect.StringPadding);
        writer.EndSection();
        return fieldStringIndices;
    }

    private void WriteTBDY(TagfileWriter writer, IReadOnlyList<HavokType> types,
        IReadOnlyDictionary<HavokType, int> typeIndices, IReadOnlyDictionary<string, int> fieldStringIndices)
    {
        writer.BeginSection("TBDY");
        foreach (HavokType type in types)
        {
            writer.WriteHavokVarUInt((ulong)typeIndices[type]);
            writer.WriteHavokVarUInt(type.Parent is null ? 0 : (ulong)typeIndices[type.Parent]);

            HavokType.Optional optionals = type.Optionals;
            writer.WriteHavokVarUInt((ulong)optionals);
            if (optionals.HasFlag(HavokType.Optional.Format))
            {
                writer.WriteHavokVarUInt((ulong)type.Format);
            }

            if (optionals.HasFlag(HavokType.Optional.SubType))
            {
                writer.WriteHavokVarUInt(type.SubType is null ? 0 : (ulong)typeIndices[type.SubType]);
            }

            if (optionals.HasFlag(HavokType.Optional.Version))
            {
                writer.WriteHavokVarUInt(type.Version);
            }

            if (optionals.HasFlag(HavokType.Optional.SizeAlign))
            {
                writer.WriteHavokVarUInt((ulong)type.Size);
                writer.WriteHavokVarUInt((ulong)type.Alignment);
            }

            if (optionals.HasFlag(HavokType.Optional.Flags))
            {
                writer.WriteHavokVarUInt((ulong)type.Flags);
            }

            if (optionals.HasFlag(HavokType.Optional.Members))
            {
                int startIndex = type.Parent?.Fields.Count ?? 0;
                int numFields = type.Fields.Count - startIndex;

                writer.WriteHavokVarUInt((uint)(numFields & 0xffff) | ((uint)type.Properties.Count << 16));
                for (int j = startIndex; j < type.Fields.Count; j++)
                {
                    HavokType.Member field = type.Fields[j];
                    writer.WriteHavokVarUInt((ulong)fieldStringIndices[field.Name]);

                    if (Dialect.MemberExtraValues &&
                        _memberExtraValues.TryGetValue($"{type.Identity}.{field.Name}", out ulong extraValue))
                    {
                        writer.WriteHavokVarUInt((ulong)field.Flags | MemberExtraValueFlag);
                        writer.WriteHavokVarUInt(extraValue);
                    }
                    else
                    {
                        writer.WriteHavokVarUInt((ulong)field.Flags);
                    }

                    writer.WriteHavokVarUInt((ulong)field.Offset);
                    writer.WriteHavokVarUInt((ulong)typeIndices[field.Type]);
                }
            }

            if (optionals.HasFlag(HavokType.Optional.Interfaces))
            {
                int startIndex = type.Parent?.Interfaces.Count ?? 0;
                writer.WriteHavokVarUInt((ulong)(type.Interfaces.Count - startIndex));
                for (int j = startIndex; j < type.Interfaces.Count; j++)
                {
                    HavokType.Interface iface = type.Interfaces[j];
                    writer.WriteHavokVarUInt((ulong)typeIndices[iface.Type]);
                    writer.WriteHavokVarUInt((ulong)iface.Offset);
                }
            }

            if (optionals.HasFlag(HavokType.Optional.Attributes))
            {
                throw new NotImplementedException("Type attributes are not supported.");
            }
        }

        writer.Pad(4);
        writer.EndSection();
    }

    private static void WriteTHSH(TagfileWriter writer, IReadOnlyList<HavokType> topLevelTypes,
        IReadOnlyDictionary<HavokType, int> typeIndices)
    {
        writer.BeginSection("THSH");

        writer.WriteHavokVarUInt((ulong)topLevelTypes.Count);
        foreach (HavokType type in topLevelTypes)
        {
            if (type.Hash is not { } hash)
            {
                throw new ArgumentException(
                    $"The type {type.Identity} has no associated hash and cannot be serialized as a top level type.");
            }

            writer.WriteHavokVarUInt((ulong)typeIndices[type]);
            writer.WriteUInt32(hash);
        }

        writer.Pad(4);
        writer.EndSection();
    }

    #endregion

    /// <summary>
    /// A TYPE section as it was read. Some files hold a fixed set of types with bodies for only some of them, which
    /// can't be reproduced from the registry, so the section is written back while it still holds every type needed.
    /// </summary>
    private sealed class TypeSection
    {
        public TypeSection(byte[] bytes, IReadOnlyList<HavokType> types)
        {
            Bytes = bytes;
            Types = types;
        }

        public byte[] Bytes { get; }

        /// <summary>
        /// The types by their index in the section, starting with the null type.
        /// </summary>
        public IReadOnlyList<HavokType> Types { get; }

        public bool TryGetIndices(IEnumerable<HavokType> types, out IReadOnlyDictionary<HavokType, int> typeIndices)
        {
            Dictionary<HavokType, int> indices = new();
            typeIndices = indices;

            for (int i = 1; i < Types.Count; i++)
            {
                if (Types[i] is { } type) indices.TryAdd(type, i);
            }

            return types.All(indices.ContainsKey);
        }
    }
}
