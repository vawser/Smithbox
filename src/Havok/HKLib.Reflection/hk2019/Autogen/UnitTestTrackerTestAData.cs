// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class UnitTestTrackerTestAData : HavokData<HKLib.hk2019.UnitTest.TrackerTestA> 
{
    public UnitTestTrackerTestAData(HavokType type, HKLib.hk2019.UnitTest.TrackerTestA instance) : base(type, instance) {}

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
            case "m_ptrA":
            case "ptrA":
            {
                if (instance.m_ptrA is null)
                {
                    return true;
                }
                if (instance.m_ptrA is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_l":
            case "l":
            {
                if (instance.m_l is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_reflected":
            case "reflected":
            {
                if (instance.m_reflected is not TGet castValue) return false;
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
            case "m_ptrA":
            case "ptrA":
            {
                if (value is null)
                {
                    instance.m_ptrA = default;
                    return true;
                }
                if (value is HKLib.hk2019.UnitTest.TrackerTestA castValue)
                {
                    instance.m_ptrA = castValue;
                    return true;
                }
                return false;
            }
            case "m_l":
            case "l":
            {
                if (value is not long castValue) return false;
                instance.m_l = castValue;
                return true;
            }
            case "m_reflected":
            case "reflected":
            {
                if (value is not HKLib.hk2019.UnitTest.Reflected castValue) return false;
                instance.m_reflected = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
