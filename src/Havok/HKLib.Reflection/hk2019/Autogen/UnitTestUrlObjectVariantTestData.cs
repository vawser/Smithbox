// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestUrlObjectVariantTestData : HavokData<HKLib.hk2019.UnitTest.UrlObject.VariantTest> 
{
    public UnitTestUrlObjectVariantTestData(HavokType type, HKLib.hk2019.UnitTest.UrlObject.VariantTest instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_array1":
            case "array1":
            {
                if (instance.m_array1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_a1":
            case "a1":
            {
                if (instance.m_a1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_array2":
            case "array2":
            {
                if (instance.m_array2 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_a4":
            case "a4":
            {
                if (instance.m_a4 is not TGet castValue) return false;
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
            case "m_array1":
            case "array1":
            {
                if (value is not List<object> castValue) return false;
                instance.m_array1 = castValue;
                return true;
            }
            case "m_a1":
            case "a1":
            {
                if (value is not int castValue) return false;
                instance.m_a1 = castValue;
                return true;
            }
            case "m_array2":
            case "array2":
            {
                if (value is not List<object> castValue) return false;
                instance.m_array2 = castValue;
                return true;
            }
            case "m_a4":
            case "a4":
            {
                if (value is not float castValue) return false;
                instance.m_a4 = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
