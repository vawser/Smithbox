// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbStateMachineTimeIntervalData : HavokData<hkbStateMachine.TimeInterval> 
{
    public hkbStateMachineTimeIntervalData(HavokType type, hkbStateMachine.TimeInterval instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_enterEventId":
            case "enterEventId":
            {
                if (instance.m_enterEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_exitEventId":
            case "exitEventId":
            {
                if (instance.m_exitEventId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enterTime":
            case "enterTime":
            {
                if (instance.m_enterTime is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_exitTime":
            case "exitTime":
            {
                if (instance.m_exitTime is not TGet castValue) return false;
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
