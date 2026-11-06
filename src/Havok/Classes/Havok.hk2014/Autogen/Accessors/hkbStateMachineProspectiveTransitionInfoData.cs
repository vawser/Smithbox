// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbStateMachineProspectiveTransitionInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbStateMachine.ProspectiveTransitionInfo>
{
    public hkbStateMachineProspectiveTransitionInfoData(Havok.hk2014.hkbStateMachine.ProspectiveTransitionInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_toStateId":
            case "toStateId":
            {
                value = instance.m_toStateId;
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
            case "m_transitionInfoReference":
            case "transitionInfoReference":
            {
                if (value is not Havok.hk2014.hkbStateMachine.TransitionInfoReference castValue) return false;
                instance.m_transitionInfoReference = castValue;
                return true;
            }
            case "m_transitionInfoReferenceForTE":
            case "transitionInfoReferenceForTE":
            {
                if (value is not Havok.hk2014.hkbStateMachine.TransitionInfoReference castValue) return false;
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
