// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbClipTriggerArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbClipTriggerArray>
{
    public hkbClipTriggerArrayData(Havok.hk2016.hkbClipTriggerArray instance) : base(instance) { }

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
            case "m_triggers":
            case "triggers":
            {
                value = instance.m_triggers;
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
            case "m_triggers":
            case "triggers":
            {
                if (value is not List<Havok.hk2016.hkbClipTrigger> castValue) return false;
                instance.m_triggers = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
