using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

/// <summary>
/// A reflection wrapper for <see cref="IHavokObject" /> implementations. Provides fast, non-boxing get/set access to
/// fields.
/// </summary>
public abstract class HavokData
{
    private static readonly Dictionary<Type, Type> ReflectorCache = new();

    internal HavokData(HavokType type)
    {
        Type = type;
    }

    /// <summary>
    /// The HavokType which describes this object
    /// </summary>
    public HavokType Type { get; }

    /// <summary>
    /// Gets the underlying <see cref="IHavokObject" /> represented by this <see cref="HavokData" /> instance as type
    /// <typeparamref name="T" />.
    /// </summary>
    /// <typeparam name="T">
    /// Type of object to get. The object represented by this <see cref="HavokData" /> instance must derive from this type.
    /// </typeparam>
    /// <returns>The object as type <typeparamref name="T" /> or null if it is not of the requested type.</returns>
    public abstract T? GetObject<T>() where T : class, IHavokObject;

    /// <summary>
    /// Gets the underlying havok object of the value of the specified field as type <typeparamref name="T" />.
    /// </summary>
    /// <param name="fieldName">Name of the field to get the value of</param>
    /// <param name="value">
    /// Will be assigned the result if the function returns true, otherwise it will be assigned the default
    /// value of type <typeparamref name="T" />
    /// </param>
    /// <typeparam name="T">
    /// Type of object to get, the object represented by this <see cref="HavokData" /> instance must be
    /// implicitly or explicitly convertible to this type
    /// </typeparam>
    /// <returns>
    /// <see langword="true" /> if the field was found and the conversion succeeded, otherwise
    /// <see langword="false" />
    /// </returns>
    public abstract bool TryGetField<T>(string fieldName, out T? value);

    /// <summary>
    /// Sets the value of the specified field.
    /// </summary>
    /// <param name="fieldName">Name of the field to set the value of</param>
    /// <param name="value">Value to set the field to</param>
    /// <typeparam name="T">
    /// Type of value to set. Must be implicitly or explicitly convertible to the type of the value of the
    /// specified field
    /// </typeparam>
    /// <returns>
    /// <see langword="true" /> if the field was found and the conversion succeeded, otherwise
    /// <see langword="false" />
    /// </returns>
    public abstract bool TrySetField<T>(string fieldName, T value);

    /// <summary>
    /// Gets a <see cref="HavokData" /> wrapper for the provided <see cref="IHavokObject" />. Automatically resolves the
    /// <see cref="HavokType" /> corresponding to the provided object using the default type registry.
    /// </summary>
    public static HavokData Of<T>(T obj) where T : class, IHavokObject
    {
        HavokType? type = HavokTypeRegistry.Instance.GetType(obj.GetType());
        if (type is null) throw new ArgumentException("No HavokType instance found for the given object.", nameof(obj));
        return Of(obj, type);
    }

    /// <summary>
    /// Gets a <see cref="HavokData" /> wrapper for the provided <see cref="IHavokObject" /> with <paramref name="type" /> as
    /// the corresponding <see cref="HavokType" />.
    /// </summary>
    public static HavokData Of<T>(T obj, HavokType type) where T : class, IHavokObject
    {
        Type objectType = obj.GetType();
        if (!ReflectorCache.TryGetValue(objectType, out Type? reflectorType))
        {
            if (objectType.IsConstructedGenericType)
            {
                // it's not possible to create a partially open generic type so we have to create the final closed type for each type we test
                Type[] genericArguments = objectType.GetGenericArguments();
                Type reflectorParentType = typeof(HavokData<>).MakeGenericType(objectType);
                reflectorType = Assembly.GetAssembly(typeof(HavokData))!.GetTypes()
                    .Where(x => x.IsGenericType && x.Namespace == reflectorParentType.Namespace)
                    .Select(x =>
                    {
                        // performing checks in advance should be faster than catching exceptions
                        Type[] args = x.GetGenericArguments();
                        if (args.Length != genericArguments.Length) return null;
                        return args.Any(typeArg => typeArg.GetGenericParameterConstraints().Length != 0)
                            ? null
                            : x.MakeGenericType(genericArguments);
                    })
                    .Single(x => x?.IsSubclassOf(reflectorParentType) ?? false)!;
            }
            else
            {
                Type reflectorParentType = typeof(HavokData<>).MakeGenericType(objectType);
                reflectorType = Assembly.GetAssembly(typeof(HavokData))!.GetTypes()
                    .Single(x => x.IsSubclassOf(reflectorParentType))!;
            }

            ReflectorCache.Add(objectType, reflectorType);
        }

        return (HavokData)Activator.CreateInstance(reflectorType, type, obj)!;
    }

    /// <summary>
    /// Instantiates the C# type which corresponds to the <see cref="HavokType.Type" /> property of the provided
    /// <see cref="HavokType" /> and returns its <see cref="HavokData" /> representation. Only types with
    /// <see cref="HavokType.Kind" />
    /// equal to <see cref="HavokType.TypeKind.Record" /> are supported.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// <paramref name="type" /> was not a <see cref="HavokType.TypeKind.Record" /> type or
    /// does not have an instantiatable C# representation.
    /// </exception>
    public static HavokData Instantiate(HavokType type)
    {
        if (type.Kind != HavokType.TypeKind.Record)
        {
            throw new ArgumentException(
                "The provided type does not have a HavokData representation. Only record types are supported.",
                nameof(type));
        }

        if (type.Instantiate() is not IHavokObject obj)
        {
            throw new ArgumentException("The provided type does not have a HavokData representation.", nameof(type));
        }

        return Of(obj, type);
    }
}

internal abstract class HavokData<TData> : HavokData where TData : class, IHavokObject
{
    protected readonly TData instance;

    protected HavokData(HavokType type, TData instance) : base(type)
    {
        this.instance = instance;
    }

    public override T? GetObject<T>() where T : class
    {
        return instance as T;
    }
}