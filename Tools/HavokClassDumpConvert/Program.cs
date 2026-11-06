using System.Xml.Linq;

// Converts the DS3 hk_2014 class dumps (src/Havok/HKX.ClassGeneration/classxmlds3, the hkClass reflection HKX2 was
// generated from) into a type registry in the HKLib XML format, so Havok.Core, Havok.ClassGeneration and the packfile
// engine can use hk2014 like every other SDK.
//
//   HavokClassDumpConvert <class dump folder> <2018 registry xml> <output registry xml>
//
// Packfile reflection describes members by vtype/vsubtype (TYPE_ARRAY of TYPE_STRUCT…). These become tagfile-style types
// with the formats of HKLib's 2018 registry (hkArray<T>, T*, hkEnum<E, S>, hkVector4…), so the engines dispatch on them
// the same way and the generator binds the same C# types. Nested classes, which the dumps flatten
// (hkRootLevelContainerNamedVariant), take HKLib's 2018 identity where it matches (hkRootLevelContainer::NamedVariant).

if (args.Length < 3)
{
    Console.WriteLine("Usage: HavokClassDumpConvert <class dump folder> <2018 registry xml> <output registry xml> [pointer size = 8] [version = hk_2014.1.0-r1] [game = DS3] [source note]");
    return 1;
}

// 32-bit SDKs (DeS Havok-5.x, DS1 hk_2010) come from the hkClass reflection in the games' own files (survey --dumpclasses)
int ps = args.Length > 3 ? int.Parse(args[3]) : 8;
string sdkVersion = args.Length > 4 ? args[4] : "hk_2014.1.0-r1";
string gameName = args.Length > 5 ? args[5] : "DS3";
string sourceNote = args.Length > 6 ? args[6] : "the DS3 hk_2014 class dumps (src/Havok/HKX.ClassGeneration/classxmlds3)";
int arraySize = ps + 8; // data pointer, size, capacity and flags


var dumps = Directory.GetFiles(args[0], "*.xml")
    .Select(f => XElement.Parse(File.ReadAllText(f)))
    .ToDictionary(e => e.Attribute("name")!.Value);

// 2018 identities by their flattened name, for nested classes
var registry2018 = XElement.Load(args[1]);
var nested = new Dictionary<string, string>();
var topLevel2018 = new HashSet<string>();
foreach (var e in registry2018.Elements().SelectMany(x => x.Elements()))
{
    var name = e.Attribute("Name")!.Value;
    if (e.Element("Template") != null) continue;
    if (name.Contains("::")) nested.TryAdd(name.Replace("::", ""), name);
    else topLevel2018.Add(name);
}

string Identity(string dumpName) =>
    !topLevel2018.Contains(dumpName) && nested.TryGetValue(dumpName, out var identity) && dumps.ContainsKey(dumpName)
        ? identity
        : dumpName;

// ---- Types ----------------------------------------------------------------------------------------------------------

var types = new List<XElement>();
var ids = new Dictionary<string, int>();
var warnings = new List<string>();

int Add(string identity, Func<int, XElement> create)
{
    if (ids.TryGetValue(identity, out var existing)) return existing;
    int id = types.Count + 1;
    ids[identity] = id;
    types.Add(null!); // reserve the Id before creating, so recursive types can refer to it
    types[id - 1] = create(id);
    return id;
}

XElement Type(int id, string name, int format, int size, int alignment, int parent = 0, int subType = 0,
    int optionals = 0b1011, int flags = 0, int version = 0, uint? hash = null) =>
    new XElement("HavokType",
        new XAttribute("Id", id), new XAttribute("Name", name), new XAttribute("Parent", parent),
        new XAttribute("Optionals", optionals), new XAttribute("Format", format), new XAttribute("SubType", subType),
        new XAttribute("Size", size), new XAttribute("Alignment", alignment), new XAttribute("Flags", flags),
        new XAttribute("Version", version), hash is { } h ? new XAttribute("Hash", h) : null);

XElement Template(params (string Name, string Kind, int Value)[] parameters) =>
    new("Template", parameters.Select(p => new XElement("Parameter",
        new XAttribute("Name", p.Name), new XAttribute("Kind", p.Kind), new XAttribute("Value", p.Value))));

// optionals: Format 1, SubType 2, Version 4, SizeAlign 8, Flags 16, Members 32
const int OptFormatSize = 1 | 8;
const int OptFormatSubSize = 1 | 2 | 8;

int Size(int id) => int.Parse(types[id - 1].Attribute("Size")!.Value);
// alignment of a type that may still be under construction: 0 until the final pass works it out
int Align(int id) => types[id - 1] is { } e ? int.Parse(e.Attribute("Alignment")!.Value) : 0;

int Primitive(string vtype) => vtype switch
{
    "TYPE_BOOL" => Add("hkBool", id => Type(id, "hkBool", 8194, 1, 1, optionals: OptFormatSize)),
    "TYPE_CHAR" => Add("char", id => Type(id, "char", 8196, 1, 1, optionals: OptFormatSize)),
    "TYPE_INT8" => Add("hkInt8", id => Type(id, "hkInt8", 8708, 1, 1, optionals: OptFormatSize)),
    "TYPE_UINT8" => Add("hkUint8", id => Type(id, "hkUint8", 8196, 1, 1, optionals: OptFormatSize)),
    "TYPE_INT16" => Add("hkInt16", id => Type(id, "hkInt16", 16900, 2, 2, optionals: OptFormatSize)),
    "TYPE_UINT16" => Add("hkUint16", id => Type(id, "hkUint16", 16388, 2, 2, optionals: OptFormatSize)),
    "TYPE_INT32" => Add("hkInt32", id => Type(id, "hkInt32", 33284, 4, 4, optionals: OptFormatSize)),
    "TYPE_UINT32" => Add("hkUint32", id => Type(id, "hkUint32", 32772, 4, 4, optionals: OptFormatSize)),
    "TYPE_INT64" => Add("hkInt64", id => Type(id, "hkInt64", 66052, 8, 8, optionals: OptFormatSize)),
    "TYPE_UINT64" => Add("hkUint64", id => Type(id, "hkUint64", 65540, 8, 8, optionals: OptFormatSize)),
    "TYPE_ULONG" => ps == 8
        ? Add("hkUlong", id => Type(id, "hkUlong", 65540, 8, 8, optionals: OptFormatSize))
        : Add("hkUlong", id => Type(id, "hkUlong", 32772, 4, 4, optionals: OptFormatSize)),
    "TYPE_REAL" => Add("hkReal", id => Type(id, "hkReal", 1525253, 4, 4, optionals: OptFormatSize)),
    "TYPE_HALF" => Add("hkHalf16", id => Type(id, "hkHalf16", 476677, 2, 2, optionals: OptFormatSize)),
    "TYPE_VECTOR4" => Add("hkVector4", id => Type(id, "hkVector4", 1064, 16, 16, subType: Primitive("TYPE_REAL"), optionals: OptFormatSubSize)),
    "TYPE_QUATERNION" => Add("hkQuaternion", id => Type(id, "hkQuaternion", 1064, 16, 16, subType: Primitive("TYPE_REAL"), optionals: OptFormatSubSize)),
    "TYPE_MATRIX3" => Add("hkMatrix3", id => Type(id, "hkMatrix3", 3112, 48, 16, subType: Primitive("TYPE_REAL"), optionals: OptFormatSubSize)),
    "TYPE_ROTATION" => Add("hkRotation", id => Type(id, "hkRotation", 3112, 48, 16, subType: Primitive("TYPE_REAL"), optionals: OptFormatSubSize)),
    "TYPE_MATRIX4" => Add("hkMatrix4", id => Type(id, "hkMatrix4", 4136, 64, 16, subType: Primitive("TYPE_REAL"), optionals: OptFormatSubSize)),
    "TYPE_TRANSFORM" => Add("hkTransform", id => Type(id, "hkTransform", 4136, 64, 16, subType: Primitive("TYPE_REAL"), optionals: OptFormatSubSize)),
    "TYPE_QSTRANSFORM" => QsTransform(),
    "TYPE_STRINGPTR" => Add("hkStringPtr", id => Type(id, "hkStringPtr", 131, ps, ps, optionals: OptFormatSize)),
    "TYPE_CSTRING" => Add("char*", id => Type(id, "char*", 3, ps, ps, optionals: OptFormatSize)),
    "TYPE_VARIANT" => Variant(),
    _ => throw new InvalidDataException($"No primitive for {vtype}")
};

int QsTransform() => Add("hkQsTransform", id =>
{
    int vector = Primitive("TYPE_VECTOR4");
    int quaternion = Primitive("TYPE_QUATERNION");
    var e = Type(id, "hkQsTransform", 7, 48, 16, optionals: OptFormatSize | 32);
    e.Add(new XElement("Members",
        Member("translation", 0, vector, false), Member("rotation", 16, quaternion, false), Member("scale", 32, vector, false)));
    return e;
});

// hkVariant: an object pointer and a class pointer
int Variant() => Add("hkVariant", id =>
{
    var e = Type(id, "hkVariant", 7, 2 * ps, ps, optionals: OptFormatSize | 32);
    e.Add(new XElement("Members",
        // untyped on the 32-bit SDKs: Havok 5.x roots (hkaAnimationContainer...) aren't hkReferencedObjects
        Member("object", 0, ps == 8 ? Pointer(Record("hkReferencedObject")) : VoidPointer(), false),
        Member("class", ps, Pointer(Record("hkClass")), false)));
    return e;
});

XElement Member(string name, int offset, int type, bool ignored) => new("Member",
    new XAttribute("Name", name), new XAttribute("Offset", offset), new XAttribute("Type", type),
    new XAttribute("Flags", 32 | (ignored ? 1 : 0)));

int Pointer(int target)
{
    string targetIdentity = IdentityOf(target);
    return Add($"T*<{targetIdentity}>", id =>
    {
        var e = Type(id, "T*", 6, ps, ps, subType: target, optionals: OptFormatSubSize);
        e.Add(Template(("T", "Type", target)));
        return e;
    });
}

int VoidPointer() => Add("void*", id => Type(id, "void*", 6, ps, ps, optionals: OptFormatSize));

int Container(string name, int element, int size, int alignment) =>
    Add($"{name}<{IdentityOf(element)}>", id =>
    {
        var e = Type(id, name, 8, size, alignment, subType: element, optionals: OptFormatSubSize);
        e.Add(Template(("T", "Type", element)));
        return e;
    });

int FixedArray(int element, int count) =>
    Add($"T[N]<{IdentityOf(element)}, {count}>", id =>
    {
        var e = Type(id, "T[N]", 8 | (count << 8), Size(element) * count, Align(element), subType: element, optionals: OptFormatSubSize);
        e.Add(Template(("T", "Type", element), ("N", "Value", count)));
        return e;
    });

// enums: the enum itself (an int with presets), wrapped as hkEnum<E, storage> or hkFlags<E, storage>
int EnumType(string identity, XElement definition) => Add(identity, id =>
{
    var e = Type(id, identity, 33284, 4, 4, optionals: OptFormatSize);
    e.Add(new XElement("Presets", definition.Elements("enumitem").Select(i =>
        new XElement("Preset", new XAttribute("Name", i.Attribute("name")!.Value), new XAttribute("Value", i.Attribute("value")!.Value)))));
    return e;
});

int Wrapped(string wrapper, int enumType, int storage) =>
    Add($"{wrapper}<{IdentityOf(enumType)}, {IdentityOf(storage)}>", id =>
    {
        var s = types[storage - 1];
        var e = Type(id, wrapper, int.Parse(s.Attribute("Format")!.Value), Size(storage), Align(storage), optionals: OptFormatSize);
        e.Add(Template((wrapper == "hkFlags" ? "BITS" : "ENUM", "Type", enumType), ("STORAGE", "Type", storage)));
        return e;
    });

string IdentityOf(int id) => ids.First(x => x.Value == id).Key;

// a class, by its dump name
int Record(string dumpName)
{
    if (!dumps.TryGetValue(dumpName, out var cls))
    {
        warnings.Add($"No dump for {dumpName}; an empty record stands in");
        return Add(dumpName, id => Type(id, dumpName, 7, 0, 1, optionals: OptFormatSize));
    }

    string identity = Identity(dumpName);
    if (ids.TryGetValue(identity, out var known)) return known;

    int id = types.Count + 1;
    ids[identity] = id;
    types.Add(null!);

    int parent = cls.Attribute("parent")?.Value is { } p ? Record(p) : 0;
    int size = int.Parse(cls.Attribute("size")!.Value);
    var members = new List<XElement>();
    int alignment = parent == 0 ? 1 : Align(parent);
    foreach (var m in cls.Element("members")?.Elements("member") ?? Enumerable.Empty<XElement>())
    {
        int memberType = MemberType(cls, identity, m);
        alignment = Math.Max(alignment, Align(memberType));
        members.Add(Member(m.Attribute("name")!.Value, int.Parse(m.Attribute("offset")!.Value), memberType,
            m.Attribute("flags")!.Value.Contains("SERIALIZE_IGNORED") || m.Attribute("vtype")!.Value == "TYPE_HOMOGENEOUSARRAY"));
    }

    var e = Type(id, identity, 7, size, alignment, parent: parent, optionals: OptFormatSize | 4 | 32,
        version: int.Parse(cls.Attribute("version")?.Value ?? "0"),
        hash: Convert.ToUInt32(cls.Attribute("signature")!.Value, 16));
    if (members.Count > 0) e.Add(new XElement("Members", members));
    types[id - 1] = e;
    return id;
}

int MemberType(XElement cls, string classIdentity, XElement m)
{
    string vtype = m.Attribute("vtype")!.Value;
    string vsubtype = m.Attribute("vsubtype")!.Value;
    string? ctype = m.Attribute("ctype")?.Value;
    int count = int.Parse(m.Attribute("arrsize")!.Value);

    int Element(string kind) => kind switch
    {
        "TYPE_STRUCT" => Record(ctype!),
        "TYPE_POINTER" => ctype != null ? Pointer(Record(ctype)) : VoidPointer(),
        "TYPE_VOID" => VoidPointer(),
        _ => Primitive(kind)
    };

    int type = vtype switch
    {
        "TYPE_STRUCT" => Record(ctype!),
        "TYPE_POINTER" when vsubtype == "TYPE_CHAR" => Primitive("TYPE_CSTRING"),
        "TYPE_POINTER" => ctype != null ? Pointer(Record(ctype)) : VoidPointer(),
        "TYPE_ARRAY" => Container("hkArray", Element(vsubtype), arraySize, ps),
        "TYPE_SIMPLEARRAY" => Container("hkSimpleArray", Element(vsubtype), ps + 4, ps), // T* data, int size: the next member may follow at +12
        "TYPE_RELARRAY" => Container("hkRelArray", Element(vsubtype), 4, 2),
        // an hkClass pointer, data and count: the data is only readable through the class (Havok 5.x hkxVertexBuffer)
        "TYPE_HOMOGENEOUSARRAY" => Add("hkHomogeneousArray", id => Type(id, "hkHomogeneousArray", 7, 2 * ps + 4, ps, optionals: OptFormatSize)),
        "TYPE_FUNCTIONPOINTER" => VoidPointer(),
        "TYPE_ENUM" or "TYPE_FLAGS" => EnumOrStorage(cls, classIdentity, m, vtype == "TYPE_FLAGS" ? "hkFlags" : "hkEnum", vsubtype),
        _ => Primitive(vtype)
    };

    return count > 0 ? FixedArray(type, count) : type;
}

int EnumOrStorage(XElement cls, string classIdentity, XElement m, string wrapper, string storageKind)
{
    int storage = Primitive(storageKind);
    if (m.Attribute("etype")?.Value is not { } etype) return storage;

    // enums are declared on the class or one of its parents
    for (XElement? c = cls; c != null; c = c.Attribute("parent")?.Value is { } p && dumps.TryGetValue(p, out var pc) ? pc : null)
    {
        var definition = c.Element("enums")?.Elements("enum").FirstOrDefault(x => x.Attribute("name")!.Value == etype);
        if (definition == null) continue;
        return Wrapped(wrapper, EnumType($"{Identity(c.Attribute("name")!.Value)}::{etype}", definition), storage);
    }

    // or on an enclosing class (hkxVertexDescription::ElementDecl uses hkxVertexDescription's DataUsage): the longest
    // class name the dump name starts with that declares it
    string dumpName = cls.Attribute("name")!.Value;
    var outer = dumps.Values
        .Where(c => dumpName.StartsWith(c.Attribute("name")!.Value) && c != cls)
        .OrderByDescending(c => c.Attribute("name")!.Value.Length)
        .Select(c => (Class: c, Definition: c.Element("enums")?.Elements("enum").FirstOrDefault(x => x.Attribute("name")!.Value == etype)))
        .FirstOrDefault(x => x.Definition != null);
    if (outer.Definition != null)
        return Wrapped(wrapper, EnumType($"{Identity(outer.Class.Attribute("name")!.Value)}::{etype}", outer.Definition), storage);

    // or on another class altogether (WeldingType is hkpWeldingUtility's), when exactly one declares it
    var declaring = dumps.Values
        .Select(c => (Class: c, Definition: c.Element("enums")?.Elements("enum").FirstOrDefault(x => x.Attribute("name")!.Value == etype)))
        .Where(x => x.Definition != null)
        .ToList();
    if (declaring.Count == 1)
        return Wrapped(wrapper, EnumType($"{Identity(declaring[0].Class.Attribute("name")!.Value)}::{etype}", declaring[0].Definition!), storage);

    warnings.Add($"{classIdentity}.{m.Attribute("name")!.Value}: enum {etype} not found");
    return storage;
}

foreach (var name in dumps.Keys.OrderBy(x => x, StringComparer.Ordinal)) Record(name);

// Alignments of records and fixed arrays depend on their members, which may not have existed when they were created
// (classes refer to each other), so they are worked out over the finished graph
var alignments = new Dictionary<int, int>();
int FinalAlign(int id)
{
    if (alignments.TryGetValue(id, out var known)) return known;
    var e = types[id - 1];
    string name = e.Attribute("Name")!.Value;
    int alignment;
    if (e.Attribute("Format")!.Value == "7") // record
    {
        alignments[id] = 1; // a placeholder in case of a cycle
        alignment = 1;
        if (int.Parse(e.Attribute("Parent")!.Value) is var parent and > 0) alignment = Math.Max(alignment, FinalAlign(parent));
        foreach (var m in e.Element("Members")?.Elements() ?? Enumerable.Empty<XElement>())
            alignment = Math.Max(alignment, FinalAlign(int.Parse(m.Attribute("Type")!.Value)));
    }
    else if (name == "T[N]")
    {
        alignment = FinalAlign(int.Parse(e.Attribute("SubType")!.Value));
    }
    else
    {
        alignment = int.Parse(e.Attribute("Alignment")!.Value);
    }

    alignments[id] = alignment;
    e.Attribute("Alignment")!.Value = alignment.ToString();
    return alignment;
}

for (int id = 1; id <= types.Count; id++) FinalAlign(id);

// a fixed array of records is as big as its elements, which may also have been unknown
foreach (var e in types.Where(e => e.Attribute("Name")!.Value == "T[N]"))
{
    int element = int.Parse(e.Attribute("SubType")!.Value);
    int count = int.Parse(e.Attribute("Format")!.Value) >> 8;
    e.Attribute("Size")!.Value = (Size(element) * count).ToString();
}

var root = new XElement("HavokTypeRegistry",
    new XAttribute("Version", sdkVersion),
    new XAttribute("Optionals", "Serialized"),
    new XAttribute("Game", gameName),
    new XComment($" Converted by Tools/HavokClassDumpConvert from {sourceNote}. Class hashes are the packfile class signatures. "),
    new XElement("Types", types));
root.Save(args[2]);

foreach (var w in warnings.Distinct()) Console.WriteLine($"Warning: {w}");
Console.WriteLine($"Classes: {dumps.Count} | Types: {types.Count} | Nested names from 2018: {ids.Keys.Count(k => k.Contains("::") && !k.Contains('<'))}");
return 0;
