using HKLib.hk2018;
using HKLib.Reflection.hk2018;

namespace HKLib.Tests;

public static class HavokDataTest
{
    private static HavokTypeRegistry? _registry;

    private static HavokTypeRegistry GetRegistry()
    {
        _registry ??= HavokTypeRegistry.Instance;
        return _registry;
    }

    private static IEnumerable<HavokType> ImplementedHavokTypes() =>
        GetRegistry().Types.Where(x => x.Type is not null);

    // Certain record types which are non-serializable are not implemented
    private static IEnumerable<HavokType> ImplementedHavokRecordTypes() =>
        ImplementedHavokTypes().Where(x => x.Kind == HavokType.TypeKind.Record);

    private static IEnumerable<HavokType> InstantiatableHavokRecordTypes() =>
        ImplementedHavokRecordTypes().Where(x => !x.Type!.IsInterface && !x.Type.IsAbstract);

    private static IEnumerable<object[]> InstantiatableHavokTypeFields() => InstantiatableHavokRecordTypes()
        .Where(x => !x.Type!.IsInterface).SelectMany(type =>
        {
            HavokData havokData = HavokData.Instantiate(type);
            return type.Fields.Where(x => !x.NonSerializable && !x.Type.Type!.IsInterface && !x.Type.Type!.IsAbstract)
                .Select(field => { return new object[] { type, field, havokData }; });
        });

    private static IEnumerable<object[]> NullableHavokTypeFields() => InstantiatableHavokRecordTypes()
        .Where(x => !x.Type!.IsInterface).SelectMany(type =>
        {
            HavokData havokData = HavokData.Instantiate(type);
            return type.Fields.Where(x => x is { NonSerializable: false, Type.Kind: HavokType.TypeKind.Pointer })
                .Select(field => { return new object[] { type, field, havokData }; });
        });

    [TestCaseSource(nameof(InstantiatableHavokRecordTypes))]
    public static void InstantiateTest(HavokType type)
    {
        Assert.That(() => HavokData.Instantiate(type), Throws.Nothing);
    }

    [TestCaseSource(nameof(InstantiatableHavokTypeFields))]
    public static void GetSetObjectTest(HavokType type, HavokType.Member field, HavokData havokData)
    {
        object setValue = field.Type.Instantiate();
        Assert.That(havokData.TrySetField(field.Name, setValue), Is.True,
            $"Could not set field {field.Name} in type {type.Identity}.");
        Assert.That(havokData.TryGetField(field.Name, out object? getValue), Is.True,
            $"Could not get field {field.Name} in type {type.Identity}.");
        Assert.That(setValue, Is.EqualTo(getValue), "The set value does not match the get value.");
    }

    [TestCaseSource(nameof(NullableHavokTypeFields))]
    public static void GetSetNullPointerTest(HavokType type, HavokType.Member field, HavokData havokData)
    {
        Assert.That(havokData.TrySetField(field.Name, (object?)null), Is.True,
            $"Could not set null value for field {field.Name} in type {type.Identity}.");
        Assert.That(havokData.TryGetField(field.Name, out object? getValue), Is.True,
            $"Could not get null value for field {field.Name} in type {type.Identity}.");
        Assert.That(getValue, Is.Null,
            $"The returned value for field {field.Name} in type {type.Identity} was not null.");
    }

    [Test]
    public static void EnumGetSetTest()
    {
        HavokData data = HavokData.Instantiate(GetRegistry().GetType("CustomManualSelectorGenerator")!);
        Assert.That(data.TrySetField("m_offsetType", 3), "Failed to set enum from underlying type");
        Assert.That(data.TryGetField("m_offsetType", out int _), "Failed to get enum as underlying type");

        Assert.That(data.TrySetField("m_offsetType", CustomManualSelectorGenerator.OffsetType.AnimIdOffset),
            "Failed to set enum as proper type");
        Assert.That(data.TryGetField("m_offsetType", out CustomManualSelectorGenerator.OffsetType _),
            "Failed to get enum as proper type");
    }

    [Test]
    public static void ArraySizeEnforcementTest()
    {
        HavokData data = HavokData.Instantiate(GetRegistry().GetType("hkcdPlanarGeometryPrimitives::Collection<28>")!);
        Assert.That(data.TrySetField("m_secondaryBitmaps", new uint[21]), Is.False, "Array size was not enforced");
        Assert.That(data.TrySetField("m_freeBlocks", new uint[1, 26]), Is.False,
            "Array size was not enforced in first dimension of 2-dimensional array");
        Assert.That(data.TrySetField("m_freeBlocks", new uint[32, 1]), Is.False,
            "Array size was not enforced in second dimension of 2-dimensional array");
    }
}