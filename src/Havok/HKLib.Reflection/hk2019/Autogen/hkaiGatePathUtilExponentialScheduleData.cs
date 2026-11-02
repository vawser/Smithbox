// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiGatePathUtilExponentialScheduleData : HavokData<HKLib.hk2019.hkaiGatePathUtil.ExponentialSchedule> 
{
    public hkaiGatePathUtilExponentialScheduleData(HavokType type, HKLib.hk2019.hkaiGatePathUtil.ExponentialSchedule instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_round":
            case "round":
            {
                if (instance.m_round is not TGet castValue) return false;
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
            case "m_round":
            case "round":
            {
                if (value is not int castValue) return false;
                instance.m_round = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
