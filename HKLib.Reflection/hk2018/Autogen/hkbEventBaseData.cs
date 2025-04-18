// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbEventBaseData : HavokData<hkbEventBase> 
{
    public hkbEventBaseData(HavokType type, hkbEventBase instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_payload":
            case "payload":
            {
                if (instance.m_payload is null)
                {
                    return true;
                }
                if (instance.m_payload is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (value is not int castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_payload":
            case "payload":
            {
                if (value is null)
                {
                    instance.m_payload = default;
                    return true;
                }
                if (value is hkbEventPayload castValue)
                {
                    instance.m_payload = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
