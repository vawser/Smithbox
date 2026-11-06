// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class UnitTestClassWithHashMapIntData : Havok.Formats.HavokFieldAccess<Havok.hk2018.UnitTest.ClassWithHashMapInt>
{
    public UnitTestClassWithHashMapIntData(Havok.hk2018.UnitTest.ClassWithHashMapInt instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_keys":
            case "keys":
            {
                value = instance.m_keys;
                return true;
            }
            case "m_hashMap":
            case "hashMap":
            {
                value = instance.m_hashMap;
                return true;
            }
            case "m_hashSet":
            case "hashSet":
            {
                value = instance.m_hashSet;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_keys":
            case "keys":
            {
                if (value is not List<int> castValue) return false;
                instance.m_keys = castValue;
                return true;
            }
            case "m_hashMap":
            case "hashMap":
            {
                if (value is not Havok.hk2018.hkHashMap<int, int> castValue) return false;
                instance.m_hashMap = castValue;
                return true;
            }
            case "m_hashSet":
            case "hashSet":
            {
                if (value is not Havok.hk2018.hkHashSet<int> castValue) return false;
                instance.m_hashSet = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
