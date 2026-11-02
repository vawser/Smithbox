// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestSetterTestFooData : HavokData<HKLib.hk2019.UnitTest.SetterTest.Foo> 
{
    public UnitTestSetterTestFooData(HavokType type, HKLib.hk2019.UnitTest.SetterTest.Foo instance) : base(type, instance) {}

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
            default:
            return false;
        }
    }

}
