using System.Collections.Concurrent;
using System.Numerics;
using Havok.Formats.Tagfile;
using Havok.Reflection;
using HKLib.ValueTypes;

namespace Havok.Formats.HKLib;

/// <summary>
/// HKLib's hk2018 classes as an <see cref="IHavokClassSet" />, so the unified tagfile engine can read and write them
/// unchanged (Phase 2). Field access goes through HKLib's generated HavokData accessors. This bridge goes away once
/// the generated Havok.hk2018 classes replace HKLib's (Phase 7).
/// </summary>
public sealed class HKLib2018ClassSet : IHavokClassSet
{
    private static readonly ConcurrentDictionary<string, HKLib2018ClassSet> Instances = new();

    private readonly global::HKLib.Reflection.hk2018.HavokTypeRegistry _hklibRegistry;
    private readonly ConcurrentDictionary<string, global::HKLib.Reflection.hk2018.HavokType> _hklibTypes = new();

    private HKLib2018ClassSet(HavokProfile profile, global::HKLib.Reflection.hk2018.HavokTypeRegistry hklibRegistry)
    {
        Profile = profile;
        _hklibRegistry = hklibRegistry;
        Registry = profile.LoadRegistry(identity =>
                       global::HKLib.hk2018.IdentityTypeMap.Map.TryGetValue(identity, out Type? type) ? type : null)
                   ?? throw new InvalidOperationException($"{profile} has no type registry.");
    }

    /// <summary>
    /// The class set for a 2018 game (ER, NR, DB_NT, DB), with that game's overlays. One per game, shared.
    /// </summary>
    public static HKLib2018ClassSet For(HavokProfile profile)
    {
        if (profile.Sdk != HavokSdk.Hk2018)
            throw new ArgumentException($"{profile} is not a 2018 game.", nameof(profile));

        return Instances.GetOrAdd(profile.Game, _ => new HKLib2018ClassSet(profile,
            profile == HavokProfile.NR
                ? global::HKLib.Reflection.hk2018.HavokTypeRegistry.Nightreign
                : global::HKLib.Reflection.hk2018.HavokTypeRegistry.Instance));
    }

    public HavokProfile Profile { get; }

    public string Name => $"HKLib hk2018 ({Profile.Game})";

    public HavokTypeRegistry Registry { get; }

    public Type? Matrix3x3Type => typeof(Matrix3x3);

    public object ToMatrix3x3(Matrix4x4 matrix) => (Matrix3x3)matrix;

    public Matrix4x4 FromMatrix3x3(object matrix) => (Matrix3x3)matrix;

    public bool IsHavokObject(object value) => value is global::HKLib.hk2018.IHavokObject;

    public IHavokFieldAccess Instantiate(HavokType type)
    {
        if (type.Instantiate() is not global::HKLib.hk2018.IHavokObject obj)
            throw new ArgumentException($"Failed to instantiate type {type.Identity}", nameof(type));
        return Access(obj, type);
    }

    public IHavokFieldAccess Access(object havokObject, HavokType type)
    {
        var obj = (global::HKLib.hk2018.IHavokObject)havokObject;
        var data = global::HKLib.Reflection.hk2018.HavokData.Of(obj, HKLibType(type));
        return new Access2018(obj, data);
    }

    /// <summary>
    /// HKLib's own type with the same identity. HavokData only uses it for messages, but keep it right.
    /// </summary>
    private global::HKLib.Reflection.hk2018.HavokType HKLibType(HavokType type) =>
        _hklibTypes.GetOrAdd(type.Identity, identity => _hklibRegistry.GetType(identity)
            ?? throw new KeyNotFoundException($"HKLib's 2018 registry has no type \"{identity}\"."));

    private sealed class Access2018(global::HKLib.hk2018.IHavokObject obj, global::HKLib.Reflection.hk2018.HavokData data)
        : IHavokFieldAccess
    {
        public object Object => obj;
        public bool TryGetField(string fieldName, out object? value) => data.TryGetField(fieldName, out value);
        public bool TrySetField(string fieldName, object? value) => data.TrySetField(fieldName, value);
    }
}

/// <summary>
/// HKLib's hk2019 classes as an <see cref="IHavokClassSet" /> (AC6). See <see cref="HKLib2018ClassSet" />.
/// </summary>
public sealed class HKLib2019ClassSet : IHavokClassSet
{
    private static readonly Lazy<HKLib2019ClassSet> AC6 = new(() => new HKLib2019ClassSet(HavokProfile.AC6));

    private readonly ConcurrentDictionary<string, global::HKLib.Reflection.hk2019.HavokType> _hklibTypes = new();

    private HKLib2019ClassSet(HavokProfile profile)
    {
        Profile = profile;
        Registry = profile.LoadRegistry(identity =>
                       global::HKLib.hk2019.IdentityTypeMap.Map.TryGetValue(identity, out Type? type) ? type : null)
                   ?? throw new InvalidOperationException($"{profile} has no type registry.");
    }

    public static HKLib2019ClassSet For(HavokProfile profile)
    {
        if (profile.Sdk != HavokSdk.Hk2019)
            throw new ArgumentException($"{profile} is not a 2019 game.", nameof(profile));
        return AC6.Value;
    }

    public HavokProfile Profile { get; }

    public string Name => $"HKLib hk2019 ({Profile.Game})";

    public HavokTypeRegistry Registry { get; }

    public Type? Matrix3x3Type => typeof(Matrix3x3);

    public object ToMatrix3x3(Matrix4x4 matrix) => (Matrix3x3)matrix;

    public Matrix4x4 FromMatrix3x3(object matrix) => (Matrix3x3)matrix;

    public bool IsHavokObject(object value) => value is global::HKLib.hk2019.IHavokObject;

    public IHavokFieldAccess Instantiate(HavokType type)
    {
        if (type.Instantiate() is not global::HKLib.hk2019.IHavokObject obj)
            throw new ArgumentException($"Failed to instantiate type {type.Identity}", nameof(type));
        return Access(obj, type);
    }

    public IHavokFieldAccess Access(object havokObject, HavokType type)
    {
        var obj = (global::HKLib.hk2019.IHavokObject)havokObject;
        var data = global::HKLib.Reflection.hk2019.HavokData.Of(obj, HKLibType(type));
        return new Access2019(obj, data);
    }

    private global::HKLib.Reflection.hk2019.HavokType HKLibType(HavokType type) =>
        _hklibTypes.GetOrAdd(type.Identity, identity =>
            global::HKLib.Reflection.hk2019.HavokTypeRegistry.Instance.GetType(identity)
            ?? throw new KeyNotFoundException($"HKLib's 2019 registry has no type \"{identity}\"."));

    private sealed class Access2019(global::HKLib.hk2019.IHavokObject obj, global::HKLib.Reflection.hk2019.HavokData data)
        : IHavokFieldAccess
    {
        public object Object => obj;
        public bool TryGetField(string fieldName, out object? value) => data.TryGetField(fieldName, out value);
        public bool TrySetField(string fieldName, object? value) => data.TrySetField(fieldName, value);
    }
}

/// <summary>
/// Tagfile serializers for HKLib's classes, by game.
/// </summary>
public static class HKLibTagfiles
{
    /// <summary>
    /// A serializer for a 2018 or 2019 game's files. <paramref name="dialect" /> defaults to the SDK's dialect.
    /// </summary>
    public static HavokTagfileSerializer Create(HavokProfile profile, TagfileDialect? dialect = null) => profile.Sdk switch
    {
        HavokSdk.Hk2018 => new HavokTagfileSerializer(HKLib2018ClassSet.For(profile), dialect ?? TagfileDialect.Hk2018),
        HavokSdk.Hk2019 => new HavokTagfileSerializer(HKLib2019ClassSet.For(profile), dialect ?? TagfileDialect.Hk2019),
        _ => throw new ArgumentException($"HKLib has no classes for {profile}.", nameof(profile))
    };
}
