// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestSetterTestNestedData : HavokData<HKLib.hk2019.UnitTest.SetterTest.Nested> 
{
    public UnitTestSetterTestNestedData(HavokType type, HKLib.hk2019.UnitTest.SetterTest.Nested instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_int":
            case "int":
            {
                if (instance.m_int is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_float":
            case "float":
            {
                if (instance.m_float is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_plain":
            case "plain":
            {
                if (instance.m_plain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_twiceNestedSet":
            case "twiceNestedSet":
            {
                if (instance.m_twiceNestedSet is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_twiceNested":
            case "twiceNested":
            {
                if (instance.m_twiceNested is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_array":
            case "array":
            {
                if (instance.m_array is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_variant":
            case "variant":
            {
                if (instance.m_variant is not TGet castValue) return false;
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
            case "m_int":
            case "int":
            {
                if (value is not int castValue) return false;
                instance.m_int = castValue;
                return true;
            }
            case "m_float":
            case "float":
            {
                if (value is not float castValue) return false;
                instance.m_float = castValue;
                return true;
            }
            case "m_plain":
            case "plain":
            {
                if (value is not HKLib.hk2019.UnitTest.SetterTest.TwiceNested castValue) return false;
                instance.m_plain = castValue;
                return true;
            }
            case "m_twiceNestedSet":
            case "twiceNestedSet":
            {
                if (value is not int castValue) return false;
                instance.m_twiceNestedSet = castValue;
                return true;
            }
            case "m_twiceNested":
            case "twiceNested":
            {
                if (value is not HKLib.hk2019.UnitTest.SetterTest.TwiceNested castValue) return false;
                instance.m_twiceNested = castValue;
                return true;
            }
            case "m_array":
            case "array":
            {
                if (value is not List<int> castValue) return false;
                instance.m_array = castValue;
                return true;
            }
            case "m_variant":
            case "variant":
            {
                if (value is not List<object> castValue) return false;
                instance.m_variant = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
