using System.Text.RegularExpressions;
using Havok.Reflection;

namespace Havok.ClassGeneration;

internal enum DeclKind
{
    Class,
    Interface,
    Enum
}

/// <summary>
/// How a field is declared and accessed by its reflection data class.
/// </summary>
internal enum Shape
{
    // primitive or other value which needs no initializer
    Value,

    // non-nullable reference or struct which is initialized with new()
    Init,

    // nullable reference
    Nullable,

    // enum, which may also be get/set as its storage type
    Enum,

    // fixed size array, set through reflection as the field is readonly
    Fixed,

    // generic type parameter
    Generic
}

/// <summary>
/// A rendered C# type. References to generated declarations are marked with backticks so they can be written relative
/// to the hk2019 namespace or fully qualified.
/// </summary>
internal record TypeInfo(Shape Shape, string Text)
{
    public static readonly Regex DeclReference = new("`([^`]+)`", RegexOptions.Compiled);

    // storage type of an enum
    public string? Storage { get; init; }

    // element type and dimensions of a fixed size array
    public string? Element { get; init; }
    public int[] Dimensions { get; init; } = Array.Empty<int>();

    public string Write(bool qualified) => Materialize(Text, qualified);

    public static string Materialize(string text, bool qualified) =>
        DeclReference.Replace(text, m => qualified ? Options.Namespace + "." + m.Groups[1].Value : m.Groups[1].Value);

    /// <summary>
    /// Replaces generic parameters, used when a generic base type's fields are inherited by a constructed type.
    /// </summary>
    public TypeInfo Substitute(IReadOnlyDictionary<string, TypeInfo> arguments)
    {
        if (arguments.Count == 0) return this;
        if (Shape == Shape.Generic) return arguments.TryGetValue(Text, out TypeInfo? argument) ? argument : this;

        return this with
        {
            Text = SubstituteText(Text, arguments),
            Element = Element is null ? null : SubstituteText(Element, arguments)
        };
    }

    private static string SubstituteText(string text, IReadOnlyDictionary<string, TypeInfo> arguments)
    {
        // only replace outside of the marked declaration references
        string[] parts = text.Split('`');
        for (int i = 0; i < parts.Length; i += 2)
        {
            parts[i] = Regex.Replace(parts[i], @"\b[A-Za-z_]\w*\b",
                m => arguments.TryGetValue(m.Value, out TypeInfo? argument) ? argument.Text : m.Value);
        }

        return string.Join('`', parts);
    }
}

internal class FieldDecl
{
    public FieldDecl(string name, TypeInfo type)
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }
    public TypeInfo Type { get; }
}

internal class Decl
{
    public Decl(string name, string ns, Decl? outer, DeclKind kind)
    {
        Name = name;
        Namespace = ns;
        Outer = outer;
        Kind = kind;
        outer?.Nested.Add(this);
    }

    public string Name { get; }

    // relative to the target namespace, only used for top level declarations
    public string Namespace { get; }
    public Decl? Outer { get; }
    public DeclKind Kind { get; }

    public List<string> GenericParameters { get; } = new();
    public List<Decl> Nested { get; } = new();

    // candidate havok types the members are built from, generic declarations have one per instantiation
    public List<HavokType> Sources { get; } = new();

    public bool Built { get; set; }
    public bool Partial { get; set; }

    public List<string> BaseTypes { get; } = new();
    public List<string> SharedInterfaces { get; } = new();
    public List<string> Usings { get; } = new();

    // name of the reflection data class, unique across all declarations
    public string DataName { get; set; } = "";
    public List<FieldDecl> Fields { get; } = new();

    // base class for inheriting fields in the reflection data classes
    public Decl? BaseDecl { get; set; }

    // the 2018 C# type (Havok.hk2018) this declaration mirrors, if any (its enum values fill enums the registry has no presets for)
    public Type? ClrDefinition { get; set; }
    public List<TypeInfo> BaseArguments { get; } = new();

    // enum
    public List<(string Name, long Value)> EnumValues { get; } = new();
    public bool IsFlags { get; set; }
    public string EnumUnderlyingType { get; set; } = "int";

    /// <summary>
    /// Name relative to the target namespace without generic parameters, e.g. hkcdStaticMeshTree.Section
    /// </summary>
    public string RelativeName => Outer is not null
        ? Outer.RelativeName + "." + Name
        : Namespace.Length == 0
            ? Name
            : Namespace + "." + Name;

    /// <summary>
    /// Name used for the reflection data class, e.g. hkcdStaticMeshTreeSection
    /// </summary>
    public string FlatName => Outer is not null
        ? Outer.FlatName + Name
        : Namespace.Replace(".", "") + Name;

    public Decl Root => Outer?.Root ?? this;

    public string GenericSuffix => GenericParameters.Count == 0 ? "" : "<" + string.Join(", ", GenericParameters) + ">";
}
