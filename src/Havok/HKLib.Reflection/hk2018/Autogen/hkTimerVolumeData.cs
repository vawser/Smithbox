// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkTimerVolumeData : HavokData<hkTimerVolume> 
{
    public hkTimerVolumeData(HavokType type, hkTimerVolume instance) : base(type, instance) {}

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
                if (value is not hkFloat3 castValue) return false;
                instance.m_aabbMin = castValue;
                return true;
            }
            case "m_aabbMax":
            case "aabbMax":
            {
                if (value is not hkFloat3 castValue) return false;
                instance.m_aabbMax = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
