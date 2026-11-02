// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkTimerVolumeUtilsAabbData : HavokData<HKLib.hk2019.hkTimerVolumeUtils.Aabb> 
{
    public hkTimerVolumeUtilsAabbData(HavokType type, HKLib.hk2019.hkTimerVolumeUtils.Aabb instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_min":
            case "min":
            {
                if (instance.m_min is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (instance.m_max is not TGet castValue) return false;
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
            case "m_min":
            case "min":
            {
                if (value is not HKLib.hk2019.hkFloat3 castValue) return false;
                instance.m_min = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (value is not HKLib.hk2019.hkFloat3 castValue) return false;
                instance.m_max = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
