// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestSimpleRelArrayData : HavokData<HKLib.hk2019.UnitTest.SimpleRelArray> 
{
    public UnitTestSimpleRelArrayData(HavokType type, HKLib.hk2019.UnitTest.SimpleRelArray instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_intRelArray":
            case "intRelArray":
            {
                if (instance.m_intRelArray is not TGet castValue) return false;
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
            case "m_intRelArray":
            case "intRelArray":
            {
                if (value is not List<int> castValue) return false;
                instance.m_intRelArray = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
