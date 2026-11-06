// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbStateMachineTimeIntervalData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbStateMachine.TimeInterval>
{
    public hkbStateMachineTimeIntervalData(Havok.hk2016.hkbStateMachine.TimeInterval instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_enterEventId":
            case "enterEventId":
            {
                value = instance.m_enterEventId;
                return true;
            }
            case "m_exitEventId":
            case "exitEventId":
            {
                value = instance.m_exitEventId;
                return true;
            }
            case "m_enterTime":
            case "enterTime":
            {
                value = instance.m_enterTime;
                return true;
            }
            case "m_exitTime":
            case "exitTime":
            {
                value = instance.m_exitTime;
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
            case "m_enterEventId":
            case "enterEventId":
            {
                if (value is not int castValue) return false;
                instance.m_enterEventId = castValue;
                return true;
            }
            case "m_exitEventId":
            case "exitEventId":
            {
                if (value is not int castValue) return false;
                instance.m_exitEventId = castValue;
                return true;
            }
            case "m_enterTime":
            case "enterTime":
            {
                if (value is not float castValue) return false;
                instance.m_enterTime = castValue;
                return true;
            }
            case "m_exitTime":
            case "exitTime":
            {
                if (value is not float castValue) return false;
                instance.m_exitTime = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
