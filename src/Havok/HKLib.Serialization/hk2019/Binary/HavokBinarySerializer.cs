using HKLib.hk2019;
using HKLib.Reflection.hk2019;
using HKLib.Serialization.hk2019.Binary.Util;

namespace HKLib.Serialization.hk2019.Binary;

/// <summary>
/// Reads Havok 2019 tagfiles (AC6). These are laid out like 2018 tagfiles, with these differences:
/// the type and field string sections are TST1/FST1 and padded with 0xFF, type bodies are only stored for some types,
/// and members with <see cref="MemberExtraValueFlag" /> are followed by an extra value.
/// Writing is not supported as the extra member values are not stored in the type registry.
/// </summary>
public class HavokBinarySerializer : HavokSerializer
{
    private const string SdkVersion = "20190100";

    private const int MemberExtraValueFlag = 0x80;

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
        throw new NotSupportedException("Writing Havok 2019 files is not supported.");
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
        throw new NotSupportedException("Writing Havok 2019 files is not supported.");
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

        // only used for inplace loading
        reader.EnterSection("PTCH");
        reader.SkipSection();

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

        IReadOnlyList<HavokType> types = ReadTYPE(reader);
        reader.ExitSection();

        return new HavokCompendium(ids.ToList(), types.ToList());
    }

    #endregion

    #region TYPE

    private IReadOnlyList<HavokType> ReadTYPE(HavokBinaryReader reader)
    {
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
            ReadTHSH(reader, types);
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

                    // the meaning of the extra value is unknown, it isn't needed as the type registry provides the members
                    if ((flags & MemberExtraValueFlag) != 0)
                    {
                        reader.ReadHavokVarUInt();
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
}
