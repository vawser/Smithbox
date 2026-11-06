using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using Havok.Reflection;

namespace Havok.Formats;

/// <summary>
/// Base of the field accessors Havok.ClassGeneration emits, one per class.
/// </summary>
public abstract class HavokFieldAccess<T> : IHavokFieldAccess where T : class
{
    protected readonly T instance;

    protected HavokFieldAccess(T instance)
    {
        this.instance = instance;
    }

    public object Object => instance;

    public abstract bool TryGetField(string fieldName, out object? value);

    public abstract bool TrySetField(string fieldName, object? value);
}

/// <summary>
/// The class set of a generated SDK assembly (Havok.hk2016, …): its registry bound through the generated identity type
/// map, and its generated accessors, found once per assembly and created through compiled constructors.
/// </summary>
public sealed class GeneratedClassSet : IHavokClassSet
{
    // keyed by game and overlays: DeS has two profiles (Havok 5.5, and 5.1 with its overlay) under one game name
    private static readonly ConcurrentDictionary<(string Game, string Overlays, Type Marker), GeneratedClassSet> Instances = new();

    private readonly Type _marker;
    private readonly Dictionary<Type, Type> _accessorTypes = new(); // class (or generic definition) → accessor
    private readonly ConcurrentDictionary<Type, Func<object, IHavokFieldAccess>> _factories = new();

    private GeneratedClassSet(HavokProfile profile, Type marker, IReadOnlyDictionary<string, Type> identityTypeMap)
    {
        Profile = profile;
        _marker = marker;
        Registry = profile.LoadRegistry(identity => identityTypeMap.TryGetValue(identity, out Type? type) ? type : null)
                   ?? throw new InvalidOperationException($"{profile} has no type registry.");

        foreach (Type type in marker.Assembly.GetTypes())
        {
            for (Type? b = type.BaseType; b is not null; b = b.BaseType)
            {
                if (!b.IsGenericType || b.GetGenericTypeDefinition() != typeof(HavokFieldAccess<>)) continue;
                Type target = b.GetGenericArguments()[0];
                _accessorTypes.TryAdd(target.IsGenericType ? target.GetGenericTypeDefinition() : target, type);
                break;
            }
        }
    }

    /// <summary>
    /// The class set of the assembly holding <paramref name="marker" /> (the SDK's IHavokObject) for a game.
    /// </summary>
    public static GeneratedClassSet For(HavokProfile profile, Type marker, IReadOnlyDictionary<string, Type> identityTypeMap) =>
        Instances.GetOrAdd((profile.Game, string.Join("|", profile.OverlayFiles), marker), _ => new GeneratedClassSet(profile, marker, identityTypeMap));

    public HavokProfile Profile { get; }

    public string Name => $"{_marker.Namespace} ({Profile.Game})";

    public HavokTypeRegistry Registry { get; }

    public Type? Matrix3x3Type => typeof(Matrix3x3);

    public object ToMatrix3x3(Matrix4x4 matrix) => (Matrix3x3)matrix;

    public Matrix4x4 FromMatrix3x3(object matrix) => (Matrix3x3)matrix;

    public bool IsHavokObject(object value) => _marker.IsInstanceOfType(value);

    public IHavokFieldAccess Instantiate(HavokType type)
    {
        object obj = type.Instantiate();
        if (!IsHavokObject(obj))
            throw new ArgumentException($"Failed to instantiate type {type.Identity}", nameof(type));
        return Access(obj, type);
    }

    public IHavokFieldAccess Access(object havokObject, HavokType type) =>
        _factories.GetOrAdd(havokObject.GetType(), CreateFactory)(havokObject);

    private Func<object, IHavokFieldAccess> CreateFactory(Type objectType)
    {
        Type key = objectType.IsGenericType ? objectType.GetGenericTypeDefinition() : objectType;
        if (!_accessorTypes.TryGetValue(key, out Type? accessorType))
            throw new KeyNotFoundException($"{Name} has no accessor for {objectType}.");

        if (accessorType.IsGenericTypeDefinition)
            accessorType = accessorType.MakeGenericType(objectType.GetGenericArguments());

        ConstructorInfo constructor = accessorType.GetConstructor(new[] { objectType })
                                      ?? throw new MissingMethodException($"{accessorType} has no constructor taking {objectType}.");
        ParameterExpression parameter = Expression.Parameter(typeof(object));
        return Expression.Lambda<Func<object, IHavokFieldAccess>>(
            Expression.New(constructor, Expression.Convert(parameter, objectType)), parameter).Compile();
    }
}
