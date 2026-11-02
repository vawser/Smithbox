// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkProcessCapabilitiesData : HavokData<HKLib.hk2019.hkProcessCapabilities> 
{
    public hkProcessCapabilitiesData(HavokType type, HKLib.hk2019.hkProcessCapabilities instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minFrameLatency":
            case "minFrameLatency":
            {
                if (instance.m_minFrameLatency is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxFrameLatency":
            case "maxFrameLatency":
            {
                if (instance.m_maxFrameLatency is not TGet castValue) return false;
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
