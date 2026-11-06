// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbEventRaisedInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbEventRaisedInfo>
{
    public hkbEventRaisedInfoData(Havok.hk2018.hkbEventRaisedInfo instance) : base(instance) { }

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
            case "m_characterId":
            case "characterId":
            {
                value = instance.m_characterId;
                return true;
            }
            case "m_eventName":
            case "eventName":
            {
                value = instance.m_eventName;
                return true;
            }
            case "m_raisedBySdk":
            case "raisedBySdk":
            {
                value = instance.m_raisedBySdk;
                return true;
            }
            case "m_senderId":
            case "senderId":
            {
                value = instance.m_senderId;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                value = instance.m_padding;
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
            case "m_characterId":
            case "characterId":
            {
                if (value is not ulong castValue) return false;
                instance.m_characterId = castValue;
                return true;
            }
            case "m_eventName":
            case "eventName":
            {
                if (value is null)
                {
                    instance.m_eventName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_eventName = castValue;
                    return true;
                }
                return false;
            }
            case "m_raisedBySdk":
            case "raisedBySdk":
            {
                if (value is not bool castValue) return false;
                instance.m_raisedBySdk = castValue;
                return true;
            }
            case "m_senderId":
            case "senderId":
            {
                if (value is not int castValue) return false;
                instance.m_senderId = castValue;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                if (value is not int castValue) return false;
                instance.m_padding = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
