// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbStateMachineTransitionInfoArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbStateMachine.TransitionInfoArray>
{
    public hkbStateMachineTransitionInfoArrayData(Havok.hk2014.hkbStateMachine.TransitionInfoArray instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transitions":
            case "transitions":
            {
                value = instance.m_transitions;
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
            case "m_transitions":
            case "transitions":
            {
                if (value is not List<Havok.hk2014.hkbStateMachine.TransitionInfo> castValue) return false;
                instance.m_transitions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
