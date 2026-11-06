// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbStateMachineActiveTransitionInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbStateMachine.ActiveTransitionInfo>
{
    public hkbStateMachineActiveTransitionInfoData(Havok.hk2018.hkbStateMachine.ActiveTransitionInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transitionEffectInternalStateInfo":
            case "transitionEffectInternalStateInfo":
            {
                value = instance.m_transitionEffectInternalStateInfo;
                return true;
            }
            case "m_transitionInfoReference":
            case "transitionInfoReference":
            {
                value = instance.m_transitionInfoReference;
                return true;
            }
            case "m_transitionInfoReferenceForTE":
            case "transitionInfoReferenceForTE":
            {
                value = instance.m_transitionInfoReferenceForTE;
                return true;
            }
            case "m_fromStateId":
            case "fromStateId":
            {
                value = instance.m_fromStateId;
                return true;
            }
            case "m_toStateId":
            case "toStateId":
            {
                value = instance.m_toStateId;
                return true;
            }
            case "m_isReturnToPreviousState":
            case "isReturnToPreviousState":
            {
                value = instance.m_isReturnToPreviousState;
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
            case "m_transitionEffectInternalStateInfo":
            case "transitionEffectInternalStateInfo":
            {
                if (value is null)
                {
                    instance.m_transitionEffectInternalStateInfo = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbNodeInternalStateInfo castValue)
                {
                    instance.m_transitionEffectInternalStateInfo = castValue;
                    return true;
                }
                return false;
            }
            case "m_transitionInfoReference":
            case "transitionInfoReference":
            {
                if (value is not Havok.hk2018.hkbStateMachine.TransitionInfoReference castValue) return false;
                instance.m_transitionInfoReference = castValue;
                return true;
            }
            case "m_transitionInfoReferenceForTE":
            case "transitionInfoReferenceForTE":
            {
                if (value is not Havok.hk2018.hkbStateMachine.TransitionInfoReference castValue) return false;
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
