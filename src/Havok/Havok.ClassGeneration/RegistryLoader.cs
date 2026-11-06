using System.Xml.Linq;
using HKLib.Reflection.hk2018;

namespace Havok.ClassGeneration;

/// <summary>
/// Loads a type registry xml the same way as <see cref="HavokTypeRegistry" />, but keeps the xml of each type so
/// enum values can be read. The C# types of the 2018 identity type map are attached to the types, which the generator
/// uses to match the naming of the 2018 classes.
/// </summary>
internal class RegistryLoader
{
    // Runtime optional bit -> serialized optional bit for the 2018 runtime
    private static readonly Dictionary<int, int> OptionalBitMap2018 = new()
    {
        { 0, 0 }, { 1, 1 }, { 4, 2 }, { 23, 3 }, { 24, 4 }, { 26, 5 }, { 17, 6 }, { 28, 7 }
    };

    // Runtime optional bit -> serialized optional bit for the 2019 runtime
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

    // The 2015/2016 SDKs name some math types differently: fields there are typed hkMatrix3f etc., which HKLib's 2018
    // map binds to List<float> (2018 fields use hkMatrix3/hkMatrix4). They are the same matrices.
    private static readonly Dictionary<string, Type> MathAliases = new()
    {
        { "hkMatrix3f", typeof(HKLib.ValueTypes.Matrix3x3) },
        { "hkMatrix3Impl<float>", typeof(HKLib.ValueTypes.Matrix3x3) },
        { "hkRotationf", typeof(HKLib.ValueTypes.Matrix3x3) },
        { "hkRotationImpl<float>", typeof(HKLib.ValueTypes.Matrix3x3) },
        { "hkMatrix4f", typeof(System.Numerics.Matrix4x4) },
        { "hkMatrix4Impl<float>", typeof(System.Numerics.Matrix4x4) }
    };

    /// <summary>
    /// The HKLib 2018 C# type for an identity (used for naming and placement), with the math aliases above.
    /// </summary>
    private static Type? ResolveClrType(string identity) =>
        MathAliases.TryGetValue(identity, out Type? alias) ? alias
        : HKLib.hk2018.IdentityTypeMap.Map.TryGetValue(identity, out Type? type) ? type
        : null;

    public List<HavokType> Types { get; } = new();

    public Dictionary<string, HavokType> ByIdentity { get; } = new();

    public Dictionary<HavokType, XElement> Elements { get; } = new();

    public static RegistryLoader Load(string path)
    {
        RegistryLoader loader = new();
        XElement registry = XElement.Load(path);
        Dictionary<int, int>? bitMap = registry.Attribute("Optionals")?.Value == "Serialized"
            ? null
            : registry.Attribute("Version")?.Value?.StartsWith("2019") == true ? OptionalBitMap2019 : OptionalBitMap2018;
        List<XElement> elements = registry.Elements().SelectMany(x => x.Elements()).ToList();

        int maxId = elements.Max(x => int.Parse(x.Attribute("Id")!.Value));
        HavokTypeBuilder[] builders = new HavokTypeBuilder[maxId + 1];
        XElement?[] elementsById = new XElement?[maxId + 1];
        foreach (XElement element in elements)
        {
            int id = int.Parse(element.Attribute("Id")!.Value);
            builders[id] = new HavokTypeBuilder(ResolveClrType);
            elementsById[id] = element;
        }

        foreach (XElement type in elements)
        {
            HavokTypeBuilder builder = builders[int.Parse(type.Attribute("Id")!.Value)];

            builder.WithName(type.Attribute("Name")!.Value)
                .WithFlags((HavokType.TypeFlags)int.Parse(type.Attribute("Flags")!.Value))
                .WithFormat(int.Parse(type.Attribute("Format")!.Value))
                .WithSizeAlignment(int.Parse(type.Attribute("Size")!.Value),
                    int.Parse(type.Attribute("Alignment")!.Value));

            ulong version = ulong.Parse(type.Attribute("Version")!.Value);
            if (version != 0) builder.WithVersion(version);

            if (type.Attribute("Hash")?.Value is { } hash) builder.WithHash(uint.Parse(hash));

            builder.WithOptionals(ConvertOptional(int.Parse(type.Attribute("Optionals")!.Value), bitMap));

            // registries built from files only know how many properties a type has
            if (type.Attribute("PropertyCount")?.Value is { } propertyCount)
                builder.WithProperties(new HavokType.Member?[int.Parse(propertyCount)].ToList());

            string parentIndex = type.Attribute("Parent")!.Value;
            if (parentIndex != "0") builder.WithParent(builders[int.Parse(parentIndex)]);

            string subTypeIndex = type.Attribute("SubType")!.Value;
            if (subTypeIndex != "0") builder.WithSubType(builders[int.Parse(subTypeIndex)]);

            if (type.Element("Template") is { } template)
            {
                foreach (XElement param in template.Elements())
                {
                    string name = param.Attribute("Name")!.Value;
                    int value = int.Parse(param.Attribute("Value")!.Value);
                    if (param.Attribute("Kind")!.Value == "Type")
                        builder.WithTemplateParameter(name, builders[value]);
                    else
                        builder.WithTemplateParameter(name, value);
                }
            }

            if (type.Element("Members") is { } members)
            {
                foreach (XElement member in members.Elements())
                {
                    string name = member.Attribute("Name")!.Value;
                    int offset = int.Parse(member.Attribute("Offset")!.Value);
                    HavokTypeBuilder memberType = builders[int.Parse(member.Attribute("Type")!.Value)];
                    HKLib.Reflection.HavokType.Member.MemberFlags flags =
                        (HKLib.Reflection.HavokType.Member.MemberFlags)int.Parse(member.Attribute("Flags")!.Value);
                    if (flags.HasFlag(HKLib.Reflection.HavokType.Member.MemberFlags.Property))
                        builder.WithProperty(name, flags, offset, memberType);
                    else
                        builder.WithField(name, flags, offset, memberType);
                }
            }

            if (type.Element("Interfaces") is { } interfaces)
            {
                foreach (XElement iface in interfaces.Elements())
                {
                    builder.WithInterface(builders[int.Parse(iface.Attribute("Type")!.Value)],
                        int.Parse(iface.Attribute("Offset")!.Value));
                }
            }

            if (type.Element("Presets") is { } presets)
            {
                foreach (XElement preset in presets.Elements())
                {
                    builder.WithPreset(preset.Attribute("Name")!.Value);
                }
            }
        }

        for (int id = 0; id <= maxId; id++)
        {
            if (builders[id] is null) continue;

            HavokType type = builders[id].Build();
            if (loader.ByIdentity.ContainsKey(type.Identity)) continue;

            loader.ByIdentity.Add(type.Identity, type);
            loader.Types.Add(type);
            loader.Elements.Add(type, elementsById[id]!);
        }

        return loader;
    }

    /// <summary>
    /// Enum values from the presets of the type. The 2018 registry loader only keeps the names.
    /// </summary>
    public List<(string Name, long Value)> GetPresetValues(HavokType type)
    {
        List<(string, long)> values = new();
        if (!Elements.TryGetValue(type, out XElement? element) || element.Element("Presets") is not { } presets)
            return values;

        foreach (XElement preset in presets.Elements())
        {
            values.Add((preset.Attribute("Name")!.Value, long.Parse(preset.Attribute("Value")!.Value)));
        }

        return values;
    }

    // bitMap null: the registry stores serialized optionals (built from game files)
    private static HavokType.Optional ConvertOptional(int optional, Dictionary<int, int>? bitMap)
    {
        int serializeOptional = 0;
        if (bitMap is null) return (HavokType.Optional)optional;
        foreach (KeyValuePair<int, int> bits in bitMap)
        {
            serializeOptional |= ((optional >> bits.Key) & 1) << bits.Value;
        }

        return (HavokType.Optional)serializeOptional;
    }
}
