// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbEventPayloadListData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbEventPayloadList>
{
    public hkbEventPayloadListData(Havok.hk2014.hkbEventPayloadList instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_payloads":
            case "payloads":
            {
                if (value is not List<Havok.hk2014.hkbEventPayload?> castValue) return false;
                instance.m_payloads = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
