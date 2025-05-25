// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbStateMachineActiveTransitionInfoData : HavokData<hkbStateMachine.ActiveTransitionInfo> 
{
    public hkbStateMachineActiveTransitionInfoData(HavokType type, hkbStateMachine.ActiveTransitionInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transitionEffectInternalStateInfo":
            case "transitionEffectInternalStateInfo":
            {
                if (instance.m_transitionEffectInternalStateInfo is null)
                {
                    return true;
                }
                if (instance.m_transitionEffectInternalStateInfo is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_transitionInfoReference":
            case "transitionInfoReference":
            {
                if (instance.m_transitionInfoReference is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transitionInfoReferenceForTE":
            case "transitionInfoReferenceForTE":
            {
                if (instance.m_transitionInfoReferenceForTE is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fromStateId":
            case "fromStateId":
            {
                if (instance.m_fromStateId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toStateId":
            case "toStateId":
            {
                if (instance.m_toStateId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isReturnToPreviousState":
            case "isReturnToPreviousState":
            {
                if (instance.m_isReturnToPreviousState is not TGet castValue) return false;
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
            case "m_transitionEffectInternalStateInfo":
            case "transitionEffectInternalStateInfo":
            {
                if (value is null)
                {
                    instance.m_transitionEffectInternalStateInfo = default;
                    return true;
                }
                if (value is hkbNodeInternalStateInfo castValue)
                {
                    instance.m_transitionEffectInternalStateInfo = castValue;
                    return true;
                }
                return false;
            }
            case "m_transitionInfoReference":
            case "transitionInfoReference":
            {
                if (value is not hkbStateMachine.TransitionInfoReference castValue) return false;
                instance.m_transitionInfoReference = castValue;
                return true;
            }
            case "m_transitionInfoReferenceForTE":
            case "transitionInfoReferenceForTE":
            {
                if (value is not hkbStateMachine.TransitionInfoReference castValue) return false;
                instance.m_transitionInfoReferenceForTE = castValue;
                return true;
            }
            case "m_fromStateId":
            case "fromStateId":
            {
                if (value is not int castValue) return false;
                instance.m_fromStateId = castValue;
                return true;
            }
            case "m_toStateId":
            case "toStateId":
            {
                if (value is not int castValue) return false;
                instance.m_toStateId = castValue;
                return true;
            }
            case "m_isReturnToPreviousState":
            case "isReturnToPreviousState":
            {
                if (value is not bool castValue) return false;
                instance.m_isReturnToPreviousState = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
