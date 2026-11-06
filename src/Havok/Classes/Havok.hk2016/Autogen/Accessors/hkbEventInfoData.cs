// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbEventInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbEventInfo>
{
    public hkbEventInfoData(Havok.hk2016.hkbEventInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2016.hkbEventInfo.Flags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_flags = (Havok.hk2016.hkbEventInfo.Flags)uintValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
