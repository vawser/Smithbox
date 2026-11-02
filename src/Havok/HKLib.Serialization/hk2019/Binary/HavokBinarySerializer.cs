using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using HKLib.hk2019;
using HKLib.Reflection.hk2019;
using HKLib.Serialization.hk2019.Binary.Util;

namespace HKLib.Serialization.hk2019.Binary;

/// <summary>
/// Reads and writes Havok 2019 tagfiles (AC6). These are laid out like 2018 tagfiles, with these differences:
/// the type and field string sections are TST1/FST1 and padded with 0xFF, type bodies are only stored for some types,
/// and members with <see cref="MemberExtraValueFlag" /> are followed by an extra value.
/// When a compendium is loaded and holds every type an object needs, the written file refers to it rather than
/// holding its own types, as AC6 map collision does.
/// </summary>
public class HavokBinarySerializer : HavokSerializer
{
    private const string SdkVersion = "20190100";

    private const int MemberExtraValueFlag = 0x80;

    /// <summary>
    /// The extra values of flagged members, keyed by type and member name. The type registry doesn't store them, so
    /// they are the values found in the AC6 files, which are the same in every file. Values read from other files are
    /// added as they are found.
    /// </summary>
    private static readonly ConcurrentDictionary<string, ulong> MemberExtraValues = new()
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
    };

    /// <summary>
    /// A compendium can have several ids, and files may refer to any of them, so the id a root object was read with
    /// is kept to write it back with.
    /// </summary>
    private static readonly ConditionalWeakTable<IHavokObject, StrongBox<ulong>> CompendiumIdsRead = new();

    /// <summary>
    /// The TYPE section a root object was read with, written back while it holds every type that is needed.
    /// </summary>
    private static readonly ConditionalWeakTable<IHavokObject, TypeSection> TypeSectionsRead = new();

    private HavokCompendium? _compendium;

    public HavokBinarySerializer() : this(HavokTypeRegistry.Instance) { }

    public HavokBinarySerializer(HavokTypeRegistry typeRegistry) : base(typeRegistry) { }

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

    private IReadOnlyList<HavokType> GetTypesFromRegistry(
        IReadOnlyList<HavokTypeBuilder> typeBuilders)
    {
        HavokType[] types = new HavokType[typeBuilders.Count];

        // ignore first type as it is the null type
        for (int i = 1; i < typeBuilders.Count; i++)
        {
            string typeIdentity = typeBuilders[i].Build().Identity;
            types[i] = TypeRegistry.GetType(typeIdentity) ??
                       throw new KeyNotFoundException(
                           $"The type \"{typeIdentity}\" was not found in the type registry.");
        }

        return types;
    }

    public override void LoadCompendium(Stream stream)
    {
        _compendium = ReadCompendium(stream);
    }

    public override void LoadCompendium(HavokCompendium compendium)
    {
        _compendium = compendium;
    }

    public override HavokCompendium ReadCompendium(Stream stream)
    {
        HavokBinaryReader reader = new(stream);
        HavokCompendium havokCompendium = ReadTCM0(reader);
        reader.Close();

        return havokCompendium;
    }

    public override void Write(HavokCompendium compendium, Stream stream)
    {
        HavokBinaryWriter writer = new(stream);
        WriteTCM0(writer, compendium);
        writer.Finish();
    }

    public override IEnumerable<IHavokObject> ReadAllObjects(Stream stream)
    {
        HavokBinaryReader reader = new(stream);
        IReadOnlyList<IHavokObject> havokObjects = ReadTAG0(reader);
        reader.Close();
        return havokObjects.Distinct();
    }

    public override void Write(IHavokObject havokObject, Stream stream)
    {
        HavokBinaryWriter writer = new(stream);
        WriteTAG0(writer, havokObject);
        writer.Finish();
    }

    /// <summary>
    /// Gets the indices of the types in the loaded compendium, if it holds all of them.
    /// </summary>
    private bool TryGetCompendiumIndices(IEnumerable<HavokType> types,
        out IReadOnlyDictionary<HavokType, int> typeIndices)
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

    /// <summary>
    /// String sections are padded with 0xFF.
    /// </summary>
    private static void PadStrings(HavokBinaryWriter writer)
    {
        while (writer.Position % 4 != 0)
        {
            writer.WriteByte(0xFF);
        }
    }

    #region DATA

    private long ReadDATA(HavokBinaryReader reader)
    {
        // will be read later by ITEM
        reader.EnterSection("DATA");
        long offset = reader.Position;
        reader.SkipSection();
        return offset;
    }

    #endregion

    #region INDX

    private IReadOnlyList<IHavokObject> ReadINDX(HavokBinaryReader reader,
        IReadOnlyList<HavokType> types, long dataOffset)
    {
        reader.EnterSection("INDX");
        IReadOnlyList<IHavokObject> havokObjects = ReadITEM(reader, types, dataOffset);

        // only used for inplace loading, and left out when there are no pointers
        if (reader.Position < reader.GetSectionEnd())
        {
            reader.EnterSection("PTCH");
            reader.SkipSection();
        }

        reader.ExitSection();

        return havokObjects;
    }

    #endregion

    #region ITEM

    private IReadOnlyList<IHavokObject> ReadITEM(HavokBinaryReader reader,
        IReadOnlyList<HavokType> types, long dataOffset)
    {
        BinaryDeserializeContext context = new(TypeRegistry);

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

        List<IHavokObject> havokObjects = new();
        for (int i = 1; i < context.Items.Count; i++)
        {
            object[] objects = context.GetItem((ulong)i, reader);
            foreach (object obj in objects)
            {
                if (obj is IHavokObject havokObject)
                {
                    havokObjects.Add(havokObject);
                }
            }
        }

        return havokObjects;
    }

    #endregion

    #region TCM0

    private HavokCompendium ReadTCM0(HavokBinaryReader reader)
    {
        reader.EnterSection("TCM0");

        reader.EnterSection("TCID");
        ulong[] ids = reader.ReadUInt64s(reader.GetSectionLength() / 8);
        reader.ExitSection();

        IReadOnlyList<HavokType> types = ReadTYPE(reader, out List<HavokType> hashedTypes, out TypeSection section);
        reader.ExitSection();

        HavokCompendium compendium = new(ids.ToList(), types.ToList()) { TypeSection = section };
        compendium.TopLevelTypes.AddRange(hashedTypes);
        return compendium;
    }

    #endregion

    #region TYPE

    /// <param name="hashedTypes">The types whose hash is stored, which are the top level types when writing</param>
    /// <param name="section">The section as it was read, to write it back</param>
    private IReadOnlyList<HavokType> ReadTYPE(HavokBinaryReader reader, out List<HavokType> hashedTypes,
        out TypeSection section)
    {
        long start = reader.Position;
        IReadOnlyList<HavokType> types = ReadTYPE(reader, out hashedTypes);
        section = new TypeSection(reader.GetBytes(start, (int)(reader.Position - start)), types);
        return types;
    }

    private IReadOnlyList<HavokType> ReadTYPE(HavokBinaryReader reader, out List<HavokType> hashedTypes)
    {
        hashedTypes = new List<HavokType>();
        reader.EnterSection("TYPE");

        // TPTR contains extra space for pointers for inplace loading, the contents are just 0
        reader.EnterSection("TPTR");
        reader.ExitSection();

        IReadOnlyList<string> typeStrings = ReadStrings(reader, "TST1");
        IReadOnlyList<HavokTypeBuilder> typeBuilders = ReadTNA1(reader, typeStrings);

        // these sections could be skipped since we look up types by their identity
        IReadOnlyList<string> fieldStrings = ReadStrings(reader, "FST1");
        ReadTBDY(reader, typeBuilders, fieldStrings);

        IReadOnlyList<HavokType> types = GetTypesFromRegistry(typeBuilders);

        // only packfiles have hashes
        if (reader.GetSectionId() == "THSH")
        {
            ReadTHSH(reader, types, hashedTypes);
        }

        reader.EnterSection("TPAD");
        reader.ExitSection();

        reader.ExitSection();

        return types;
    }

    /// <summary>
    /// Reads a section of null terminated strings, which is padded with 0xFF.
    /// </summary>
    private static IReadOnlyList<string> ReadStrings(HavokBinaryReader reader, string sectionId)
    {
        reader.EnterSection(sectionId);

        List<string> strings = new();
        while (reader.Position < reader.GetSectionEnd() && reader.GetByte(reader.Position) != 0xFF)
        {
            strings.Add(reader.ReadASCII());
        }

        reader.SkipSection();
        return strings;
    }

    #region TNA1

    private IReadOnlyList<HavokTypeBuilder> ReadTNA1(HavokBinaryReader reader, IReadOnlyList<string> typeStrings)
    {
        reader.EnterSection("TNA1");
        int numTypes = (int)reader.ReadHavokVarUInt();

        // Type Id 0 is always the null type which is not serialized so we start reading types at 1
        HavokTypeBuilder[] typeBuilders = new HavokTypeBuilder[numTypes];
        typeBuilders[0] = new HavokTypeBuilder().WithName("NULL").WithFlags(HavokType.TypeFlags.NonSerializable);
        for (int i = 1; i < numTypes; i++) typeBuilders[i] = new HavokTypeBuilder();
        for (int i = 1; i < numTypes; i++)
        {
            HavokTypeBuilder builder = typeBuilders[i];
            int nameIndex = (int)reader.ReadHavokVarUInt();
            builder.WithName(typeStrings[nameIndex]);
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

        reader.SkipSection();

        return typeBuilders;
    }

    #endregion

    #region TBDY

    private void ReadTBDY(HavokBinaryReader reader, IReadOnlyList<HavokTypeBuilder> builders,
        IReadOnlyList<string> fieldStrings)
    {
        reader.EnterSection("TBDY");

        // not every type has a body, so read until the end of the section
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

            HavokType.Optional optionals =
                (HavokType.Optional)reader.ReadHavokVarUInt();
            builder.WithOptionals(optionals);
            if (optionals.HasFlag(HavokType.Optional.Format))
            {
                builder.WithFormat((int)reader.ReadHavokVarUInt());
            }

            if (optionals.HasFlag(HavokType.Optional.SubType))
            {
                int subTypeIndex = (int)reader.ReadHavokVarUInt();
                builder.WithSubType(builders[subTypeIndex]);
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
                // the two high bytes denote the number of properties
                uint numMembers = (uint)reader.ReadHavokVarUInt();
                int numProperties = (int)(numMembers >> 16);
                builder.WithProperties(new HavokType.Member?[numProperties].ToList());

                int numFields = (int)(numMembers & 0xffff);
                for (int j = 0; j < numFields; j++)
                {
                    int nameIndex = (int)reader.ReadHavokVarUInt();
                    string name = fieldStrings[nameIndex];
                    int flags = (int)reader.ReadHavokVarUInt();

                    // the meaning of the extra value is unknown, it is kept so it can be written back
                    if ((flags & MemberExtraValueFlag) != 0)
                    {
                        MemberExtraValues.TryAdd($"{builder.Name}.{name}", reader.ReadHavokVarUInt());
                        flags &= ~MemberExtraValueFlag;
                    }

                    int offset = (int)reader.ReadHavokVarUInt();
                    int memberTypeIndex = (int)reader.ReadHavokVarUInt();
                    builder.WithField(name, (Reflection.HavokType.Member.MemberFlags)flags, offset,
                        builders[memberTypeIndex]);
                }
            }

            if (optionals.HasFlag(HavokType.Optional.Interfaces))
            {
                int numInterfaces = (int)reader.ReadHavokVarUInt();
                for (int j = 0; j < numInterfaces; j++)
                {
                    int memberTypeIndex = (int)reader.ReadHavokVarUInt();
                    int offset = (int)reader.ReadHavokVarUInt();
                    builder.WithInterface(builders[memberTypeIndex], offset);
                }
            }

            if (optionals.HasFlag(HavokType.Optional.Attributes))
            {
                throw new NotImplementedException();
            }
        }

        reader.ExitSection();
    }

    #endregion

    #region THSH

    private void ReadTHSH(HavokBinaryReader reader, IReadOnlyList<HavokType> types, List<HavokType> hashedTypes)
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

    #endregion

    #region TAG0

    private IReadOnlyList<IHavokObject> ReadTAG0(HavokBinaryReader reader)
    {
        reader.EnterSection("TAG0");
        ReadSDKV(reader);
        long dataOffset = ReadDATA(reader);
        IReadOnlyList<HavokType> types;
        ulong? compendiumId = null;
        TypeSection? typeSection = null;
        switch (reader.GetSectionId())
        {
            case "TYPE":
                types = ReadTYPE(reader, out _, out typeSection);
                break;
            case "TCRF":
                compendiumId = ReadTCRF(reader);
                types = GetTypesFromCompendium(compendiumId.Value);
                break;
            default:
                throw new InvalidDataException("No TYPE section or compendium reference found");
        }

        IReadOnlyList<IHavokObject> havokObjects = ReadINDX(reader, types, dataOffset);
        reader.ExitSection();

        if (havokObjects.FirstOrDefault() is { } rootLevelObject)
        {
            if (compendiumId is { } id) CompendiumIdsRead.AddOrUpdate(rootLevelObject, new StrongBox<ulong>(id));
            if (typeSection is not null) TypeSectionsRead.AddOrUpdate(rootLevelObject, typeSection);
        }

        return havokObjects;
    }

    #endregion

    #region SDKV

    private void ReadSDKV(HavokBinaryReader reader)
    {
        reader.EnterSection("SDKV");
        string sdkVersion = reader.ReadASCII(8);
        if (sdkVersion != SdkVersion)
        {
            throw new InvalidDataException(
                $"Unsupported SDK Version. Found: {sdkVersion} | Expected: {SdkVersion}");
        }

        reader.ExitSection();
    }

    #endregion

    #region TCRF

    private ulong ReadTCRF(HavokBinaryReader reader)
    {
        reader.EnterSection("TCRF");
        ulong id = reader.ReadUInt64();
        reader.AssertUInt64(0);
        reader.AssertUInt64(0);
        reader.ExitSection();
        return id;
    }

    #endregion

    #region Writing

    private void WriteTAG0(HavokBinaryWriter writer, IHavokObject rootLevelObject)
    {
        writer.BeginSection("TAG0");

        writer.BeginSection("SDKV");
        writer.WriteASCII(SdkVersion);
        writer.EndSection();

        BinarySerializeContext context = new(TypeRegistry);
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

        // The types come from the loaded compendium if it has them all, then from the types the object was read with,
        // and are only written out from the type registry when neither has them
        if (!TryGetCompendiumIndices(topLevelTypes, out IReadOnlyDictionary<HavokType, int> typeIndices))
        {
            if (TypeSectionsRead.TryGetValue(rootLevelObject, out TypeSection? typeSection) &&
                typeSection.TryGetIndices(topLevelTypes, out typeIndices))
            {
                writer.WriteBytes(typeSection.Bytes);
            }
            else
            {
                typeIndices = WriteTYPE(writer, context.Types, topLevelTypes.ToList());
            }
        }
        else
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

        writer.BeginSection("INDX");
        WriteITEM(writer, typeIndices, context, dataOffset);
        WritePTCH(writer, typeIndices, context, dataOffset);
        writer.EndSection();

        writer.EndSection();
    }

    private long WriteDATA(HavokBinaryWriter writer, IHavokObject rootLevelObject, BinarySerializeContext context)
    {
        writer.BeginSection("DATA");
        long dataOffset = writer.Position;

        HavokType objectType = TypeRegistry.GetType(rootLevelObject.GetType()) ??
                               throw new ArgumentException(
                                   "No type data found for the provided object",
                                   nameof(rootLevelObject));
        context.Enqueue(objectType, dataOffset, rootLevelObject);

        // Objects which are pointed to are written in the order they are found. Each is followed by the arrays and
        // strings it owns, breadth first, as the 2019 SDK does.
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

        writer.Pad(16);
        writer.EndSection();
        return dataOffset;
    }

    /// <summary>
    /// Writes an item and sorts the items it refers to into pointed to objects and owned arrays.
    /// </summary>
    private static void WriteItem(HavokBinaryWriter writer, BinarySerializeContext context, int index,
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

    private void WriteITEM(HavokBinaryWriter writer, IReadOnlyDictionary<HavokType, int> typeIndices,
        BinarySerializeContext context, long dataOffset)
    {
        writer.BeginSection("ITEM");

        // null item
        writer.WriteInt32(0);
        writer.WriteInt32(0);
        writer.WriteInt32(0);

        for (int index = 1; index < context.Items.Count; index++)
        {
            IndexItem item = context.Items[index];
            int typeIndex = typeIndices[item.Type];
            int typeIndexAndFlags = typeIndex | ((int)item.Kind << 28);
            writer.WriteInt32(typeIndexAndFlags);
            writer.WriteInt32((int)(item.Offset - dataOffset));
            writer.WriteInt32(item.Count);
        }

        writer.EndSection();
    }

    private void WritePTCH(HavokBinaryWriter writer, IReadOnlyDictionary<HavokType, int> typeIndices,
        BinarySerializeContext context, long dataOffset)
    {
        // left out when there are no pointers
        if (context.Patches.Count == 0) return;

        writer.BeginSection("PTCH");

        // grouped by type in the order of the type indices
        foreach ((HavokType type, List<long> positions) in context.Patches.OrderBy(x => typeIndices[x.Key]))
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

    private void WriteTCM0(HavokBinaryWriter writer, HavokCompendium compendium)
    {
        writer.BeginSection("TCM0");

        writer.BeginSection("TCID");
        foreach (ulong id in compendium.Ids)
        {
            writer.WriteUInt64(id);
        }

        writer.EndSection();

        if (compendium.TypeSection is { } typeSection && typeSection.Types.SequenceEqual(compendium.Types))
        {
            writer.WriteBytes(typeSection.Bytes);
        }
        else
        {
            // the type list of a compendium which was read starts with the null type. Every type the types refer to
            // is written too, as types written out from the registry describe all of their members.
            List<HavokType> types = compendium.Types.Where(x => x is not null).ToList();
            BinarySerializeContext context = new(TypeRegistry);
            foreach (HavokType type in types) context.GetTypesForWrite(type);
            types.AddRange(context.Types.Except(types));

            WriteTYPE(writer, types, compendium.TopLevelTypes);
        }

        writer.EndSection();
    }

    private IReadOnlyDictionary<HavokType, int> WriteTYPE(HavokBinaryWriter writer,
        IReadOnlyList<HavokType> types, IReadOnlyList<HavokType> topLevelTypes)
    {
        writer.BeginSection("TYPE");

        // extra space for pointers for inplace loading, null type is not included in the list
        writer.BeginSection("TPTR");
        writer.WriteUInt64s(new ulong[types.Count + 1]);
        writer.EndSection();

        IReadOnlyDictionary<string, int> typeStringIndices = WriteTST1(writer, types);
        IReadOnlyDictionary<HavokType, int> typeIndices = WriteTNA1(writer, types, typeStringIndices);
        IReadOnlyDictionary<string, int> fieldStringIndices = WriteFST1(writer, types);
        WriteTBDY(writer, types, typeIndices, fieldStringIndices);
        WriteTHSH(writer, topLevelTypes, typeIndices);

        writer.BeginSection("TPAD");
        writer.Pad(4);
        writer.EndSection();

        writer.EndSection();

        return typeIndices;
    }

    private static IReadOnlyDictionary<string, int> WriteTST1(HavokBinaryWriter writer,
        IReadOnlyList<HavokType> types)
    {
        writer.BeginSection("TST1");

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

                // the prefix denotes a type or value parameter, the name is used without it when writing TNA1
                char prefix = param.Type is null ? 'v' : 't';
                writer.WriteASCII(prefix + param.Name, true);
                typeStringIndices.Add(param.Name, typeStringIndices.Count);
            }
        }

        PadStrings(writer);
        writer.EndSection();

        return typeStringIndices;
    }

    private static IReadOnlyDictionary<HavokType, int> WriteTNA1(HavokBinaryWriter writer,
        IReadOnlyList<HavokType> types, IReadOnlyDictionary<string, int> typeStringIndices)
    {
        writer.BeginSection("TNA1");

        Dictionary<HavokType, int> typeIndices = new();
        for (int i = 0; i < types.Count; i++) typeIndices.Add(types[i], i + 1);

        // count includes null type which is not serialized
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

    private static IReadOnlyDictionary<string, int> WriteFST1(HavokBinaryWriter writer,
        IReadOnlyList<HavokType> types)
    {
        writer.BeginSection("FST1");

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

        PadStrings(writer);
        writer.EndSection();

        return fieldStringIndices;
    }

    private static void WriteTBDY(HavokBinaryWriter writer, IReadOnlyList<HavokType> types,
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

                    if (MemberExtraValues.TryGetValue($"{type.Identity}.{field.Name}", out ulong extraValue))
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
                throw new NotImplementedException();
            }
        }

        writer.Pad(4);
        writer.EndSection();
    }

    private static void WriteTHSH(HavokBinaryWriter writer, IReadOnlyList<HavokType> topLevelTypes,
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
}
