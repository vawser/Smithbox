// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkServerSettingsData : HavokData<HKLib.hk2019.hkServerSettings> 
{
    public hkServerSettingsData(HavokType type, HKLib.hk2019.hkServerSettings instance) : base(type, instance) {}

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
            case "m_allowedFrameLatency":
            case "allowedFrameLatency":
            {
                if (instance.m_allowedFrameLatency is not TGet castValue) return false;
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
            case "m_allowedFrameLatency":
            case "allowedFrameLatency":
            {
                if (value is not int castValue) return false;
                instance.m_allowedFrameLatency = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
