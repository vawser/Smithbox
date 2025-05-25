using System.Diagnostics;
using HKLib.hk2018;
using HKLib.Reflection.hk2018;
using HKLib.Serialization.hk2018.Binary.Util;

namespace HKLib.Serialization.hk2018.Binary;

public class HavokBinarySerializer : HavokSerializer
{
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

    #region DATA

    private long ReadDATA(HavokBinaryReader reader)
    {
        // will be read later by ITEM
        reader.EnterSection("DATA");
        long offset = reader.Position;
        reader.SkipSection();
        return offset;
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

        for (int i = 1; i < context.Items.Count; i++)
        {
            IndexItem item = context.Items[i];
            item.WriteObjects(writer, context);
        }

        writer.EndSection();
        return dataOffset;
    }

    #endregion

    #region INDX

    private IReadOnlyList<IHavokObject> ReadINDX(HavokBinaryReader reader,
        IReadOnlyList<HavokType> types, long dataOffset)
    {
        reader.EnterSection("INDX");
        IReadOnlyList<IHavokObject> havokObjects = ReadITEM(reader, types, dataOffset);
        ReadPTCH(reader, types, dataOffset);
        reader.ExitSection();

        return havokObjects;
    }

    private void WriteINDX(HavokBinaryWriter writer, IReadOnlyDictionary<HavokType, int> typeIndices,
        BinarySerializeContext context, long dataOffset)
    {
        writer.BeginSection("INDX");
        WriteITEM(writer, typeIndices, context, dataOffset);
        WritePTCH(writer, typeIndices, context, dataOffset);
        writer.EndSection();
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

    private void WriteITEM(HavokBinaryWriter writer, IReadOnlyDictionary<HavokType, int> typeIndices,
        BinarySerializeContext context, long dataOffset)
    {
        writer.BeginSection("ITEM");

        // write null item
        writer.WriteInt32(0);
        writer.WriteInt32(0);
        writer.WriteInt32(0);

        for (int index = 1; index < context.Items.Count; index++)
        {
            IndexItem item = context.Items[index];
            int typeIndex = typeIndices[item.Type];
            int itemKind = (int)item.Kind;
            int typeIndexAndFlags = typeIndex | (itemKind << 28);
            writer.WriteInt32(typeIndexAndFlags);
            int offset = (int)(item.Offset - dataOffset);
            writer.WriteInt32(offset);
            writer.WriteInt32(item.Count);
        }

        writer.EndSection();
    }

    #endregion

    #region PTCH

    private void ReadPTCH(HavokBinaryReader reader, IReadOnlyList<HavokType> types, long dataOffset)
    {
        // we do not need to read this section as it is only used for inplace loading
        reader.EnterSection("PTCH");

#if DEBUG
        Dictionary<HavokType, List<uint>> patches = new();
        while (reader.Position < reader.GetSectionEnd())
        {
            int typeIndex = reader.ReadInt32();
            HavokType type = types[typeIndex];
            int count = reader.ReadInt32();
            List<uint> offsets = new(count);
            for (int i = 0; i < count; i++)
            {
                offsets.Add(reader.ReadUInt32());
            }

            patches.Add(type, offsets);
        }
#endif

        reader.SkipSection();
    }

    private void WritePTCH(HavokBinaryWriter writer, IReadOnlyDictionary<HavokType, int> typeIndices,
        BinarySerializeContext context, long dataOffset)
    {
        writer.BeginSection("PTCH");
        foreach ((HavokType type, List<long> positions) in context.Patches)
        {
            int typeIndex = typeIndices[type];
            writer.WriteInt32(typeIndex);

            int count = positions.Count;
            writer.WriteInt32(count);
            foreach (long position in positions.Order())
            {
                writer.WriteUInt32((uint)(position - dataOffset));
            }
        }

        writer.EndSection();
    }

    #endregion

    #region TCM0

    private HavokCompendium ReadTCM0(HavokBinaryReader reader)
    {
        reader.EnterSection("TCM0");
        IReadOnlyList<ulong> ids = ReadTCID(reader);
        IReadOnlyList<HavokType> types = ReadTYPE(reader);
        reader.ExitSection();

        return new HavokCompendium(ids.ToList(), types.ToList());
    }

    private void WriteTCM0(HavokBinaryWriter writer, HavokCompendium compendium)
    {
        writer.BeginSection("TCM0");
        WriteTCID(writer, compendium.Ids);
        WriteTYPE(writer, compendium.Types, compendium.TopLevelTypes);
        writer.EndSection();
    }

    #endregion

    #region TCID

    private IReadOnlyList<ulong> ReadTCID(HavokBinaryReader reader)
    {
        reader.EnterSection("TCID");
        ulong[] ids = reader.ReadUInt64s(reader.GetSectionLength() / 8);
        reader.ExitSection();

        return ids;
    }

    private void WriteTCID(HavokBinaryWriter writer, IReadOnlyList<ulong> ids)
    {
        writer.BeginSection("TCID");
        foreach (ulong id in ids)
        {
            writer.WriteUInt64(id);
        }

        writer.EndSection();
    }

    #endregion

    #region TYPE

    private IReadOnlyList<HavokType> ReadTYPE(HavokBinaryReader reader)
    {
        reader.EnterSection("TYPE");

        ReadTPTR(reader);
        IReadOnlyList<string> typeStrings = ReadTSTR(reader);
        IReadOnlyList<HavokTypeBuilder> typeBuilders = ReadTNA1(reader, typeStrings);

        // these sections could be skipped since we look up types by their identity
        IReadOnlyList<string> fieldStrings = ReadFSTR(reader);
        ReadTBDY(reader, typeBuilders, fieldStrings);

        IReadOnlyList<HavokType> types = GetTypesFromRegistry(typeBuilders);

        // only packfiles have hashes
        if (reader.GetSectionId() == "THSH")
        {
            ReadTHSH(reader, types);
        }

        ReadTPAD(reader);

        reader.ExitSection();

        return types;
    }

    private IReadOnlyDictionary<HavokType, int> WriteTYPE(HavokBinaryWriter writer,
        IReadOnlyList<HavokType> types, IReadOnlyList<HavokType> topLevelTypes)
    {
        writer.BeginSection("TYPE");

        // null type is not included in the list
        WriteTPTR(writer, types.Count + 1);
        IReadOnlyDictionary<string, int> typeStringIndices = WriteTSTR(writer, types);
        IReadOnlyDictionary<HavokType, int> typeIndices = WriteTNA1(writer, types, typeStringIndices);
        IReadOnlyDictionary<string, int> fieldStringIndices = WriteFSTR(writer, types);
        WriteTBDY(writer, types, typeIndices, fieldStringIndices);
        WriteTHSH(writer, topLevelTypes, typeIndices);
        WriteTPAD(writer);
        writer.EndSection();

        return typeIndices;
    }

    #region TPTR

    // TPTR contains extra space for pointers for inplace loading
    private void ReadTPTR(HavokBinaryReader reader)
    {
        reader.EnterSection("TPTR");
        // the contents are just 0
        reader.ExitSection();
    }

    private void WriteTPTR(HavokBinaryWriter writer, int numTypes)
    {
        writer.BeginSection("TPTR");
        ulong[] pointers = new ulong[numTypes];
        writer.WriteUInt64s(pointers);
        writer.EndSection();
    }

    #endregion

    #region TSTR

    private IReadOnlyList<string> ReadTSTR(HavokBinaryReader reader)
    {
        reader.EnterSection("TSTR");

        List<string> strings = new();
        while (reader.Position < reader.GetSectionEnd())
        {
            strings.Add(reader.ReadASCII());
        }

        reader.ExitSection();
        return strings;
    }

    private IReadOnlyDictionary<string, int> WriteTSTR(HavokBinaryWriter writer,
        IReadOnlyList<HavokType> types)
    {
        writer.BeginSection("TSTR");

        Dictionary<string, int> typeStringIndices = new();

        int typeStringIndex = 0;
        foreach (HavokType type in types)
        {
            string typeName = type.Name;
            if (!typeStringIndices.ContainsKey(typeName))
            {
                writer.WriteASCII(typeName, true);
                typeStringIndices.Add(typeName, typeStringIndex);
                typeStringIndex++;
            }

            foreach (HavokType.TemplateParameter param in type.TemplateParameters)
            {
                char prefix = param.Type is null ? 'v' : 't';
                string paramName = prefix + param.Name;
                if (!typeStringIndices.ContainsKey(param.Name))
                {
                    writer.WriteASCII(paramName, true);

                    // Name is used without prefix when writing TNA1
                    typeStringIndices.Add(param.Name, typeStringIndex);
                    typeStringIndex++;
                }
            }
        }

        writer.Pad(4);
        writer.EndSection();

        return typeStringIndices;
    }

    #endregion

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

        reader.ExitSection();

        return typeBuilders;
    }

    private IReadOnlyDictionary<HavokType, int> WriteTNA1(HavokBinaryWriter writer,
        IReadOnlyList<HavokType> types,
        IReadOnlyDictionary<string, int> typeStringIndices)
    {
        writer.BeginSection("TNA1");

        Dictionary<HavokType, int> typeIndices = new();
        for (int i = 0; i < types.Count; i++) typeIndices.Add(types[i], i + 1);

        // count includes null type which is not serialized
        writer.WriteHavokVarUInt((ulong)types.Count + 1);

        foreach (HavokType type in types)
        {
            int nameIndex = typeStringIndices[type.Name];
            writer.WriteHavokVarUInt((ulong)nameIndex);
            writer.WriteHavokVarUInt((ulong)type.TemplateParameters.Count);
            foreach (HavokType.TemplateParameter param in type.TemplateParameters)
            {
                int paramNameIndex = typeStringIndices[param.Name];
                writer.WriteHavokVarUInt((ulong)paramNameIndex);
                if (param.Value != null)
                {
                    writer.WriteHavokVarUInt((ulong)param.Value);
                    continue;
                }

                int typeIndex = typeIndices[param.Type!];
                writer.WriteHavokVarUInt((ulong)typeIndex);
            }
        }

        writer.Pad(4);
        writer.EndSection();

        return typeIndices;
    }

    #endregion

    #region FSTR

    private IReadOnlyList<string> ReadFSTR(HavokBinaryReader reader)
    {
        reader.EnterSection("FSTR");

        List<string> strings = new();

        while (reader.Position < reader.GetSectionEnd())
        {
            strings.Add(reader.ReadASCII());
        }

        reader.ExitSection();
        return strings;
    }

    private IReadOnlyDictionary<string, int> WriteFSTR(HavokBinaryWriter writer,
        IReadOnlyList<HavokType> types)
    {
        writer.BeginSection("FSTR");

        Dictionary<string, int> fieldStringIndices = new();

        int fieldStringIndex = 0;
        foreach (HavokType type in types)
        {
            foreach (HavokType.Member member in type.Fields)
            {
                if (fieldStringIndices.ContainsKey(member.Name)) continue;
                writer.WriteASCII(member.Name, true);
                fieldStringIndices.Add(member.Name, fieldStringIndex);
                fieldStringIndex++;
            }
        }

        writer.Pad(4);
        writer.EndSection();

        return fieldStringIndices;
    }

    #endregion

    #region TBDY

    private void ReadTBDY(HavokBinaryReader reader, IReadOnlyList<HavokTypeBuilder> builders,
        IReadOnlyList<string> fieldStrings)
    {
        reader.EnterSection("TBDY");

        for (int i = 1; i < builders.Count; i++)
        {
            int typeIndex = (int)reader.ReadHavokVarUInt();
            Debug.Assert(typeIndex != 0);

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
                    Reflection.HavokType.Member.MemberFlags flags =
                        (Reflection.HavokType.Member.MemberFlags)reader.ReadHavokVarUInt();
                    int offset = (int)reader.ReadHavokVarUInt();
                    int memberTypeIndex = (int)reader.ReadHavokVarUInt();
                    builder.WithField(name, flags, offset, builders[memberTypeIndex]);
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

    private void WriteTBDY(HavokBinaryWriter writer, IReadOnlyList<HavokType> types,
        IReadOnlyDictionary<HavokType, int> typeIndices,
        IReadOnlyDictionary<string, int> fieldStringIndices)
    {
        writer.BeginSection("TBDY");
        foreach (HavokType type in types)
        {
            int typeIndex = typeIndices[type];
            writer.WriteHavokVarUInt((ulong)typeIndex);
            int parentIndex = type.Parent is null ? 0 : typeIndices[type.Parent];
            writer.WriteHavokVarUInt((ulong)parentIndex);

            HavokType.Optional optionals = type.Optionals;
            writer.WriteHavokVarUInt((ulong)optionals);
            if (optionals.HasFlag(HavokType.Optional.Format))
            {
                writer.WriteHavokVarUInt((ulong)type.Format);
            }

            if (optionals.HasFlag(HavokType.Optional.SubType))
            {
                if (type.SubType is null)
                {
                    writer.WriteHavokVarUInt(0);
                }
                else
                {
                    writer.WriteHavokVarUInt((ulong)typeIndices[type.SubType]);
                }
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
                int numFields = type.Fields.Count;
                int startIndex = 0;
                if (type.Parent is not null)
                {
                    int parentFieldCount = type.Parent.Fields.Count;
                    startIndex = parentFieldCount;
                    numFields -= parentFieldCount;
                }

                uint numFieldsAndProperties = (uint)(numFields & 0xffff) | ((uint)type.Properties.Count << 16);
                writer.WriteHavokVarUInt(numFieldsAndProperties);
                for (int j = startIndex; j < type.Fields.Count; j++)
                {
                    HavokType.Member field = type.Fields[j];
                    int nameIndex = fieldStringIndices[field.Name];
                    writer.WriteHavokVarUInt((ulong)nameIndex);
                    writer.WriteHavokVarUInt((ulong)field.Flags);
                    writer.WriteHavokVarUInt((ulong)field.Offset);
                    int fieldTypeIndex = typeIndices[field.Type];
                    writer.WriteHavokVarUInt((ulong)fieldTypeIndex);
                }
            }

            if (optionals.HasFlag(HavokType.Optional.Interfaces))
            {
                int numInterfaces = type.Interfaces.Count;
                int startIndex = 0;
                if (type.Parent is not null)
                {
                    int parentInterfaceCount = type.Parent.Interfaces.Count;
                    startIndex = parentInterfaceCount;
                    numInterfaces -= parentInterfaceCount;
                }

                writer.WriteHavokVarUInt((ulong)numInterfaces);

                for (int j = startIndex; j < type.Interfaces.Count; j++)
                {
                    HavokType.Interface iface = type.Interfaces[j];
                    int interfaceTypeIndex = typeIndices[iface.Type];
                    writer.WriteHavokVarUInt((ulong)interfaceTypeIndex);
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

    #endregion

    #region THSH

    private void ReadTHSH(HavokBinaryReader reader, IReadOnlyList<HavokType> types)
    {
        reader.EnterSection("THSH");
        int numHashes = (int)reader.ReadHavokVarUInt();
        for (int i = 0; i < numHashes; i++)
        {
            int typeIndex = (int)reader.ReadHavokVarUInt();

            if (types[typeIndex].Hash is not { } hash)
            {
                throw new InvalidDataException($"Unable to verify hash for type {types[typeIndex].Identity}.");
            }

            if (reader.ReadUInt32() != hash)
            {
                throw new InvalidDataException($"Incorrect hash encountered for type {types[typeIndex].Identity}.");
            }
        }

        reader.ExitSection();
    }

    private void WriteTHSH(HavokBinaryWriter writer, IReadOnlyList<HavokType> topLevelTypes,
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

            writer.WriteHavokVarUInt((ulong)(typeIndices[type]));
            writer.WriteUInt32(hash);
        }

        writer.Pad(4);
        writer.EndSection();
    }

    #endregion

    #region TPAD

    private void ReadTPAD(HavokBinaryReader reader)
    {
        reader.EnterSection("TPAD");
        reader.ExitSection();
    }

    private void WriteTPAD(HavokBinaryWriter writer)
    {
        writer.BeginSection("TPAD");
        writer.Pad(4);
        writer.EndSection();
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
        switch (reader.GetSectionId())
        {
            case "TYPE":
                types = ReadTYPE(reader);
                break;
            case "TCRF":
                ulong compendiumId = ReadTCRF(reader);
                types = GetTypesFromCompendium(compendiumId);
                break;
            default:
                throw new InvalidDataException("No TYPE section or compendium reference found");
        }

        IReadOnlyList<IHavokObject> havokObjects = ReadINDX(reader, types, dataOffset);
        reader.ExitSection();
        return havokObjects;
    }

    private void WriteTAG0(HavokBinaryWriter writer, IHavokObject rootLevelObject)
    {
        writer.BeginSection("TAG0");
        WriteSDKV(writer);
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

        IReadOnlyDictionary<HavokType, int> typeIndices =
            WriteTYPE(writer, context.Types, topLevelTypes.ToList());
        WriteINDX(writer, typeIndices, context, dataOffset);
        writer.EndSection();
    }

    #endregion

    #region SDKV

    private void ReadSDKV(HavokBinaryReader reader)
    {
        reader.EnterSection("SDKV");
        if (reader.ReadASCII(8) != "20180100")
        {
            throw new InvalidDataException("Unsupported SDK Version.");
        }

        reader.ExitSection();
    }

    private void WriteSDKV(HavokBinaryWriter writer)
    {
        writer.BeginSection("SDKV");
        writer.WriteASCII("20180100");
        writer.EndSection();
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

    private void WriteTCRF(HavokBinaryWriter writer, ulong id)
    {
        writer.BeginSection("TCRF");
        writer.WriteUInt64(id);
        writer.WriteUInt64s(new ulong[2]);
        writer.EndSection();
    }

    #endregion
}