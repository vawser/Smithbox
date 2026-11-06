// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbEventBaseData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbEventBase>
{
    public hkbEventBaseData(Havok.hk2014.hkbEventBase instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_payload":
            case "payload":
            {
                value = instance.m_payload;
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
                if (value is Havok.hk2014.hkbEventPayload castValue)
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
