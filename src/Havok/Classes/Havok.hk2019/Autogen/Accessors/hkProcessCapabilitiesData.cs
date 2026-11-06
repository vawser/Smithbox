// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkProcessCapabilitiesData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkProcessCapabilities>
{
    public hkProcessCapabilitiesData(Havok.hk2019.hkProcessCapabilities instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minFrameLatency":
            case "minFrameLatency":
            {
                value = instance.m_minFrameLatency;
                return true;
            }
            case "m_maxFrameLatency":
            case "maxFrameLatency":
            {
                value = instance.m_maxFrameLatency;
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
            case "m_minFrameLatency":
            case "minFrameLatency":
            {
                if (value is not int castValue) return false;
                instance.m_minFrameLatency = castValue;
                return true;
            }
            case "m_maxFrameLatency":
            case "maxFrameLatency":
            {
                if (value is not int castValue) return false;
                instance.m_maxFrameLatency = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
