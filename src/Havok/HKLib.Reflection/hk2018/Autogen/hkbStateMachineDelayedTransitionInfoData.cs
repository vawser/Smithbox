// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbStateMachineDelayedTransitionInfoData : HavokData<hkbStateMachine.DelayedTransitionInfo> 
{
    public hkbStateMachineDelayedTransitionInfoData(HavokType type, hkbStateMachine.DelayedTransitionInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_delayedTransition":
            case "delayedTransition":
            {
                if (instance.m_delayedTransition is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeDelayed":
            case "timeDelayed":
            {
                if (instance.m_timeDelayed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isDelayedTransitionReturnToPreviousState":
            case "isDelayedTransitionReturnToPreviousState":
            {
                if (instance.m_isDelayedTransitionReturnToPreviousState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_wasInAbutRangeLastFrame":
            case "wasInAbutRangeLastFrame":
            {
                if (instance.m_wasInAbutRangeLastFrame is not TGet castValue) return false;
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
            case "m_delayedTransition":
            case "delayedTransition":
            {
                if (value is not hkbStateMachine.ProspectiveTransitionInfo castValue) return false;
                instance.m_delayedTransition = castValue;
                return true;
            }
            case "m_timeDelayed":
            case "timeDelayed":
            {
                if (value is not float castValue) return false;
                instance.m_timeDelayed = castValue;
                return true;
            }
            case "m_isDelayedTransitionReturnToPreviousState":
            case "isDelayedTransitionReturnToPreviousState":
            {
                if (value is not bool castValue) return false;
                instance.m_isDelayedTransitionReturnToPreviousState = castValue;
                return true;
            }
            case "m_wasInAbutRangeLastFrame":
            case "wasInAbutRangeLastFrame":
            {
                if (value is not bool castValue) return false;
                instance.m_wasInAbutRangeLastFrame = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
