// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbStateMachineTransitionInfoReferenceData : HavokData<hkbStateMachine.TransitionInfoReference> 
{
    public hkbStateMachineTransitionInfoReferenceData(HavokType type, hkbStateMachine.TransitionInfoReference instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_fromStateIndex":
            case "fromStateIndex":
            {
                if (instance.m_fromStateIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transitionIndex":
            case "transitionIndex":
            {
                if (instance.m_transitionIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stateMachineId":
            case "stateMachineId":
            {
                if (instance.m_stateMachineId is not TGet castValue) return false;
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
