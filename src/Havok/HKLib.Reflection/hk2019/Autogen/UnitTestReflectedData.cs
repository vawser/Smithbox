// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestReflectedData : HavokData<HKLib.hk2019.UnitTest.Reflected> 
{
    public UnitTestReflectedData(HavokType type, HKLib.hk2019.UnitTest.Reflected instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_ptr":
            case "ptr":
            {
                if (instance.m_ptr is null)
                {
                    return true;
                }
                if (instance.m_ptr is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_ptr":
            case "ptr":
            {
                if (value is null)
                {
                    instance.m_ptr = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_ptr = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
