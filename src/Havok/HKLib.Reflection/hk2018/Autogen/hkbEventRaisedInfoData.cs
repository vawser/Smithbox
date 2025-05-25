// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbEventRaisedInfoData : HavokData<hkbEventRaisedInfo> 
{
    public hkbEventRaisedInfoData(HavokType type, hkbEventRaisedInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterId":
            case "characterId":
            {
                if (instance.m_characterId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_eventName":
            case "eventName":
            {
                if (instance.m_eventName is null)
                {
                    return true;
                }
                if (instance.m_eventName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_raisedBySdk":
            case "raisedBySdk":
            {
                if (instance.m_raisedBySdk is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_senderId":
            case "senderId":
            {
                if (instance.m_senderId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_padding":
            case "padding":
            {
                if (instance.m_padding is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
