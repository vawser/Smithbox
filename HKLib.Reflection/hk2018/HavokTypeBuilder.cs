using HKLib.hk2018;
using System.Diagnostics;

namespace HKLib.Reflection.hk2018;

/// <summary>
/// Builds <see cref="HavokType" /> objects. References between HavokTypes are not guaranteed to be valid until all
/// dependent types have been built.
/// </summary>
public class HavokTypeBuilder
{
    private readonly Func<string, Type?> _identityToTypeFunc = identity =>
    {
        IdentityTypeMap.Map.TryGetValue(identity, out Type? type);
        return type;
    };

    private bool _built;
    private List<HavokType.Member> _fields = new();

    private string? _identity;

    private List<HavokType.Interface> _interfaces = new();

    private TypeReference? _parent;
    private HavokTypeBuilder? _parentBuilder;
    private List<string> _presets = new();
    private List<HavokType.Member?> _properties = new();
    private TypeReference? _subType;
    private List<HavokTypeBuilder?> _templateBuilders = new();

    private List<HavokType.TemplateParameter> _templateParameters = new();

    /// <summary>
    /// Constructs a <see cref="HavokTypeBuilder" /> which automatically resolves the C# representation of the
    /// <see cref="HavokType" /> it builds from a built-in type map
    /// </summary>
    public HavokTypeBuilder() { }

    /// <summary>
    /// Constructs a <see cref="HavokTypeBuilder" /> which uses the provided function to resolve the C# representation of the
    /// <see cref="HavokType" /> it builds from its <see cref="Reflection.HavokType.Identity" />.
    /// </summary>
    public HavokTypeBuilder(Func<string, Type?> identityToTypeFunc)
    {
        _identityToTypeFunc = identityToTypeFunc;
    }

    /// <inheritdoc cref="HavokType.Name" />
    public string Name { get; private set; } = "";

    /// <inheritdoc cref="HavokType.Type" />
    public Type? Type { get; private set; }

    /// <inheritdoc cref="HavokType.Size" />
    public int Size { get; private set; }

    /// <inheritdoc cref="HavokType.Alignment" />
    public int Alignment { get; private set; }

    /// <inheritdoc cref="HavokType.Serializable" />
    public bool Serializable => !Flags.HasFlag(HavokType.TypeFlags.NonSerializable);

    /// <inheritdoc cref="HavokType.SubType" />
    public HavokType? SubType => _subType?.Value;

    /// <inheritdoc cref="HavokType.Flags" />
    public HavokType.TypeFlags Flags { get; private set; } = HavokType.TypeFlags.None;

    /// <inheritdoc cref="HavokType.Optionals" />
    public HavokType.Optional Optionals { get; private set; } = HavokType.Optional.None;

    /// <inheritdoc cref="HavokType.TemplateParameters" />
    public IReadOnlyList<HavokType.TemplateParameter> TemplateParameters => _templateParameters.AsReadOnly();

    /// <inheritdoc cref="HavokType.Interfaces" />
    public IReadOnlyList<HavokType.Interface> Interfaces => _interfaces.AsReadOnly();

    /// <inheritdoc cref="HavokType.Fields" />
    public IReadOnlyList<HavokType.Member> Fields => _fields.AsReadOnly();

    /// <inheritdoc cref="HavokType.Properties" />
    public IReadOnlyList<HavokType.Member?> Properties => _properties.AsReadOnly();

    /// <inheritdoc cref="HavokType.Presets" />
    public IReadOnlyList<string> Presets => _presets.AsReadOnly();

    /// <inheritdoc cref="HavokType.Format" />
    public int Format { get; private set; }

    /// <inheritdoc cref="HavokType.Version" />
    public ulong Version { get; private set; }

    /// <inheritdoc cref="HavokType.Hash" />
    public uint? Hash { get; private set; }

    /// <summary>
    /// Provides a reference to the resulting type for all registered <see cref="TypeReference" /> objects
    /// </summary>
    private event EventHandler<HavokType>? TypeBuilt;

    /// <summary>
    /// Builds the <see cref="HavokType" />.
    /// Modifications to related <see cref="HavokTypeBuilder" /> objects are no longer valid after calling this method.
    /// </summary>
    /// <exception cref="InvalidOperationException">Parent type was not set</exception>
    public HavokType Build()
    {
        CheckIfBuilt();

        SetIdentity();
        Type ??= _identityToTypeFunc(_identity!);

        if (Name == "hkcdPlanarEntity::PlanesCollection") { }

        if (_parent is not null)
        {
            Inherit();
        }


        HavokType type = new(_parent, _subType)
        {
            Name = Name,
            Identity = _identity!,
            Size = Size,
            Alignment = Alignment,
            Flags = Flags,
            Optionals = Optionals,
            TemplateParameters = TemplateParameters,
            Interfaces = Interfaces,
            Fields = Fields,
            Properties = Properties,
            Format = Format,
            Version = Version,
            Hash = Hash,
            Presets = Presets,
            Type = Type
        };

        _built = true;

        OnTypeBuilt(type);
        return type;
    }

    private void SetIdentity()
    {
        if (_identity is not null) return;

        string identity = Name;
        if (_templateParameters.Count > 0)
        {
            identity += "<";

            for (int i = 0; i < _templateParameters.Count; i++)
            {
                HavokType.TemplateParameter parameter = _templateParameters[i];
                if (parameter.Value is { } val)
                {
                    identity += val;
                }
                else if (_templateBuilders[i] is { } templateBuilder)
                {
                    templateBuilder.SetIdentity();
                    identity += templateBuilder._identity;
                }
                else
                {
                    identity += parameter.Type!.Identity;
                }

                if (i != _templateParameters.Count - 1)
                {
                    identity += ", ";
                }
            }

            identity += ">";
        }

        _identity = identity;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Optionals" /> property
    /// </summary>
    public HavokTypeBuilder WithOptionals(HavokType.Optional optionals)
    {
        CheckIfBuilt();
        Optionals = optionals;
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Format" /> property.
    /// </summary>
    public HavokTypeBuilder WithFormat(int format)
    {
        CheckIfBuilt();
        Format = format;
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Name" /> property.
    /// </summary>
    public HavokTypeBuilder WithName(string name)
    {
        CheckIfBuilt();
        Name = name;
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Type" /> property.
    /// </summary>
    public HavokTypeBuilder WithType(Type type)
    {
        CheckIfBuilt();
        Type = type;
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Version" /> property.
    /// </summary>
    public HavokTypeBuilder WithVersion(ulong version)
    {
        CheckIfBuilt();
        Version = version;
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Size" /> and <see cref="HavokType.Alignment" /> properties.
    /// </summary>
    public HavokTypeBuilder WithSizeAlignment(int size, int alignment)
    {
        CheckIfBuilt();
        Size = size;
        Alignment = alignment;
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Flags" /> property.
    /// </summary>
    public HavokTypeBuilder WithFlags(HavokType.TypeFlags flags)
    {
        CheckIfBuilt();
        Flags = flags;
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Hash" /> property.
    /// </summary>
    public HavokTypeBuilder WithHash(uint hash)
    {
        CheckIfBuilt();
        Hash = hash;
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Parent" /> property.
    /// </summary>
    public HavokTypeBuilder WithParent(HavokType parent)
    {
        CheckIfBuilt();
        _parent = new TypeReference(parent);
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Parent" /> property from a <see cref="HavokTypeBuilder" /> which will build the
    /// parent type.
    /// The <see cref="HavokType" /> built by this instance will not be valid until the provided builder has finished building
    /// the parent.
    /// </summary>
    public HavokTypeBuilder WithParent(HavokTypeBuilder parentBuilder)
    {
        CheckIfBuilt();
        _parent = new TypeReference();
        parentBuilder.RegisterReference(_parent);
        _parentBuilder = parentBuilder;
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.SubType" /> property.
    /// </summary>
    public HavokTypeBuilder WithSubType(HavokType subType)
    {
        CheckIfBuilt();
        _subType = new TypeReference(subType);
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.SubType" /> property from a <see cref="HavokTypeBuilder" /> which will build the
    /// subtype.
    /// The <see cref="HavokType" /> built by this instance will not be valid until the provided builder has finished building
    /// the subtype.
    /// </summary>
    public HavokTypeBuilder WithSubType(HavokTypeBuilder subTypeBuilder)
    {
        CheckIfBuilt();
        _subType = new TypeReference();
        subTypeBuilder.RegisterReference(_subType);
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.TemplateParameters" /> property.
    /// </summary>
    public HavokTypeBuilder WithTemplateParameters(List<HavokType.TemplateParameter> templateParameters)
    {
        CheckIfBuilt();
        _templateParameters = new List<HavokType.TemplateParameter>(templateParameters);
        _templateBuilders = templateParameters.Select(_ => (HavokTypeBuilder?)null).ToList();
        return this;
    }

    /// <summary>
    /// Adds a <see cref="HavokType.TemplateParameter" /> to the <see cref="HavokType.TemplateParameters" /> property.
    /// </summary>
    public HavokTypeBuilder WithTemplateParameter(HavokType.TemplateParameter templateParameter)
    {
        CheckIfBuilt();
        _templateParameters.Add(templateParameter);
        _templateBuilders.Add(null);
        return this;
    }

    /// <summary>
    /// Constructs and adds a <see cref="HavokType.TemplateParameter" /> representing a value to the
    /// <see cref="HavokType.TemplateParameters" /> property.
    /// </summary>
    public HavokTypeBuilder WithTemplateParameter(string paramName, int value)
    {
        CheckIfBuilt();
        _templateParameters.Add(new HavokType.TemplateParameter(paramName, value, null));
        _templateBuilders.Add(null);
        return this;
    }

    /// <summary>
    /// Constructs and adds a <see cref="HavokType.TemplateParameter" /> representing a <see cref="HavokType" /> to the
    /// <see cref="HavokType.TemplateParameters" /> property.
    /// </summary>
    public HavokTypeBuilder WithTemplateParameter(string paramName, HavokType type)
    {
        CheckIfBuilt();
        _templateParameters.Add(new HavokType.TemplateParameter(paramName, null, new TypeReference(type)));
        _templateBuilders.Add(null);
        return this;
    }

    /// <summary>
    /// Constructs a <see cref="HavokType.TemplateParameter" /> representing the <see cref="HavokType" /> which
    /// will be built by the provided <see cref="HavokTypeBuilder" /> and adds it to the
    /// <see cref="HavokType.TemplateParameters" /> property. The <see cref="HavokType" /> built by this instance will
    /// not be valid until the provided builder has finished building the type.
    /// </summary>
    public HavokTypeBuilder WithTemplateParameter(string paramName, HavokTypeBuilder builder)
    {
        CheckIfBuilt();
        TypeReference reference = new();
        builder.RegisterReference(reference);
        _templateParameters.Add(new HavokType.TemplateParameter(paramName, null, reference));
        _templateBuilders.Add(builder);
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Interfaces" /> property.
    /// </summary>
    public HavokTypeBuilder WithInterfaces(IEnumerable<HavokType.Interface> interfaces)
    {
        CheckIfBuilt();
        _interfaces = new List<HavokType.Interface>(interfaces);
        return this;
    }

    /// <summary>
    /// Adds an <see cref="HavokType.Interface" /> to the <see cref="HavokType.Interfaces" /> property.
    /// </summary>
    public HavokTypeBuilder WithInterface(HavokType.Interface @interface)
    {
        CheckIfBuilt();
        _interfaces.Add(@interface);
        return this;
    }

    /// <summary>
    /// Constructs and adds an <see cref="HavokType.Interface" /> to the <see cref="HavokType.Interfaces" /> property.
    /// </summary>
    public HavokTypeBuilder WithInterface(HavokType @interface, long offset)
    {
        CheckIfBuilt();
        _interfaces.Add(new HavokType.Interface(new TypeReference(@interface), offset));
        return this;
    }

    /// <summary>
    /// Constructs an <see cref="HavokType.Interface" /> representing the <see cref="HavokType" /> which
    /// will be built by the provided <see cref="HavokTypeBuilder" /> and adds it to the
    /// <see cref="HavokType.Interfaces" /> property. The <see cref="HavokType" /> built by this instance will
    /// not be valid until the provided builder has finished building the type.
    /// </summary>
    public HavokTypeBuilder WithInterface(HavokTypeBuilder interfaceBuilder, long offset)
    {
        CheckIfBuilt();
        TypeReference reference = new();
        interfaceBuilder.RegisterReference(reference);
        _interfaces.Add(new HavokType.Interface(reference, offset));
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Fields" /> property.
    /// </summary>
    public HavokTypeBuilder WithFields(IEnumerable<HavokType.Member> fields)
    {
        CheckIfBuilt();
        _fields = new List<HavokType.Member>(fields);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="HavokType.Member" /> to the <see cref="HavokType.Fields" /> property.
    /// </summary>
    public HavokTypeBuilder WithField(HavokType.Member field)
    {
        CheckIfBuilt();
        _fields.Add(field);
        return this;
    }

    /// <summary>
    /// Constructs and adds a <see cref="HavokType.Member" /> to the <see cref="HavokType.Fields" /> property.
    /// </summary>
    public HavokTypeBuilder WithField(string name, Reflection.HavokType.Member.MemberFlags flags, int offset,
        HavokType type)
    {
        CheckIfBuilt();
        _fields.Add(new HavokType.Member(name, flags, offset, new TypeReference(type)));
        return this;
    }

    /// <summary>
    /// Constructs a <see cref="HavokType.Member" />, the type of which will be built by the provided
    /// <see cref="HavokTypeBuilder" />,
    /// and adds it to the <see cref="HavokType.Fields" /> property. The <see cref="HavokType" /> built by this instance will
    /// not be valid until the provided builder has finished building the type.
    /// </summary>
    public HavokTypeBuilder WithField(string name, Reflection.HavokType.Member.MemberFlags flags, int offset,
        HavokTypeBuilder typeBuilder)
    {
        CheckIfBuilt();
        TypeReference reference = new();
        typeBuilder.RegisterReference(reference);
        _fields.Add(new HavokType.Member(name, flags, offset, reference));
        return this;
    }

    /// <summary>
    /// Initializes the <see cref="HavokType.Properties" /> property.
    /// </summary>
    public HavokTypeBuilder WithProperties(IEnumerable<HavokType.Member?> properties)
    {
        CheckIfBuilt();
        _properties = new List<HavokType.Member?>(properties);
        return this;
    }

    /// <summary>
    /// Adds a <see cref="HavokType.Member" /> to the <see cref="HavokType.Properties" /> property.
    /// </summary>
    public HavokTypeBuilder WithProperty(HavokType.Member property)
    {
        CheckIfBuilt();
        _properties.Add(property);
        return this;
    }

    /// <summary>
    /// Constructs and adds a <see cref="HavokType.Member" /> to the <see cref="HavokType.Properties" /> property.
    /// </summary>
    public HavokTypeBuilder WithProperty(string name, Reflection.HavokType.Member.MemberFlags flags, int offset,
        HavokType type)
    {
        CheckIfBuilt();
        _properties.Add(new HavokType.Member(name, flags, offset, new TypeReference(type)));
        return this;
    }

    /// <summary>
    /// Constructs a <see cref="HavokType.Member" />, the type of which will be built by the provided
    /// <see cref="HavokTypeBuilder" />,
    /// and adds it to the <see cref="HavokType.Properties" /> property. The <see cref="HavokType" /> built by this instance
    /// will
    /// not be valid until the provided builder has finished building the type.
    /// </summary>
    public HavokTypeBuilder WithProperty(string name, Reflection.HavokType.Member.MemberFlags flags, int offset,
        HavokTypeBuilder typeBuilder)
    {
        CheckIfBuilt();
        TypeReference reference = new();
        typeBuilder.RegisterReference(reference);
        _properties.Add(new HavokType.Member(name, flags, offset, reference));
        return this;
    }

    /// <summary>
    /// Sets the value of the <see cref="HavokType.Presets" /> property;
    /// </summary>
    public HavokTypeBuilder WithPresets(IEnumerable<string> presets)
    {
        CheckIfBuilt();
        _presets = new List<string>(presets);
        return this;
    }

    /// <summary>
    /// Adds a preset to the <see cref="HavokType.Presets" /> property;
    /// </summary>
    public HavokTypeBuilder WithPreset(string name)
    {
        CheckIfBuilt();
        _presets.Add(name);
        return this;
    }

    private void Inherit()
    {
        if (_parentBuilder is not null)
        {
            // Opposite inheritance orders for lists so we can insert at 0 and keep the correct ordering of elements
            InheritListsFrom(_parentBuilder);
            InheritFrom(_parentBuilder);
        }
        else if (_parent is not null)
        {
            InheritFrom(_parent.Value);
        }
    }

    private void InheritListsFrom(HavokTypeBuilder builder)
    {
        if (builder._fields.Count > 0)
        {
            _fields.InsertRange(0, builder._fields);
        }

        if (builder._interfaces.Count > 0)
        {
            _interfaces.InsertRange(0, builder._interfaces);
        }

        if (builder._parentBuilder is not null && !builder._built)
        {
            InheritListsFrom(builder._parentBuilder);
        }
    }

    private void InheritFrom(HavokTypeBuilder builder)
    {
        if (builder._parentBuilder is not null && !builder._built)
        {
            InheritFrom(builder._parentBuilder);
        }

        if (!Optionals.HasFlag(HavokType.Optional.Format) && builder.Optionals.HasFlag(HavokType.Optional.Format))
        {
            Format = builder.Format;
        }

        if (!Optionals.HasFlag(HavokType.Optional.SizeAlign) && builder.Optionals.HasFlag(HavokType.Optional.SizeAlign))
        {
            Size = builder.Size;
            Alignment = builder.Alignment;
        }

        if (!Optionals.HasFlag(HavokType.Optional.SubType) && builder.Optionals.HasFlag(HavokType.Optional.SubType))
        {
            _subType = builder._subType;
        }
    }

    private void InheritFrom(HavokType type)
    {
        if (type.Fields.Count != 0)
        {
            _fields.InsertRange(0, type.Fields);
        }

        if (type.Interfaces.Count != 0)
        {
            _interfaces.InsertRange(0, type.Interfaces);
        }

        if (!Optionals.HasFlag(HavokType.Optional.Format))
        {
            Format = type.Format;
        }

        if (!Optionals.HasFlag(HavokType.Optional.SizeAlign))
        {
            Size = type.Size;
            Alignment = type.Alignment;
        }

        if (!Optionals.HasFlag(HavokType.Optional.SubType) && type.SubType is not null)
        {
            _subType = new TypeReference(type.SubType);
        }
    }

    private void OnTypeBuilt(HavokType type)
    {
        TypeBuilt?.Invoke(this, type);
    }

    private void CheckIfBuilt()
    {
        if (_built)
        {
            throw new InvalidOperationException("Type has already been built");
        }
    }

    private void RegisterReference(TypeReference reference)
    {
        TypeBuilt += (_, type) => { reference.SetValue(type); };
    }

    /// <summary>
    /// A reference to a <see cref="HavokType" />. Used for lazy initialization.
    /// </summary>
    private class TypeReference : HavokType.ITypeReference
    {
        private HavokType _value = null!;

        public TypeReference() { }

        public TypeReference(HavokType type)
        {
            _value = type;
        }

        public HavokType Value
        {
            get
            {
                Debug.Assert(_value is not null);
                return _value;
            }
        }

        /// <summary>
        /// Sets the value of the reference if it is null.
        /// </summary>
        /// <param name="type">Value to set the reference to.</param>
        /// <exception cref="InvalidOperationException">The value has already been set</exception>
        public void SetValue(HavokType type)
        {
            if (_value is not null)
            {
                throw new InvalidOperationException("Reference value has already been set.");
            }

            _value = type;
        }
    }
}