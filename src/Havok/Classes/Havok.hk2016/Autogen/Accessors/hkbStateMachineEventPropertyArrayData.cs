// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbStateMachineEventPropertyArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbStateMachine.EventPropertyArray>
{
    public hkbStateMachineEventPropertyArrayData(Havok.hk2016.hkbStateMachine.EventPropertyArray instance) : base(instance) { }

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
            case "m_events":
            case "events":
            {
                value = instance.m_events;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_events":
            case "events":
            {
                if (value is not List<Havok.hk2016.hkbEventProperty> castValue) return false;
                instance.m_events = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
