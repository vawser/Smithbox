// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiGatePathUtilExponentialScheduleData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiGatePathUtil.ExponentialSchedule>
{
    public hkaiGatePathUtilExponentialScheduleData(Havok.hk2019.hkaiGatePathUtil.ExponentialSchedule instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_round":
            case "round":
            {
                value = instance.m_round;
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
