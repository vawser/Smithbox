// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbStateMachineTransitionInfoArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbStateMachine.TransitionInfoArray>
{
    public hkbStateMachineTransitionInfoArrayData(Havok.hk2016.hkbStateMachine.TransitionInfoArray instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_transitions":
            case "transitions":
            {
                if (value is not List<Havok.hk2016.hkbStateMachine.TransitionInfo> castValue) return false;
                instance.m_transitions = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
