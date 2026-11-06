// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbStateMachineTransitionInfoReferenceData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbStateMachine.TransitionInfoReference>
{
    public hkbStateMachineTransitionInfoReferenceData(Havok.hk2018.hkbStateMachine.TransitionInfoReference instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_fromStateIndex":
            case "fromStateIndex":
            {
                value = instance.m_fromStateIndex;
                return true;
            }
            case "m_transitionIndex":
            case "transitionIndex":
            {
                value = instance.m_transitionIndex;
                return true;
            }
            case "m_stateMachineId":
            case "stateMachineId":
            {
                value = instance.m_stateMachineId;
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
            case "m_fromStateIndex":
            case "fromStateIndex":
            {
                if (value is not short castValue) return false;
                instance.m_fromStateIndex = castValue;
                return true;
            }
            case "m_transitionIndex":
            case "transitionIndex":
            {
                if (value is not short castValue) return false;
                instance.m_transitionIndex = castValue;
                return true;
            }
            case "m_stateMachineId":
            case "stateMachineId":
            {
                if (value is not short castValue) return false;
                instance.m_stateMachineId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
