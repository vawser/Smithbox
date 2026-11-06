// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbStateMachineDelayedTransitionInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbStateMachine.DelayedTransitionInfo>
{
    public hkbStateMachineDelayedTransitionInfoData(Havok.hk2018.hkbStateMachine.DelayedTransitionInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_delayedTransition":
            case "delayedTransition":
            {
                value = instance.m_delayedTransition;
                return true;
            }
            case "m_timeDelayed":
            case "timeDelayed":
            {
                value = instance.m_timeDelayed;
                return true;
            }
            case "m_isDelayedTransitionReturnToPreviousState":
            case "isDelayedTransitionReturnToPreviousState":
            {
                value = instance.m_isDelayedTransitionReturnToPreviousState;
                return true;
            }
            case "m_wasInAbutRangeLastFrame":
            case "wasInAbutRangeLastFrame":
            {
                value = instance.m_wasInAbutRangeLastFrame;
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
            case "m_delayedTransition":
            case "delayedTransition":
            {
                if (value is not Havok.hk2018.hkbStateMachine.ProspectiveTransitionInfo castValue) return false;
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
