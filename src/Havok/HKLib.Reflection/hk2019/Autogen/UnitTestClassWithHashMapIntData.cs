// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestClassWithHashMapIntData : HavokData<HKLib.hk2019.UnitTest.ClassWithHashMapInt> 
{
    public UnitTestClassWithHashMapIntData(HavokType type, HKLib.hk2019.UnitTest.ClassWithHashMapInt instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_keys":
            case "keys":
            {
                if (instance.m_keys is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hashMap":
            case "hashMap":
            {
                if (instance.m_hashMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hashSet":
            case "hashSet":
            {
                if (instance.m_hashSet is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
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
                if (value is not HKLib.hk2019.hkHashMap<int, int> castValue) return false;
                instance.m_hashMap = castValue;
                return true;
            }
            case "m_hashSet":
            case "hashSet":
            {
                if (value is not HKLib.hk2019.hkHashSet<int> castValue) return false;
                instance.m_hashSet = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
