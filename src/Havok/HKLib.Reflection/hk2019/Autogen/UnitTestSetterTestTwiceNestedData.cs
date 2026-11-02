// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestSetterTestTwiceNestedData : HavokData<HKLib.hk2019.UnitTest.SetterTest.TwiceNested> 
{
    public UnitTestSetterTestTwiceNestedData(HavokType type, HKLib.hk2019.UnitTest.SetterTest.TwiceNested instance) : base(type, instance) {}

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
            default:
            return false;
        }
    }

}
