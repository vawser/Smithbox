using System.Text.RegularExpressions;
using Havok.Reflection;

namespace Havok.ClassGeneration;

/// <summary>
/// Builds the declarations of an SDK from its type registry. Where a type also exists in the 2018 classes (Havok.hk2018), the naming and
/// placement of the 2018 C# type is reused so code written against hk2018 reads the same against every SDK. Members, enum
/// values (when the registry has them) and base types always come from the SDK's registry.
/// </summary>
internal class Generator
{
    private const string Namespace2018 = "Havok.hk2018";

    // classes with hand written members in the SDK project's Manual folder
    private static HashSet<string> PartialClasses => Options.PartialClasses;

    private static readonly Dictionary<Type, string> Keywords = new()
    {
        { typeof(bool), "bool" }, { typeof(byte), "byte" }, { typeof(sbyte), "sbyte" },
        { typeof(short), "short" }, { typeof(ushort), "ushort" }, { typeof(int), "int" },
        { typeof(uint), "uint" }, { typeof(long), "long" }, { typeof(ulong), "ulong" },
        { typeof(float), "float" }, { typeof(double), "double" }, { typeof(string), "string" },
        { typeof(object), "object" }
    };

    private static readonly HashSet<Type> MathTypes = new()
    {
        typeof(Vector4), typeof(Quaternion), typeof(Matrix4x4), typeof(Matrix3x3)
    };

    private readonly RegistryLoader _registry;

    private readonly Dictionary<string, Decl> _declsByIdentity = new();
    private readonly Dictionary<Type, Decl> _declsByClrType = new();
    private readonly Dictionary<string, Decl> _declsByName = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _dataNames = new(StringComparer.OrdinalIgnoreCase);

    // havok template name -> 2018 generic type definitions used for its instantiations
    private readonly Dictionary<string, List<Type>> _genericDefinitions = new();

    // identities of enums used as the bits of an hkFlags
    private readonly HashSet<string> _flagsEnums = new();

    // the registry again with each game's overlay applied (NR, DB_NT): classes get the union of every variant's fields
    private readonly List<RegistryLoader> _variants;

    // declarations removed before emitting (see Run)
    private readonly HashSet<Decl> _dropped = new();

    public Generator(RegistryLoader registry, HavokTypeRegistry registry2018, IEnumerable<RegistryLoader>? variants = null)
    {
        _registry = registry;
        _variants = variants?.ToList() ?? new List<RegistryLoader>();

        foreach (HavokType type in registry2018.Types.Concat(registry.Types))
        {
            if (type.TemplateParameters.Count == 0 || type.Type is not { IsConstructedGenericType: true } clrType ||
                !Is2018(clrType)) continue;

            Type definition = clrType.GetGenericTypeDefinition();
            if (!_genericDefinitions.TryGetValue(type.Name, out List<Type>? definitions))
            {
                definitions = new List<Type>();
                _genericDefinitions.Add(type.Name, definitions);
            }

            if (!definitions.Contains(definition)) definitions.Add(definition);
        }

        foreach (HavokType type in registry.Types.Where(x => x.Name == "hkFlags"))
        {
            if (type.TemplateParameters.FirstOrDefault(x => x.Name == "BITS")?.Type is { } bits &&
                FindEnum(bits) is { } enumType)
            {
                _flagsEnums.Add(enumType.Identity);
            }
        }
    }

    public List<Decl> Decls { get; } = new();

    public List<(string Identity, string Type)> IdentityTypeMap { get; } = new();

    public List<string> Warnings { get; } = new();

    public void Run()
    {
        foreach (HavokType type in _registry.Types)
        {
            if (type.Kind == HavokType.TypeKind.Record) GetRecordDecl(type);
            else if (IsEnum(type)) GetEnumDecl(type);
        }

        // Game variants: a type the base also has becomes another source of its declaration, whose extra fields are
        // added; a type only the variant has gets a declaration of its own
        foreach (HavokType type in _variants.SelectMany(x => x.Types))
        {
            if (type.Kind == HavokType.TypeKind.Record)
            {
                if (_declsByIdentity.TryGetValue(type.Identity, out Decl? existing))
                {
                    // HavokType equality is by identity, so variants of a type must be told apart by reference
                    if (existing.Kind != DeclKind.Enum && !existing.Sources.Any(x => ReferenceEquals(x, type))) existing.Sources.Add(type);
                }
                else
                {
                    GetRecordDecl(type);
                }
            }
            else if (IsEnum(type)) GetEnumDecl(type);
        }

        // building a declaration can reference new ones
        while (Decls.FirstOrDefault(x => !x.Built) is { } decl)
        {
            Build(decl);
        }

        // An empty class whose name is also a namespace (hk2014's hkaiHierarchyUtils, where 2018 places
        // hkaiHierarchyUtils::ClusterSettings in a namespace) only exists as a scope: drop it, as C# can't have both
        HashSet<string> namespaces = Decls.Where(x => x.Namespace.Length > 0).Select(x => x.Namespace.Split('.')[0]).ToHashSet();
        foreach (Decl clash in Decls.Where(x => x.Outer is null && x.Namespace.Length == 0 && namespaces.Contains(x.Name) &&
                                                 x.Kind == DeclKind.Class && x.Fields.Count == 0 && x.Nested.Count == 0).ToList())
        {
            Decls.Remove(clash);
            _dropped.Add(clash);
            Warnings.Add($"Dropped the empty class {clash.Name}: its name is also a namespace");
        }

        HashSet<string> mapped = new();
        foreach (HavokType type in _registry.Types.Concat(_variants.SelectMany(x => x.Types)))
        {
            if (!mapped.Add(type.Identity)) continue;
            if (type.Kind == HavokType.TypeKind.Record && _declsByIdentity.TryGetValue(type.Identity, out Decl? d) && _dropped.Contains(d)) continue;
            if (GetTypeofText(type) is { } text) IdentityTypeMap.Add((type.Identity, text));
        }

        // Registries built from game files (2015, 2016) have no enum presets; enums that mirror a 2018 enum take its
        // values, so they keep their names
        foreach (Decl decl in Decls.Where(x => x.Kind == DeclKind.Enum && x.EnumValues.Count == 0))
        {
            if (decl.ClrDefinition is not { IsEnum: true } clrEnum) continue;
            foreach (string name in Enum.GetNames(clrEnum))
            {
                object value = Enum.Parse(clrEnum, name);
                long number = Enum.GetUnderlyingType(clrEnum) == typeof(ulong)
                    ? unchecked((long)Convert.ToUInt64(value))
                    : Convert.ToInt64(value);
                decl.EnumValues.Add((name, number));
            }
        }

        foreach (Decl decl in Decls.Where(x => x.Kind == DeclKind.Enum && x.EnumValues.Count == 0))
        {
            Warnings.Add($"Enum {decl.RelativeName} has no values");
        }
    }

    #region Declarations

    private Decl? GetRecordDecl(HavokType type)
    {
        if (_declsByIdentity.TryGetValue(type.Identity, out Decl? decl)) return decl;

        Type? clrType = GetClrType(type);
        if (clrType is not null)
        {
            if (!Is2018(clrType) || IsManual(clrType)) return null;
            decl = GetClrDecl(clrType.IsGenericType ? clrType.GetGenericTypeDefinition() : clrType);
        }
        else
        {
            bool isInterface = type.Flags.HasFlag(HavokType.TypeFlags.Interface);
            decl = CreateDeclFromIdentity(type, isInterface ? DeclKind.Interface : DeclKind.Class);
        }

        if (decl.Kind != DeclKind.Enum) decl.Sources.Add(type);
        _declsByIdentity[type.Identity] = decl;
        return decl;
    }

    private Decl GetEnumDecl(HavokType type)
    {
        if (_declsByIdentity.TryGetValue(type.Identity, out Decl? decl)) return decl;

        if (type.Type is { IsEnum: true } clrType && Is2018(clrType))
        {
            decl = GetClrDecl(clrType);
        }
        else
        {
            decl = CreateDeclFromIdentity(type, DeclKind.Enum);
            decl.IsFlags = _flagsEnums.Contains(type.Identity);
        }

        if (decl.EnumValues.Count == 0)
        {
            HashSet<string> names = new();
            foreach ((string name, long value) in _registry.GetPresetValues(type))
            {
                string memberName = Sanitize(name);
                if (names.Add(memberName)) decl.EnumValues.Add((memberName, value));
            }
        }

        decl.Built = true;
        _declsByIdentity[type.Identity] = decl;
        return decl;
    }

    /// <summary>
    /// Gets the declaration matching a 2018 C# type, placed the same way as the 2018 type.
    /// </summary>
    private Decl GetClrDecl(Type definition)
    {
        if (_declsByClrType.TryGetValue(definition, out Decl? decl)) return decl;

        Decl? outer = definition.DeclaringType is { } declaringType ? GetClrDecl(declaringType) : null;
        string ns = outer is not null || definition.Namespace == Namespace2018
            ? ""
            : definition.Namespace![(Namespace2018.Length + 1)..];

        DeclKind kind = definition.IsEnum ? DeclKind.Enum :
            definition.IsInterface ? DeclKind.Interface : DeclKind.Class;
        decl = CreateDecl(StripArity(definition.Name), ns, outer, kind);
        decl.ClrDefinition = definition;

        if (definition.IsGenericTypeDefinition)
        {
            int outerCount = outer?.GenericParameters.Count ?? 0;
            decl.GenericParameters.AddRange(definition.GetGenericArguments().Skip(outerCount).Select(x => x.Name));
        }

        if (kind == DeclKind.Enum)
        {
            decl.EnumUnderlyingType = Keywords[Enum.GetUnderlyingType(definition)];
            decl.IsFlags = definition.IsDefined(typeof(FlagsAttribute), false);
            decl.Built = true;
        }
        else
        {
            // the marker interfaces shared across Havok versions. Only these: the 2018 classes also implement the
            // Havok.Shared views (IHavokSkeleton...) in their Manual partials, which each SDK adds by hand
            Type[] baseInterfaces = definition.BaseType?.GetInterfaces() ?? Array.Empty<Type>();
            foreach (Type iface in definition.GetInterfaces())
            {
                if (!SharedMarkers.Contains(iface) || baseInterfaces.Contains(iface)) continue;
                decl.SharedInterfaces.Add(iface.Name);
            }
        }

        _declsByClrType.Add(definition, decl);
        return decl;
    }

    /// <summary>
    /// Places a type which has no 2018 equivalent. Scopes which are records become outer classes, other scopes become
    /// namespaces, e.g. hknpCollisionFlags::Enum -> namespace hknpCollisionFlags, enum Enum
    /// </summary>
    private Decl CreateDeclFromIdentity(HavokType type, DeclKind kind)
    {
        List<string> scopes = SplitScopes(type.Identity);
        Decl? outer = null;
        List<string> namespaces = new();

        for (int i = 0; i < scopes.Count - 1; i++)
        {
            string prefix = string.Join("::", scopes.Take(i + 1));
            if (outer is null && _registry.ByIdentity.TryGetValue(prefix, out HavokType? scopeType) &&
                scopeType.Kind == HavokType.TypeKind.Record && GetRecordDecl(scopeType) is { } scopeDecl)
            {
                outer = scopeDecl;
                continue;
            }

            if (outer is not null)
            {
                // a scope inside a class which is not a record itself
                string name = Sanitize(scopes[i]);
                outer = outer.Nested.FirstOrDefault(x => x.Name == name) ?? CreateDecl(name, "", outer, DeclKind.Class);
                continue;
            }

            namespaces.Add(Sanitize(scopes[i]));
        }

        return CreateDecl(GetDeclName(type, scopes[^1]), string.Join(".", namespaces), outer, kind);
    }

    private Decl CreateDecl(string name, string ns, Decl? outer, DeclKind kind)
    {
        string uniqueName = name;
        for (int i = 2; _declsByName.ContainsKey(GetRelativeName(uniqueName, ns, outer)); i++)
        {
            uniqueName = name + "_" + i;
        }

        // Named on the partial list by its relative name: the bare name of a top-level class, and Outer.Name for a nested
        // one (hkaiNavMesh.Face), whose outer class must be on the list too, as a nested partial needs a partial outer
        Decl decl = new(uniqueName, ns, outer, kind)
        {
            Partial = PartialClasses.Contains(GetRelativeName(uniqueName, ns, outer))
        };

        string dataName = decl.FlatName;
        for (int i = 2; !_dataNames.Add(dataName); i++)
        {
            dataName = decl.FlatName + "_" + i;
        }

        decl.DataName = dataName + "Data";

        _declsByName.Add(decl.RelativeName, decl);
        Decls.Add(decl);
        return decl;
    }

    private static string GetRelativeName(string name, string ns, Decl? outer) =>
        outer is not null ? outer.RelativeName + "." + name : ns.Length == 0 ? name : ns + "." + name;

    /// <summary>
    /// Templates without a 2018 generic equivalent become a class per instantiation, named after the template
    /// arguments as in 2018, e.g. hkFreeListArrayElement&lt;hknpShapeInstance&gt; -> hkFreeListArrayElementHknpShapeInstance
    /// </summary>
    private static string GetDeclName(HavokType type, string scope)
    {
        int templateStart = scope.IndexOf('<');
        string name = Sanitize(templateStart < 0 ? scope : scope[..templateStart]);
        foreach (HavokType argument in GetTypeArguments(type))
        {
            name += string.Concat(Regex.Split(argument.Identity, "[^A-Za-z0-9]+")
                .Where(x => x.Length > 0)
                .Select(x => char.ToUpperInvariant(x[0]) + x[1..]));
        }

        return name;
    }

    #endregion

    #region Building

    private void Build(Decl decl)
    {
        decl.Built = true;
        if (decl.Kind == DeclKind.Enum) return;

        decl.BaseTypes.Clear();
        HavokType? source = ChooseSource(decl);
        if (source is null)
        {
            decl.BaseTypes.Add("IHavokObject");
            AddSharedInterfaces(decl);
            return;
        }

        Dictionary<string, string> substitutions = new();
        List<HavokType> typeArguments = GetTypeArguments(source);
        for (int i = 0; i < decl.GenericParameters.Count && i < typeArguments.Count; i++)
        {
            substitutions.TryAdd(typeArguments[i].Identity, decl.GenericParameters[i]);
        }

        HavokType? parent = source.Parent;
        if (parent is { Kind: HavokType.TypeKind.Record })
        {
            decl.BaseTypes.Add(Render(parent, substitutions).Text.TrimEnd('?'));
            if (GetRecordDecl(parent) is { } baseDecl)
            {
                decl.BaseDecl = baseDecl;
                decl.BaseArguments.AddRange(GetGenericArguments(parent, baseDecl, substitutions));
            }
        }
        else
        {
            decl.BaseTypes.Add("IHavokObject");
        }

        foreach (HavokType.Interface iface in source.Interfaces.Skip(parent?.Interfaces.Count ?? 0))
        {
            decl.BaseTypes.Add(Render(iface.Type, substitutions).Text.TrimEnd('?'));
        }

        AddSharedInterfaces(decl);

        if (decl.Kind == DeclKind.Interface) return;

        foreach (HavokType.Member field in source.Fields.Skip(parent?.Fields.Count ?? 0))
        {
            if (field.NonSerializable) continue;
            decl.Fields.Add(new FieldDecl("m_" + field.Name, Render(field.Type, substitutions)));
        }

        // fields only some game variants have (e.g. NR's CustomManualSelectorGenerator.isBasePoseAnim) are added, so one
        // class holds every game's data. Generic declarations are built from their instantiations instead.
        if (decl.GenericParameters.Count > 0) return;
        foreach (HavokType other in decl.Sources.Where(x => !ReferenceEquals(x, source) && x.Identity == source.Identity))
        {
            foreach (HavokType.Member field in other.Fields.Skip(other.Parent?.Fields.Count ?? 0))
            {
                if (field.NonSerializable || decl.Fields.Any(x => x.Name == "m_" + field.Name)) continue;
                decl.Fields.Add(new FieldDecl("m_" + field.Name, Render(field.Type, Empty)));
            }
        }
    }

    private static void AddSharedInterfaces(Decl decl)
    {
        if (decl.SharedInterfaces.Count == 0) return;

        decl.BaseTypes.AddRange(decl.SharedInterfaces);
        decl.Usings.Add("Havok.Shared");
    }

    /// <summary>
    /// Generic declarations are built from the instantiation whose type arguments are most distinguishable, as fields
    /// are mapped back to generic parameters by matching their types against the type arguments.
    /// </summary>
    private static HavokType? ChooseSource(Decl decl)
    {
        if (decl.Sources.Count == 0) return null;
        if (decl.GenericParameters.Count == 0) return decl.Sources.OrderByDescending(x => x.Fields.Count).First();

        return decl.Sources.OrderByDescending(source =>
        {
            List<HavokType> arguments = GetTypeArguments(source);
            int score = arguments.Select(x => x.Identity).Distinct().Count() == arguments.Count ? 100 : 0;
            return score + arguments.Count(x => x.Kind is HavokType.TypeKind.Record or HavokType.TypeKind.Pointer);
        }).First();
    }

    #endregion

    #region Rendering

    private TypeInfo Render(HavokType type, IReadOnlyDictionary<string, string> substitutions)
    {
        if (substitutions.TryGetValue(type.Identity, out string? parameter))
        {
            return new TypeInfo(Shape.Generic, parameter);
        }

        // hkPropertyBag is written by hand, as in HKLib: an empty class (its only field isn't serialized)
        if (type.Identity == "hkPropertyBag" || type.Type == typeof(Havok.hk2018.hkPropertyBag))
        {
            return new TypeInfo(Shape.Init, "`hkPropertyBag`");
        }

        return type.Kind switch
        {
            HavokType.TypeKind.Bool => new TypeInfo(Shape.Value, "bool"),
            HavokType.TypeKind.String => new TypeInfo(Shape.Nullable, "string?"),
            HavokType.TypeKind.Float => new TypeInfo(Shape.Value,
                type.Type == typeof(double) || (type.Type is null && type.Size == 8) ? "double" : "float"),
            HavokType.TypeKind.Int => RenderInt(type),
            HavokType.TypeKind.Pointer => RenderPointer(type, substitutions),
            HavokType.TypeKind.Array => RenderArray(type, substitutions),
            HavokType.TypeKind.Record => RenderRecord(type, substitutions),
            _ => new TypeInfo(Shape.Nullable, "object?")
        };
    }

    private TypeInfo RenderInt(HavokType type)
    {
        string storage = GetIntKeyword(type.Format);
        if (type.Type is { IsEnum: false } clrType) return RenderClr(clrType);

        if (FindEnum(type) is { } enumType)
        {
            return new TypeInfo(Shape.Enum, Mark(GetEnumDecl(enumType))) { Storage = storage };
        }

        if (type.Type is { } enumClrType && Is2018(enumClrType))
        {
            return new TypeInfo(Shape.Enum, Mark(GetClrDecl(enumClrType))) { Storage = storage };
        }

        return new TypeInfo(Shape.Value, storage);
    }

    private TypeInfo RenderPointer(HavokType type, IReadOnlyDictionary<string, string> substitutions)
    {
        HavokType? subType = GetSubType(type);
        if (subType is not null && substitutions.TryGetValue(subType.Identity, out string? parameter))
        {
            return new TypeInfo(Shape.Generic, parameter);
        }

        switch (subType?.Kind)
        {
            case HavokType.TypeKind.Record:
                TypeInfo record = RenderRecord(subType, substitutions);
                return new TypeInfo(Shape.Nullable, record.Text.TrimEnd('?') + "?");
            case HavokType.TypeKind.String:
                return new TypeInfo(Shape.Nullable, "string?");
        }

        // void pointers, e.g. hkRefVariant, use the type 2018 mapped them to
        if (type.Type is { } clrType && (Is2018(clrType) || clrType == typeof(object)))
        {
            TypeInfo clr = RenderClr(clrType);
            return new TypeInfo(Shape.Nullable, clr.Text.TrimEnd('?') + "?");
        }

        return new TypeInfo(Shape.Nullable, "object?");
    }

    private TypeInfo RenderArray(HavokType type, IReadOnlyDictionary<string, string> substitutions)
    {
        if (type.Type is { } clrType && (MathTypes.Contains(clrType) || clrType == typeof(object)))
        {
            return RenderClr(clrType);
        }

        HavokType? subType = GetSubType(type);
        if (type.Name == "T[N]" && subType is not null)
        {
            int length = type.TemplateParameters.FirstOrDefault(x => x.Name == "N")?.Value ?? 1;
            TypeInfo element = Render(subType, substitutions);
            if (element.Shape == Shape.Fixed)
            {
                int[] dimensions = element.Dimensions.Prepend(length).ToArray();
                return new TypeInfo(Shape.Fixed, element.Element + "[" + new string(',', dimensions.Length - 1) + "]")
                {
                    Element = element.Element,
                    Dimensions = dimensions
                };
            }

            return new TypeInfo(Shape.Fixed, element.Text + "[]")
            {
                Element = element.Text,
                Dimensions = new[] { length }
            };
        }

        if (subType is null) return new TypeInfo(Shape.Init, "List<object>");

        return new TypeInfo(Shape.Init, "List<" + Render(subType, substitutions).Text + ">");
    }

    private TypeInfo RenderRecord(HavokType type, IReadOnlyDictionary<string, string> substitutions)
    {
        Decl? decl = GetRecordDecl(type);
        if (decl is null) return type.Type is { } clrType ? RenderClr(clrType) : new TypeInfo(Shape.Nullable, "object?");

        string text = Mark(decl);
        if (decl.GenericParameters.Count > 0)
        {
            text += "<" + string.Join(", ", GetGenericArguments(type, decl, substitutions).Select(x => x.Text)) + ">";
        }

        return decl.Kind == DeclKind.Interface
            ? new TypeInfo(Shape.Nullable, text + "?")
            : new TypeInfo(Shape.Init, text);
    }

    /// <summary>
    /// Renders a C# type which the 2018 identity type map used, for types which keep their 2018 representation.
    /// </summary>
    private TypeInfo RenderClr(Type clrType)
    {
        if (clrType == typeof(string)) return new TypeInfo(Shape.Nullable, "string?");
        if (clrType == typeof(object)) return new TypeInfo(Shape.Nullable, "object?");
        if (Keywords.TryGetValue(clrType, out string? keyword)) return new TypeInfo(Shape.Value, keyword);
        if (MathTypes.Contains(clrType)) return new TypeInfo(Shape.Init, clrType.Name);
        if (clrType == typeof(Color) || clrType == typeof(Half)) return new TypeInfo(Shape.Value, clrType.Name);

        // optional values, patterns can't test for nullable value types so they are accessed like nullable references
        if (Nullable.GetUnderlyingType(clrType) is { } underlyingType)
        {
            return new TypeInfo(Shape.Nullable, RenderClr(underlyingType).Text + "?");
        }

        if (clrType.IsArray)
        {
            TypeInfo element = RenderClr(clrType.GetElementType()!);
            return new TypeInfo(Shape.Init, element.Text + "[" + new string(',', clrType.GetArrayRank() - 1) + "]");
        }

        if (clrType.IsGenericType && clrType.GetGenericTypeDefinition() == typeof(List<>))
        {
            return new TypeInfo(Shape.Init, "List<" + RenderClr(clrType.GetGenericArguments()[0]).Text + ">");
        }

        if (IsManual(clrType)) return new TypeInfo(Shape.Nullable, "`" + clrType.Name + "`?");

        if (Is2018(clrType))
        {
            Decl decl = GetClrDecl(clrType.IsGenericType ? clrType.GetGenericTypeDefinition() : clrType);
            string text = Mark(decl);
            if (clrType.IsConstructedGenericType)
            {
                text += "<" + string.Join(", ", clrType.GetGenericArguments().Select(x => RenderClr(x).Text)) + ">";
            }

            return decl.Kind switch
            {
                DeclKind.Enum => new TypeInfo(Shape.Value, text),
                DeclKind.Interface => new TypeInfo(Shape.Nullable, text + "?"),
                _ => new TypeInfo(Shape.Init, text)
            };
        }

        Warnings.Add($"No representation for C# type {clrType}");
        return new TypeInfo(Shape.Nullable, "object?");
    }

    private List<TypeInfo> GetGenericArguments(HavokType type, Decl decl,
        IReadOnlyDictionary<string, string> substitutions)
    {
        List<HavokType> arguments = GetTypeArguments(type);
        List<TypeInfo> result = new();
        for (int i = 0; i < decl.GenericParameters.Count; i++)
        {
            result.Add(i < arguments.Count
                ? Render(arguments[i], substitutions)
                : new TypeInfo(Shape.Nullable, "object?"));
        }

        return result;
    }

    /// <summary>
    /// The C# type for the identity type map. Pointers map to the type they point to, as in 2018.
    /// </summary>
    private string? GetTypeofText(HavokType type)
    {
        string? text = type.Kind switch
        {
            HavokType.TypeKind.Record => RenderRecord(type, Empty).Text,
            HavokType.TypeKind.Pointer => GetSubType(type) switch
            {
                { Kind: HavokType.TypeKind.Record } subType => RenderRecord(subType, Empty).Text,
                { Kind: HavokType.TypeKind.String } => "string",
                _ => type.Type is { } clrType ? RenderClr(clrType).Text : "object"
            },
            HavokType.TypeKind.Void or HavokType.TypeKind.Opaque => type.Type is { } clrType
                ? RenderClr(clrType).Text
                : null,
            _ => Render(type, Empty).Text
        };

        // typeof does not take nullable reference annotations, and they are only added to reference types
        return text?.Replace("?", "");
    }

    private static readonly IReadOnlyDictionary<string, string> Empty = new Dictionary<string, string>();

    #endregion

    #region Helpers

    private static readonly HashSet<Type> SharedMarkers = new()
    {
        typeof(Havok.Shared.IRootContainer), typeof(Havok.Shared.IClipGenerator),
        typeof(Havok.Shared.IStateMachine), typeof(Havok.Shared.ICustomManualSelectorGenerator)
    };

    // a type of the 2018 class set, whose naming and placement the other SDKs follow
    private static bool Is2018(Type type) =>
        type.Namespace is { } ns && (ns == Namespace2018 || ns.StartsWith(Namespace2018 + "."));

    // types written by hand in hk2019/Manual
    private static bool IsManual(Type type) =>
        type == typeof(Havok.hk2018.IHavokObject) || type == typeof(Havok.hk2018.hkPropertyBag);

    // hkEnum/hkFlags have presets too, but they are represented by the enum they wrap
    private static bool IsEnum(HavokType type) =>
        type.Kind == HavokType.TypeKind.Int && type.Presets.Count > 0 && GetEnumParameter(type) is null &&
        type.Type is null or { IsEnum: true };

    private static string Mark(Decl decl) => "`" + decl.RelativeName + "`";

    private Type? GetClrType(HavokType type)
    {
        if (type.Type is not null) return type.Type;
        if (type.TemplateParameters.Count == 0 || !_genericDefinitions.TryGetValue(type.Name, out List<Type>? definitions))
            return null;

        int arity = GetTypeArguments(type).Count;
        return definitions.FirstOrDefault(x => x.GetGenericArguments().Length == arity);
    }

    /// <summary>
    /// Finds the enum of an int type, which is either the ENUM/BITS of an hkEnum/hkFlags, the type itself or the enum
    /// of the parent. 2019 hkEnum/hkFlags copy the presets of their enum, so the template parameter is checked first.
    /// </summary>
    private static HavokType? FindEnum(HavokType type)
    {
        for (HavokType? current = type; current is not null; current = current.Parent)
        {
            if (GetEnumParameter(current) is { } enumType) return FindEnum(enumType);
            if (current.Presets.Count > 0) return current;
        }

        return null;
    }

    private static HavokType? GetEnumParameter(HavokType type) =>
        type.TemplateParameters.FirstOrDefault(x => x.Name is "ENUM" or "BITS" && x.Type is not null && x.Type != type)
            ?.Type;

    private static HavokType? GetSubType(HavokType type)
    {
        for (HavokType? current = type; current is not null; current = current.Parent)
        {
            if (current.SubType is not null) return current.SubType;
        }

        return null;
    }

    private static List<HavokType> GetTypeArguments(HavokType type) =>
        type.TemplateParameters
            .Where(x => x.Type is not null && x.Type.Name != "void")
            .Select(x => x.Type!)
            .ToList();

    private static string GetIntKeyword(int format)
    {
        bool isSigned = (format & 0x200) != 0;
        return ((format >> 10) / 8, isSigned) switch
        {
            (1, true) => "sbyte",
            (1, false) => "byte",
            (2, true) => "short",
            (2, false) => "ushort",
            (4, true) => "int",
            (4, false) => "uint",
            (8, true) => "long",
            _ => "ulong"
        };
    }

    private static string StripArity(string name)
    {
        int index = name.IndexOf('`');
        return index < 0 ? name : name[..index];
    }

    public static string Sanitize(string name)
    {
        string sanitized = Regex.Replace(name, "[^A-Za-z0-9_]", "");
        if (sanitized.Length == 0) return "_";
        return char.IsDigit(sanitized[0]) ? "_" + sanitized : sanitized;
    }

    /// <summary>
    /// Splits an identity at the :: which are not inside template arguments
    /// </summary>
    private static List<string> SplitScopes(string identity)
    {
        List<string> scopes = new();
        int depth = 0;
        int start = 0;
        for (int i = 0; i < identity.Length; i++)
        {
            switch (identity[i])
            {
                case '<':
                    depth++;
                    break;
                case '>':
                    depth--;
                    break;
                case ':' when depth == 0 && i + 1 < identity.Length && identity[i + 1] == ':':
                    scopes.Add(identity[start..i]);
                    start = i + 2;
                    i++;
                    break;
            }
        }

        scopes.Add(identity[start..]);
        return scopes;
    }

    #endregion
}
