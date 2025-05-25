namespace HKLib.Reflection.hk2018;

/// <summary>
/// Stores Havok reflection type info
/// </summary>
public class HavokType : Reflection.HavokType
{
    /// <summary>
    /// Optional components which can be serialized along with a type.
    /// </summary>
    [Flags]
    public enum Optional
    {
        None = 0,
        Format = 1 << 0,
        SubType = 1 << 1,
        Version = 1 << 2,
        SizeAlign = 1 << 3,
        Flags = 1 << 4,
        Members = 1 << 5,
        Interfaces = 1 << 6,
        Attributes = 1 << 7
    }

    /// <summary>
    /// Flags which provide extra info about the type
    /// </summary>
    [Flags]
    public enum TypeFlags
    {
        None = 0,
        NonSerializable = 1 << 0,
        Abstract = 1 << 1,
        Property = 1 << 3,
        Interface = 1 << 4
    }

    /// <summary>
    /// <see cref="HavokType.Format" /> categories
    /// </summary>
    public enum TypeKind
    {
        Void = 0,
        Opaque,
        Bool,
        String,
        Int,
        Float,
        Pointer,
        Record,
        Array
    }

    private readonly ITypeReference? _parent;
    private readonly ITypeReference? _subtype;

    internal HavokType(ITypeReference? parent, ITypeReference? subtype)
    {
        _parent = parent;
        _subtype = subtype;
    }

    public override bool Serializable => !Flags.HasFlag(TypeFlags.NonSerializable);

    /// <summary>
    /// Kind of Havok Type represented by this object.
    /// </summary>
    public TypeKind Kind => (TypeKind)(Format & 0xf);

    /// <summary>
    /// Flags which provide extra information about the type.
    /// </summary>
    public TypeFlags Flags { get; init; }

    /// <summary>
    /// Optional non-inherited components which are serialized with this type
    /// </summary>
    public Optional Optionals { get; init; }

    /// <summary>
    /// Stores container element or pointer type
    /// </summary>
    public HavokType? SubType => _subtype?.Value;

    public override HavokType? Parent => _parent?.Value;

    /// <summary>
    /// List of C++ template parameters
    /// </summary>
    public IReadOnlyList<TemplateParameter> TemplateParameters { get; init; } = Array.Empty<TemplateParameter>();

    /// <summary>
    /// List of interfaces implemented by this type
    /// </summary>
    public IReadOnlyList<Interface> Interfaces { get; init; } = Array.Empty<Interface>();

    /// <summary>
    /// List of all fields of this type
    /// </summary>
    public IReadOnlyList<Member> Fields { get; init; } = Array.Empty<Member>();

    /// <summary>
    /// List of all properties of this type (fields with getter/setter). These are typically not serialized.
    /// </summary>
    public IReadOnlyList<Member?> Properties { get; init; } = Array.Empty<Member>();

    /// <summary>
    /// Provides information about how types are serialized.
    /// </summary>
    public int Format { get; init; }

    /// <summary>
    /// Type version.
    /// </summary>
    public ulong Version { get; init; }

    /// <summary>
    /// Hash of the type. Not calculated for all types, only for top-level types when serialized.
    /// </summary>
    public uint? Hash { get; init; }

    /// <summary>
    /// Named instances of this type. Used for enum values.
    /// </summary>
    public IReadOnlyList<string> Presets { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Instantiates the C# type which corresponds to this
    /// <see cref="HavokType" />. Only types with an existing C# representation as denoted by the value of the
    /// <see cref="HavokType.Type" /> property are supported. The type in question must also be instantiatable. Note that the
    /// <see cref="HavokType.TypeFlags.Interface" /> flag does not necessarily correspond to a C# interface as it only denotes
    /// whether or not a type has fields.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The type has no C# representation (<see cref="HavokType.Type" /> is
    /// <see langword="null" />) or it cannot be instantiated.
    /// </exception>
    public object Instantiate()
    {
        if (Type is null)
        {
            throw new InvalidOperationException($"The type {Identity} has no C# representation.");
        }

        if (Type.IsInterface || Type.IsAbstract)
        {
            throw new InvalidOperationException($"The type {Identity} cannot be instantiated.");
        }

        switch (Kind)
        {
            case TypeKind.String:
                return string.Empty;
            case TypeKind.Array:
                if (!Type.IsArray) return Activator.CreateInstance(Type)!;

                int length = TemplateParameters[1].Value!.Value;
                if (!SubType!.Type!.IsArray) return Array.CreateInstance(Type.GetElementType()!, length);

                int innerLength = SubType!.TemplateParameters[1].Value!.Value;
                return Array.CreateInstance(Type.GetElementType()!, length, innerLength);
            case TypeKind.Void:
            case TypeKind.Opaque:
                throw new InvalidOperationException($"The type {Identity} cannot be instantiated.");
            case TypeKind.Bool:
                return new bool();
            case TypeKind.Pointer:
                // Some pointers are mapped to System.String which cannot be instantiated by Activator without constructor arguments
                return Type == typeof(string) ? string.Empty : Activator.CreateInstance(Type)!;
            case TypeKind.Int:
            case TypeKind.Float:
            case TypeKind.Record:
            default:
                return Activator.CreateInstance(Type)!;
        }
    }

    /// <summary>
    /// Describes a reference to a <see cref="HavokType" />. Allows for circular references or abitrary creation order while
    /// maintaining immutability.
    /// </summary>
    internal interface ITypeReference
    {
        public HavokType Value { get; }
    }

    /// <summary>
    /// Template argument for a <see cref="HavokType" />. Is either an integer or a HavokType
    /// </summary>
    public record TemplateParameter
    {
        private readonly ITypeReference? _type;

        internal TemplateParameter(string name, int? value, ITypeReference? type)
        {
            Name = name;
            Value = value;
            _type = type;
        }

        public HavokType? Type => _type?.Value;

        public string Name { get; }
        public int? Value { get; }

        public void Deconstruct(out string name, out int? value, out HavokType? type)
        {
            name = Name;
            value = Value;
            type = Type;
        }
    }

    /// <summary>
    /// An interface implemented by a <see cref="HavokType" />
    /// </summary>
    public record Interface
    {
        private readonly ITypeReference _type;

        internal Interface(ITypeReference type, long offset)
        {
            _type = type;
            Offset = offset;
        }

        public HavokType Type => _type.Value;
        public long Offset { get; }

        public void Deconstruct(out HavokType? type, out long offset)
        {
            type = Type;
            offset = Offset;
        }
    }

    /// <inheritdoc cref="Reflection.HavokType.Member" />
    public new record Member : Reflection.HavokType.Member
    {
        private readonly ITypeReference _type;

        internal Member(string name, MemberFlags flags, int offset, ITypeReference type) : base(name, flags, offset)
        {
            _type = type;
        }

        public override HavokType Type => _type.Value;
    }
}