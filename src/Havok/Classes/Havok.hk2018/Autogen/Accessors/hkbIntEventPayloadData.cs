// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbIntEventPayloadData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbIntEventPayload>
{
    public hkbIntEventPayloadData(Havok.hk2018.hkbIntEventPayload instance) : base(instance) { }

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
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is not int castValue) return false;
                instance.m_data = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
