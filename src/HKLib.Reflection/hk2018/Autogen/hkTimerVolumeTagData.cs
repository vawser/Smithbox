// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkTimerVolumeTagData : HavokData<hkTimerVolumeTag> 
{
    public hkTimerVolumeTagData(HavokType type, hkTimerVolumeTag instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_tag":
            case "tag":
            {
                if (instance.m_tag is not TGet castValue) return false;
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
            case "m_tag":
            case "tag":
            {
                if (value is not int castValue) return false;
                instance.m_tag = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
