// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbEventPayloadListData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbEventPayloadList>
{
    public hkbEventPayloadListData(Havok.hk2018.hkbEventPayloadList instance) : base(instance) { }

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
            case "m_payloads":
            case "payloads":
            {
                value = instance.m_payloads;
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
            case "m_payloads":
            case "payloads":
            {
                if (value is not List<Havok.hk2018.hkbEventPayload?> castValue) return false;
                instance.m_payloads = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
