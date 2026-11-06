using System.Xml.Linq;

namespace Havok.Reflection;

/// <summary>
/// The Havok types of one SDK version, loaded from a registry XML (the HKLib format) with any number of overlays on
/// top. Ported from HKLib's hk2018/hk2019 registries, which differed only in their built-in instances.
/// </summary>
public sealed class HavokTypeRegistry
{
    // Registries store the runtime optionals bitfield; tagfiles serialize a compact one. Runtime bit → serialized bit.
    private static readonly Dictionary<int, int> OptionalBitMap2018 = new()
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

    // The 2019 runtime moved the size/align, flags, members, interfaces and attributes bits down by 3 (from HKLib's
    // hk2019 registry, verified there against the AC6 compendiums).
    private static readonly Dictionary<int, int> OptionalBitMap2019 = new()
    {
        { 0, 0 },
        { 1, 1 },
        { 4, 2 },
        { 20, 3 },
        { 21, 4 },
        { 23, 5 },
        { 14, 6 },
        { 25, 7 }
    };

    /// <summary>
    /// The runtime → serialized optionals map for a registry's SDK version. Only 2018 and 2019 registries exist so
    /// far; the 2015/2016 registries (Phase 3) must check which layout their runtime uses.
    /// </summary>
    private static Dictionary<int, int> OptionalBitMapFor(string sdkVersion) =>
        sdkVersion.StartsWith("2019") ? OptionalBitMap2019 : OptionalBitMap2018;

    // Registries built from game files (2015, 2016) store the serialized optionals as they are (root
    // Optionals="Serialized"), so the conversion is the identity
    private static readonly Dictionary<int, int> OptionalBitMapSerialized =
        Enumerable.Range(0, 8).ToDictionary(i => i, i => i);

    private readonly Dictionary<string, HavokType> _typesFromIdentity = new();
    private readonly Dictionary<Type, HavokType> _typesFromType = new();

    public HavokTypeRegistry(IReadOnlyList<HavokType> types, string sdkVersion = "", IReadOnlyList<string>? overlays = null)
    {
        SdkVersion = sdkVersion;
        Overlays = overlays ?? Array.Empty<string>();

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

    /// <summary>
    /// The registry's SDK version string, e.g. "20180100" (the root element's Version attribute).
    /// </summary>
    public string SdkVersion { get; }

    /// <summary>
    /// The Game attribute of each overlay applied, in order (e.g. "NR").
    /// </summary>
    public IReadOnlyList<string> Overlays { get; }

    public IEnumerable<HavokType> Types => _typesFromIdentity.Values;

    public int Count => _typesFromIdentity.Count;

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

    /// <summary>
    /// Loads a registry, then applies each overlay in order. Each overlay HavokType element replaces the base element
    /// with the same Id, and must have the same Name. Type references in an overlay use the base registry's Ids.
    /// </summary>
    /// <param name="path">The base registry XML.</param>
    /// <param name="resolveType">Maps a type identity to its C# class; null leaves every type without one.</param>
    /// <param name="overlayPaths">Overlay XMLs, applied in order.</param>
    public static HavokTypeRegistry Load(string path, Func<string, Type?>? resolveType = null, params string[] overlayPaths)
    {
        if (!File.Exists(path))
        {
            throw new ArgumentException($"The registry file \"{path}\" does not exist", nameof(path));
        }

        XElement typeRegistry = XElement.Load(path);
        var overlays = new List<string>();
        foreach (string overlayPath in overlayPaths)
        {
            if (!File.Exists(overlayPath))
            {
                throw new ArgumentException($"The overlay file \"{overlayPath}\" does not exist", nameof(overlayPaths));
            }

            XElement overlay = XElement.Load(overlayPath);
            ApplyOverlay(typeRegistry, overlay);
            overlays.Add(overlay.Attribute("Game")?.Value ?? Path.GetFileNameWithoutExtension(overlayPath));
        }

        HavokType[] types = Load(typeRegistry, resolveType);
        return new HavokTypeRegistry(types, typeRegistry.Attribute("Version")?.Value ?? "", overlays);
    }

    private static void ApplyOverlay(XElement typeRegistry, XElement overlay)
    {
        string? baseVersion = overlay.Attribute("Base")?.Value;
        string? registryVersion = typeRegistry.Attribute("Version")?.Value;
        if (baseVersion != null && registryVersion != null && baseVersion != registryVersion)
        {
            throw new InvalidDataException($"Overlay is for registry {baseVersion}, but the base registry is {registryVersion}.");
        }

        Dictionary<string, XElement> baseTypes = typeRegistry.Elements().SelectMany(x => x.Elements())
            .ToDictionary(x => x.Attribute("Id")!.Value);

        foreach (XElement type in overlay.Elements("HavokType"))
        {
            string id = type.Attribute("Id")!.Value;
            if (!baseTypes.TryGetValue(id, out XElement? baseType))
            {
                throw new InvalidDataException($"Overlay type Id {id} is not in the base registry.");
            }

            string name = type.Attribute("Name")!.Value;
            if (baseType.Attribute("Name")!.Value != name)
            {
                throw new InvalidDataException(
                    $"Overlay type Id {id} is \"{name}\", but the base registry has \"{baseType.Attribute("Name")!.Value}\".");
            }

            var replacement = new XElement(type);
            baseType.ReplaceWith(replacement);
            baseTypes[id] = replacement;
        }
    }

    private static HavokType[] Load(XElement typeRegistry, Func<string, Type?>? resolveType)
    {
        Dictionary<int, int> optionalBitMap = typeRegistry.Attribute("Optionals")?.Value == "Serialized"
            ? OptionalBitMapSerialized
            : OptionalBitMapFor(typeRegistry.Attribute("Version")?.Value ?? "");
        List<XElement> types = typeRegistry.Elements().SelectMany(x => x.Elements()).ToList();
        HavokTypeBuilder[] builders = types.OrderBy(x => int.Parse(x.Attribute("Id")!.Value))
            .Select(_ => resolveType is null ? new HavokTypeBuilder() : new HavokTypeBuilder(resolveType))
            .Prepend(null!).ToArray();

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
            builder.WithOptionals(ConvertOptional(optional, optionalBitMap));

            string parentIndex = type.Attribute("Parent")!.Value;
            if (parentIndex != "0")
            {
                builder.WithParent(builders[int.Parse(parentIndex)]);
            }

            string subTypeIndex = type.Attribute("SubType")!.Value;
            if (subTypeIndex != "0")
            {
                builder.WithSubType(builders[int.Parse(subTypeIndex)]);
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
                            builder.WithTemplateParameter(name, builders[value]);
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

            // registries built from files only know how many properties a type has
            if (type.Attribute("PropertyCount")?.Value is { } propertyCount)
            {
                builder.WithProperties(new HavokType.Member?[int.Parse(propertyCount)].ToList());
            }

            if (type.Element("Members") is { } members)
            {
                foreach (XElement member in members.Elements())
                {
                    string name = member.Attribute("Name")!.Value;
                    int offset = int.Parse(member.Attribute("Offset")!.Value);
                    HavokTypeBuilder memberTypeBuilder = builders[int.Parse(member.Attribute("Type")!.Value)];
                    var memberFlags = (HavokType.Member.MemberFlags)int.Parse(member.Attribute("Flags")!.Value);
                    if (memberFlags.HasFlag(HavokType.Member.MemberFlags.Property))
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
                    builder.WithInterface(builders[int.Parse(iface.Attribute("Type")!.Value)], offset);
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
    private static HavokType.Optional ConvertOptional(int optional, Dictionary<int, int> optionalBitMap)
    {
        int serializeOptional = 0;
        foreach (KeyValuePair<int, int> bits in optionalBitMap)
        {
            serializeOptional |= ((optional >> bits.Key) & 1) << bits.Value;
        }

        return (HavokType.Optional)serializeOptional;
    }
}
