// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbStateMachineProspectiveTransitionInfoData : HavokData<hkbStateMachine.ProspectiveTransitionInfo> 
{
    public hkbStateMachineProspectiveTransitionInfoData(HavokType type, hkbStateMachine.ProspectiveTransitionInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_toStateId":
            case "toStateId":
            {
                if (instance.m_toStateId is not TGet castValue) return false;
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
            case "m_toStateId":
            case "toStateId":
            {
                if (value is not int castValue) return false;
                instance.m_toStateId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
