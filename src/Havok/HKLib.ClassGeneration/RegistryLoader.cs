using System.Xml.Linq;
using HKLib.Reflection.hk2018;

namespace HKLib.ClassGeneration;

/// <summary>
/// Loads a type registry xml the same way as <see cref="HavokTypeRegistry" />, but keeps the xml of each type so
/// enum values can be read. The C# types of the 2018 identity type map are attached to the types, which the generator
/// uses to match the naming of the 2018 classes.
/// </summary>
internal class RegistryLoader
{
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

    public List<HavokType> Types { get; } = new();

    public Dictionary<string, HavokType> ByIdentity { get; } = new();

    public Dictionary<HavokType, XElement> Elements { get; } = new();

    public static RegistryLoader Load(string path)
    {
        RegistryLoader loader = new();
        XElement registry = XElement.Load(path);
        List<XElement> elements = registry.Elements().SelectMany(x => x.Elements()).ToList();

        int maxId = elements.Max(x => int.Parse(x.Attribute("Id")!.Value));
        HavokTypeBuilder[] builders = new HavokTypeBuilder[maxId + 1];
        XElement?[] elementsById = new XElement?[maxId + 1];
        foreach (XElement element in elements)
        {
            int id = int.Parse(element.Attribute("Id")!.Value);
            builders[id] = new HavokTypeBuilder();
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

            builder.WithOptionals(ConvertOptional(int.Parse(type.Attribute("Optionals")!.Value)));

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
                    Reflection.HavokType.Member.MemberFlags flags =
                        (Reflection.HavokType.Member.MemberFlags)int.Parse(member.Attribute("Flags")!.Value);
                    if (flags.HasFlag(Reflection.HavokType.Member.MemberFlags.Property))
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

    private static HavokType.Optional ConvertOptional(int optional)
    {
        int serializeOptional = 0;
        foreach (KeyValuePair<int, int> bits in OptionalBitMap2019)
        {
            serializeOptional |= ((optional >> bits.Key) & 1) << bits.Value;
        }

        return (HavokType.Optional)serializeOptional;
    }
}
