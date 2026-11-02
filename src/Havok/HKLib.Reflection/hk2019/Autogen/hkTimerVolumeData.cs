// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkTimerVolumeData : HavokData<HKLib.hk2019.hkTimerVolume> 
{
    public hkTimerVolumeData(HavokType type, HKLib.hk2019.hkTimerVolume instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aabbMin":
            case "aabbMin":
            {
                if (instance.m_aabbMin is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aabbMax":
            case "aabbMax":
            {
                if (instance.m_aabbMax is not TGet castValue) return false;
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
            case "m_aabbMin":
            case "aabbMin":
            {
                if (value is not HKLib.hk2019.hkFloat3 castValue) return false;
                instance.m_aabbMin = castValue;
                return true;
            }
            case "m_aabbMax":
            case "aabbMax":
            {
                if (value is not HKLib.hk2019.hkFloat3 castValue) return false;
                instance.m_aabbMax = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
