// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbStringEventPayloadData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbStringEventPayload>
{
    public hkbStringEventPayloadData(Havok.hk2014.hkbStringEventPayload instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_data":
            case "data":
            {
                if (value is null)
                {
                    instance.m_data = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
