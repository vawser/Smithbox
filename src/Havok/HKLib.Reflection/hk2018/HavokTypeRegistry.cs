using System.Xml.Linq;

namespace HKLib.Reflection.hk2018;

public class HavokTypeRegistry
{
    private static readonly Dictionary<int, int> OptionalBitMap = new()
    {
        { 0, 0 },
        { 1, 1 },
        { 4, 2 },
        { 23, 3 },
        { 24, 4 },
        { 26, 5 },
        { 17, 6 },
        { 28, 7 }
    };

    private readonly Dictionary<string, HavokType> _typesFromIdentity = new();
    private readonly Dictionary<Type, HavokType> _typesFromType = new();

    public static readonly HavokTypeRegistry Instance = LoadDefault();

    private static HavokTypeRegistry LoadDefault() 
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string typeRegPath = Path.Join(basePath, "Res", "HavokTypeRegistry20180100.xml");
        return Load(typeRegPath);
    }

    public HavokTypeRegistry(IReadOnlyList<HavokType> types)
    {
        foreach (HavokType type in types)
        {
            // some types reference duplicates of other types, we only add one copy to the registry
            if (type is null || _typesFromIdentity.ContainsKey(type.Identity)) continue;

            _typesFromIdentity.Add(type.Identity, type);

            if (type is { Kind: HavokType.TypeKind.Record, Type: { } })
            {
                _typesFromType.TryAdd(type.Type, type);
            }
        }
    }

    public IEnumerable<HavokType> Types => _typesFromIdentity.Values;

    public HavokType? GetType(string identity)
    {
        _typesFromIdentity.TryGetValue(identity, out HavokType? type);
        return type;
    }

    public HavokType? GetType(Type actualType)
    {
        _typesFromType.TryGetValue(actualType, out HavokType? type);
        return type;
    }

    public static HavokTypeRegistry Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new ArgumentException("The specified file does not exist", nameof(path));
        }

        XElement typeRegistry = XElement.Load(path);
        HavokType[] types = Load(typeRegistry);
        return new HavokTypeRegistry(types);
    }

    private static HavokType[] Load(XElement typeRegistry)
    {
        List<XElement> types = typeRegistry.Elements().SelectMany(x => x.Elements()).ToList();
        HavokTypeBuilder[] builders = types.OrderBy(x => int.Parse(x.Attribute("Id")!.Value))
            .Select(_ => new HavokTypeBuilder()).Prepend(null!).ToArray();


        for (int i = 0; i < types.Count; i++)
        {
            XElement type = types[i];
            int id = int.Parse(type.Attribute("Id")!.Value);
            HavokTypeBuilder builder = builders[id];

            builder.WithName(type.Attribute("Name")!.Value)
                .WithFlags((HavokType.TypeFlags)int.Parse(type.Attribute("Flags")!.Value))
                .WithFormat(int.Parse(type.Attribute("Format")!.Value))
                .WithSizeAlignment(int.Parse(type.Attribute("Size")!.Value),
                    int.Parse(type.Attribute("Alignment")!.Value));

            ulong version = ulong.Parse(type.Attribute("Version")!.Value);
            if (version != 0)
            {
                builder.WithVersion(version);
            }

            if (type.Attribute("Hash")?.Value is { } hash)
            {
                builder.WithHash(uint.Parse(hash));
            }

            // convert from runtime optional to serialized optional
            int optional = int.Parse(type.Attribute("Optionals")!.Value);
            builder.WithOptionals(ConvertOptional(optional));

            string parentIndex = type.Attribute("Parent")!.Value;
            if (parentIndex != "0")
            {
                HavokTypeBuilder parentBuilder = builders[int.Parse(parentIndex)];
                builder.WithParent(parentBuilder);
            }

            string subTypeIndex = type.Attribute("SubType")!.Value;
            if (subTypeIndex != "0")
            {
                HavokTypeBuilder subTypeBuilder = builders[int.Parse(subTypeIndex)];
                builder.WithSubType(subTypeBuilder);
            }

            if (type.Element("Template") is { } template)
            {
                foreach (XElement param in template.Elements())
                {
                    string name = param.Attribute("Name")!.Value;
                    int value = int.Parse(param.Attribute("Value")!.Value);
                    switch (param.Attribute("Kind")!.Value)
                    {
                        case "Type":
                            HavokTypeBuilder templateTypeBuilder = builders[value];
                            builder.WithTemplateParameter(name, templateTypeBuilder);
                            break;
                        case "Value":
                            builder.WithTemplateParameter(name, value);
                            break;
                        default:
                            throw new InvalidDataException(
                                $"Invalid template parameter kind \"{param.Attribute("Kind")!.Value}\" encountered.");
                    }
                }
            }

            if (type.Element("Members") is { } members)
            {
                foreach (XElement member in members.Elements())
                {
                    string name = member.Attribute("Name")!.Value;
                    int offset = int.Parse(member.Attribute("Offset")!.Value);
                    int typeIndex = int.Parse(member.Attribute("Type")!.Value);
                    HavokTypeBuilder memberTypeBuilder = builders[typeIndex];
                    Reflection.HavokType.Member.MemberFlags memberFlags = (Reflection.HavokType.Member.MemberFlags)
                        int.Parse(member.Attribute("Flags")!.Value);
                    if (memberFlags.HasFlag(Reflection.HavokType.Member.MemberFlags.Property))
                    {
                        builder.WithProperty(name, memberFlags, offset, memberTypeBuilder);
                    }
                    else
                    {
                        builder.WithField(name, memberFlags, offset, memberTypeBuilder);
                    }
                }
            }

            if (type.Element("Interfaces") is { } interfaces)
            {
                foreach (XElement iface in interfaces.Elements())
                {
                    int offset = int.Parse(iface.Attribute("Offset")!.Value);
                    int typeIndex = int.Parse(iface.Attribute("Type")!.Value);
                    HavokTypeBuilder interfaceTypeBuilder = builders[typeIndex];
                    builder.WithInterface(interfaceTypeBuilder, offset);
                }
            }

            if (type.Element("Presets") is { } presets)
            {
                foreach (XElement preset in presets.Elements())
                {
                    builder.WithPreset(preset.Attribute("Name")!.Value);
                }
            }

            if (type.Element("Type") is not { } typeDescription) continue;
            Type csType = Type.GetType(typeDescription.Attribute("Name")!.Value) ??
                          throw new InvalidDataException("C# Type was not found.");
            builder.WithType(csType);
        }

        return builders.Skip(1).Select(x => x.Build()).ToArray();
    }

    /// <summary>
    /// Converts from runtime optional to serialized optional
    /// </summary>
    private static HavokType.Optional ConvertOptional(int optional)
    {
        int serializeOptional = 0;
        foreach (KeyValuePair<int, int> bits in OptionalBitMap)
        {
            serializeOptional |= ((optional >> bits.Key) & 1) << bits.Value;
        }

        return (HavokType.Optional)serializeOptional;
    }
}